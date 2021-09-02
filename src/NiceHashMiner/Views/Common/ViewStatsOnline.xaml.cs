﻿using DCLCore;
using System.Windows;
using System.Windows.Controls;

namespace Decloud.Views.Common
{
    /// <summary>
    /// Interaction logic for ViewStatsOnline.xaml
    /// </summary>
    public partial class ViewStatsOnline : UserControl
    {
        public ViewStatsOnline()
        {
            InitializeComponent();
        }

        private void Click_VisitStatsOnline(object sender, RoutedEventArgs e)
        {
            ApplicationStateManager.VisitMiningStatsPage();
        }
    }
}
