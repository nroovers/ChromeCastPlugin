namespace Loupedeck.ChromeCastPlugin.Adjustments
{
    using System;

    using Loupedeck.ChromeCastPlugin.ChromeCastWrapper;

    internal class VolumeAdjustment : PluginDynamicAdjustment
    {

        public VolumeAdjustment() : base(
            "CCast volume",
            "Adjust chromecast volume level",
            "Chromecast",
            true)
        {
        }

        private IChromeCastWrapper ChromeCastWrapper => (this.Plugin as ChromeCastPlugin).ChromeCastApi;

        protected override Boolean OnLoad()
        {
            if (this.ChromeCastWrapper != null)
            {
                this.ChromeCastWrapper.ChromeCastConnected += this.ChromeCastApi_onChromeCastConnected;
                this.ChromeCastWrapper.StatusChanged += this.ChromeCastApi_onStatusChanged;
            }

            return base.OnLoad();
        }

        protected override Boolean OnUnload()
        {
            if (this.ChromeCastWrapper != null)
            {
                this.ChromeCastWrapper.ChromeCastConnected -= this.ChromeCastApi_onChromeCastConnected;
                this.ChromeCastWrapper.StatusChanged -= this.ChromeCastApi_onStatusChanged;
            }

            return base.OnUnload();
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 ticks)
        {
            if (this.ChromeCastWrapper == null)
            {
                return;
            }

            this.ChromeCastWrapper.Volume += ticks; // increase or decrease counter on the number of ticks

            this.ActionImageChanged(actionParameter);
        }

        protected override void RunCommand(String actionParameter)
        {
            if (this.ChromeCastWrapper == null)
            {
                return;
            }

            this.ChromeCastWrapper.IsMuted = !this.ChromeCastWrapper.IsMuted;
            this.ActionImageChanged(actionParameter);
        }

        protected override String GetAdjustmentValue(String actionParameter)
        {
            if (this.ChromeCastWrapper == null ||
                this.ChromeCastWrapper.ConnectedChromeCast == null)
            {
                return "?";
            }

            if (this.ChromeCastWrapper.IsMuted)
            {
                return "x";
            }

            return this.ChromeCastWrapper.Volume.ToString();
        }

        private void ChromeCastApi_onChromeCastConnected(Object sender, ChromeCastWrapper.ChromeCastEventArgs e) => this.ActionImageChanged();

        private void ChromeCastApi_onStatusChanged(Object sender, ChromeCastWrapper.ChromeCastStatusEventArgs e) => this.ActionImageChanged();
    }
}
