using System.Net;

namespace MyLife.Core
{
    public class HttpHelperRequest : IHttpHelperRequest
    {
        public HttpHelperRequest()
        {
            Headers = new WebHeaderCollection();
        }


        public string Url { get; set; }

        public string Method { get; set; }
        
        public string ContentType { get; set; }

        public object Data { get; set; }

        public WebHeaderCollection Headers { get; set; }

    }
}
