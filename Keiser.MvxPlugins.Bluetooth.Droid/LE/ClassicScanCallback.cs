namespace Keiser.MvxPlugins.Bluetooth.Droid.LE
{
    using Android.Bluetooth;

    public class ClassicScanCallback : Java.Lang.Object, BluetoothAdapter.ILeScanCallback
    {
        protected CallbackQueuer CallbackQueuer;

        public ClassicScanCallback(CallbackQueuer callbackQueuer)
        {
            CallbackQueuer = callbackQueuer;
        }

        public void OnLeScan(BluetoothDevice device, int rssi, byte[] scanRecord)
        {
            CallbackQueuer.Push(new Device(device, rssi, scanRecord));
        }
    }
}