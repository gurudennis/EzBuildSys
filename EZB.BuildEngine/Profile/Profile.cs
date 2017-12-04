using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace EZB.BuildEngine.Profile
{
    internal enum ItemType
    {
        Solution,
        Project,
        PowerShellScript,
        BatchScript,
        ShellCommand
    }

    internal class Item
    {
        public Item()
        {
        }

        public ItemType Type { get; internal set; }
        public string Path { get; internal set; }
    }

    internal class Stage
    {
        public Stage()
        {
        }

        public IList<Item> Items { get; internal set; }
    }

    internal class Profile
    {
        public Profile(string path)
        {
            string json = File.ReadAllText(path);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> profileRoot = serializer.Deserialize<Dictionary<string, object>>(json);

            string type = GetJSONValue<string>(profileRoot, "type", null);
            if (type != ExpectedType)
                throw new ApplicationException($"Profile type \"{type??string.Empty}\" does not match the expected type \"{ExpectedType}\"");

            Version = GetJSONValue<int>(profileRoot, "version", DefaultVersion);
            if (Version > MaxUnderstoodVersion)
                throw new ApplicationException($"Profile version {Version} is greater than {MaxUnderstoodVersion} that is supported by this executable");

            PreBuild = StageFromJSON(GetJSONValue<Dictionary<string, object>>(profileRoot, "prebuild", null));

            Build = StageFromJSON(GetJSONValue<Dictionary<string, object>>(profileRoot, "build", null));
            if (Build == null)
                throw new ApplicationException($"A valid build must have a main \"build\" stage");

            PostBuild = StageFromJSON(GetJSONValue<Dictionary<string, object>>(profileRoot, "postbuild", null));
        }

        public int Version { get; private set; }

        public Stage PreBuild { get; private set; }
        public Stage Build { get; private set; }
        public Stage PostBuild { get; private set; }

        private Stage StageFromJSON(Dictionary<string, object> stageRoot)
        {
            // ...

            return null;
        }

        private T GetJSONValue<T>(Dictionary<string, object> obj, string name, T defValue)
        {
            object value = null;
            if (!obj.TryGetValue(name, out value))
                return defValue;

            if (!(value is T))
                return defValue;

            return (T)value;
        }

        private static string ExpectedType = "EZB.BuildProfile";
        private static int DefaultVersion = 1;
        private static int MaxUnderstoodVersion = 1;
    }
}
