using Decloud.Views.Common.NHBase;
using System.Windows.Controls;

namespace Decloud
{
    internal static class CustomDialogManager
    {
        internal static DCLMainWindow MainWindow { get; set; }
        internal static void ShowDialog(UserControl userControl)
        {
            MainWindow?.ShowContentAsDialog(userControl);
        }

        internal static void ShowModalDialog(UserControl userControl)
        {
            MainWindow?.ShowContentAsModalDialog(userControl);
        }


        internal static void HideCurrentModal()
        {
            MainWindow?.HideModal();
        }
    }
}
