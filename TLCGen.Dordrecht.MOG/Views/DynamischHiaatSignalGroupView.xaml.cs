﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TLCGen.Dordrecht.DynamischHiaat.Views
{
    /// <summary>
    /// Interaction logic for DynamischHiaatSignalGroupView.xaml
    /// </summary>
    public partial class DynamischHiaatSignalGroupView : UserControl
    {
        public DynamischHiaatSignalGroupView()
        {
            InitializeComponent();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.codingconnected.eu/tlcgenwiki/");
        }
    }
}
