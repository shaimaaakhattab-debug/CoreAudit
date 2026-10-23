using CoreAudit.Models;
using CoreAudit.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAudit.Controllers;

public class ContactController : Controller
{
    private readonly IContentService _content;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ContactController> _logger;

    public ContactController(IContentService content, IEmailSender emailSender, ILogger<ContactController> logger)
    {
        _content = content;
        _emailSender = emailSender;
        _logger = logger;
    }

    // GET /Contact -> "تواصل معنا"
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Services"] = _content.GetServices();
        return View(new ContactFormModel());
    }

    // POST /Contact
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactFormModel model, CancellationToken cancellationToken)
    {
        ViewData["Services"] = _content.GetServices();

        // Anti-spam honeypot: real users never fill the hidden "Website" field.
        // If it's populated, silently accept without sending (don't tip off bots).
        if (!string.IsNullOrWhiteSpace(model.Website))
        {
            _logger.LogInformation("Contact form submission ignored by spam filter.");
            TempData["ContactSubmitted"] = true;
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Attempt delivery. Email is tried first; if SMTP is unavailable the
        // submission is stored locally so the enquiry is never lost.
        var result = await _emailSender.SendContactMessageAsync(model, cancellationToken);

        if (result == ContactDeliveryResult.Failed)
        {
            // No internal exception details or secrets are surfaced to the user.
            ModelState.AddModelError(string.Empty,
                "تعذر إرسال رسالتك في الوقت الحالي. يرجى المحاولة لاحقًا أو التواصل معنا عبر البريد الإلكتروني أو الهاتف.");
            return View(model);
        }

        // Sent OR SavedLocally -> the enquiry was captured; show a genuine success.
        TempData["ContactSubmitted"] = true;
        return RedirectToAction(nameof(Index));
    }
}
