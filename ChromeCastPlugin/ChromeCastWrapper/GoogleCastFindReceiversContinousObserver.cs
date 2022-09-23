//namespace Loupedeck.ChromeCastPlugin.ChromeCastWrapper
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Text;
//    using System.Threading.Tasks;

//    using GoogleCast;

//    internal class GoogleCastFindReceiversContinousObserver : IObserver<IReceiver>
//    {

//        private readonly IChromeCastWrapper _chromeCastWrapper;

//        public GoogleCastFindReceiversContinousObserver (IChromeCastWrapper chromeCastWrapper)
//        {
//            this._chromeCastWrapper = chromeCastWrapper;
//        }

//        public void OnCompleted() => throw new NotImplementedException();
//        public void OnError(Exception error) => throw new NotImplementedException();
//        public void OnNext(IReceiver value)
//        {
//            this._receivers = this._receivers.Append(receiver);
//            this._chromeCastWrapper?.ChromeCastsUpdated?.Invoke(this, new ChromeCastsUpdatedEventArgs());
//        }
//    }
//}
