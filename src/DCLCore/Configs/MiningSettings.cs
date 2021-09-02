using DCL.Common;

namespace DCLCore.Configs
{
    public class MiningSettings : NotifyChangedBase
    {
        public static MiningSettings Instance { get; } = new MiningSettings();
        private MiningSettings() { }

        private bool _nVIDIAP0State = false;
        public bool NVIDIAP0State
        {
            get => _nVIDIAP0State;
            set
            {
                _nVIDIAP0State = value;
                OnPropertyChanged(nameof(NVIDIAP0State));
            }
        }

        private bool _autoStartMining = false;
        public bool AutoStartMining
        {
            get => _autoStartMining;
            set
            {
                _autoStartMining = value;
                OnPropertyChanged(nameof(AutoStartMining));
            }
        }

        private int _DecloudAPIQueryInterval = 5;
        public int DecloudAPIQueryInterval
        {
            get => _DecloudAPIQueryInterval;
            set
            {
                _DecloudAPIQueryInterval = value;
                OnPropertyChanged(nameof(DecloudAPIQueryInterval));
            }
        }

        private bool _hideMiningWindows = false;
        public bool HideMiningWindows
        {
            get => _hideMiningWindows;
            set
            {
                _hideMiningWindows = value;
                DCL.DecloudPluginToolkitV1.DecloudToolkit.HideMiningWindows = value;
                OnPropertyChanged(nameof(HideMiningWindows));
                OnPropertyChanged(nameof(HideMiningWindowsAlertVisible));
            }
        }

        private bool _minimizeMiningWindows = false;
        public bool MinimizeMiningWindows
        {
            get => _minimizeMiningWindows;
            set
            {
                _minimizeMiningWindows = value;
                DCL.DecloudPluginToolkitV1.DecloudToolkit.MinimizeMiningWindows = value;
                OnPropertyChanged(nameof(MinimizeMiningWindows));
                OnPropertyChanged(nameof(HideMiningWindowsAlertVisible));
            }
        }

        private int _DecloudRestartDelayMS = 1000;
        public int DecloudRestartDelayMS
        {
            get => _DecloudRestartDelayMS;
            set
            {
                _DecloudRestartDelayMS = value;
                DCL.DecloudPluginToolkitV1.DecloudToolkit.DecloudRestartDelayMS = value;
                OnPropertyChanged(nameof(DecloudRestartDelayMS));
            }
        }

        private int _apiBindPortPoolStart = 4000;
        public int ApiBindPortPoolStart
        {
            get => _apiBindPortPoolStart;
            set
            {
                _apiBindPortPoolStart = value;
                DCL.DecloudPluginToolkitV1.FreePortsCheckerManager.ApiBindPortPoolStart = value;
                OnPropertyChanged(nameof(ApiBindPortPoolStart));
            }
        }


        public bool HideMiningWindowsAlertVisible => MinimizeMiningWindows && HideMiningWindows;
    }
}
