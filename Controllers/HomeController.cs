using CoreAudit.Models;
using CoreAudit.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CoreAudit.Controllers;

public class HomeController : Controller
{
    private readonly IContentService _content;

    public HomeController(IContentService content)
    {
        _content = content;
    }

    public IActionResult Index()
    {
        var vm = new HomeViewModel
        {
            Services = _content.GetServices(),
            Principles = _content.GetPrinciples(),
            ProcessSteps = _content.GetProcessSteps()
        };
        return View(vm);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
