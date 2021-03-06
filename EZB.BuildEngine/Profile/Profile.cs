﻿using System;
using System.Collections;
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

    internal enum StageType
    {
        PreBuild,
        Build,
        PostBuild,
        PostClean,
    }

    internal class Stage
    {
        public Stage()
        {
            Items = new List<Item>();
        }

        public StageType Type { get; internal set; }
        public IList<Item> Items { get; private set; }
    }

    internal class Profile
    {
        public Profile(string path)
        {
            string json = File.ReadAllText(path);

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> profileRoot = serializer.Deserialize<Dictionary<string, object>>(json);

            string type = Common.JSON.GetJSONValue<string>(profileRoot, "type", null);
            if (type != ExpectedType)
                throw new ApplicationException($"Profile type \"{type??string.Empty}\" does not match the expected type \"{ExpectedType}\"");

            Version = Common.JSON.GetJSONValue<int>(profileRoot, "version", DefaultVersion);
            if (Version > MaxUnderstoodVersion)
                throw new ApplicationException($"Profile version {Version} is greater than {MaxUnderstoodVersion} that is supported by this executable");

            Dictionary<string, object> profileObjRoot = Common.JSON.GetJSONValue<Dictionary<string, object>>(profileRoot, "profile", null);
            if (profileObjRoot == null)
                throw new ApplicationException($"A build must have a valid non-empty main \"profile\" section");

            PreBuild = StageFromJSON(Common.JSON.GetJSONValue<Dictionary<string, object>>(profileObjRoot, "prebuild", null), StageType.PreBuild);

            Build = StageFromJSON(Common.JSON.GetJSONValue<Dictionary<string, object>>(profileObjRoot, "build", null), StageType.Build);
            if (Build == null)
                throw new ApplicationException($"A build must have a valid non-empty main \"build\" stage");

            PostBuild = StageFromJSON(Common.JSON.GetJSONValue<Dictionary<string, object>>(profileObjRoot, "postbuild", null), StageType.PostBuild);

            PostClean = StageFromJSON(Common.JSON.GetJSONValue<Dictionary<string, object>>(profileObjRoot, "postclean", null), StageType.PostClean);
        }

        public int Version { get; private set; }

        public Stage PreBuild { get; private set; }
        public Stage Build { get; private set; }
        public Stage PostBuild { get; private set; }
        public Stage PostClean { get; private set; }

        private Stage StageFromJSON(Dictionary<string, object> stageRoot, StageType stageType)
        {
            if (stageRoot == null)
                return null;

            ArrayList itemsObj = Common.JSON.GetJSONValue<ArrayList>(stageRoot, "items", null);
            if (itemsObj == null || itemsObj.Count == 0)
                return null;

            Stage stage = new Stage();
            stage.Type = stageType;

            foreach (Dictionary<string, object> itemObj in itemsObj)
            {
                Item item = ItemFromJSON(itemObj);
                if (item == null)
                    throw new ApplicationException($"Invalid build item found");

                stage.Items.Add(item);
            }

            return stage;
        }

        private Item ItemFromJSON(Dictionary<string, object> itemRoot)
        {
            if (itemRoot == null)
                return null;

            Item item = new Item();

            item.Path = Common.JSON.GetJSONValue<string>(itemRoot, "path", null);
            if (string.IsNullOrEmpty(item.Path))
                throw new ApplicationException($"A build item must have a path");

            string type = Common.JSON.GetJSONValue<string>(itemRoot, "type", null);
            if (string.IsNullOrEmpty(type))
                item.Type = DeduceItemTypeFromPath(item.Path);
            else
                item.Type = (ItemType)Enum.Parse(typeof(ItemType), type);

            return item;
        }

        private ItemType DeduceItemTypeFromPath(string path)
        {
            string ext = Path.GetExtension(path);
            if (ext == ".sln")
                return ItemType.Solution;
            else if (ext == ".csproj" || ext == ".vcxproj" || ext == ".vbproj")
                return ItemType.Project;
            else if (ext == ".ps1")
                return ItemType.PowerShellScript;
            else if (ext == ".cmd" || ext == ".bat")
                return ItemType.BatchScript;

            return ItemType.ShellCommand;
        }

        private static string ExpectedType = "EZB.BuildProfile";
        private static int DefaultVersion = 1;
        private static int MaxUnderstoodVersion = 1;
    }
}
