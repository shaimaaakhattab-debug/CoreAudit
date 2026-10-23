namespace CoreAudit.Models;

/// <summary>
/// SMTP configuration for sending contact-form emails.
///
/// NON-secret values (Host, Port, UseStartTls, FromName, FromAddress, ToAddress)
/// live in appsettings.json. SECRET values (Username, Password) must come from
/// .NET User Secrets (local dev) or environment variables (production) and must
/// NEVER be committed to source control.
/// </summary>
public class EmailSettings
{
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

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Host) &&
        Port > 0 &&
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(FromAddress) &&
        !string.IsNullOrWhiteSpace(ToAddress);
}
