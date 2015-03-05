using System;

namespace MyLife.Core
{
    public abstract class WebChannel : ChannelBase
    {
        protected WebChannel(HttpHelperBase httpHelper)
        {
            HttpHelper = httpHelper;
        }

        
        public HttpHelperBase HttpHelper { get; private set; }


        public override void Init()
        {
            base.Init();
            HttpHelper.ResponseReceived += HttpHelper_OnResponseReceived;
        }

        protected virtual void HttpHelper_OnResponseReceived(object sender, HttpHelperResponseEventArgs args)
        {
            
        }


    }
}
