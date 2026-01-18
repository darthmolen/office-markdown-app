namespace OfficeMarkdownApp.Views;

using Android.App;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using OfficeMarkdownApp.Models;
using OfficeMarkdownApp.Services;

[Activity(Label = "Markdown Editor")]
public class EditorActivity : Activity
{
    private EditText? _markdownEditor;
    private WebView? _previewWebView;
    private LinearLayout? _editorLayout;
    private LinearLayout? _previewLayout;
    private LinearLayout? _splitLayout;
    private Button? _btnRawMode;
    private Button? _btnPreviewMode;
    private Button? _btnSplitMode;
    
    private MarkdownService _markdownService = new();
    private MarkdownDocument _currentDocument = new();
    private EditorMode _currentMode = EditorMode.Raw;
    
    private enum EditorMode
    {
        Raw,
        Preview,
        Split
    }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        SetContentView(Resource.Layout.activity_editor);
        
        InitializeViews();
        SetupEventHandlers();
        SetEditorMode(EditorMode.Raw);
    }

    private void InitializeViews()
    {
        _markdownEditor = FindViewById<EditText>(Resource.Id.markdown_editor);
        _previewWebView = FindViewById<WebView>(Resource.Id.preview_webview);
        _editorLayout = FindViewById<LinearLayout>(Resource.Id.editor_layout);
        _previewLayout = FindViewById<LinearLayout>(Resource.Id.preview_layout);
        _splitLayout = FindViewById<LinearLayout>(Resource.Id.split_layout);
        _btnRawMode = FindViewById<Button>(Resource.Id.btn_raw_mode);
        _btnPreviewMode = FindViewById<Button>(Resource.Id.btn_preview_mode);
        _btnSplitMode = FindViewById<Button>(Resource.Id.btn_split_mode);
        
        if (_previewWebView != null)
        {
            _previewWebView.Settings.JavaScriptEnabled = true;
            _previewWebView.Settings.LoadWithOverviewMode = true;
            _previewWebView.Settings.UseWideViewPort = true;
        }
    }

    private void SetupEventHandlers()
    {
        if (_btnRawMode != null)
            _btnRawMode.Click += (s, e) => SetEditorMode(EditorMode.Raw);
            
        if (_btnPreviewMode != null)
            _btnPreviewMode.Click += (s, e) => SetEditorMode(EditorMode.Preview);
            
        if (_btnSplitMode != null)
            _btnSplitMode.Click += (s, e) => SetEditorMode(EditorMode.Split);
            
        if (_markdownEditor != null)
        {
            _markdownEditor.TextChanged += (s, e) =>
            {
                _currentDocument.Content = _markdownEditor.Text ?? string.Empty;
                if (_currentMode != EditorMode.Raw)
                {
                    UpdatePreview();
                }
            };
        }
    }

    private void SetEditorMode(EditorMode mode)
    {
        _currentMode = mode;
        
        if (_editorLayout == null || _previewLayout == null) return;
        
        switch (mode)
        {
            case EditorMode.Raw:
                _editorLayout.Visibility = ViewStates.Visible;
                _previewLayout.Visibility = ViewStates.Gone;
                break;
                
            case EditorMode.Preview:
                _editorLayout.Visibility = ViewStates.Gone;
                _previewLayout.Visibility = ViewStates.Visible;
                UpdatePreview();
                break;
                
            case EditorMode.Split:
                _editorLayout.Visibility = ViewStates.Visible;
                _previewLayout.Visibility = ViewStates.Visible;
                UpdatePreview();
                break;
        }
    }

    private void UpdatePreview()
    {
        if (_previewWebView == null || _markdownEditor == null) return;
        
        var html = _markdownService.ConvertToHtml(_markdownEditor.Text ?? string.Empty);
        var wrappedHtml = _markdownService.WrapHtmlForPreview(html);
        _previewWebView.LoadDataWithBaseURL(null, wrappedHtml, "text/html", "UTF-8", null);
    }

    public override bool OnCreateOptionsMenu(IMenu? menu)
    {
        MenuInflater.Inflate(Resource.Menu.editor_menu, menu);
        return true;
    }

    public override bool OnOptionsItemSelected(IMenuItem item)
    {
        if (item.ItemId == Resource.Id.action_save)
        {
            SaveDocument();
            return true;
        }
        else if (item.ItemId == Resource.Id.action_open)
        {
            OpenDocument();
            return true;
        }
        else if (item.ItemId == Resource.Id.action_new)
        {
            NewDocument();
            return true;
        }
        
        return base.OnOptionsItemSelected(item);
    }

    private void SaveDocument()
    {
        // Implement save functionality
        Toast.MakeText(this, "Save functionality - to be implemented with file picker", ToastLength.Short)?.Show();
    }

    private void OpenDocument()
    {
        // Implement open functionality
        Toast.MakeText(this, "Open functionality - to be implemented with file picker", ToastLength.Short)?.Show();
    }

    private void NewDocument()
    {
        _currentDocument = new MarkdownDocument();
        if (_markdownEditor != null)
        {
            _markdownEditor.Text = string.Empty;
        }
        Toast.MakeText(this, "New document created", ToastLength.Short)?.Show();
    }
}
