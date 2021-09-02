using DCL.Common;
using Decloud.ViewModels;
using Decloud.Views.Common;
using System.Windows.Controls;

namespace Decloud.Views
{
    /// <summary>
    /// Interaction logic for TemporaryStartupLoadingControl.xaml
    /// </summary>
    public partial class TemporaryStartupLoadingControl : UserControl
    {
        public IStartupLoader StartupLoader { get; }
        public TemporaryStartupLoadingControl()
        {
            InitializeComponent();
            StartupLoader = this.AssertViewModel<StartupLoadingVM>();
        }
    }
}
