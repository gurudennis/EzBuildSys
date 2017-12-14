using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace EZB.PackClient
{
    class Program
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
            Console.WriteLine($"== EzBuildSys command line package client tool v.{Assembly.GetEntryAssembly().GetName().Version.ToString(3)} ==\r\n");

            int retcode = 0;

            Common.CommandLine cmdLine = null;

            try
            {
                cmdLine = new Common.CommandLine(args);
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

                retcode = -1;
            }

            if (cmdLine != null && cmdLine.GetValue("/interactive", false))
            {
                Console.Write("== Press any key to exit... ");
                Console.ReadKey();
            }

            return retcode;
        }

        private void RunInternal(Common.CommandLine cmdLine)
        {
            if (cmdLine.GetValue<bool>("/help") || cmdLine.IsEmpty)
            {
                Usage();
                return;
            }

            string prevDirPath = null;

            string rootPath = cmdLine.GetValue<string>("/rootPath");
            if (!string.IsNullOrEmpty(rootPath))
            {
                prevDirPath = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(rootPath);
            }

            try
            {
                PackEngine.Engine engine = new PackEngine.Engine();

                string verb = cmdLine.GetValue<string>("/verb", "Build");
                if (verb == "Pack")
                    DoPack(engine, cmdLine);
                else if (verb == "Unpack")
                    DoUnpack(engine, cmdLine);
                else
                    throw new ApplicationException("Invalid verb specified");
            }
            finally
            {
                if (!string.IsNullOrEmpty(prevDirPath))
                    Directory.SetCurrentDirectory(prevDirPath);
            }

            Console.WriteLine("\r\n== Package operation completed successfully.");
        }

        private void Usage()
        {
            Console.WriteLine("Parameters:");
            Console.WriteLine("    /help            Show this information");
            Console.WriteLine("    /verb            Pack, Unpack");
            Console.WriteLine("    /rootPath        Optional root directory");
            Console.WriteLine("    /pathIn          Input path");
            Console.WriteLine("    /pathOut         Output path");
            Console.WriteLine("    /name            Package name");
            Console.WriteLine("    /version         Package version");
            Console.WriteLine("    /interactive     Interactive mode (requires some input)");
            Console.WriteLine("\r\nExamples:");
            Console.WriteLine("    EZB.PackClient /verb Pack /pathIn c:\\temp\\MyPackageRoot");
            Console.WriteLine("                   /pathOut c:\\temp\\{PackageName}_{PackageVersion}.zip");
            Console.WriteLine("                   /name MyPackage /version 1.2.3.4");
            Console.WriteLine("    EZB.PackClient /verb Unpack /pathIn c:\\temp\\MyPackage.zip");
            Console.WriteLine("                   /pathOut c:\\temp\\{PackageName}_{PackageVersion}");
            Console.WriteLine();
        }

        private void DoPack(PackEngine.Engine engine, Common.CommandLine cmdLine)
        {
            string pathIn = cmdLine.GetValue<string>("/pathIn");
            if (string.IsNullOrEmpty(pathIn))
                throw new ApplicationException("Input path is required");

            string pathOut = cmdLine.GetValue<string>("/pathOut");
            if (string.IsNullOrEmpty(pathOut))
                throw new ApplicationException("Output path is required");

            string versionStr = cmdLine.GetValue<string>("/version", "1.0.0.0");
            Version version = null;
            if (!Version.TryParse(versionStr, out version))
                throw new ApplicationException($"Invalid version string \"{versionStr}\"");

            PackEngine.PackageInfo info = new PackEngine.PackageInfo();
            info.Name = cmdLine.GetValue<string>("/name", Path.GetFileNameWithoutExtension(pathOut));
            info.Version = version;

            pathIn = ReplacePackageInfoWildcards(pathIn, info);
            pathOut = ReplacePackageInfoWildcards(pathOut, info);

            Console.WriteLine($"== Creating package \"{info.Name}\".\r\n");

            PackEngine.PackageWriter writer = engine.CreatePackageWriter(pathOut, info);
            writer.AddFolder(pathIn, string.Empty);
            writer.Save();
        }

        private void DoUnpack(PackEngine.Engine engine, Common.CommandLine cmdLine)
        {
            string pathIn = cmdLine.GetValue<string>("/pathIn");
            if (string.IsNullOrEmpty(pathIn))
                throw new ApplicationException("Input path is required");

            string pathOut = cmdLine.GetValue<string>("/pathOut");
            if (string.IsNullOrEmpty(pathOut))
                throw new ApplicationException("Output path is required");

            PackEngine.PackageReader reader = engine.CreatePackageReader(pathIn);

            PackEngine.PackageInfo info = reader.GetInfo();
            if (info == null)
                throw new ApplicationException("The package metadata is invalid");

            pathOut = ReplacePackageInfoWildcards(pathOut, info);

            Console.WriteLine($"== Extracting package \"{info.Name}\".\r\n");

            reader.Extract(pathOut);
        }

        private string ReplacePackageInfoWildcards(string str, PackEngine.PackageInfo info)
        {
            return str.Replace("{PackageName}", info.Name).Replace("{PackageVersion}", info.Version.ToString());
        }
    }
}
