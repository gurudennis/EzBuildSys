using System;
using System.Diagnostics;

namespace EZB.BuildEngine.Actions
{
    class ProcessAction : IAction
    {
        public ProcessAction(string path, string cmdLine, bool isConsole)
        {
            _path = path;
            _cmdLine = cmdLine;
            _isConsole = isConsole;

            if (string.IsNullOrEmpty(path))
                throw new ApplicationException("Cannot execute an empty path");
        }

        public ProcessAction(string cmdLine, bool isConsole)
        {
            string[] args = Common.CommandLine.SplitArgs(cmdLine);

            _path = (args == null || args.Length == 0) ? null : args[0];
            _cmdLine = Common.CommandLine.JoinArgs(args, 1);
            _isConsole = isConsole;

            if (string.IsNullOrEmpty(_path))
                throw new ApplicationException("Cannot execute an empty path");
        }

        public void Execute()
        {
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = _path,
                Arguments = _cmdLine,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = _isConsole,
                RedirectStandardError = _isConsole
            };

            using (Process process = Process.Start(psi))
            {
                while (!process.WaitForExit(100))
                    FunnelOutput(process);

                FunnelOutput(process);

                if (process.ExitCode != 0)
                    throw new ApplicationException($"Command \"{_path} {_cmdLine}\" failed with exit code {process.ExitCode}");
            }
        }

        private void FunnelOutput(Process process)
        {
            if (!_isConsole)
                return;

            while (process.StandardOutput.Peek() > 0)
                Console.Write(process.StandardOutput.ReadToEnd());

            while (process.StandardError.Peek() > 0)
                Console.Write(process.StandardError.ReadToEnd());
        }

        string _path;
        string _cmdLine;
        bool _isConsole;
    }
}
