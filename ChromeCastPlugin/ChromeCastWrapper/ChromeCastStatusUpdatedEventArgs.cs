namespace Loupedeck.ChromeCastPlugin.ChromeCastWrapper
{
    using System;

    public class ChromeCastStatusUpdatedEventArgs
    {
        internal Int32 Volume { get; set; }

        internal Boolean IsMuted { get; set; }
    }
}