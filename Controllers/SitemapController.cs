using System.Text;
using System.Xml.Linq;
using CoreAudit.Models;
using CoreAudit.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CoreAudit.Controllers;

/// <summary>
/// Serves /sitemap.xml for search engines. URLs are generated from the same routes the site
/// links to (on the production host from SiteSettings.Domain), and the service pages come from
/// IContentService, so the sitemap stays in sync when services are added or removed.
/// </summary>
public class SitemapController : Controller
{
    private static readonly XNamespace Ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

    private readonly IContentService _content;
    private readonly SiteSettings _site;

    public SitemapController(IContentService content, IOptions<SiteSettings> site)
    {
        _content = content;
        _site = site.Value;
    }

    [HttpGet("sitemap.xml")]
    [ResponseCache(Duration = 3600)]
    public IActionResult Index()
    {
        var urls = new List<string> { $"https://{_site.Domain}/" };
        urls.AddRange(new[]
        {
            Absolute("Index", "About"),
            Absolute("Index", "Services"),
        });
        urls.AddRange(_content.GetServices().Select(s => Absolute("Details", "Services", s.Slug)));
        urls.AddRange(new[]
        {
            Absolute("Index", "CrownPrince"),
            Absolute("Index", "Contact"),
            Absolute("Privacy", "Home"),
        });

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(Ns + "urlset",
                urls.Distinct().Select(u => new XElement(Ns + "url", new XElement(Ns + "loc", u)))));

        return Content(doc.Declaration + Environment.NewLine + doc.Root, "application/xml", Encoding.UTF8);
    }

    private string Absolute(string action, string controller, string? id = null) =>
        Url.Action(action, controller, id is null ? null : new { id }, "https", _site.Domain)
        ?? $"https://{_site.Domain}/";
}
