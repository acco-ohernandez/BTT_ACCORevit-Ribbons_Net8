#region Namespaces

using Brushes = System.Windows.Media.Brushes;
using TreeNode = RevitRibbon_MainSourceCode_Resources.TreeNode;
using WinData = System.Windows.Data;
#endregion

namespace RevitRibbon_MainSourceCode
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_UpdateAppliedDependentViews : IExternalCommand
    {
        public bool DependentViewsMatchBIMSetupViews { get; set; } = true;
        public bool NoDependentViewsForTheForm { get; private set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            // Check if the 'BIM Setup View' exists, exclude dependent views with the same name
            // Declare a variable to hold the 'BIM Setup View'
            View BIMSetupView;
            // Call a utility method to get the 'BIM Setup View' from the document
            // This method sets the BIMSetupView variable if found and returns a Result indicating success or failure
            Result result = MyUtils.GetBIMSetupView(doc, out BIMSetupView);
            // Check if the result indicates that the 'BIM Setup View' was successfully found
            // If the view was not found, return the result indicating failure
            if (result != Result.Succeeded) { return result; }

            // Retrieve dependent views of 'BIM Setup View'
            List<View> BIMSetupViewDependentViews = MyUtils.GetDependentViewsFromParentView(BIMSetupView);

            if (BIMSetupViewDependentViews.Count == 0)
            {
                //TaskDialog.Show("INFO", "'BIM Setup View' has no dependent views");
                MyUtils.M_MyTaskDialog("Action Required", "Please create Dependent views on the\r\n'BIM Setup View' before proceeding.", "Warning");
                return Result.Cancelled;
            }
            // Get the dependent views assigned Scope Boxes
            var dependentViewsWithScopeBoxParams = new List<Parameter>();
            foreach (View dependentView in BIMSetupViewDependentViews)
            {
                var assignedScopeBox = MyUtils.GetAssignedScopeBox(dependentView);
                dependentViewsWithScopeBoxParams.Add(assignedScopeBox);
            }

            // Check if dependent views exist
            var dependentViews = Cmd_DependentViewsBrowserTree.GetOnlyDependentViews(doc);
            if (dependentViews.Count == 0)
            {
                MyUtils.M_MyTaskDialog("Info:", "There are no dependent views.");
                return Result.Cancelled;
            }

            // Retrieve parent IDs of dependent views
            List<ElementId> parentViewIds = new List<ElementId>();
            foreach (View dependentView in dependentViews)
            {
                ElementId parentViewId = dependentView.GetPrimaryViewId();
                parentViewIds.Add(parentViewId);
            }

            // All dependenet views selected by the user from the UpdateAppliedDependentViewsForm
            List<View> selectedDependentViews = GetAllDependentViesFromViewsTreeForm(doc);
            if (NoDependentViewsForTheForm)  // This property is set in the GetAllDependentViesFromViewsTreeForm method
            {
                MyUtils.M_MyTaskDialog("Action Required", "Please \"Apply Dependent Views...\" from the\n'BIM Setup View' to Parent views before proceeding.", "Warning");
                return Result.Cancelled; // Cancel if there are no dependent views to update
            }
            if (selectedDependentViews == null || selectedDependentViews.Count == 0) { return Result.Cancelled; } // Cancel if user closes or cancels the form

            var listOfListsOfViews = GroupViewsByPrimaryViewId(selectedDependentViews);


            List<Dictionary<View, Element>> listOfDependentViewsDictionaries = GetListOfDependentViewsDictionaries(doc, listOfListsOfViews, dependentViewsWithScopeBoxParams);

            if (DependentViewsMatchBIMSetupViews == false)
            {
                MyUtils.M_MyTaskDialog("Cannot Proceed",
                                        $"The number of Dependent views on the Parent view does not match the number of Dependent views on the 'BIM Setup View'.",
                                        "Error");
                return Result.Cancelled;
            }

            List<ViewsRenameReport> NamesResult = new List<ViewsRenameReport>();
            // Start a transaction to rename the view
            using (Transaction trans = new Transaction(doc, "Update Applied Dependent Views"))
            {
                trans.Start();

                foreach (var viewsAndScopeBoxes in listOfDependentViewsDictionaries)
                {
                    foreach (var keyValuePair in viewsAndScopeBoxes)
                    {
                        //var namesResult = new List<ViewsRenameReport>();
                        View view = keyValuePair.Key;
                        Element scopeBox = keyValuePair.Value;
                        MyUtils.AssignScopeBoxToView(view, scopeBox);
                        var previousName = view.Name;
                        var scopeBoxName = scopeBox.Name;

                        // Generate new view name that includes the scope box name and remove anything from "PARENT" onwards
                        var viewWithScopeboxName = RenameViewWithScopeBoxName(view, scopeBoxName);
                        //var uniqueName = MyUtils.GetUniqueViewName(doc, view.Name);

                        // Check if the view name already exists in the document
                        if (MyUtils.ViewNameExists(doc, viewWithScopeboxName))
                        {
                            viewWithScopeboxName = MyUtils.GetUniqueViewName(doc, viewWithScopeboxName);
                        }
                        view.Name = viewWithScopeboxName;

                        // Add the NamesResult to the ViewsRenameReport list
                        NamesResult.Add(new ViewsRenameReport
                        {
                            PreviousName = previousName,
                            ScopeBoxName = scopeBoxName,
                            NewName = viewWithScopeboxName
                        });
                    }

                }
                trans.Commit();
            }


            // Show the rename report in a WPF Window
            ShowRenameReport(NamesResult);

            return Result.Succeeded;
        }

        //public static Result GetBIMSetupView(Document doc, out View BIMSetupView)
        //{
        //    BIMSetupView = Cmd_DependentViewsBrowserTree.GetAllViews(doc)
        //                                                .Where(v => v.Name.StartsWith("BIM Setup View") && !v.IsTemplate && v.GetPrimaryViewId() == ElementId.InvalidElementId)
        //                                                .FirstOrDefault();
        //    if (BIMSetupView == null)
        //    {
        //        MyUtils.M_MyTaskDialog("INFO", "No 'BIM Setup View' found");
        //        return Result.Cancelled;
        //    }
        //    return Result.Succeeded;
        //}

        private List<Dictionary<View, Element>> GetListOfDependentViewsDictionaries(Document doc, List<List<View>> listOfListsOfViews, List<Parameter> dependentViewsWithScopeBoxParams)
        {
            List<Dictionary<View, Element>> listOfDependentViewsDictionaries = new List<Dictionary<View, Element>>();
            foreach (var listOfDictionaries in listOfListsOfViews)
            {

                var selectedDependentViews = listOfDictionaries;
                // Create a dictionary to map views to scope boxes
                Dictionary<View, Element> viewsAndScopeBoxes = new Dictionary<View, Element>();
                if (selectedDependentViews == null) { DependentViewsMatchBIMSetupViews = false; return null; }// Result.Cancelled; }
                if (selectedDependentViews.Count == dependentViewsWithScopeBoxParams.Count)
                {
                    for (int i = 0; i < selectedDependentViews.Count; i++)
                    {
                        viewsAndScopeBoxes.Add(selectedDependentViews[i], doc.GetElement(dependentViewsWithScopeBoxParams[i].AsElementId()));
                    }
                    listOfDependentViewsDictionaries.Add(viewsAndScopeBoxes);
                }
                else
                {
                    DependentViewsMatchBIMSetupViews = false; return null;
                }
            }
            return listOfDependentViewsDictionaries;

        }

        private static List<List<View>> GroupViewsByPrimaryViewId(List<View> selectedDependentViews)
        {
            Dictionary<ElementId, List<View>> viewsGroupedByPrimaryView = new Dictionary<ElementId, List<View>>();

            // Group views by their PrimaryViewId
            foreach (View dependentView in selectedDependentViews)
            {
                ElementId primaryViewId = dependentView.GetPrimaryViewId();

                if (!viewsGroupedByPrimaryView.ContainsKey(primaryViewId))
                {
                    viewsGroupedByPrimaryView[primaryViewId] = new List<View>();
                }

                viewsGroupedByPrimaryView[primaryViewId].Add(dependentView);
            }

            // Convert dictionary values to a list of lists of views
            List<List<View>> listOfListsOfViews = viewsGroupedByPrimaryView.Values.ToList();
            return listOfListsOfViews;
        }

        private string RenameViewWithScopeBoxName(View view, string scopeBoxName)
        {
            // Get the original view name
            string viewName = view.Name;

            // Split the view name at "PARENT"
            string[] splitName = viewName.Split(new string[] { "PARENT" }, StringSplitOptions.None);
            //string[] splitName = viewName.Split(new string[] { "PARENT - Dependent" }, StringSplitOptions.None);

            // If the split results in more than one part, use the first part
            string newViewName = splitName.Length > 0 ? splitName[0].Trim() : viewName;

            // Append the scope box name to the first part of the original name
            newViewName = $"{newViewName} {scopeBoxName}";

            return newViewName;
        }
        //private void RenameViewWithScopeBoxName(View view, string scopeBoxName)
        //{
        //    // Get the original view name
        //    string viewName = view.Name;

        //    // Split the view name at "PARENT"
        //    string[] splitName = viewName.Split(new string[] { "PARENT" }, StringSplitOptions.None);
        //    //string[] splitName = viewName.Split(new string[] { "PARENT - Dependent" }, StringSplitOptions.None);

        //    // If the split results in more than one part, use the first part
        //    string newViewName = splitName.Length > 0 ? splitName[0].Trim() : viewName;

        //    // Append the scope box name to the first part of the original name
        //    newViewName = $"{newViewName} {scopeBoxName}";

        //    //// Start a transaction to rename the view
        //    //using (Transaction trans = new Transaction(view.Document, "Rename View"))
        //    //{
        //    //    trans.Start();
        //    view.Name = newViewName;
        //    //    trans.Commit();
        //    //}
        //}


        private bool AreBoundingBoxesEqual(BoundingBoxXYZ box1, BoundingBoxXYZ box2)
        {
            return box1.Min.IsAlmostEqualTo(box2.Min) && box1.Max.IsAlmostEqualTo(box2.Max);
        }

        public List<View> GetAllDependentViesFromViewsTreeForm(Document doc)
        {
            // Populate the tree data
            //var treeData = Cmd_DependentViewsBrowserTree.PopulateTreeView(doc);
            var treeData = PopulateTreeView2(doc);

            //using LINQ to check if any TreeNode in the list has children.
            bool treeDataHasChildren = treeData.Any(node => node.Children != null && node.Children.Any());

            if (!treeDataHasChildren)
            {
                NoDependentViewsForTheForm = true;
                return null;
            }

            //// Create and show the WPF form
            var form = new UpdateAppliedDependentViewsForm();


            if (!treeDataHasChildren)
            {
                form.lbl_info.Content = "No dependent views to update were found.";
                form.lbl_info.Background = Brushes.Red; // Set the background color to red
                form.lbl_info.Foreground = Brushes.White; // Set the background color to red
                form.lbl_info.FontSize = 20;
            }

            form.InitializeTreeData(treeData);
            bool? dialogResult = form.ShowDialog();

            if (dialogResult != true) // if user does not click OK, cancel command
                return null;


            //var selectedItems = Cmd_DependentViewsBrowserTree.GetSelectedViews(doc, form.TreeData);
            var selectedItems = GetSelectedViews2(doc, form.TreeData);
            selectedItems = Cmd_DependentViewsBrowserTree.GetDependentViews(selectedItems);

            //TaskDialog.Show("INFO", $"Selected views count {selectedItems.Count}");
            return selectedItems;

        }

        public static List<RevitRibbon_MainSourceCode_Resources.TreeNode> PopulateTreeView2(Document doc)
        {
            var treeNodes = new List<RevitRibbon_MainSourceCode_Resources.TreeNode>();

            var viewsNode = new RevitRibbon_MainSourceCode_Resources.TreeNode { Header = "Views" };
            List<RevitRibbon_MainSourceCode_Resources.TreeNode> treeNodes1 = GetDependentsViewsTree(doc);
            viewsNode.Children.AddRange(treeNodes1);
            treeNodes.Add(viewsNode);

            return treeNodes;
        }
        public static List<View> GetSelectedViews2(Document doc, IEnumerable<RevitRibbon_MainSourceCode_Resources.TreeNode> nodes)
        {
            var selectedViews = new List<View>();

            if (nodes == null) return selectedViews; // Check if nodes is null

            foreach (var node in nodes)
            {
                if (node != null && node.IsSelected && node.ViewId != null)
                {
                    // Check if node or ViewId is null
                    var view = doc.GetElement(node.ViewId) as View;
                    if (view != null)
                    {
                        selectedViews.Add(view);
                    }
                }

                // Recursively check for selected children
                selectedViews.AddRange(GetSelectedViews2(doc, node.Children)); // Ensure Children is never null
            }

            return selectedViews;
        }
        public static List<RevitRibbon_MainSourceCode_Resources.TreeNode> GetDependentsViewsTree(Document doc)
        {
            var treeNodes = new List<RevitRibbon_MainSourceCode_Resources.TreeNode>();

            // Collect all views, excluding view templates and "BIM Setup View"
            List<View> allViews = GetAllViews(doc).Where(v => !v.Name.StartsWith("BIM Setup View")).ToList();

            // Group views by their type
            var viewsByType = allViews.GroupBy(v => v.ViewType);

            foreach (var group in viewsByType)
            {
                var viewTypeNode = new RevitRibbon_MainSourceCode_Resources.TreeNode { Header = group.Key.ToString() };

                // Filter to get only parent views that have dependent views
                List<View> independentViewsWithDependents = new List<View>();
#if REVIT2020 || REVIT2021 || REVIT2022 || REVIT2023
                independentViewsWithDependents = group
                    .Where(v => v.GetPrimaryViewId().IntegerValue == -1 &&
                                allViews.Any(dv => dv.GetPrimaryViewId() == v.Id))
                    .ToList();
#else // REVIT2024 or later
                independentViewsWithDependents = group
                    .Where(v => v.GetPrimaryViewId().Value == -1 &&
                                allViews.Any(dv => dv.GetPrimaryViewId() == v.Id))
                    .ToList();
#endif
                foreach (var view in independentViewsWithDependents)
                {
                    var viewNode = new RevitRibbon_MainSourceCode_Resources.TreeNode
                    {
                        Header = view.Name,
                        ViewId = view.Id,
                        Children = new List<RevitRibbon_MainSourceCode_Resources.TreeNode>() // Initialize Children
                    };

                    // Add dependent views as children
                    var dependentViews = allViews.Where(dv => dv.GetPrimaryViewId() == view.Id);
                    foreach (var depView in dependentViews)
                    {
                        var depViewNode = new RevitRibbon_MainSourceCode_Resources.TreeNode
                        {
                            Header = depView.Name,
                            ViewId = depView.Id
                        };
                        viewNode.Children.Add(depViewNode);
                    }

                    viewTypeNode.Children.Add(viewNode);
                }

                if (viewTypeNode.Children.Any())
                {
                    treeNodes.Add(viewTypeNode);
                }
            }

            return treeNodes;
        }
        public static List<View> GetAllViews(Document doc)
        {
            return new FilteredElementCollector(doc)
                            .OfClass(typeof(View))
                            .Cast<View>()
                            .Where(v => !v.IsTemplate)
                            .OrderBy(v => v.LookupParameter("Browser Sub-Category")?.AsString())
                            .ToList();
        }
        // Method to show the report
        private void ShowRenameReport0(List<ViewsRenameReport> namesResult) // This method works, but causes Revit to close when closing the form. Keeping it posible future use.
        {
            var app = new System.Windows.Application();
            RenameReportWindow reportWindow = new RenameReportWindow(namesResult);
            app.Run(reportWindow);
        }
        private void ShowRenameReport(List<ViewsRenameReport> namesResult)
        {
            RenameReportWindow reportWindow = new RenameReportWindow(namesResult);
            reportWindow.ShowDialog();
        }
    }

    public class ViewsRenameReport
    {
        public string PreviousName { get; set; }
        public string ScopeBoxName { get; set; }
        public string NewName { get; set; }
    }

    public class RenameReportWindow : Window
    {
        public RenameReportWindow(List<ViewsRenameReport> reports)
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Title = "Update Applied Dependent Views Summary";
            this.Width = 750;
            this.Height = 500;

            var dataGrid = new System.Windows.Controls.DataGrid
            {
                AutoGenerateColumns = false,
                IsReadOnly = true,
                ItemsSource = reports,
            };

            DataGridTextColumn previousNameColumn = new DataGridTextColumn
            {
                Header = "Previous View Name",
                Binding = new WinData.Binding("PreviousName"),
                CellStyle = GetCenteredCellStyle(),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            };
            dataGrid.Columns.Add(previousNameColumn);

            DataGridTextColumn scopeBoxNameColumn = new DataGridTextColumn
            {
                Header = "Scope Box Name",
                Binding = new WinData.Binding("ScopeBoxName"),
                CellStyle = GetCenteredCellStyle(),
                Width = new DataGridLength(1, DataGridLengthUnitType.Auto)
            };
            dataGrid.Columns.Add(scopeBoxNameColumn);

            DataGridTextColumn newNameColumn = new DataGridTextColumn
            {
                Header = "New View Name",
                Binding = new WinData.Binding("NewName"),
                CellStyle = GetCenteredCellStyle(),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
            };
            dataGrid.Columns.Add(newNameColumn);

            this.Content = dataGrid;
        }

        private Style GetCenteredCellStyle()
        {
            Style style = new Style(typeof(System.Windows.Controls.DataGridCell));
            style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
            style.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
            return style;
        }
    }

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return !booleanValue;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return !booleanValue;
            }
            return value;
        }
    }
}

