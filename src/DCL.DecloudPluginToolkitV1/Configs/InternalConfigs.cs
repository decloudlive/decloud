using DCL.DecloudPluginToolkitV1.Interfaces;
using System;


namespace DCL.DecloudPluginToolkitV1.Configs
{
    [Obsolete("Use DCL.Common.Configs.InternalConfigs", true)]
    public static class InternalConfigs
    {
        [Obsolete("Use DCL.Common.Configs.InternalConfigs.ReadFileSettings", false)]
        public static T ReadFileSettings<T>(string filePath) where T : class => DCL.Common.Configs.InternalConfigs.ReadFileSettings<T>(filePath);

        [Obsolete("Use DCL.Common.Configs.InternalConfigs.WriteFileSettings", false)]
        public static bool WriteFileSettings<T>(string filePath, T settingsValue) where T : class => DCL.Common.Configs.InternalConfigs.WriteFileSettings(filePath, settingsValue);

        [Obsolete("Use DCL.Common.Configs.InternalConfigs.WriteFileSettings", false)]
        public static bool WriteFileSettings(string filePath, string settingsText) => DCL.Common.Configs.InternalConfigs.WriteFileSettings(filePath, settingsText);

        [Obsolete("Use DCL.Common.Configs.InternalConfigs.InitInternalSetting", false)]
        public static T InitInternalSetting<T>(string pluginRoot, T setting, string settingFileName) where T : class, IInternalSetting => DCL.Common.Configs.InternalConfigs.InitInternalSetting(pluginRoot, setting, settingFileName);
    }
}
