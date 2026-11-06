using System.Xml.Linq;
using CoreAudit.Controllers;
using CoreAudit.Models;
using CoreAudit.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CoreAudit.Tests;

public class SitemapControllerTests
{
    private static readonly XNamespace Ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

    private static List<string> GetSitemapUrls()
    {
        // Fake URL generation: https://{host}/{controller}/{action}[/{id}], enough to check the list.
        var url = new Mock<IUrlHelper>();
        url.Setup(u => u.Action(It.IsAny<UrlActionContext>()))
           .Returns((UrlActionContext c) =>
           {
               var id = new RouteValueDictionary(c.Values)["id"];
               return $"{c.Protocol}://{c.Host}/{c.Controller}/{c.Action}" + (id is null ? "" : $"/{id}");
           });

        var controller = new SitemapController(
            new ContentService(),
            Options.Create(new SiteSettings { Domain = "coreauditcpa.com" }))
        {
            Url = url.Object
        };

        var content = Assert.IsType<ContentResult>(controller.Index());
        Assert.StartsWith("application/xml", content.ContentType);
        var doc = XDocument.Parse(content.Content!);
        return doc.Root!.Elements(Ns + "url").Select(e => e.Element(Ns + "loc")!.Value).ToList();
    }

    [Fact]
    public void Sitemap_starts_with_the_https_homepage()
    {
        Assert.Equal("https://coreauditcpa.com/", GetSitemapUrls()[0]);
    }

    [Fact]
    public void Sitemap_lists_every_current_service_and_not_the_removed_one()
    {
        var urls = GetSitemapUrls();
        foreach (var s in new ContentService().GetServices())
        {
            Assert.Contains($"https://coreauditcpa.com/Services/Details/{s.Slug}", urls);
        }
        Assert.DoesNotContain(urls, u => u.Contains("business-consulting"));
        Assert.All(urls, u => Assert.StartsWith("https://coreauditcpa.com/", u));
        Assert.Equal(urls.Count, urls.Distinct().Count());
    }
}
