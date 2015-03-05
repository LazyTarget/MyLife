using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyLife.Core
{
    public abstract class HttpHelperBase : IHttpHelper, IHttpHelperAsync
    {
        protected HttpHelperBase()
        {
            Timeout = 100000;
            Encoding = Encoding.UTF8;
        }


        /// <summary>
        /// Timeout in milliseconds, default is 100 secs
        /// </summary>
        public int Timeout { get; set; }


        public Encoding Encoding { get; set; }


        public event EventHandler<HttpHelperResponseEventArgs> ResponseReceived;



        private async Task<IHttpHelperResponse> Process(IHttpHelperRequest request)
        {
            var httpWebRequest = await BuildRequest(request);
            //var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            var httpWebResponse = (HttpWebResponse) await httpWebRequest.GetResponseAsync();
            var response = new HttpHelperResponse(httpWebRequest, httpWebResponse);
            if (ResponseReceived != null)
                ResponseReceived(this, new HttpHelperResponseEventArgs(response));
            return response;
        }
        
        private async Task<IHttpHelperResponse<T>> Process<T>(IHttpHelperRequest request)
        {
            var httpWebRequest = await BuildRequest(request);
            //var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            var httpWebResponse = (HttpWebResponse) await httpWebRequest.GetResponseAsync();
            var responseData = GetResponseData(request, httpWebResponse);
            var result = Deserialize<T>(responseData);
            var response = new HttpHelperResponse<T>(result, httpWebRequest, httpWebResponse);
            if (ResponseReceived != null)
                ResponseReceived(this, new HttpHelperResponseEventArgs(response));
            return response;
        }


        protected virtual async Task<HttpWebRequest> BuildRequest(IHttpHelperRequest request)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(request.Url);
            httpWebRequest.Method = request.Method ?? "GET";
            httpWebRequest.ContentType = request.ContentType;
            //httpWebRequest.Timeout = Timeout;
            if (request.Headers != null && request.Headers.Count > 0)
            {
                foreach (var header in request.Headers.AllKeys)
                {
                    httpWebRequest.Headers[header] = request.Headers[header];
                }
            }


            // Set payload
            if (!httpWebRequest.Method.Equals("GET"))
            {
                using (var stream = await httpWebRequest.GetRequestStreamAsync())
                {
                    SetRequestPayload(request, stream);
                    //stream.Close();
                }
            }

            return httpWebRequest;
        }


        protected virtual void SetRequestPayload(IHttpHelperRequest request, Stream requestStream)
        {
            if (request.Data == null)
                return;

            var dataString = request.Data.ToString();
            var byteArray = Encoding.GetBytes(dataString);
            requestStream.Write(byteArray, 0, byteArray.Length);

            //using (var sw = new StreamWriter(requestStream, Encoding))
            //{
            //    sw.WriteLine(request.Data);

            //    sw.Close();
            //}
        }



        private object GetResponseData(IHttpHelperRequest request, HttpWebResponse httpWebResponse)
        {
            object result;
            using (var stream = httpWebResponse.GetResponseStream())
            {
                result = ReadResponseData(request, stream);
                //stream.Close();
            }
            return result;
        }


        protected virtual object ReadResponseData(IHttpHelperRequest request, Stream responseStream)
        {
            string resultString;
            using (var sr = new StreamReader(responseStream, Encoding))
            {
                resultString = sr.ReadToEnd();
                //sr.Close();
            }
            return resultString;
        }



        protected virtual T Deserialize<T>(object data)
        {
            var result = (T)data;
            return result;
        }




        public async Task<IHttpHelperResponse> SendAsync(IHttpHelperRequest request)
        {
            var response = await Process(request);
            return response;
        }

        public async Task<IHttpHelperResponse<T>> SendAsync<T>(IHttpHelperRequest request)
        {
            var response = await Process<T>(request);
            return response;
        }


        public IHttpHelperResponse Send(IHttpHelperRequest request)
        {
            //var response = Process(request);
            //return response;
            throw new NotSupportedException();
        }

        public IHttpHelperResponse<T> Send<T>(IHttpHelperRequest request)
        {
            //var response = Process<T>(request);
            //return response;
            throw new NotSupportedException();
        }

    }
}
