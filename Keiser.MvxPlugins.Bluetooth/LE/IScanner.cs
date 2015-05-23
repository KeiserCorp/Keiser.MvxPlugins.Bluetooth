namespace Keiser.MvxPlugins.Bluetooth.LE
{
    public interface IScanner
    {
        bool LESupported { get; }
        bool IsScanning { get; }
        void StartScan(IScanCallback scanCallback);
        void StopScan();
    }
}
