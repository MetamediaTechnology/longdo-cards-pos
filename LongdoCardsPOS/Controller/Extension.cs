using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LongdoCardsPOS.Controller
{
    static class Extension
    {
        // string

        public static string Md5(this string text)
        {
            string result;
            using(var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(text);
                var outputByte = md5.ComputeHash(inputBytes);
                result = string.Join("", outputByte.Select(b => b.ToString("X2")));
            }
            return result;
        }

        // object

        public static object[] ToArray(this object data)
        {
            return (object[])data;
        }

        public static IDictionary<string, object> ToDict(this object data)
        {
            return (IDictionary<string, object>)data;
        }

        public static string String(this IDictionary<string, object> data, string key)
        {
            return data[key]?.ToString();
        }

        public static IDictionary<string, object> Dict(this IDictionary<string, object> data, string key)
        {
            return data[key].ToDict();
        }
    }
}
