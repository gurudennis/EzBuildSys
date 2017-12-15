using System;
using System.Collections.Generic;
using System.Net;

namespace EZB.PackServerEngine
{
    public class Server : IDisposable
    {
        public const short DefaultPort = 8710;

        internal Server(short port, string iface)
        {
            _restServer = new Common.RESTServer(OnRequest, port, "packages", iface);
            _restServer.Start();
        }

        public void Dispose()
        {
            if (_restServer != null)
            {
                _restServer.Stop();
                _restServer.Dispose();
                _restServer = null;
            }
        }

        private void OnRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            // ...

            response.StatusCode = 400;
        }

        private Common.RESTServer _restServer;
    }
}
