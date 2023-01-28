namespace Loupedeck.ChromeCastPlugin.CommandFolders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Loupedeck.ChromeCastPlugin.ChromeCastWrapper;
    using Loupedeck.ChromeCastPlugin.Common;

    internal class ChromeCastSelectorCommandFolder : PluginDynamicFolder
    {
        private readonly String _loadingCommand = "Searching...";
        private readonly String _notFoundCommand = "Chromecast not found";
        private Boolean _isLoaded;

        public ChromeCastSelectorCommandFolder()
        {
            this.DisplayName = "Select Chromecast";
            this.GroupName = "Devices";
        }

        private ChromeCastPlugin ChromeCastPlugin => this.Plugin as ChromeCastPlugin;

        private IChromeCastWrapper ChromeCastWrapper => this.ChromeCastPlugin.ChromeCastApi;

        public override Boolean Load()
        {
            this._isLoaded = this.ChromeCastWrapper.IsContinuousSearchActive;
            return base.Load();
        }

        public override Boolean Activate()
        {
            if (this.ChromeCastWrapper != null)
            {
                this.ChromeCastWrapper.ChromeCastConnected += this.ChromeCastWrapper_ChromeCastConnected;
                this.ChromeCastWrapper.ChromeCastsUpdated += this.ChromeCastWrapper_ChromeCastsUpdated;
            }

            if (!this._isLoaded)
            {
                this.LoadChromeCastRecievers();
            }

            return base.Activate();
        }

        public override Boolean Deactivate()
        {
            if (this.ChromeCastWrapper != null)
            {
                this.ChromeCastWrapper.ChromeCastConnected -= this.ChromeCastWrapper_ChromeCastConnected;
                this.ChromeCastWrapper.ChromeCastsUpdated -= this.ChromeCastWrapper_ChromeCastsUpdated;
            }

            return base.Deactivate();
        }


        public override PluginDynamicFolderNavigation GetNavigationArea(DeviceType _) => PluginDynamicFolderNavigation.ButtonArea;

        public override IEnumerable<String> GetButtonPressActionNames(DeviceType _)
        {
            if (!this._isLoaded)
            {
                return new String[] { this.CreateCommandName(this._loadingCommand) };
            }
            else if (this.ChromeCastWrapper.ChromeCasts == null || !this.ChromeCastWrapper.ChromeCasts.Any())
            {
                return new String[] { this.CreateCommandName(this._notFoundCommand) };
            }
            else
            {
                return this.ChromeCastWrapper.ChromeCasts
                    .OrderBy(chromeCast => chromeCast.Name)
                    .Select(chromeCast => this.CreateCommandName(chromeCast.Id));
            }
        }

        public override String GetButtonDisplayName(PluginImageSize imageSize) =>
            this.ChromeCastWrapper.IsConnected
            ? this.GetChromecastDisplayName(this.ChromeCastWrapper.ConnectedChromeCast) + " selected"
            : base.GetButtonDisplayName(imageSize);

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                bitmapBuilder.FillRectangle(0, 0, 90, 90, BitmapColor.Black);
                if (this.ChromeCastWrapper.ConnectedChromeCast?.Id == actionParameter)
                {
                    bitmapBuilder.DrawText(this.GetCommandDisplayName(actionParameter), BitmapColor.White);
                    bitmapBuilder.DrawLine(0, 5, 90, 5, Theme.PrimaryColor, 5);
                    bitmapBuilder.DrawLine(0, 75, 90, 75, Theme.PrimaryColor, 5);
                }
                else if (actionParameter == this._notFoundCommand ||
                    actionParameter == this._loadingCommand)
                {
                    bitmapBuilder.DrawText(actionParameter, BitmapColor.White);
                }
                else
                {
                    bitmapBuilder.DrawText(this.GetCommandDisplayName(actionParameter), BitmapColor.White);
                }

                return bitmapBuilder.ToImage();
            }
        }

        public override void RunCommand(String commandParameter)
        {
            if (commandParameter == this._loadingCommand)
            {
                return;
            }

            if (commandParameter == this._notFoundCommand)
            {
                this._isLoaded = false;
                this.ButtonActionNamesChanged();
                this.LoadChromeCastRecievers();
                return;
            }

            try
            {
                var previousChromeCastCommandParameter = this.ChromeCastWrapper.ConnectedChromeCast?.Id;

                var selectedChromeCast = this.ChromeCastWrapper.ChromeCasts.FirstOrDefault(x => x.Id == commandParameter);

                if (selectedChromeCast != null)
                {
                    this.ChromeCastWrapper.Connect(selectedChromeCast.Id);
                    this.CommandImageChanged(commandParameter);
                }

                if (!String.IsNullOrEmpty(previousChromeCastCommandParameter))
                {
                    this.CommandImageChanged(previousChromeCastCommandParameter);
                }
            }
            catch (Exception e)
            {
                this.ChromeCastPlugin.HandleError("Select chromecast failed", e);
            }
        }

        private void ChromeCastWrapper_ChromeCastConnected(Object sender, ChromeCastConnectedEventArgs e) => this.ButtonActionNamesChanged();

        private void ChromeCastWrapper_ChromeCastsUpdated(Object sender, ChromeCastsUpdatedEventArgs e) => this.ButtonActionNamesChanged();

        private async void LoadChromeCastRecievers()
        {
            try
            {
                await this.ChromeCastWrapper.SearchChromeCasts();
                this._isLoaded = true;
                this.ButtonActionNamesChanged();
            }
            catch (Exception e)
            {
                this.ChromeCastPlugin.HandleError("Loading chromecasts failed", e);
            }
        }

        private String GetCommandDisplayName(String commandParameter)
        {
            return commandParameter == this._loadingCommand
                ? this._loadingCommand
                : this.GetChromecastDisplayName(this.ChromeCastWrapper.ChromeCasts.FirstOrDefault(cc => cc.Id == commandParameter));
        }

        private String GetChromecastDisplayName(ChromeCast chromeCast)
        {
            return chromeCast?.Name;
        }
    }
}