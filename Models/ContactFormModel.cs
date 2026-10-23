using System.ComponentModel.DataAnnotations;

namespace CoreAudit.Models;

/// <summary>
/// Contact form model with server-side validation (Arabic messages).
/// Submissions are delivered by email via IEmailSender (see ContactController).
/// </summary>
public class ContactFormModel
{
    [Required(ErrorMessage = "الاسم مطلوب.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "يرجى إدخال اسم صحيح.")]
    [Display(Name = "الاسم")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
    [EmailAddress(ErrorMessage = "يرجى إدخال بريد إلكتروني صحيح.")]
    [Display(Name = "البريد الإلكتروني")]
    public string Email { get; set; } = "";

    [Phone(ErrorMessage = "يرجى إدخال رقم هاتف صحيح.")]
    [StringLength(30, ErrorMessage = "رقم الهاتف طويل جدًا.")]
    [Display(Name = "رقم الهاتف (اختياري)")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "يرجى اختيار الخدمة المطلوبة.")]
    [Display(Name = "الخدمة المطلوبة")]
    public string Service { get; set; } = "";

    [Required(ErrorMessage = "الرسالة مطلوبة.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "يرجى كتابة رسالة لا تقل عن 10 أحرف.")]
    [Display(Name = "الرسالة")]
    public string Message { get; set; } = "";

    /// <summary>
    /// Honeypot anti-spam field. It is hidden from humans via CSS and left empty;
    /// automated bots tend to fill every field, so a non-empty value signals spam.
    /// Not shown to users and not validated as required.
    /// </summary>
    public string? Website { get; set; }
}
