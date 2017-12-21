using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Web.Script.Serialization;

namespace EZB.PackEngine
{
    public class PackageManager
    {
        public const string Latest = "latest";

        internal PackageManager(string serverURI)
        {
            _client = new WebClient();
            _client.BaseAddress = serverURI + (serverURI.EndsWith("/") ? string.Empty : "/");
        }

        public List<PackageInfo> ListPackages(string name = null, string version = null, int maxResults = -1)
        {
            string separator = null;
            string query = "packages/list" + PackNameAndVersionQuery(name, version, ref separator);
            query += $"{separator}limit={maxResults}";

            string json = _client.DownloadString(_client.BaseAddress + query);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> root = serializer.Deserialize<Dictionary<string, object>>(json);

            return PackageInfoSerializer.Deserialize(Common.JSON.GetJSONValue<ArrayList>(root, "packages", null));
        }

        public void GetPackage(string name, string version, string path)
        {
            // ...
        }

        public void AddPackage(string path)
        {
            // ...
        }

        public void RemovePackage(string name, Version version)
        {
            // ...
        }

        private string PackNameAndVersionQuery(string name, string version, ref string separator)
        {
            if (string.IsNullOrEmpty(separator))
                separator = "?";

            string query = string.Empty;
            if (!string.IsNullOrEmpty(name))
            {
                query += $"{separator}name={WebUtility.UrlEncode(name)}";
                separator = "&";
            }
            if (version != null)
            {
                query += $"{separator}version={WebUtility.UrlEncode(version)}";
                separator = "&";
            }

            return query;
        }

        private WebClient _client;
    }
}
