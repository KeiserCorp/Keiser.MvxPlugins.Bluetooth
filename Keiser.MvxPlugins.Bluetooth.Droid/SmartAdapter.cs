namespace Keiser.MvxPlugins.Bluetooth.Droid
{
    using Android.Bluetooth;
    using Android.Bluetooth.LE;
    using Android.Content;
    using Keiser.MvxPlugins.Bluetooth.Droid.LE;
    using System.Collections.Generic;
    using System.Threading;

    public class Adapter : BroadcastReceiver
    {
        private Context _context;
        public Context Context
        {
            get
            {
                if (_context == null)
                    _context = Android.App.Application.Context;
                return _context;
            }
        }

        private BluetoothManager _bluetoothManager;
        public BluetoothManager BluetoothManager
        {
            get
            {
                if (_bluetoothManager == null)
                    _bluetoothManager = (BluetoothManager)Context.GetSystemService(Context.BluetoothService);
                return _bluetoothManager;
            }
        }

        public BluetoothAdapter BluetoothAdapter
        {
            get
            {
                return BluetoothManager.Adapter;
            }
        }

        private object _leSupportedLocker = new object();
        private bool _leSupported, _leSupportedSet;
        public bool LESupported
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

        private object _isLollipopLocker = new object();
        private bool _isLollipop, _isLollipopSet;
        public bool IsLollipop
        {
            get
            {
                lock (_isLollipopLocker)
                {
                    if (!_isLollipopSet)
                    {
                        _isLollipopSet = true;
                        _isLollipop = ((int)Android.OS.Build.VERSION.SdkInt) >= 21;
                    }
                    return _isLollipop;
                }
            }
        }

        protected volatile bool AutoScan = false;
        protected volatile bool AutoEnable = false;
        protected volatile bool AutoClearCache = false;

        protected const int AdapterEnableTimeout = 6000;
        protected Bluetooth.Timer AdapterEnableTimer;

        public Adapter()
        {
            Register();
        }

        protected override void Dispose(bool disposing)
        {
            Unregister();
            base.Dispose(disposing);
        }

        protected void Register()
        {
            IntentFilter filter = new IntentFilter();
            filter.AddAction(BluetoothAdapter.ActionStateChanged);
            Context.RegisterReceiver(this, filter);
        }

        protected void Unregister()
        {
            Context.UnregisterReceiver(this);
        }

        public override void OnReceive(Context context, Intent intent)
        {
            int state = intent.GetIntExtra(BluetoothAdapter.ExtraState, BluetoothAdapter.Error);
            switch (state)
            {
                case BluetoothAdapter.Error:
                    Error("Change State Error");
                    break;
                case 12: // STATE_ON
                    Enabled();
                    break;
                case 10: // STATE_OFF
                    Disabled();
                    break;
            }
        }

        protected void Error(string message = "Unknown")
        {
            Trace.Error("Bluetooth Adapter: Error[ " + message + " ]");
            if (AdapterEnableTimer != null)
            {
                AdapterEnableTimer.Cancel();
            }
            Cycle();
        }

        protected void Enable()
        {
#if DEBUG
            Trace.Info("Bluetooth Adapter: Enable Issued");
#endif
            if (Wifi.IsEnabled)
            {
                Wifi.Disable();
            }
            if (IsEnabled)
            {
                Enabled();
            }
            else
            {
                if (BluetoothAdapter.Enable())
                {
                    AdapterEnableTimer = new Bluetooth.Timer(_ => Error("Failed To Enable"), null, AdapterEnableTimeout, Timeout.Infinite); ;
                }
                else
                {
                    Error("Enable Error");
                }
            }
        }

        protected bool IsEnabled { get { return (BluetoothAdapter.State == State.On); } }

        protected void Enabled()
        {
#if DEBUG
            Trace.Info("Bluetooth Adapter: Enabled");
#endif
            if (AdapterEnableTimer != null)
            {
                AdapterEnableTimer.Cancel();
            }
            if (AutoScan)
            {
                StartAdapterScan();
            }
        }

        protected void Disable()
        {
#if DEBUG
            Trace.Info("Bluetooth Adapter: Disable Issued");
#endif
            if (!IsEnabled)
            {
                Disabled();
            }
            else if (!BluetoothAdapter.Disable())
            {
                Error("Disable Error");
            }
        }

        protected void Disabled()
        {
#if DEBUG
            Trace.Info("Bluetooth Adapter: Disabled");
#endif
            if (AutoEnable)
            {
                AutoEnable = false;
                Enable();
            }
            if (AutoClearCache)
            {
                AutoClearCache = false;
                ClearCache();
            }
        }

        protected void Cycle()
        {
            AutoEnable = true;
            Disable();
        }

        protected CallbackQueuer CallbackQueuer;
        protected ClassicScanCallback ClassicScanCallback;
        protected LEScanCallback LEScanCallback;
        public void StartLEScan(CallbackQueuer callbackQueuer)
        {
#if DEBUG
            Trace.Info("LE Scanner: Start Issued");
#endif
            CallbackQueuer = callbackQueuer;
            AutoScan = true;
            if (!IsEnabled)
            {
                Enable();
            }
            else
            {
                StartAdapterScan();
            }
        }

        protected void StartAdapterScan()
        {
            if (IsLollipop)
            {
                LEScanCallback = new LEScanCallback(CallbackQueuer);
                ScanSettings settings = new ScanSettings.Builder().SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency).Build();
                List<ScanFilter> filters = new List<ScanFilter>() { };
                BluetoothAdapter.BluetoothLeScanner.StartScan(filters, settings, LEScanCallback);
            }
            else
            {
                ClassicScanCallback = new ClassicScanCallback(CallbackQueuer);
                if (!BluetoothAdapter.StartLeScan(ClassicScanCallback))
                {
                    Error("Start LE Scan Error");
                }
            }
#if DEBUG
            Trace.Info("LE Scanner: Starting");
#endif
        }

        public void StopLEScan()
        {
#if DEBUG
            Trace.Info("LE Scanner: Stop Issued");
#endif
            AutoScan = false;
            StopAdapterScan();
            AutoClearCache = true;
            Disable();
        }

        protected void StopAdapterScan()
        {
            if (IsLollipop)
            {
                BluetoothAdapter.BluetoothLeScanner.StopScan(LEScanCallback);
            }
            else
            {
                BluetoothAdapter.StopLeScan(ClassicScanCallback);
            }
#if DEBUG
            Trace.Info("LE Scanner: Stopping");
#endif
        }

        protected void ClearCache()
        {
#if DEBUG
            Trace.Info("Bluetooth Adapter: Starting Cache Clear");
#endif
            if (!IsEnabled)
            {
                Shell.Command("pm disable com.android.bluetooth");
                Shell.Command("am force-stop com.android.bluetooth");
                Shell.Command("rm -rf /data/misc/bluedroid/*");
                Shell.Command("pm enable com.android.bluetooth");
#if DEBUG
                Trace.Info("Bluetooth Adapter: Finished Cache Clear");
#endif
            }
#if DEBUG
            else
            {
                Trace.Info("Bluetooth Adapter: Adapter Still Active");
            }
#endif

        }


    }
}