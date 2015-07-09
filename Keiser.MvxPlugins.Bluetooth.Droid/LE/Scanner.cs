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

        private object _adapterLocker = new object();
        private bool _adapterChanging = false;
        public bool AdapterChanging
        {
            get { lock (_adapterLocker) return _adapterChanging; }
            protected set { lock (_adapterLocker) _adapterChanging = value; }
        }

        //private object _adapterRunTimeLocker = new object();
        //private DateTime _adapterRunTime;
        //public DateTime AdapterRunTime
        //{
        //    get { lock (_adapterRunTimeLocker) return _adapterRunTime; }
        //    protected set { lock (_adapterRunTimeLocker) _adapterRunTime = value; }
        //}

        protected IScanCallback ScanCallback;
        protected CallbackQueuer CallbackQueuer;
        protected Bluetooth.Timer ScanPeriodTimer;
        //protected const int /*ScanPeriod = 7000,*/ ScanCycleLength = 45000;

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
            while (AdapterChanging) { await Task.Delay(100); }
            AdapterChanging = true;
            Wifi.Disable();
            await Adapter.ClearCache();
            await Adapter.Enable();
            CallbackQueuer = new CallbackQueuer(ScanCallback, EmptyQueueEvent);
            CallbackQueuer.Start();
            StartAdapterScan();
            //AdapterRunTime = DateTime.Now;
            //ScanTimerStart();
            AdapterChanging = false;
        }

        protected async void EndScan()
        {
            while (AdapterChanging) { await Task.Delay(100); }
            AdapterChanging = true;
            //ScanTimerStop();
            StopAdapterScan();
            CallbackQueuer.Stop();
            await Adapter.Disable();
            Wifi.Enable();
            AdapterChanging = false;
        }

        //protected void ScanTimerStart(int period = ScanPeriod)
        //{
        //    ScanTimerStop();
        //        ScanPeriodTimer = new Bluetooth.Timer(_ => ScanTimerCallback(), null, period, period);
        //}

        //protected void ScanTimerStop()
        //{
        //    if (ScanPeriodTimer != null)
        //        ScanPeriodTimer.Cancel();
        //    ScanPeriodTimer = null;
        //}

        //protected void ScanTimerCallback()
        //{
        //    if (AdapterRunTime.AddMilliseconds(ScanCycleLength) <= DateTime.Now)
        //        ScanTimerStop();
        //    CycleAdapterScan(false);
        //}

        protected void StartAdapterScan()
        {
            Adapter.BluetoothAdapter.StartLeScan(this);
        }

        protected void StopAdapterScan()
        {
            Adapter.BluetoothAdapter.StopLeScan(this);
        }

        private object _emptyQueueTimeLocker = new object();
        private DateTime _emptyQueueTime;
        public DateTime EmptyQueueTime
        {
            get { lock (_emptyQueueTimeLocker) return _emptyQueueTime; }
            protected set { lock (_emptyQueueTimeLocker) _emptyQueueTime = value; }
        }

        protected void EmptyQueueEvent(object sender, EventArgs e)
        {
#if DEBUG
            Trace.Info("LE Scanner: Empty Queue Event");
#endif
            //if (AdapterRunTime.AddMilliseconds(ScanCycleLength) <= DateTime.Now)
            //{
            bool hardReset = (EmptyQueueTime != null && EmptyQueueTime >= DateTime.Now);
            CycleAdapterScan(hardReset);
            EmptyQueueTime = DateTime.Now.AddMilliseconds(CallbackQueuer.MaxEmptyQueueThreshold + 1000);
            //}
        }

        protected async void CycleAdapterScan(bool hardCycle = false)
        {
#if DEBUG
            Trace.Info("LE Scanner: Cycling Adapter [Hard: " + hardCycle.ToString() + "]");
#endif
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