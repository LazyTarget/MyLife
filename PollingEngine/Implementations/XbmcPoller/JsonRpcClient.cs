// Copied from https://jsonrpc2.codeplex.com/SourceControl/changeset/view/18174#AustinHarris.JsonRpc/AustinHarris.JsonRpc.Client/client.cs


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using AustinHarris.JsonRpc;
using Newtonsoft.Json.Linq;

namespace XbmcPoller
{
    public class JsonRpcClient
    {
        private static object idLock = new object();
        private static int id = 0;
        public Uri ServiceEndpoint = null;
        private ICredentials _credentials;

        public JsonRpcClient(Uri serviceEndpoint, ICredentials credentials)
        {
            ServiceEndpoint = serviceEndpoint;
            _credentials = credentials;
        }

        private static Stream CopyAndClose(Stream inputStream)
        {
            const int readSize = 256;
            byte[] buffer = new byte[readSize];
            MemoryStream ms = new MemoryStream();

            int count = inputStream.Read(buffer, 0, readSize);
            while (count > 0)
            {
                ms.Write(buffer, 0, count);
                count = inputStream.Read(buffer, 0, readSize);
            }
            ms.Position = 0;
            inputStream.Close();
            return ms;
        }

        public IObservable<JsonResponse<T>> Invoke<T>(string method, object arg, IScheduler scheduler)
        {
            var req = new AustinHarris.JsonRpc.JsonRequest()
            {
                Method = method,
                Params = new object[] { arg }
            };
            return Invoke<T>(req, scheduler);
        }

        public IObservable<JsonResponse<T>> Invoke<T>(string method, object[] args, IScheduler scheduler)
        {
            var req = new AustinHarris.JsonRpc.JsonRequest()
            {
                Method = method,
                Params = args
            };
            return Invoke<T>(req, scheduler);
        }
        
        public IObservable<JsonResponse<T>> Invoke<T>(JsonRequest jsonRpc, IScheduler scheduler)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(jsonRpc);
            return Invoke<T>(json, scheduler);
        }

        public IObservable<JsonResponse<T>> Invoke<T>(string json, IScheduler scheduler)
        {
            var res = Observable.Create<JsonResponse<T>>((obs) =>
                scheduler.Schedule(() =>
                {
                    WebRequest req = null;
                    var hasError = false;
                    try
                    {
                        int myId;
                        lock (idLock)
                        {
                            myId = ++id;
                        }
                        //jsonRpc.Id = myId.ToString();
                        req = HttpWebRequest.Create(new Uri(ServiceEndpoint, "?callid=" + myId.ToString()));
                        req.Credentials = _credentials;
                        req.Timeout = (int)TimeSpan.FromSeconds(15).TotalMilliseconds;
                        req.Method = "Post";
                        req.ContentType = "application/json-rpc";
                    }
                    catch (Exception ex)
                    {
                        hasError = true;
                        obs.OnError(ex);
                    }

                    if (hasError)
                        return;

                    var ar = req.BeginGetRequestStream(new AsyncCallback((iar) =>
                    {
                        HttpWebRequest request = null;
                        var hasError2 = false;
                        try
                        {
                            request = (HttpWebRequest)iar.AsyncState;
                            var stream = new StreamWriter(req.EndGetRequestStream(iar));
                            stream.Write(json);

                            stream.Close();
                        }
                        catch (Exception ex)
                        {
                            hasError2 = true;
                            obs.OnError(ex);
                        }

                        if (hasError2)
                            return;

                        var rar = req.BeginGetResponse(new AsyncCallback((riar) =>
                        {
                            JsonResponse<T> rjson = null;
                            string sstream = "";
                            try
                            {
                                var request1 = (HttpWebRequest)riar.AsyncState;
                                var resp = (HttpWebResponse)request1.EndGetResponse(riar);

                                using (var rstream = new StreamReader(CopyAndClose(resp.GetResponseStream())))
                                {
                                    sstream = rstream.ReadToEnd();
                                }

                                rjson = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonResponse<T>>(sstream);
                            }
                            catch (Exception ex)
                            {

                            }

                            if (rjson == null)
                            {
                                if (!string.IsNullOrEmpty(sstream))
                                {
                                    JObject jo = Newtonsoft.Json.JsonConvert.DeserializeObject(sstream) as JObject;
                                    obs.OnError(new Exception(jo["Error"].ToString()));
                                }
                                else
                                {
                                    obs.OnError(new Exception("Empty response"));
                                }
                            }

                            obs.OnNext(rjson);
                            obs.OnCompleted();
                        }), request);

                    }), req);
                }));

            return res;
        }

    }
}
