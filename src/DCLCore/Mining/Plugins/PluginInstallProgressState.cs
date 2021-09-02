
namespace DCLCore.Mining.Plugins
{
    public enum PluginInstallProgressState
    {
        Pending,
        // plugin dll
        PendingDownloadingPlugin,
        DownloadingPlugin,
        PendingExtractingPlugin,
        ExtractingPlugin,
        // Decloud bin
        PendingDownloadingDecloud,
        DownloadingDecloud,
        PendingExtractingDecloud,
        ExtractingDecloud,
        // failed cases
        FailedDownloadingPlugin,
        FailedExtractingPlugin,
        FailedDownloadingDecloud,
        FailedExtractingDecloud,
        // installing
        FailedPluginLoad,
        FailedPluginInit,
        FailedUnknown,
        Success,
        Canceled
    }
}
