namespace CoreAudit.Models;

/// <summary>
/// Central, strongly-typed site configuration bound from appsettings.json ("SiteSettings").
/// Keeping brand and contact details here means they can be updated in ONE place
/// (appsettings.json) without touching any Razor view or controller.
/// </summary>
public class SiteSettings
{
    public string BrandNameEn { get; set; } = "CORE AUDIT";
    public string BrandSubtitleEn { get; set; } = "ACCOUNTANTS AND AUDITORS";
    public string BrandNameAr { get; set; } = "جوهر المراجعة";
    public string BrandSubtitleAr { get; set; } = "محاسبون ومراجعون قانونيون";

    public string Domain { get; set; } = "coreauditcpa.com";

    /// <summary>Official contact email.</summary>
    public string Email { get; set; } = "coreauditcpa@gmail.com";

    /// <summary>
    /// Phone number exactly as provided. The country code is NOT assumed.
    /// Stored as-is so it can be confirmed/updated later.
    /// </summary>
    public string Phone { get; set; } = "0567334550";

    /// <summary>
    /// Optional confirmed international phone (e.g. +9665XXXXXXXX) used ONLY to
    /// enable the WhatsApp button. Left empty until the country code is confirmed,
    /// so no WhatsApp link is shown by default.
    /// </summary>
    public string WhatsAppInternational { get; set; } = "";

    public string LogoPath { get; set; } = "/images/core-audit-logo.jpeg";

    /// <summary>Convenience flag: only show WhatsApp once a verified number exists.</summary>
    public bool HasWhatsApp => !string.IsNullOrWhiteSpace(WhatsAppInternational);
}
