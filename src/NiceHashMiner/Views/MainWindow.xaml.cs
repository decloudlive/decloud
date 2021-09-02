using DCL.Common;
using DCLCore;
using DCLCore.ApplicationState;
using DCLCore.Configs;
using DCLCore.Mining.Plugins;
using DCLCore.Notifications;
using DCLCore.Utils;
using Decloud.ViewModels;
using Decloud.Views.Common;
using Decloud.Views.Common.NHBase;
using Decloud.Views.TDPSettings;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace Decloud.Views
{
    /// <summary>
    /// Interaction logic for MainWindowNew2.xaml
    /// </summary>
    public partial class MainWindow : DCLMainWindow
    {
        private readonly MainVM _vm;
        private bool _miningStoppedOnClose;
        private Timer _timer = new Timer();

        public readonly bool? LoginSuccess = null;

        public MainWindow(bool? loginSuccess)
        {
            InitializeComponent();
            LoginSuccess = loginSuccess;
            _vm = this.AssertViewModel<MainVM>();
            Title = ApplicationStateManager.Title;

            base.SizeChanged += new SizeChangedEventHandler(this.OnSizeChangedSave);

            Translations.LanguageChanged += (s, e) => WindowUtils.Translate(this);
            LoadingBar.Visibility = Visibility.Visible;
            Topmost = GUISettings.Instance.GUIWindowsAlwaysOnTop;
            CustomDialogManager.MainWindow = this;
            SetBurnCalledAction();
            SetNoDeviceAction();
            _timer.Interval = 1000 * 60 * 2; //2min
            _timer.Elapsed += CheckConnection;
            _timer.Start();

            if (GUISettings.Instance.MainFormSize != System.Drawing.Size.Empty)
            {
                this.Width = GUISettings.Instance.MainFormSize.Width;
                this.Height = GUISettings.Instance.MainFormSize.Height;
            }
        }

        private void OnSizeChangedSave(object sender, SizeChangedEventArgs e)
        {
            GUISettings.Instance.MainFormSize = new System.Drawing.Size((int)e.NewSize.Width, (int)e.NewSize.Height);
        }

        private void GUISettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GUISettings.GUIWindowsAlwaysOnTop))
            {
                this.Topmost = _vm.GUISettings.GUIWindowsAlwaysOnTop;
            }
        }

        #region Start-Loaded/Closing
        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            SetBuildTag();
            ThemeSetterManager.SetThemeSelectedThemes();
            UpdateHelpers.OnAutoUpdate = () =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var DCLUpdaterDialog = new CustomDialog()
                    {
                        Title = Translations.Tr("Decloud Starting Update"),
                        Description = Translations.Tr("Decloud auto updater in progress."),
                        CancelVisible = Visibility.Collapsed,
                        OkVisible = Visibility.Collapsed,
                        AnimationVisible = Visibility.Visible,
                        ExitVisible = Visibility.Collapsed
                    };
                    ShowContentAsModalDialog(DCLUpdaterDialog);
                });
            };
            await MainWindow_OnLoadedTask();
            _vm.GUISettings.PropertyChanged += GUISettings_PropertyChanged;
            NotificationsManager.Instance.PropertyChanged += Instance_PropertyChanged;
            MiningState.Instance.PropertyChanged += MiningStateInstance_PropertyChanged;
            SetNotificationCount(NotificationsManager.Instance.NotificationNewCount);

            if (!HasWriteAccessToFolder(Paths.Root))
            {
                this.Dispatcher.Invoke(() =>
                {
                    var DCLNoPermissions = new CustomDialog()
                    {
                        Title = Translations.Tr("Folder lacks permissions"),
                        Description = Translations.Tr("Decloud folder doesn't have write access. This can prevent some features from working."),
                        OkText = Translations.Tr("OK"),
                        CancelVisible = Visibility.Collapsed,
                        OkVisible = Visibility.Visible,
                        AnimationVisible = Visibility.Collapsed
                    };
                    ShowContentAsModalDialog(DCLNoPermissions);
                });
            }
        }

        private void MiningStateInstance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (nameof(MiningState.Instance.IsDemoMining) == e.PropertyName && MiningState.Instance.IsDemoMining)
            {
                Dispatcher.Invoke(() =>
                {
                    var demoMiningDialog = new EnterWalletDialogDemo();
                    CustomDialogManager.ShowModalDialog(demoMiningDialog);
                });
            }
        }

        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (nameof(NotificationsManager.NotificationNewCount) == e.PropertyName)
            {
                Dispatcher.Invoke(() =>
                {
                    SetNotificationCount(NotificationsManager.Instance.NotificationNewCount);
                });
            }
        }

        public void SetBurnCalledAction()
        {
            ApplicationStateManager.BurnCalledAction = () =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var DCLBurnDialog = new CustomDialog()
                    {
                        Title = Translations.Tr("Burn Error!"),
                        Description = Translations.Tr("Error during burn"),
                        OkText = Translations.Tr("OK"),
                        CancelVisible = Visibility.Collapsed,
                        AnimationVisible = Visibility.Collapsed
                    };
                    DCLBurnDialog.OnExit += (s, e) =>
                    {
                        ApplicationStateManager.ExecuteApplicationExit();
                    };
                    ShowContentAsModalDialog(DCLBurnDialog);
                });
            };
        }

        public void SetNoDeviceAction()
        {
            ApplicationStateManager.NoDeviceAction = () =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var DCLNoDeviceDialog = new CustomDialog()
                    {
                        Title = Translations.Tr("No Supported Devices"),
                        Description = Translations.Tr("No supported devices are found. Select the OK button for help or cancel to continue."),
                        OkText = Translations.Tr("OK"),
                        CancelText = Translations.Tr("Cancel"),
                        AnimationVisible = Visibility.Collapsed
                    };
                    DCLNoDeviceDialog.OKClick += (s, e) =>
                    {
                        Process.Start(Links.DCLNoDevHelp);
                    };
                    DCLNoDeviceDialog.OnExit += (s, e) =>
                    {
                        ApplicationStateManager.ExecuteApplicationExit();
                    };
                    ShowContentAsModalDialog(DCLNoDeviceDialog);
                });
            };
        }

        // just in case we add more awaits this signature will await all of them
        private async Task MainWindow_OnLoadedTask()
        {
            try
            {
                await _vm.InitializeDCL(LoadingBar.StartupLoader);
            }
            finally
            {
                LoadingBar.Visibility = Visibility.Collapsed;
                // Re-enable managed controls
                IsEnabled = true;
                SetTabButtonsEnabled();
                if (BuildOptions.SHOW_TDP_SETTINGS)
                {
                    var tdpWindow = new TDPSettingsWindow();
                    tdpWindow.DataContext = _vm;
                    tdpWindow.Show();
                }

                if (DecloudPluginsManager.EulaConfirm.Any())
                {
                    var pluginsPopup = new Plugins.PluginsConfirmDialog();
                    pluginsPopup.DataContext = new Plugins.PluginsConfirmDialog.VM
                    {
                        Plugins = new ObservableCollection<PluginPackageInfoCR>(DecloudPluginsManager.EulaConfirm)
                    };
                    ShowContentAsModalDialog(pluginsPopup);
                }

                if (LoginSuccess.HasValue)
                {
                    var description = LoginSuccess.Value ? Translations.Tr("Login performed successfully.") : Translations.Tr("Unable to retreive BTC address. Please retreive it by yourself from web page.");
                    var btcLoginDialog = new CustomDialog()
                    {
                        Title = Translations.Tr("Login"),
                        OkText = Translations.Tr("Ok"),
                        CancelVisible = Visibility.Collapsed,
                        AnimationVisible = Visibility.Collapsed,
                        Description = description
                    };
                    btcLoginDialog.OKClick += (s, e) =>
                    {
                        if (!LoginSuccess.Value) Process.Start(Links.Login);
                    };
                    CustomDialogManager.ShowModalDialog(btcLoginDialog);
                }

                if (Launcher.IsUpdated)
                {
                    var DCLUpdatedDialog = new CustomDialog()
                    {
                        Title = Translations.Tr("Decloud Updated"),
                        Description = Translations.Tr("Completed Decloud auto update."),
                        OkText = Translations.Tr("OK"),
                        CancelVisible = Visibility.Collapsed,
                        AnimationVisible = Visibility.Collapsed
                    };
                    ShowContentAsModalDialog(DCLUpdatedDialog);
                }

                if (Launcher.IsUpdatedFailed)
                {
                    var DCLUpdatedDialog = new CustomDialog()
                    {
                        Title = Translations.Tr("Decloud Autoupdate Failed"),
                        Description = Translations.Tr("Decloud auto update failed to complete. Autoupdates are disabled until next Decloud launch."),
                        OkText = Translations.Tr("OK"),
                        CancelVisible = Visibility.Collapsed,
                        AnimationVisible = Visibility.Collapsed
                    };
                    ShowContentAsModalDialog(DCLUpdatedDialog);
                }
            }
        }

        private async void MainWindow_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await MainWindow_OnClosingTask(e);
        }

        private async Task MainWindow_OnClosingTask(System.ComponentModel.CancelEventArgs e)
        {
            // Only ever try to prevent closing once
            if (_miningStoppedOnClose) return;

            _miningStoppedOnClose = true;
            //e.Cancel = true;
            IsEnabled = false;
            //await _vm.StopMining();
            await ApplicationStateManager.BeforeExit();
            ApplicationStateManager.ExecuteApplicationExit();
            //Close();
        }
        #endregion Start-Loaded/Closing

        protected override void OnTabSelected(ToggleButtonType tabType)
        {
            var tabName = tabType.ToString();
            foreach (TabItem tab in MainTabs.Items)
            {
                if (tabName.Contains(tab.Name))
                {
                    MainTabs.SelectedItem = tab;
                    break;
                }
            }
        }

        #region Minimize to tray stuff
        private void CloseMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TaskbarIcon_OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void MainWindow_OnStateChanged(object sender, EventArgs e)
        {
            if (!_vm.GUISettings.MinimizeToTray) return;
            if (WindowState == WindowState.Minimized) // TODO && config min to tray
                Hide();
        }
        #endregion Minimize to tray stuff

        private void StartAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => _vm.StartMining());
        }

        private void StopAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => _vm.StopMining());
        }


        private bool HasWriteAccessToFolder(string folderPath)
        {
            try
            {
                // Attempt to get a list of security permissions from the folder. 
                // This will raise an exception if the path is read only or do not have access to view the permissions. 
                var ds = Directory.GetAccessControl(folderPath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private bool DCLwsDialogShown = false;
        private void CheckConnection(object sender, ElapsedEventArgs e)
        {
            if (!_vm.DCLWSConnected && !DCLwsDialogShown)
            {
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        var dialog = new CustomDialog
                        {
                            Title = Translations.Tr("DCLWS not connected"),
                            Description = Translations.Tr("Not connected to DCLWS. Please check your internet connection."),
                            CancelVisible = Visibility.Collapsed,
                            OkVisible = Visibility.Collapsed,
                            AnimationVisible = Visibility.Collapsed,

                        };
                        CustomDialogManager.ShowModalDialog(dialog);
                    });
                    DCLwsDialogShown = true;
                    _timer.Stop();
                    _timer.Interval = 1000;
                    _timer.Start();
                }
                catch (Exception ex)
                {
                    Logger.Error("MainVM.IsDCLWSConnected", ex.Message);
                }
            }
            else if (_vm.DCLWSConnected && DCLwsDialogShown)
            {
                try
                {
                    Dispatcher.Invoke(() => CustomDialogManager.HideCurrentModal());
                    DCLwsDialogShown = false;
                    _timer.Stop();
                    _timer.Interval = 1000 * 60 * 2; //2min
                    _timer.Start();
                }
                catch (Exception ex)
                {
                    _timer.Stop();
                    Logger.Error("MainVM.IsDCLWSConnected", ex.Message);
                }
            }
        }
    }
}
