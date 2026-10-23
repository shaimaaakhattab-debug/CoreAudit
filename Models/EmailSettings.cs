namespace CoreAudit.Models;

/// <summary>
/// Configuration for sending contact-form emails. Shared across providers.
///
/// NON-secret values (Provider, Host, Port, UseStartTls, FromName, FromAddress,
/// ToAddress) live in appsettings.json. SECRET values (SMTP Username/Password,
/// and the Resend API key — see <see cref="ResendSettings"/>) must come from
/// .NET User Secrets (local dev) or environment variables (production) and must
/// NEVER be committed to source control.
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// Selects the delivery mechanism:
    ///   "Resend" (default) -> HTTPS API, works on hosts that block outbound SMTP.
    ///   "Smtp"             -> direct SMTP (kept for local dev / hosts that allow it).
    /// Set via appsettings.json ("EmailSettings:Provider") or the EmailSettings__Provider env var.
    /// </summary>
    public string Provider { get; set; } = "Resend";

    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool UseStartTls { get; set; } = true;

    /// <summary>Display name shown on the "From" header.</summary>
    public string FromName { get; set; } = "Core Audit Website";

    /// <summary>
    /// The authenticated sender address. For Gmail this must equal the Gmail
    /// account that owns the app password (i.e. the Username).
    /// </summary>
    public string FromAddress { get; set; } = "";

    /// <summary>Where contact submissions are delivered.</summary>
    public string ToAddress { get; set; } = "";

    // ----- Secrets (from User Secrets / environment variables) -----
    public string Username { get; set; } = "";
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
