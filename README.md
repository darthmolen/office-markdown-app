# Office Markdown App

An Android application for editing Markdown files with WYSIWYG support, editable in Visual Studio. This app provides a powerful markdown editing experience with local storage and optional cloud integration.

## Features

✅ **Raw Markdown Support with Preview**
- Full-featured markdown editor with live preview
- Split-screen view to see markdown and rendered output simultaneously
- Supports all common markdown features through Markdig library

✅ **WYSIWYG Editor Support**
- Visual editing mode for markdown content
- Real-time preview rendering
- Switch between raw markdown and preview modes

✅ **Local Storage Support**
- Save and load markdown files from device storage
- Recent files tracking
- Auto-save functionality (placeholder for implementation)

✅ **Optional OneDrive Integration**
- Cloud storage integration with Microsoft OneDrive
- Authentication using Microsoft Identity Client (MSAL)
- Browse, open, and save files to OneDrive
- Service architecture ready for implementation

✅ **Optional Google Drive Integration**
- Cloud storage integration with Google Drive
- Google Sign-In authentication
- Browse, open, and save files to Google Drive
- Service architecture ready for implementation

## Requirements

- Visual Studio 2022 or later (Windows/Mac)
- .NET 10.0 SDK with Android workload installed
- Android SDK (automatically installed with .NET Android workload)

## Getting Started

### Prerequisites

1. **Install .NET 10.0 SDK**
   ```bash
   # Download from https://dotnet.microsoft.com/download
   ```

2. **Install Android Workload**
   ```bash
   dotnet workload install android
   ```

3. **Open in Visual Studio**
   - Open `OfficeMarkdownApp.sln` in Visual Studio 2022+
   - Visual Studio will automatically restore NuGet packages

### Building the App

#### Using Command Line
```bash
dotnet build OfficeMarkdownApp.sln
```

#### Using Visual Studio
1. Open `OfficeMarkdownApp.sln`
2. Select Build → Build Solution (or press Ctrl+Shift+B)

### Running the App

#### Using Visual Studio
1. Select an Android emulator or connect an Android device
2. Press F5 or click the "Run" button
3. The app will deploy and launch on the selected device

#### Using Command Line
```bash
# Deploy to connected device or running emulator
dotnet build OfficeMarkdownApp.sln -t:Run
```

## Project Structure

```
OfficeMarkdownApp/
├── Models/
│   └── MarkdownDocument.cs          # Data model for markdown documents
├── Services/
│   ├── IStorageService.cs           # Storage service interface
│   ├── LocalStorageService.cs       # Local file storage implementation
│   ├── MarkdownService.cs           # Markdown parsing and rendering
│   ├── OneDriveService.cs           # OneDrive integration (stub)
│   └── GoogleDriveService.cs        # Google Drive integration (stub)
├── Views/
│   └── EditorActivity.cs            # Main markdown editor activity
├── Resources/
│   ├── layout/
│   │   ├── activity_main.xml        # Main menu layout
│   │   └── activity_editor.xml      # Editor layout with split view
│   ├── menu/
│   │   └── editor_menu.xml          # Editor menu options
│   └── values/
│       └── strings.xml              # String resources
├── MainActivity.cs                   # App entry point
└── AndroidManifest.xml              # App configuration and permissions
```

## Usage

### Creating a New Document
1. Launch the app
2. Tap "New Document"
3. Start typing markdown in the editor

### Switching Modes
- **Raw Mode**: Edit raw markdown text
- **Preview Mode**: View rendered HTML output
- **Split Mode**: See both editor and preview side-by-side

### Saving Documents
1. Tap the menu icon (⋮) in the editor
2. Select "Save" or "Save As..."
3. Choose location (implementation uses Android file picker)

### Cloud Integration (Optional)
To enable cloud integration, you need to:

#### OneDrive Integration
1. Register app in Azure AD (https://portal.azure.com)
2. Add Microsoft.Graph and Microsoft.Identity.Client NuGet packages
3. Configure authentication in `OneDriveService.cs`
4. Implement authentication flow and file operations

#### Google Drive Integration
1. Create project in Google Cloud Console
2. Enable Google Drive API
3. Add Google.Apis.Drive.v3 NuGet package
4. Configure OAuth 2.0 credentials
5. Implement authentication in `GoogleDriveService.cs`

## NuGet Packages Used

- **Markdig (0.44.0)**: Markdown parsing and HTML conversion
- **Microsoft.Graph**: OneDrive integration (add for cloud features)
- **Microsoft.Identity.Client**: Microsoft authentication (add for cloud features)
- **Google.Apis.Drive.v3**: Google Drive integration (add for cloud features)

## Architecture

The app follows a clean architecture pattern:

- **Models**: Data structures for documents
- **Services**: Business logic and external integrations
  - `IStorageService`: Common interface for all storage providers
  - `LocalStorageService`: Local file system operations
  - `OneDriveService`: OneDrive cloud storage (stub)
  - `GoogleDriveService`: Google Drive cloud storage (stub)
  - `MarkdownService`: Markdown rendering using Markdig
- **Views**: Android activities and UI components

## Development Notes

### Visual Studio Compatibility
This project is fully compatible with:
- Visual Studio 2022+ for Windows
- Visual Studio 2022+ for Mac
- Visual Studio Code (with C# Dev Kit extension)

### Extending the App

#### Adding Custom Markdown Styles
Edit the CSS in `MarkdownService.WrapHtmlForPreview()` method to customize the preview appearance.

#### Implementing Cloud Services
1. Implement the authentication methods in cloud service classes
2. Add required NuGet packages
3. Configure API credentials (Azure AD for OneDrive, Google Cloud for Drive)
4. Implement file browser UI for cloud files

#### Adding More Features
- Export to PDF
- Markdown syntax highlighting in editor
- Custom markdown templates
- Collaboration features
- Version control integration

## Troubleshooting

### Build Errors
- Ensure .NET 10.0 SDK is installed
- Verify Android workload is installed: `dotnet workload list`
- Clean and rebuild: `dotnet clean && dotnet build`

### Deployment Issues
- Check Android emulator is running
- Verify device is connected: `adb devices`
- Ensure USB debugging is enabled on physical device

## License

BSD 2-Clause License - See LICENSE file for details

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues.

## Support

For issues and questions:
- GitHub Issues: [Create an issue](https://github.com/darthmolen/office-markdown-app/issues)
- Documentation: See this README

---

**Note**: Cloud integration services (OneDrive and Google Drive) are provided as architectural stubs. Full implementation requires additional configuration and API credentials.
