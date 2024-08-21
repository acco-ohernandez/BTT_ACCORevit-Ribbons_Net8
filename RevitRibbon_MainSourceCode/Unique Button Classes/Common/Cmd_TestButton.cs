// Version 1.1.0 2023-06-22
using System;
using System.Collections.Generic;

using Autodesk.Revit.Attributes;
// Added Revit External Libraries

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Microsoft.VisualBasic;

namespace RevitRibbon_MainSourceCode
{
    // Transaction required when using IExternalCommand for Revit
    [Transaction(TransactionMode.Manual)] // Starts a new transaction manually for every process that modifies the Revit database
    [Regeneration(RegenerationOption.Manual)]
    public class Cmd_TestButton : IExternalCommand // Implementing IExternalCommand gets Revit to recognize our plugins
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            try
            {
                // Code here
#if REVIT2020
                TaskDialog.Show("INFO", "This is for Revit 2020");
#elif REVIT2021
                TaskDialog.Show("INFO", "This is for Revit 2021");
#elif REVIT2022
            TaskDialog.Show("INFO", "This is for Revit 2022");
#elif REVIT2023
            TaskDialog.Show("INFO", "This is for Revit 2023");
#elif REVIT2024
            TaskDialog.Show("INFO", "This is for Revit 2024");
#elif REVIT2025
                TaskDialog.Show("INFO", "This is for Revit 2025");


#endif

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

    }
}
