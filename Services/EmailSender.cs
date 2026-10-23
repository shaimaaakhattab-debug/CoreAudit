using CoreAudit.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CoreAudit.Services;

/// <summary>Outcome of attempting to deliver a contact submission.</summary>
public enum ContactDeliveryResult
{
    /// <summary>The email provider accepted the message for delivery.</summary>
    Sent,
    /// <summary>The message could not be delivered (provider unreachable, auth error, misconfigured, etc.).</summary>
    Failed
}

/// <summary>Abstraction for delivering the contact-form message.</summary>
public interface IEmailSender
{
    /// <summary>
    /// Attempts to deliver the contact submission.
    /// Returns <see cref="ContactDeliveryResult.Sent"/> ONLY when the provider has
    /// accepted the message. Any other outcome returns <see cref="ContactDeliveryResult.Failed"/>.
    /// Never throws for transport/IO problems — surfaces a result instead so the
    /// controller can respond with a safe Arabic message. Must not log message
    /// content or credentials.
    /// </summary>
    Task<ContactDeliveryResult> SendContactMessageAsync(ContactFormModel model, CancellationToken cancellationToken = default);
}

/// <summary>
/// Direct SMTP delivery (MailKit). Kept for local development and for hosts that
/// permit outbound SMTP. On hosts that block outbound SMTP (e.g. SmarterASP.NET)
/// use the Resend provider instead — see <see cref="ResendEmailSender"/>.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<ContactDeliveryResult> SendContactMessageAsync(ContactFormModel model, CancellationToken cancellationToken = default)
    {
        if (!_settings.IsSmtpConfigured)
        {
            _logger.LogWarning("SMTP settings are not fully configured; cannot send.");
            return ContactDeliveryResult.Failed;
        }

        return await TrySendByEmailAsync(model, cancellationToken)
            ? ContactDeliveryResult.Sent
            : ContactDeliveryResult.Failed;
    }

    private async Task<bool> TrySendByEmailAsync(ContactFormModel model, CancellationToken cancellationToken)
    {
        var message = BuildMessage(model);
        var attempts = BuildTransportAttempts();

        Exception? lastError = null;
        foreach (var (port, options) in attempts)
        {
            try
            {
                using var client = new SmtpClient { Timeout = 20000 };
                await client.ConnectAsync(_settings.Host, port, options, cancellationToken);
                await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                // Do NOT log message content or the visitor's data.
                _logger.LogInformation("Contact message sent successfully on port {Port}.", port);
                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastError = ex;
                // Log the exception TYPE and port only — never credentials or message body.
                _logger.LogWarning("Send attempt on port {Port} failed. Error type: {ErrorType}", port, ex.GetType().Name);
            }
        }

        _logger.LogError("Failed to send contact message after all transport attempts. Last error type: {ErrorType}",
            lastError?.GetType().Name ?? "Unknown");
        return false;
    }

    /// <summary>
    /// Ordered transports to try: the configured one first, then the alternative.
    /// 587 => STARTTLS, 465 => implicit SSL on connect.
    /// </summary>
    private List<(int Port, SecureSocketOptions Options)> BuildTransportAttempts()
    {
        var list = new List<(int, SecureSocketOptions)>();

        var primaryOptions = _settings.Port == 465
            ? SecureSocketOptions.SslOnConnect
            : (_settings.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
        list.Add((_settings.Port, primaryOptions));

        if (_settings.Port != 465)
            list.Add((465, SecureSocketOptions.SslOnConnect));
        if (_settings.Port != 587)
            list.Add((587, SecureSocketOptions.StartTls));

        return list;
    }

    private MimeMessage BuildMessage(ContactFormModel model)
    {
        var message = new MimeMessage();

        // From = authenticated sender (never the visitor's address).
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(_settings.ToAddress));

        // Reply-To = the visitor's validated email so replies reach them.
        if (!string.IsNullOrWhiteSpace(model.Email))
        {
            try { message.ReplyTo.Add(MailboxAddress.Parse(model.Email)); }
            catch { /* already validated by model binding; ignore malformed edge cases */ }
        }

        message.Subject = ContactMessageFormatter.BuildSubject(model);
        message.Body = new BodyBuilder
        {
            HtmlBody = ContactMessageFormatter.BuildHtml(model),
            TextBody = ContactMessageFormatter.BuildPlainText(model)
        }.ToMessageBody();

        return message;
    }
}
