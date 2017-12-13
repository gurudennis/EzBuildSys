using System;

namespace EZB.PackEngine
{
    public class Engine
    {
        public PackageWriter CreatePackageWriter(string path, PackageInfo info)
        {
            return new PackageWriter(path, info);
        }
    }
}
