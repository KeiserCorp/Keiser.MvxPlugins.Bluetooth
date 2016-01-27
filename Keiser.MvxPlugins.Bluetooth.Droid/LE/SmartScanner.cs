namespace Keiser.MvxPlugins.Bluetooth.Droid.LE
{
    using Keiser.MvxPlugins.Bluetooth;
    using Keiser.MvxPlugins.Bluetooth.LE;

    public class SmartScanner : Java.Lang.Object, IScanner
    {
        public bool LESupported { get { return SmartAdapter.LESupported; } }

        private object _isScanningLocker = new object();
        private bool _isScanning = false;
        public bool IsScanning
        {
            get { lock (_isScanningLocker) return _isScanning; }
            protected set { lock (_isScanningLocker) _isScanning = value; }
        }

        protected SmartAdapter SmartAdapter;
        protected CallbackQueuer CallbackQueuer;
        //protected SmartScanCallback SmartScanCallback;

        public SmartScanner()
        {
            SmartAdapter = new SmartAdapter();
            CallbackQueuer = new CallbackQueuer();
            //SmartScanCallback = new SmartScanCallback(CallbackQueuer);
#if DEBUG
            Trace.Info("LE Scanner: Constructed");
#endif
        }

        public void StartScan(IScanCallback scanCallback)
        {
            if (!SmartAdapter.LESupported)
                return;
#if DEBUG
            Trace.Info("LE Scanner: Starting");
#endif
            IsScanning = true;
            CallbackQueuer.Start(scanCallback);
            SmartAdapter.StartLEScan(new SmartScanCallback(CallbackQueuer));
        }

        public void StopScan()
        {
            if (!SmartAdapter.LESupported)
                return;
#if DEBUG
            Trace.Info("LE Scanner: Stopping");
#endif
            CallbackQueuer.Stop();
            SmartAdapter.StopLEScan();
            IsScanning = false;
        }
    }
}