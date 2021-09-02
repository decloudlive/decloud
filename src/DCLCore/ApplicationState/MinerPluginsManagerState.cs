using DCL.Common;
using DCLCore.Mining.Plugins;
using System.Collections.Generic;

namespace DCLCore.ApplicationState
{
    public class DecloudPluginsManagerState : NotifyChangedBase
    {
        public static DecloudPluginsManagerState Instance { get; } = new DecloudPluginsManagerState();

        private DecloudPluginsManagerState() { }



        private List<PluginPackageInfoCR> _rankedPlugins = null;
        public List<PluginPackageInfoCR> RankedPlugins
        {
            get => _rankedPlugins;
            set
            {
                _rankedPlugins = value;
                OnPropertyChanged();
            }
        }
    }
}
