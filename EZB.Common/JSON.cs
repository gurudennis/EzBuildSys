using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZB.Common
{
    public static class JSON
    {
        public static T GetJSONValue<T>(Dictionary<string, object> obj, string name, T defValue)
        {
            object value = null;
            if (!obj.TryGetValue(name, out value))
                return defValue;

            if (!(value is T))
                return defValue;

            return (T)value;
        }
    }
}
