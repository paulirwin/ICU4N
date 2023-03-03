﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ICU4N
{
    internal static class PlatformDetection
    {
        private static readonly bool isWindows = LoadIsWindows();
        private static readonly bool isLinux = LoadIsLinux();

        private static bool LoadIsWindows()
        {
#if FEATURE_RUNTIMEINFORMATION_ISOSPLATFORM
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
            PlatformID p = Environment.OSVersion.Platform;
            return p == PlatformID.Win32NT || p == PlatformID.Win32S || p == PlatformID.Win32Windows || p == PlatformID.WinCE;
#endif
        }

        private static bool LoadIsLinux()
        {
#if FEATURE_RUNTIMEINFORMATION_ISOSPLATFORM
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
#else
            int p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
#endif
        }

        public static string BaseDirectory
        {
            get
            {
#if FEATURE_APPCONTEXT_BASEDIRECTORY
                return AppContext.BaseDirectory;
#else
                return AppDomain.CurrentDomain.BaseDirectory;
#endif
            }
        }

        public static bool IsWindows => isWindows;

        public static bool IsLinux => isLinux;

    }
}
