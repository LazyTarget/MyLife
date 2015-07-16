using System;

namespace MyLife.Core
{
    public class HttpHelperResponseEventArgs : EventArgs
    {
        public HttpHelperResponseEventArgs(HttpHelperResponse response)
        {
            Response = response;
        }
        
        public HttpHelperResponse Response { get; private set; }
    }
}
