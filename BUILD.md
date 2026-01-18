# Build and Deployment Guide

## Quick Start

### Prerequisites
- Visual Studio 2022 or later (Windows/Mac)
- .NET 10.0 SDK
- Android workload for .NET

### Setup Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/darthmolen/office-markdown-app.git
   cd office-markdown-app
   ```

2. **Install .NET Android workload** (if not already installed)
   ```bash
   dotnet workload install android
   ```

3. **Open in Visual Studio**
   - Double-click `OfficeMarkdownApp.sln`
   - Visual Studio will automatically restore NuGet packages

4. **Build the solution**
   - In Visual Studio: Build → Build Solution (Ctrl+Shift+B)
   - Or via command line: `dotnet build`

5. **Run the app**
   - Select an Android emulator or connect a device
   - Press F5 or click Run
   - The app will deploy and launch

## Build Configurations

### Debug Build
```bash
dotnet build OfficeMarkdownApp.sln -c Debug
```

### Release Build
```bash
dotnet build OfficeMarkdownApp.sln -c Release
```

## Deployment Options

### Deploy to Emulator
1. Start Android emulator from Visual Studio or Android Studio
2. Run: `dotnet build -t:Run`

### Deploy to Physical Device
1. Enable Developer Options on Android device
2. Enable USB Debugging
3. Connect device via USB
4. Run: `dotnet build -t:Run`

### Create APK for Distribution
```bash
dotnet publish -f net10.0-android -c Release
```

Output: `OfficeMarkdownApp/bin/Release/net10.0-android/publish/`

## System Requirements

### Development Environment
- **OS**: Windows 10/11, macOS 10.15+, or Linux
- **IDE**: Visual Studio 2022+ or VS Code with C# Dev Kit
- **.NET**: Version 10.0 or later
- **Disk Space**: ~5GB for Android SDK and tools

### Target Devices
- **OS**: Android 7.0 (API 24) or higher
- **RAM**: 2GB minimum, 4GB recommended
- **Storage**: 50MB for app installation

## Continuous Integration

### GitHub Actions Example

```yaml
name: Build Android App

on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      
      - name: Install Android workload
        run: dotnet workload install android
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Test
        run: dotnet test --no-build
```

## Troubleshooting

### Common Build Issues

**Issue**: "Android SDK not found"
**Solution**: 
```bash
dotnet workload install android
# Or set ANDROID_HOME environment variable
```

**Issue**: "Java SDK not found"
**Solution**: Install Java JDK 11 or later

**Issue**: "Package restore failed"
**Solution**: 
```bash
dotnet nuget locals all --clear
dotnet restore --force
```

### Runtime Issues

**Issue**: App crashes on startup
**Solution**: Check Android version is 7.0+ (API 24)

**Issue**: WebView not displaying
**Solution**: Enable JavaScript in WebView settings (already configured)

**Issue**: File picker not opening
**Solution**: Grant storage permissions in app settings

## Performance Tips

### Build Performance
- Use incremental builds (default in Visual Studio)
- Close unused projects in solution
- Disable antivirus scanning of build directories

### App Performance
- Test on multiple devices
- Monitor memory usage with Android Profiler
- Optimize WebView rendering for large documents

## Next Steps

1. **Customize the app**
   - Modify colors in Resources/values/
   - Update app icon in Resources/mipmap-*/
   - Change app name in Resources/values/strings.xml

2. **Add cloud integration**
   - Follow guides in IMPLEMENTATION.md
   - Configure Azure AD for OneDrive
   - Set up Google Cloud Console for Drive

3. **Publish to Google Play**
   - Create developer account
   - Generate signed APK
   - Upload to Play Console

## Support

- **Documentation**: See README.md and IMPLEMENTATION.md
- **Issues**: https://github.com/darthmolen/office-markdown-app/issues
- **Discussions**: GitHub Discussions tab

---

**Last Updated**: 2026-01-18
**Version**: 1.0.0
