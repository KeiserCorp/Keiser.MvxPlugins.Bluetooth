namespace Keiser.MvxPlugins.Bluetooth.Droid.LE
{
    using Android.Bluetooth;
    using Android.Bluetooth.LE;
    using Keiser.MvxPlugins.Bluetooth.LE;
    using System.Collections.Generic;

    public class LEScanCallback : Android.Bluetooth.LE.ScanCallback, IScanCallback
    {
        protected CallbackQueuer CallbackQueuer;

        public LEScanCallback(CallbackQueuer callbackQueuer)
            : base()
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
            CallbackQueuer.Push(new Device(result.Device, result.Rssi, result.ScanRecord.GetBytes()));
        }

        public void ScanCallback(IDevice device)
        {
            CallbackQueuer.Push(device);
        }

    }
}