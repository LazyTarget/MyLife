using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MyLife.Core
{
    public class JsonHttpHelper : HttpHelperBase
    {
        private JsonSerializerSettings Settings { get; set; }

        public JsonHttpHelper()
        {
            Settings = new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.DateTime,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            };
            //Settings.Converters.Add(new JavaScriptDateTimeConverter());
            //Settings.Converters.Add(new TickDateTimeConverter());
            Settings.Converters.Add(new IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffK",
                DateTimeStyles = DateTimeStyles.RoundtripKind | DateTimeStyles.AssumeUniversal
            });
        }


        protected override async Task<HttpWebRequest> BuildRequest(IHttpHelperRequest request)
        {
            var httpWebRequest = await base.BuildRequest(request);
            httpWebRequest.Accept = null;
            if (request.ContentType == null)
                httpWebRequest.ContentType = "application/json; charset=UTF-8";
            httpWebRequest.Headers["X-Accept"] = "application/json";
            return httpWebRequest;
        }


        protected override void SetRequestPayload(IHttpHelperRequest request, Stream requestStream)
        {
            if (request.Data == null)
                return;

            var json = JsonConvert.SerializeObject(request.Data, Settings);
            var byteArray = Encoding.GetBytes(json);
            requestStream.Write(byteArray, 0, byteArray.Length);
        }


        protected override object ReadResponseData(IHttpHelperRequest request, Stream responseStream)
        {
            var responseData = base.ReadResponseData(request, responseStream);
            return responseData;
        }


        protected override T Deserialize<T>(object data)
        {
            T result;
            try
            {
                if (data is string)
                {
                    if (typeof (T) == typeof (string))
                        result = (T) data;
                    else
                    {
                        var json = (string)data;
                        result = JsonConvert.DeserializeObject<T>(json, Settings);
                    }
                }
                else
                {
                    var json = JsonConvert.SerializeObject(data, Settings);
                    result = JsonConvert.DeserializeObject<T>(json, Settings);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }


    }
}
