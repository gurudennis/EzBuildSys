using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace EZB.PackServerEngine
{
    internal class PackageIndex : IDisposable
    {
        public const string Latest = "latest";

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

            command.Parameters.Add("@v", System.Data.DbType.UInt64);
            command.Parameters["@v"].Value = CompressVersion(entry.Info.Version);

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

            command.Parameters.Add("@v", System.Data.DbType.UInt64);
            command.Parameters["@v"].Value = CompressVersion(version);

            if (command.ExecuteNonQuery() != 1)
                throw new ApplicationException("Failed to delete from the index");
        }

        public Entry GetEntry(string name, string version)
        {
            if (version == Latest)
                version = null;

            string versionQuery = string.IsNullOrEmpty(version) ? string.Empty : " AND version=@v";

            SQLiteCommand command = new SQLiteCommand($"SELECT * FROM packages WHERE name=@n{versionQuery} ORDER BY version DESC LIMIT 1;", _db);

            command.Parameters.Add("@n", System.Data.DbType.String);
            command.Parameters["@n"].Value = name;

            if (!string.IsNullOrEmpty(versionQuery))
            {
                command.Parameters.Add("@v", System.Data.DbType.UInt64);
                command.Parameters["@v"].Value = CompressVersion(Version.Parse(version));
            }

            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Entry entry = new Entry();
                entry.Info.Name = (string)reader["name"];
                entry.Info.Version = DecompressVersion((ulong)(long)reader["version"]);
                entry.StoreFileName = (string)reader["storeName"];
                return entry;
            }

            return null;
        }

        public List<Entry> ListEntries(string name, string version = null, int maxResults = -1)
        {
            if (version == Latest)
                maxResults = 1;
            
            string limit = maxResults > 0 ? $" LIMIT {maxResults}" : string.Empty;

            string sql = null;
            if (name == null)
                sql = $"SELECT * FROM packages ORDER BY version DESC{limit};";
            else if (version == null || version == Latest)
                sql = $"SELECT * FROM packages WHERE name LIKE @n ORDER BY version DESC{limit};";
            else
                sql = $"SELECT * FROM packages WHERE name LIKE @n AND version=@v ORDER BY version DESC{limit};";

            SQLiteCommand command = new SQLiteCommand(sql, _db);

            if (name != null)
            {
                command.Parameters.Add("@n", System.Data.DbType.String);
                command.Parameters["@n"].Value = "%" + name + "%";
            }

            if (version != null && version != Latest)
            {
                command.Parameters.Add("@v", System.Data.DbType.UInt64);
                command.Parameters["@v"].Value = CompressVersion(Version.Parse(version));
            }

            List<Entry> entries = new List<Entry>();

            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Entry entry = new Entry();
                entry.Info.Name = (string)reader["name"];
                entry.Info.Version = DecompressVersion((ulong)(long)reader["version"]);
                entry.StoreFileName = (string)reader["storeName"];

                if (entry.IsValid)
                    entries.Add(entry);
            }

            return entries;
        }

        private void InitializeDatabase()
        {
            (new SQLiteCommand("CREATE TABLE packages (name VARCHAR(128) NOT NULL, version INTEGER NOT NULL, storeName VARCHAR(256) NOT NULL);", _db)).ExecuteNonQuery();
            (new SQLiteCommand("CREATE UNIQUE INDEX packages_nameVersionIndex ON packages (name, version);", _db)).ExecuteNonQuery();
        }

        private string GetIndexFilePath()
        {
            return Path.Combine(_indexRoot, IndexFileName);
        }

        private ulong CompressVersion(Version version)
        {
            if (version.Major > ushort.MaxValue || version.Minor > ushort.MaxValue ||
                version.Build > ushort.MaxValue || version.Revision > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException("Version component value is too high");
            }

            ulong v1 = ((ulong)version.Major) << 48;
            ulong v2 = ((ulong)version.Minor) << 32;
            ulong v3 = ((ulong)version.Build) << 16;
            ulong v4 = ((ulong)version.Revision);

            return v1 | v2 | v3 | v4;
        }

        private Version DecompressVersion(ulong compressed)
        {
            ulong v1 = ((compressed & 0xFFFF000000000000) >> 48);
            ulong v2 = ((compressed & 0x0000FFFF00000000) >> 32);
            ulong v3 = ((compressed & 0x00000000FFFF0000) >> 16);
            ulong v4 = (compressed & 0x000000000000FFFF);

            if (v1 == 0 && v2 == 0 && v3 == 0 && v4 == 0)
                return null;

            return new Version((int)v1, (int)v2, (int)v3, (int)v4);
        }

        private const string IndexFileName = "EZB.PackageIndex.sqlite";

        private string _indexRoot;
        private SQLiteConnection _db;
    }
}
