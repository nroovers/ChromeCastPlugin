namespace Loupedeck.ChromeCastPlugin.Commands
{
    using System;

    using Loupedeck.ChromeCastPlugin.ChromeCastWrapper;

    internal class TogglePlayCommand : PluginMultistateDynamicCommand
    {
        public TogglePlayCommand()
            : base(
                "Play",
                "Select, play and pause online media",
                String.Empty)
        {
            this.AddState("Pause", "Pause");
            this.AddState("Play", "Play");
            this.MakeProfileAction("text;URL of media to cast");
        }

        private ChromeCastPlugin ChromeCastPlugin => this.Plugin as ChromeCastPlugin;

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
    }
}
