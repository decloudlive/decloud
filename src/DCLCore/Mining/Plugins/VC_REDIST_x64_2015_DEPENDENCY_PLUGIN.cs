using Microsoft.Win32;
using DCL.Common;
using DCL.Common.Algorithm;
using DCL.Common.Device;
using DCL.DecloudPlugin;
using DCL.DecloudPluginToolkitV1;
using DCL.DecloudPluginToolkitV1.Interfaces;
using DCLCore.Configs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DCLCore.Mining.Plugins
{
    // ALL CAPS
    // This isn't really a plugin it just a hack to piggyback on the Decloud plugins downloader and file checker
    class VC_REDIST_x64_2015_2019_DEPENDENCY_PLUGIN : IDecloudPlugin, IPluginDependency, IBinaryPackageMissingFilesChecker, IDecloudBinsSource
    {
        public static VC_REDIST_x64_2015_2019_DEPENDENCY_PLUGIN Instance { get; } = new VC_REDIST_x64_2015_2019_DEPENDENCY_PLUGIN();
        VC_REDIST_x64_2015_2019_DEPENDENCY_PLUGIN() { }
        public string PluginUUID => "VC_REDIST_x64_2015_2019";

        public Version Version => new Version(1, 0);
        public string Name => "VC_REDIST_x64_2015_2019";

        public string Author => "info@Decloud.com";

        public Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            // return empty
            return new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();
        }

        public bool IsPluginDependency { get; } = true;

        #region IDecloudPlugin stubs
        public IDecloud CreateDecloud()
        {
            return null;
        }

        public bool CanGroup(MiningPair a, MiningPair b)
        {
            return false;
        }
        #endregion IDecloudPlugin stubs


        private bool IsVcRedistInstalled()
        {

            // x64 - 14.23.27820
            const int minMajor = 14;
            const int minMinor = 23;
            try
            {
                using (var vcredist = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\x64"))
                {
                    var major = Int32.Parse(vcredist.GetValue("Major")?.ToString());
                    var minor = Int32.Parse(vcredist.GetValue("Minor")?.ToString());
                    //var build = vcredist.GetValue("Bld");
                    if (major < minMajor) return false;
                    if (minor < minMinor) return false;
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.Error(Name, $"IsVcRedistInstalled {e}");
            }
            return false;
        }

        public string VcRedistBinPath()
        {
            var binPath = Paths.DecloudPluginsPath(PluginUUID, "bins", $"{Version.Major}.{Version.Minor}", "VC_redist.x64_2015_2019.exe");
            return binPath;
        }

        public IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles("", new List<string> { VcRedistBinPath() });
        }

        public void InstallVcRedist()
        {
            if (IsVcRedistInstalled())
            {
                Logger.Info("VC_REDIST_x64_2015_DEPENDENCY_PLUGIN", $"Skipping installation minimum version newer already installed");
                return;
            }
            if (MiscSettings.Instance.DisableVisualCRedistributableCheck)
            {
                Logger.Info("VC_REDIST_x64_2015_DEPENDENCY_PLUGIN", $"Skipping installation MiscSettings.Instance.DisableVisualCRedistributableCheck=true");
                return;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = VcRedistBinPath(),
                    Arguments = "/install /quiet /norestart",
                    UseShellExecute = false,
                    RedirectStandardError = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = false
                };
                using (var cudaDevicesDetection = new Process { StartInfo = startInfo })
                {
                    cudaDevicesDetection.Start();
                }
            }
            catch (Exception e)
            {
                Logger.Error("VC_REDIST_x64_2015_DEPENDENCY_PLUGIN", $"InstallVcRedist error: {e.Message}");
            }
        }

        IEnumerable<string> IDecloudBinsSource.GetDecloudBinsUrlsForPlugin()
        {
            yield return "https://github.com/Decloud/DecloudDownloads/releases/download/v1.0/VC_redist.x64_2015_2019.7z";
        }
    }
}
