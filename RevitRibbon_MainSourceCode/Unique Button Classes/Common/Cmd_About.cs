// Version 1.0.0 2023-02-28
using Microsoft.VisualBasic;

namespace RevitRibbon_MainSourceCode
{
    // Transaction required when using IExternalCommand for Revit
    [Transaction(TransactionMode.Manual)] // Starts a new transaction manually for every process that modifies the Revit database
    [Regeneration(RegenerationOption.Manual)]
    public class Cmd_About : IExternalCommand // Implementing IExternalCommand gets Revit to recognize our plugins
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
                // TaskDialog.Show("ConTech Tab Info", "Version 1.0.0 2023-02-28");

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

