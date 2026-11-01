using CoreAudit.Services;
using Xunit;

namespace CoreAudit.Tests;

public class ContentServiceTests
{
    private static readonly string[] OriginalSlugs =
    {
        "external-audit", "accounting-bookkeeping", "tax-financial-consulting",
        "internal-audit-risk", "financial-reporting", "business-consulting"
    };

    [Fact]
    public void Judicial_support_is_the_seventh_and_last_service()
    {
        var services = new ContentService().GetServices();

        Assert.Equal(7, services.Count);
        var last = services[^1];
        Assert.Equal("judicial-support", last.Slug);
        Assert.Equal("الدعم القضائي", last.TitleAr);
        Assert.True(last.HasDetails);
    }

    [Fact]
    public void Judicial_support_has_the_three_required_bullets()
    {
        var item = new ContentService().GetServices().Single(s => s.Slug == "judicial-support");

        Assert.Equal(new[]
        {
            "إعداد تقارير الخبرة المحاسبية للنزاعات التجارية.",
            "حصر الموجودات والمديونيات.",
            "إعداد تقارير الاختلاسات والتعاملات غير المشروعة."
        }, item.BulletsAr);
    }

    [Fact]
    public void Slugs_are_unique()
    {
        var slugs = new ContentService().GetServices().Select(s => s.Slug).ToList();
        Assert.Equal(slugs.Count, slugs.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void Original_services_remain_without_detail_pages()
    {
        var services = new ContentService().GetServices();
        foreach (var slug in OriginalSlugs)
        {
            var item = Assert.Single(services, s => s.Slug == slug);
            Assert.False(item.HasDetails);
        }
    }
}
