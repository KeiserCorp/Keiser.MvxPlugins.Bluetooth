namespace Keiser.MvxPlugins.Bluetooth.Droid
{
    using Android.Bluetooth;
    using Android.Bluetooth.LE;
    using Android.Content;
    using Keiser.MvxPlugins.Bluetooth.Droid.LE;
    using System.Collections.Generic;

    public class SmartAdapter : BroadcastReceiver
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
                return true;
                lock (_leSupportedLocker)
                {
                    if (!_leSupportedSet)
                    {
                        _leSupportedSet = true;
                        _leSupported = Context.PackageManager.HasSystemFeature(Android.Content.PM.PackageManager.FeatureBluetoothLe);
                    }
#if DEBUG
                    Trace.Info("LE Supported: " + _leSupported);
#endif
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

        protected const int AdapterEnableTimeout = 1000;

        public SmartAdapter()
        {
            Register();
#if DEBUG
            Trace.Info("Smart Adapter: Constructed");
#endif
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
                    Error();
                    break;
                case 12: // STATE_ON
                    Enabled();
                    break;
                case 10: // STATE_OFF
                    Disabled();
                    break;
            }
        }

        protected void Error()
        {
#if DEBUG
            Trace.Info("Bluetooth Adapter: Error");
#endif
            Cycle();
        }

        protected void Enable()
        {
            if (Wifi.IsEnabled)
            {
                Wifi.Disable();
            }
            if (IsEnabled)
            {
                Enabled();
            }
            else if (!BluetoothAdapter.Enable())
            {
                Error();
            }
        }

        protected bool IsEnabled { get { return (BluetoothAdapter.State == State.On); } }


        protected void Enabled()
        {
#if DEBUG
            Trace.Info("Bluetooth Adapter: Enabled");
#endif
            if (AutoScan)
            {
                StartAdapterScan();
            }
        }

        protected void Disable()
        {
            if (!IsEnabled)
            {
                Disabled();
            }
            else if (!BluetoothAdapter.Disable())
            {
                Error();
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

        protected SmartScanCallback ScanCallback;
        public void StartLEScan(SmartScanCallback scanCallback)
        {
#if DEBUG
            Trace.Info("LE Scanner: Start Issued");
#endif
            ScanCallback = scanCallback;
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
                //ScanSettings settings = new ScanSettings.Builder().SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency).Build();
                //List<ScanFilter> filters = new List<ScanFilter>() { };
                //BluetoothAdapter.BluetoothLeScanner.StartScan(filters, settings, ScanCallback);
            }
            else
            {
                BluetoothAdapter.StartLeScan(ScanCallback);
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
                BluetoothAdapter.BluetoothLeScanner.StopScan(ScanCallback);
            }
            else
            {
                BluetoothAdapter.StopLeScan(ScanCallback);
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