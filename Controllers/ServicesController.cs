using CoreAudit.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAudit.Controllers;

public class ServicesController : Controller
{
    private readonly IContentService _content;

    public ServicesController(IContentService content)
    {
        _content = content;
    }

    // GET /Services -> "خدماتنا"
    public IActionResult Index()
    {
        var model = _content.GetServices();
        ViewData["ProcessSteps"] = _content.GetProcessSteps();
        return View(model);
    }

    // GET /Services/Details/{slug} -> detail page for each service
    public IActionResult Details(string id)
    {
        var service = _content.GetServices().FirstOrDefault(s =>
            string.Equals(s.Slug, id, StringComparison.OrdinalIgnoreCase));

        if (service is null)
        {
            return NotFound();
        }

        return View(service);
    }
}
