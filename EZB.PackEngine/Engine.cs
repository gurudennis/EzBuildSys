using System;

namespace EZB.PackEngine
{
    public class Engine
    {
        public static string MakePackageFileName(string name, Version version)
        {
            return name + "_" + version.ToString(4) + ".zip";
        }

        public PackageWriter CreatePackageWriter(string path, PackageInfo info)
        {
            return new PackageWriter(path, info);
        }

        public PackageReader CreatePackageReader(string path)
        {
            return new PackageReader(path);
        }
    }
}
