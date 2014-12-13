namespace Keiser.MvxPlugins.Bluetooth.Droid.LE
{
    using Android.Bluetooth;
    using Keiser.MvxPlugins.Bluetooth.LE;
    using System;

    public class Device : DeviceBase
    {
        protected BluetoothDevice _nativeDevice;
        protected int _rssi;
        protected byte[] _advertisementData;

        public Device(BluetoothDevice nativeDevice, int rssi, byte[] scanRecord)
        {
            _nativeDevice = nativeDevice;
            _rssi = rssi;
            SetAdvertisingData(scanRecord);
        }

        protected void SetAdvertisingData(byte[] scanRecord)
        {
            int startingIndex = 0;
            // Name Section
            startingIndex += scanRecord[startingIndex] + 1;
            // Flags Section
            startingIndex += scanRecord[startingIndex] + 1;
            // Advertisment Data Section
            int length = scanRecord[startingIndex] - 1;
            startingIndex += 2;
            _advertisementData = new byte[length];
            Array.Copy(scanRecord, startingIndex, _advertisementData, 0, length);
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
                return _advertisementData;
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