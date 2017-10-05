using System;
using Android.Views;
using Android.Widget;
using Android.Hardware.Display;
using Android.Util;
using Android.OS;
using Android.Content;
using Android.App;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Fragment = Android.App.Fragment;
using System.Linq;

namespace WeatherApp.Droid
{
    public class DisplayService
    {
        static string DISP_PATTERN = "overlay";
        const string LOG_TAG = "ExtDispServ";

        class DisplayListener : Java.Lang.Object, DisplayManager.IDisplayListener
        {
            DisplayService service;
            public DisplayListener(DisplayService s)
            {
                service = s;
            }
            public void OnDisplayAdded(int displayId)
            {
                Display d = service.displayManager.GetDisplay(displayId);
                Log.Debug(LOG_TAG, string.Format("Display {0:s} {1:d} added", d.Name, displayId));
                if (d.Name.ToLower().Contains(DISP_PATTERN))
                    service.Display = d;
            }
            public void OnDisplayChanged(int displayId)
            {
                Display d = service.displayManager.GetDisplay(displayId);
                Log.Debug(LOG_TAG, string.Format("Display {0:s} {1:d} changed", d.Name, displayId));
                if (service.Display != null && service.Display.DisplayId == displayId)
                    service.Display = d;
            }
            public void OnDisplayRemoved(int displayId)
            {
                Log.Debug(LOG_TAG, "Display removed");
                if (service.Display != null && service.Display.DisplayId == displayId)
                    service.Display = null;
            }
        }

        DisplayManager displayManager;
        DisplayListener displayListener;

        Display display;
        Display Display
        {
            get { return display; }
            set
            {
                if (value == null)
                {
                    Presentation = null;
                }
                else
                {
                    Presentation = new FragmentPresentation(activity, this.fragman, value);
                    Presentation.Show();
                }
                display = value;
            }
        }

        public bool DisplayPresent { get { return Display != null; } }

        Activity activity;
        public void Init(Activity activity, FragmentManager fragman)
        {
            this.activity = activity;
            displayManager = (DisplayManager)activity.GetSystemService(Context.DisplayService);
            this.fragman = fragman;
            Display = displayManager.GetDisplays().Where(
                x => x.Name.ToLower().Contains(DISP_PATTERN)).LastOrDefault();
            displayListener = new DisplayListener(this);
            displayManager.RegisterDisplayListener(displayListener, null);

        }

        private FragmentManager fragman;
        public FragmentPresentation Presentation;

        public bool ShowPage(ContentPage p)
        {
            if (Display == null)
                return false;

            if (Presentation == null)
                return false;

            Presentation.ShowPage(p);
            return true;
        }
    }

    public class FragmentPresentation : Presentation
    {
        FragmentManager fragman;

        public FragmentPresentation(Context outerContext, FragmentManager fragman, Display display) :
                base(outerContext, display)
        {
            this.fragman = fragman;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Forms.Forms.Init(this.Context, savedInstanceState);
            SetContentView(Resource.Layout.Main);
        }

        public void ShowPage(ContentPage f)
        {
            FragmentTransaction ft = fragman.BeginTransaction();
            ft.Replace(Resource.Id.mainFragment, f.CreateFragment(this.Context));

            ft.Commit();
        }
    }
}
