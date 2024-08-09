#region Namespaces
using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

//using RevitRibbon_MainSourceCode_Resources.Forms;

#endregion

namespace RevitRibbon_MainSourceCode //.Unique_Button_Classes.Common
{
    [Transaction(TransactionMode.Manual)]

    public class Cmd_DependentViewsBrowserTree : IExternalCommand
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
                object treeData = PopulateTreeView(doc);

                //// Create and show the WPF form
                ViewsTreeForm form = new ViewsTreeForm();
                form.InitializeTreeData(treeData);
                bool? dialogResult = form.ShowDialog();

                if (dialogResult != true) // if user does not click OK, cancel command
                    return Result.Cancelled;

                IEnumerable<RevitRibbon_MainSourceCode.TreeNode> forTreeData = (IEnumerable<RevitRibbon_MainSourceCode.TreeNode>)form.TreeData;
                //var selectedItems = GetSelectedViews(doc, form.TreeData);
                var selectedItems = GetSelectedViews(doc, forTreeData);
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

        /// <summary>
        /// Retrieves the dependent views from a list of views.
        /// </summary>
        /// <param name="views">The list of views.</param>
        /// <returns>The list of dependent views.</returns>
        public static List<View> GetDependentViews(List<View> views)
        {
            List<View> dependentViews = new List<View>();
#if REVIT2020 || REVIT2021 || REVIT2022 || REVIT2023
            dependentViews = views.Where(view => view.GetPrimaryViewId()
                                                         .IntegerValue != -1 && !view.IsTemplate)
                                                         .ToList();
#else // REVIT2024 or later
            dependentViews = views.Where(view => view.GetPrimaryViewId().Value != -1 && !view.IsTemplate).ToList();
#endif
            return dependentViews;
        }
        public static List<View> GetOnlyDependentViews(Document doc)
        {
            // Get all the views 
            var allViews = GetAllViews(doc);
            //return only dependent views
            return GetDependentViews(allViews);
        }

        public static List<View> GetSelectedViews(Document doc, IEnumerable<TreeNode> nodes)
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

            // Collect all views, excluding view templates and "BIM Setup View"
            List<View> allViews = GetAllViews(doc).Where(v => !v.Name.StartsWith("BIM Setup View")).ToList();

            // Group views by their type
            var viewsByType = allViews.GroupBy(v => v.ViewType);

            foreach (var group in viewsByType)
            {
                var viewTypeNode = new TreeNode { Header = group.Key.ToString() };

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
                    var viewNode = new TreeNode
                    {
                        Header = view.Name,
                        ViewId = view.Id,
                        Children = new List<TreeNode>() // Initialize Children
                    };

                    // Add dependent views as children
                    var dependentViews = allViews.Where(dv => dv.GetPrimaryViewId() == view.Id);
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
                IEnumerable<View> independentViews = new List<View>();
                IEnumerable<View> dependentViews = new List<View>();
#if REVIT2020 || REVIT2021 || REVIT2022 || REVIT2023
                independentViews = group.Where(v => v.GetPrimaryViewId().IntegerValue == -1);
                dependentViews = group.Where(v => v.GetPrimaryViewId().IntegerValue != -1);
#else // REVIT2024 or later
                independentViews = group.Where(v => v.GetPrimaryViewId().Value == -1);
                dependentViews = group.Where(v => v.GetPrimaryViewId().Value != -1);
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
            List<TreeNode> treeNodes1 = GetDependentsViewsTree(doc);
            viewsNode.Children.AddRange(treeNodes1);
            //viewsNode.Children.AddRange(GetDependentsViewsTree(doc));
            treeNodes.Add(viewsNode);

            //// Uncomment this part if you want to show the tree view for Sheets
            //var sheetsNode = new TreeNode { Header = "Sheets" };
            //sheetsNode.Children.AddRange(GetSheetsTree(doc));
            //treeNodes.Add(sheetsNode);

            return treeNodes;
        }
        public static List<TreeNode> PopulateTreeView1(Document doc)
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

