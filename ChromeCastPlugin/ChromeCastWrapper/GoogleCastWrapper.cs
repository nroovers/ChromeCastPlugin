namespace Loupedeck.ChromeCastPlugin.ChromeCastWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using GoogleCast;
    using GoogleCast.Channels;
    using GoogleCast.Messages;
    using GoogleCast.Models.Media;
    using GoogleCast.Models.Receiver;

    internal class GoogleCastWrapper : IChromeCastWrapper
    {
        private readonly Sender _sender = new Sender();
        private IEnumerable<IReceiver> _receivers;
        private IReceiver _selectedReceiver;
        private Boolean _isMuted = false;
        private Int32 _volume = 50;
        private IDisposable _unsubscribeFindReceiversContinuous;
        private Boolean _areEventhandlersConnected = false;

        public GoogleCastWrapper()
        {
        }

        public event EventHandler<ChromeCastConnectedEventArgs> ChromeCastConnected;

        public event EventHandler<ChromeCastStatusUpdatedEventArgs> StatusChanged;

        public event EventHandler<ChromeCastsUpdatedEventArgs> ChromeCastsUpdated;

        public Boolean IsMuted
        {
            get => this._isMuted;
            set
            {
                this._isMuted = value;
                this._sender.GetChannel<IReceiverChannel>()?.SetIsMutedAsync(this._isMuted);
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

                this._sender.GetChannel<IReceiverChannel>()?.SetVolumeAsync(this._volume == 0 ? 0 : this._volume / (Single)100);
            }
        }

        public IEnumerable<ChromeCast> ChromeCasts =>
            this._receivers != null && this._receivers.Any()
            ? this._receivers.Select(receiver => this.ToChromeCast(receiver))
            : new List<ChromeCast>();

        public ChromeCast ConnectedChromeCast => this.IsConnected ? this.ToChromeCast(this._selectedReceiver) : null;

        public Boolean IsConnected => this._selectedReceiver != null;

        public PlayBackState PlayBackState { get; private set; } = PlayBackState.Idle;

        public String PlayBackUrl { get; private set; }

        public Boolean IsContinuousSearchActive { get; private set; } = false;

        public Boolean ActivateContinuousSearch()
        {
            this._unsubscribeFindReceiversContinuous?.Dispose();
            this._unsubscribeFindReceiversContinuous = new DeviceLocator().FindReceiversContinuous().Subscribe(this.OnNext);
            this.IsContinuousSearchActive = true;
            return true;
        }

        public Boolean DeactivateContinuousSearch()
        {
            this._unsubscribeFindReceiversContinuous?.Dispose();
            this.IsContinuousSearchActive = false;
            return true;
        }

        public async Task<Boolean> SearchChromeCasts()
        {
            this._receivers = await new DeviceLocator().FindReceiversAsync();
            this.ChromeCastsUpdated?.Invoke(this, new ChromeCastsUpdatedEventArgs());
            return true;
        }

        public async Task<Boolean> Connect(String chromeCastId)
        {
            if (String.IsNullOrEmpty(chromeCastId))
            {
                throw new ArgumentNullException("chromeCastId", "Parameter 'chromeCastId' cannot be null or empty");
            }

            var receiver = this._receivers.FirstOrDefault(r => r.Id == chromeCastId);

            if (receiver == null)
            {
                throw new ArgumentException($"Chromecast with id {chromeCastId} not found", "chromeCastId");
            }

            if (this._selectedReceiver?.Id == receiver.Id)
            {
                return true;
            }

            this._selectedReceiver = receiver;

            await this._sender.ConnectAsync(this._selectedReceiver);

            var status = await this._sender.GetChannel<IReceiverChannel>().GetStatusAsync();
            this.SetReceiverStatus(status);

            this.ConnectEventHandlers();

            this.ChromeCastConnected?.Invoke(this, new ChromeCastConnectedEventArgs() { ChromeCastId = chromeCastId });

            return true;
        }


        public async Task<Boolean> Reconnect()
        {
            if (this._selectedReceiver == null)
            {
                return true;
            }

            if (this._receivers == null || this._receivers.Count() == 0)
            {
                await this.SearchChromeCasts();
            }

            if (this._receivers.FirstOrDefault(r => r.Id == this._selectedReceiver.Id) == null)
            {
                this._selectedReceiver = null;
                this.ChromeCastConnected?.Invoke(this, new ChromeCastConnectedEventArgs());
                return true;
            }

            await this._sender.ConnectAsync(this._selectedReceiver);

            var status = await this._sender.GetChannel<IReceiverChannel>().GetStatusAsync();
            this.SetReceiverStatus(status);

            this.ConnectEventHandlers();

            this.ChromeCastConnected?.Invoke(this, new ChromeCastConnectedEventArgs() { ChromeCastId = this._selectedReceiver.Id });

            return true;
        }

        public Boolean Disconnect()
        {
            this._selectedReceiver = null;
            this.PlayBackUrl = String.Empty;
            this.PlayBackState = PlayBackState.Idle;
            this.DisconnectEventHandlers();
            this._sender.Disconnect();
            return true;
        }

        public async Task<Boolean> PlayCast(String url)
        {
            if (String.IsNullOrEmpty(url))
            {
                throw new ArgumentException("Argument not set", "url");
            }

            var mediaChannel = this._sender.GetChannel<IMediaChannel>();
            if (mediaChannel == null ||
                this._selectedReceiver == null)
            {
                return false;
            }

            if (this.PlayBackState == PlayBackState.Idle)
            {
                await this._sender.LaunchAsync(mediaChannel);
            }

            if (this.PlayBackUrl != url)
            {
                await mediaChannel.LoadAsync(new MediaInformation()
                {
                    ContentId = url,
                    Metadata = new GenericMediaMetadata()
                    {
                        Title = "Chromecast Plugin for Loupedeck",
                        Subtitle = url,
                    },
                });
            }
            else if (this.PlayBackState != PlayBackState.Playing)
            {
                await mediaChannel.PlayAsync();
            }

            return true;
        }

        public async Task<Boolean> PauseCast()
        {
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

        public async Task<Boolean> StopCast()
        {
            if (this._sender.GetChannel<IReceiverChannel>() == null ||
                this._selectedReceiver == null)
            {
                return false;
            }

            await this._sender.GetChannel<IReceiverChannel>().StopAsync();
            return true;
        }

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

            this.StatusChanged?.Invoke(this, new ChromeCastStatusUpdatedEventArgs()
            {
                Volume = this.Volume,
                IsMuted = this.IsMuted,
                PlayBackUrl = this.PlayBackUrl,
                PlayBackState = this.PlayBackState,
            });
        }

        private void SetMediaStatus(MediaStatus status)
        {
            if (status == null)
            {
                this.PlayBackState = PlayBackState.Idle;
                this.PlayBackUrl = String.Empty;
            }
            else if (status.PlayerState == "PLAYING")
            {
                this.PlayBackState = PlayBackState.Playing;
                if (status.Media != null)
                {
                    this.PlayBackUrl = status.Media.ContentId;
                }
            }
            else if (status.PlayerState == "PAUSED")
            {
                this.PlayBackState = PlayBackState.Paused;
            }

            this.StatusChanged?.Invoke(this, new ChromeCastStatusUpdatedEventArgs()
            {
                Volume = this.Volume,
                IsMuted = this.IsMuted,
                PlayBackUrl = this.PlayBackUrl,
                PlayBackState = this.PlayBackState,
            });
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

        private void ConnectEventHandlers()
        {
            if (this._sender.GetChannel<IReceiverChannel>() != null && !this._areEventhandlersConnected)
            {
                this._sender.GetChannel<IReceiverChannel>().StatusChanged += this.GoogleCastWrapper_ReceiverStatusChanged;
                this._sender.GetChannel<IMediaChannel>().StatusChanged += this.GoogleCastWrapper_MediaStatusChanged;
                this._areEventhandlersConnected = true;
            }
        }

        private void DisconnectEventHandlers()
        {
            if (this._sender.GetChannel<IReceiverChannel>() != null)
            {
                this._sender.GetChannel<IReceiverChannel>().StatusChanged -= this.GoogleCastWrapper_ReceiverStatusChanged;
                this._sender.GetChannel<IMediaChannel>().StatusChanged -= this.GoogleCastWrapper_MediaStatusChanged;
                this._areEventhandlersConnected = false;
            }
        }

        private void OnNext(IReceiver receiver)
        {
            if (this._receivers == null)
            {
                this._receivers = new List<IReceiver>();
            }

            this._receivers = this._receivers.Append(receiver);
            this.ChromeCastsUpdated?.Invoke(this, new ChromeCastsUpdatedEventArgs());
        }
    }
}
