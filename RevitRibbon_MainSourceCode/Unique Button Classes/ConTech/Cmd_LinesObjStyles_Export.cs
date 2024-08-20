using System.Windows.Forms;

using OfficeOpenXml;

namespace RevitRibbon_MainSourceCode//.Unique_Button_Classes.ConTech
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_LinesObjStyles_Export : IExternalCommand
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

                // Get all Model categories
                List<Category> typeCategories = ExportersCommonTools.GetCategoriesByType(doc, CategoryType.Model);
                typeCategories.RemoveAll(c => c.Name != "Lines");

                if (typeCategories != null && typeCategories.Any())
                {
                    // Create a list to store data for exporting to EPPlus
                    List<ExportersCommonTools.ExportData> exportDataList = new List<ExportersCommonTools.ExportData>();

                    // Export annotation object styles for each category, including sub-settings
                    foreach (Category category in typeCategories)
                    {
                        Debug.Print($"{category.Name} {category.LineColor.Red}-{category.LineColor.Green}-{category.LineColor.Blue}");

                        // Export object styles and add data to the list
                        ExportersCommonTools.ExportObjectStyles(doc, category, exportDataList);


                        // Export sub-settings for this category
                        ExportersCommonTools.ExportSubSettings(doc, category, exportDataList);
                    }

                    // Export data to EPPlus with Save As functionality
                    //string categoryName = typeCategories[0].CategoryType.ToString();
                    string categoryName = "Lines";
                    ExportersCommonTools.ExportDataToEPPlusWithSaveAs(exportDataList, categoryName + " Styles");
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