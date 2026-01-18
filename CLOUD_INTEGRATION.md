# Cloud Integration Examples

This document provides code examples for implementing OneDrive and Google Drive integration.

## OneDrive Integration

### 1. Add Required Packages

Add to `OfficeMarkdownApp.csproj`:

```xml
<PackageReference Include="Microsoft.Graph" Version="5.100.0" />
<PackageReference Include="Microsoft.Identity.Client" Version="4.81.0" />
```

### 2. Configure Azure AD

1. Register app at https://portal.azure.com
2. Note your Client ID and Tenant ID
3. Add Mobile and Desktop applications platform
4. Redirect URI: `msal{ClientId}://auth`

### 3. Implement Authentication

```csharp
using Microsoft.Identity.Client;
using Microsoft.Graph;

public class OneDriveService : IStorageService
{
    private const string ClientId = "YOUR_CLIENT_ID";
    private const string TenantId = "YOUR_TENANT_ID";
    private readonly string[] Scopes = { "Files.ReadWrite.All" };
    
    private IPublicClientApplication _msalClient;
    private GraphServiceClient _graphClient;
    
    public OneDriveService()
    {
        _msalClient = PublicClientApplicationBuilder
            .Create(ClientId)
            .WithAuthority($"https://login.microsoftonline.com/{TenantId}")
            .WithRedirectUri($"msal{ClientId}://auth")
            .Build();
    }
    
    public async Task<bool> AuthenticateAsync()
    {
        try
        {
            var accounts = await _msalClient.GetAccountsAsync();
            AuthenticationResult result;
            
            try
            {
                result = await _msalClient.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                result = await _msalClient.AcquireTokenInteractive(Scopes)
                    .WithParentActivityOrWindow(Platform.CurrentActivity)
                    .ExecuteAsync();
            }
            
            _graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(async (requestMessage) =>
                {
                    requestMessage.Headers.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);
                }));
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<List<MarkdownDocument>> GetRecentFilesAsync()
    {
        if (_graphClient == null)
            return new List<MarkdownDocument>();
        
        var files = await _graphClient.Me.Drive.Root.Children
            .Request()
            .Filter("endsWith(name,'.md')")
            .OrderBy("lastModifiedDateTime desc")
            .Top(10)
            .GetAsync();
        
        return files.Select(f => new MarkdownDocument
        {
            FileName = f.Name,
            FilePath = f.Id,
            IsCloudFile = true,
            CloudProvider = "OneDrive",
            LastModified = f.LastModifiedDateTime?.DateTime ?? DateTime.Now
        }).ToList();
    }
    
    public async Task<MarkdownDocument?> OpenFileAsync(string fileId)
    {
        if (_graphClient == null) return null;
        
        var file = await _graphClient.Me.Drive.Items[fileId]
            .Request()
            .GetAsync();
        
        using var stream = await _graphClient.Me.Drive.Items[fileId]
            .Content
            .Request()
            .GetAsync();
        
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();
        
        return new MarkdownDocument
        {
            FileName = file.Name,
            FilePath = fileId,
            Content = content,
            IsCloudFile = true,
            CloudProvider = "OneDrive",
            LastModified = file.LastModifiedDateTime?.DateTime ?? DateTime.Now
        };
    }
    
    public async Task<bool> SaveFileAsync(MarkdownDocument document)
    {
        if (_graphClient == null || string.IsNullOrEmpty(document.FilePath))
            return false;
        
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(document.Content));
        
        await _graphClient.Me.Drive.Items[document.FilePath]
            .Content
            .Request()
            .PutAsync<DriveItem>(stream);
        
        return true;
    }
}
```

## Google Drive Integration

### 1. Add Required Packages

Add to `OfficeMarkdownApp.csproj`:

```xml
<PackageReference Include="Google.Apis.Drive.v3" Version="1.73.0.3996" />
<PackageReference Include="Google.Apis.Auth" Version="1.73.0" />
```

### 2. Configure Google Cloud

1. Create project at https://console.cloud.google.com
2. Enable Google Drive API
3. Create OAuth 2.0 Client ID (Android type)
4. Note your Client ID

### 3. Implement Authentication

```csharp
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

public class GoogleDriveService : IStorageService
{
    private const string ClientId = "YOUR_CLIENT_ID.apps.googleusercontent.com";
    private const string ClientSecret = "YOUR_CLIENT_SECRET";
    private readonly string[] Scopes = { DriveService.Scope.Drive };
    
    private DriveService _driveService;
    
    public async Task<bool> AuthenticateAsync()
    {
        try
        {
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = ClientId,
                    ClientSecret = ClientSecret
                },
                Scopes,
                "user",
                CancellationToken.None);
            
            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Office Markdown App"
            });
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<List<MarkdownDocument>> GetRecentFilesAsync()
    {
        if (_driveService == null)
            return new List<MarkdownDocument>();
        
        var request = _driveService.Files.List();
        request.Q = "mimeType='text/markdown' or name contains '.md'";
        request.OrderBy = "modifiedTime desc";
        request.PageSize = 10;
        request.Fields = "files(id, name, modifiedTime)";
        
        var result = await request.ExecuteAsync();
        
        return result.Files.Select(f => new MarkdownDocument
        {
            FileName = f.Name,
            FilePath = f.Id,
            IsCloudFile = true,
            CloudProvider = "GoogleDrive",
            LastModified = f.ModifiedTime ?? DateTime.Now
        }).ToList();
    }
    
    public async Task<MarkdownDocument?> OpenFileAsync(string fileId)
    {
        if (_driveService == null) return null;
        
        var fileRequest = _driveService.Files.Get(fileId);
        var file = await fileRequest.ExecuteAsync();
        
        var stream = new MemoryStream();
        await fileRequest.DownloadAsync(stream);
        stream.Position = 0;
        
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();
        
        return new MarkdownDocument
        {
            FileName = file.Name,
            FilePath = fileId,
            Content = content,
            IsCloudFile = true,
            CloudProvider = "GoogleDrive",
            LastModified = file.ModifiedTime ?? DateTime.Now
        };
    }
    
    public async Task<bool> SaveFileAsync(MarkdownDocument document)
    {
        if (_driveService == null || string.IsNullOrEmpty(document.FilePath))
            return false;
        
        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = document.FileName
        };
        
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(document.Content));
        
        var request = _driveService.Files.Update(
            fileMetadata,
            document.FilePath,
            stream,
            "text/markdown");
        
        await request.UploadAsync();
        
        return true;
    }
    
    public async Task<bool> SaveFileAsAsync(MarkdownDocument document)
    {
        if (_driveService == null) return false;
        
        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = document.FileName ?? "untitled.md",
            MimeType = "text/markdown"
        };
        
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(document.Content));
        
        var request = _driveService.Files.Create(
            fileMetadata,
            stream,
            "text/markdown");
        
        request.Fields = "id";
        var file = await request.UploadAsync();
        
        document.FilePath = file.ResponseBody.Id;
        document.IsCloudFile = true;
        document.CloudProvider = "GoogleDrive";
        
        return true;
    }
}
```

## UI Integration Example

### Add Cloud Sync Button to MainActivity

```csharp
protected override void OnCreate(Bundle? savedInstanceState)
{
    base.OnCreate(savedInstanceState);
    SetContentView(Resource.Layout.activity_main);
    
    var btnOneDriveSync = FindViewById<Button>(Resource.Id.btn_onedrive_sync);
    var btnGoogleDriveSync = FindViewById<Button>(Resource.Id.btn_gdrive_sync);
    
    btnOneDriveSync.Click += async (s, e) =>
    {
        var oneDriveService = new OneDriveService();
        var authenticated = await oneDriveService.AuthenticateAsync();
        
        if (authenticated)
        {
            var files = await oneDriveService.GetRecentFilesAsync();
            ShowFileList(files);
        }
        else
        {
            Toast.MakeText(this, "OneDrive authentication failed", ToastLength.Short)?.Show();
        }
    };
    
    btnGoogleDriveSync.Click += async (s, e) =>
    {
        var driveService = new GoogleDriveService();
        var authenticated = await driveService.AuthenticateAsync();
        
        if (authenticated)
        {
            var files = await driveService.GetRecentFilesAsync();
            ShowFileList(files);
        }
        else
        {
            Toast.MakeText(this, "Google Drive authentication failed", ToastLength.Short)?.Show();
        }
    };
}

private void ShowFileList(List<MarkdownDocument> files)
{
    // Create dialog or new activity to show file list
    var fileNames = files.Select(f => f.FileName).ToArray();
    
    var builder = new AlertDialog.Builder(this);
    builder.SetTitle("Select File");
    builder.SetItems(fileNames, async (sender, args) =>
    {
        var selectedFile = files[args.Which];
        // Open file in editor
        await OpenCloudFile(selectedFile);
    });
    builder.Show();
}
```

## Android Manifest Updates

Add for OneDrive:

```xml
<activity 
    android:name="microsoft.identity.client.BrowserTabActivity"
    android:exported="true">
    <intent-filter>
        <action android:name="android.intent.action.VIEW" />
        <category android:name="android.intent.category.DEFAULT" />
        <category android:name="android.intent.category.BROWSABLE" />
        <data 
            android:scheme="msalYOUR_CLIENT_ID" 
            android:host="auth" />
    </intent-filter>
</activity>
```

## Testing Cloud Integration

### Mock Service for Testing

```csharp
public class MockCloudService : IStorageService
{
    private List<MarkdownDocument> _mockFiles = new()
    {
        new MarkdownDocument 
        { 
            FileName = "test.md", 
            Content = "# Test Document", 
            IsCloudFile = true 
        }
    };
    
    public Task<MarkdownDocument?> OpenFileAsync()
    {
        return Task.FromResult<MarkdownDocument?>(_mockFiles.FirstOrDefault());
    }
    
    public Task<bool> SaveFileAsync(MarkdownDocument document)
    {
        return Task.FromResult(true);
    }
    
    public Task<bool> SaveFileAsAsync(MarkdownDocument document)
    {
        _mockFiles.Add(document);
        return Task.FromResult(true);
    }
    
    public Task<List<MarkdownDocument>> GetRecentFilesAsync()
    {
        return Task.FromResult(_mockFiles);
    }
}
```

## Security Best Practices

### 1. Store Credentials Securely

```csharp
using Android.Security.Keystore;

public class SecureStorage
{
    public static void SaveToken(string token)
    {
        var prefs = Application.Context.GetSharedPreferences("secure", FileCreationMode.Private);
        var editor = prefs.Edit();
        // Encrypt token before saving
        var encrypted = EncryptToken(token);
        editor.PutString("auth_token", encrypted);
        editor.Apply();
    }
    
    private static string EncryptToken(string token)
    {
        // Use Android Keystore for encryption
        // Implementation depends on Android version
        return token; // Placeholder
    }
}
```

### 2. Handle Token Refresh

Both MSAL and Google APIs handle token refresh automatically, but you should catch authentication exceptions:

```csharp
try
{
    await cloudService.SaveFileAsync(document);
}
catch (MsalUiRequiredException)
{
    // Re-authenticate user
    await cloudService.AuthenticateAsync();
    await cloudService.SaveFileAsync(document);
}
```

## Further Resources

- [Microsoft Graph SDK Documentation](https://learn.microsoft.com/graph/sdks/sdks-overview)
- [Google Drive API Documentation](https://developers.google.com/drive/api/guides/about-sdk)
- [MSAL for Android](https://learn.microsoft.com/azure/active-directory/develop/msal-android-overview)
- [OAuth 2.0 for Mobile Apps](https://developers.google.com/identity/protocols/oauth2/native-app)

---

For complete implementation, refer to the service stubs in `Services/OneDriveService.cs` and `Services/GoogleDriveService.cs`.
