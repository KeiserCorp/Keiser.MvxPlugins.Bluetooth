namespace Keiser.MvxPlugins.Bluetooth.Droid.LE
{
    using Android.Bluetooth;
    using Keiser.MvxPlugins.Bluetooth.LE;

    public class Device : DeviceBase
    {
        protected BluetoothDevice _nativeDevice;
        protected int _rssi;
        protected byte[] _scanRecord;

        public Device(BluetoothDevice nativeDevice, int rssi, byte[] scanRecord)
        {
            _nativeDevice = nativeDevice;
            _rssi = rssi;
            SetAdvertisingData(scanRecord);
        }

        protected void SetAdvertisingData(byte[] scanRecord)
        {
            string print = "AdvData: ";
            foreach (var value in scanRecord)
            {
                print += "[" + value.ToString() + "] ";
            }
            Keiser.MvxPlugins.Bluetooth.Trace.Info(print);
            _scanRecord = scanRecord;
        }

        public override Address ID
        {
            get
            {
                return new Address(_nativeDevice.Address);
            }
        }

        public override string Name
        {
            get
            {
                return _nativeDevice.Name;
            }
        }

        public override int Rssi
        {
            get
            {
                return _rssi;
            }
        }

        public override byte[] AdvertisingData
        {
            get
            {
                return _scanRecord;
            }
        }

        public override object NativeDevice
        {
            get
            {
                return _nativeDevice;
            }
        }
    }
}