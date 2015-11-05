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
    using Android.Bluetooth.LE;
    using System.Collections.Generic;

    public class Scanner : Java.Lang.Object, IScanner, BluetoothAdapter.ILeScanCallback
    {
        public bool LESupported { get { return Adapter.LESupported; } }

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

        protected bool isLollipop = false;

        protected IScanCallback ExternalScanCallback;
        protected CallbackQueuer CallbackQueuer;

        public Scanner()
        {
            isLollipop = ((int)Android.OS.Build.VERSION.SdkInt) >= 21;
        }

        public void StartScan(IScanCallback scanCallback)
        {
            if (!Adapter.LESupported)
                return;
#if DEBUG
            Trace.Info("LE Scanner: Starting");
#endif
            IsScanning = true;
            ExternalScanCallback = scanCallback;
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
            CallbackQueuer = new CallbackQueuer(ExternalScanCallback, EmptyQueueEvent);
            CallbackQueuer.Start();
            StartAdapterScan();
            AdapterChanging = false;
        }

        protected async void EndScan()
        {
            while (AdapterChanging) { await Task.Delay(100); }
            AdapterChanging = true;
            StopAdapterScan();
            CallbackQueuer.Stop();
            await Adapter.Disable();
            Wifi.Enable();
            AdapterChanging = false;
        }

        protected BluetoothLeScanner LEScanner;
        protected ScanCallback LEScanCallback;

        protected void StartAdapterScan()
        {
            if (isLollipop)
            {
                LEScanCallback = new ScanCallback(CallbackQueuer);
                LEScanner = Adapter.BluetoothAdapter.BluetoothLeScanner;
                ScanSettings settings = new ScanSettings.Builder().SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency).Build();
                List<ScanFilter> filters = new List<ScanFilter>() { };
                LEScanner.StartScan(filters, settings, LEScanCallback);
            }
            else
            {
                Adapter.BluetoothAdapter.StartLeScan(this);
            }
        }

        protected void StopAdapterScan()
        {
            if (isLollipop)
            {
                LEScanner.StopScan(LEScanCallback);
            }
            else
            {
                Adapter.BluetoothAdapter.StopLeScan(this);
            }
        }

        protected void EmptyQueueEvent(object sender, EventArgs e)
        {
#if DEBUG
            Trace.Info("LE Scanner: Empty Queue Event");
#endif
            CallbackQueuer.Pause();
            CycleAdapterScan(true);
            CallbackQueuer.Resume();
        }

        protected async void CycleAdapterScan(bool hardCycle = false, int scanPause = 0)
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
            else if (scanPause > 0)
            {
                await Task.Delay(scanPause);
            }
            StartAdapterScan();
        }
    }
}