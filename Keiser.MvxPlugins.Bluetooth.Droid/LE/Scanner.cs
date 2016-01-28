namespace Keiser.MvxPlugins.Bluetooth.Droid.LE
{
    using Keiser.MvxPlugins.Bluetooth;
    using Keiser.MvxPlugins.Bluetooth.LE;

    public class Scanner : Java.Lang.Object, IScanner
    {
        public bool LESupported { get { return Adapter.LESupported; } }

        private object _isScanningLocker = new object();
        private bool _isScanning = false;
        public bool IsScanning
        {
            get { lock (_isScanningLocker) return _isScanning; }
            protected set { lock (_isScanningLocker) _isScanning = value; }
        }

        protected Adapter Adapter;
        protected CallbackQueuer CallbackQueuer;

        public Scanner()
        {
            Adapter = new Adapter();
            CallbackQueuer = new CallbackQueuer();
        }

        public void StartScan(IScanCallback scanCallback)
        {
            if (!Adapter.LESupported)
                return;
#if DEBUG
            Trace.Info("LE Scanner: Starting");
#endif
            IsScanning = true;
            CallbackQueuer.Start(scanCallback);
            Adapter.StartLEScan(CallbackQueuer);
        }

        public void StopScan()
        {
            if (!Adapter.LESupported)
                return;
#if DEBUG
            Trace.Info("LE Scanner: Stopping");
#endif
            CallbackQueuer.Stop();
            Adapter.StopLEScan();
            IsScanning = false;
        }
    }
}