using System.Net;
using CoreAudit.Models;
using CoreAudit.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CoreAudit.Tests;

public class ResendEmailSenderTests
{
    private static ContactFormModel ValidModel() => new()
    {
        Name = "عميل تجريبي",
        Email = "visitor@example.com",
        Phone = "0500000000",
        Service = "المراجعة الخارجية",
        Message = "هذه رسالة اختبار للتأكد من عمل النموذج."
    };

    private static ResendEmailSender BuildSender(
        FakeHttpMessageHandler handler,
        ResendSettings resend,
        EmailSettings email)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(ResendEmailSender.HttpClientName))
               .Returns(() => new HttpClient(handler));

        return new ResendEmailSender(
            factory.Object,
            Options.Create(resend),
            Options.Create(email),
            NullLogger<ResendEmailSender>.Instance);
    }

    private static ResendSettings ConfiguredResend() => new()
    {
        ApiBaseUrl = "https://api.resend.test",
        ApiKey = "TEST-FAKE-KEY-PLACEHOLDER",
        FromAddress = "Core Audit <noreply@coreauditcpa.com>"
    };

    private static EmailSettings EmailWithRecipient() => new()
    {
        Provider = "Resend",
        ToAddress = "coreauditcpa@gmail.com"
    };

    [Fact]
    public async Task Returns_Sent_when_provider_accepts_with_id()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{\"id\":\"abc-123\"}");
        var sender = BuildSender(handler, ConfiguredResend(), EmailWithRecipient());

        var result = await sender.SendContactMessageAsync(ValidModel());

        Assert.Equal(ContactDeliveryResult.Sent, result);
        Assert.Equal(1, handler.CallCount);
        // Auth header carried the key; endpoint was /emails.
        Assert.Equal("Bearer", handler.LastRequest!.Headers.Authorization!.Scheme);
        Assert.EndsWith("/emails", handler.LastRequest!.RequestUri!.AbsoluteUri);
        // The visitor's address is used as reply_to, and the body is JSON.
        Assert.Contains("reply_to", handler.LastRequestBody);
        Assert.Contains("coreauditcpa@gmail.com", handler.LastRequestBody);
    }

    [Fact]
    public async Task Returns_Failed_on_provider_error_status()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.UnprocessableEntity,
            "{\"statusCode\":422,\"message\":\"domain is not verified\"}");
        var sender = BuildSender(handler, ConfiguredResend(), EmailWithRecipient());

        var result = await sender.SendContactMessageAsync(ValidModel());

        Assert.Equal(ContactDeliveryResult.Failed, result);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task Returns_Failed_when_success_status_but_no_id()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{}");
        var sender = BuildSender(handler, ConfiguredResend(), EmailWithRecipient());

        var result = await sender.SendContactMessageAsync(ValidModel());

        Assert.Equal(ContactDeliveryResult.Failed, result);
    }

    [Fact]
    public async Task Returns_Failed_and_does_not_call_api_when_apikey_missing()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{\"id\":\"should-not-happen\"}");
        var resend = ConfiguredResend();
        resend.ApiKey = ""; // missing secret
        var sender = BuildSender(handler, resend, EmailWithRecipient());

        var result = await sender.SendContactMessageAsync(ValidModel());

        Assert.Equal(ContactDeliveryResult.Failed, result);
        Assert.Equal(0, handler.CallCount); // never hit the network
    }

    [Fact]
    public async Task Returns_Failed_when_recipient_missing()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{\"id\":\"should-not-happen\"}");
        var email = EmailWithRecipient();
        email.ToAddress = ""; // no destination
        var sender = BuildSender(handler, ConfiguredResend(), email);

        var result = await sender.SendContactMessageAsync(ValidModel());

        Assert.Equal(ContactDeliveryResult.Failed, result);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task Returns_Failed_when_network_throws()
    {
        var handler = new FakeHttpMessageHandler(_ => throw new HttpRequestException("connection reset"));
        var sender = BuildSender(handler, ConfiguredResend(), EmailWithRecipient());

        var result = await sender.SendContactMessageAsync(ValidModel());

        Assert.Equal(ContactDeliveryResult.Failed, result);
    }
}
