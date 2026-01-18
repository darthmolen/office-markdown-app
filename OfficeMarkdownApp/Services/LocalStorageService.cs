namespace OfficeMarkdownApp.Services;

using Android.Content;
using OfficeMarkdownApp.Models;

public class LocalStorageService : IStorageService
{
    private readonly Context _context;
    private const string RecentFilesKey = "recent_files";

    public LocalStorageService(Context context)
    {
        _context = context;
    }

    public Task<MarkdownDocument?> OpenFileAsync()
    {
        // This will be implemented with file picker intent
        // For now, returning a placeholder
        return Task.FromResult<MarkdownDocument?>(null);
    }

    public async Task<bool> SaveFileAsync(MarkdownDocument document)
    {
        try
        {
            if (string.IsNullOrEmpty(document.FilePath))
            {
                return await SaveFileAsAsync(document);
            }

            await File.WriteAllTextAsync(document.FilePath, document.Content);
            document.LastModified = DateTime.Now;
            await UpdateRecentFilesAsync(document);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Task<bool> SaveFileAsAsync(MarkdownDocument document)
    {
        // This will be implemented with file picker intent
        // For now, returning false
        return Task.FromResult(false);
    }

    public async Task<List<MarkdownDocument>> GetRecentFilesAsync()
    {
        try
        {
            var prefs = _context.GetSharedPreferences("OfficeMarkdownApp", FileCreationMode.Private);
            var recentFilesJson = prefs?.GetString(RecentFilesKey, "[]");
            
            // For simplicity, returning empty list for now
            // In a full implementation, this would deserialize JSON
            return await Task.FromResult(new List<MarkdownDocument>());
        }
        catch
        {
            return new List<MarkdownDocument>();
        }
    }

    private async Task UpdateRecentFilesAsync(MarkdownDocument document)
    {
        var recentFiles = await GetRecentFilesAsync();
        recentFiles.Insert(0, document);
        
        // Keep only last 10 files
        if (recentFiles.Count > 10)
        {
            recentFiles = recentFiles.Take(10).ToList();
        }

        var prefs = _context.GetSharedPreferences("OfficeMarkdownApp", FileCreationMode.Private);
        var editor = prefs?.Edit();
        // In a full implementation, serialize to JSON
        editor?.PutString(RecentFilesKey, "[]");
        editor?.Apply();
    }
}
