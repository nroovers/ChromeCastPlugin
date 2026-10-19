namespace Loupedeck.ChromeCastPlugin.ChromeCastWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal enum PlayBackState
    {
        Idle,
        Playing,
        Paused,
    }

    internal interface IChromeCastWrapper
    {
        event EventHandler<ChromeCastConnectedEventArgs> ChromeCastConnected;

        event EventHandler<ChromeCastStatusUpdatedEventArgs> StatusChanged;

        event EventHandler<ChromeCastsUpdatedEventArgs> ChromeCastsUpdated;

        Boolean IsContinuousSearchActive { get; }

        Boolean IsMuted { get; }

        Int32 Volume { get; }

        PlayBackState PlayBackState { get; }

        String PlayBackUrl { get; }

        IEnumerable<ChromeCast> ChromeCasts { get; }

        ChromeCast ConnectedChromeCast { get; }

        Boolean IsConnected { get; }

        Task<Boolean> ActivateContinuousSearch();

        Task<Boolean> DeactivateContinuousSearch();

        Task<Boolean> SearchChromeCasts();

        Task<Boolean> Connect(String chromeCastId);

        Task<Boolean> Reconnect();

        Task<Boolean> Disconnect();

        Task<Boolean> SetMuted(Boolean isMuted);

        Task<Boolean> SetVolume(Int32 volume);

        Task<Boolean> PlayCast(String url);

        Task<Boolean> PauseCast();

        Task<Boolean> StopCast();
    }
}
