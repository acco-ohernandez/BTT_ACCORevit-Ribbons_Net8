#region Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitRibbon_MainSourceCode;

//using RevitRibbon_MainSourceCode_Resources.Forms;

#endregion

//namespace RevitRibbon_MainSourceCode.Unique_Button_Classes.BIM_Team.NewProjectSetups
namespace RevitRibbon_MainSourceCode
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_CreateBimSetupView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uiapp.ActiveUIDocument.Document;

            //string character = " ";
            //MyUtils.GetUnicodeValue(character);

            var allLevels = MyUtils.GetAllLevels(doc).Where(l => l.Name != "!CAD Link Template").ToList();
            var scalesDictionary = MyUtils.ScalesList();

            var form = new CreateBIMSetupView(allLevels, scalesDictionary);
            if (form.ShowDialog() != true) { return Result.Cancelled; }

            var selectedLevel = form.SelectedLevel;
            var selectedScale = form.SelectedScale;
            //string selectedLevelName = selectedLevel.Name;
            string selectedLevelName = MyUtils.ConvertSpaceToAlt255(selectedLevel.Name);

            using (Transaction trans = new Transaction(doc, "Create BIM Setup View"))
            {
                trans.Start();
                string viewName = MyUtils.GetUniqueViewName(doc, $"BIM Setup View - {selectedLevelName}");
                ViewPlan view = MyUtils.CreateFloorPlanView(doc, viewName, selectedLevel);
                view.Scale = selectedScale;

                // This can be commented out if you don't want to set the Category and SubCategory on the Project Browser
                MyUtils.SetViewBrowserCategory(view);

                trans.Commit();
                uidoc.ActiveView = view;
            }

            return Result.Succeeded;

        }

    }
}
