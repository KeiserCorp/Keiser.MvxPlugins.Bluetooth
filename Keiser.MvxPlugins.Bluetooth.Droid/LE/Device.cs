namespace Keiser.MvxPlugins.Bluetooth.Droid.LE
{
    using Android.Bluetooth;
    using Keiser.MvxPlugins.Bluetooth.LE;
    using System;

    public class Device : DeviceBase
    {
        public Device(BluetoothDevice nativeDevice, int rssi, byte[] scanRecord)
        {
            try
            {
                _id = nativeDevice.Address;
            }
            catch
            {
                _id = string.Empty;
            }
            try
            {
                _name = nativeDevice.Name;
            }
            catch
            {
                _name = string.Empty;
            }
            _rssi = rssi;
            SetAdvertisingData(scanRecord);
        }

        protected byte[] _advertisementData = new byte[0];
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

        private string _id;
        public override Address ID
        {
            get
            {
                try
                {
                    return new Address(_id);
                }
                catch
                {
                    return new Address(string.Empty);
                }
            }
        }

        private string _name;
        public override string Name
        {
            get
            {
                return _name;
            }
        }

        protected int _rssi;
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
                return null;
            }
        }
    }
}