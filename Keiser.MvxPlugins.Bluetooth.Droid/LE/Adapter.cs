namespace Keiser.MvxPlugins.Bluetooth.Droid.LE
{
    using Android.Bluetooth;
    using Android.Content;
    using Keiser.MvxPlugins.Bluetooth.LE;

    public class Adapter : Java.Lang.Object, IAdapter, BluetoothAdapter.ILeScanCallback
    {
        protected static Context _context = Android.App.Application.Context;
        protected static BluetoothManager _manager = (BluetoothManager)_context.GetSystemService(Context.BluetoothService);
        protected static BluetoothAdapter _adapter = _manager.Adapter;

        protected static bool _leSupported = _context.PackageManager.HasSystemFeature(Android.Content.PM.PackageManager.FeatureBluetoothLe);
        public bool LESupported { get { return _leSupported; } }

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