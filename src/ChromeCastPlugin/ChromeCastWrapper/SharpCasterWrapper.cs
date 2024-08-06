//namespace Loupedeck.CCastControllerPlugin.CCastApi
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Collections.ObjectModel;
//    using System.Linq;
//    using System.Net;
//    using System.Net.Sockets;
//    using System.Text;
//    using System.Threading.Tasks;

//    using SharpCaster.Models;
//    using SharpCaster.Services;

//    internal class SharpCasterApi : ICCastApi
//    {

//        private ObservableCollection<Chromecast> _chromecasts;

//        private Chromecast _connectedChromeCast;

//        public Boolean IsMuted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
//        public Int32 Volume { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

//        public IEnumerable<ChromeCast> ChromeCasts => this._chromecasts.Select(cc => this.ToChromeCast(cc));

//        public ChromeCast ConnectedChromeCast => this._connectedChromeCast != null ? this.ToChromeCast(this._connectedChromeCast) : null;

//        public event EventHandler<ChromeCastEventArgs> onChromeCastConnected;

//        public Task<Boolean> Connect(ChromeCast chromeCast) => throw new NotImplementedException();
//        public Boolean Disconnect() => throw new NotImplementedException();
//        public async Task<Boolean> SearchChromeCasts()
//        {
//            this._chromecasts = await ChromecastService.Current.DeviceLocator.LocateDevicesAsync();

//            if (this._chromecasts != null && !this._chromecasts.Any())
//            {
//                foreach (var ipAddress in this.GetLocalIpAddresses())
//                {
//                    this._chromecasts = await ChromecastService.Current.StartLocatingDevices(ipAddress);
//                    if (this._chromecasts != null && this._chromecasts.Any())
//                    {
//                        break;
//                    }
//                }
//                //if (!String.IsNullOrEmpty(ipAddress))
//                //{
//                //    this._chromecasts = await ChromecastService.Current.DeviceLocator.LocateDevicesAsync(ipAddress);
//                //}
//            }

//            return true;
//        }
//        public Boolean StopCast() => throw new NotImplementedException();
//        public Boolean TogglePlay() => throw new NotImplementedException();

//        private ChromeCast ToChromeCast(Chromecast cc)
//        {
//            return new ChromeCast()
//            {
//                Id = cc.DeviceUri.ToString(),
//                Name = cc.FriendlyName,
//            };
//        }

//        private IEnumerable<String> GetLocalIpAddresses()
//        {
//            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
//            return host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).Select(ip => ip.ToString());
//            //IPAddress ipAddress = host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();

//            //return ipAddress != null ? ipAddress.ToString() : "";
//        }
//    }
//}
