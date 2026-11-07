using System.Text.Json;
using CoreAudit.Models;
using CoreAudit.Services;
using MailKit.Security;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CoreAudit.Tests;

/// <summary>
/// SMTP (Namecheap Private Email) tests. None of these open a network connection or
/// send an email: they cover configuration, TLS mode and message addressing only.
/// Actual delivery can only be verified against the live mail server.
/// </summary>
public class SmtpEmailSenderTests
{
    private const string CompanyEmail = "info@coreauditcpa.com";

    private static ContactFormModel ValidModel() => new()
    {
        Name = "عميل تجريبي",
        Email = "visitor@example.com",
        Phone = "0500000000",
        Service = "المراجعة الخارجية",
        Message = "هذه رسالة اختبار للتأكد من عمل النموذج."
    };

    // ---- Defaults ----

    [Fact]
    public void Defaults_target_namecheap_private_email_with_company_recipient()
    {
        var s = new EmailSettings();

        Assert.Equal("Smtp", s.Provider);
        Assert.Equal("mail.privateemail.com", s.Host);
        Assert.Equal(465, s.Port);
        Assert.Equal(CompanyEmail, s.Username);
        Assert.Equal(CompanyEmail, s.FromAddress);
        Assert.Equal(CompanyEmail, s.ToAddress);
        Assert.Equal("", s.Password); // the secret never has a default
    }

    // ---- TLS mode ----

    [Fact]
    public void Port_465_uses_implicit_ssl_tls()
    {
        var s = new EmailSettings { Port = 465, UseStartTls = false };
        Assert.Equal(SecureSocketOptions.SslOnConnect, SmtpEmailSender.ResolveSecureSocketOptions(s));
    }

    [Fact]
    public void Port_465_stays_implicit_ssl_even_if_starttls_flag_is_set()
    {
        var s = new EmailSettings { Port = 465, UseStartTls = true };
        Assert.Equal(SecureSocketOptions.SslOnConnect, SmtpEmailSender.ResolveSecureSocketOptions(s));
    }

    [Fact]
    public void Other_port_uses_starttls_when_enabled()
    {
        var s = new EmailSettings { Port = 587, UseStartTls = true };
        Assert.Equal(SecureSocketOptions.StartTls, SmtpEmailSender.ResolveSecureSocketOptions(s));
    }

    // ---- Configuration / missing password ----

    [Fact]
    public void Is_not_configured_without_password()
    {
        var s = new EmailSettings { Password = "" };
        Assert.False(s.IsSmtpConfigured);
    }

    [Fact]
    public void Is_configured_when_password_is_supplied()
    {
        var s = new EmailSettings { Password = "test-only-placeholder" };
        Assert.True(s.IsSmtpConfigured);
    }

    [Fact]
    public async Task Send_fails_without_connecting_when_password_missing()
    {
        var sender = new SmtpEmailSender(
            Options.Create(new EmailSettings { Password = "" }),
            NullLogger<SmtpEmailSender>.Instance);

        var result = await sender.SendContactMessageAsync(ValidModel());

        // Returns immediately (no network attempt) and never reports success.
        Assert.Equal(ContactDeliveryResult.Failed, result);
    }

    // ---- Message addressing ----

    [Fact]
    public void Message_is_sent_from_company_mailbox_to_company_inbox_with_visitor_as_reply_to()
    {
        var settings = new EmailSettings();
        var model = ValidModel();

        var message = SmtpEmailSender.BuildMessage(settings, model);

        var from = Assert.Single(message.From.Mailboxes);
        var to = Assert.Single(message.To.Mailboxes);
        var replyTo = Assert.Single(message.ReplyTo.Mailboxes);

        Assert.Equal(CompanyEmail, from.Address);        // authenticated sender, not the visitor
        Assert.Equal(CompanyEmail, to.Address);          // company inbox
        Assert.Equal("visitor@example.com", replyTo.Address);
        Assert.NotEqual(model.Email, from.Address);
    }

    [Fact]
    public void Message_without_visitor_email_builds_with_no_reply_to()
    {
        var model = ValidModel();
        model.Email = null; // optional field left empty

        var message = SmtpEmailSender.BuildMessage(new EmailSettings(), model);

        Assert.Empty(message.ReplyTo.Mailboxes);
        Assert.Equal(CompanyEmail, Assert.Single(message.To.Mailboxes).Address);
        Assert.Contains("البريد الإلكتروني: غير محدد", message.TextBody);
        // The HTML body entity-encodes all values, so decode before checking.
        Assert.Contains("غير محدد", System.Net.WebUtility.HtmlDecode(message.HtmlBody));
    }

    [Fact]
    public void Recipient_is_configurable()
    {
        var settings = new EmailSettings { ToAddress = "accounts@coreauditcpa.com" };

        var message = SmtpEmailSender.BuildMessage(settings, ValidModel());

        Assert.Equal("accounts@coreauditcpa.com", Assert.Single(message.To.Mailboxes).Address);
    }

    // ---- Committed configuration ----

    [Fact]
    public void Committed_appsettings_has_company_email_config_and_no_password()
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(FindRepoFile("appsettings.json")));
        var root = doc.RootElement;
        var email = root.GetProperty("EmailSettings");

        Assert.Equal(CompanyEmail, root.GetProperty("SiteSettings").GetProperty("Email").GetString());
        Assert.Equal("Smtp", email.GetProperty("Provider").GetString());
        Assert.Equal("mail.privateemail.com", email.GetProperty("Host").GetString());
        Assert.Equal(465, email.GetProperty("Port").GetInt32());
        Assert.False(email.GetProperty("UseStartTls").GetBoolean());
        Assert.Equal(CompanyEmail, email.GetProperty("Username").GetString());
        Assert.Equal(CompanyEmail, email.GetProperty("FromAddress").GetString());
        Assert.Equal(CompanyEmail, email.GetProperty("ToAddress").GetString());

        // The SMTP password and the Resend API key must never be committed.
        Assert.False(email.TryGetProperty("Password", out var pw) && !string.IsNullOrEmpty(pw.GetString()));
        Assert.False(root.GetProperty("Resend").TryGetProperty("ApiKey", out var key) && !string.IsNullOrEmpty(key.GetString()));
    }

    [Fact]
    public void Committed_appsettings_has_no_old_gmail_address()
    {
        var text = File.ReadAllText(FindRepoFile("appsettings.json"));
        Assert.DoesNotContain("gmail", text, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepoFile(string name)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "CoreAudit.csproj")))
                return Path.Combine(dir.FullName, name);
            dir = dir.Parent;
        }
        throw new FileNotFoundException("Could not locate the repository root (CoreAudit.csproj).");
    }
}
