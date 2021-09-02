using DCL.Common;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace DCL.UUID
{
    public static class WindowsMacUtils
    {
        [DllImport("rpcrt4.dll", SetLastError = true)]
        private static extern int UuidCreateSequential(out System.Guid guid);

        private static string MACHexFallbackPath => Paths.RootPath("saved_mac.txt");

        private static bool IsValidMACHex(string macHex) => !string.IsNullOrEmpty(macHex) && macHex.Length == 12;

        public static (bool ok, string macHex) GetAndCacheMACHex()
        {
            // #1 read cached
            try
            {
                var lastSavedMACHex = File.ReadAllText(MACHexFallbackPath);
                if (IsValidMACHex(lastSavedMACHex))
                {
                    Logger.Info("DCL.UUID", $"Returning valid MAC from file '{lastSavedMACHex}'");
                    return (true, lastSavedMACHex);
                }
                else
                {
                    Logger.Warn("DCL.UUID", $"Read invalid MAC from file '{lastSavedMACHex}'");
                }
            }
            catch (Exception e)
            {
                Logger.Error("DCL.UUID", $"Error while reading saved MAC: {e.Message}");
            }

            // #2 nothing in file or invalid check; get from HW
            try
            {
                UuidCreateSequential(out var guid);
                var macHex = guid.ToString().Split('-').LastOrDefault();
                if (IsValidMACHex(macHex) && SaveMACHexToFile(macHex))
                {
                    Logger.Info("DCL.UUID", $"Got MAC and saved to file. Returning valid MAC '{macHex}'");
                    return (true, macHex);
                }
                else if (!IsValidMACHex(macHex))
                {
                    Logger.Warn("DCL.UUID", $"Got invalid MAC '{macHex}'.");
                }
            }
            catch (Exception e)
            {
                Logger.Error("DCL.UUID", $"Error while getting and or saving MAC: {e.Message}");
            }

            // #3 failure 
            return (false, "");
        }

        private static bool SaveMACHexToFile(string macHex)
        {
            bool saveSucess = true;
            // save
            try
            {
                File.WriteAllText(MACHexFallbackPath, macHex);
            }
            catch (Exception e)
            {
                saveSucess = false;
                Logger.Error("DCL.UUID", $"Error while saving MAC {e.Message}");
            }
            // log fallback to logs
            try
            {
                File.AppendAllText(Paths.RootPath("logs", "macHexSavedHistory.txt"), macHex + Environment.NewLine);
            }
            catch (Exception e)
            {
                Logger.Error("DCL.UUID", $"Error logging new MAC {e.Message}");
            }

            return saveSucess;
        }
    }
}
