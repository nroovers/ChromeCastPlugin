namespace Loupedeck.ChromeCastPlugin.CommandFolders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Loupedeck.ChromeCastPlugin.ChromeCastWrapper;

    internal class ChromeCastSelectorCommandFolder : PluginDynamicFolder
    {

        private IChromeCastWrapper ChromeCastWrapper => (this.Plugin as ChromeCastPlugin).ChromeCastApi;

        private Boolean _isLoaded = false;
        private readonly String _loadingCommand = "loading...";
        private readonly String _notFoundCommand = "chromecast not found";

        public ChromeCastSelectorCommandFolder()
        {
            this.DisplayName = "Select ChromeCast";
            this.GroupName = "Chromecast";
            this.Navigation = PluginDynamicFolderNavigation.ButtonArea;
        }

        //public override LibraryImage GetButtonLibraryImage() => new LibraryImage("Devices.png");

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
                Tracer.Trace($"ChromeCastPlugin: ", e);
            }
        }

        public override String GetCommandDisplayName(String commandParameter, PluginImageSize imageSize) => this.GetChromeCastDisplayName(commandParameter);

        public override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            if (actionParameter == this._loadingCommand ||
                actionParameter == this._notFoundCommand)
            {
                return null;
            }

            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                if(this.ChromeCastWrapper.ConnectedChromeCast?.Id == actionParameter)
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
                Tracer.Trace($"ChromeCastPlugin: ", e);
            }
        }
    }
}