
namespace RevitRibbon_MainSourceCode
{
    // Transaction required when using IExternalCommand for Revit
    [Transaction(TransactionMode.Manual)]
    public class Cmd_CreateViewReferencesDuplicates : IExternalCommand
    {
        public int ViewReferenceCopiesCount { get; private set; } = 0;
        public BoundingBoxXYZ BoundingBoxContaingAllScopeboxes { get; set; } = null;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            Element viewReference = GetFirstViewReference(doc);
            if (viewReference == null)
            {
                MyUtils.M_MyTaskDialog("Action Required", "Please place one View Reference somewhere in the active view before proceeding.", "Warning");
                //TaskDialog.Show("Error", "No View Reference found in the current view.");
                return Result.Failed;
            }

            List<Element> selectedScopeBoxes = GetSelectedScopeBoxes(doc);
            if (selectedScopeBoxes.Count < 2)
            {
                MyUtils.M_MyTaskDialog("Selection Required", "Please select a minimum of two overlapping Scope Boxes before proceeding.", "Warning");
                //TaskDialog.Show("Error", "You must select at least 2 overlapped Scope Boxes.");
                return Result.Failed;
            }

            try
            {
                // ------------------     Main process call     -----------------------
                PlaceViewReferences(doc, selectedScopeBoxes, viewReference);
            }
            catch (Exception ex)
            {
                MyUtils.M_MyTaskDialog("Error", $"An error occurred: {ex.Message}", "Error");
                //TaskDialog.Show("Error", $"An error occurred: {ex.Message}");
                return Result.Failed;
            }

            MyUtils.M_MyTaskDialog("Place View References", $"{ViewReferenceCopiesCount} View References placed successfully.", true);
            //TaskDialog.Show("Success", $"{ViewReferenceCopiesCount} View References placed successfully.");
            return Result.Succeeded;
        }

        private Element GetFirstViewReference(Document doc)
        {
            return new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_ReferenceViewer)
                .WhereElementIsNotElementType()
                .FirstOrDefault();
        }

        private List<Element> GetSelectedScopeBoxes(Document doc)
        {
            return Cmd_RenameScopeBoxes.GetSelectedScopeBoxes(doc);
        }

        private void PlaceViewReferences(Document doc, List<Element> selectedScopeBoxes, Element viewReference)
        {
            using (Transaction trans = new Transaction(doc, "Copy and Place View References"))
            {
                trans.Start();
                SetBoundingBoxContaingAllScopeboxes(selectedScopeBoxes); // update the global property BoundingBoxContaingAllScopeboxes

                foreach (Element scopeBox in selectedScopeBoxes)
                {
                    ProcessScopeBoxForViewReferenceInsertion(doc, scopeBox, selectedScopeBoxes, viewReference);
                }

                // Delete the original view reference
                doc.Delete(viewReference.Id);

                trans.Commit();
            }
        }

        private void ProcessScopeBoxForViewReferenceInsertion(Document doc, Element scopeBox, List<Element> selectedScopeBoxes, Element viewReference)
        {
            List<XYZ> insertPoints = GetBoxFourInsertPoints(doc, scopeBox);
            List<Element> otherScopeBoxes = selectedScopeBoxes.Except(new[] { scopeBox }).ToList();

            for (int i = 0; i < insertPoints.Count; i++)
            {
                var insertPoint = insertPoints[i];
                ProcessInsertPointForViewReferenceInsertion(doc, insertPoint, scopeBox, otherScopeBoxes, viewReference, i);
            }
        }

        private void ProcessInsertPointForViewReferenceInsertion(Document doc, XYZ insertPoint, Element scopeBox, List<Element> otherScopeBoxes, Element viewReference, int pointNum)
        {
            List<string> overlapDirections = CheckOverlapsForCorner(insertPoint, otherScopeBoxes, scopeBox.get_BoundingBox(null));

            double x = MyUtils.GetViewScaleMultipliedValue(doc.ActiveView, 48, 2.0); // Dynamicaly sets the X offset based on the view scale
            double y = MyUtils.GetViewScaleMultipliedValue(doc.ActiveView, 48, 2.0); // Dynamicaly sets the Y offset based on the view scale

            var Offset_X = new XYZ(x, 0, 0);
            var Offset_Y = new XYZ(0, y, 0);

            if (overlapDirections.Contains("Horizontal"))
            {
                //bool scopeboxIsSurrounded = ScopeBoxHasTopBottomLeftRighSurroundingScopeboxes(scopeBox, otherScopeBoxes);
                if (CheckVerticalBoxOverlaps(insertPoint, scopeBox, otherScopeBoxes, doc.ActiveView))
                {
                    var newInsertPoint = (insertPoint - GetElementCenter(doc, viewReference));
                    if (IsLeftBox(scopeBox))
                    {
                        if (pointNum == 0 || pointNum == 2)
                        {
                            newInsertPoint += Offset_X;

                            InsertViewReference(doc, viewReference.Id, newInsertPoint);
                        }
                    }

                    if (IsRightBox(scopeBox))
                    {
                        if (pointNum == 1 || pointNum == 3)
                        {
                            newInsertPoint -= Offset_X;

                            InsertViewReference(doc, viewReference.Id, newInsertPoint);
                        }
                    }

                }
            }
            //if (overlapDirections.Contains("Horizontal"))
            //{
            //    if (CheckVerticalBoxOverlaps(insertPoint, scopeBox, otherScopeBoxes, doc.ActiveView))
            //    {
            //        var newInsertPoint = (insertPoint - GetElementCenter(doc, viewReference));
            //        if (pointNum == 0 || pointNum == 2) { newInsertPoint += Offset_X; } // Left Top and Bottom Points OffSet
            //        if (pointNum == 1 || pointNum == 3) { newInsertPoint -= Offset_X; } // Right Top and Bottom Points OffSet
            //        //InsertViewReference(doc, viewReference.Id, insertPoint - GetElementCenter(doc, viewReference));
            //        InsertViewReference(doc, viewReference.Id, newInsertPoint);
            //    }
            //}

            if (overlapDirections.Contains("Vertical"))
            {
                if (CheckHorizontalBoxOverlaps(insertPoint, scopeBox, otherScopeBoxes, doc.ActiveView))
                {
                    var newInsertPoint = (insertPoint - GetElementCenter(doc, viewReference));
                    if (pointNum == 0 || pointNum == 1) { newInsertPoint -= Offset_Y; } // Left and Right Top Points OffSet
                    if (pointNum == 2 || pointNum == 3) { newInsertPoint += Offset_Y; } // Left and Right Bottom  Points OffSet
                    //ElementId newVerticalViewRefId = InsertViewReference(doc, viewReference.Id, insertPoint - GetElementCenter(doc, viewReference));
                    ElementId newVerticalViewRefId = InsertViewReference(doc, viewReference.Id, newInsertPoint);

                    RotateElementFromCenter(doc, newVerticalViewRefId);
                }
            }
        }
        bool IsLeftBox(global::Autodesk.Revit.DB.Element scopeBox)
        {
            BoundingBoxXYZ scopeBoxBBox = scopeBox.get_BoundingBox(null);
            if (scopeBoxBBox.Min.X == BoundingBoxContaingAllScopeboxes.Min.X)
                return true;

            return false;
        }
        bool IsRightBox(global::Autodesk.Revit.DB.Element scopeBox)
        {
            BoundingBoxXYZ scopeBoxBBox = scopeBox.get_BoundingBox(null);
            if (scopeBoxBBox.Max.X == BoundingBoxContaingAllScopeboxes.Max.X)
                return true;

            return false;
        }

        // Determine if the scopebox has lateral or top and bottom copeboxes sourounding it by 
        // checking if the scopeBox Center is vertically or horizontally align with four other scope boxes. 
        public bool ScopeBoxHasTopBottomLeftRightSurroundingScopeboxes(Element scopeBox, List<Element> otherScopeBoxes)
        {
            BoundingBoxXYZ scopeBoxBBox = scopeBox.get_BoundingBox(null);
            XYZ scopeBoxCenter = (scopeBoxBBox.Min + scopeBoxBBox.Max) / 2;

            bool hasLeft = false, hasRight = false, hasTop = false, hasBottom = false;

            foreach (Element otherBox in otherScopeBoxes)
            {
                if (otherBox.Id == scopeBox.Id) continue; // Skip comparison with itself

                BoundingBoxXYZ otherBoxBBox = otherBox.get_BoundingBox(null);
                XYZ otherBoxCenter = (otherBoxBBox.Min + otherBoxBBox.Max) / 2;

                // Check horizontal alignment (left/right)
                if (otherBoxCenter.Y == scopeBoxCenter.Y)
                {
                    if (otherBoxCenter.X < scopeBoxCenter.X) hasLeft = true;
                    else if (otherBoxCenter.X > scopeBoxCenter.X) hasRight = true;
                }

                // Check vertical alignment (top/bottom)
                if (otherBoxCenter.X == scopeBoxCenter.X)
                {
                    if (otherBoxCenter.Y > scopeBoxCenter.Y) hasTop = true;
                    else if (otherBoxCenter.Y < scopeBoxCenter.Y) hasBottom = true;
                }

                // If all four sides are found to be surrounded, exit early
                if (hasLeft && hasRight && hasTop && hasBottom) return true;
            }
            bool isSurrounded = hasLeft && hasRight && hasTop && hasBottom;
            return isSurrounded;
        }


        public bool CheckIfboxLateral(Element scopeBox)
        {
            BoundingBoxXYZ scopeBoxBBox = scopeBox.get_BoundingBox(null);

            // Check if the scopeBox's Min and Max X values align with the encompassing bounding box's Min and Max X values.
            bool isLateral = scopeBoxBBox.Min.X == BoundingBoxContaingAllScopeboxes.Min.X ||
                             scopeBoxBBox.Max.X == BoundingBoxContaingAllScopeboxes.Max.X;

            return isLateral;
        }


        private bool CheckHorizontalBoxOverlaps(XYZ insertPoint, Element scopeBox, List<Element> otherScopeBoxes, View view)
        {
            BoundingBoxXYZ scopeBoxBBox = scopeBox.get_BoundingBox(view);
            bool isPointInsideScopeBox = IsPointInsideBox(insertPoint, scopeBoxBBox);

            if (!isPointInsideScopeBox)
            {
                return false;  // The point isn't even inside the primary scope box.
            }

            // Check against all other scope boxes to find a horizontal overlap
            foreach (Element otherBox in otherScopeBoxes)
            {
                BoundingBoxXYZ otherBoxBBox = otherBox.get_BoundingBox(view);

                // Check if the insertPoint is inside otherBox in the X direction
                if (IsPointInsideBox(insertPoint, otherBoxBBox))
                {
                    // Check for horizontal overlap: They overlap horizontally if their Y ranges intersect
                    if (DoBoxesAlignHorizontally(scopeBoxBBox, otherBoxBBox))
                    {
                        return true;  // Found a horizontal overlap
                    }
                }
            }

            return false;  // No horizontal overlaps found
        }

        private bool CheckVerticalBoxOverlaps(XYZ insertPoint, Element scopeBox, List<Element> otherScopeBoxes, View view)
        {
            BoundingBoxXYZ scopeBoxBBox = scopeBox.get_BoundingBox(view);
            bool isPointInsideScopeBox = IsPointInsideBox(insertPoint, scopeBoxBBox);

            if (!isPointInsideScopeBox)
            {
                return false;  // The point isn't even inside the primary scope box.
            }

            // Check against all other scope boxes to find a horizontal overlap
            foreach (Element otherBox in otherScopeBoxes)
            {
                BoundingBoxXYZ otherBoxBBox = otherBox.get_BoundingBox(view);

                // Check if the insertPoint is inside otherBox in the X direction
                if (IsPointInsideBox(insertPoint, otherBoxBBox))
                {
                    // Check for horizontal overlap: They overlap horizontally if their Y ranges intersect
                    if (DoBoxesAlignVertically(scopeBoxBBox, otherBoxBBox))
                    {
                        return true;  // Found a horizontal overlap
                    }
                }
            }

            return false;  // No horizontal overlaps found
        }

        private bool IsPointInsideBox(XYZ point, BoundingBoxXYZ bbox)
        {
            return point.X >= bbox.Min.X && point.X <= bbox.Max.X &&
                   point.Y >= bbox.Min.Y && point.Y <= bbox.Max.Y;// &&
                                                                  //point.Z >= bbox.Min.Z && point.Z <= bbox.Max.Z;
        }
        private bool DoBoxesAlignHorizontally(BoundingBoxXYZ box1, BoundingBoxXYZ box2)
        {
            XYZ box1Center = (box1.Min + box1.Max) / 2;
            XYZ box2Center = (box2.Min + box2.Max) / 2;

            // if box1 and box2 are both in the same horizontal line, they will both have the same Y center point
            return box1Center.Y == box2Center.Y;
        }
        private bool DoBoxesAlignVertically(BoundingBoxXYZ box1, BoundingBoxXYZ box2)
        {
            XYZ box1Center = (box1.Min + box1.Max) / 2;
            XYZ box2Center = (box2.Min + box2.Max) / 2;

            // if box1 and box2 are both in the same vertical line, they will both have the same Y center point
            return box1Center.X == box2Center.X;
        }

        private XYZ GetElementCenter(Document doc, Element element)
        {
            // Get the bounding box of the element in the active view
            BoundingBoxXYZ bbox = element.get_BoundingBox(doc.ActiveView);

            // Check if the bounding box is valid
            if (bbox != null)
            {
                // Calculate the center point of the bounding box
                XYZ center = (bbox.Min + bbox.Max) * 0.5;
                return center;
            }
            else
            {
                throw new InvalidOperationException("Cannot find bounding box for the given element.");
            }
        }

        private ElementId InsertViewReference(Document doc, ElementId viewReferenceId, XYZ translationVector)
        {
            // Start a sub-transaction if necessary to duplicate the View Reference
            // Note: Ensure that the main transaction is already open before calling this method
            var subTrans = new SubTransaction(doc);
            subTrans.Start();

            // Copy the element to the same place, which creates a duplicate
            ElementId copiedViewRefId = ElementTransformUtils.CopyElement(doc, viewReferenceId, XYZ.Zero).FirstOrDefault();

            // Check if the element was copied successfully
            if (copiedViewRefId == null)
            {
                subTrans.RollBack();
                throw new InvalidOperationException("The View Reference could not be copied.");
            }
            ViewReferenceCopiesCount++;

            // Move the copied element to the desired location
            ElementTransformUtils.MoveElement(doc, copiedViewRefId, translationVector);

            subTrans.Commit();

            return copiedViewRefId;
        }

        private List<string> CheckOverlapsForCorner(XYZ cornerPoint, List<Element> allScopeBoxes, BoundingBoxXYZ currentBox)
        {
            List<string> overlaps = new List<string>();

            foreach (Element scopeBox in allScopeBoxes)
            {
                BoundingBoxXYZ bbox = scopeBox.get_BoundingBox(null);
                if (cornerPoint.X > bbox.Min.X && cornerPoint.X < bbox.Max.X &&
                    cornerPoint.Y > bbox.Min.Y && cornerPoint.Y < bbox.Max.Y)
                {
                    // The corner is inside this scope box; now determine the specific overlaps
                    if (cornerPoint.Y >= currentBox.Min.Y && cornerPoint.Y <= currentBox.Max.Y)
                    {
                        overlaps.Add("Horizontal");
                    }
                    if (cornerPoint.X >= currentBox.Min.X && cornerPoint.X <= currentBox.Max.X)
                    {
                        overlaps.Add("Vertical");
                    }
                }
            }

            return overlaps;
        }

        private static List<XYZ> GetBoxFourInsertPoints(Document doc, Element scopeBox)
        {
            ExpandedBoundingBox scopeboxFourCorners = new ExpandedBoundingBox(doc, scopeBox.get_BoundingBox(null));

            // Convert the corners into a list to iterate over
            List<XYZ> insertPoints = new List<XYZ>
                                                    {
                                                        scopeboxFourCorners.LeftTop,
                                                        scopeboxFourCorners.RightTop,
                                                        scopeboxFourCorners.LeftBottom,
                                                        scopeboxFourCorners.RightBottom
                                                    };
            return insertPoints;
        }

        /// <summary>
        /// Rotates elements with bounding box 90 degrees CounterClockWise
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elementId"></param>
        public void RotateElementFromCenter(Document doc, ElementId elementId)
        {
            Element element = doc.GetElement(elementId);
            if (element == null) return;

            // Assuming the element has a bounding box, calculate its center
            //BoundingBoxXYZ bbox = element.get_BoundingBox(null); // null for the active view
            BoundingBoxXYZ bbox = element.get_BoundingBox(doc.ActiveView); // null for the active view
            if (bbox == null) return;

            XYZ bboxCenter = (bbox.Min + bbox.Max) * 0.5;
            Line rotationAxis = Line.CreateBound(bboxCenter, bboxCenter + XYZ.BasisZ);

            double angleRadians = Math.PI / 2; // 90 degrees counterclockwise

            if (element.Location is Location location)
            {
                bool rotated = location.Rotate(rotationAxis, angleRadians);

                if (!rotated)
                {
                    TaskDialog.Show("Info", "Rotation failed.");
                }
            }
        }

        public void SetBoundingBoxContaingAllScopeboxes(List<Element> otherScopeBoxes)
        {
            // Initialize min and max points with extreme values so any real point will replace them
            XYZ minPoint = new XYZ(double.MaxValue, double.MaxValue, 0);
            XYZ maxPoint = new XYZ(double.MinValue, double.MinValue, 0);

            foreach (Element box in otherScopeBoxes)
            {
                // Get the bounding box of the current scope box
                BoundingBoxXYZ bbox = box.get_BoundingBox(null);

                // Check and update the minimum X and Y
                if (bbox.Min.X < minPoint.X) minPoint = new XYZ(bbox.Min.X, minPoint.Y, minPoint.Z);
                if (bbox.Min.Y < minPoint.Y) minPoint = new XYZ(minPoint.X, bbox.Min.Y, minPoint.Z);

                // Check and update the maximum X and Y
                if (bbox.Max.X > maxPoint.X) maxPoint = new XYZ(bbox.Max.X, maxPoint.Y, maxPoint.Z);
                if (bbox.Max.Y > maxPoint.Y) maxPoint = new XYZ(maxPoint.X, bbox.Max.Y, maxPoint.Z);
            }

            // Create a new bounding box that encompasses all the provided scope boxes
            BoundingBoxXYZ encompassingBox = new BoundingBoxXYZ();
            encompassingBox.Min = minPoint;
            encompassingBox.Max = new XYZ(maxPoint.X, maxPoint.Y, minPoint.Z + 1); // Add a nominal height to avoid a flat box

            BoundingBoxContaingAllScopeboxes = encompassingBox; // Set gloval property BoundingBoxContaingAllScopeboxes
        }

    }

    public class ExpandedBoundingBox
    {
        public XYZ LeftTop { get; private set; }
        public XYZ RightTop { get; private set; }
        public XYZ LeftBottom { get; private set; }
        public XYZ RightBottom { get; private set; }

        public ExpandedBoundingBox(Document doc, BoundingBoxXYZ boundingBox)
        {
            // Calculate the adjested corners
            CalculateAdjestedCorners(doc, boundingBox);
        }
        private void CalculateAdjestedCorners(Document doc, BoundingBoxXYZ boundingBox)
        {
            // Assuming Z is constant and we're expanding in X and Y directions
            //double expandDistance = 3.5; // 1 foot in Revit's internal units
            double expandDistance = MyUtils.GetViewScaleMultipliedValue(doc.ActiveView, 48, 3.75);  // 1 foot in Revit's internal units

            // Original Min and Max points
            XYZ min = boundingBox.Min;
            XYZ max = boundingBox.Max;

            // Inwards Offset BoundingBox Corner Points 
            LeftBottom = new XYZ(min.X + expandDistance, min.Y + expandDistance, min.Z);
            RightBottom = new XYZ(max.X - expandDistance, min.Y + expandDistance, min.Z);
            LeftTop = new XYZ(min.X + expandDistance, max.Y - expandDistance, max.Z);
            RightTop = new XYZ(max.X - expandDistance, max.Y - expandDistance, max.Z);
        }
    }
}
