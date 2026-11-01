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

    // Optional detail-page content. A service has a detail page only when BulletsAr is not empty.
    public string IntroAr { get; set; } = "";
    public string LeadInAr { get; set; } = "";
    public IReadOnlyList<string> BulletsAr { get; set; } = Array.Empty<string>();
    public string CtaHeadingAr { get; set; } = "";
    public string CtaTextAr { get; set; } = "";
    public bool HasDetails => BulletsAr.Count > 0;
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
