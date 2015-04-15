namespace Keiser.MvxPlugins.Bluetooth.LE
{
    using System;

    public class DeviceBase : IDevice
    {
        public virtual Address ID
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual int Rssi
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual byte[] AdvertisingData
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
