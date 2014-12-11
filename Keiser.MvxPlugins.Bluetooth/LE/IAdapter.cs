namespace Keiser.MvxPlugins.Bluetooth.LE
{
    public interface IAdapter
    {
        bool LESupported { get; }
        bool IsScanning { get; }
        void StartScan(IScanCallback scanCallback);
        void StopScan();
    }
}
