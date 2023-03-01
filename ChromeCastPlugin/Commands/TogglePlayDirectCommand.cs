namespace Loupedeck.ChromeCastPlugin.Commands
{
    using System;

    using Loupedeck.ChromeCastPlugin.ChromeCastWrapper;

    internal class TogglePlayDirectCommand : ActionEditorCommand
    {
        private const String UrlControlName = "url";
        private const String IsPreSelectedControlName = "pre-selected";
        private const String DeviceControlName = "device";

        public TogglePlayDirectCommand()
        {
            this.DisplayName = "Play Cast";
            this.Description = "Play and pause online media";

            this.ActionEditor.AddControl(
                new ActionEditorTextbox(name: UrlControlName, labelText: "Url").SetRequired());
            this.ActionEditor.AddControl(
                new ActionEditorCheckbox(
                    name: IsPreSelectedControlName,
                    labelText: "Preselect chromecast?",
                    description: "If checked, select a chromecast from the list below to play cast from that device"));
            this.ActionEditor.AddControl(
                new ActionEditorListbox(name: DeviceControlName, labelText: "Chromecast"));

            this.ActionEditor.ListboxItemsRequested += this.ActionEditor_ListboxItemsRequested;
            this.ActionEditor.ControlValueChanged += this.ActionEditor_ControlValueChanged;
        }

        private Chromecast ChromeCastPlugin => this.Plugin as Chromecast;

        private IChromeCastWrapper ChromeCastWrapper => this.ChromeCastPlugin.ChromeCastApi;

        protected override Boolean OnLoad()
        {
            this.ChromeCastWrapper.ChromeCastsUpdated += this.ChromeCastWrapper_ChromeCastsUpdated;

            return base.OnLoad();
        }

        protected override Boolean OnUnload()
        {
            this.ChromeCastWrapper.ChromeCastsUpdated -= this.ChromeCastWrapper_ChromeCastsUpdated;

            return base.OnUnload();
        }

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            if (this.ChromeCastWrapper == null)
            {
                return true;
            }

            this.PlayPauseAsync(new Parameters()
            {
                Url = actionParameters.GetString(UrlControlName),
                IsPreselected = actionParameters.GetBoolean(IsPreSelectedControlName),
                Device = actionParameters.GetString(DeviceControlName),
            });
            return true;
        }

        private async void PlayPauseAsync(Parameters parameters)
        {
            try
            {
                if (parameters.IsPreselected &&
                    (!this.ChromeCastWrapper.IsConnected || this.ChromeCastWrapper.ConnectedChromeCast.Id != parameters.Device))
                {
                    await this.ChromeCastWrapper.Connect(parameters.Device);
                }

                if (this.ChromeCastWrapper.PlayBackUrl != parameters.Url ||
                    this.ChromeCastWrapper.PlayBackState != PlayBackState.Playing)
                {
                    await this.ChromeCastWrapper.PlayCast(parameters.Url);
                }
                else
                {
                    await this.ChromeCastWrapper.PauseCast();
                }
            }
            catch (Exception e)
            {
                this.ChromeCastPlugin.HandleError("Toggle play failed", e);
            }
        }

        private void ChromeCastWrapper_ChromeCastsUpdated(Object _, ChromeCastsUpdatedEventArgs e) => this.ActionEditor.ListboxItemsChanged(DeviceControlName);

        private void ActionEditor_ControlValueChanged(Object _, ActionEditorControlValueChangedEventArgs e)
        {
            var isDeviceControlValid = !(e.ActionEditorState.GetControlValue(IsPreSelectedControlName) == "true" &&
                String.IsNullOrEmpty(e.ActionEditorState.GetControlValue(DeviceControlName)));

            e.ActionEditorState.SetValidity(DeviceControlName, isDeviceControlValid, "Please select a chromecast");
        }

        private void ActionEditor_ListboxItemsRequested(Object _, ActionEditorListboxItemsRequestedEventArgs e)
        {
            if (e.ControlName.EqualsNoCase(DeviceControlName))
            {
                foreach (var chromeCast in this.ChromeCastWrapper.ChromeCasts)
                {
                    e.AddItem(name: chromeCast.Id, displayName: chromeCast.Name, description: null);
                }
            }
        }

        private class Parameters
        {
            public String Url { get; set; }

            public Boolean IsPreselected { get; set; }

            public String Device { get; set; }
        }
    }
}
