using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using OfficeOpenXml;

using RevitRibbon_MainSourceCode;

namespace RevitRibbon_MainSourceCode //.Unique_Button_Classes.ConTech
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_Reset3DView : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            var app = uiapp.Application;
            Document doc = uidoc.Document;

            // Get the current view
            var curView = doc.ActiveView;

            try
            {
                using (Transaction t = new Transaction(doc, "Reset 3D View"))
                {
                    t.Start();

                    string successMessage = "Reset of 'View Template' and Hide 'Levels annotation'\nDONE!";
                    string warningMessage = "You must select a 3D View to reset the\nView Template and Hide Levels annotation";

                    if (curView.ViewType == ViewType.ThreeD)
                    {
                        // Debug message for a 3D view
                        Debug.Print(successMessage);

                        // Reset View Template to none
                        curView.ViewTemplateId = ElementId.InvalidElementId;

                        // Find the "Show Levels" annotation category
                        var catAnnotateLevelsSettings = doc.Settings.Categories
                            .Cast<Category>()
                            .FirstOrDefault(c => c.CategoryType == CategoryType.Annotation && c.Name.Contains("Levels"));

                        if (catAnnotateLevelsSettings != null)
                        {
                            // Hide the "Show Levels" annotation category
                            curView.SetCategoryHidden(catAnnotateLevelsSettings.Id, true);

                            //===================================================
                            // Zoom Extents the current View
                            // I made this ZoomToFitActiveView method by referencing this website:
                            // https://forums.autodesk.com/t5/revit-api-forum/getting-screen-coordinate-from-revit-coordinate/m-p/7421655
                            // although they are doing something different, it help understand the UIView class
                            MyUtils.ZoomToFitActiveView(uidoc);


                            // Commit the changes
                            doc.Regenerate();

                            MyUtils.M_MyTaskDialog("Info", successMessage);
                        }
                        else
                        {
                            // Handle the case where the category is not found
                            MyUtils.M_MyTaskDialog("Error", "Category 'Show Levels' not found.");
                        }
                    }
                    else
                    {
                        // Debug message for non-3D views
                        Debug.Print($"The current view is: {curView.ViewType}\n{warningMessage}");
                        MyUtils.M_MyTaskDialog("Warning", warningMessage);
                    }

                    t.Commit(); // Commit the transaction
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;

        }




        public void ZoomToFit(UIView uiView)
        {
            // Check if the UIView is valid
            if (uiView != null && uiView.IsValidObject)
            {
                // Use the ZoomToFit method of the UIView to zoom the view to fit its contents
                uiView.ZoomToFit();
            }
            else
            {
                // Handle the case where the UIView is not valid
                // You can throw an exception or log an error message here
            }
        }
    }
}
