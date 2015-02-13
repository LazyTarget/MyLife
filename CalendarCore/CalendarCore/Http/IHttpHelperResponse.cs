using System.Net;

namespace CalendarCore.Http
{
    public interface IHttpHelperResponse
    {
        HttpWebRequest Request { get; }

        HttpWebResponse Response { get; }
    }

    public interface IHttpHelperResponse<out T> : IHttpHelperResponse
    {
        T Result { get; }

    }
}
