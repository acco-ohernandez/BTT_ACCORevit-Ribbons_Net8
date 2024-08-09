using System.Collections.Generic;
using System.Windows;

//using MessageBox = System.Windows.MessageBox;
//using RevitRibbon_MainSourceCode;
using TreeNode = RevitRibbon_MainSourceCode_Resources.TreeNode;
//using Template_Tab;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RevitRibbon_MainSourceCode_Resources.Forms
{
    /// <summary>
    /// Interaction logic for ViewsTreeForm.xaml
    /// </summary>
    public partial class ViewsTreeForm : Window
    {
        // Property to hold tree data
        public List<TreeNode> TreeData { get; set; }

        public ViewsTreeForm()
        {
            InitializeComponent();
        }

        // Method to be called after InitializeComponent to set the data context
        //public void InitializeTreeData(List<TreeNode> treeData)
        public void InitializeTreeData(object _treeData)
        {
            //List<TreeNode> treeData = _treeData ?? new List<TreeNode>();
            List<TreeNode> treeData = _treeData as List<TreeNode>;
            TreeData = treeData; // TreeData is a property in this form
            viewsTreeView.ItemsSource = TreeData; // Bind the tree data
            this.DataContext = this;  // Setting the data context of the window
        }

        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            //var selectedViewsCount = TreeData.SelectMany(v => v.Children).SelectMany(v => v.Children).SelectMany(v => v.Children).Where(v => v.IsSelected).ToList();
            //if (selectedViewsCount.Count == 0)
            if (!AnyViewSelected(TreeData))// using this method makes sure if the depth of the tree changes, the method will still work
            {
                System.Windows.MessageBox.Show("Please select at least one view before proceeding.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            this.DialogResult = true;
            this.Close();
        }
        private bool AnyViewSelected(IEnumerable<TreeNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.IsSelected)
                {
                    return true;
                }

                if (AnyViewSelected(node.Children))
                {
                    return true;
                }
            }

            return false;
        }
        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        //private void viewsTreeView_Loaded(object sender, RoutedEventArgs e)
        //{
        //    ExpandTreeViewItems(viewsTreeView.Items);
        //}

        //private void ExpandTreeViewItems(ItemCollection items)
        //{
        //    foreach (object item in items)
        //    {
        //        if (item is TreeViewItem treeViewItem)
        //        {
        //            treeViewItem.IsExpanded = true;
        //            ExpandTreeViewItems(treeViewItem.Items);
        //        }
        //    }
        //}

    }
}
