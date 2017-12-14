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
            Console.WriteLine($"== EzBuildSys package server v.{Assembly.GetEntryAssembly().GetName().Version.ToString(3)} ==\r\n");

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
            
            return retcode;
        }

        private void RunInternal(Common.CommandLine cmdLine)
        {
            if (cmdLine.GetValue<bool>("/help") || cmdLine.IsEmpty)
            {
                Usage();
                return;
            }

            string root = cmdLine.GetValue<string>("/root", cmdLine.GetParameter(0));
            if (string.IsNullOrEmpty(root))
                throw new ApplicationException("No root path specified");

            string prevDirPath = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(root);

            try
            {
                // TODO: implement
            }
            finally
            {
                Directory.SetCurrentDirectory(prevDirPath);
            }

            Console.WriteLine("\r\n== Server stopping gracefully.");
        }

        private void Usage()
        {
            Console.WriteLine("Parameters:");
            Console.WriteLine("    /help            Show this information");
            Console.WriteLine("    /root            Path to the server storage root. Can be positional (1st argument)");
            Console.WriteLine("\r\nExamples:");
            Console.WriteLine("    EZB.PackServer /root c:\\temp");
            Console.WriteLine();
        }
    }
}
