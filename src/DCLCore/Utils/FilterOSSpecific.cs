using DCL.Common;
using System;
using System.Management;

namespace DCLCore.Utils
{
    public static class FilterOSSpecific
    {
        public static void GetWindowsVersion()
        {
            Logger.Info("DCL", "Get OS version START");
            var OSName = "";
            var BuildNumber = 0;
            try
            {
                using (var searcher = new ManagementObjectSearcher("root\\CIMV2", $"SELECT Caption,BuildNumber FROM Win32_OperatingSystem"))
                using (var query = searcher.Get())
                {
                    foreach (var item in query)
                    {
                        if (!(item is ManagementObject mo)) continue;
                        OSName = item.GetPropertyValue("Caption").ToString();
                        BuildNumber = Convert.ToInt32(item.GetPropertyValue("BuildNumber"));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("DCL", $"Get OS version error: {e.Message}");
            }

            SystemVersion.FullName = OSName;
            SystemVersion.BuildNumber = BuildNumber;

            //win 7 builds
            if (BuildNumber == 7600 || BuildNumber == 7601)
            {
                SystemVersion.OsVersion = 7;
            }
            else if (BuildNumber > 10000) //windows 10 builds
            {
                SystemVersion.OsVersion = 10;
            }

            Logger.Info("DCL", "Get OS version END");
        }
    }
}
