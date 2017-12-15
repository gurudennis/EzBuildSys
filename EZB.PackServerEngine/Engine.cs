using System;

namespace EZB.PackServerEngine
{
    public class Engine
    {
        public Engine()
        {
        }

        public Server CreateServer(short port = Server.DefaultPort, string iface = Common.RESTServer.AnyInterface)
        {
            return new Server(port, iface);
        }
    }
}
