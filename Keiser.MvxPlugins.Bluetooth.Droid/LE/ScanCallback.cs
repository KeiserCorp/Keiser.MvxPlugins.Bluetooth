namespace Keiser.MvxPlugins.Bluetooth.Droid.LE
{
    using Android.Bluetooth.LE;
    using System.Collections.Generic;

    public class ScanCallback : Android.Bluetooth.LE.ScanCallback
    {
        protected CallbackQueuer CallbackQueuer;

        public ScanCallback(CallbackQueuer callbackQueuer)
        {
            CallbackQueuer = callbackQueuer;
        }

        public override void OnBatchScanResults(IList<ScanResult> results)
        {
            Trace.Info("OnBatch Scan Results: " + results.Count);
        }

        public override void OnScanFailed(ScanFailure errorCode)
        {
            Trace.Info("OnScan Failed: " + errorCode.ToString());
        }

        public override void OnScanResult(ScanCallbackType callbackType, ScanResult result)
        {
            Trace.Info("OnScan Result: " + result.Device.Address);
            CallbackQueuer.Push(new Device(result.Device, result.Rssi, result.ScanRecord.GetBytes()));
        }
    }
}