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
#if DEBUG
                return version + " DEV";
#else
                return version;
#endif
            }
        }

        public static string Server
        {
            get
            {
#if DEBUG
                return "https://card-test.longdo.com/";
#else
                return "https://card.longdo.com/"
#endif
            }
        }
    }
}
