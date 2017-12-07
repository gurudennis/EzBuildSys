using System;
using System.Linq;

namespace EZB.Common
{
    public class CommandLine
    {
        public CommandLine(string[] args)
        {
            _args = args;
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

        private string[] _args;
    }
}
