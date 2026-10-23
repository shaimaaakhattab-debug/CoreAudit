using System.Text;
using CoreAudit.Models;

namespace CoreAudit.Services;

/// <summary>
/// Backstop persistence for contact submissions. This is NOT a success path:
/// it only exists so an enquiry is not lost entirely when the email provider
/// is unreachable. The controller still reports failure to the visitor when a
/// message could only be stored locally (never a false "sent" message).
///
/// NOTE: on hosts that recycle or redeploy the site, files under App_Data can be
/// wiped, so this is a best-effort safety net, not durable storage. The primary
/// delivery path is the email provider.
/// </summary>
public interface IContactMessageStore
{
    /// <summary>Writes the submission to disk. Returns true on success. Never throws.</summary>
    bool TrySave(ContactFormModel model);
}

public class FileContactMessageStore : IContactMessageStore
{
    private readonly ILogger<FileContactMessageStore> _logger;
    private readonly string _inboxDirectory;

    public FileContactMessageStore(ILogger<FileContactMessageStore> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        // Stored outside wwwroot so saved enquiries are never web-accessible.
        _inboxDirectory = Path.Combine(env.ContentRootPath, "App_Data", "contact-messages");
    }

    public bool TrySave(ContactFormModel model)
    {
        try
        {
            Directory.CreateDirectory(_inboxDirectory);

            var stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var fileName = $"{stamp}_{Guid.NewGuid():N}.txt";
            var path = Path.Combine(_inboxDirectory, fileName);

            File.WriteAllText(path, ContactMessageFormatter.BuildPlainText(model),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

            // Do NOT log the submission content.
            _logger.LogInformation("Contact submission stored locally as a backstop (content not logged).");
            return true;
        }
        catch (Exception ex)
        {
            // Type only — never the submission content.
            _logger.LogError("Failed to store contact submission locally. Error type: {ErrorType}", ex.GetType().Name);
            return false;
        }
    }
}
