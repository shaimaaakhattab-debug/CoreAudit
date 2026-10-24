using Microsoft.AspNetCore.Mvc;

namespace CoreAudit.Controllers;

/// <summary>
/// Dedicated page honouring HRH Crown Prince Mohammed bin Salman.
/// Route: /CrownPrince
/// </summary>
public class CrownPrinceController : Controller
{
    // GET /CrownPrince  -> "ولي العهد الأمير محمد بن سلمان"
    public IActionResult Index() => View();
}
