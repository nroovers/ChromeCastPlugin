namespace Loupedeck.ChromeCastPlugin
{
    using System;

    public class ChromeCastApplication : ClientApplication
    {
        public ChromeCastApplication()
        {
        }

        protected override String GetProcessName() => String.Empty;

        protected override String GetBundleName() => String.Empty;
    }
}