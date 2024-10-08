﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using UIFramework;


namespace RevitRibbon_MainSourceCode_Resources.Forms
{
    /// <summary>
    /// Interaction logic for ListForm1.xaml
    /// </summary>
    public partial class ListForm1 : Window
    {
        public ObservableCollection<ViewTemplateData> selectedViewTemplates { get; set; }
        public ListForm1(ObservableCollection<ViewTemplateData> allViewTemplates)
        {
            InitializeComponent();
            DataContext = this;
            selectedViewTemplates = allViewTemplates; // Use the same collection reference
            dataGrid.Items.Clear();
            dataGrid.ItemsSource = selectedViewTemplates;

        }

        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            //var list = selectedViewTemplates;
            this.DialogResult = true;
            this.Close();
        }
        private void btn_CheckAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in selectedViewTemplates)
            {
                item.IsSelected = true;
            }
        }

        private void btn_UnCheckAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in selectedViewTemplates)
            {
                item.IsSelected = false;
            }
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isShiftKeyDown)
            {
                foreach (var item in e.AddedItems)
                {
                    var selectedTemplate = (ViewTemplateData)item;
                    selectedTemplate.IsSelected = true;
                }
            }
            else if (e.AddedItems.Count > 0)
            {
                // Get the selected ViewTemplateData item
                var selectedTemplate = (ViewTemplateData)e.AddedItems[0];

                // Toggle the 'IsSelected' property of the selected template
                selectedTemplate.IsSelected = !selectedTemplate.IsSelected;
            }
        }

        private bool isShiftKeyDown = false;
        private void dataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                isShiftKeyDown = true;
            }
            else
            {
                isShiftKeyDown = false;
            }
        }


    }
}
