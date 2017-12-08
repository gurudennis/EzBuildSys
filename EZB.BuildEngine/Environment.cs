using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZB.BuildEngine
{
    internal class Environment
    {
        public Environment()
        {
        }

        public void Discover()
        {
            DiscoverMSBuild();
        }

        private void DiscoverMSBuild()
        {
            string[] pathOptions =
            {
                "%programfiles(x86)%\\Microsoft Visual Studio\\2017\\Community\\MSBuild\\15.0\\Bin\\MSBuild.exe"
            };

            foreach (string path in pathOptions)
            {
                string expandedPath = System.Environment.ExpandEnvironmentVariables(path);
                if (!File.Exists(expandedPath))
                    continue;

                MSBuildPath = expandedPath;
            }
        }

        public string MSBuildPath { get; private set; }
    }
}
