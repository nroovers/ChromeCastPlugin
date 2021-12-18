namespace Loupedeck.ChromeCastPlugin.ChromeCastWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal enum PlayBackState
    {
        Idle,
        Playing,
        Paused
    }

    internal interface IChromeCastWrapper
    {
        event EventHandler<ChromeCastEventArgs> ChromeCastConnected;

        event EventHandler<ChromeCastStatusEventArgs> StatusChanged;

        Boolean IsMuted { get; set; }

        Int32 Volume { get; set; }

        PlayBackState PlayBackState { get; }

        IEnumerable<ChromeCast> ChromeCasts { get; }

        ChromeCast ConnectedChromeCast { get; }

        Task<Boolean> SearchChromeCasts();

        Task<Boolean> Connect(ChromeCast chromeCast);

        Boolean Disconnect();

        Task<Boolean> PlayCast(String url);

        Task<Boolean> PauseCast();

        Task<Boolean> StopCast();
    }
}
