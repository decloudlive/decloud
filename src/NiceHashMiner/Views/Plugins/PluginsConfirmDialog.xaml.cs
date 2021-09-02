using DCLCore;
using DCLCore.Mining.Plugins;
using Decloud.Views.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static DCLCore.Translations;

namespace Decloud.Views.Plugins
{
    /// <summary>
    /// Interaction logic for PluginsConfirmDialog.xaml
    /// </summary>
    public partial class PluginsConfirmDialog : UserControl
    {
        public class VM
        {
            public ObservableCollection<PluginPackageInfoCR> Plugins { get; set; }
        }
        int _pluginsToAccept = 0;
        public PluginsConfirmDialog()
        {
            InitializeComponent();

            this.DataContextChanged += PluginsConfirmDialog_DataContextChanged;
        }

        private void PluginsConfirmDialog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = e.NewValue as VM;
            _pluginsToAccept = vm?.Plugins?.Count() ?? 0;
            if (_pluginsToAccept == 0) CustomDialogManager.HideCurrentModal();
        }

        private void OnAcceptOrDecline(object sender, RoutedEventArgs e)
        {
            _pluginsToAccept--;
            if (_pluginsToAccept == 0)
            {
                CustomDialogManager.HideCurrentModal();
                var DCLRestartDialog = new CustomDialog()
                {
                    Title = Tr("Restart Decloud"),
                    Description = Tr("Decloud restart is required."),
                    OkText = Tr("Restart"),
                    AnimationVisible = Visibility.Collapsed,
                    CancelVisible = Visibility.Collapsed,
                    ExitVisible = Visibility.Collapsed,
                };
                DCLRestartDialog.OKClick += (s, e1) =>
                {
                    Task.Run(() => ApplicationStateManager.RestartProgram());
                };
                CustomDialogManager.ShowModalDialog(DCLRestartDialog);

                var DCLRestartingDialog = new CustomDialog()
                {
                    Title = Tr("Decloud Restarting"),
                    Description = Tr("Decloud restart in progress."),
                    CancelVisible = Visibility.Collapsed,
                    OkVisible = Visibility.Collapsed,
                    AnimationVisible = Visibility.Visible,
                    ExitVisible = Visibility.Collapsed
                };
                CustomDialogManager.ShowModalDialog(DCLRestartingDialog);
            }
        }


    }
}
