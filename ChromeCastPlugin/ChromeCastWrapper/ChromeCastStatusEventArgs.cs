namespace Loupedeck.ChromeCastPlugin.ChromeCastWrapper
{
    using System;
    public class ChromeCastStatusEventArgs
    {
        internal Int32 Volume { get; set; }

        internal Boolean IsMuted { get; set; }
    }
}