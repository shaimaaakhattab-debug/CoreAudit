using System.ComponentModel.DataAnnotations;
using CoreAudit.Models;
using Xunit;

namespace CoreAudit.Tests;

/// <summary>
/// Server-side validation rules for the contact form's mobile number field.
/// Uses the same DataAnnotations rules MVC model binding applies on POST.
/// </summary>
public class ContactFormModelValidationTests
{
    private static ContactFormModel ValidModel(string? phone) => new()
    {
        Name = "عميل تجريبي",
        Email = "visitor@example.com",
        Phone = phone,
        Service = "المراجعة الخارجية",
        Message = "هذه رسالة اختبار للتأكد من عمل النموذج."
    };

    private static List<ValidationResult> Validate(ContactFormModel model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }

    private static IEnumerable<string?> PhoneErrors(ContactFormModel model) =>
        Validate(model)
            .Where(r => r.MemberNames.Contains(nameof(ContactFormModel.Phone)))
            .Select(r => r.ErrorMessage);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_mobile_number_is_rejected_with_arabic_message(string? phone)
    {
        Assert.Contains("رقم الجوال مطلوب.", PhoneErrors(ValidModel(phone)));
    }

    [Fact]
    public void Valid_mobile_number_passes()
    {
        Assert.Empty(Validate(ValidModel("0500000000")));
    }

    [Fact]
    public void Existing_format_validation_is_kept()
    {
        Assert.Contains("يرجى إدخال رقم جوال صحيح.", PhoneErrors(ValidModel("not-a-number")));
    }

    private static IEnumerable<string?> EmailErrors(ContactFormModel model) =>
        Validate(model)
            .Where(r => r.MemberNames.Contains(nameof(ContactFormModel.Email)))
            .Select(r => r.ErrorMessage);

    [Fact]
    public void Empty_email_is_allowed_when_mobile_is_valid()
    {
        var model = ValidModel("0500000000");
        model.Email = null; // MVC binds an empty field as null
        Assert.Empty(Validate(model));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("visitor@")]
    [InlineData("@example.com")]
    public void Invalid_email_is_rejected_when_provided(string email)
    {
        var model = ValidModel("0500000000");
        model.Email = email;
        Assert.Contains("يرجى إدخال بريد إلكتروني صحيح.", EmailErrors(model));
    }

    [Fact]
    public void Email_has_no_required_rule()
    {
        var prop = typeof(ContactFormModel).GetProperty(nameof(ContactFormModel.Email))!;
        Assert.Empty(prop.GetCustomAttributes(typeof(RequiredAttribute), inherit: true));
    }

    [Fact]
    public void Existing_length_validation_is_kept()
    {
        Assert.Contains("رقم الجوال طويل جدًا.", PhoneErrors(ValidModel(new string('5', 31))));
    }
}
