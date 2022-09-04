namespace Loupedeck.ChromeCastPlugin.Commands
{
    using System;

    using Loupedeck.ChromeCastPlugin.ChromeCastWrapper;

    internal class TogglePlayCommand : PluginDynamicCommand
    {
        // Examples:
        // http://icecast.vrtcdn.be/stubru-high.mp3 
        // http://stream.radiohelsinki.fi:8002/;?type=http&nocache=13776

        public TogglePlayCommand() : base(
            "Play",
            "Select, play and pause online media",
            "")
            => this.MakeProfileAction("text;URL of media to cast");

        private ChromeCastPlugin ChromeCastPlugin => this.Plugin as ChromeCastPlugin;
        private IChromeCastWrapper ChromeCastWrapper => this.ChromeCastPlugin.ChromeCastApi;

        #region PluginDynamicCommand overrides
        protected override void RunCommand(String actionParameter)
        {
            if (this.ChromeCastWrapper == null)
            {
                return;
            }

            try
            {
                if (this.ChromeCastWrapper.PlayBackUrl != actionParameter ||
                    this.ChromeCastWrapper.PlayBackState != PlayBackState.Playing)
                {
                    this.ChromeCastWrapper.PlayCast(actionParameter);
                }
                else
                {
                    this.ChromeCastWrapper.PauseCast();
                }
            }
            catch (Exception e)
            {
                this.ChromeCastPlugin.HandleError("Toggle play failed", e);
            }
        }
        #endregion
    }
}
