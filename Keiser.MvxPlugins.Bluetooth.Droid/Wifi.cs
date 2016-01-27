namespace Keiser.MvxPlugins.Bluetooth.Droid
{
    using Android.Content;
    using Android.Net.Wifi;

    public static class Wifi
    {
        public static WifiManager WifiManager
        {
            get
            {
                return (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);
            }
        }

        public static void Enable()
        {
            WifiManager.SetWifiEnabled(true);
        }

        public static void Disable()
        {
            WifiManager.SetWifiEnabled(false);
        }

        public static bool IsEnabled
        {
            get
            {
                var state = WifiManager.WifiState;
                return (state == Android.Net.WifiState.Enabled);
            }
        }
    }
}