# CoreAudit — Deployment Guide

Production site: https://coreauditcpa.com
Hosting: SmarterASP.NET · Application: ASP.NET Core MVC, **.NET 8**

This document describes the **safe manual FTP deployment** process (the current,
supported method) and keeps **Web Deploy** documented as a future option pending
a SmarterASP permission fix.

> There is intentionally **no GitHub Actions workflow** in the repository right
> now. Automatic deployment is on hold until the Web Deploy `contentPath`
> authorization is resolved with SmarterASP support (see the last section).

---

## 1. Manual FTP deployment (current method)

### 1.1 Build the publish output

From the repository root (`CoreAudit.csproj` is the web project):

```bash
dotnet publish CoreAudit.csproj -c Release -o publish
```

- Output folder to upload: **`./publish/`** — you upload its **contents**, not the
  folder itself.
- The output contains, at its top level:
  - `CoreAudit.dll` — the application
  - `web.config` — tells IIS / the ASP.NET Core Module how to start the app
  - dependency DLLs (`MailKit.dll`, `MimeKit.dll`, `BouncyCastle.Cryptography.dll`, …)
  - `wwwroot/` — static assets (css, js, images, local Bootstrap, favicon)
- The test project (`CoreAudit.Tests`) is **not** included in publish output — correct.
- `App_Data/` is **not** in the publish output (it is git-ignored and is runtime
  data). This is intentional — see the safety rules below.

#### 32-bit note (only if the app fails to start)

SmarterASP application pools are commonly **32-bit**. A normal framework-dependent
publish usually works. If, after upload, the live site shows **HTTP 500.30 / 500.31**
(app failed to start), re-publish self-contained for x86 and upload that instead:

```bash
dotnet publish CoreAudit.csproj -c Release -r win-x86 --self-contained true -o publish-x86
```

Then upload the contents of `./publish-x86/`.

### 1.2 Get FTP credentials and the website root

From the **SmarterASP.NET Control Panel**:

1. Open your website entry (SmarterASP website name: `HTTPCOREAUDITCP`).
2. Find the **FTP account** section — note the **FTP host**, **username**, and
   **password**. (Host is typically `ftp.smarterasp.net`; confirm in the panel.)
3. Note the **physical site root path** shown in the panel. On SmarterASP this is
   usually the **`wwwroot`** folder of your hosting space (this is the hosting
   space root, *not* the app's own `wwwroot`).

Use any FTP client: **FileZilla**, **WinSCP**, or Visual Studio
(*Publish → FTP/FTPS*). Prefer FTPS if the panel offers it.

### 1.3 Where the files go (important for ASP.NET Core)

Upload the **contents** of `./publish/` **directly into the site root**:

```
<site root>/            <-- e.g. the hosting "wwwroot" folder from the panel
├── CoreAudit.dll
├── web.config
├── *.dll                (MailKit, MimeKit, etc.)
├── CoreAudit.runtimeconfig.json
├── CoreAudit.deps.json
└── wwwroot/             <-- the APP's static files land here: <site root>/wwwroot/
    ├── css/ js/ images/ lib/ favicon.ico
└── App_Data/            <-- LEAVE UNTOUCHED (already on the server; do not delete)
```

So `web.config` and `CoreAudit.dll` sit at the site root, and the app's own
`wwwroot` becomes `<site root>/wwwroot`.

### 1.4 Safe upload procedure (protects App_Data and existing data)

1. **(Recommended) Take the app offline cleanly.** Create a local file named
   `app_offline.htm` (any simple HTML, e.g. `<h1>Updating, back shortly</h1>`) and
   upload it to the **site root** first. IIS stops the app so its DLLs are not
   locked during the copy.
2. **Upload the contents of `publish/`** to the site root, **overwriting** the
   existing application files (`CoreAudit.dll`, the other DLLs, `web.config`, and
   the `wwwroot/` assets).
3. **DO NOT touch `App_Data/`.** Configure your FTP client to **skip/exclude
   `App_Data`** so stored contact submissions are never overwritten or deleted.
   Because `App_Data` is not in the publish output, a plain "upload publish
   contents" will not create or modify it — just make sure you do not manually
   delete it on the server.
4. **DO NOT delete the remote website files** or do a "mirror/delete-extra" sync.
   Only overwrite the app files you are replacing. Leave any unrelated files and
   any server-specific configuration in place.
5. **DO NOT overwrite server-only configuration you did not intend to change.**
   If server-specific configuration files exist on the host (for example a
   server-side `appsettings.Production.json`), do not clobber them. This project
   keeps secrets in server configuration / environment variables, never in the repo.
6. **Bring the app back online:** delete `app_offline.htm` from the site root. The
   ASP.NET Core Module loads the new `CoreAudit.dll` on the next request.
   **Always remove `app_offline.htm` when you finish — even if verification fails
   or you decide to roll back** — so the site is never left offline. (If you roll
   back, remove it only after the rollback upload completes; see 1.6.)

**Files you may safely update vs. preserve:**

- **Safe to overwrite (they come from `publish/`):** `CoreAudit.dll`, the other
  application DLLs, `CoreAudit.runtimeconfig.json`, `CoreAudit.deps.json`,
  `web.config`, and everything under the app's `wwwroot/` (css, js, images, lib,
  favicon). `appsettings.json` is part of the build output and is safe to update
  (it contains no secrets).
- **Must be preserved — never overwrite or delete:** `App_Data/` (contact
  submissions and any other runtime data) and any **server-specific** files that
  live only on the host and are not produced by `publish/` (for example a
  server-side `appsettings.Production.json`, or any file a hosting tool placed in
  the site root). If it is not in your `publish/` output, do not touch it.

### 1.5 Verify the live site

After removing `app_offline.htm`:

1. Open https://coreauditcpa.com — homepage loads with correct navy/gold branding
   and Arabic RTL layout.
2. **Navigation:** the top menu links work; on mobile width the hamburger opens
   and closes (open menu, tap a link, tap outside, press Esc).
3. Visit **/About**, **/Services**, **/Contact** — all render, no 500 error.
4. Confirm **HTTPS** padlock; `http://` redirects to `https://`.
5. Static assets load (no 404s in the browser Network tab): logo image,
   `~/css/site.css`, `~/js/site.js`, `~/lib/bootstrap/...`, Bootstrap-icons font.
6. **Contact form:** submit a test message and confirm the form behaves as
   expected (success or a clear error — it must not falsely report success).
7. Confirm `App_Data` still contains any prior submissions (unchanged).

### 1.6 Roll back safely

Keep a full backup and the previous build so you can revert quickly:

1. **Before deploying, back up the complete current site root — INCLUDING
   `App_Data` and any server-specific configuration files.** Download the entire
   site root to a local folder named e.g. `backup-YYYY-MM-DD/`. The backup must
   contain `App_Data/` (all existing contact submissions) and any host-only config
   (such as a server-side `appsettings.Production.json`) so nothing is ever lost.
   This full backup is your safety net; keep it until the new deployment is
   verified.

   Separately, it is useful to also keep the **previous application build** so you
   can restore the app binaries cleanly:

   ```bash
   git stash            # if you have local changes
   git checkout <previous-commit-or-tag>
   dotnet publish CoreAudit.csproj -c Release -o publish-prev
   git checkout main
   ```

2. **To roll back:**
   1. Upload `app_offline.htm` to the site root.
   2. Re-upload the **previous** application files (from `publish-prev/`, or the
      application files in your `backup-YYYY-MM-DD/`) over the site root,
      **skipping `App_Data/`** so live data is untouched.
   3. **Remove `app_offline.htm`** so the site comes back online. Always remove it
      at the end of a rollback, even if something else went wrong — never leave the
      site offline.
3. **`App_Data` is backed up but never overwritten during deploy or rollback.** It
   holds live customer data: it is included in the backup (step 1) for safety, and
   excluded from every upload (deploy and rollback) so it is preserved intact.
4. Verify the site again using the checklist in 1.5.

---

## 2. Web Deploy (future option — currently blocked)

Automatic/Web Deploy publishing is **on hold** because deployment fails with:

```
ERROR_USER_NOT_AUTHORIZED_FOR_CONTENTPATH
```

The connection reaches the SmarterASP Web Management Service and authenticates, but
the account is **not delegated the `contentPath` provider** for the site. This is a
**hosting-side permission**, not a workflow or password problem, and cannot be fixed
by changing the deployment command.

### 2.1 What to ask SmarterASP support to confirm/enable

- Enable **Web Deploy (Web Management Service) publishing** for website
  `HTTPCOREAUDITCP` / Web Deploy site `adelalgahdly-001-site1`.
- Confirm the account is **delegated the `contentPath` provider** for that site
  (this is exactly what `ERROR_USER_NOT_AUTHORIZED_FOR_CONTENTPATH` reports).
- Provide the **exact Web Deploy publishing-profile values**: MSDeploy Service URL,
  **Site name**, **Web Deploy username**, and password — and confirm whether the
  Web Deploy username/password **differ** from the FTP / control-panel credentials
  (they often do).
- Confirm the **Web Deploy endpoint host/port** `win9060.site4now.net:8172` is
  correct for this account.

### 2.2 Known endpoint / secret format (for when it is enabled)

- Web Deploy service URL: `https://win9060.site4now.net:8172/MsDeploy.axd?site=adelalgahdly-001-site1`
- The `site` value and the `contentPath` site must both be the **Web Deploy site
  name** `adelalgahdly-001-site1` (not the display name `HTTPCOREAUDITCP`, not the
  domain).
- Credentials must be stored as **GitHub Secrets** only — never committed.

### 2.3 Re-introduction plan (only after support confirms authorization)

1. Restore a `.github/workflows/deploy.yml` configured for **`workflow_dispatch`
   only** (manual trigger; no automatic push trigger).
2. Verify the GitHub Secrets hold the Web Deploy **Site/Login/Password** values
   support confirmed.
3. Run it manually and confirm a green deploy **and** a live-site check.
4. Only then re-enable the automatic `push` trigger on the verified production
   branch (`main`).

---

## 3. Quick reference

| Item | Value |
| --- | --- |
| Web project | `CoreAudit.csproj` (.NET 8) |
| Publish command | `dotnet publish CoreAudit.csproj -c Release -o publish` |
| Upload source | contents of `./publish/` |
| Remote target | hosting site root (the panel's `wwwroot`); app files at root, assets under `wwwroot/` |
| Never touch | `App_Data/` (contact submissions), unrelated server config/secret files |
| Never do | delete remote files / mirror-delete sync / commit secrets |
| Verify | homepage, nav + hamburger, /About /Services /Contact, HTTPS, static assets, contact form, `App_Data` intact |
| Rollback | re-upload previous publish output (skip `App_Data`) behind `app_offline.htm` |
