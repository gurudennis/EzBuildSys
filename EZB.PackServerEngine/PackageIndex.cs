using System;
using System.Collections.Generic;

namespace EZB.PackServerEngine
{
    internal class PackageIndex
    {
        internal class Entry
        {
            internal Entry()
            {
                Info = new PackEngine.PackageInfo();
            }

            public bool IsValid
            {
                get
                {
                    return Info != null && Info.IsValid() && !string.IsNullOrEmpty(StoreFileName);
                }
            }

            public PackEngine.PackageInfo Info { get; set; }
            public string StoreFileName { get; set; }
        }

        internal PackageIndex(string indexRoot)
        {
            _indexRoot = indexRoot ?? throw new ApplicationException("Package index root must be specified");
        }

        public void AddEntry(Entry entry)
        {
            // ...
        }

        public void RemoveEntry(string name, Version version)
        {
            // ...
        }

        public Entry GetEntry(string name, Version version)
        {
            // ...

            return null;
        }

        public List<Entry> ListEntries(string name, Version version)
        {
            // ...

            return null;
        }

        private string _indexRoot;
    }
}
