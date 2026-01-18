# Implementation Guide

## Overview

This document provides detailed implementation information for the Office Markdown App.

## Architecture Overview

The application follows a clean, layered architecture:

1. **Presentation Layer** (Views/)
   - Android Activities that handle UI and user interactions
   - Layouts defined in XML resources

2. **Business Logic Layer** (Services/)
   - Markdown rendering and conversion
   - File storage operations (local and cloud)
   - Service interfaces for dependency injection

3. **Data Layer** (Models/)
   - Data transfer objects and models
   - Document state management

## Core Features Implementation

### 1. Raw Markdown Support with Preview

**Location**: `Views/EditorActivity.cs`, `Services/MarkdownService.cs`

The app provides three viewing modes:
- **Raw Mode**: Displays only the markdown editor
- **Preview Mode**: Shows only the rendered HTML preview
- **Split Mode**: Shows both editor and preview side-by-side

**Implementation Details:**
```csharp
// Markdown is converted to HTML using Markdig
var html = _markdownService.ConvertToHtml(markdownText);

// HTML is wrapped with CSS for beautiful rendering
var wrappedHtml = _markdownService.WrapHtmlForPreview(html);

// Displayed in WebView
_previewWebView.LoadDataWithBaseURL(null, wrappedHtml, "text/html", "UTF-8", null);
```

**Key Features:**
- Real-time preview updates as you type
- Support for all standard markdown features (headers, lists, code blocks, etc.)
- Custom CSS styling for preview
- Syntax highlighting for code blocks

### 2. WYSIWYG Editor Support

**Location**: `Views/EditorActivity.cs`

The WYSIWYG functionality is achieved through the split-view mode:
- Left pane: Raw markdown editor
- Right pane: Live preview (WYSIWYG)

Users can:
- See immediate visual feedback of their markdown
- Switch between raw and visual modes seamlessly
- Edit in raw markdown while seeing rendered output

### 3. Local Storage Support

**Location**: `Services/LocalStorageService.cs`

**Features:**
- Save markdown files to device storage
- Open existing markdown files
- Track recent files (up to 10)
- Support for "Save" and "Save As" operations

**File Operations:**
```csharp
// Save file
await File.WriteAllTextAsync(document.FilePath, document.Content);

// Open file (uses Android file picker intent)
// Implementation requires ACTION_OPEN_DOCUMENT intent
```

**Data Persistence:**
- Recent files stored in SharedPreferences
- File metadata cached for quick access
- Auto-save capability (ready for implementation)

### 4. OneDrive Integration (Optional)

**Location**: `Services/OneDriveService.cs`

**Architecture:**
- Implements `IStorageService` interface
- Uses Microsoft Graph API for file operations
- MSAL (Microsoft Authentication Library) for authentication

**Implementation Steps:**

1. **Register App in Azure AD**
   - Go to https://portal.azure.com
   - Register new application
   - Add Mobile and Desktop applications platform
   - Note the Application (client) ID

2. **Configure MSAL**
   ```csharp
   var app = PublicClientApplicationBuilder
       .Create(clientId)
       .WithAuthority(AzureCloudInstance.AzurePublic, tenant)
       .WithRedirectUri("msal{clientId}://auth")
       .Build();
   ```

3. **Authenticate User**
   ```csharp
   var result = await app.AcquireTokenInteractive(scopes)
       .WithParentActivityOrWindow(parentActivity)
       .ExecuteAsync();
   ```

4. **Access OneDrive**
   ```csharp
   var graphClient = new GraphServiceClient(authProvider);
   var files = await graphClient.Me.Drive.Root.Children.Request().GetAsync();
   ```

**Required NuGet Packages:**
- Microsoft.Graph
- Microsoft.Identity.Client

### 5. Google Drive Integration (Optional)

**Location**: `Services/GoogleDriveService.cs`

**Architecture:**
- Implements `IStorageService` interface
- Uses Google Drive API v3
- Google Sign-In for Android for authentication

**Implementation Steps:**

1. **Create Google Cloud Project**
   - Go to https://console.cloud.google.com
   - Create new project
   - Enable Google Drive API

2. **Configure OAuth 2.0**
   - Create OAuth 2.0 Client ID
   - Add Android app details
   - Note the Client ID

3. **Authenticate User**
   ```csharp
   var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
       new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
       new[] { DriveService.Scope.Drive },
       "user",
       CancellationToken.None);
   ```

4. **Access Google Drive**
   ```csharp
   var service = new DriveService(new BaseClientService.Initializer
   {
       HttpClientInitializer = credential
   });
   var files = await service.Files.List().ExecuteAsync();
   ```

**Required NuGet Packages:**
- Google.Apis.Drive.v3
- Google.Apis.Auth

## UI Components

### Main Activity (activity_main.xml)

**Purpose**: Landing page with navigation options

**Components:**
- App title and description
- "New Document" button → Opens EditorActivity
- "Open Document" button → Opens file picker
- "Settings" button → Opens settings (cloud configuration)

### Editor Activity (activity_editor.xml)

**Purpose**: Main markdown editing interface

**Layout Structure:**
```
┌─────────────────────────────────────┐
│  [Raw] [Preview] [Split]            │ ← Mode buttons
├─────────────────┬───────────────────┤
│                 │                   │
│  Markdown       │   HTML Preview    │
│  Editor         │   (WebView)       │
│  (EditText)     │                   │
│                 │                   │
└─────────────────┴───────────────────┘
```

**Components:**
- Mode selection buttons (Raw/Preview/Split)
- Markdown editor (EditText with monospace font)
- Preview pane (WebView with custom CSS)
- Menu with Save, Open, New options

## Data Models

### MarkdownDocument

**Purpose**: Represents a markdown document with metadata

**Properties:**
- `FileName`: Display name of the document
- `FilePath`: Full path on device or cloud
- `Content`: Markdown text content
- `LastModified`: Timestamp of last modification
- `IsCloudFile`: Whether stored in cloud
- `CloudProvider`: "OneDrive", "GoogleDrive", or null

## Services Architecture

### IStorageService Interface

**Purpose**: Common interface for all storage providers (local, OneDrive, Google Drive)

**Methods:**
- `OpenFileAsync()`: Browse and open a file
- `SaveFileAsync()`: Save current file
- `SaveFileAsAsync()`: Save with new name/location
- `GetRecentFilesAsync()`: Get list of recent files

**Benefits:**
- Dependency injection support
- Easy to add new storage providers
- Consistent API across providers
- Testability through mocking

### MarkdownService

**Purpose**: Handle markdown parsing and HTML generation

**Methods:**
- `ConvertToHtml(markdown)`: Convert markdown to HTML
- `WrapHtmlForPreview(html)`: Add CSS and HTML structure

**Technology:**
- Uses Markdig library (high-performance markdown processor)
- Supports GitHub Flavored Markdown (GFM)
- Extensible with custom renderers

## Permissions

### Required Permissions (AndroidManifest.xml)

```xml
<!-- Network access for cloud sync -->
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />

<!-- File system access -->
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
```

**Note**: For Android 10+ (API 29+), use scoped storage instead of WRITE_EXTERNAL_STORAGE.

## Testing

### Unit Testing Strategy

1. **Service Layer Testing**
   - Mock `IStorageService` implementations
   - Test markdown conversion accuracy
   - Verify file operations

2. **UI Testing**
   - Use Espresso for Android UI tests
   - Test mode switching
   - Verify file picker integration

### Example Test Structure

```csharp
[Test]
public void MarkdownService_ConvertToHtml_ValidMarkdown_ReturnsHtml()
{
    // Arrange
    var service = new MarkdownService();
    var markdown = "# Hello World";
    
    // Act
    var html = service.ConvertToHtml(markdown);
    
    // Assert
    Assert.Contains("<h1>Hello World</h1>", html);
}
```

## Future Enhancements

### Planned Features

1. **Syntax Highlighting in Editor**
   - Highlight markdown syntax in EditText
   - Custom TextView with SpannableString

2. **Export Options**
   - Export to PDF
   - Export to HTML file
   - Export to DOCX (using document conversion libraries)

3. **Collaboration**
   - Real-time collaboration using SignalR
   - Comments and annotations
   - Version history

4. **Advanced Markdown**
   - Math equations (KaTeX/MathJax)
   - Diagrams (Mermaid)
   - Custom extensions

5. **Offline Mode**
   - Sync queue for offline edits
   - Conflict resolution
   - Background sync service

6. **Templates**
   - Document templates (blog post, documentation, etc.)
   - Custom template creation
   - Template gallery

## Performance Optimization

### Current Optimizations

1. **Debounced Preview Updates**
   - Preview updates on text change
   - Consider adding debouncing for large documents

2. **Efficient Rendering**
   - Markdig is highly optimized
   - WebView reuses HTML structure

### Future Optimizations

1. **Large Document Handling**
   - Pagination or virtual scrolling
   - Lazy loading of preview sections
   - Background rendering

2. **Memory Management**
   - Bitmap caching for images
   - WebView memory optimization
   - Document chunking

## Security Considerations

### Current Implementation

1. **OAuth Authentication**
   - Secure token storage in KeyStore
   - Token refresh handling
   - Secure HTTPS communication

2. **File Access**
   - Scoped storage (Android 10+)
   - Permission requests
   - Secure file handling

### Best Practices

1. **Cloud Authentication**
   - Never store credentials in code
   - Use encrypted storage for tokens
   - Implement token expiration handling

2. **File Operations**
   - Validate file paths
   - Sanitize filenames
   - Check file sizes before loading

3. **WebView Security**
   - Disable JavaScript if not needed
   - Validate HTML content
   - Use Content Security Policy (CSP)

## Troubleshooting Common Issues

### Issue: Preview not updating

**Solution**: Check TextChanged event handler is attached

### Issue: WebView shows blank

**Solution**: 
- Verify JavaScript is enabled
- Check HTML structure is valid
- Ensure LoadDataWithBaseURL is called correctly

### Issue: File picker not opening

**Solution**:
- Verify permissions in AndroidManifest
- Request runtime permissions (Android 6.0+)
- Check Intent is properly configured

### Issue: Cloud authentication fails

**Solution**:
- Verify API credentials are correct
- Check redirect URIs match
- Ensure internet permission is granted

## Resources

### Official Documentation
- [.NET for Android](https://docs.microsoft.com/xamarin/android/)
- [Markdig](https://github.com/xoofx/markdig)
- [Microsoft Graph API](https://docs.microsoft.com/graph/)
- [Google Drive API](https://developers.google.com/drive/)

### Community Resources
- [Stack Overflow - Xamarin.Android](https://stackoverflow.com/questions/tagged/xamarin.android)
- [GitHub Issues](https://github.com/darthmolen/office-markdown-app/issues)

---

For more information, see the main [README.md](README.md).
