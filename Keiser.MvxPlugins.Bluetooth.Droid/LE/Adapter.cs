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

        //protected static WifiManager _wifiManager;

        private const int RadioTimeout = 8000;

        protected bool _leSupported = false;
        public bool LESupported { get { return _leSupported; } }

        private object _radioToggleLocker = new object();
        private bool _toggleRadio = false;
        protected bool ToggleRadio { get { lock (_radioToggleLocker) return _toggleRadio; } set { lock (_radioToggleLocker) _toggleRadio = value; } }

        //private object _wifiEnabledLocker = new object();
        //private bool _wifiEnabled = false;
        //protected bool WifiEnabled { get { lock (_wifiEnabledLocker) return _wifiEnabled; } set { lock (_wifiEnabledLocker) _wifiEnabled = value; } }

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
                    //_wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);
                    //WifiEnabled = _wifiManager.IsWifiEnabled;
                }
                catch { }
        }

        protected object _isScanningLocker = new object();
        protected bool _isScanning;
        public bool IsScanning
        {
            get { lock (_isScanningLocker) return _isScanning; }
            protected set { lock (_isScanningLocker) _isScanning = value; }
        }

        protected IScanCallback _scanCallback;
        public void OnLeScan(BluetoothDevice device, int rssi, byte[] scanRecord)
        {
            Task.Run(() =>
                {
                    _scanCallback.ScanCallback(new Device(device, rssi, scanRecord));
                    if (ToggleRadio)
                        SetScanTimer();
                });
        }

        public void StartScan(IScanCallback scanCallback, bool toggleRadios = false)
        {
            if (!LESupported)
                return;
            StopScan();
            _scanCallback = scanCallback;
            ToggleRadio = toggleRadios;
            StartActualScan();
            IsScanning = true;
        }

        private void StartActualScan()
        {
            if (!_adapter.IsEnabled)
                _adapter.Enable();
            _adapter.StartLeScan(this);
            if (ToggleRadio)
            {
                SetScanTimer(RadioTimeout * 4);
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

//        protected void RecycleRadios()
//        {
//#if DEBUG
//            Trace.Info("Recycling Radios");
//#endif
//            if (WifiEnabled)
//            {
//                Task.Run(() =>
//                {
//                    _wifiManager.SetWifiEnabled(false);
//                    _wifiManager.SetWifiEnabled(true);
//                });
//            }
//            RecylceScan();
//        }

        protected void DisabledBLE()
        {
            _adapter.StopLeScan(this);
            _adapter.Disable();
        }

        public void StopScan()
        {
            StopActualScan();
            IsScanning = false;
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
                //RecycleRadios();
                RecylceScan();
                SetScanTimer();
            }
        }
    }
}