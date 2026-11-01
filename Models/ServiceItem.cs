namespace CoreAudit.Models;

/// <summary>Represents a single service offered by the firm (used in cards and the services page).</summary>
public class ServiceItem
{
    public string Slug { get; set; } = "";
    public string TitleAr { get; set; } = "";
    public string TitleEn { get; set; } = "";
    public string DescriptionAr { get; set; } = "";
    /// <summary>Bootstrap Icons class name, e.g. "bi-clipboard-check".</summary>
    public string Icon { get; set; } = "bi-briefcase";
    /// <summary>Root-relative path of the service illustration, e.g. "/images/services/external-audit.svg".</summary>
    public string ImagePath { get; set; } = "";

    // Detail-page content (every service has a detail page).
    /// <summary>Optional hero subtitle; falls back to DescriptionAr when empty.</summary>
    public string HeroSubAr { get; set; } = "";
    public string IntroAr { get; set; } = "";
    public IReadOnlyList<string> DetailParagraphsAr { get; set; } = Array.Empty<string>();
    public string LeadInAr { get; set; } = "";
    public IReadOnlyList<string> BulletsAr { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> DeliverablesAr { get; set; } = Array.Empty<string>();
    public string CtaHeadingAr { get; set; } = "";
    public string CtaTextAr { get; set; } = "";
}

/// <summary>A guiding principle shown in the "Why choose us" section.</summary>
public class PrincipleItem
{
    public string TitleAr { get; set; } = "";
    public string DescriptionAr { get; set; } = "";
    public string Icon { get; set; } = "bi-check-circle";
}

/// <summary>A single step in the "Our process" section.</summary>
public class ProcessStep
{
    public int Number { get; set; }
    public string TitleAr { get; set; } = "";
    public string DescriptionAr { get; set; } = "";
    public string Icon { get; set; } = "bi-1-circle";
}
