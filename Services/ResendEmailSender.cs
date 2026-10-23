using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CoreAudit.Models;
using Microsoft.Extensions.Options;

namespace CoreAudit.Services;

/// <summary>
/// Delivers contact submissions through the Resend HTTPS API (https://api.resend.com/emails).
/// Preferred on hosts that block outbound SMTP, because it uses HTTPS (port 443),
/// which hosting providers do not block.
///
/// Reports <see cref="ContactDeliveryResult.Sent"/> ONLY when Resend returns a
/// success status with a message id. Never logs the message body, the visitor's
/// data, or the API key.
/// </summary>
public class ResendEmailSender : IEmailSender
{
    public const string HttpClientName = "Resend";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ResendSettings _resend;
    private readonly EmailSettings _email;
    private readonly ILogger<ResendEmailSender> _logger;

    public ResendEmailSender(
        IHttpClientFactory httpClientFactory,
        IOptions<ResendSettings> resend,
        IOptions<EmailSettings> email,
        ILogger<ResendEmailSender> logger)
    {
        _httpClientFactory = httpClientFactory;
        _resend = resend.Value;
        _email = email.Value;
        _logger = logger;
    }

    public async Task<ContactDeliveryResult> SendContactMessageAsync(ContactFormModel model, CancellationToken cancellationToken = default)
    {
        if (!_resend.IsConfigured || string.IsNullOrWhiteSpace(_email.ToAddress))
        {
            _logger.LogWarning("Resend is not fully configured (API key, From address, or To address missing); cannot send.");
            return ContactDeliveryResult.Failed;
        }

        var payload = new ResendEmailRequest
        {
            From = _resend.FromAddress,
            To = new[] { _email.ToAddress },
            Subject = ContactMessageFormatter.BuildSubject(model),
            Html = ContactMessageFormatter.BuildHtml(model),
            Text = ContactMessageFormatter.BuildPlainText(model),
            // So replies go to the visitor. Only set when it looks valid.
            ReplyTo = string.IsNullOrWhiteSpace(model.Email) ? null : new[] { model.Email }
        };

        try
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);
            client.BaseAddress ??= new Uri(_resend.ApiBaseUrl.TrimEnd('/') + "/");
            // Auth header is set per-request so the secret is never stored on a shared client.
            using var request = new HttpRequestMessage(HttpMethod.Post, "emails")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _resend.ApiKey);

            using var response = await client.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadFromJsonAsync<ResendEmailResponse>(cancellationToken: cancellationToken);
                if (body is not null && !string.IsNullOrWhiteSpace(body.Id))
                {
                    _logger.LogInformation("Contact message accepted by Resend (id present).");
                    return ContactDeliveryResult.Sent;
                }

                // 2xx but no id -> treat as not delivered rather than report a false success.
                _logger.LogError("Resend returned success status {Status} but no message id.", (int)response.StatusCode);
                return ContactDeliveryResult.Failed;
            }

            // Non-2xx: log status only, never the response body (may echo input) or key.
            _logger.LogError("Resend delivery failed with status {Status}.", (int)response.StatusCode);
            return ContactDeliveryResult.Failed;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Type only — never credentials or message content.
            _logger.LogError("Resend delivery threw. Error type: {ErrorType}", ex.GetType().Name);
            return ContactDeliveryResult.Failed;
        }
    }

    // ---- DTOs for the Resend /emails contract ----

    private sealed class ResendEmailRequest
    {
        [JsonPropertyName("from")] public string From { get; set; } = "";
        [JsonPropertyName("to")] public string[] To { get; set; } = Array.Empty<string>();
        [JsonPropertyName("subject")] public string Subject { get; set; } = "";
        [JsonPropertyName("html")] public string Html { get; set; } = "";
        [JsonPropertyName("text")] public string Text { get; set; } = "";

        [JsonPropertyName("reply_to")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string[]? ReplyTo { get; set; }
    }

    private sealed class ResendEmailResponse
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
    }
}
