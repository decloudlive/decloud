using DCL.Common;
using System;
using System.Diagnostics;

namespace DCLCore.Utils
{
    public static class AmdModeSwitcher
    {
        public static void SwitchAmdComputeMode()
        {
            try
            {
                var fileName = Paths.AppRootPath("AmdComputeModeSwitcher.exe");
                var startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true
                };
                startInfo.WindowStyle = ProcessWindowStyle.Hidden; // used for hidden window
                using (var amdModeSwitcher = new Process { StartInfo = startInfo })
                {
                    amdModeSwitcher.Start();
                    amdModeSwitcher?.WaitForExit(10 * 1000);
                    if (amdModeSwitcher?.ExitCode != 0)
                    {
                        Logger.Info("Decloud", "amdModeSwitcher returned error code: " + amdModeSwitcher.ExitCode);
                    }
                    else
                    {
                        Logger.Info("Decloud", "amdModeSwitcher all OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Decloud", $"SwitchAmdComputeMode error: {ex.Message}");
            }
        }
    }
}
