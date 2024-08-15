using TreeNode = RevitRibbon_MainSourceCode_Resources.TreeNode;
namespace RevitRibbon_MainSourceCode //.Unique_Button_Classes.BIM_Team.NewProjectSetups
{
    [Transaction(TransactionMode.Manual)]
    public class DependentViewsBrowserTree : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Get the Revit application and document
                UIApplication uiapp = commandData.Application;
                UIDocument uidoc = uiapp.ActiveUIDocument;
                Document doc = uidoc.Document;

                // To be used later
                //var curViewScale = doc.ActiveView.Scale;

                // Populate the tree data
                var treeData = PopulateTreeView(doc);

                //// Create and show the WPF form
                ViewsTreeForm form = new ViewsTreeForm();
                form.InitializeTreeData(treeData);
                bool? dialogResult = form.ShowDialog();

                if (dialogResult != true) // if user does not click OK, cancel command
                    return Result.Cancelled;


                var selectedItems = GetSelectedViews(doc, form.TreeData);
                selectedItems = GetDependentViews(selectedItems);
                TaskDialog.Show("INFO", $"Selected views count {selectedItems.Count}");
                // Now process selectedItems...





                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                // Handle exceptions or log errors
                TaskDialog.Show("Error", $"An unexpected error occurred: {ex.Message}");
                return Result.Failed;
            }
        }

        public static List<View> GetDependentViews(List<View> views)
        {
            List<View> dependentViews;

#if REVIT2024
            dependentViews = views.Where(view => view.GetPrimaryViewId()
                                                      .Value != -1 && !view.IsTemplate)
                                                      .ToList();
#else
            dependentViews = views.Where(view => view.GetPrimaryViewId()
                                                      .IntegerValue != -1 && !view.IsTemplate)
                                                      .ToList();
#endif

            return dependentViews;
        }

        //public static List<View> GetDependentViews(List<View> views)
        //{
        //    var dependentViews = views.Where(view => view.GetPrimaryViewId()
        //                                                 .IntegerValue != -1 && !view.IsTemplate)
        //                                                 .ToList();
        //    return dependentViews;
        //}
        public static List<View> GetOnlyDependentViews(Document doc)
        {
            // Get all the views 
            var allViews = GetAllViews(doc);
            //return only dependent views
            return GetDependentViews(allViews);
        }

        public static List<View> GetSelectedViews(Document doc, IEnumerable<RevitRibbon_MainSourceCode_Resources.TreeNode> nodes)
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
                selectedViews.AddRange(GetSelectedViews(doc, node.Children)); // Ensure Children is never null
            }

            return selectedViews;
        }

        public static List<TreeNode> GetDependentsViewsTree(Document doc)
        {
            var treeNodes = new List<TreeNode>();

            // Collect all views, excluding view templates
            List<View> allViews = GetAllViews(doc);

            // Group views by their type
            //var viewsByType = allViews.GroupBy(v => v.ViewType);
            // ORDER BY FLOOR PLAN TYPE (Sorts Form Tree view so Floor Plans are above Ceiling Plans)
            //var viewsByType = allViews.OrderBy(v => v.ViewType).GroupBy(v => v.ViewType);
            // Order by ViewType and then by "Browser Sub-Category"
            var viewsByType = allViews.OrderBy(v => v.ViewType).ThenBy(v => v.LookupParameter("Browser Sub-Category")?.AsString() ?? string.Empty).GroupBy(v => v.ViewType);


            foreach (var group in viewsByType)
            {
                var viewTypeNode = new TreeNode { Header = group.Key.ToString() };

                foreach (var view in group)
                {
#if REVIT2024
                    long primaryViewIdValue = view.GetPrimaryViewId().Value;
#else
                    int primaryViewIdValue = view.GetPrimaryViewId().IntegerValue;
#endif

                    // Check if the view is independent and has dependents
                    foreach (var dv in allViews)
                    {
#if REVIT2024
                        long dependentViewPrimaryId = dv.GetPrimaryViewId().Value;
                        long viewIdValue = view.Id.Value;
#else
                        int dependentViewPrimaryId = dv.GetPrimaryViewId().IntegerValue;
                        int viewIdValue = view.Id.IntegerValue;
#endif

                        if (primaryViewIdValue == -1 && dependentViewPrimaryId == viewIdValue)
                        {
                            var viewNode = new TreeNode
                            {
                                Header = view.Name,
                                ViewId = view.Id,
                                Children = new List<TreeNode>() // Initialize Children
                            };

                            // Add dependent views as children
                            var dependentViews = allViews.Where(depView => depView.GetPrimaryViewId().Equals(view.Id));
                            foreach (var depView in dependentViews)
                            {
                                var depViewNode = new TreeNode
                                {
                                    Header = depView.Name,
                                    ViewId = depView.Id
                                };
                                viewNode.Children.Add(depViewNode);
                            }

                            viewTypeNode.Children.Add(viewNode);
                            break; // Break the loop once a match is found
                        }
                    }
                }

                if (viewTypeNode.Children.Any())
                {
                    treeNodes.Add(viewTypeNode);
                }
            }

            return treeNodes;
        }


        //public static List<TreeNode> GetDependentsViewsTree(Document doc)
        //{
        //    var treeNodes = new List<TreeNode>();

        //    // Collect all views, excluding view templates
        //    List<View> allViews = GetAllViews(doc);

        //    // Group views by their type
        //    var viewsByType = allViews.GroupBy(v => v.ViewType);

        //    foreach (var group in viewsByType)
        //    {
        //        var viewTypeNode = new TreeNode { Header = group.Key.ToString() };

        //        // Filter to get only independent views that have dependent views
        //        var independentViewsWithDependents = group
        //            .Where(v => v.GetPrimaryViewId().IntegerValue == -1 &&
        //                        allViews.Any(dv => dv.GetPrimaryViewId() == v.Id))
        //            .ToList();

        //        foreach (var view in independentViewsWithDependents)
        //        {
        //            var viewNode = new TreeNode
        //            {
        //                Header = view.Name,
        //                ViewId = view.Id,
        //                Children = new List<TreeNode>() // Initialize Children
        //            };

        //            // Add dependent views as children
        //            var dependentViews = allViews.Where(dv => dv.GetPrimaryViewId() == view.Id);
        //            foreach (var depView in dependentViews)
        //            {
        //                var depViewNode = new TreeNode
        //                {
        //                    Header = depView.Name,
        //                    ViewId = depView.Id
        //                };
        //                viewNode.Children.Add(depViewNode);
        //            }

        //            viewTypeNode.Children.Add(viewNode);
        //        }

        //        if (viewTypeNode.Children.Any())
        //        {
        //            treeNodes.Add(viewTypeNode);
        //        }
        //    }

        //    return treeNodes;
        //}

        private static List<View> GetAllViews(Document doc)
        {
            return new FilteredElementCollector(doc)
                            .OfClass(typeof(View))
                            .Cast<View>()
                            .Where(v => !v.IsTemplate)
                            .ToList();
        }

        public List<TreeNode> GetAllViewsTree(Document doc)
        {
            var treeNodes = new List<TreeNode>();

            // Collect all views, excluding view templates
            var allViews = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate)
                .ToList();

            // Group views by their type
            var viewsByType = allViews.GroupBy(v => v.ViewType);

            foreach (var group in viewsByType)
            {
                var viewTypeNode = new TreeNode { Header = group.Key.ToString() };

                // Separate dependent and independent views
                var independentViews = new List<View>();
                var dependentViews = new List<View>();

#if REVIT2024
                independentViews = group.Where(v => v.GetPrimaryViewId().Value == -1).ToList();
                dependentViews = group.Where(v => v.GetPrimaryViewId().Value != -1).ToList();
#else
                independentViews = group.Where(v => v.GetPrimaryViewId().IntegerValue == -1).ToList();
                dependentViews = group.Where(v => v.GetPrimaryViewId().IntegerValue != -1).ToList();
#endif


                // Add independent views
                foreach (var view in independentViews)
                {
                    var viewNode = new TreeNode
                    {
                        Header = view.Name,
                        ViewId = view.Id, // Set the ElementId
                        Children = new List<TreeNode>() // Ensure Children is initialized
                    };
                    viewTypeNode.Children.Add(viewNode);
                }

                // Add dependent views under their parent view
                foreach (var view in dependentViews)
                {
                    var parentView = doc.GetElement(view.GetPrimaryViewId()) as View;
                    var parentViewNode = viewTypeNode.Children.FirstOrDefault(n => n.Header.Equals(parentView?.Name))
                                         ?? new TreeNode { Header = parentView?.Name ?? "Unknown" };

                    if (!viewTypeNode.Children.Contains(parentViewNode))
                    {
                        viewTypeNode.Children.Add(parentViewNode);
                    }

                    var dependentViewNode = new TreeNode
                    {
                        Header = view.Name,
                        ViewId = view.Id,
                        Children = new List<TreeNode>() // Ensure Children is initialized
                    };
                    parentViewNode.Children.Add(dependentViewNode);
                }

                treeNodes.Add(viewTypeNode);
            }

            return treeNodes;
        }

        public List<TreeNode> GetSheetsTree(Document doc)
        {
            var treeNodes = new List<TreeNode>();

            // Collect all sheets, assuming sheets are of type ViewSheet
            var allSheets = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .Where(vs => !vs.IsTemplate)
                .ToList();

            // Group sheets by some criteria, e.g., by Discipline
            var sheetsByDiscipline = allSheets
                .GroupBy(vs => vs.LookupParameter("Discipline")?.AsString() ?? "Undefined");

            foreach (var group in sheetsByDiscipline)
            {
                var disciplineNode = new TreeNode { Header = group.Key };

                foreach (var sheet in group)
                {
                    var sheetNode = new TreeNode
                    {
                        Header = $"{sheet.SheetNumber} - {sheet.Name}",
                        ViewId = sheet.Id,
                        Children = new List<TreeNode>()
                    };
                    disciplineNode.Children.Add(sheetNode);
                }

                treeNodes.Add(disciplineNode);
            }

            return treeNodes;
        }

        public static List<TreeNode> PopulateTreeView(Document doc)
        {
            var treeNodes = new List<TreeNode>();

            var viewsNode = new TreeNode { Header = "Views" };
            viewsNode.Children.AddRange(GetDependentsViewsTree(doc));
            treeNodes.Add(viewsNode);

            //// Uncomment this part if you want to show the tree view for Sheets
            //var sheetsNode = new TreeNode { Header = "Sheets" };
            //sheetsNode.Children.AddRange(GetSheetsTree(doc));
            //treeNodes.Add(sheetsNode);

            return treeNodes;
        }

        public void PopulateProjectBrowserTree(Document doc)
        {
            // Use BrowserOrganization to understand sorting/grouping
            BrowserOrganization orgViews = BrowserOrganization.GetCurrentBrowserOrganizationForViews(doc);
            BrowserOrganization orgSheets = BrowserOrganization.GetCurrentBrowserOrganizationForSheets(doc);

            // Use FilteredElementCollector and other methods to retrieve views, sheets
            // ...

            // Populate your TreeView based on the retrieved data and organization logic
            // ...
        }
    }
}
