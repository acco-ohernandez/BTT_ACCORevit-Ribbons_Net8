#region Namespaces
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;

//using Autodesk.Revit.Attributes;
//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
//using Autodesk.Revit.UI.Selection;

//using RevitRibbon_MainSourceCode.Forms;
using RevitRibbon_MainSourceCode_Resources.Forms;
#endregion

namespace RevitRibbon_MainSourceCode //.Unique_Button_Classes.BIM_Team.NewProjectSetups
{
    // Transaction required when using IExternalCommand for Revit
    [Transaction(TransactionMode.Manual)] // Starts a new transaction manually for every process that modifies the Revit database
    [Regeneration(RegenerationOption.Manual)]
    public class Cmd_RenameScopeBoxes : IExternalCommand // Implementing IExternalCommand gets Revit to recognize our plugins
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            try
            {
                // Lists to store selected and preselected elements
                var pickedElemsList = new List<Element>();
                var preselectedElemsList = GetSelectedScopeBoxes(doc);

                // Check if there are preselected elements; if not, allow the user to pick
                if (preselectedElemsList.Count != 0)
                    pickedElemsList = preselectedElemsList;
                else
                {
                    // Show info message to the user
                    MyUtils.M_MyTaskDialog("Instructions", "1. Select Scope Boxes in the desired order.\n2. Press ESC when finished selecting.\n\nEach Scope Box you select will be added to the list to be renamed.", "Information");
                    pickedElemsList = PickScopeBoxes(uidoc, doc);
                }

                // Check if there are selected elements; if not, cancel the command
                if (pickedElemsList == null)
                    return Result.Cancelled;

                // Display the RenameScopeBoxesForm to the user
                var renameScopeBoxesForm = new RenameScopeBoxesForm(pickedElemsList);
                renameScopeBoxesForm.ShowDialog();

                // Check if the user clicked the Rename button; if not, cancel the process
                if (renameScopeBoxesForm.DialogResult != true)
                    return Result.Cancelled;

                // Get the list of new names as a string list
                //var newNamesList = renameScopeBoxesForm.lbNewNames.Items.Cast<string>().ToList();
                var newNamesList = renameScopeBoxesForm.NewNames;//.Items.Cast<string>().ToList();


                // Get the list of elements from the form with any new list order
                //var returnedNewNamesElementList = renameScopeBoxesForm.lbOriginalNames.Items.Cast<Element>().ToList();
                var returnedNewNamesElementList = renameScopeBoxesForm.OriginalNames;


                // Rename the scope boxes using a transaction
                using (Transaction transaction = new Transaction(doc))
                {
                    transaction.Start("Rename Scopeboxes");

                    // Iterate through the selected elements and update their names
                    for (int i = 0; i < pickedElemsList.Count; i++)
                    {
                        returnedNewNamesElementList[i].Name = newNamesList[i];
                    }

                    transaction.Commit();
                }
                // output the list of new names to a task dialog
                // output the list of new names to a task dialog
                MyUtils.M_MyTaskDialog("Scope Boxes Renamed", $"Renamed Scope Boxes: {newNamesList.Count}");

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


        private List<Element> PickScopeBoxes(UIDocument uidoc, Document doc)
        {
            HashSet<ElementId> uniqueElementIds = new HashSet<ElementId>();
            //List<Element> pickedElemsList = null;
            List<Element> pickedElemsList = new List<Element>();
            bool flag = true;
            int c = 0;

            while (flag)
            {
                try
                {
                    // Prompt user to pick a scope box
                    Reference reference = uidoc.Selection.PickObject(ObjectType.Element, "Pick scope boxes in the desired order. Press ESC to stop picking.");

                    // Access the element using reference.ElementId
                    Element element = doc.GetElement(reference.ElementId);

                    if (IsScopeBox(element))
                    {
                        // Check for duplicates using HashSet
                        if (uniqueElementIds.Add(reference.ElementId))
                        {
                            // If ElementId is not a duplicate, add the reference to the list
                            pickedElemsList.Add(element);
                            c++;
                            // Do something with the picked element
                            Debug.Print($"========>{c}: {element.Name}");
                        }
                        else
                        {
                            MyUtils.M_MyTaskDialog("Warning", "Duplicate scope box selected. Ignoring the duplicate.");
                        }
                    }
                    else
                    {
                        MyUtils.M_MyTaskDialog("Selection Required", "You must only select Scope Boxes before proceeding.", "Warning");
                        throw new Exception();
                    }
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    // User pressed ESC or canceled the operation
                    flag = false;
                }
                catch (Exception)
                {
                    // Handle specific exceptions or log errors
                    // You may choose to return Result.Failed here if necessary
                }
            }

            if (pickedElemsList.Count != 0)
                return pickedElemsList;
            return null;
        }

        public static bool IsScopeBox(Element element)
        {
            // Check if the element is a scope box
            return element != null && element.Category != null && element.Category.Name == "Scope Boxes";
        }


        public static List<Element> GetSelectedScopeBoxes(Document doc)
        {
            List<Element> scopeBoxes = new List<Element>();

            // Get the handle of current document.
            UIDocument uidoc = new UIDocument(doc);

            // Get the element selection of the current document.
            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();

            // Iterate through the selected element IDs and add scope boxes to the list
            foreach (ElementId id in selectedIds)
            {
                Element element = doc.GetElement(id);
                if (IsScopeBox(element))
                {
                    scopeBoxes.Add(element);
                }
            }

            return scopeBoxes;
        }

    }
}
