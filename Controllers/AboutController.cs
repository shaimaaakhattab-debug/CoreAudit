using CoreAudit.Models;
using CoreAudit.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAudit.Controllers;

public class AboutController : Controller
{
    private readonly IContentService _content;

    public AboutController(IContentService content)
    {
        _content = content;
    }

    // GET /About  -> "من نحن"
    public IActionResult Index()
    {
        ViewData["Principles"] = _content.GetPrinciples();
        return View();
    }
}
