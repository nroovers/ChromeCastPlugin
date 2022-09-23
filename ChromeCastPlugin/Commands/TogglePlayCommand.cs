namespace Loupedeck.ChromeCastPlugin.Commands
{
    using System;
    using Loupedeck.ChromeCastPlugin.ChromeCastWrapper;

    // Examples:
    // http://icecast.vrtcdn.be/stubru-high.mp3 
    // http://stream.radiohelsinki.fi:8002/;?type=http&nocache=13776

    internal class TogglePlayCommand : PluginMultistateDynamicCommand
    {
        public TogglePlayCommand() : base(
            "Play",
            "Select, play and pause online media",
            "")
        {
            this.AddState("Pause", "Pause");
            this.AddState("Play", "Play");
            this.MakeProfileAction("text;URL of media to cast");
        }

        private ChromeCastPlugin ChromeCastPlugin => this.Plugin as ChromeCastPlugin;

        #region PluginDynamicCommand overrides

        protected override void RunCommand(String actionParameter)
        {
            if (this.ChromeCastPlugin.ChromeCastApi == null)
            {
                return;
            }

            try
            {
                if (this.ChromeCastPlugin.ChromeCastApi.PlayBackUrl != actionParameter ||
                    this.ChromeCastPlugin.ChromeCastApi.PlayBackState != PlayBackState.Playing)
                {
                    this.ChromeCastPlugin.ChromeCastApi.PlayCast(actionParameter);
                    this.SetCurrentState(actionParameter, 1);
                }
                else
                {
                    this.ChromeCastPlugin.ChromeCastApi.PauseCast();
                    this.SetCurrentState(actionParameter, 0);
                }
                this.ActionImageChanged(actionParameter);
            }
            catch (Exception e)
            {
                this.ChromeCastPlugin.HandleError("Toggle play failed", e);
            }
        }
        #endregion
    }
}
