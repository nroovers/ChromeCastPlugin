namespace Loupedeck.ChromeCastPlugin.Commands
{
    using System;

    using Loupedeck.ChromeCastPlugin.ChromeCastWrapper;

    internal class StopCastCommand : PluginDynamicCommand
    {
        private IChromeCastWrapper ChromeCastWrapper => (this.Plugin as ChromeCastPlugin).ChromeCastApi;

        public StopCastCommand() : base(
            "Stop Cast",
            "Stops the current ongoing cast",
            "Chromecast")
        { 
        }

        protected override void RunCommand(String actionParameter)
        {
            if (this.ChromeCastWrapper == null)
            {
                return;
            }

            this.ChromeCastWrapper.StopCast();
        }
    }
}
