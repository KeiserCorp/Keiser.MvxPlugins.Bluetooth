namespace Keiser.MvxPlugins.Bluetooth.Droid
{
    using Android.Bluetooth;
    using Android.Bluetooth.LE;
    using Android.Content;
    using Keiser.MvxPlugins.Bluetooth.Droid.LE;
    using System;
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
            //ClearCache();
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

        protected int FailedRecoverCount = 0;
        protected bool SuccessfullyRecovered = true;
        protected void Error(string message = "Unknown", bool hard = false)
        {
            if (SuccessfullyRecovered)
            {
                FailedRecoverCount = 0;
            }
            bool recovered = SuccessfullyRecovered;
            Trace.Error("Bluetooth Adapter: Error[ " + message + " ]");
            if (AdapterEnableTimer != null)
            {
                AdapterEnableTimer.Cancel();
            }
            SuccessfullyRecovered = false;
            FailedRecoverCount++;
            Cycle(hard || !recovered, FailedRecoverCount > 2);
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
                    AdapterEnableTimer = new Bluetooth.Timer(_ => Error("Failed To Enable", true), null, AdapterEnableTimeout, Timeout.Infinite);
                }
                else
                {
                    Error("Enable Error");
                }
            }
        }

        protected Bluetooth.Timer StartAdapterTimer;
        protected const int StartAdapterDelay = 500;

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
                StartAdapterTimer = new Bluetooth.Timer(_ => StartAdapterScan(), null, StartAdapterDelay, Timeout.Infinite);
                //StartAdapterScan();
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
            if (AutoClearCache)
            {
                AutoClearCache = false;
                ClearCache();
            }
            if (AutoEnable)
            {
                AutoEnable = false;
                Enable();
            }
        }

        protected void Cycle(bool hard = false, bool dire = false)
        {
#if DEBUG
            Trace.Info("LE Scanner: Cycling [Hard: " + hard + ", Dire: " + dire + "]");
#endif
            if (dire) {
                AutoClearCache = true;
            }
            else if (hard)
            {
                Shell.Command("am force-stop com.android.bluetooth");
            }
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
                ClassicScanCallback = new ClassicScanCallback(CallbackQueuer);
            }
        }

        protected volatile bool Running = false;
        protected volatile bool HardResetFresh = true;
        protected Bluetooth.Timer MonitorRadioTimer;
        protected void MonitorRadio(bool initial = false)
        {
            if (Running)
            {
                int activity = CallbackQueuer.ActivitySinceLastCheck;
#if DEBUG
                Trace.Info("Monitoring Activity: " + activity);
#endif
                if (HardResetFresh && activity > 0)
                {
                    HardResetFresh = false;
                }

                if (activity == 0 && !HardResetFresh)
                {
                    StopAdapterScan();
                    Disable();
                    Shell.Command("am force-stop com.android.bluetooth");
                    Enable();
                    //StartAdapterScan();
                    HardResetFresh = true;
                }
                else
                {
                    ContinueMonitorRadio(activity);
                }
            }
        }

        protected DateTime MonitorStartTime;
        protected void StartMonitorRadio(int timeout = 15000)
        {
            Running = true;
            if (MonitorRadioTimer != null)
            { MonitorRadioTimer.Cancel(); }
            MonitorRadioTimer = new Bluetooth.Timer(_ => MonitorRadio(), null, timeout, Timeout.Infinite);
            MonitorStartTime = DateTime.Now;
        }

        protected int LastActivityRatio = 0;
        protected void ContinueMonitorRadio(int activity = 0)
        {
            double elapsed = (DateTime.Now - MonitorStartTime).TotalSeconds;
            int activityRatio = (int)(activity / elapsed);

            int timeout = 10000;
            if (activityRatio >= 10)
            {
                timeout = 2000;
            }
            if (activityRatio < (LastActivityRatio * 0.75))
            {
                timeout = 1000;
            }

            LastActivityRatio = activityRatio;
            StartMonitorRadio(timeout);
        }

        protected void StopMonitorRadio()
        {
            Running = false;
            MonitorRadioTimer.Cancel();
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
                if (!BluetoothAdapter.StartLeScan(ClassicScanCallback))
                {
                    Error("Start LE Scan Error");
                }
                else
                {
                    SuccessfullyRecovered = true;
#if DEBUG
                    Trace.Info("LE Scanner: Starting");
#endif
                    StartMonitorRadio();
                }
            }
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
                StopMonitorRadio();
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
            if (true /*!IsEnabled*/)
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