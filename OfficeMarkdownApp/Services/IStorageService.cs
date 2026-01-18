namespace OfficeMarkdownApp.Services;

using OfficeMarkdownApp.Models;

public interface IStorageService
{
    Task<MarkdownDocument?> OpenFileAsync();
    Task<bool> SaveFileAsync(MarkdownDocument document);
    Task<bool> SaveFileAsAsync(MarkdownDocument document);
    Task<List<MarkdownDocument>> GetRecentFilesAsync();
}
