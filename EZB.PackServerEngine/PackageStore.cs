using System;
using System.IO;

namespace EZB.PackServerEngine
{
    internal class PackageStore : IDisposable
    {
        internal PackageStore(string storeRoot)
        {
            _storeRoot = storeRoot ?? throw new ApplicationException("Package store root must be specified");

            string tmpDirPath = Path.Combine(_storeRoot, TempDirName);
            if (Directory.Exists(tmpDirPath))
                Directory.Delete(tmpDirPath, true);
        }

        public void Dispose()
        {
        }

        public string MakeStoreFileName(string name, Version version)
        {
            return Path.Combine(name, version.ToString(4), PackEngine.Engine.MakePackageFileName(name, version));
        }

        public void ReadPackage(string storeFileName, Stream stream)
        {
            ReadPackage(storeFileName).CopyTo(stream);
        }

        public Stream ReadPackage(string storeFileName)
        {
            return GetPackageStream(storeFileName, false);
        }

        public PackEngine.PackageInfo WritePackage(Stream stream, out string storeFileName)
        {
            string tempStoreName = Path.Combine(TempDirName, Guid.NewGuid().ToString());

            using (Stream tempStream = WritePackage(tempStoreName))
                stream.CopyTo(tempStream);

            string tempStorePath = Path.Combine(_storeRoot, tempStoreName);
            string finalStorePath = null;

            try
            {
                PackEngine.Engine packEngine = new PackEngine.Engine();
                PackEngine.PackageReader reader = packEngine.CreatePackageReader(tempStorePath);
                PackEngine.PackageInfo info = reader.GetInfo();
                if (info == null || !info.IsValid())
                    throw new ApplicationException("Invalid package");

                storeFileName = MakeStoreFileName(info.Name, info.Version);
                finalStorePath = Path.Combine(_storeRoot, storeFileName);

                if (File.Exists(finalStorePath))
                    File.Delete(finalStorePath);

                (new FileInfo(finalStorePath)).Directory.Create();

                File.Move(tempStorePath, finalStorePath);

                return info;
            }
            catch
            {
                if (finalStorePath != null && File.Exists(finalStorePath))
                    File.Delete(finalStorePath);

                storeFileName = null;

                return null;
            }
            finally
            {
                if (File.Exists(tempStorePath))
                    File.Delete(tempStorePath);
            }
        }

        public Stream WritePackage(string storeFileName)
        {
            return GetPackageStream(storeFileName, true);
        }

        public void DeletePackage(string storeFileName)
        {
            string storePath = Path.Combine(_storeRoot, storeFileName);

            if (File.Exists(storePath))
                File.Delete(storePath);

            int index = storePath.IndexOf(Path.DirectorySeparatorChar);
            if (index >= 0)
                (new FileInfo(storePath)).Directory.Delete(false);
        }

        private Stream GetPackageStream(string storeFileName, bool write)
        {
            try
            {
                string fullPath = Path.Combine(_storeRoot, storeFileName);

                if (write)
                    (new FileInfo(fullPath)).Directory.Create();

                return new FileStream(
                    Path.Combine(_storeRoot, storeFileName),
                    write ? FileMode.Create : FileMode.Open,
                    write ? FileAccess.Write : FileAccess.Read);
            }
            catch
            {
                return null;
            }
        }

        private const string TempDirName = "Temporary";

        private string _storeRoot;
    }
}
