using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace EZB.Buildp
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            return new Program().Run(args);
        }

        private Program()
        {
        }

        private int Run(string[] args)
        {
            Console.WriteLine($"== EzBuildSys command line build tool v.{Assembly.GetEntryAssembly().GetName().Version.ToString(3)} ==\r\n");

            try
            {
                Common.CommandLine cmdLine = new Common.CommandLine(args);
                RunInternal(cmdLine);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"*** Error: {ex.Message} ***\r\n");
                Console.Error.WriteLine($"Complete exception dump follows:\r\n{ex}");

#if DEBUG
                if (Debugger.IsAttached)
                    Debugger.Break();
#endif

                return -1;
            }

            return 0;
        }

        private void RunInternal(Common.CommandLine cmdLine)
        {
            if (cmdLine.GetValue<bool>("/help") || cmdLine.IsEmpty)
            {
                Usage();
                return;
            }

            string profilePath = cmdLine.GetValue<string>("/profile", cmdLine.GetParameter(0));
            if (string.IsNullOrEmpty(profilePath))
                throw new ApplicationException("No profile path specified");

            string dirPath = new FileInfo(profilePath).Directory.FullName;
            string prevDirPath = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(dirPath);

            try
            {
                string verb = cmdLine.GetValue<string>("/verb", "Build");
                if (string.IsNullOrEmpty(verb))
                    throw new ApplicationException("Invalid verb specified");

                BuildEngine.BuildAction buildAction = BuildEngine.BuildAction.Build;
                if (!Enum.TryParse(verb, true, out buildAction))
                    throw new ApplicationException($"Unrecognized verb \"{verb}\" specified");

                Console.WriteLine($"== {buildAction}ing \"{profilePath}\".\r\n");

                BuildEngine.Engine engine = new BuildEngine.Engine();

                BuildEngine.Build build = engine.CreateBuild(profilePath);
                build.Execute(buildAction);
            }
            finally
            {
                Directory.SetCurrentDirectory(prevDirPath);
            }

            Console.WriteLine("\r\n== Build completed successfully.");
        }

        private void Usage()
        {
            Console.WriteLine("Parameters:");
            Console.WriteLine("    /help            Show this information");
            Console.WriteLine("    /verb            Build (default), Rebuild or Clean");
            Console.WriteLine("    /profile         Path to the *.ezb profile. Can be positional (1st argument)");
            Console.WriteLine("\r\nExamples:");
            Console.WriteLine("    EZB.Build /verb Clean /profile MyProfile.ezb");
            Console.WriteLine("    EZB.Build MyProfile.ezb");
            Console.WriteLine();
        }
    }
}
