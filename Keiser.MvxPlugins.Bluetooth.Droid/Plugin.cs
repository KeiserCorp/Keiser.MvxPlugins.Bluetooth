namespace Keiser.MvxPlugins.Bluetooth.Droid
{
    using Cirrious.CrossCore;
    using Cirrious.CrossCore.Plugins;
    using Keiser.MvxPlugins.Bluetooth.Droid.LE;
    using Keiser.MvxPlugins.Bluetooth.LE;

    public class Plugin
        : IMvxPlugin
    {
        public void Load()
        {
            Mvx.RegisterType<IDevice, Device>();
            Mvx.RegisterType<IAdapter, Adapter>();
        }
    }
}
