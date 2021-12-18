namespace Loupedeck.ChromeCastPlugin.ChromeCastWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using GoogleCast;
    using GoogleCast.Channels;
    using GoogleCast.Models.Media;
    using GoogleCast.Models.Receiver;

    internal class GoogleCastWrapper : IChromeCastWrapper
    {

        private IEnumerable<IReceiver> _receivers;
        private IReceiver _selectedReceiver;
        private Boolean _isMuted = false;
        private Int32 _volume = 50;
        private readonly Sender _sender = new Sender();
        //private Application _application;

        #region IChromeCastWrapper properties

        public Boolean IsMuted
        {
            get => this._isMuted;
            set
            {
                this._isMuted = value;
                if (this._sender.GetChannel<IReceiverChannel>() != null)
                {
                    this._sender.GetChannel<IReceiverChannel>().SetIsMutedAsync(this._isMuted);
                }
            }
        }

        public Int32 Volume
        {
            get => this._volume;
            set
            {
                this._isMuted = false;

                if (value > 100)
                {
                    this._volume = 100;
                }
                else if (value < 0)
                {
                    this._volume = 0;
                }
                else
                {
                    this._volume = value;
                }

                if (this._sender.GetChannel<IReceiverChannel>() != null)
                {
                    this._sender.GetChannel<IReceiverChannel>().SetVolumeAsync(this._volume == 0 ? 0 : this._volume / (Single)100);
                }
            }
        }

        public IEnumerable<ChromeCast> ChromeCasts =>
            this._receivers != null && this._receivers.Any()
            ? this._receivers.Select(receiver => this.ToChromeCast(receiver))
            : new List<ChromeCast>();

        public ChromeCast ConnectedChromeCast => this._selectedReceiver != null ? this.ToChromeCast(this._selectedReceiver) : null;

        public PlayBackState PlayBackState { get; private set; } = PlayBackState.Idle;

        public String PlayBackUrl { get; private set; }

        #endregion

        #region IChromeCastWrapper event handlers

        public event EventHandler<ChromeCastEventArgs> ChromeCastConnected;

        public event EventHandler<ChromeCastStatusEventArgs> StatusChanged;

        #endregion

        #region IChromeCastWrapper functions
        public async Task<Boolean> Connect(ChromeCast chromeCast)
        {
            if (chromeCast == null)
            {
                return false;
            }

            try
            {
                var receiver = this._receivers.FirstOrDefault(r => r.Id == chromeCast.Id);

                if (receiver != null && 
                    this._selectedReceiver?.Id != receiver.Id)
                {
                    this._selectedReceiver = receiver;

                    await this._sender.ConnectAsync(this._selectedReceiver);

                    var status = await this._sender.GetChannel<IReceiverChannel>().GetStatusAsync();

                    this.SetReceiverStatus(status);

                    if (this._sender.GetChannel<IReceiverChannel>() != null)
                    {
                        this._sender.GetChannel<IReceiverChannel>().StatusChanged += this.GoogleCastWrapper_ReceiverStatusChanged;
                        this._sender.GetChannel<IMediaChannel>().StatusChanged += this.GoogleCastWrapper_MediaStatusChanged;
                    }

                    this.ChromeCastConnected?.Invoke(this, new ChromeCastEventArgs() { ChromeCast = chromeCast });
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public Boolean Disconnect()
        {
            this._selectedReceiver = null;
            this.PlayBackUrl = String.Empty;
            this.PlayBackState = PlayBackState.Idle;

            if (this._sender != null)
            {
                if (this._sender.GetChannel<IReceiverChannel>() != null)
                {
                    this._sender.GetChannel<IReceiverChannel>().StatusChanged -= this.GoogleCastWrapper_ReceiverStatusChanged;
                    this._sender.GetChannel<IMediaChannel>().StatusChanged -= this.GoogleCastWrapper_MediaStatusChanged;
                }

                this._sender.Disconnect();
            }

            return true;
        }

        public async Task<Boolean> SearchChromeCasts()
        {
            this._receivers = await new DeviceLocator().FindReceiversAsync();

            return true;
        }

        public async Task<Boolean> StopCast()
        {
            if (this._sender.GetChannel<IReceiverChannel>() == null ||
                this._selectedReceiver == null)
            {
                return false;
            }

            await this._sender.GetChannel<IReceiverChannel>().StopAsync();

            this.PlayBackUrl = String.Empty;

            return true;
        }

        public async Task<Boolean> PlayCast(String url)
        {
            if (String.IsNullOrEmpty(url))
            {
                throw new ArgumentException("Argument not defined", "url");
            }

            var mediaChannel = this._sender.GetChannel<IMediaChannel>();
            if (mediaChannel == null ||
                this._selectedReceiver == null)
            {
                return false;
            }

            if (this.PlayBackState == PlayBackState.Idle)
            {
                // TODO: find a way to avoid calling this method when playing after stopping cast
                await this._sender.LaunchAsync(mediaChannel);
            }

            if (this.PlayBackUrl != url)
            {
                await mediaChannel.LoadAsync(new MediaInformation() { ContentId = url });
                this.PlayBackUrl = url;
            }
            else if (this.PlayBackState != PlayBackState.Playing)
            {
                await mediaChannel.PlayAsync();
            }

            return true;
        }

        public async Task<Boolean> PauseCast()
        {
            ////var recStatus = await this._sender.LaunchAsync(this._application.AppId);
            ////var varStatus = await this._sender.GetChannel<IMediaChannel>().GetStatusAsync();
            ////await this._sender.GetChannel<IMediaChannel>().PauseAsync();

            var mediaChannel = this._sender.GetChannel<IMediaChannel>();
            if (mediaChannel == null ||
                this._selectedReceiver == null)
            {
                return false;
            }

            if (this.PlayBackState == PlayBackState.Playing)
            {
                await mediaChannel.PauseAsync();
            }

            return true;
        }
        #endregion

        #region Private functions

        private void GoogleCastWrapper_ReceiverStatusChanged(Object sender, EventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            var receiverChannel = sender as IReceiverChannel;
            this.SetReceiverStatus(receiverChannel?.Status);
        }

        private void GoogleCastWrapper_MediaStatusChanged(Object sender, EventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            var mediaChannel = sender as IMediaChannel;
            this.SetMediaStatus(mediaChannel?.Status?.FirstOrDefault());
        }

        private void SetReceiverStatus(ReceiverStatus status)
        {
            if (status == null)
            {
                return;
            }

            this._isMuted = status.Volume.IsMuted ?? false;
            this._volume = (Int32)(status.Volume.Level * 100);
            //this._application = status.Applications?.FirstOrDefault();

            this.StatusChanged?.Invoke(this, new ChromeCastStatusEventArgs() { Volume = this._volume, IsMuted = this._isMuted });

            //else
            //{
            //    this._isMuted = false;
            //    this._volume = 50;
            //    this._application = null;
            //}
        }

        private void SetMediaStatus(MediaStatus status)
        {
            if (status == null)
            {
                this.PlayBackState = PlayBackState.Idle;
                return;
            }

            if (status.PlayerState == "PLAYING")
            {
                this.PlayBackState = PlayBackState.Playing;
            }
            else if (status.PlayerState == "PAUSED")
            {
                this.PlayBackState = PlayBackState.Paused;
            }
            else
            {
                this.PlayBackState = PlayBackState.Idle;
            }
        }

        private ChromeCast ToChromeCast(IReceiver receiver)
        {
            return new ChromeCast()
            {
                Id = receiver.Id,
                Name = receiver.FriendlyName,
                Ip = receiver.IPEndPoint.ToString(),
            };
        }
        #endregion
    }
}
