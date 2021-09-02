using Decloud.Views.Common.NHBase;
using System.Windows;

namespace Decloud.Views
{
    /// <summary>
    /// Interaction logic for SingleInstanceNotice.xaml
    /// </summary>
    public partial class SingleInstanceNotice : BaseDialogWindow
    {
        public SingleInstanceNotice()
        {
            InitializeComponent();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
