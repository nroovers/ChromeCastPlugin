namespace Loupedeck.ChromeCastPlugin.Commands
{
    using System;

    using Loupedeck.ChromeCastPlugin.ChromeCastWrapper;

    internal class TogglePlayDirectCommand : ActionEditorCommand
    {
        private const String UrlControlName = "url";
        private const String IsPreSelectedControlName = "pre-selected";
        private const String DeviceIdControlName = "deviceId";

        public TogglePlayDirectCommand()
        {
            this.DisplayName = "Play Direct";

            this.ActionEditor.AddControl(
                new ActionEditorTextbox(name: UrlControlName, labelText: "Url").SetRequired());
            this.ActionEditor.AddControl(
                new ActionEditorCheckbox(name: IsPreSelectedControlName, labelText: "Preselect device?"));
            this.ActionEditor.AddControl(
                new ActionEditorListbox(name: DeviceIdControlName, labelText: "Device"));

            this.ActionEditor.ListboxItemsRequested += this.ActionEditor_ListboxItemsRequested;
            this.ActionEditor.ControlValueChanged += this.ActionEditor_ControlValueChanged;
        }

        private ChromeCastPlugin ChromeCastPlugin => this.Plugin as ChromeCastPlugin;

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
                DeviceId = actionParameters.GetString(DeviceIdControlName),
            });
            return true;
        }

        private async void PlayPauseAsync(Parameters parameters)
        {
            if (parameters.IsPreselected &&
                (this.ChromeCastWrapper.ConnectedChromeCast == null || this.ChromeCastWrapper.ConnectedChromeCast.Id != parameters.DeviceId))
            {
                await this.ChromeCastWrapper.Connect(parameters.DeviceId);
            }

            try
            {
                if (this.ChromeCastWrapper.PlayBackUrl != parameters.Url ||
                    this.ChromeCastWrapper.PlayBackState != PlayBackState.Playing)
                {
                    this.ChromeCastWrapper.PlayCast(parameters.Url);
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

        private void ChromeCastWrapper_ChromeCastsUpdated(Object _, ChromeCastsUpdatedEventArgs e) => this.ActionEditor.ListboxItemsChanged(DeviceIdControlName);

        private void ActionEditor_ControlValueChanged(Object _, ActionEditorControlValueChangedEventArgs e)
        {
            if (e.ControlName.EqualsNoCase(IsPreSelectedControlName))
            {
                if (e.ActionEditorState.GetControlValue(IsPreSelectedControlName) == "true")
                {
                    this.ActionEditor.ListboxItemsChanged(DeviceIdControlName);
                }
            }
        }

        private void ActionEditor_ListboxItemsRequested(Object _, ActionEditorListboxItemsRequestedEventArgs e)
        {
            if (e.ControlName.EqualsNoCase(DeviceIdControlName))
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

            public String DeviceId { get; set; }
        }
    }
}
