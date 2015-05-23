namespace Keiser.MvxPlugins.Bluetooth.Droid.LE
{
    using Android.Bluetooth;
    using Android.Content;
    using Android.Net.Wifi;
    using Keiser.MvxPlugins.Bluetooth.LE;
    using System;
    using System.Threading.Tasks;
    using Keiser.MvxPlugins.Bluetooth;
    using System.Collections.Concurrent;
    using System.Threading;

    public class Scanner : Java.Lang.Object, IScanner, BluetoothAdapter.ILeScanCallback
    {
        public bool LESupported { get { return Adapter.LESupported; } }
        //public void ClearCache() { Adapter.ClearCache(); }

        private object _isScanningLocker = new object();
        private bool _isScanning = false;
        public bool IsScanning
        {
            get { lock (_isScanningLocker) return _isScanning; }
            protected set { lock (_isScanningLocker) _isScanning = value; }
        }

        protected IScanCallback ScanCallback;
        protected CallbackQueuer CallbackQueuer;
        protected Bluetooth.Timer ScanPeriodTimer;
        protected const int ScanPeriod = 1700;

        public void StartScan(IScanCallback scanCallback)
        {
            if (!Adapter.LESupported)
                return;
#if DEBUG
            Trace.Info("LE Scanner: Starting");
#endif
            IsScanning = true;
            ScanCallback = scanCallback;
            InitializeScan();
        }

        public void StopScan()
        {
            if (!Adapter.LESupported)
                return;
#if DEBUG
            Trace.Info("LE Scanner: Stopping");
#endif
            EndScan();
            IsScanning = false;
        }

        public void OnLeScan(BluetoothDevice device, int rssi, byte[] scanRecord)
        {
            CallbackQueuer.Push(new Device(device, rssi, scanRecord));
        }

        protected async void InitializeScan()
        {
            Wifi.Disable();
            await Adapter.ClearCache();
            await Adapter.Enable();
            CallbackQueuer = new CallbackQueuer(ScanCallback, EmptyQueueEvent);
            CallbackQueuer.Start();
            StartAdapterScan();
            ScanTimerStart();
        }

        protected async void EndScan()
        {
            ScanTimerStop();
            StopAdapterScan();
            CallbackQueuer.Stop();
            await Adapter.Disable();
            Wifi.Enable();
        }

        protected void ScanTimerStart(int period = ScanPeriod)
        {
            ScanTimerStop();
            ScanPeriodTimer = new Bluetooth.Timer(_ => CycleAdapterScan(false), null, period, period);
        }

        protected void ScanTimerStop()
        {
            if (ScanPeriodTimer != null)
                ScanPeriodTimer.Cancel();
            ScanPeriodTimer = null;
        }

        protected void StartAdapterScan()
        {
            Adapter.BluetoothAdapter.StartLeScan(this);
        }

        protected void StopAdapterScan()
        {
            Adapter.BluetoothAdapter.StopLeScan(this);
        }

        protected void EmptyQueueEvent(object sender, EventArgs e)
        {
#if DEBUG
            Trace.Info("LE Scanner: Empty Queue Event");
#endif
            CycleAdapterScan(true);
        }

        protected async void CycleAdapterScan(bool hardCycle = false)
        {
            StopAdapterScan();
            if (hardCycle)
            {
                await Adapter.Disable();
                await Adapter.Enable();
            }
            StartAdapterScan();
        }
    }
}