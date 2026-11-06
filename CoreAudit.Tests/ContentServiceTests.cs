using System.Text.RegularExpressions;
using CoreAudit.Services;
using Xunit;

namespace CoreAudit.Tests;

public class ContentServiceTests
{
    [Fact]
    public void Judicial_support_is_the_sixth_and_last_service()
    {
        var services = new ContentService().GetServices();

        Assert.Equal(6, services.Count);
        var last = services[^1];
        Assert.Equal("judicial-support", last.Slug);
        Assert.Equal("الدعم القضائي", last.TitleAr);
        Assert.True(last.BulletsAr.Count > 0);
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
    public void Judicial_support_keeps_its_hero_subtitle_and_has_no_extra_sections()
    {
        var item = new ContentService().GetServices().Single(s => s.Slug == "judicial-support");

        Assert.Equal("دعم محاسبي وفني للقضايا والنزاعات", item.HeroSubAr);
        Assert.Empty(item.DeliverablesAr);
        Assert.Empty(item.DetailParagraphsAr);
    }

    [Fact]
    public void Slugs_are_unique()
    {
        var slugs = new ContentService().GetServices().Select(s => s.Slug).ToList();
        Assert.Equal(slugs.Count, slugs.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void Slugs_are_url_safe()
    {
        foreach (var service in new ContentService().GetServices())
        {
            Assert.Matches(new Regex("^[a-z0-9]+(-[a-z0-9]+)*$"), service.Slug);
        }
    }

    [Fact]
    public void Every_service_has_complete_detail_content()
    {
        foreach (var s in new ContentService().GetServices())
        {
            Assert.False(string.IsNullOrWhiteSpace(s.IntroAr), s.Slug);
            Assert.False(string.IsNullOrWhiteSpace(s.LeadInAr), s.Slug);
            Assert.NotEmpty(s.BulletsAr);
            Assert.False(string.IsNullOrWhiteSpace(s.CtaHeadingAr), s.Slug);
            Assert.False(string.IsNullOrWhiteSpace(s.CtaTextAr), s.Slug);
            Assert.All(s.BulletsAr, b => Assert.False(string.IsNullOrWhiteSpace(b)));
            Assert.All(s.DetailParagraphsAr, p => Assert.False(string.IsNullOrWhiteSpace(p)));
            Assert.All(s.DeliverablesAr, d => Assert.False(string.IsNullOrWhiteSpace(d)));
        }
    }

    [Fact]
    public void Every_service_has_an_existing_valid_svg_image()
    {
        var wwwroot = FindWwwroot();
        foreach (var s in new ContentService().GetServices())
        {
            Assert.Equal($"/images/services/{s.Slug}.svg", s.ImagePath);
            var file = Path.Combine(wwwroot, s.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(file), file);
            var doc = System.Xml.Linq.XDocument.Load(file);
            Assert.Equal("svg", doc.Root!.Name.LocalName);
        }
    }

    private static string FindWwwroot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "wwwroot");
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(dir.FullName, "CoreAudit.csproj")))
                return candidate;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException("wwwroot not found");
    }

    [Fact]
    public void Approved_arabic_content_is_exact()
    {
        var services = new ContentService().GetServices();

        Assert.Equal(
            "نقدم في كور أوديت خدمات مراجعة مهنية تهدف إلى دعم موثوقية ووضوح التقارير المالية وتعزيز الثقة في البيانات المعروضة. نعمل بدقة وحياد وسرية، ونعرض ملاحظاتنا بصورة منظمة وواضحة.",
            services.Single(s => s.Slug == "external-audit").IntroAr);
        Assert.Equal(
            "هل ترغب في تقارير مالية أوضح؟",
            services.Single(s => s.Slug == "financial-reporting").CtaHeadingAr);
    }

    [Fact]
    public void Business_consulting_service_has_been_removed()
    {
        var services = new ContentService().GetServices();

        Assert.DoesNotContain(services, s => s.Slug == "business-consulting");
        Assert.DoesNotContain(services, s => s.TitleAr == "استشارات الأعمال");
    }
}
