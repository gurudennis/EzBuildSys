using System;
using System.Net;
using System.Threading;

namespace EZB.Common
{
    // Heavily based on https://codehosting.net/blog/BlogEngine/post/Simple-C-Web-Server
    public class RESTServer : IDisposable
    {
        public const string AnyInterface = "+";
        public const string LoopbackInterface = "localhost";

        public delegate void RequestDelegate(HttpListenerRequest request, HttpListenerResponse response);

        public RESTServer(RequestDelegate onRequest, short port, string uriPath = "", string iface = AnyInterface)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException("This system is not supported. Required: Windows XP SP2, Server 2003 or later.");

            _onRequest = onRequest ?? throw new ArgumentException("Request handler is mandatory");

            _listener = new HttpListener();

            string prefix = "http://" + iface + ":" + port + "/" + uriPath;
            if (!prefix.EndsWith("/"))
                prefix += "/";

            _listener.Prefixes.Add(prefix);
        }

        public void Start()
        {
            if (_listener.IsListening)
                return;

            _listener.Start();

            ThreadPool.QueueUserWorkItem((o) =>
            {
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                _onRequest(ctx.Request, ctx.Response);
                            }
                            catch
                            {
                                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            }
                            finally
                            {
                                ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch { }
            });
        }

        public void Stop()
        {
            if (_listener == null || !_listener.IsListening)
                return;

            _listener.Stop();
        }

        public void Dispose()
        {
            Stop();

            if (_listener != null)
            {
                _listener.Close();
                _listener = null;
            }
        }

        private HttpListener _listener;
        private RequestDelegate _onRequest;
    }
}
