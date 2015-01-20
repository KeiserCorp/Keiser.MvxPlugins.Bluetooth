namespace Keiser.MvxPlugins.Bluetooth.Droid.LE
{
    using Android.Bluetooth;
    using Keiser.MvxPlugins.Bluetooth.LE;
    using System;

    public class Device : DeviceBase
    {
        protected BluetoothDevice _nativeDevice;
        protected int _rssi;
        protected byte[] _advertisementData = new byte[0];

        public Device(BluetoothDevice nativeDevice, int rssi, byte[] scanRecord)
        {
            _nativeDevice = nativeDevice;
            _rssi = rssi;
            SetAdvertisingData(scanRecord);
        }

        protected void SetAdvertisingData(byte[] scanRecord)
        {
            int startingIndex = 0;
            if (scanRecord.Length > 3)
            {
                while ((startingIndex + 1) < scanRecord.Length && scanRecord[startingIndex + 1] != 0xff)
                {
                    startingIndex += scanRecord[startingIndex] + 1;
                }
                if ((startingIndex + 1) < scanRecord.Length && scanRecord[startingIndex + 1] == 0xff)
                {
                    int length = scanRecord[startingIndex] - 1;
                    if (length > 0)
                    {
                        startingIndex += 2;
                        _advertisementData = new byte[length];
                        Array.Copy(scanRecord, startingIndex, _advertisementData, 0, length);
                    }
                }
            }
        }

        public override Address ID
        {
            get
            {
                try
                {
                    return new Address(_nativeDevice.Address);
                }
                catch
                {
                    return new Address(string.Empty);
                }
            }
        }

        public override string Name
        {
            get
            {
                try
                {
                    return _nativeDevice.Name;
                }
                catch
                {
                    return string.Empty;
                }
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