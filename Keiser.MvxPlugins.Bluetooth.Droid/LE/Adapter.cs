namespace Keiser.MvxPlugins.Bluetooth.Droid.LE
{
    using Android.Bluetooth;
    using Android.Content;
    using Keiser.MvxPlugins.Bluetooth.LE;

    public class Adapter : Java.Lang.Object, IAdapter, BluetoothAdapter.ILeScanCallback
    {
        protected Context _context = Android.App.Application.Context;
        protected BluetoothManager _manager;
        protected BluetoothAdapter _adapter;

        protected bool _leSupported = false;
        public bool LESupported { get { return _leSupported; } }

        public Adapter()
        {
            try
            {
                _leSupported = _context.PackageManager.HasSystemFeature(Android.Content.PM.PackageManager.FeatureBluetoothLe);
            }
            catch{ }
            if (LESupported)
                try
                {
                    _manager = (BluetoothManager)_context.GetSystemService(Context.BluetoothService);
                    _adapter = _manager.Adapter;
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
        }

        public void StartScan(IScanCallback scanCallback)
        {
            if (!LESupported)
                return;
            StopScan();
            _scanCallback = scanCallback;
            if (!_adapter.IsEnabled)
                _adapter.Enable();
            _isScanning = true;
            _adapter.StartLeScan(this);
        }

        public void StopScan()
        {
            if (_isScanning)
            {
                _adapter.StopLeScan(this);
                _isScanning = false;
            }
        }
    }
}