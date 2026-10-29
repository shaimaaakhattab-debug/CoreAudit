namespace CoreAudit.Models;

/// <summary>
/// Configuration for sending contact-form emails. Shared across providers.
///
/// NON-secret values (Provider, Host, Port, UseStartTls, FromName, FromAddress,
/// ToAddress, Username) live in appsettings.json. The only SECRETS are the SMTP
/// <see cref="Password"/> and the Resend API key (see <see cref="ResendSettings"/>);
/// they must come from .NET User Secrets (local dev) or server-side configuration /
/// environment variables (production) and must NEVER be committed to source control.
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// Selects the delivery mechanism:
    ///   "Smtp" (default)   -> direct SMTP to Namecheap Private Email (mail.privateemail.com:465, implicit SSL/TLS).
    ///   "Resend"           -> HTTPS API, for hosts that block outbound SMTP.
    /// Set via appsettings.json ("EmailSettings:Provider") or the EmailSettings__Provider env var.
    /// </summary>
    public string Provider { get; set; } = "Smtp";

    public string Host { get; set; } = "mail.privateemail.com";

    /// <summary>465 = implicit SSL/TLS (SslOnConnect). Any other port uses STARTTLS when <see cref="UseStartTls"/> is true.</summary>
    public int Port { get; set; } = 465;
    public bool UseStartTls { get; set; } = false;

    /// <summary>Display name shown on the "From" header.</summary>
    public string FromName { get; set; } = "Core Audit Website";

    /// <summary>
    /// The sender address. Namecheap Private Email only accepts mail whose sender is
    /// the authenticated mailbox, so this must equal <see cref="Username"/>. The
    /// visitor's address is never used as the sender (it is set as Reply-To).
    /// </summary>
    public string FromAddress { get; set; } = "info@coreauditcpa.com";

    /// <summary>Where contact submissions are delivered. Defaults to the company inbox.</summary>
    public string ToAddress { get; set; } = "info@coreauditcpa.com";

    /// <summary>SMTP login (the mailbox address). Not a secret.</summary>
    public string Username { get; set; } = "info@coreauditcpa.com";

    /// <summary>SECRET. Supplied only via User Secrets / server-side configuration. Never in Git.</summary>
    public string Password { get; set; } = "";

    /// <summary>True only when every value the SMTP transport needs is present.</summary>
    public bool IsSmtpConfigured =>
        !string.IsNullOrWhiteSpace(Host) &&
        Port > 0 &&
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(FromAddress) &&
        !string.IsNullOrWhiteSpace(ToAddress);
}

/// <summary>
/// Settings for the Resend HTTPS email API (https://resend.com).
/// Used when <see cref="EmailSettings.Provider"/> is "Resend" — the recommended
/// choice on hosts (SmarterASP.NET, MonsterASP.NET, Azure, etc.) that block
/// outbound SMTP, because the API is called over HTTPS (port 443).
///
/// The <see cref="ApiKey"/> is a SECRET: supply it via the Resend__ApiKey
/// environment variable (production) or User Secrets (dev). Never commit it.
/// </summary>
public class ResendSettings
{
    /// <summary>Resend REST endpoint. Overridable only for testing.</summary>
    public string ApiBaseUrl { get; set; } = "https://api.resend.com";

    /// <summary>Secret API key (starts with "re_"). From env var / User Secrets only.</summary>
    public string ApiKey { get; set; } = "";

    /// <summary>
    /// Verified sender, e.g. "Core Audit &lt;noreply@coreauditcpa.com&gt;" or a bare
    /// address. The domain must be verified in the Resend dashboard.
    /// </summary>
    public string FromAddress { get; set; } = "";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ApiBaseUrl) &&
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !string.IsNullOrWhiteSpace(FromAddress);
}
