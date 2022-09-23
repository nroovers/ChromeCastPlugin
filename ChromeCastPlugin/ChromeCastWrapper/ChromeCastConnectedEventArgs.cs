namespace Loupedeck.ChromeCastPlugin.ChromeCastWrapper
{
    using System;

    internal class ChromeCastConnectedEventArgs : EventArgs
    {
        internal String ChromeCastId { get; set; }
    }
}