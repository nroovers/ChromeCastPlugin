namespace Loupedeck.ChromeCastPlugin
{
    using System;

    using Loupedeck.ChromeCastPlugin.ChromeCastWrapper;

    public class Chromecast : Plugin
    {
        public Chromecast()
        {
            this.ChromeCastApi.ActivateContinuousSearch();
        }

        public override Boolean UsesApplicationApiOnly => true;

        public override Boolean HasNoApplication => true;

        internal IChromeCastWrapper ChromeCastApi { get; } = new GoogleCastWrapper();

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

        public override void Unload()
        {
            this.ChromeCastApi.DeactivateContinuousSearch();

            if (this.ChromeCastApi.IsConnected)
            {
                this.ChromeCastApi.Disconnect();
            }
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
