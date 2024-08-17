using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

using DataGrid = System.Windows.Controls.DataGrid;



namespace RevitRibbon_MainSourceCode_Resources.Forms
{
    public partial class LevelsParentViewsForm : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _selectedScale;
        private Dictionary<int, string> _scales;


        private string _filterText;
        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                OnPropertyChanged();
                FilterViewTemplates();
            }
        }

        private bool _isWildCardEnabled;
        public bool IsWildCardEnabled
        {
            get => _isWildCardEnabled;
            set
            {
                _isWildCardEnabled = value;
                OnPropertyChanged();
                FilterViewTemplates();
            }
        }

        public List<LevelSelection> Levels { get; set; }
        public List<ViewTemplateSelection> ViewTemplates { get; set; }
        public List<ViewTemplateSelection> SelectedViewTemplates { get; set; }
        private List<ViewTemplateSelection> FilteredViewTemplates { get; set; }
        //private bool isUpdatingSelection = false;

        public LevelsParentViewsForm(List<LevelSelection> levels, List<ViewTemplateSelection> viewTemplates)
        {
            InitializeComponent();
            Levels = levels;
            Levels = levels;

            //var viewScalesMappingDictionary =  MyUtils.ScalesList();
            var viewScalesMappingDictionary = ScalesList();
            Scales = viewScalesMappingDictionary;
            SelectedScale = viewScalesMappingDictionary.First(s => s.Value == "1/4\" = 1'-0\"").Key;

            ViewTemplates = viewTemplates;

            FilteredViewTemplates = new List<ViewTemplateSelection>(ViewTemplates);

            DataContext = this;

            LevelsDataGrid.ItemsSource = Levels;
            ViewTemplatesDataGrid.ItemsSource = FilteredViewTemplates;

            PopulateFilterComboBox();

            //View Type Dropdown 
            LoadComboBoxItems();
        }

        /// <summary>
        /// This method returns a Dictionary with the Mapped out View Scales
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> ScalesList()
        {
            var scaleMappings = new (int scaleNum, string ViewScaleString)[]
            {
                (1, "12\" = 1'-0\""),
                (2, "6\" = 1'-0\""),
                (4, "3\" = 1'-0\""),
                (8, "1-1/2\" = 1'-0\""),
                (12, "1\" = 1'-0\""),
                (16, "3/4\" = 1'-0\""),
                (24, "1/2\" = 1'-0\""),
                (32, "3/8\" = 1'-0\""),
                (48, "1/4\" = 1'-0\""),
                (64, "3/16\" = 1'-0\""),
                (96, "1/8\" = 1'-0\""),
                (120, "1\" = 10'-0\""),
                (128, "3/32\" = 1'-0\""),
                (192, "1/16\" = 1'-0\""),
                (240, "1\" = 20'-0\""),
                (256, "3/64\" = 1'-0\""),
                (360, "1\" = 30'-0\""),
                (384, "1/32\" = 1'-0\""),
                (480, "1\" = 40'-0\""),
                (600, "1\" = 50'-0\""),
                (720, "1\" = 60'-0\""),
                (768, "1/64\" = 1'-0\""),
                (960, "1\" = 80'-0\""),
                (1200, "1\" = 100'-0\""),
                (1920, "1\" = 160'-0\""),
                (2400, "1\" = 200'-0\""),
                (3600, "1\" = 300'-0\""),
                (4800, "1\" = 400'-0\""),
            };
            var scaleDictionary = new Dictionary<int, string>();

            foreach (var (scaleNum, viewScaleString) in scaleMappings)
            {
                scaleDictionary[scaleNum] = viewScaleString;
            }

            return scaleDictionary;
        }


        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public Dictionary<int, string> Scales
        {
            get => _scales;
            set
            {
                _scales = value;
                OnPropertyChanged(nameof(Scales));
            }
        }
        public int SelectedScale
        {
            get => _selectedScale;
            set
            {
                _selectedScale = value;
                OnPropertyChanged(nameof(SelectedScale));
            }
        }
        private void PopulateFilterComboBox()
        {
            var uniqueFirstWords = ViewTemplates
                .Select(v => v.Name.Split(' ')[0])
                .Distinct()
                .OrderBy(word => word)
                .ToList();

            FilterComboBox.Items.Clear();
            FilterComboBox.Items.Add("All");
            foreach (var word in uniqueFirstWords)
            {
                FilterComboBox.Items.Add(word);
            }
            FilterComboBox.SelectedIndex = 0;
        }

        private void FilterViewTemplates()
        {
            string filter = FilterText;
            if (string.IsNullOrEmpty(filter) || filter == "All")
            {
                FilteredViewTemplates = new List<ViewTemplateSelection>(ViewTemplates);
            }
            else if (IsWildCardEnabled)
            {
                FilteredViewTemplates = ViewTemplates.Where(v => v.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            else
            {
                FilteredViewTemplates = ViewTemplates.Where(v => v.Name.StartsWith(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            ViewTemplatesDataGrid.ItemsSource = null;
            ViewTemplatesDataGrid.ItemsSource = FilteredViewTemplates;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedViewTemplates = ViewTemplates.Where(v => v.IsSelected).ToList();
            var SelectedLevels = Levels.Where(v => v.IsSelected).ToList();

            if (SelectedViewTemplates.Count == 0 || SelectedLevels.Count == 0)
            {
                // using the MessageBox allows the dialog to open on top of the form, but I I use a TaskDialog, it opens  and the form gets sent to the back of the Revit window.
                MessageBox.Show("Please select at least one view template and one level before proceeding.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);


                return;
            }


            DialogResult = true;
        }



        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ChkBox_SelectAllLevels_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var level in Levels)
            {
                level.IsSelected = true;
            }
            LevelsDataGrid.ItemsSource = null;
            LevelsDataGrid.ItemsSource = Levels;
        }

        private void ChkBox_SelectAllLevels_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var level in Levels)
            {
                level.IsSelected = false;
            }
            LevelsDataGrid.ItemsSource = null;
            LevelsDataGrid.ItemsSource = Levels;
        }

        private void ChkBox_SelectAllTemplates_Checked(object sender, RoutedEventArgs e) // Not in use. XAML checkbox is commented out.
        {
            foreach (var template in ViewTemplates)
            {
                template.IsSelected = true;
            }
            ViewTemplatesDataGrid.ItemsSource = null;
            ViewTemplatesDataGrid.ItemsSource = FilteredViewTemplates;
        }

        private void ChkBox_SelectAllTemplates_Unchecked(object sender, RoutedEventArgs e) // Not in use. XAML checkbox is commented out.
        {
            foreach (var template in ViewTemplates)
            {
                template.IsSelected = false;
            }
            ViewTemplatesDataGrid.ItemsSource = null;
            ViewTemplatesDataGrid.ItemsSource = FilteredViewTemplates;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as System.Windows.Controls.CheckBox;
            if (checkBox == null) return;

            var dataGridRow = GetParentOfType<DataGridRow>(checkBox);
            if (dataGridRow == null) return;

            var item = dataGridRow.Item;
            bool isChecked = checkBox.IsChecked == true;

            var dataGrid = GetParentOfType<DataGrid>(dataGridRow);
            if (dataGrid == null) return;

            if (dataGrid.SelectedItems.Count > 1 && dataGrid.SelectedItems.Contains(item))
            {
                foreach (var selectedItem in dataGrid.SelectedItems)
                {
                    if (selectedItem is LevelSelection level)
                    {
                        level.IsSelected = isChecked;
                    }
                    else if (selectedItem is ViewTemplateSelection template)
                    {
                        template.IsSelected = isChecked;
                    }
                }
            }
            else
            {
                if (item is LevelSelection level)
                {
                    level.IsSelected = isChecked;
                }
                else if (item is ViewTemplateSelection template)
                {
                    template.IsSelected = isChecked;
                }
            }

            // Refresh the DataGrid to update the UI
            dataGrid.ItemsSource = null;
            if (dataGrid == LevelsDataGrid)
            {
                dataGrid.ItemsSource = Levels;
            }
            else if (dataGrid == ViewTemplatesDataGrid)
            {
                dataGrid.ItemsSource = FilteredViewTemplates;
            }




        }

        /// <summary>
        /// Retrieves the first parent of the specified type in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the parent to search for.</typeparam>
        /// <param name="element">The starting element to begin the search from.</param>
        /// <returns>The first parent of type T if found; otherwise, null.</returns>
        private T GetParentOfType<T>(DependencyObject element) where T : DependencyObject
        {
            while (element != null)
            {
                if (element is T correctlyTyped)
                {
                    return correctlyTyped;
                }
                element = VisualTreeHelper.GetParent(element);
            }
            return null;
        }

        private void ClearSelectedTemplates_Click(object sender, RoutedEventArgs e)
        {
            foreach (var template in ViewTemplates)
            {
                template.IsSelected = false;
            }
            ViewTemplatesDataGrid.ItemsSource = null;
            ViewTemplatesDataGrid.ItemsSource = FilteredViewTemplates;
        }


        private void LoadComboBoxItems()
        {
            var items = new Dictionary<string, string>
            {
                { "FloorPlan", "Floor Plan" },
                { "CeilingPlan", "Ceiling Plan" }
            };

            CmBox_ViewType.ItemsSource = items;
            CmBox_ViewType.SelectedValue = "FloorPlan"; // Default to "Floor Plan"
        }
    }    // --- end of LevelsParentViewsForm

    public class LevelSelection
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public ElementId Id { get; set; }
    }

    public class ViewTemplateSelection
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public ElementId Id { get; set; }
    }
}
