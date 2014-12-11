namespace Keiser.MvxPlugins.Bluetooth
{
    using Cirrious.CrossCore;
    using Cirrious.CrossCore.Plugins;

    public class PluginLoader
        : IMvxPluginLoader
    {
        public static readonly PluginLoader Instance = new PluginLoader();

        public void EnsureLoaded()
        {
            var manager = Mvx.Resolve<IMvxPluginManager>();
            manager.EnsurePlatformAdaptionLoaded<PluginLoader>();
        }
    }
}
