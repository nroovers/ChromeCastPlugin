namespace Loupedeck.ChromeCastPlugin
{
    using System;

    using Loupedeck.ChromeCastPlugin.ChromeCastWrapper;

    public class ChromeCastPlugin : Plugin
    {
        public override Boolean UsesApplicationApiOnly => true;

        public override Boolean HasNoApplication => true;

        internal readonly IChromeCastWrapper ChromeCastApi = new GoogleCastWrapper();

        public ChromeCastPlugin()
        {
            this.ChromeCastApi.ActivateContinuousSearch();
        }

        public override void Load()
        {
            this.Info.Icon16x16 = EmbeddedResources.ReadImage("Loupedeck.ChromeCastPlugin.Resources.Icons.PluginIcon16x16.png");
            this.Info.Icon32x32 = EmbeddedResources.ReadImage("Loupedeck.ChromeCastPlugin.Resources.Icons.PluginIcon32x32.png");
            this.Info.Icon48x48 = EmbeddedResources.ReadImage("Loupedeck.ChromeCastPlugin.Resources.Icons.PluginIcon48x48.png");
            this.Info.Icon256x256 = EmbeddedResources.ReadImage("Loupedeck.ChromeCastPlugin.Resources.Icons.PluginIcon256x256.png");

            if (!this.ChromeCastApi.IsContinuousSearchActive)
            {
                this.ChromeCastApi.ActivateContinuousSearch();
            }
            if (this.ChromeCastApi.ConnectedChromeCast != null)
            {
                this.ChromeCastApi.ReConnect();
            }
        }

        public override void Unload()
        {
            this.ChromeCastApi.DeactivateContinuousSearch();
            this.ChromeCastApi.Disconnect();
        }

        public override void RunCommand(String commandName, String parameter)
        {
        }

        public override void ApplyAdjustment(String adjustmentName, String parameter, Int32 diff)
        {
        }

        internal void HandleError(String msg, Exception e)
        {
            Tracer.Error(msg, e);
        }

        private void OnApplicationStarted(Object sender, EventArgs e)
        {
        }

        private void OnApplicationStopped(Object sender, EventArgs e)
        {
        }
    }
}
