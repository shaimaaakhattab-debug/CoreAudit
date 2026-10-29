# Core Audit (شركة كور أوديت) — Corporate Website

A premium, Arabic-first (RTL) corporate website for **CORE AUDIT — Accountants and Auditors / شركة كور أوديت — محاسبون ومراجعون قانونيون**, built with ASP.NET Core MVC.

The site presents the firm's services, principles, process, and contact details with a professional navy + gold visual identity derived from the official logo.

---

## 1. Technology stack

| Area | Choice |
|------|--------|
| Framework | ASP.NET Core MVC |
| Target | **.NET 7.0** (matches the installed SDK, `7.0.306`) |
| Language | C# |
| Views | Razor (`.cshtml`) |
| CSS | Bootstrap 5 (RTL) + custom `wwwroot/css/site.css` |
| Icons | Bootstrap Icons (CDN) |
| Fonts | Cairo + Tajawal (Google Fonts, Arabic-friendly) |
| JS | Vanilla `wwwroot/js/site.js` + Bootstrap bundle — **no Angular/React** |
| Email | Namecheap Private Email SMTP (port 465, SSL/TLS) via **MailKit** (contact form) |
| Data | **None** — no database in this phase |

No paid services, no external accounts, no secrets committed.

---

## 2. Prerequisites

- [.NET SDK 7.0](https://dotnet.microsoft.com/download/dotnet/7.0) (confirmed installed: `7.0.306`)
- A modern web browser
- Internet access at runtime (Bootstrap, icons, and fonts load from public CDNs)

---

## 3. How to run locally

From the project folder (`d:\proj\CoreAudit`):

```powershell
dotnet restore      # first time only
dotnet build
dotnet run
```

Then open the URL shown in the console (e.g. `http://localhost:5080` or the `https://localhost:xxxx` from `launchSettings.json`).

To force a specific port:

```powershell
dotnet run --urls http://localhost:5080
```

> Development servers are long-running. Run `dotnet run` yourself in a terminal; stop it with `Ctrl+C`.

---

## 4. Project structure

```
CoreAudit/
├── Program.cs                     # App startup, DI, routing, SiteSettings binding
├── appsettings.json               # Brand + contact info (SiteSettings section) — EDIT HERE
├── Controllers/
│   ├── HomeController.cs           # الرئيسية + Privacy + Error
│   ├── AboutController.cs          # من نحن
│   ├── ServicesController.cs       # خدماتنا
│   └── ContactController.cs        # تواصل معنا (GET form + POST handler)
├── Models/
│   ├── SiteSettings.cs             # Strongly-typed brand/contact config
│   ├── ServiceItem.cs              # Service / Principle / ProcessStep types
│   ├── HomeViewModel.cs
│   └── ContactFormModel.cs         # Form model + Arabic validation rules
├── Services/
│   └── ContentService.cs           # Central Arabic content (services, principles, steps)
├── Views/
│   ├── _ViewImports.cshtml         # Injects SiteSettings into every view
│   ├── Shared/
│   │   ├── _Layout.cshtml          # Master layout (RTL <html>, meta, CSS/JS)
│   │   ├── _Header.cshtml          # Header + nav + "اطلب استشارة" CTA
│   │   ├── _Footer.cshtml          # Footer (logo, links, contact)
│   │   ├── _ServiceCard.cshtml     # Reusable service card partial
│   │   ├── _CtaSection.cshtml      # Reusable call-to-action partial
│   │   └── Error.cshtml
│   ├── Home/Index.cshtml           # Hero, services, about teaser, why-us, process, CTA
│   ├── About/Index.cshtml
│   ├── Services/Index.cshtml
│   └── Contact/Index.cshtml        # Contact details + validated form
└── wwwroot/
    ├── css/site.css                # All custom styling (navy/gold palette)
    ├── js/site.js                  # Back-to-top, mobile nav
    └── images/core-audit-logo.jpeg # Official logo (copied from Downloads)
```

### Key files to edit

- **Main layout / design shell:** `Views/Shared/_Layout.cshtml`
- **Header & footer:** `Views/Shared/_Header.cshtml`, `Views/Shared/_Footer.cshtml`
- **Page content:** the `Index.cshtml` under each `Views/<Page>/` folder
- **Colors / styling:** `wwwroot/css/site.css` (palette is in the `:root` CSS variables)
- **Services / principles / process text:** `Services/ContentService.cs`

---

## 5. How to update the logo

1. Replace the file at `wwwroot/images/core-audit-logo.jpeg` with your new logo, **or**
2. Add a new file and update `SiteSettings:LogoPath` in `appsettings.json`.

> The current logo is a JPEG photo of the logo mounted on a wall (no transparent
> background). For a crisper header, supply a **transparent PNG or SVG** of just
> the logo and point `LogoPath` at it. The code does not recreate or alter the logo.

---

## 6. How to update contact information

All brand and contact details live in **one place**: the `SiteSettings` section of `appsettings.json`.

```json
"SiteSettings": {
  "Email": "info@coreauditcpa.com",
  "Phone": "0567334550",
  "WhatsAppInternational": ""
}
```

- **Email** → used for the clickable `mailto:` links.
- **Phone** → used for the clickable `tel:` links. Kept **exactly as provided**; no country code is assumed.
- **WhatsAppInternational** → leave empty to hide the WhatsApp button. Once the
  full international number is confirmed (e.g. `+9665XXXXXXXX`), put it here and a
  WhatsApp link appears automatically in the footer and contact page.

No views need editing when contact details change.

---

## 7. Contact form & email delivery

The contact form emails each submission to **info@coreauditcpa.com** through
**Namecheap Private Email** SMTP, using the [MailKit](https://github.com/jstedfast/MailKit) library.

### How it works

- **Client-side** (jQuery unobtrusive) **and server-side** validation with Arabic
  messages, plus **antiforgery protection** (`@Html.AntiForgeryToken()` +
  `[ValidateAntiForgeryToken]`).
- **Anti-spam honeypot:** a hidden `Website` field; if a bot fills it, the submission
  is silently accepted without sending. Field lengths are capped (name ≤ 100,
  phone ≤ 30, message ≤ 2000).
- **Async sending** through `IEmailSender`. `EmailSettings:Provider` selects the
  implementation: **`Smtp`** (default, `SmtpEmailSender`) or `Resend`
  (`ResendEmailSender`, HTTPS API — use it only if the host blocks outbound SMTP).
- **SMTP transport:** `mail.privateemail.com`, port **465**, **implicit SSL/TLS**
  (TLS starts on connect). One attempt on the configured port — there is no silent
  fallback to another port.
- **Honest outcome:** the Arabic **success** message is shown **only** after the SMTP
  server has accepted the message. If sending fails (or the password is not
  configured), the visitor sees the generic Arabic **error** message — no exception
  details or secrets are exposed.
- **Backstop copy:** when sending fails, the submission is also written as a UTF-8
  text file under `App_Data/contact-messages/` (outside `wwwroot`, git-ignored, holds
  customer PII). This is a best-effort safety net, **not** a success path and not
  durable storage.
- The visitor's email is used **only as `Reply-To`** (after validation). The `From`
  address is always the authenticated company mailbox — never the visitor's address.
- **Privacy:** passwords are never logged, and submitted field values
  (name, email, phone, message) are **not written to logs**.

### SMTP configuration

Non-secret settings live in `appsettings.json` under `EmailSettings`:

```jsonc
"EmailSettings": {
  "Provider": "Smtp",
  "Host": "mail.privateemail.com",
  "Port": 465,
  "UseStartTls": false,
  "Username": "info@coreauditcpa.com",
  "FromName": "Core Audit Website",
  "FromAddress": "info@coreauditcpa.com",   // must equal Username
  "ToAddress": "info@coreauditcpa.com"      // recipient — change here to redirect enquiries
  // Password is a SECRET — do NOT put it here.
}
```

The only **secret** is the mailbox password, key `EmailSettings:Password`. It is
supplied from User Secrets (local dev) or server-side configuration (production) and
is **never committed**. It is never sent to the browser: the configuration is read
only on the server.

### Configure the password with .NET User Secrets (local dev)

Run from the project folder. The `UserSecretsId` is already set in `CoreAudit.csproj`:

```powershell
dotnet user-secrets set "EmailSettings:Password" "THE_MAILBOX_PASSWORD"
dotnet user-secrets list
```

Secrets are stored in your Windows user profile
(`%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`), **outside** the
project, so they are never committed. Restart the app after changing them.

### Production (SmarterASP.NET)

The password must be supplied on the server, not in Git. Two options:

1. **Server-only `appsettings.Production.json` (recommended with FTP deploys).**
   Create this file **on the server only**, in the site root next to `web.config`:

   ```json
   { "EmailSettings": { "Password": "THE_MAILBOX_PASSWORD" } }
   ```

   `appsettings.Production.json` is git-ignored and is **not** part of the publish
   output, so uploading the contents of `publish/` never overwrites or removes it.
   It is not served to browsers (only `wwwroot` is static). Recycle/restart the app
   after creating or editing it.

2. **Environment variable `EmailSettings__Password`**, if the SmarterASP control panel
   offers an environment-variable / application-settings screen for your plan (this
   has not been verified for SmarterASP). Setting it in the server's `web.config`
   under `<aspNetCore><environmentVariables>` also works, but `web.config` is part of
   the publish output, so an upload would overwrite it.

### Test the contact form

1. Open `/Contact` (locally, or on the live site).
2. Fill in the form with a **real email you control** (so you can confirm `Reply-To`).
3. Submit.
   - **Success:** a green Arabic message appears and the email arrives at
     `info@coreauditcpa.com`. Replying to it goes to the visitor.
   - **Failure:** a red Arabic message appears with no internal details — expected
     if the password is not set or is wrong.

### Troubleshooting

| Symptom | Likely cause & fix |
|---------|--------------------|
| Error message on every submit | `EmailSettings:Password` not set, or the app was not restarted after setting it. |
| `AuthenticationException` in logs (type only) | Wrong mailbox password, or `Username` is not the full address `info@coreauditcpa.com`. |
| `SocketException` / timeout | The host blocks outbound connections to `mail.privateemail.com:465`. Ask the host to allow it, or set `EmailSettings:Provider` to `Resend`. |
| `SslHandshakeException` | Port/mode mismatch: keep `Port` 465 (implicit SSL/TLS). |
| Rejected sender | `FromAddress` must equal the authenticated `Username`. |
| Sends but nothing arrives | The domain's MX records must point to Namecheap Private Email, and the `info@` mailbox must exist. Also check Spam. |

Logs record only the **exception type** (never credentials or message content), e.g.
`SMTP delivery failed on port 465. Error type: ...`.

---

## 8. Pages

| Page | Route | Arabic |
|------|-------|--------|
| Home | `/` | الرئيسية |
| About | `/About` | من نحن |
| Services | `/Services` | خدماتنا |
| Contact | `/Contact` | تواصل معنا |

---

## 9. SEO & accessibility

- Per-page `<title>` and `<meta name="description">`
- Semantic HTML (`header`, `main`, `nav`, `section`, `footer`) with ARIA labels
- Skip-to-content link, keyboard-focusable navigation
- Descriptive `alt` text on the logo
- Favicon support
- Correct `lang="ar"` / `dir="rtl"`

> Full WCAG conformance requires manual testing with assistive technologies and
> expert accessibility review; the above are the implemented baseline measures.

---

## 10. Deployment readiness (not deployed yet)

The domain **coreauditcpa.com** is purchased but **nothing is deployed and no DNS is changed**.

Before going live you will need:

- A host that runs **ASP.NET Core / .NET 7 (or a later LTS)** — e.g. Azure App Service,
  IIS with the ASP.NET Core Hosting Bundle, or a Linux server with Nginx + Kestrel.
- HTTPS/TLS certificate for the domain (e.g. via the host or Let's Encrypt).
- A production config (`appsettings.Production.json`, kept **out of source control**)
  for any future secrets such as email/SMTP credentials.
- Point the domain's DNS to the host **after** deployment is verified.

Publish command (for later):

```powershell
dotnet publish -c Release -o ./publish
```

> `.NET 7 reached end of support in May 2024.` For a production launch, consider
> upgrading the `TargetFramework` in `CoreAudit.csproj` to a current LTS (e.g. `net8.0`)
> once that SDK is installed. The app code is compatible with a straightforward retarget.

---

## 11. Content & compliance notes

- No company history, licenses, certifications, client names, testimonials, years of
  experience, or physical address are claimed anywhere — only the firm's name, subtitle,
  email, and phone, plus the suggested marketing copy.
- The design is original and only inspired by the general structure of professional
  accounting-firm sites; no third-party source code, branding, or images are copied.
- The site is bilingual-ready: it is Arabic/RTL by default and structured so an English/LTR
  version can be added later.
