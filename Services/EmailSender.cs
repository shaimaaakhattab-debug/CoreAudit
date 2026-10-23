using System.Text;
using System.Text.Encodings.Web;
using CoreAudit.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CoreAudit.Services;

/// <summary>Outcome of attempting to deliver a contact submission.</summary>
public enum ContactDeliveryResult
{
    /// <summary>Delivered by email via SMTP.</summary>
    Sent,
    /// <summary>SMTP unavailable; the submission was safely stored on disk as a fallback.</summary>
    SavedLocally,
    /// <summary>Could neither send nor store the submission.</summary>
    Failed
}

/// <summary>Abstraction for delivering the contact-form message.</summary>
public interface IEmailSender
{
    /// <summary>
    /// Attempts to deliver the contact submission by email. If SMTP cannot be
    /// reached (e.g. the network blocks outbound mail), it falls back to storing
    /// the submission on disk so the enquiry is never lost.
    /// Never throws for SMTP/IO problems — returns a <see cref="ContactDeliveryResult"/>
    /// so the controller can respond with a safe Arabic message.
    /// </summary>
    Task<ContactDeliveryResult> SendContactMessageAsync(ContactFormModel model, CancellationToken cancellationToken = default);
}

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;
    private readonly string _inboxDirectory;

    public SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger, IWebHostEnvironment env)
    {
        _settings = settings.Value;
        _logger = logger;
        // Stored outside wwwroot so saved enquiries are never web-accessible.
        _inboxDirectory = Path.Combine(env.ContentRootPath, "App_Data", "contact-messages");
    }

    public async Task<ContactDeliveryResult> SendContactMessageAsync(ContactFormModel model, CancellationToken cancellationToken = default)
    {
        if (_settings.IsConfigured)
        {
            if (await TrySendByEmailAsync(model, cancellationToken))
                return ContactDeliveryResult.Sent;
        }
        else
        {
            _logger.LogWarning("Email settings are not fully configured; using local fallback store.");
        }

        // Email failed or isn't configured — fall back to saving locally so the
        // enquiry is never lost. The controller still shows a genuine success state.
        if (TrySaveLocally(model))
            return ContactDeliveryResult.SavedLocally;

        return ContactDeliveryResult.Failed;
    }

    private async Task<bool> TrySendByEmailAsync(ContactFormModel model, CancellationToken cancellationToken)
    {
        var message = BuildMessage(model);

        // Build an ordered list of transports to try. Start with the configured
        // port/mode, then fall back to the common alternative, because some networks
        // block 587 (STARTTLS) but allow 465 (implicit SSL), or vice-versa.
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

        // Add the alternative port if different from the configured one.
        if (_settings.Port != 465)
            list.Add((465, SecureSocketOptions.SslOnConnect));
        if (_settings.Port != 587)
            list.Add((587, SecureSocketOptions.StartTls));

        return list;
    }

    /// <summary>
    /// Fallback when SMTP is unavailable: writes the submission to a file under
    /// App_Data/contact-messages (outside wwwroot, never web-accessible) so the
    /// firm can read enquiries even while email delivery is blocked.
    /// Values are plain text here (not web output), so no HTML encoding is needed.
    /// </summary>
    private bool TrySaveLocally(ContactFormModel model)
    {
        try
        {
            Directory.CreateDirectory(_inboxDirectory);

            var phone = string.IsNullOrWhiteSpace(model.Phone) ? "غير محدد" : model.Phone;
            var stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var fileName = $"{stamp}_{Guid.NewGuid():N}.txt";
            var path = Path.Combine(_inboxDirectory, fileName);

            var content = new StringBuilder()
                .AppendLine("رسالة جديدة من نموذج التواصل (محفوظة محليًا - تعذّر إرسال البريد)")
                .AppendLine($"التاريخ: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                .AppendLine($"الاسم: {model.Name}")
                .AppendLine($"البريد الإلكتروني: {model.Email}")
                .AppendLine($"رقم الهاتف: {phone}")
                .AppendLine($"الخدمة المطلوبة: {model.Service}")
                .AppendLine()
                .AppendLine("الرسالة:")
                .AppendLine(model.Message)
                .ToString();

            File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            _logger.LogInformation("Contact submission stored locally as a fallback (content not logged).");
            return true;
        }
        catch (Exception ex)
        {
            // Type only — never the submission content.
            _logger.LogError("Failed to store contact submission locally. Error type: {ErrorType}", ex.GetType().Name);
            return false;
        }
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

        message.Subject = $"رسالة جديدة من نموذج التواصل - {Truncate(model.Name, 60)}";

        var phone = string.IsNullOrWhiteSpace(model.Phone) ? "غير محدد" : model.Phone;

        // HTML body — HTML-encode every user value to prevent HTML/script injection.
        var html = new StringBuilder();
        html.Append("<div dir=\"rtl\" style=\"font-family:Tahoma,Arial,sans-serif;font-size:14px;color:#1b2a4a;\">");
        html.Append("<h2 style=\"color:#1b2a4a;\">رسالة جديدة من نموذج التواصل</h2>");
        html.Append("<table cellpadding=\"6\" style=\"border-collapse:collapse;\">");
        AppendRow(html, "الاسم", model.Name);
        AppendRow(html, "البريد الإلكتروني", model.Email);
        AppendRow(html, "رقم الهاتف", phone);
        AppendRow(html, "الخدمة المطلوبة", model.Service);
        html.Append("</table>");
        html.Append("<h3 style=\"color:#1b2a4a;\">الرسالة</h3>");
        html.Append($"<p style=\"white-space:pre-wrap;\">{HtmlEncoder.Default.Encode(model.Message)}</p>");
        html.Append("</div>");

        // Plain-text alternative.
        var text = new StringBuilder()
            .AppendLine("رسالة جديدة من نموذج التواصل")
            .AppendLine($"الاسم: {model.Name}")
            .AppendLine($"البريد الإلكتروني: {model.Email}")
            .AppendLine($"رقم الهاتف: {phone}")
            .AppendLine($"الخدمة المطلوبة: {model.Service}")
            .AppendLine()
            .AppendLine("الرسالة:")
            .AppendLine(model.Message)
            .ToString();

        var body = new BodyBuilder
        {
            HtmlBody = html.ToString(),
            TextBody = text
        };
        message.Body = body.ToMessageBody();

        return message;
    }

    private static void AppendRow(StringBuilder sb, string label, string value)
    {
        sb.Append("<tr>")
          .Append($"<td style=\"font-weight:bold;border:1px solid #e3e3e3;\">{HtmlEncoder.Default.Encode(label)}</td>")
          .Append($"<td style=\"border:1px solid #e3e3e3;\">{HtmlEncoder.Default.Encode(value)}</td>")
          .Append("</tr>");
    }

    private static string Truncate(string value, int max) =>
        string.IsNullOrEmpty(value) || value.Length <= max ? value : value.Substring(0, max);
}
