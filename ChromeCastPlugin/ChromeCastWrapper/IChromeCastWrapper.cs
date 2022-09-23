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
        event EventHandler<ChromeCastConnectedEventArgs> ChromeCastConnected;

        event EventHandler<ChromeCastStatusUpdatedEventArgs> StatusChanged;

        event EventHandler<ChromeCastsUpdatedEventArgs> ChromeCastsUpdated;

        Boolean IsContinuousSearchActive { get; }

        Boolean IsMuted { get; set; }

        Int32 Volume { get; set; }

        PlayBackState PlayBackState { get; }

        String PlayBackUrl { get; }

        IEnumerable<ChromeCast> ChromeCasts { get; }

        ChromeCast ConnectedChromeCast { get; }

        Task<Boolean> SearchChromeCasts();

        Boolean ActivateContinuousSearch();

        Boolean DeactivateContinuousSearch();

        Task<Boolean> ReConnect();

        Task<Boolean> Connect(ChromeCast chromeCast);

        Boolean Disconnect();

        Task<Boolean> PlayCast(String url);

        Task<Boolean> PauseCast();

        Task<Boolean> StopCast();
    }
}
