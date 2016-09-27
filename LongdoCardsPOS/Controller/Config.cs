using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace LongdoCardsPOS
{
    class Config
    {
        public static string Version
        {
            get
            {
                var version = FileVersionInfo.GetVersionInfo(Application.ResourceAssembly.Location).ProductVersion;
                return version;
            }
        }

        public static string Server
        {
            get
            {
                return "https://card.longdo.com/";
            }
        }
    }
}
