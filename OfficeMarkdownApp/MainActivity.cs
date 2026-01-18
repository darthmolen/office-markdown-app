namespace OfficeMarkdownApp;

using Android.Content;
using Android.Widget;
using OfficeMarkdownApp.Views;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_main);
        
        var btnNewDocument = FindViewById<Button>(Resource.Id.btn_new_document);
        var btnOpenDocument = FindViewById<Button>(Resource.Id.btn_open_document);
        var btnSettings = FindViewById<Button>(Resource.Id.btn_settings);
        
        if (btnNewDocument != null)
        {
            btnNewDocument.Click += (s, e) =>
            {
                var intent = new Intent(this, typeof(EditorActivity));
                StartActivity(intent);
            };
        }
        
        if (btnOpenDocument != null)
        {
            btnOpenDocument.Click += (s, e) =>
            {
                Toast.MakeText(this, "Open document - file picker will be implemented", ToastLength.Short)?.Show();
            };
        }
        
        if (btnSettings != null)
        {
            btnSettings.Click += (s, e) =>
            {
                Toast.MakeText(this, "Settings - cloud integration configuration", ToastLength.Short)?.Show();
            };
        }
    }
}