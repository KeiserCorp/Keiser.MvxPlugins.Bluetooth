namespace Keiser.MvxPlugins.Bluetooth.Droid.LE
{
    using Android.Bluetooth;
    using Android.Content;
    using Android.Net.Wifi;
    using Keiser.MvxPlugins.Bluetooth.LE;
    using System;

    public class Adapter : Java.Lang.Object, IAdapter, BluetoothAdapter.ILeScanCallback
    {
        protected Context _context = Android.App.Application.Context;
        protected BluetoothManager _manager;
        protected BluetoothAdapter _adapter;

        protected static WifiManager _wifiManager;

        private const int Timeout = 8000;

        protected bool _leSupported = false;
        public bool LESupported { get { return _leSupported; } }

        private object _radioToggleLocker = new object();
        private bool _toggleRadio = false;
        protected bool ToggleRadio { get { lock (_radioToggleLocker) return _toggleRadio; } set { lock (_radioToggleLocker) _toggleRadio = value; } }

        private object _wifiEnabledLocker = new object();
        private bool _wifiEnabled = false;
        protected bool WifiEnabled { get { lock (_wifiEnabledLocker) return _wifiEnabled; } set { lock (_wifiEnabledLocker) _wifiEnabled = value; } }

        private object _radioTimerLocker = new object();
        private Timer _radioTimer;
        protected Timer RadioTimer { get { lock (_radioTimerLocker) return _radioTimer; } set { lock (_radioTimerLocker) _radioTimer = value; } }


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
                    _wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);
                    WifiEnabled = _wifiManager.IsWifiEnabled;
                }
                catch { }
        }

        protected bool _isScanning;
        public bool IsScanning
        {
            get { return _isScanning; }
        }

        protected IScanCallback _scanCallback;
        public void OnLeScan(BluetoothDevice device, int rssi, byte[] scanRecord)
        {
            _scanCallback.ScanCallback(new Device(device, rssi, scanRecord));
            SetScanTimer();
        }

        public void StartScan(IScanCallback scanCallback, bool toggleRadios = true)
        {
            if (!LESupported)
                return;
            StopScan();
            _scanCallback = scanCallback;
            ToggleRadio = toggleRadios;
            StartActualScan();
            _isScanning = true;
        }

        private void StartActualScan()
        {
            if (!_adapter.IsEnabled)
                _adapter.Enable();
            _adapter.StartLeScan(this);
            SetScanTimer();
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
            if (_isScanning)
            {
                StopActualScan();
                _isScanning = false;
            }
        }

        private void StopActualScan()
        {
            CancelScanTimer();
            _adapter.StopLeScan(this);
        }

        protected void SetScanTimer()
        {
            if (ToggleRadio)
            {
                CancelScanTimer();
                RadioTimer = new Timer(_ => RadioScanTimeout(), null, Timeout, 0);
            }
        }

        protected void CancelScanTimer()
        {
            if (RadioTimer != null)
            {
                RadioTimer.Cancel();
            }
            RadioTimer = null;
        }

        private object _radioTimeoutLocker = new object();
        private DateTime _radioTimeoutLast = DateTime.Now;
        protected DateTime RadioTimeoutLast { get { lock (_radioTimeoutLocker) return _radioTimeoutLast; } set { lock (_radioTimeoutLocker) _radioTimeoutLast = value; } }

        protected void RadioScanTimeout()
        {
#if DEBUG
            Trace.Info("Radio Timeout: " + DateTime.Now.ToLongTimeString() + " Last: " + RadioTimeoutLast.ToLongTimeString());
#endif
            // First time or last time was more than twice the timeout duration
            if (RadioTimeoutLast.AddMilliseconds(Timeout * 2) < DateTime.Now)
            {
                RecylceScan();
            }
            else
            {
                RecycleRadios();
            }
            RadioTimeoutLast = DateTime.Now;
            SetScanTimer();
        }

        public void CheckScan()
        {
#pragma warning disable 4014
            System.Threading.Tasks.Task.Run(() => RadioScanTimeout());
#pragma warning restore 4014
        }

        protected void RecycleRadios()
        {
#if DEBUG
            Trace.Info("Recycling Radios");
#endif
            //DisabledBLE();
            if (WifiEnabled)
            {
#pragma warning disable 4014
                System.Threading.Tasks.Task.Run(() =>
                {
                    _wifiManager.SetWifiEnabled(false);
                    _wifiManager.SetWifiEnabled(true);
                });
#pragma warning restore 4014
            }
            RecylceScan();
        }

        protected void DisabledBLE()
        {
            _adapter.StopLeScan(this);
            _adapter.Disable();
        }

    }
}