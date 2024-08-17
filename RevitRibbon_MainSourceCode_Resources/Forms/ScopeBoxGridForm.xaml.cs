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

//namespace RevitRibbon_MainSourceCode.Forms
namespace RevitRibbon_MainSourceCode_Resources.Forms
{
    public partial class ScopeBoxGridForm : Window
    {
        public ScopeBoxGridForm()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            KeyDown += OnKeyDown;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Center the window on the screen
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void BtnCreateScopeBoxGrid_Click(object sender, RoutedEventArgs e)
        {

            bool isTxtRowsNumeric = int.TryParse(txtRows.Text, out int n);
            bool isTxtColumnsNumeric = int.TryParse(txtColumns.Text, out int n2);
            bool isTxtBaseScopeBoxName = string.IsNullOrEmpty(txtBaseScopeBoxName.Text);
            // Check if all the bools are true and if not, show a message box
            if (!isTxtRowsNumeric)
            {
                MessageBox.Show("Please input the Number of Rows before proceeding.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!isTxtColumnsNumeric)
            {
                MessageBox.Show("Please input the Number of Columns before proceeding.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (isTxtBaseScopeBoxName)
            {
                MessageBox.Show("Please input the Scope Box Name before proceeding.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnCreateScopeBoxGrid_Click(sender, e);
            }
        }

    }
}
