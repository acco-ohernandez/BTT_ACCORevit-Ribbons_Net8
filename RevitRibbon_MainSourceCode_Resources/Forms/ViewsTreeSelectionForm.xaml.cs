using System;
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
    public partial class ViewsTreeSelectionForm : Window
    {
        public List<ViewsTreeNode> TreeData { get; set; }

        public ViewsTreeSelectionForm()
        {
            InitializeComponent();
        }

        public void InitializeTreeData(List<ViewsTreeNode> treeData)
        {
            TreeData = treeData;
            viewsTreeView.ItemsSource = TreeData; // Bind the tree data
            this.DataContext = this;  // Setting the data context of the window
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            var selectedViewsCount = TreeData.SelectMany(v => v.Children).Count(v => v.IsSelected);
            if (selectedViewsCount == 0)
            {
                MessageBox.Show("Please select at least one view before proceeding.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
