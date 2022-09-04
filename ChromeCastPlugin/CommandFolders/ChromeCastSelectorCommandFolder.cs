﻿namespace Loupedeck.ChromeCastPlugin.CommandFolders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Loupedeck.ChromeCastPlugin.ChromeCastWrapper;
    using Loupedeck.ChromeCastPlugin.Common;

    internal class ChromeCastSelectorCommandFolder : PluginDynamicFolder
    {
        public ChromeCastSelectorCommandFolder()
        {
            this.DisplayName = "Select Chromecast";
            this.GroupName = "Devices2";
            this.Navigation = PluginDynamicFolderNavigation.ButtonArea;
        }

        private ChromeCastPlugin ChromeCastPlugin => this.Plugin as ChromeCastPlugin;
        private IChromeCastWrapper ChromeCastWrapper => this.ChromeCastPlugin.ChromeCastApi;

        private Boolean _isLoaded = false;
        private readonly String _loadingCommand = "Loading...";
        private readonly String _notFoundCommand = "Chromecast not found";

        #region PluginDynamicFolder overrides

        public override Boolean Load()
        {
            if (this.ChromeCastWrapper != null)
            {
                this.ChromeCastWrapper.ChromeCastConnected += this.ChromeCastApi_onChromeCastConnected;
            }
            return base.Load();
        }

        public override Boolean Unload()
        {
            if (this.ChromeCastWrapper != null)
            {
                this.ChromeCastWrapper.ChromeCastConnected -= this.ChromeCastApi_onChromeCastConnected;
            }
            return base.Unload();
        }

        public override Boolean Activate()
        {
            this.LoadChromeCastRecievers();
            return true;
        }

        public override IEnumerable<String> GetButtonPressActionNames()
        {
            if (!this._isLoaded)
            {
                return new String[] { this.CreateCommandName(this._loadingCommand) };
            }
            if (this.ChromeCastWrapper.ChromeCasts == null || !this.ChromeCastWrapper.ChromeCasts.Any())
            {
                return new String[] { this.CreateCommandName(this._notFoundCommand) };
            }
            return this.ChromeCastWrapper.ChromeCasts
                .OrderBy(chromeCast => chromeCast.Name)
                .Select(chromeCast => this.CreateCommandName(chromeCast.Id));
        }

        public override String GetButtonDisplayName(PluginImageSize imageSize) =>
            this.ChromeCastWrapper.ConnectedChromeCast != null
            ? this.GetChromecastDisplayName(this.ChromeCastWrapper.ConnectedChromeCast) + " selected"
            : base.GetButtonDisplayName(imageSize);

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            //if (actionParameter == this._loadingCommand)
            //{
            //    return EmbeddedResources.ReadImage("Loupedeck.ChromeCastPlugin.Resources.Icons.loading_90x90.gif");
            //}
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
                    this.ChromeCastWrapper.Connect(selectedChromeCast);
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
        #endregion

        #region Private functions
        private void ChromeCastApi_onChromeCastConnected(Object sender, ChromeCastConnectedEventArgs e) => this.ButtonActionNamesChanged();

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

            //if (deviceDisplayName != null && !deviceDisplayName.Contains(" ") && deviceDisplayName.Length > 9)
            //{
            //    var updatedDisplayName = deviceDisplayName.Insert(9, "\n");
            //    return updatedDisplayName.Length > 18 ? updatedDisplayName.Insert(18, "\n") : updatedDisplayName;
            //}
        }
        #endregion
    }
}