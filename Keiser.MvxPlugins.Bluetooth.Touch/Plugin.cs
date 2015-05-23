namespace Keiser.MvxPlugins.Bluetooth.Touch
{
    using Cirrious.CrossCore;
    using Cirrious.CrossCore.Plugins;
    using Keiser.MvxPlugins.Bluetooth.Touch.LE;
    using Keiser.MvxPlugins.Bluetooth.LE;

    public class Plugin
        : IMvxPlugin
    {
        public void Load()
        {
            Mvx.RegisterType<IDevice, Device>();
            Mvx.RegisterType<IScanner, Adapter>();
        }
    }
}
