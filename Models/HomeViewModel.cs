namespace CoreAudit.Models;

public class HomeViewModel
{
    public IReadOnlyList<ServiceItem> Services { get; set; } = new List<ServiceItem>();
    public IReadOnlyList<PrincipleItem> Principles { get; set; } = new List<PrincipleItem>();
    public IReadOnlyList<ProcessStep> ProcessSteps { get; set; } = new List<ProcessStep>();
}
