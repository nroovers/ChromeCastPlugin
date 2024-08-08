namespace Loupedeck.ChromeCastPlugin.Adjustments
{
    using System;

    using Loupedeck.ChromeCastPlugin.ChromeCastWrapper;

    internal class VolumeAdjustment : PluginDynamicAdjustment
    {
        public VolumeAdjustment()
            : base(
                "Chromecast Volume",
                "Adjust chromecast volume level",
                String.Empty,
                true)
        {
        }

        private ChromeCastPlugin ChromeCastPlugin => this.Plugin as ChromeCastPlugin;

        private IChromeCastWrapper ChromeCastWrapper => this.ChromeCastPlugin.ChromeCastApi;

        protected override Boolean OnLoad()
        {
            if (this.ChromeCastWrapper != null)
            {
                this.ChromeCastWrapper.ChromeCastConnected += this.ChromeCastWrapper_ChromeCastConnected;
                this.ChromeCastWrapper.StatusChanged += this.ChromeCastWrapper_StatusChanged;
            }

            return base.OnLoad();
        }

        protected override Boolean OnUnload()
        {
            if (this.ChromeCastWrapper != null)
            {
                this.ChromeCastWrapper.ChromeCastConnected -= this.ChromeCastWrapper_ChromeCastConnected;
                this.ChromeCastWrapper.StatusChanged -= this.ChromeCastWrapper_StatusChanged;
            }

            return base.OnUnload();
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 ticks)
        {
            if (this.ChromeCastWrapper == null)
            {
                return;
            }

            try
            {
                this.ChromeCastWrapper.Volume += ticks;
                this.ActionImageChanged(actionParameter);
            }
            catch (Exception e)
            {
                this.ChromeCastPlugin.HandleError("Adjusting volume failed", e);
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            if (this.ChromeCastWrapper == null)
            {
                return;
            }

            try
            {
                this.ChromeCastWrapper.IsMuted = !this.ChromeCastWrapper.IsMuted;
                this.ActionImageChanged(actionParameter);
            }
            catch (Exception e)
            {
                this.ChromeCastPlugin.HandleError("Muting volume failed", e);
            }
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
            imageSize == PluginImageSize.Width60 ? "CC Vol." : "Chromecast Volume";

        protected override String GetAdjustmentValue(String actionParameter)
        {
            if (this.ChromeCastWrapper == null ||
                !this.ChromeCastWrapper.IsConnected)
            {
                return "?";
            }

            if (this.ChromeCastWrapper.IsMuted)
            {
                return "x";
            }

            return this.ChromeCastWrapper.Volume.ToString();
        }

        private void ChromeCastWrapper_ChromeCastConnected(Object sender, ChromeCastWrapper.ChromeCastConnectedEventArgs e) => this.ActionImageChanged();

        private void ChromeCastWrapper_StatusChanged(Object sender, ChromeCastWrapper.ChromeCastStatusUpdatedEventArgs e) => this.ActionImageChanged();

    }
}
