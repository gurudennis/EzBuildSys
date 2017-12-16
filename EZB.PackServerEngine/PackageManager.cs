using System;
using System.Collections.Generic;
using System.IO;

namespace EZB.PackServerEngine
{
    public class PackageManager : IDisposable
    {
        internal PackageManager(string root)
        {
            _root = root ?? throw new ArgumentException("Package manager root must be specified");
            _guard = new object();
            _index = new PackageIndex(Path.Combine(_root, "Index"));
            _store = new PackageStore(Path.Combine(_root, "Packages"));
        }

        public void Dispose()
        {
            if (_index != null)
            {
                _index.Dispose();
                _index = null;
            }

            if (_store != null)
            {
                _store.Dispose();
                _store = null;
            }
        }

        public object Guard { get { return _guard; } }

        public List<PackEngine.PackageInfo> ListPackages(string name, Version version)
        {
            List<PackageIndex.Entry> entries = _index.ListEntries(name, version);
            if (entries == null)
                return null;

            List<PackEngine.PackageInfo> packages = new List<PackEngine.PackageInfo>();
            foreach (PackageIndex.Entry entry in entries)
            {
                if (entry.Info.IsValid())
                    packages.Add(entry.Info);
            }

            return packages;
        }

        public void AddPackage(string path)
        {
            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                AddPackage(stream);
        }

        public bool AddPackage(Stream stream)
        {
            lock (_guard)
            {
                string storeFileName = null;

                try
                {
                    PackEngine.PackageInfo info = _store.WritePackage(stream, out storeFileName);
                    if (info == null || !info.IsValid() || string.IsNullOrEmpty(storeFileName))
                        throw new ApplicationException("Failed to handle the package");

                    if (_index.GetEntry(info.Name, info.Version) != null)
                        return false;
                
                    _index.AddEntry(new PackageIndex.Entry { Info = info, StoreFileName = storeFileName });
                }
                catch
                {
                    if (!string.IsNullOrEmpty(storeFileName))
                        _store.DeletePackage(storeFileName);

                    throw;
                }
            }

            return true;
        }

        public void RemovePackage(string name, Version version)
        {
            lock (_guard)
            {
                string storeFileName = GetStoreFileName(name, version);

                try { _store.DeletePackage(storeFileName); } catch { }

                try { _index.RemoveEntry(name, version); } catch { }
            }
        }

        public void GetPackage(string name, Version version, string path)
        {
            using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                GetPackage(name, version, stream);
        }

        public void GetPackage(string name, Version version, Stream stream)
        {
            lock (_guard)
            {
                using (Stream sourceStream = GetPackage(name, version))
                    sourceStream.CopyTo(stream);
            }
        }

        public Stream GetPackage(string name, Version version)
        {
            lock (_guard)
            {
                return _store.ReadPackage(GetStoreFileName(name, version));
            }
        }

        public PackEngine.PackageInfo GetPackageInfo(string name, Version version)
        {
            PackageIndex.Entry index = _index.GetEntry(name, version);
            return (index == null || !index.IsValid) ? null : index.Info;
        }

        public void GenerateIndex()
        {
            // TODO: implement automatic index recovery
        }

        private string GetStoreFileName(string name, Version version)
        {
            try
            {
                PackageIndex.Entry entry = _index.GetEntry(name, version);
                if (entry != null && !string.IsNullOrEmpty(entry.StoreFileName))
                    return entry.StoreFileName;
            }
            catch { }

            return _store.MakeStoreFileName(name, version);
        }

        private string _root;
        private object _guard;
        private PackageIndex _index;
        private PackageStore _store;
    }
}
