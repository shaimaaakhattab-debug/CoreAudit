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
/// Direct SMTP delivery (MailKit) to Namecheap Private Email (mail.privateemail.com:465,
/// implicit SSL/TLS). Requires the host to allow outbound connections on that port. If a
/// host blocks outbound SMTP, switch <c>EmailSettings:Provider</c> to "Resend"
/// (HTTPS API) — see <see cref="ResendEmailSender"/>.
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
            _logger.LogWarning("SMTP settings are not fully configured; cannot send. Category: ConfigurationMissing");
            return ContactDeliveryResult.Failed;
        }

        return await TrySendByEmailAsync(model, cancellationToken)
            ? ContactDeliveryResult.Sent
            : ContactDeliveryResult.Failed;
    }

    private async Task<bool> TrySendByEmailAsync(ContactFormModel model, CancellationToken cancellationToken)
    {
        var message = BuildMessage(_settings, model);
        var tlsMode = ResolveSecureSocketOptions(_settings);

        try
        {
            using var client = new SmtpClient { Timeout = 20000 };
            await client.ConnectAsync(_settings.Host, _settings.Port, tlsMode, cancellationToken);
            await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            // SendAsync completes only after the server has accepted the message.
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            // Do NOT log message content or the visitor's data.
            _logger.LogInformation("Contact message accepted by the SMTP server on port {Port}.", _settings.Port);
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Log the failure category, exception TYPE and port only — never credentials or message body.
            _logger.LogError("SMTP delivery failed on port {Port}. Category: {Category}. Error type: {ErrorType}",
                _settings.Port, ClassifyFailure(ex), ex.GetType().Name);
            return false;
        }
    }

    /// <summary>
    /// Maps an SMTP exception to a coarse, safe category for logs (never shown to visitors):
    /// Authentication (bad login), Rejected (server refused the command/message),
    /// Connection (unreachable, blocked port, timeout, TLS failure) or Unknown.
    /// </summary>
    public static string ClassifyFailure(Exception ex) => ex switch
    {
        // Also covers SaslException, which derives from it.
        MailKit.Security.AuthenticationException => "Authentication",
        MailKit.Net.Smtp.SmtpCommandException => "Rejected",
        MailKit.Net.Smtp.SmtpProtocolException => "Connection",
        System.Net.Sockets.SocketException => "Connection",
        TimeoutException => "Connection",
        System.IO.IOException => "Connection",
        System.Security.Authentication.AuthenticationException => "Connection",
        _ => "Unknown"
    };

    /// <summary>
    /// Port 465 always uses implicit SSL/TLS (TLS starts on connect), as required by
    /// Namecheap Private Email. Other ports use STARTTLS when <c>UseStartTls</c> is set.
    /// A single attempt is made on the configured port — there is no silent fallback to
    /// a different port, so the connection mode is always the one configured.
    /// </summary>
    public static SecureSocketOptions ResolveSecureSocketOptions(EmailSettings settings) =>
        settings.Port == 465
            ? SecureSocketOptions.SslOnConnect
            : (settings.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

    /// <summary>
    /// Builds the outgoing message. The sender is the authenticated company mailbox and
    /// the recipient is the configured company inbox; the visitor's address is used only
    /// as Reply-To, never as the sender.
    /// </summary>
    public static MimeMessage BuildMessage(EmailSettings settings, ContactFormModel model)
    {
        var message = new MimeMessage();

        // From = authenticated sender (never the visitor's address).
        message.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(settings.ToAddress));

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
