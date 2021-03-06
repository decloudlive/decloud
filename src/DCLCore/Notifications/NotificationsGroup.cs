using System;

namespace DCLCore.Notifications
{
    public enum NotificationsGroup
    {
        Misc,
        Market,
        Profit,
        MonitoringNvidiaElevate,
        EthlargementElevate,
        EthlargementNotEnabled,
        ConnectionLost,
        NoEnabledDevice,
        DemoMining,
        NoSma,
        [Obsolete]
        NoDeviceSelectedBenchmark,
        [Obsolete]
        NothingToBenchmark,
        FailedBenchmarks,
        NoSupportedDevices,
        MissingDecloud,
        MissingDecloudBins,
        [Obsolete]
        FailedVideoController,
        [Obsolete]
        WmiEnabled,
        [Obsolete]
        Net45,
        [Obsolete]
        BitOS64,
        DCLUpdate,
        DCLUpdateFailed,
        DCLWasUpdated,
        PluginUpdate,
        NvidiaDCH,
        WindowsDefenderException,
        ComputeModeAMD,
        LargePages,
        VirtualMemory,
        OpenClFallback,
        NoAvailableAlgorithms,
        LogArchiveUpload,
        MissingGPUs,
        NVMLInitFail,
        NVMLLoadFail
    }
}
