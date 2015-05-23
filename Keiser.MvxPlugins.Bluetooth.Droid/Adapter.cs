namespace Keiser.MvxPlugins.Bluetooth.Droid
{
    using Android.Bluetooth;
    using Android.Content;
    using System.Threading;
    using System.Threading.Tasks;

    public static class Adapter
    {
        private static Context _context;
        public static Context Context
        {
            get
            {
                if (_context == null)
                    _context = Android.App.Application.Context;
                return _context;
            }
        }

        private static BluetoothManager _bluetoothManager;
        public static BluetoothManager BluetoothManager
        {
            get
            {
                if (_bluetoothManager == null)
                    _bluetoothManager = (BluetoothManager)Context.GetSystemService(Context.BluetoothService);
                return _bluetoothManager;
            }
        }

        public static BluetoothAdapter BluetoothAdapter
        {
            get
            {
                return BluetoothManager.Adapter;
            }
        }

        private static object _leSupportedLocker = new object();
        private static bool _leSupported, _leSupportedSet;
        public static bool LESupported
        {
            get
            {
                lock (_leSupportedLocker)
                {
                    if (!_leSupportedSet)
                    {
                        _leSupportedSet = true;
                        _leSupported = Context.PackageManager.HasSystemFeature(Android.Content.PM.PackageManager.FeatureBluetoothLe);
                    }
                    return _leSupported;
                }
            }
        }

        public static async Task ClearCache()
        {
#if DEBUG
            Trace.Info("Bluetooth Adapter: Starting Cache Clear");
#endif
            bool enabled = false;
            if (BluetoothAdapter.State != State.Off)
            {
                enabled = true;
                await Task.Run(() => Disable());
            }
            Shell.Command("pm disable com.android.bluetooth");
            Shell.Command("am force-stop com.android.bluetooth");
            Shell.Command("rm -rf /data/misc/bluedroid/*");
            Shell.Command("pm enable com.android.bluetooth");
            if (enabled)
            {
                await Task.Run(() => Enable());
            }
#if DEBUG
            Trace.Info("Bluetooth Adapter: Finished Cache Clear");
#endif
        }

        public static async Task Enable()
        {
            if (BluetoothAdapter.State != State.On)
            {
                await Task.Run(async () =>
                {
                    BluetoothAdapter.Enable();
                    while (BluetoothAdapter.State != State.On) { await Task.Delay(50); }
                });
#if DEBUG
                Trace.Info("Bluetooth Adapter: Enabled");
#endif
            }
        }

        public static async Task Disable()
        {
            if (BluetoothAdapter.State != State.Off)
            {
                await Task.Run(async () =>
                {
                    BluetoothAdapter.Disable();
                    while (BluetoothAdapter.State != State.Off) { await Task.Delay(50); }
                });
#if DEBUG
                Trace.Info("Bluetooth Adapter: Disabled");
#endif
            }
        }
    }
}