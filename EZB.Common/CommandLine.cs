using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace EZB.Common
{
    public class CommandLine
    {
        public CommandLine(string[] args)
        {
            _args = args;
        }

        public static string[] SplitArgs(string commandLine)
        {
            var argv = CommandLineToArgvW(commandLine, out int argc);
            if (argv == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();

            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }

        public static string JoinArgs(string[] args, int startIndex = 0)
        {
            if (args == null)
                return null;

            if (args.Length == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            for (int i = startIndex; i < args.Length; ++i)
            {
                if (i != 0)
                    sb.Append(" ");

                sb.Append("\"" + args[i] + "\"");
            }

            return sb.ToString();
        }

        public T GetValue<T>(string parameter, T defValue = default(T))
        {
            if (_args == null)
                return defValue;

            if (typeof(T) == typeof(bool))
                return (T)(object)_args.Contains(parameter);

            for (int i = 0; i < _args.Length; ++i)
            {
                if ((_args[i] == parameter) && (i < (_args.Length - 1)))
                    return (T)Convert.ChangeType(_args[i + 1], typeof(T));
            }

            return defValue;
        }

        public string GetParameter(int index, string defValue = null)
        {
            return (_args != null && _args.Length > index) ? _args[index] : defValue;
        }

        public int ParameterCount { get { return _args == null ? 0 : _args.Length; } }

        public bool IsEmpty { get { return _args == null || _args.Length == 0; } }

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        private string[] _args;
    }
}
