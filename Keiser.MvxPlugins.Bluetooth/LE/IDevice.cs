namespace Keiser.MvxPlugins.Bluetooth.LE
{
    public interface IDevice
    {
        Address ID { get; }
        string Name { get; }
        int Rssi { get; }
        byte[] AdvertisingData { get; }
    }
}
