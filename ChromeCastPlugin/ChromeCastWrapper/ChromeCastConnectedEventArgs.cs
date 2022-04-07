namespace Loupedeck.ChromeCastPlugin.ChromeCastWrapper
{
    using System;

    internal class ChromeCastConnectedEventArgs : EventArgs
    {
        internal ChromeCast ChromeCast { get; set; }
    }
}