using LongdoCardsPOS.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace LongdoCardsPOS.Controller
{
    class Util
    {
        public static void Log(object data)
        {
            Console.WriteLine(DateTime.Now + " > " + data);
        }

        public static void CreateUuid()
        {
            if (!string.IsNullOrEmpty(Settings.Default.Uuid)) return;

            Task.Factory.StartNew(() =>
            {
                Log("Create UUID");
                var collection = new ManagementClass("win32_processor").GetInstances();
                foreach (var data in collection)
                {
                    Settings.Default.Uuid = data.Properties["processorID"].Value.ToString();
                    Settings.Default.Save();
                    return;
                }
            });
        }

        public static void Login(object data)
        {
            Settings.Default.Token = data.ToDict().String("token"); // TODO: Use ProtectedData Class
            Settings.Default.Save();
        }

        public static void Logout()
        {
            var uuid = Settings.Default.Uuid;
            Settings.Default.Reset();
            Settings.Default.Uuid = uuid;
            Settings.Default.Save();
        }

        public static DateTime DateTimeFromTimestamp(int timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp).ToLocalTime();
        }
    }
}
