using System.Text;
using System.Text.Encodings.Web;
using CoreAudit.Models;

namespace CoreAudit.Services;

/// <summary>
/// Builds the subject, HTML body and plain-text body for a contact submission.
/// Centralised so every delivery path (SMTP, Resend, local backstop) formats the
/// message identically. Every user-supplied value is HTML-encoded in the HTML
/// body to prevent HTML/script injection.
/// </summary>
public static class ContactMessageFormatter
{
    public static string BuildSubject(ContactFormModel model) =>
        $"رسالة جديدة من نموذج التواصل - {Truncate(model.Name, 60)}";

    public static string BuildHtml(ContactFormModel model)
    {
        var phone = string.IsNullOrWhiteSpace(model.Phone) ? "غير محدد" : model.Phone;

        var html = new StringBuilder();
        html.Append("<div dir=\"rtl\" style=\"font-family:Tahoma,Arial,sans-serif;font-size:14px;color:#1b2a4a;\">");
        html.Append("<h2 style=\"color:#1b2a4a;\">رسالة جديدة من نموذج التواصل</h2>");
        html.Append("<table cellpadding=\"6\" style=\"border-collapse:collapse;\">");
        AppendRow(html, "الاسم", model.Name);
        AppendRow(html, "البريد الإلكتروني", model.Email);
        AppendRow(html, "رقم الهاتف", phone);
        AppendRow(html, "الخدمة المطلوبة", model.Service);
        html.Append("</table>");
        html.Append("<h3 style=\"color:#1b2a4a;\">الرسالة</h3>");
        html.Append($"<p style=\"white-space:pre-wrap;\">{HtmlEncoder.Default.Encode(model.Message)}</p>");
        html.Append("</div>");
        return html.ToString();
    }

    public static string BuildPlainText(ContactFormModel model)
    {
        var phone = string.IsNullOrWhiteSpace(model.Phone) ? "غير محدد" : model.Phone;
        return new StringBuilder()
            .AppendLine("رسالة جديدة من نموذج التواصل")
            .AppendLine($"التاريخ: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
            .AppendLine($"الاسم: {model.Name}")
            .AppendLine($"البريد الإلكتروني: {model.Email}")
            .AppendLine($"رقم الهاتف: {phone}")
            .AppendLine($"الخدمة المطلوبة: {model.Service}")
            .AppendLine()
            .AppendLine("الرسالة:")
            .AppendLine(model.Message)
            .ToString();
    }

    private static void AppendRow(StringBuilder sb, string label, string value)
    {
        sb.Append("<tr>")
          .Append($"<td style=\"font-weight:bold;border:1px solid #e3e3e3;\">{HtmlEncoder.Default.Encode(label)}</td>")
          .Append($"<td style=\"border:1px solid #e3e3e3;\">{HtmlEncoder.Default.Encode(value)}</td>")
          .Append("</tr>");
    }

    private static string Truncate(string value, int max) =>
        string.IsNullOrEmpty(value) || value.Length <= max ? value : value.Substring(0, max);
}
