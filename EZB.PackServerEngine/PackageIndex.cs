using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace EZB.PackServerEngine
{
    internal class PackageIndex : IDisposable
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

            bool mustInitialize = false;
            string indexFilePath = GetIndexFilePath();
            if (!File.Exists(indexFilePath))
            {
                (new FileInfo(indexFilePath)).Directory.Create();
                SQLiteConnection.CreateFile(indexFilePath);
                mustInitialize = true;
            }

            _db = new SQLiteConnection($"Data Source=\"{indexFilePath}\";Version=3;");
            _db.Open();

            if (mustInitialize)
                InitializeDatabase();
        }

        public void Dispose()
        {
            if (_db != null)
            {
                _db.Close();
                _db.Dispose();
                _db = null;
            }
        }

        public void AddEntry(Entry entry)
        {
            SQLiteCommand command = new SQLiteCommand("INSERT INTO packages (name, version, storeName) VALUES (@n, @v, @s);", _db);

            command.Parameters.Add("@n", System.Data.DbType.String);
            command.Parameters["@n"].Value = entry.Info.Name;

            command.Parameters.Add("@v", System.Data.DbType.String);
            command.Parameters["@v"].Value = entry.Info.Version;

            command.Parameters.Add("@s", System.Data.DbType.String);
            command.Parameters["@s"].Value = entry.StoreFileName;

            if (command.ExecuteNonQuery() != 1)
                throw new ApplicationException("Failed to insert into the index");
        }

        public void RemoveEntry(string name, Version version)
        {
            SQLiteCommand command = new SQLiteCommand("DELETE * FROM packages WHERE name=@n AND version=@v;", _db);

            command.Parameters.Add("@n", System.Data.DbType.String);
            command.Parameters["@n"].Value = name;

            command.Parameters.Add("@v", System.Data.DbType.String);
            command.Parameters["@v"].Value = version;

            if (command.ExecuteNonQuery() != 1)
                throw new ApplicationException("Failed to delete from the index");
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

        private void InitializeDatabase()
        {
            (new SQLiteCommand("CREATE TABLE packages (name VARCHAR(128) NOT NULL, version VARCHAR(16) NOT NULL, storeName VARCHAR(256) NOT NULL);", _db)).ExecuteNonQuery();
            (new SQLiteCommand("CREATE INDEX packages_nameVersionIndex ON packages (name, version);", _db)).ExecuteNonQuery();
        }

        private string GetIndexFilePath()
        {
            return Path.Combine(_indexRoot, IndexFileName);
        }

        private const string IndexFileName = "EZB.PackageIndex.sqlite";

        private string _indexRoot;
        private SQLiteConnection _db;
    }
}
