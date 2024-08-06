namespace Loupedeck.ChromeCastPlugin
{
    using System;

    using Loupedeck.ChromeCastPlugin.ChromeCastWrapper;

    // This class contains the plugin-level logic of the Loupedeck plugin.

    public class ChromeCastPlugin : Plugin
    {
        // Gets a value indicating whether this is an API-only plugin.
        public override Boolean UsesApplicationApiOnly => true;

        // Gets a value indicating whether this is a Universal plugin or an Application plugin.
        public override Boolean HasNoApplication => true;

        internal IChromeCastWrapper ChromeCastApi { get; } = new GoogleCastWrapper();


        // Initializes a new instance of the plugin class.
        public ChromeCastPlugin()
        {
            // Initialize the plugin log.
            PluginLog.Init(this.Log);

            // Initialize the plugin resources.
            PluginResources.Init(this.Assembly);
        }

        // This method is called when the plugin is loaded.
        public override void Load()
        {
            this.Info.Icon16x16 = EmbeddedResources.ReadImage("Loupedeck.ChromeCastPlugin.Resources.PackageMetadata.Icon16x16.png");
            this.Info.Icon32x32 = EmbeddedResources.ReadImage("Loupedeck.ChromeCastPlugin.Resources.PackageMetadata.Icon32x32.png");
            this.Info.Icon48x48 = EmbeddedResources.ReadImage("Loupedeck.ChromeCastPlugin.Resources.PackageMetadata.Icon48x48.png");
            this.Info.Icon256x256 = EmbeddedResources.ReadImage("Loupedeck.ChromeCastPlugin.Resources.PackageMetadata.Icon256x256.png");

            if (!this.ChromeCastApi.IsContinuousSearchActive)
            {
                this.ChromeCastApi.ActivateContinuousSearch();
            }
        }

        // This method is called when the plugin is unloaded.
        public override void Unload()
        {
            this.ChromeCastApi.DeactivateContinuousSearch();

            if (this.ChromeCastApi.IsConnected)
            {
                this.ChromeCastApi.Disconnect();
            }
        }

        internal void HandleError(String msg, Exception e) => Tracer.Error(msg, e);

    }
}
