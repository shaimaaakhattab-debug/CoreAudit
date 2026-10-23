using CoreAudit.Models;
using CoreAudit.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Strongly-typed site settings (brand + contact info) bound from appsettings.json.
builder.Services.Configure<SiteSettings>(builder.Configuration.GetSection("SiteSettings"));

// Email (SMTP) settings: non-secret values from appsettings.json,
// secrets (Username/Password) from User Secrets or environment variables.
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// App services via DI.
builder.Services.AddSingleton<IContentService, ContentService>();
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

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
