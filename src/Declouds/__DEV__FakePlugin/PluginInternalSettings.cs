using DCL.DecloudPluginToolkitV1.Configs;
using DCL.DecloudPluginToolkitV1.ExtraLaunchParameters;
using System;

namespace FakePlugin
{
    internal static class PluginInternalSettings
    {
        internal static TimeSpan DefaultTimeout = new TimeSpan(0, 2, 0);

        internal static DecloudApiMaxTimeoutSetting GetApiMaxTimeoutConfig = new DecloudApiMaxTimeoutSetting
        {
            GeneralTimeout = DefaultTimeout,
        };

        internal static DecloudOptionsPackage DecloudOptionsPackage = new DecloudOptionsPackage
        {
        };
    }
}
