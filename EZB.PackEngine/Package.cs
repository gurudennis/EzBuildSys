﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web.Script.Serialization;

namespace EZB.PackEngine
{
    public class PackageInfo
    {
        public string Name { get; set; }
        public Version Version { get; set; }

        public bool IsValid() { return !string.IsNullOrEmpty(Name) && Version != null; }

        public static readonly string FileName = "EZB.PackageInfo.json";
    }

    public static class PackageInfoSerializer
    {
        public static Dictionary<string, object> Serialize(PackageInfo info)
        {
            if (info == null)
                return null;

            Dictionary<string, object> root = new Dictionary<string, object>();
            root["name"] = info.Name;
            root["version"] = info.Version.ToString(4);

            return root;
        }

        public static ArrayList Serialize(List<PackageInfo> infos)
        {
            if (infos == null)
                return null;

            ArrayList list = new ArrayList();
            foreach (PackageInfo info in infos)
                list.Add(Serialize(info));

            return list;
        }

        public static PackageInfo Deserialize(Dictionary<string, object> root)
        {
            if (root == null)
                return null;

            PackageInfo info = new PackageInfo();
            info.Name = (string)root["name"];
            info.Version = Version.Parse((string)root["version"]);

            return info;
        }

        public static List<PackageInfo> Deserialize(ArrayList list)
        {
            List<PackageInfo> infos = new List<PackageInfo>();
            foreach (Dictionary<string, object> root in list)
                infos.Add(Deserialize(root));

            return infos;
        }
    }

    public class PackageWriter
    {
        internal PackageWriter(string path, PackageInfo info)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Package path must be non-empty");

            if (!info.IsValid())
                throw new ArgumentException("Package information is invalid");

            _path = path;
            _info = info;

            _tmpFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tmpFolder);
        }

        public void AddFile(string path, string pathInPackage)
        {
            File.Copy(path, Path.Combine(_tmpFolder, pathInPackage), true);
        }

        public void AddFolder(string path, string pathInPackage, string[] exts = null, bool allowExts = false)
        {
            string[] entries = Directory.GetFileSystemEntries(path, "*", SearchOption.AllDirectories);
            foreach (string entry in entries)
            {
                if (exts != null)
                {
                    string ext = Path.GetExtension(entry);
                    if (exts.Contains(ext) != allowExts)
                        continue;
                }

                string targetPath = entry.Substring(path.Length + 1);
                if (Directory.Exists(entry))
                {
                    string fullTargetPath = Path.Combine(_tmpFolder, targetPath);
                    Directory.CreateDirectory(fullTargetPath);
                }
                else
                {
                    AddFile(entry, targetPath);
                }
            }
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(_tmpFolder))
                throw new NullReferenceException("No temporary directory found");

            SaveMetadata();

            Directory.CreateDirectory(Directory.GetParent(_path).FullName);

            ZipFile.CreateFromDirectory(_tmpFolder, _path);

            Directory.Delete(_tmpFolder, true);
        }

        private void SaveMetadata()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string json = serializer.Serialize(PackageInfoSerializer.Serialize(_info));

            File.WriteAllText(Path.Combine(_tmpFolder, PackageInfo.FileName), json);
        }

        private PackageInfo _info;
        private string _path;
        private string _tmpFolder;
    }

    public class PackageReader
    {
        internal PackageReader(string path)
        {
            if (!File.Exists(path))
                throw new ArgumentException("The package path is invalid");

            _path = path;
        }

        public PackageInfo GetInfo()
        {
            if (_info != null)
                return _info;

            try
            {
                string json = null;

                using (ZipArchive archive = new ZipArchive(new FileStream(_path, FileMode.Open), ZipArchiveMode.Read))
                {
                    ZipArchiveEntry metadataEntry = archive.Entries.Where((ZipArchiveEntry e) => e.FullName == PackageInfo.FileName).First();

                    using (Stream stream = metadataEntry.Open())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            json = reader.ReadToEnd();
                        }
                    }
                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> root = serializer.Deserialize<Dictionary<string, object>>(json);
                _info = PackageInfoSerializer.Deserialize(root);
            }
            catch (Exception)
            {
                return null;
            }

            return _info;
        }

        public void Extract(string path)
        {
            Directory.CreateDirectory(path);
            ZipFile.ExtractToDirectory(_path, path);
        }

        private string _path;
        private PackageInfo _info;
    }
}
