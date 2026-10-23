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
}
