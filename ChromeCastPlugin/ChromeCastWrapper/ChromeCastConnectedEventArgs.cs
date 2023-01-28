namespace Loupedeck.ChromeCastPlugin.ChromeCastWrapper
{
    using System;

    internal class ChromeCastConnectedEventArgs : EventArgs
    {
        public String ChromeCastId { get; set; }
    }
}