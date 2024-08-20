using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace RevitRibbon_MainSourceCode
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_DimensionTypes_Export : IExternalCommand
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
                Document doc = uidoc.Document;

                // Get all DimensionTypes
                var dimensionTypes = new FilteredElementCollector(doc)
                    .OfClass(typeof(DimensionType))
                    .Cast<DimensionType>()
                    .Select(dimType => new ExportersCommonTools.DimensionData
                    {
                        FamilyName = dimType.FamilyName,
                        DimensionTypeName = dimType.Name
                    })
                    .OrderBy(dimType => dimType.FamilyName)
                    .ToList();
                // Remove duplicates Types
                dimensionTypes = dimensionTypes
                                .GroupBy(d => d.DimensionTypeName) // Group by Type
                                .Select(g => g.First()) // Flatten the groups to preserve entries with the same FamilyName
                                .ToList(); // Convert the result back to a List

                //Debug.Print
                // Print DimensionType names
                foreach (var dimensionType in dimensionTypes)
                {
                    Debug.Print($"Family: {dimensionType.FamilyName} === DimensionType Name: {dimensionType.DimensionTypeName}");
                }

                // Specify the file name
                string fileName = "Dimension Styles.xlsx";
                // Call the export method
                ExportersCommonTools.ExportDimensionDataToEPPlusWithSaveAs(fileName, dimensionTypes);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
