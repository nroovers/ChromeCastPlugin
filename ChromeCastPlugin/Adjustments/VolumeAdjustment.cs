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

        private ChromeCastPlugin ChromeCastPlugin => this.Plugin as ChromeCastPlugin;
        private IChromeCastWrapper ChromeCastWrapper => this.ChromeCastPlugin.ChromeCastApi;

        #region PluginDynamicAdjustment overrides

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

            try
            {
                this.ChromeCastWrapper.Volume += ticks; // increase or decrease counter on the number of ticks
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
        #endregion

        #region Private functions

        private void ChromeCastApi_onChromeCastConnected(Object sender, ChromeCastWrapper.ChromeCastConnectedEventArgs e) => this.ActionImageChanged();

        private void ChromeCastApi_onStatusChanged(Object sender, ChromeCastWrapper.ChromeCastStatusEventArgs e) => this.ActionImageChanged();

        #endregion
    }
}
