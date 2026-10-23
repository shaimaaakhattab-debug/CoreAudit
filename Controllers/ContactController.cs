using CoreAudit.Models;
using CoreAudit.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreAudit.Controllers;

public class ContactController : Controller
{
    private readonly IContentService _content;
    private readonly IEmailSender _emailSender;
    private readonly IContactMessageStore _messageStore;
    private readonly ILogger<ContactController> _logger;

    public ContactController(
        IContentService content,
        IEmailSender emailSender,
        IContactMessageStore messageStore,
        ILogger<ContactController> logger)
    {
        _content = content;
        _emailSender = emailSender;
        _messageStore = messageStore;
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

        // Attempt delivery via the configured email provider.
        var result = await _emailSender.SendContactMessageAsync(model, cancellationToken);

        if (result != ContactDeliveryResult.Sent)
        {
            // Delivery did NOT succeed. Save a local backstop copy so the enquiry
            // is not lost entirely, but DO NOT report success to the visitor.
            _messageStore.TrySave(model);

            // No internal exception details or secrets are surfaced to the user.
            ModelState.AddModelError(string.Empty,
                "تعذر إرسال رسالتك في الوقت الحالي. يرجى المحاولة لاحقًا أو التواصل معنا عبر البريد الإلكتروني أو الهاتف.");
            return View(model);
        }

        // The provider accepted the message -> genuine success.
        TempData["ContactSubmitted"] = true;
        return RedirectToAction(nameof(Index));
    }
}
