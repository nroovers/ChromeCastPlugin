namespace Loupedeck.ChromeCastPlugin.ChromeCastWrapper
{
    using System;

    internal class ChromeCastStatusUpdatedEventArgs
    {
        public Int32 Volume { get; set; }

        public Boolean IsMuted { get; set; }

        public PlayBackState PlayBackState { get; set; }

        public String PlayBackUrl { get; set; }
    }
}