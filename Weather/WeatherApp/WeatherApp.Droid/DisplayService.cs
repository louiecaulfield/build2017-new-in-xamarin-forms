using System;
using Android.Views;
using Android.Widget;
using Android.Hardware.Display;
using Android.Util;
using Android.OS;
using Android.Content;
using Android.App;
using Android.Runtime;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Fragment = Android.App.Fragment;
using System.Linq;

namespace WeatherApp.Droid
{
    public class DisplayService
    {
        static string[] DISP_PATTERN = new string[] { "iristick", "overlay" };
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
                if (DISP_PATTERN.Contains(d.Name.ToLower()))
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
                {
                    service.Display = null;
                    if (service.displayManager != null)
                    {

                    }
                }

            }
        }

        DisplayManager displayManager;
        Context displayContext;
        DisplayListener displayListener;
        IWindowManager WindowManager;

        Display display;
        Display Display
        {
            get { return display; }
            set
            {
                if (value == null)
                {
                    WindowManager = null;
                    displayContext = null;
                    return;
                }
                displayContext = activity.CreateDisplayContext(value);
                display = value;
            }
        }
        public bool DisplayPresent { get { return Display != null; } }

        Activity activity;
        public DisplayService(Activity activity)
        {
            this.activity = activity;
            displayManager = (DisplayManager)activity.GetSystemService(Context.DisplayService);

            Display = displayManager.GetDisplays().Where(
                x =>
                    DISP_PATTERN.Where(
                        y => x.Name.ToLower().Contains(y)).Count() > 0
            ).LastOrDefault();
            displayListener = new DisplayListener(this);
            displayManager.RegisterDisplayListener(displayListener, null);
        }


        public bool ShowFragment(Fragment f)
        {
            if (displayContext == null)
                return false;

            WindowManager = displayContext.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            WindowManagerLayoutParams layoutparams =
                new WindowManagerLayoutParams(WindowManagerTypes.SystemOverlay,
                                               WindowManagerFlags.LayoutNoLimits,
                                               Android.Graphics.Format.Translucent);

            LayoutInflater vi = displayContext.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();


            Android.Views.View v = f.OnCreateView(vi, null, null);
            if (v.IsAttachedToWindow)
                WindowManager.RemoveView(v);
            WindowManager.AddView(v, layoutparams);

            return true;
        }
    }
}
