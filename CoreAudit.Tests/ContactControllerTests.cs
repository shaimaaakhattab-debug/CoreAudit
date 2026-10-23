using CoreAudit.Controllers;
using CoreAudit.Models;
using CoreAudit.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CoreAudit.Tests;

public class ContactControllerTests
{
    private static ContactFormModel ValidModel() => new()
    {
        Name = "عميل تجريبي",
        Email = "visitor@example.com",
        Service = "المراجعة الخارجية",
        Message = "هذه رسالة اختبار للتأكد من عمل النموذج."
    };

    private static ContactController BuildController(
        Mock<IEmailSender> sender,
        Mock<IContactMessageStore> store)
    {
        var content = new Mock<IContentService>();
        content.Setup(c => c.GetServices()).Returns(new List<ServiceItem>());

        var controller = new ContactController(
            content.Object, sender.Object, store.Object, NullLogger<ContactController>.Instance)
        {
            TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
        };
        return controller;
    }

    [Fact]
    public async Task Successful_delivery_reports_success_and_redirects()
    {
        var sender = new Mock<IEmailSender>();
        sender.Setup(s => s.SendContactMessageAsync(It.IsAny<ContactFormModel>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(ContactDeliveryResult.Sent);
        var store = new Mock<IContactMessageStore>();

        var controller = BuildController(sender, store);
        var result = await controller.Index(ValidModel(), CancellationToken.None);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ContactController.Index), redirect.ActionName);
        Assert.True(controller.TempData["ContactSubmitted"] as bool?);
        // No backstop file is written on success.
        store.Verify(s => s.TrySave(It.IsAny<ContactFormModel>()), Times.Never);
    }

    [Fact]
    public async Task Failed_delivery_does_not_report_success_and_saves_backstop()
    {
        var sender = new Mock<IEmailSender>();
        sender.Setup(s => s.SendContactMessageAsync(It.IsAny<ContactFormModel>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(ContactDeliveryResult.Failed);
        var store = new Mock<IContactMessageStore>();

        var controller = BuildController(sender, store);
        var result = await controller.Index(ValidModel(), CancellationToken.None);

        // Returns the view with an error — NOT a success redirect.
        var view = Assert.IsType<ViewResult>(result);
        Assert.Null(controller.TempData["ContactSubmitted"]);
        Assert.False(controller.ModelState.IsValid);
        // The enquiry is retained as a backstop.
        store.Verify(s => s.TrySave(It.IsAny<ContactFormModel>()), Times.Once);
    }

    [Fact]
    public async Task Invalid_input_does_not_attempt_delivery()
    {
        var sender = new Mock<IEmailSender>();
        var store = new Mock<IContactMessageStore>();

        var controller = BuildController(sender, store);
        controller.ModelState.AddModelError(nameof(ContactFormModel.Email), "البريد الإلكتروني مطلوب.");

        var result = await controller.Index(new ContactFormModel(), CancellationToken.None);

        Assert.IsType<ViewResult>(result);
        sender.Verify(s => s.SendContactMessageAsync(It.IsAny<ContactFormModel>(), It.IsAny<CancellationToken>()), Times.Never);
        store.Verify(s => s.TrySave(It.IsAny<ContactFormModel>()), Times.Never);
    }

    [Fact]
    public async Task Honeypot_filled_is_silently_accepted_without_sending()
    {
        var sender = new Mock<IEmailSender>();
        var store = new Mock<IContactMessageStore>();

        var controller = BuildController(sender, store);
        var model = ValidModel();
        model.Website = "http://spam.example"; // bot filled the hidden field

        var result = await controller.Index(model, CancellationToken.None);

        Assert.IsType<RedirectToActionResult>(result);
        sender.Verify(s => s.SendContactMessageAsync(It.IsAny<ContactFormModel>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
