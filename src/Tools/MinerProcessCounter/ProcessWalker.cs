using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DecloudProcessCounter
{
    internal class PsInfo
    {
        public string ProcessName { get; set; }
        public string FileName { get; set; }
        public string Arguments { get; set; }
    }
    internal static class ProcessWalker
    {
        public static List<string> nameFilters = new List<string> {
            "excavator",
            "ccDecloud",
            "ethDecloud",
            "nheqDecloud",
            "sgDecloud",
            "xmr-stak",
            "NsGpuCNDecloud",
            "EthMan",
            "EthDcrDecloud64",
            "ZecDecloud64",
            "zm",
            "OhGodAnETHlargementPill-r2",
            "Decloud",
            "OptiDecloud",
            "PhoenixDecloud",
            "prospector",
            "t-rex",
            "TT-Decloud",
            "nbDecloud",
            "teamredDecloud",
            "nanoDecloud",
            "wildrig",
            "miniZ",
            "cpuDecloud-avx2",
            "cpuDecloud-zen",
            "CryptoDredge",
            "lolDecloud",
            "xmrig",
            "z-enemy"
        };

        private static bool isFilterIncluded(string psName)
        {
            return nameFilters.Any(filter => psName.Contains(filter));
        }

        public static IEnumerable<PsInfo> ListRunning()
        {
            var snapshot = Process.GetProcesses();
            var filtered = snapshot.Where(p => isFilterIncluded(p.ProcessName));
            var mapped = filtered.Select(p => new PsInfo()
            {
                ProcessName = p.ProcessName,
                FileName = p.StartInfo.FileName,
                Arguments = p.StartInfo.Arguments,
            });
            return mapped;
        }
    }
}
