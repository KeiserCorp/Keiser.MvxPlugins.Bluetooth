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
#if DEBUG
            Trace.Info("OnScan Result: " + result.Device.Address);
#endif
            CallbackQueuer.Push(new Device(result.Device, result.Rssi, result.ScanRecord.GetBytes()));
        }
    }
}