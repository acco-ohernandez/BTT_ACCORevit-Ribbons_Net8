//// Version 1.0.0 2023-02-28
//using System;
//using System.Collections.Generic;

//using Autodesk.Revit.Attributes;
//// Added Revit External Libraries

//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;

//using RevitRibbon_MainSourceCode.Forms;

using RevitRibbon_MainSourceCode_Resources.Forms;

namespace RevitRibbon_MainSourceCode
{
    // Transaction required when using IExternalCommand for Revit
    [Transaction(TransactionMode.Manual)] // Starts a new transaction manually for every process that modifies the Revit database
    [Regeneration(RegenerationOption.Manual)]
    public class Cmd_ScopeBoxGrid : IExternalCommand // Implementing IExternalCommand gets Revit to recognize our plugins
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            // Check current view is in focus
            if (MyUtils.CheckCurrentViewFocus(doc, "Action Required", "Please select a single Scope Box before proceeding.", "Warning") == false)
                return Result.Cancelled;

            try
            {
                // Get the user's drawn scope box using the provided method
                Element userDrawnScopeBox = MyUtils.GetSelectedScopeBox(doc, uiapp);

                // Check if a valid scope box was selected
                if (userDrawnScopeBox == null)
                {
                    return Result.Failed; // If not no valid scope box selected, Teminate the command
                }

                // Get the dimensions of the userScopeBoxBoundingBox
                BoundingBoxXYZ userScopeBoxBoundingBox = userDrawnScopeBox.get_BoundingBox(null);
                double newScopeBoxX = userScopeBoxBoundingBox.Max.X - userScopeBoxBoundingBox.Min.X;
                double newScopeBoxY = userScopeBoxBoundingBox.Max.Y - userScopeBoxBoundingBox.Min.Y;

                // ================== Form data ===================
                // Access the controls from the form
                ScopeBoxGridForm form = new ScopeBoxGridForm();
                form.txtBaseScopeBoxName.Text = userDrawnScopeBox.Name;
                form.txtHorizontalOverlap.Text = MyUtils.GetViewScaleMultipliedValue(doc.ActiveView, 48, 5).ToString();
                form.txtVerticalOverlap.Text = MyUtils.GetViewScaleMultipliedValue(doc.ActiveView, 48, 5).ToString();
                form.lblCurrenViewScale.Content = MyUtils.GetViewScaleString(doc.ActiveView);
                form.txtHorizontalOverlap.IsEnabled = false;
                form.txtVerticalOverlap.IsEnabled = false;
                form.ShowDialog();
                if (form.DialogResult != true) return Result.Cancelled; // If the user cancels the form, terminate the command

                // Read values from textboxes
                int rows = int.Parse(form.txtRows.Text);
                int columns = int.Parse(form.txtColumns.Text);
                double HorizontalFeetOverlap = double.Parse(form.txtHorizontalOverlap.Text);
                double VerticalFeetOverlap = double.Parse(form.txtVerticalOverlap.Text);
                string scopeBoxBaseName = form.txtBaseScopeBoxName.Text;
                // ================== Form data End===================
                var listOfScopeBoxesCreated = new List<Element>();
                listOfScopeBoxesCreated.Add(userDrawnScopeBox);

                using (Transaction transaction = new Transaction(doc))
                {
                    // Start the transaction to create inner scope boxes and delete the original one
                    transaction.Start("Create Grid Scope Boxes and Delete Original");
                    userDrawnScopeBox.Name = scopeBoxBaseName;

                    // Calculate the starting point for rows and columns
                    XYZ startingPoint = new XYZ(0, 0, 0);

                    // Iterate through rows and columns to create the grid of scope boxes
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < columns; j++)
                        {
                            if (i == 0 && j == 0) continue; // Skip first copy to avoid copying over the existing ScopeBox

                            // Calculate the origin for the new scope box with overlaps
                            // Note: Adding j * (newScopeBoxX - HorizontalFeetOverlap) to move Horizontally
                            // Note: Subtracting i * (newScopeBoxY - VerticalFeetOverlap) to move vertically
                            XYZ origin = new XYZ(
                                                    startingPoint.X + j * (newScopeBoxX - HorizontalFeetOverlap),
                                                    startingPoint.Y - i * (newScopeBoxY - VerticalFeetOverlap),
                                                    startingPoint.Z
                                                );

                            // Use ElementTransformUtils.CopyElements to replicate the outer scope box
                            ICollection<ElementId> copiedScopeBoxes = ElementTransformUtils.CopyElements(
                                                                            doc,
                                                                            new List<ElementId> { userDrawnScopeBox.Id },
                                                                            XYZ.Zero
                                                                        );

                            // Iterate through the copied scope boxes to move them to their respective positions
                            foreach (ElementId copiedScopeBoxId in copiedScopeBoxes)
                            {
                                Element copiedScopeBox = doc.GetElement(copiedScopeBoxId);
                                //copiedScopeBox.Name = $"{scopeBoxBaseName} {nameChar}";  // Rename New Scope Box
                                //nameChar++; // Increase the Char value for nameChar
                                ElementTransformUtils.MoveElement(doc, copiedScopeBoxId, origin); // Move the new Scope Box
                                listOfScopeBoxesCreated.Add(copiedScopeBox);
                            }
                        }
                    }

                    // Delete the original user-drawn scope box
                    //doc.Delete(userDrawnScopeBox.Id);

                    // Commit the transaction
                    transaction.Commit();
                }
            }
            catch (Exception ex) // Catch errors
            {
                TaskDialog exceptStatus = new TaskDialog(ex.GetType().FullName.ToString());
                exceptStatus.MainInstruction = ex.Message;
                exceptStatus.Show();

                return Result.Failed;
            }

            // Return the result indicating success
            return Result.Succeeded;
        }

    }
}
