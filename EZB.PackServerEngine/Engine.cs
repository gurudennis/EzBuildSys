using System;

namespace EZB.PackServerEngine
{
    public class Engine
    {
        public Engine()
        {
        }

        public PackageManager CreatePackageManager(string root)
        {
            return new PackageManager(root);
        }

        public Server CreateServer(PackageManager packageManager, short port = Server.DefaultPort, string iface = Common.RESTServer.AnyInterface)
        {
            return new Server(packageManager, port, iface);
        }
    }
}
