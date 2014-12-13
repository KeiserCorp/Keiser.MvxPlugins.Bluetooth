namespace Keiser.MvxPlugins.Bluetooth.Touch.LE
{
    using Keiser.MvxPlugins.Bluetooth.LE;
    using MonoTouch.CoreBluetooth;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class Adapter : IAdapter
    {
        protected static CBCentralManager _central = new CBCentralManager(MonoTouch.CoreFoundation.DispatchQueue.DefaultGlobalQueue);
        protected static Adapter _adapter = new Adapter();

        protected static bool _leSupported = true;
        public bool LESupported { get { return _leSupported; } }

        protected bool _isScanning;
        public bool IsScanning
        {
            get { return _isScanning; }
        }

        protected Adapter()
        {
            _central.DiscoveredPeripheral += (object sender, CBDiscoveredPeripheralEventArgs e) =>
            {
                if (_scanCallback != null)
                    _scanCallback.ScanCallback(new Device(e.Peripheral, e.RSSI, e.AdvertisementData));
            };
        }

        protected IScanCallback _scanCallback;
        public void StartScan(IScanCallback scanCallback)
        {
            _scanCallback = scanCallback;
            AsyncStartScan();
        }

        public void StopScan()
        {
            if(_isScanning)
            {
                _central.StopScan();
                _isScanning = false;
            }
        }

        readonly AutoResetEvent stateChanged = new AutoResetEvent(false);

        async Task WaitForState(CBCentralManagerState state)
        {
            while (_central.State != state)
            {
                await Task.Run(() => stateChanged.WaitOne());
            }
        }

        protected async void AsyncStartScan()
        {
            StopScan();
            await WaitForState(CBCentralManagerState.PoweredOn);
            _isScanning = true;
            _central.ScanForPeripherals((CBUUID)null);
        }


    }
}