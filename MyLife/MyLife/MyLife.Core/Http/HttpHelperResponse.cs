using System.Net;

namespace MyLife.Core
{
    public class HttpHelperResponse : IHttpHelperResponse
    {
        public HttpHelperResponse(HttpWebRequest request, HttpWebResponse response)
        {
            Request = request;
            Response = response;
        }


        public HttpWebRequest Request { get; private set; }

        public HttpWebResponse Response { get; private set; }
    }


    public class HttpHelperResponse<T> : HttpHelperResponse, IHttpHelperResponse<T>
    {
        public HttpHelperResponse(T result, HttpWebRequest request, HttpWebResponse response)
            : base(request, response)
        {
            Result = result;
        }


        public T Result { get; private set; }

    }
}
