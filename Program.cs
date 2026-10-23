using CoreAudit.Models;
using CoreAudit.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Strongly-typed site settings (brand + contact info) bound from appsettings.json.
builder.Services.Configure<SiteSettings>(builder.Configuration.GetSection("SiteSettings"));

// Email settings: non-secret values from appsettings.json; secrets (SMTP
// Username/Password, Resend API key) from User Secrets or environment variables.
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<ResendSettings>(builder.Configuration.GetSection("Resend"));

// App services via DI.
builder.Services.AddSingleton<IContentService, ContentService>();

// Local backstop store for submissions that could not be delivered.
builder.Services.AddSingleton<IContactMessageStore, FileContactMessageStore>();

// Choose the email delivery provider. Default is "Resend" (HTTPS API) because the
// production host blocks outbound SMTP. Set EmailSettings:Provider to "Smtp" to use
// direct SMTP (e.g. local development).
var emailProvider = builder.Configuration["EmailSettings:Provider"] ?? "Resend";
if (string.Equals(emailProvider, "Smtp", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
}
else
{
    builder.Services.AddHttpClient(ResendEmailSender.HttpClientName);
    builder.Services.AddTransient<IEmailSender, ResendEmailSender>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. Adjust for production as needed.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
