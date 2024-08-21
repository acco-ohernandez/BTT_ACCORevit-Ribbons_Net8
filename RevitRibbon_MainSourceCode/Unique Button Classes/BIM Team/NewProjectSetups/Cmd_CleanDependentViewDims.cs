using TreeNode = RevitRibbon_MainSourceCode_Resources.TreeNode;
namespace RevitRibbon_MainSourceCode
{
    // Transaction required when using IExternalCommand for Revit
    [Transaction(TransactionMode.Manual)] // Starts a new transaction manually for every process that modifies the Revit database
    [Regeneration(RegenerationOption.Manual)]
    public class Cmd_CleanDependentViewDims : IExternalCommand // Implementing IExternalCommand gets Revit to recognize our plugins
    {
        public int DimensionsHiden { get; set; } = 0;
        public List<ElementId> CleanedDependentViewIds { get; private set; } = new List<ElementId>();

        private static ElementId originalTickMarkValue = null;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get the Revit application and document
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            try
            {
                // Check if dependent views exist
                if (DependentViewsBrowserTree.GetOnlyDependentViews(doc).Count == 0)
                {
                    //TaskDialog.Show("Info:", "There are no dependent views.");
                    MyUtils.M_MyTaskDialog("Action Required", "Dependent views are required before proceeding.", "Warning");
                    return Result.Cancelled;
                }

                // All dependenet views
                //List<View> selectedViews = GetAllDependentVies(doc);
                List<View> selectedViews = GetAllDependentViesFromViewsTreeForm(doc);
                if (selectedViews == null) // User cancelled the operation
                    return Result.Cancelled;

                string noCropBoxFoundList = "";

                using (Transaction transaction = new Transaction(doc, "Hide Dimensions in Dependent View"))
                {
                    transaction.Start();
                    //doc.Regenerate();
                    // Step 2: Process Each View
                    foreach (var curView in selectedViews)
                    {
                        // Step 3: Check if the current view has the CropBoxActive
                        if (curView.CropBoxActive == true)
                        {
                            var gridDimensionInCurrentView = GetGridDimensionsInView(doc, curView);

                            // Step 4: Get Dimensions Inside Crop Box in Parent View
                            var dimensionsInsideCropBox = GetDimensionsInsideCropBox(curView);

                            // Step 5: Hide Dimensions in Dependent View
                            HideDimensionsInDependentView(doc, curView, dimensionsInsideCropBox);

                            ////Restore TickMark To Original 
                            //gridDimensionInCurrentView
                            //    .Select(dim => dim.DimensionType) // Extract DimensionType Ids
                            //    .Distinct() // Ensure unique DimensionType Ids
                            //    .ToList() // Convert to List for iteration
                            //    .ForEach(dimType => RestoreTickMarkToOriginal(doc, dimType)); // Apply operation
                        }
                        else
                        {
                            noCropBoxFoundList += $"{curView.Name}\n";
                        }
                    }
                    transaction.Commit();
                }

                if (!string.IsNullOrEmpty(noCropBoxFoundList))
                    TaskDialog.Show("Info", $"CropBox is no active in view(s):\n {noCropBoxFoundList}");

                MyUtils.M_MyTaskDialog("Clean Dependent View Dimensions",
                                        $"{DimensionsHiden} dimensions hidden from\n" +
                                        $"{CleanedDependentViewIds.Count} Dependent views.",
                                        false);

                return Result.Succeeded;
            }
            catch (Exception ex) // Catch errors
            {
                TaskDialog exceptStatus = new TaskDialog(ex.GetType().FullName.ToString());
                exceptStatus.MainInstruction = ex.Message;
                exceptStatus.Show();

                return Result.Failed;
            }

        }

        public static void RestoreTickMarkToOriginal(Document doc, DimensionType dimensionType)
        {
            // Get the "Tick Mark" parameter
            Parameter tickMarkParam = dimensionType.LookupParameter("Tick Mark");

            if (tickMarkParam != null && !tickMarkParam.IsReadOnly)
            {
                if (originalTickMarkValue == null)
                    TaskDialog.Show("Info", $"The value of originalTickMarkValue is: {originalTickMarkValue}");

                bool setResult = tickMarkParam.Set(originalTickMarkValue);

                // Check if the parameter was set successfully
                if (!setResult)
                {
                    TaskDialog.Show("Error", "Unable to set 'Tick Mark' parameter to None.");
                }
            }
        }
        public static void StoreAndSetTickMarkToZero(Document doc, DimensionType dimensionType)
        {
            // Get the "Tick Mark" parameter
            Parameter tickMarkParam = dimensionType.LookupParameter("Tick Mark");

            if (tickMarkParam != null && !tickMarkParam.IsReadOnly)
            {
                if (originalTickMarkValue == null)
                    originalTickMarkValue = tickMarkParam.AsElementId();

                // Attempt to set the parameter to ElementId.InvalidElementId
                // Note: This might not be directly possible for all parameter types, especially for system parameters
                // You might need to find the specific method or workaround for setting this parameter to "None"
                bool setResult = tickMarkParam.Set(ElementId.InvalidElementId);

                // Check if the parameter was set successfully
                if (!setResult)
                {
                    TaskDialog.Show("Error", "Unable to set 'Tick Mark' parameter to None.");
                }
            }
        }





        public static List<Dimension> GetGridDimensionsInView(Document doc, View view)
        {
            List<Dimension> gridDimensions = new List<Dimension>();

            // Collect all dimensions in the view
            FilteredElementCollector collector = new FilteredElementCollector(doc, view.Id);
            ICollection<Element> allDimensions = collector.OfClass(typeof(Dimension)).ToElements();

            // Iterate through the collected dimensions
            foreach (Element elem in allDimensions)
            {
                Dimension dim = elem as Dimension;
                if (dim != null)
                {
                    // Retrieve the DimensionType of the current dimension
                    DimensionType dimType = doc.GetElement(dim.GetTypeId()) as DimensionType;

                    // Check if the DimensionType name matches "GRID DIMENSIONS"
                    if (dimType != null && dimType.Name.Equals("GRID DIMENSIONS", StringComparison.OrdinalIgnoreCase))
                    {
                        gridDimensions.Add(dim);
                    }
                }
            }

            return gridDimensions;
        }


        //<---###########
        public List<View> GetAllDependentViesFromViewsTreeForm(Document doc)
        {
            // Populate the tree data
            //var treeData = DependentViewsBrowserTree.PopulateTreeView(doc);
            var treeData = PopulateTreeView(doc);

            //// Create and show the WPF form
            ViewsTreeForm form = new ViewsTreeForm();
            form.InitializeTreeData(treeData);
            bool? dialogResult = form.ShowDialog();

            if (dialogResult != true) // if user does not click OK, cancel command
                return null;

            //var selectedItems = DependentViewsBrowserTree.GetSelectedViews(doc, form.TreeData);
            //var selectedItems = GetSelectedViews(doc, form.TreeData as ICollection<ElementId>);
            ICollection<ElementId> elementIds = form.TreeData.SelectMany(v => v.Children).SelectMany(v => v.Children).Select(v => v.ViewId).ToList();

            //List<ElementId> elementIDs = new List<ElementId>();
            //foreach (var node in form.TreeData)
            //{
            //    if (node.Children != null && !elementIDs.Contains(node.ViewId))
            //    {
            //        elementIDs.Add(node.ViewId);
            //    }
            //}

            //var selectedItems = GetSelectedViews(doc, elementIds);
            var elementIDs = GetElementIdsFromTreeNodes(form.TreeData);
            var selectedItems = GetSelectedViews(doc, elementIDs);

            //selectedItems = DependentViewsBrowserTree.GetDependentViews(selectedItems);
            selectedItems = GetDependentViews(selectedItems);
            //TaskDialog.Show("INFO", $"Selected views count {selectedItems.Count}");
            return selectedItems;

        }
        public List<ElementId> GetElementIdsFromTreeNodes(List<RevitRibbon_MainSourceCode_Resources.TreeNode> treeNodes)
        {
            List<ElementId> elementIds = new List<ElementId>();

            foreach (var node in treeNodes)
            {
                // Add the current node's ViewId if it hasn't been added already
                if (!elementIds.Contains(node.ViewId))
                {
                    elementIds.Add(node.ViewId);
                }

                // Recursively add the children nodes' ViewIds
                if (node.Children != null && node.Children.Count > 0)
                {
                    elementIds.AddRange(GetElementIdsFromTreeNodes(node.Children));
                }
            }

            return elementIds;
        }

        public static List<View> GetDependentViews(List<View> views)
        {
            List<View> dependentViews;

#if REVIT2020 || RREVIT2021 || REVIT2022 || REVIT2023
            dependentViews = views.Where(view => view.GetPrimaryViewId()
                                                      .IntegerValue != -1 && !view.IsTemplate)
                                                      .ToList();
#else

            dependentViews = views.Where(view => view.GetPrimaryViewId()
                                                      .Value != -1 && !view.IsTemplate)
                                                      .ToList();
#endif

            return dependentViews;
        }
        private List<View> GetAllDependentVies(Document doc)
        {
            var views = new FilteredElementCollector(doc).OfClass(typeof(View));
            var DependentViews = new List<View>();
            foreach (View view in views)
            {
                ElementId parentId = view.GetPrimaryViewId();
                int parentIdValue; // Declare outside of conditional compilation to ensure it's always in scope
#if REVIT2020 || REVIT2021 || REVIT2022 || REVIT2023
                Debug.Print("Revit2020-2023");
                parentIdValue = parentId.IntegerValue;
#elif REVIT2024 || REVIT2025
                Debug.Print("Revit2024 OR Revit2025");
                parentIdValue = (int)parentId.Value; // Explicit cast from long to int
#endif
                if (parentIdValue == -1 && !view.IsTemplate)
                {
                    // View is Not a dependent
                }
                else if (parentIdValue != -1 && !view.IsTemplate)
                {
                    // View is dependent
                    DependentViews.Add(view);
                }
            }
            return DependentViews;
        }

        private List<View> GetSelectedViews(Document doc, ICollection<ElementId> selectedIds)
        {
            // Filter out only the views from the selected elements
            return selectedIds
                .Where(id => id is not null && id != ElementId.InvalidElementId)
                .Select(id => doc.GetElement(id))
                .OfType<View>()
                .ToList();
        }
        public static List<RevitRibbon_MainSourceCode_Resources.TreeNode> PopulateTreeView(Document doc)
        {
            var treeNodes = new List<RevitRibbon_MainSourceCode_Resources.TreeNode>();

            var viewsNode = new RevitRibbon_MainSourceCode_Resources.TreeNode { Header = "Views" };
            viewsNode.Children.AddRange(GetDependentsViewsTree(doc));
            treeNodes.Add(viewsNode);

            //// Uncomment this part if you want to show the tree view for Sheets
            //var sheetsNode = new TreeNode { Header = "Sheets" };
            //sheetsNode.Children.AddRange(GetSheetsTree(doc));
            //treeNodes.Add(sheetsNode);

            return treeNodes;
        }
        public static List<RevitRibbon_MainSourceCode_Resources.TreeNode> GetDependentsViewsTree(Document doc)
        {
            var treeNodes = new List<RevitRibbon_MainSourceCode_Resources.TreeNode>();

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
                var viewTypeNode = new RevitRibbon_MainSourceCode_Resources.TreeNode { Header = group.Key.ToString() };

                foreach (var view in group)
                {
#if REVIT2020 || REVIT2021 || REVIT2022 || REVIT2023
                    int primaryViewIdValue = view.GetPrimaryViewId().IntegerValue;
#else
                    long primaryViewIdValue = view.GetPrimaryViewId().Value;
#endif

                    // Check if the view is independent and has dependents
                    foreach (var dv in allViews)
                    {
#if REVIT2020 || REVIT2021 || REVIT2022 || REVIT2023
                        int dependentViewPrimaryId = dv.GetPrimaryViewId().IntegerValue;
                        int viewIdValue = view.Id.IntegerValue;
#else
                        long dependentViewPrimaryId = dv.GetPrimaryViewId().Value;
                        long viewIdValue = view.Id.Value;
#endif

                        if (primaryViewIdValue == -1 && dependentViewPrimaryId == viewIdValue)
                        {
                            var viewNode = new RevitRibbon_MainSourceCode_Resources.TreeNode
                            {
                                Header = view.Name,
                                ViewId = view.Id,
                                Children = new List<RevitRibbon_MainSourceCode_Resources.TreeNode>() // Initialize Children
                            };

                            // Add dependent views as children
                            var dependentViews = allViews.Where(depView => depView.GetPrimaryViewId().Equals(view.Id));
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
        private static List<View> GetAllViews(Document doc)
        {
            return new FilteredElementCollector(doc)
                            .OfClass(typeof(View))
                            .Cast<View>()
                            .Where(v => !v.IsTemplate)
                            .ToList();
        }

        private List<Element> GetDimensionsInsideCropBox(View view)
        {
            List<Element> dimensionsInsideCropBox = new List<Element>();

            // Step 1: Get all dimensions in the current view
            FilteredElementCollector collector = new FilteredElementCollector(view.Document, view.Id);
            List<Element> allDimensions = collector.OfClass(typeof(Dimension)).ToList();

            // Step 2: Check if each dimension is inside the CropBox
            foreach (var dimension in allDimensions)
            {
                // Ensure the dimension and its bounding box are not null
                if (dimension == null || dimension.get_BoundingBox(view) == null)
                    continue;

                BoundingBoxXYZ dimensionBoundingBox = dimension.get_BoundingBox(view);

                // Ensure the CropBox is not null
                if (view.CropBoxActive && view.CropBox != null)
                {
                    BoundingBoxXYZ cropBoxBoundingBox = view.CropBox;

                    //if (IsBoundingBoxInsideBoundingBox(dimensionBoundingBox, cropBoxBoundingBox))
                    if (IsCenterOfBoundingBoxInsideBoundingBox(dimensionBoundingBox, cropBoxBoundingBox))
                    {
                        dimensionsInsideCropBox.Add(dimension);
                    }
                }
            }

            return dimensionsInsideCropBox;
        }
        private bool IsCenterOfBoundingBoxInsideBoundingBox(BoundingBoxXYZ innerBox, BoundingBoxXYZ outerBox)
        {
            // Calculate the center point of the innerBox
            XYZ innerBoxCenter = (innerBox.Min + innerBox.Max) / 2;

            // Check if the center of the innerBox is inside the outerBox
            bool xInside = innerBoxCenter.X >= outerBox.Min.X && innerBoxCenter.X <= outerBox.Max.X;
            bool yInside = innerBoxCenter.Y >= outerBox.Min.Y && innerBoxCenter.Y <= outerBox.Max.Y;
            //bool zInside = innerBoxCenter.Z >= outerBox.Min.Z && innerBoxCenter.Z <= outerBox.Max.Z;

            // If the center point is inside the outerBox in all dimensions, return true
            var trueOrFalse = xInside && yInside;// && zInside;
            return trueOrFalse;
        }

        private bool IsBoundingBoxInsideBoundingBox(BoundingBoxXYZ innerBox, BoundingBoxXYZ outerBox)
        {
            // Check if the innerBox is completely inside the outerBox
            bool xInside = innerBox.Min.X >= outerBox.Min.X && innerBox.Max.X <= outerBox.Max.X;
            bool yInside = innerBox.Min.Y >= outerBox.Min.Y && innerBox.Max.Y <= outerBox.Max.Y;
            //bool zInside = innerBox.Min.Z >= outerBox.Min.Z && innerBox.Max.Z <= outerBox.Max.Z;

            // Check if all dimensions are inside the crop box
            //return xInside && yInside && zInside;
            return xInside && yInside;
        }

        private void HideDimensionsInDependentView(Document doc, View dependentView, List<Element> dimensions)
        {
            // Check if the dependentView is a dependent view
            if (dependentView == null)
            {
                MyUtils.M_MyTaskDialog("Error", "The provided view is null.", "Error");
                //TaskDialog.Show("Error", "The provided view is null.");
                return;
            }

            // Iterate through each dimension element and hide it in the dependent view
            foreach (Element dimension in dimensions)
            {
                // Check if the dimension element is null
                if (dimension == null)
                    continue; // Skip null dimension elements

                // 		FamilyName	"Linear Dimension Style"	string
                //if (dimension.Name == "Linear - 3/32\" Arial")
                if (dimension.Name == "GRID DIMENSIONS")
                {
                    dependentView.HideElements(new List<ElementId> { dimension.Id });
                    DimensionsHiden += 1; // update the global property variable

                    // if the CleanedDependentView does not contain the current dependentView, add it - this will be used to count the number of dependent views cleaned
                    if (!CleanedDependentViewIds.Contains(dependentView.Id))
                        CleanedDependentViewIds.Add(dependentView.Id);
                }
            }
        }

    }
}
