﻿namespace Loupedeck.ChromeCastPlugin.CommandFolders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Loupedeck.ChromeCastPlugin.ChromeCastWrapper;

    internal class ChromeCastSelectorCommandFolder : PluginDynamicFolder
    {
        public ChromeCastSelectorCommandFolder()
        {
            this.DisplayName = "Select Chromecast";
            this.GroupName = "Chromecast";
            this.Navigation = PluginDynamicFolderNavigation.ButtonArea;
        }

        private ChromeCastPlugin ChromeCastPlugin => this.Plugin as ChromeCastPlugin;
        private IChromeCastWrapper ChromeCastWrapper => this.ChromeCastPlugin.ChromeCastApi;

        private Boolean _isLoaded = false;
        private readonly String _loadingCommand = "Loading...";
        private readonly String _notFoundCommand = "Chromecast not found";

        #region PluginDynamicFolder overrides
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
            ? this.ChromeCastWrapper.ConnectedChromeCast.Name + " selected"
            : base.GetButtonDisplayName(imageSize);

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            if (actionParameter == this._loadingCommand ||
                actionParameter == this._notFoundCommand)
            {
                return null;
            }

            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                if (this.ChromeCastWrapper.ConnectedChromeCast?.Id == actionParameter)
                {
                    bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage("Loupedeck.ChromeCastPlugin.Resources.Icons.selected_receiver.png"));
                    bitmapBuilder.DrawText(this.GetChromeCastDisplayName(actionParameter), BitmapColor.Black);
                    return bitmapBuilder.ToImage();
                }
                else
                {
                    bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage("Loupedeck.ChromeCastPlugin.Resources.Icons.unselected_receiver.png"));
                    bitmapBuilder.DrawText(this.GetChromeCastDisplayName(actionParameter), BitmapColor.White);
                    return bitmapBuilder.ToImage();
                }
            }
        }

        public override void RunCommand(String commandParameter)
        {
            if (commandParameter == this._loadingCommand)
            {
                return;
            }

            try
            {
                var previousReceiverCommandName = this.ChromeCastWrapper.ConnectedChromeCast?.Id;

                this.ChromeCastWrapper.Connect(this.ChromeCastWrapper.ChromeCasts.FirstOrDefault(x => x.Id == commandParameter));

                if (!String.IsNullOrEmpty(previousReceiverCommandName))
                {
                    this.CommandImageChanged(previousReceiverCommandName);
                }

                this.CommandImageChanged(commandParameter);
            }
            catch (Exception e)
            {
                this.ChromeCastPlugin.HandleError("Select chromecast failed", e);
            }
        }
        #endregion

        #region Private functions
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

        private String GetChromeCastDisplayName(String commandParameter)
        {
            if (commandParameter == this._loadingCommand)
            {
                return this._loadingCommand;
            }

            var receiverDisplayName = this.ChromeCastWrapper.ChromeCasts.FirstOrDefault(cc => cc.Id == commandParameter)?.Name;
            //if (deviceDisplayName != null && !deviceDisplayName.Contains(" ") && deviceDisplayName.Length > 9)
            //{
            //    var updatedDisplayName = deviceDisplayName.Insert(9, "\n");
            //    return updatedDisplayName.Length > 18 ? updatedDisplayName.Insert(18, "\n") : updatedDisplayName;
            //}

            return receiverDisplayName;
        }
        #endregion
    }
}