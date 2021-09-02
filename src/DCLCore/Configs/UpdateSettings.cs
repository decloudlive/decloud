using DCL.Common;

namespace DCLCore.Configs
{
    public class UpdateSettings : NotifyChangedBase
    {
        private static object _lock = new object();

        public static UpdateSettings Instance { get; } = new UpdateSettings();
        private UpdateSettings() { }

        public bool _autoUpdateDecloud = false;
        public bool AutoUpdateDecloud
        {
            get
            {
                lock (_lock)
                {
                    return _autoUpdateDecloud;
                }
            }
            set
            {
                lock (_lock)
                {
                    _autoUpdateDecloud = value;
                }
                OnPropertyChanged(nameof(AutoUpdateDecloud));
            }
        }

        public bool _autoUpdateDecloudPlugins = true;
        public bool AutoUpdateDecloudPlugins
        {
            get
            {
                lock (_lock)
                {
                    return _autoUpdateDecloudPlugins;
                }
            }
            set
            {
                lock (_lock)
                {
                    _autoUpdateDecloudPlugins = value;
                }
                OnPropertyChanged(nameof(AutoUpdateDecloudPlugins));
            }
        }
    }
}
