# Core Audit (جوهر المراجعة) — Corporate Website

A premium, Arabic-first (RTL) corporate website for **CORE AUDIT — Accountants and Auditors / جوهر المراجعة — محاسبون ومراجعون قانونيون**, built with ASP.NET Core MVC.

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
| Email | Gmail SMTP via **MailKit** (contact form) |
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
  "Email": "coreauditcpa@gmail.com",
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

The contact form sends each submission by email to **coreauditcpa@gmail.com** using
Gmail SMTP via the [MailKit](https://github.com/jstedfast/MailKit) library.

### How it works

- **Client-side** (jQuery unobtrusive) **and server-side** validation with Arabic
  messages, plus **antiforgery protection** (`@Html.AntiForgeryToken()` +
  `[ValidateAntiForgeryToken]`).
- **Anti-spam honeypot:** a hidden `Website` field; if a bot fills it, the submission
  is silently accepted without sending. Field lengths are capped (name ≤ 100,
  phone ≤ 30, message ≤ 2000).
- **Async sending** through `IEmailSender` / `SmtpEmailSender` (registered in DI).
  The sender tries port **587 (STARTTLS)** first and automatically falls back to
  **465 (SSL)**, so it works on whichever port the network/host allows.
- **Local fallback (never lose an enquiry):** if SMTP cannot be reached at all
  (e.g. the network blocks outbound mail), the submission is saved as a UTF-8 text
  file under `App_Data/contact-messages/` — a folder **outside `wwwroot`** (not
  web-accessible) and **git-ignored** (it holds customer PII). The visitor still
  sees a genuine success message, and the firm can read the saved enquiries. As
  soon as SMTP is reachable, delivery reverts to email automatically with no code
  change. Delivery outcome is `Sent`, `SavedLocally`, or `Failed` (only `Failed`
  shows the error).
- The visitor's email is used **only as `Reply-To`** (after validation). The `From`
  address is always the authenticated Gmail account — never the visitor's address.
- The Arabic **success** message is shown only after the submission is captured
  (either emailed or saved locally).
  If sending fails, a generic Arabic **error** message is shown — no exception details
  or secrets are ever exposed.
- **Privacy:** passwords are never logged, and the submitted field values
  (name, email, phone, message) are **not written to logs**.

### Gmail SMTP configuration

Non-secret settings live in `appsettings.json` under `EmailSettings`:

```jsonc
"EmailSettings": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "UseStartTls": true,
  "FromName": "Core Audit Website",
  "FromAddress": "coreauditcpa@gmail.com",
  "ToAddress": "coreauditcpa@gmail.com"
  // Username and Password are SECRETS — do NOT put them here.
}
```

The two **secrets** come from User Secrets (local dev) or environment variables
(production) and are **never committed**:

| Key | Value |
|-----|-------|
| `EmailSettings:Username` | the Gmail account, e.g. `coreauditcpa@gmail.com` |
| `EmailSettings:Password` | a Gmail **App Password** (16 characters) |

> **Gmail App Password** (required — a normal account password will not work):
> 1. Enable **2-Step Verification** on the Gmail account.
> 2. Go to Google Account → **Security** → **App passwords**.
> 3. Generate a password for "Mail" and copy the 16-character value.
> Use that value for `EmailSettings:Password`.

### Configure secrets with .NET User Secrets (local dev)

Run from the project folder (`d:\proj\CoreAudit`). The `UserSecretsId` is already
set in `CoreAudit.csproj`:

```powershell
dotnet user-secrets init
dotnet user-secrets set "EmailSettings:Username" "coreauditcpa@gmail.com"
dotnet user-secrets set "EmailSettings:Password" "YOUR_16_CHAR_APP_PASSWORD"
```

Verify the keys are stored (does not reveal values to source control):

```powershell
dotnet user-secrets list
```

Secrets are stored in your Windows user profile
(`%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`), **outside** the
project, so they are never committed.

For **production**, set the same keys as environment variables instead, e.g.:

```powershell
setx EmailSettings__Username "coreauditcpa@gmail.com"
setx EmailSettings__Password "YOUR_16_CHAR_APP_PASSWORD"
```

(Note the double underscore `__` separator for environment variables.)

### Restart the application after configuring secrets

Configuration (including User Secrets) is read at startup, so **restart the app**
after setting the secrets:

1. In the terminal running the app, press **Ctrl+C** to stop it.
2. Start it again:

```powershell
dotnet run --project CoreAudit.csproj --urls http://localhost:5090
```

### Test the contact form locally

1. Open **http://localhost:5090/Contact**.
2. Fill in the form using a **real email you control** (so you can confirm `Reply-To`).
3. Submit.
   - **Success:** a green Arabic message appears ("تم إرسال رسالتك بنجاح…") and the
     email arrives at `coreauditcpa@gmail.com`. Replying to it goes to the visitor.
   - **Failure:** a red Arabic message appears ("تعذر إرسال رسالتك…") with no internal
     details — this is what you'll see if the secrets are not set or are wrong.

### Troubleshooting SMTP / sending failures

| Symptom | Likely cause & fix |
|---------|--------------------|
| Error message on every submit | Secrets not set, or app not restarted after setting them. Run the `user-secrets set` commands and restart. |
| `AuthenticationException` in logs (type only) | Wrong username or using the normal password. Use a **Gmail App Password**, not the account password. |
| Auth fails even with app password | 2-Step Verification not enabled on the account, or the app password was revoked/typed with spaces. Regenerate and re-set it. |
| Connection times out | Port `587` blocked by a firewall/network, or `Host`/`Port` wrong. The app auto-retries on 465. Confirm `smtp.gmail.com` and that outbound 587/465 is allowed. |
| `SocketException` / times out on BOTH 587 and 465 | The network/ISP/firewall blocks outbound SMTP entirely. The app then **saves the submission to `App_Data/contact-messages/`** so nothing is lost, and the visitor still sees success. Test from a network that allows SMTP (e.g. a phone hotspot), unblock ports 587/465, or rely on the host's mail once deployed — email resumes automatically. |
| TLS/handshake error | Ensure `UseStartTls` is `true` (Gmail on 587 requires STARTTLS). |
| Email sends but lands in Spam | Expected for a plain Gmail sender; mark as "not spam", or use a verified domain/sender for production. |
| Nothing in inbox, no error | Check the Gmail **Sent** folder and Spam; confirm `ToAddress` is correct. |

Logs intentionally record only the **exception type** (never credentials or message
content), so check the console output for a line like
`Failed to send contact message. Error type: ...` when diagnosing.

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
