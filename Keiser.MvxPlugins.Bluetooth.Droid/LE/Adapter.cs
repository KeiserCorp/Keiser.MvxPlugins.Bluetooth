namespace Keiser.MvxPlugins.Bluetooth.Droid.LE
{
    using Android.Bluetooth;
    using Android.Content;
    using Android.Net.Wifi;
    using Keiser.MvxPlugins.Bluetooth.LE;
    using System;
    using System.Threading.Tasks;

    public class Adapter : Java.Lang.Object, IAdapter, BluetoothAdapter.ILeScanCallback
    {
        protected Context _context = Android.App.Application.Context;
        protected BluetoothManager _manager;
        protected BluetoothAdapter _adapter;

        private const int RadioTimeout = 350;//15000;
        private const int CheckTimeout = 15000;
        private const int LongCheckTimeout = 60000;

        protected bool _leSupported = false;
        public bool LESupported { get { return _leSupported; } }

        private object _radioToggleLocker = new object();
        private bool _toggleRadio = false;
        protected bool ToggleRadio { get { lock (_radioToggleLocker) return _toggleRadio; } set { lock (_radioToggleLocker) _toggleRadio = value; } }

        private object _radioTimerLocker = new object();
        private Timer _radioTimer;
        protected Timer RadioTimer { get { lock (_radioTimerLocker) return _radioTimer; } set { lock (_radioTimerLocker) _radioTimer = value; } }

        private object _checkTimerLocker = new object();
        private Timer _checkTimer;
        protected Timer CheckTimer { get { lock (_checkTimerLocker) return _checkTimer; } set { lock (_checkTimerLocker) _checkTimer = value; } }

        protected static WifiManager WifiManager;
        private object _wifiEnabledLocker = new object();
        private bool _wifiEnabled = false;
        protected bool WifiEnabled { get { lock (_wifiEnabledLocker) return _wifiEnabled; } set { lock (_wifiEnabledLocker) _wifiEnabled = value; } }

        public Adapter()
        {
            try
            {
                _leSupported = _context.PackageManager.HasSystemFeature(Android.Content.PM.PackageManager.FeatureBluetoothLe);
            }
            catch { }
            if (LESupported)
                try
                {
                    _manager = (BluetoothManager)_context.GetSystemService(Context.BluetoothService);
                    _adapter = _manager.Adapter;
                    Task.Run(() => ClearBluedroidCache());
                    GetWifi();
                }
                catch { }
        }

        protected void GetWifi()
        {
            WifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);
            WifiEnabled = WifiManager.IsWifiEnabled;
        }

        protected void DisableWifi()
        {
            if (WifiEnabled)
                WifiManager.SetWifiEnabled(false);
        }

        protected void EnableWifi()
        {
            if (WifiEnabled)
                WifiManager.SetWifiEnabled(true);
        }

        protected object _isScanningLocker = new object();
        protected bool _isScanning = false;
        public bool IsScanning
        {
            get { lock (_isScanningLocker) return _isScanning; }
            protected set { lock (_isScanningLocker) _isScanning = value; }
        }

        protected IScanCallback _scanCallback;
        public void OnLeScan(BluetoothDevice device, int rssi, byte[] scanRecord)
        {
            Device basicDevice = new Device(device, rssi, scanRecord);
            Task.Run(() =>
                {
#if DEBUG
                    Trace.Info("Scan Found Device: " + basicDevice.ID);
#endif
                    _scanCallback.ScanCallback(basicDevice);
                    SetCheckTimer();
                });
        }

        public void StartScan(IScanCallback scanCallback, bool toggleRadios = true)
        {
            if (!LESupported)
                return;
            StopScan();
            _scanCallback = scanCallback;
            ToggleRadio = toggleRadios;
            SetCheckTimer();
            StartActualScan();
            IsScanning = true;
            Task.Run(() => DisableWifi());
        }

        private void StartActualScan()
        {
            if (!_adapter.IsEnabled)
                _adapter.Enable();
            _adapter.StartLeScan(this);
            if (ToggleRadio)
            {
                SetScanTimer();
            }
        }

        protected void RecylceScan()
        {
#if DEBUG
            Trace.Info("Recycling Scan");
#endif
            StopActualScan();
            StartActualScan();
        }

        public void StopScan()
        {
            StopActualScan();
            CancelCheckTimer();
            IsScanning = false;
            EnableWifi();
            Task.Run(() => ClearBluedroidCache());
        }

        private void StopActualScan()
        {
            CancelScanTimer();
            _adapter.StopLeScan(this);
        }

        protected void SetScanTimer(int timeout = 0)
        {
            if (timeout == 0)
                timeout = RadioTimeout;
            CancelScanTimer();
            RadioTimer = new Timer(_ => RadioScanTimeout(), null, timeout, 0);
        }

        protected void CancelScanTimer()
        {
            if (RadioTimer != null)
            {
                RadioTimer.Cancel();
            }
            RadioTimer = null;
        }

        protected void RadioScanTimeout()
        {
            if (IsScanning)
            {
                RecylceScan();
                SetScanTimer();
            }
        }

        protected void SetCheckTimer(int timeout = 0)
        {
            if (timeout == 0)
                timeout = CheckTimeout;
            CancelCheckTimer();
            CheckTimer = new Timer(_ => RadioCheckTimeout(), null, timeout, 0);
        }

        protected void CancelCheckTimer()
        {
            if (CheckTimer != null)
            {
                CheckTimer.Cancel();
            }
            CheckTimer = null;
        }

        protected void RadioCheckTimeout()
        {
            if (IsScanning)
            {
#if DEBUG
                Trace.Info("Performing Hard Radio Reset");
#endif
                CancelScanTimer();
                _adapter.StopLeScan(this);
                _adapter.Disable();
                _adapter.Enable();
                _adapter.StartLeScan(this);
                SetScanTimer();
                SetCheckTimer(LongCheckTimeout);
            }
        }

        protected void ClearBluedroidCache()
        {
#if DEBUG
            Trace.Info("Clearing Bluedroid Cache");
#endif
            _adapter.StopLeScan(this);
            _adapter.Disable();
            RunSystemCommand("pm disable com.android.bluetooth");
            RunSystemCommand("am force-stop com.android.bluetooth");
            RunSystemCommand("rm -rf /data/misc/bluedroid/*");
            RunSystemCommand("pm enable com.android.bluetooth");
            _adapter.Enable();
        }

        private static void RunSystemCommand(string command)
        {
            // Requires root access to run
            try
            {
                Java.Lang.Process process = Java.Lang.Runtime.GetRuntime().Exec("su");
                Java.IO.DataOutputStream outputStream = new Java.IO.DataOutputStream(process.OutputStream);

                outputStream.WriteBytes(command + "\n");
                outputStream.WriteBytes("exit\n");
                outputStream.Flush();
                try
                {
                    process.WaitFor();
                }
#if DEBUG
                catch (Exception e)
                {

                    Trace.Error(e.ToString());
                }
#else
                catch { }
#endif
                outputStream.Close();
            }
#if DEBUG
            catch (Exception e)
            {

                Trace.Error(e.ToString());
            }
#else
            catch { }
#endif
        }
    }
}