using System.Net;

namespace MyLife.Core
{
    public interface IHttpHelperRequest
    {
        string Url { get; set; }

        string Method { get; set; }

        string ContentType { get; set; }

        object Data { get; set; }

        WebHeaderCollection Headers { get; set; }

    }
}
