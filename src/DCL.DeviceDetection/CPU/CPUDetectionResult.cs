using System.Collections.Generic;

namespace DCL.DeviceDetection.CPU
{
    internal class CPUDetectionResult
    {
        public int NumberOfCPUCores { get; internal set; }
        public int VirtualCoresCount { get; internal set; }
        public bool IsHyperThreadingEnabled => VirtualCoresCount > NumberOfCPUCores;
        public List<CpuInfo> CpuInfos { get; internal set; }
    }
}
