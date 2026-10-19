namespace Loupedeck.ChromeCastPlugin.ChromeCastWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Threading.Tasks;
    using GoogleCast;
    using GoogleCast.Channels;
    using GoogleCast.Models.Media;
    using GoogleCast.Models.Receiver;

    internal class GoogleCastWrapper : IChromeCastWrapper
    {
        private readonly Sender _sender = new Sender();
        private IEnumerable<IReceiver> _receivers;
        private IReceiver _selectedReceiver;
        private IDisposable _unsubscribeFindReceiversContinuous;
        private Boolean _areEventhandlersConnected = false;

        public GoogleCastWrapper()
        {
        }

        public event EventHandler<ChromeCastConnectedEventArgs> ChromeCastConnected;

        public event EventHandler<ChromeCastStatusUpdatedEventArgs> StatusChanged;

        public event EventHandler<ChromeCastsUpdatedEventArgs> ChromeCastsUpdated;

        public Boolean IsMuted { get; private set; } = false;

        public Int32 Volume { get; private set; } = 50;

        public IEnumerable<ChromeCast> ChromeCasts =>
            this._receivers != null && this._receivers.Any()
            ? this._receivers.Select(receiver => this.ToChromeCast(receiver))
            : new List<ChromeCast>();

        public ChromeCast ConnectedChromeCast => this.IsConnected ? this.ToChromeCast(this._selectedReceiver) : null;

        public Boolean IsConnected => this._selectedReceiver != null;

        public PlayBackState PlayBackState { get; private set; } = PlayBackState.Idle;

        public String PlayBackUrl { get; private set; }

        public Boolean IsContinuousSearchActive { get; private set; } = false;

        public Task<Boolean> ActivateContinuousSearch()
        {
            this._unsubscribeFindReceiversContinuous?.Dispose();
            this._unsubscribeFindReceiversContinuous = new DeviceLocator().FindReceiversContinuous().Subscribe(this.OnNext);
            this.IsContinuousSearchActive = true;
            return Task.FromResult(true);
        }

        public Task<Boolean> DeactivateContinuousSearch()
        {
            this._unsubscribeFindReceiversContinuous?.Dispose();
            this._unsubscribeFindReceiversContinuous = null;
            this.IsContinuousSearchActive = false;
            return Task.FromResult(true);
        }

        public async Task<Boolean> SearchChromeCasts()
        {
            var deviceLocator = new DeviceLocator();
            var receivers = new List<IReceiver>();

            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(networkInterface => networkInterface.OperationalStatus == OperationalStatus.Up)
                .Where(networkInterface => networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Where(networkInterface => networkInterface.SupportsMulticast);

            foreach (var networkInterface in networkInterfaces)
            {
                try
                {
                    var foundReceivers = await deviceLocator.FindReceiversAsync(networkInterface);
                    if (foundReceivers != null)
                    {
                        receivers.AddRange(foundReceivers);
                    }
                }
                catch (Exception e)
                {
                    Loupedeck.ChromeCastPlugin.PluginLog.Warning(e, $"Receiver discovery failed on adapter '{networkInterface.Name}'");
                }
            }

            var discoveredReceivers = receivers
                .GroupBy(receiver => receiver.Id)
                .Select(group => group.First())
                .ToList();

            this._receivers = discoveredReceivers;

            this.ChromeCastsUpdated?.Invoke(this, new ChromeCastsUpdatedEventArgs());
            return true;
        }

        public async Task<Boolean> Connect(String chromeCastId)
        {
            if (String.IsNullOrEmpty(chromeCastId))
            {
                throw new ArgumentNullException("chromeCastId", "Parameter 'chromeCastId' cannot be null or empty");
            }

            var receiver = this._receivers?.FirstOrDefault(r => r.Id == chromeCastId);

            if (receiver == null)
            {
                throw new ArgumentException($"Chromecast with id {chromeCastId} not found", "chromeCastId");
            }

            var selectedReceiver = this._selectedReceiver;
            if (selectedReceiver?.Id == receiver.Id)
            {
                return true;
            }

            return await this.ConnectAndPublishAsync(receiver, chromeCastId);
        }

        public async Task<Boolean> Reconnect()
        {
            var selectedReceiver = this._selectedReceiver;
            if (selectedReceiver == null)
            {
                return true;
            }

            if (this._receivers == null || !this._receivers.Any())
            {
                await this.SearchChromeCasts();
            }

            if (this._receivers?.FirstOrDefault(r => r.Id == selectedReceiver.Id) == null)
            {
                this._selectedReceiver = null;

                this.ChromeCastConnected?.Invoke(this, new ChromeCastConnectedEventArgs());
                return true;
            }

            return await this.ConnectAndPublishAsync(selectedReceiver, selectedReceiver.Id);
        }

        public Task<Boolean> Disconnect()
        {
            this._selectedReceiver = null;
            this.PlayBackUrl = String.Empty;
            this.PlayBackState = PlayBackState.Idle;
            this.DisconnectEventHandlers();
            this._sender.Disconnect();
            return Task.FromResult(true);
        }

        public async Task<Boolean> SetMuted(Boolean isMuted)
        {
            this.IsMuted = isMuted;

            var receiverChannel = this._sender.GetChannel<IReceiverChannel>();
            if (receiverChannel == null)
            {
                return false;
            }

            await receiverChannel.SetIsMutedAsync(this.IsMuted);
            return true;
        }

        public async Task<Boolean> SetVolume(Int32 volume)
        {
            this.IsMuted = false;

            if (volume > 100)
            {
                this.Volume = 100;
            }
            else if (volume < 0)
            {
                this.Volume = 0;
            }
            else
            {
                this.Volume = volume;
            }

            var receiverChannel = this._sender.GetChannel<IReceiverChannel>();
            if (receiverChannel == null)
            {
                return false;
            }

            await receiverChannel.SetVolumeAsync(this.Volume == 0 ? 0 : this.Volume / (Single)100);
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

            this.IsMuted = status.Volume.IsMuted ?? false;
            this.Volume = (Int32)(status.Volume.Level * 100);

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
            var receiverChannel = this._sender.GetChannel<IReceiverChannel>();
            var mediaChannel = this._sender.GetChannel<IMediaChannel>();

            if (receiverChannel != null && mediaChannel != null && !this._areEventhandlersConnected)
            {
                receiverChannel.StatusChanged += this.GoogleCastWrapper_ReceiverStatusChanged;
                mediaChannel.StatusChanged += this.GoogleCastWrapper_MediaStatusChanged;
                this._areEventhandlersConnected = true;
            }
        }

        private void DisconnectEventHandlers()
        {
            var receiverChannel = this._sender.GetChannel<IReceiverChannel>();
            var mediaChannel = this._sender.GetChannel<IMediaChannel>();

            if (receiverChannel != null)
            {
                receiverChannel.StatusChanged -= this.GoogleCastWrapper_ReceiverStatusChanged;
            }

            if (mediaChannel != null)
            {
                mediaChannel.StatusChanged -= this.GoogleCastWrapper_MediaStatusChanged;
            }

            this._areEventhandlersConnected = false;
        }

        private async Task<Boolean> ConnectAndPublishAsync(IReceiver receiver, String chromeCastId)
        {
            this._selectedReceiver = receiver;

            await this._sender.ConnectAsync(receiver);
            await this.RefreshReceiverStatusAsync();
            this.ConnectEventHandlers();

            this.ChromeCastConnected?.Invoke(this, new ChromeCastConnectedEventArgs() { ChromeCastId = chromeCastId });
            return true;
        }

        private async Task RefreshReceiverStatusAsync()
        {
            var receiverChannel = this._sender.GetChannel<IReceiverChannel>();
            if (receiverChannel == null)
            {
                return;
            }

            var status = await receiverChannel.GetStatusAsync();
            this.SetReceiverStatus(status);
        }

        private void OnNext(IReceiver receiver)
        {
            if (receiver == null)
            {
                return;
            }

            if (this._receivers == null)
            {
                this._receivers = new List<IReceiver>();
            }

            this._receivers = this._receivers
                .Where(existing => existing.Id != receiver.Id)
                .Append(receiver)
                .ToList();

            this.ChromeCastsUpdated?.Invoke(this, new ChromeCastsUpdatedEventArgs());
        }
    }
}
