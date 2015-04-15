namespace Keiser.MvxPlugins.Bluetooth.Touch.LE
{
    using Keiser.MvxPlugins.Bluetooth.LE;
    using MonoTouch.CoreBluetooth;
    using MonoTouch.Foundation;
    using System.Collections.Generic;

    public class Device : DeviceBase
    {
        protected CBPeripheral _nativeDevice;
        protected int _rssi;
        protected byte[] _advertisementData;

        public Device(CBPeripheral nativeDevice, NSNumber rssi, NSDictionary advertisementData)
        {
            _nativeDevice = nativeDevice;
            _rssi = (int)rssi;
            SetAdvertisingData(advertisementData);
        }

        protected void SetAdvertisingData(NSDictionary advertisementData)
        {
            List<byte> tmpList = new List<byte>();
            string print = "AdvData: ";
            foreach(var pair in advertisementData)
            {
                //((NSNumber)pair.Value).ByteValue;
                print += "[" + pair.Key.ToString() + " - " + pair.Value.ToString() + "] ";
            }
            Keiser.MvxPlugins.Bluetooth.Trace.Info(print);
        }

        public override Address ID
        {
            get
            {
                return new Address(_nativeDevice.Identifier.AsString());
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

        private object NativeDevice
        {
            get
            {
                return _nativeDevice;
            }
        }
    }
}
