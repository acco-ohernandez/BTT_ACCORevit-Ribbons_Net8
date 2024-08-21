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

namespace RevitRibbon_MainSourceCode//.Unique_Button_Classes.ConTech
{
    [Transaction(TransactionMode.Manual)]
    // This class is not linked to any button in the Ribbon as per Perry's request
    //It is used to export analytical model object styles to Excel.
    public class Cmd_AnalyticalModelObjStyles_Export : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            try
            {
                UIApplication uiapp = commandData.Application;
                UIDocument uidoc = uiapp.ActiveUIDocument;
                var app = uiapp.Application;
                Document doc = uidoc.Document;

                // Get all AnalyticalModel categories
                List<Category> typeCategories = ExportersCommonTools.GetCategoriesByType(doc, CategoryType.AnalyticalModel);

                if (typeCategories != null && typeCategories.Any())
                {
                    // Create a list to store data for exporting to EPPlus
                    List<ExportersCommonTools.ExportData> exportDataList = new List<ExportersCommonTools.ExportData>();

                    // Export annotation object styles for each category, including sub-settings
                    foreach (Category category in typeCategories)
                    {
                        Debug.Print($"{category.Name} {category.LineColor.Red}-{category.LineColor.Green}-{category.LineColor.Blue}");

                        // Export object styles and add data to the list
                        //ExportersCommonTools.ExportObjectStyles(doc, category, exportDataList);
                        ExportersCommonTools.ExportModelObjectStyles(doc, category, exportDataList);


                        // Export sub-settings for this category
                        ExportersCommonTools.ExportSubSettings(doc, category, exportDataList);
                    }

                    // Export data to EPPlus with Save As functionality
                    string categoryName = typeCategories[0].CategoryType.ToString();
                    ExportersCommonTools.ExportModelDataToEPPlusWithSaveAs(exportDataList, $"Object Styles - {categoryName} Objects");
                }
                else
                {
                    MyUtils.M_MyTaskDialog("No Annotation Categories", "There are no annotation categories in the document.");
                }

                // Return a success result
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        // Helper method to get all annotation categories, including sub-settings


    }
}
