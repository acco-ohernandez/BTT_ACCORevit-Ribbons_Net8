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
using System.Windows.Shapes;

namespace RevitRibbon_MainSourceCode_Resources.Forms
{
    /// <summary>
    /// Interaction logic for SchedulesImport_Form.xaml
    /// </summary>
    public partial class SchedulesImport_Form : Window
    {
        public SchedulesImport_Form()
        {
            InitializeComponent();
        }

        private void btn_Import_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
