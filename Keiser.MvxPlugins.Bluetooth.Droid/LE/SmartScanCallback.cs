namespace Keiser.MvxPlugins.Bluetooth.Droid.LE
{
    using Android.Bluetooth;
    using Android.Bluetooth.LE;
    using Keiser.MvxPlugins.Bluetooth.LE;
    using System.Collections.Generic;

    public class SmartScanCallback : Android.Bluetooth.LE.ScanCallback, BluetoothAdapter.ILeScanCallback, IScanCallback
    {
        protected CallbackQueuer CallbackQueuer;

        public SmartScanCallback(CallbackQueuer callbackQueuer) : base()
        {
#if DEBUG
            Trace.Info("SmartScanCallback: Begin");
#endif
            CallbackQueuer = callbackQueuer;
#if DEBUG
            Trace.Info("SmartScanCallback: Constructed");
#endif
        }

        public override void OnBatchScanResults(IList<ScanResult> results)
        {
#if DEBUG
            Trace.Info("OnBatch Scan Results: " + results.Count);
#endif
        }

        public override void OnScanFailed(ScanFailure errorCode)
        {
#if DEBUG
            Trace.Info("OnScan Failed: " + errorCode.ToString());
#endif
        }

        public override void OnScanResult(ScanCallbackType callbackType, ScanResult result)
        {
            ScanCallback(new Device(result.Device, result.Rssi, result.ScanRecord.GetBytes()));
        }

        public void OnLeScan(BluetoothDevice device, int rssi, byte[] scanRecord)
        {
            ScanCallback(new Device(device, rssi, scanRecord));
        }

        public void ScanCallback(IDevice device)
        {
            CallbackQueuer.Push(device);
        }

    }
}