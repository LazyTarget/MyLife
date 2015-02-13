using System.Net;

namespace CalendarCore.Http
{
    public class HttpHelperRequest : IHttpHelperRequest
    {
        public HttpHelperRequest()
        {
            Headers = new WebHeaderCollection();
        }


        public string Url { get; set; }

        public string Method { get; set; }

        public object Data { get; set; }

        public WebHeaderCollection Headers { get; set; }

    }
}
