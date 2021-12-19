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
            "Chromecast")
            => this.MakeProfileAction("text;Enter url of media to cast:");

        private ChromeCastPlugin ChromeCastPlugin => this.Plugin as ChromeCastPlugin;
        private IChromeCastWrapper ChromeCastWrapper => this.ChromeCastPlugin.ChromeCastApi;

        #region PluginDynamicCommand overrides
        //protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
        //    (this.ChromeCastWrapper.PlayBackState != PlayBackState.Playing ? "Play" : "Pause") +
        //    "\n" +
        //    base.GetCommandDisplayName(actionParameter, imageSize);

        protected override PluginProfileActionData GetProfileActionData() => base.GetProfileActionData();

        protected override void RunCommand(String actionParameter)
        {
            if (this.ChromeCastWrapper == null)
            {
                return;
            }

            try
            {
                if (this.ChromeCastWrapper.PlayBackState != PlayBackState.Playing)
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
