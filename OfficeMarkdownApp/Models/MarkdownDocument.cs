namespace OfficeMarkdownApp.Models;

public class MarkdownDocument
{
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime LastModified { get; set; } = DateTime.Now;
    public bool IsCloudFile { get; set; }
    public string? CloudProvider { get; set; } // "OneDrive" or "GoogleDrive"
}
