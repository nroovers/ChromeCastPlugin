namespace Loupedeck.ChromeCastPlugin.Commands
{
    using System;

    using Loupedeck.ChromeCastPlugin.ChromeCastWrapper;

    internal class StopCastCommand : PluginDynamicCommand
    {
        public StopCastCommand() : base(
            "Stop Cast",
            "Stop the current ongoing cast",
            "Chromecast")
        {
        }

        private ChromeCastPlugin ChromeCastPlugin => this.Plugin as ChromeCastPlugin;
        private IChromeCastWrapper ChromeCastWrapper => this.ChromeCastPlugin.ChromeCastApi;

        protected override void RunCommand(String actionParameter)
        {
            if (this.ChromeCastWrapper == null)
            {
                return;
            }

            try
            {
                this.ChromeCastWrapper.StopCast();
            }
            catch (Exception e)
            {
                this.ChromeCastPlugin.HandleError("Stop cast failed", e);
            }
        }
    }
}
