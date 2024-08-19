using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Dynamic;
//using System.Runtime.Remoting.Lifetime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Visual;

using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;

using OfficeOpenXml;

//using static OfficeOpenXml.ExcelErrorValue;

namespace RevitRibbon_MainSourceCode
{
    [Transaction(TransactionMode.Manual)]
    public class ExportersCommonTools
    {
        /// <summary>
        /// Helper method to get all Object Style Category Settings.
        /// options available
        /// List<Category> ModelCategories = ExportersCommonTools.GetCategoriesByType(doc, CategoryType.Model);
        /// List<Category> annotationCategories = ExportersCommonTools.GetCategoriesByType(doc, CategoryType.Annotation);
        /// List<Category> analyticalModelCategories = ExportersCommonTools.GetCategoriesByType(doc, CategoryType.AnalyticalModel);
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="categoryType"></param>
        /// <returns></returns>
        public static List<Category> GetCategoriesByType(Document doc, CategoryType categoryType)
        {
            // Get the categories from the document settings
            Categories categories = doc.Settings.Categories;

            // Use LINQ to filter and sort the categories
            List<Category> filteredCategories = categories.Cast<Category>()
                // Filter categories based on the specified CategoryType
                .Where(cat => cat.CategoryType == categoryType)
                // Sort categories by name in ascending order
                .OrderBy(cat => cat.Name)
                // Convert the result to a List<Category>
                .ToList();

            // Return the list of filtered categories
            return filteredCategories;
        }

        // Helper method to export object styles for a category
        public static void ExportObjectStyles(Document doc, Category category, List<ExportData> exportDataList, string prefix = "")
        {
            // Check if the category is null
            if (category == null)
            {
                Debug.Print("Category is null.");
                return;
            }

            // Create an ExportData object with the required information
            ExportData exportData = new ExportData
            {
                CategoryName = $"{prefix}{category.Name}", // Include the prefix in the category name
                LineWeight = category.GetLineWeight(GraphicsStyleType.Projection)?.ToString() ?? "-",
                LinePattern = GetLinePatternName(doc, category),
            };

            // Check if GraphicsStyle is available
            if (category.GetGraphicsStyle(GraphicsStyleType.Projection) is GraphicsStyle graphicsStyle)
            {
                // Check if LineColor is available and not null
                var lineColor = graphicsStyle.GraphicsStyleCategory.LineColor;
                if (lineColor != null)
                {
                    // Create a Revit color from RGB values
                    Autodesk.Revit.DB.Color revitColor = new Autodesk.Revit.DB.Color(
                        lineColor.Red,
                        lineColor.Green,
                        lineColor.Blue
                    );

                    exportData.LineColor = revitColor;
                }
            }

            // Add the ExportData object to the list
            exportDataList.Add(exportData);

        }
        public static void ExportModelObjectStyles(Document doc, Category category, List<ExportData> exportDataList, string prefix = "")
        {
            // Check if the category is null
            if (category == null)
            {
                Debug.Print("Category is null.");
                return;
            }

            // Create an ExportData object with the required information
            ExportData exportData = new ExportData
            {
                CategoryName = $"{prefix}{category.Name}", // Include the prefix in the category name
                LineWeight = category.GetLineWeight(GraphicsStyleType.Projection)?.ToString() ?? "-",
                LinePattern = GetLinePatternName(doc, category),
                Material = GetCategoryMaterialAsString(category.Material)
            };

            exportData.Cut = GetCategoryCutAsString(category, exportData);

            // Check if GraphicsStyle for Projection is available
            if (category.GetGraphicsStyle(GraphicsStyleType.Projection) is GraphicsStyle projectionGraphicsStyle)
            {
                // Check if LineColor is available and not null
                var lineColor = projectionGraphicsStyle.GraphicsStyleCategory?.LineColor;
                if (lineColor != null)
                {
                    // Create a Revit color from RGB values
                    Autodesk.Revit.DB.Color revitColor = new Autodesk.Revit.DB.Color(
                        lineColor.Red,
                        lineColor.Green,
                        lineColor.Blue
                    );

                    exportData.LineColor = revitColor;
                }
            }

            // Add the ExportData object to the list
            exportDataList.Add(exportData);
        }

        private static string GetCategoryCutAsString(Category category, ExportData exportData)
        {
            // Check if GraphicsStyle for Cut is available
            if (category.GetGraphicsStyle(GraphicsStyleType.Cut) is GraphicsStyle cutGraphicsStyle)
            {
                // Extract the relevant information from cutGraphicsStyle
                return category.GetLineWeight(GraphicsStyleType.Cut).ToString(); // You might need to adjust this based on your requirements
            }
            else
            {
                return exportData.Cut = ""; // Set a default value if GraphicsStyle for Cut is not available
            }
        }

        private static string GetCategoryMaterialAsString(Material catMaterial)
        {
            if (catMaterial != null)
                return catMaterial.Name.ToString();
            else
                return "";
        }

        public static string GetParameterValueAsString(Document doc, Element element, BuiltInParameter parameter)
        {
            // Check if the element and document are not null
            if (element != null && doc != null)
            {
                // Use the Revit API to retrieve the parameter
                Parameter revitParameter = element.get_Parameter(parameter);

                // Check if the parameter exists and is valid
                if (revitParameter != null)
                {
                    // Check the parameter's storage type (e.g., String, Integer, Double)
                    if (revitParameter.StorageType == StorageType.String)
                    {
                        // Convert the parameter value to a string
                        return revitParameter.AsString();
                    }
                    else
                    {
                        // Handle other storage types as needed (e.g., Integer, Double)
                        return revitParameter.AsValueString();
                    }
                }
            }

            // Return a default value or an indication that the parameter was not found
            return "N/A";
        }

        public static void ExportModelObjectStyles2(Document doc, Category category, List<ExportData> exportDataList, string prefix = "")
        {
            // Check if the category is null
            if (category == null)
            {
                Debug.Print("Category is null.");
                return;
            }

            // Create an ExportData object with the required information
            ExportData exportData = new ExportData
            {
                CategoryName = $"{prefix}{category.Name}", // Include the prefix in the category name
                LineWeight = category.GetLineWeight(GraphicsStyleType.Projection)?.ToString() ?? "-",
                Cut = category.GetGraphicsStyle(GraphicsStyleType.Cut).ToString(),
                LinePattern = GetLinePatternName(doc, category),
                Material = category.Material.ToString(),
            };

            // Check if GraphicsStyle is available
            if (category.GetGraphicsStyle(GraphicsStyleType.Projection) is GraphicsStyle graphicsStyle)
            {
                // Check if LineColor is available and not null
                var lineColor = graphicsStyle.GraphicsStyleCategory.LineColor;
                if (lineColor != null)
                {
                    // Create a Revit color from RGB values
                    Autodesk.Revit.DB.Color revitColor = new Autodesk.Revit.DB.Color(
                        lineColor.Red,
                        lineColor.Green,
                        lineColor.Blue
                    );

                    exportData.LineColor = revitColor;
                }
            }

            // Add the ExportData object to the list
            exportDataList.Add(exportData);
        }

        // Helper method to export sub-settings of a category
        public static void ExportSubSettings(Document doc, Category category, List<ExportData> exportDataList, string prefix = "")
        {
            // Get the sub-categories of the category
            CategoryNameMap subCategories = category.SubCategories;

            //if (subCategories != null)
            //{
            //    var sortedSubcategories = subCategories.Cast<Category>().OrderBy(subcat => subcat.Name);

            //    //foreach (Category subCategory in sortedSubcategories)
            //    //{
            //    //    // Process each subcategory here
            //    //    Debug.Print(subCategory.Name);
            //    //}
            //}

            if (subCategories != null && subCategories.Size > 0)
            {

                var sortedSubcategories = subCategories.Cast<Category>().OrderBy(subcat => subcat.Name);

                //foreach (Category subCategory in sortedSubcategories)
                //{
                //    // Process each subcategory here
                //    Debug.Print(subCategory.Name);
                //}


                // Define the prefix for sub-categories
                string subCategoryPrefix = " |--";

                // Iterate through the sub-categories
                //foreach (Category subCategory in subCategories)
                foreach (Category subCategory in sortedSubcategories)
                {

                    // Print sub-category details (you can remove this if not needed)
                    Debug.Print($"{prefix}{subCategoryPrefix}{subCategory.Name} - {subCategory.IsReadOnly} - {subCategory.LineColor}");

                    // Export object styles and add data to the list
                    if (category.CategoryType == CategoryType.Model || category.CategoryType == CategoryType.AnalyticalModel)
                    {
                        ExportModelObjectStyles(doc, subCategory, exportDataList, prefix + subCategoryPrefix);
                    }
                    else
                    {
                        ExportObjectStyles(doc, subCategory, exportDataList, prefix + subCategoryPrefix);
                    }

                    // Recursively export sub-settings of this sub-category with the updated prefix
                    ExportSubSettings(doc, subCategory, exportDataList, prefix + subCategoryPrefix);
                }
            }
        }

        public static string GetLinePatternName(Document doc, Category category)
        {
            // Get the ElementId of the line pattern for the specified category
            ElementId linePatternId = category.GetLinePatternId(GraphicsStyleType.Projection);

            // Try to get the line pattern element from the document using the ElementId
            Element linePattern = doc.GetElement(linePatternId);

            if (linePattern != null)
            {
                // If a line pattern element was found, return its name
                return linePattern.Name;
            }
            else
            {
                // Check if the line pattern Id corresponds to a solid line
#if REVIT2020 || REVIT2021 || REVIT2022 || REVIT2023
                if (linePatternId.IntegerValue == -3000010)
#else
                if (linePatternId.Value == -3000010)
#endif
                {
                    return "Solid";
                }
                else
                {
                    // If no line pattern element was found and it's not a solid line, return "-"
                    return "-";
                }
            }
        }

        //public static string GetLinePatternName(Document doc, Category category)
        //{
        //    // Get the ElementId of the line pattern for the specified category
        //    ElementId linePatternId = category.GetLinePatternId(GraphicsStyleType.Projection);

        //    // Try to get the line pattern element from the document using the ElementId
        //    Element linePattern = doc.GetElement(linePatternId);

        //    if (linePattern != null)
        //    {
        //        // If a line pattern element was found, return its name
        //        return linePattern.Name;
        //    }
        //    else if (linePatternId.IntegerValue == -3000010)
        //    {
        //        // Check if the line pattern Id corresponds to a solid line (-3000010)
        //        return "Solid";
        //    }
        //    else
        //    {
        //        // If no line pattern element was found and it's not a solid line, return "-"
        //        return "-";
        //    }
        //}

        // Helper method to export data to EPPlus with Save As functionality
        public static void ExportDataToEPPlusWithSaveAs(List<ExportData> exportDataList, string categoryName)
        {

            // Create a file save dialog
            var saveFileDialog = CreateSaveAsFileDialog(categoryName);

            // Show the file save dialog
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string excelFilePath = saveFileDialog.FileName;
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                // Create a new Excel package
                using (var package = new ExcelPackage())
                {
                    // Add a new worksheet
                    var worksheet = package.Workbook.Worksheets.Add(categoryName);

                    // Set headers
                    worksheet.Cells[1, 1].Value = "Category";
                    worksheet.Cells[1, 2].Value = "Line Weight";
                    worksheet.Cells[1, 3].Value = "Line Color (RGB)";
                    worksheet.Cells[1, 4].Value = "Line Pattern";

                    // Fill in the data
                    int row = 2; // Start from the second row
                    foreach (var exportData in exportDataList)
                    {
                        worksheet.Cells[row, 1].Value = exportData.CategoryName;
                        worksheet.Cells[row, 2].Value = exportData.LineWeight;
                        worksheet.Cells[row, 3].Value = $"{exportData.LineColor.Red}-{exportData.LineColor.Green}-{exportData.LineColor.Blue}";
                        worksheet.Cells[row, 4].Value = exportData.LinePattern;
                        row++;
                    }

                    // Save the Excel file to the chosen location
                    File.WriteAllBytes(excelFilePath, package.GetAsByteArray());
                    Debug.Print($"Data exported to '{excelFilePath}'.");

                    // Format the Excel file using EPPlus
                    FormatObjectStylesExcelExport(excelFilePath);

                    DoYouWantToOpenThisFile(excelFilePath);
                }
            }
        }

        public static void ExportModelDataToEPPlusWithSaveAs(List<ExportData> exportDataList, string categoryName)
        {

            // Create a file save dialog
            var saveFileDialog = CreateSaveAsFileDialog(categoryName);

            // Show the file save dialog
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string excelFilePath = saveFileDialog.FileName;
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
#if REVIT2025
                // Register the CodePagesEncodingProvider to support additional encodings. This is required for .NET8 Revit 2025
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif
                // Create a new Excel package
                using (var package = new ExcelPackage())
                {
                    // Add a new worksheet
                    var worksheet = package.Workbook.Worksheets.Add(categoryName);

                    // Set headers
                    worksheet.Cells[1, 1].Value = "Category";
                    worksheet.Cells[1, 2].Value = "Line Weight";
                    worksheet.Cells[1, 3].Value = "Cut";
                    worksheet.Cells[1, 4].Value = "Line Color (RGB)";
                    worksheet.Cells[1, 5].Value = "Line Pattern";
                    worksheet.Cells[1, 6].Value = "Material";

                    // Fill in the data
                    int row = 2; // Start from the second row
                    foreach (var exportData in exportDataList)
                    {
                        worksheet.Cells[row, 1].Value = exportData.CategoryName;
                        worksheet.Cells[row, 2].Value = exportData.LineWeight;
                        worksheet.Cells[row, 3].Value = exportData.Cut;
                        worksheet.Cells[row, 4].Value = $"{exportData.LineColor.Red}-{exportData.LineColor.Green}-{exportData.LineColor.Blue}";
                        worksheet.Cells[row, 5].Value = exportData.LinePattern;
                        worksheet.Cells[row, 6].Value = exportData.Material;
                        row++;
                    }

                    // Save the Excel file to the chosen location
                    File.WriteAllBytes(excelFilePath, package.GetAsByteArray());
                    Debug.Print($"Data exported to '{excelFilePath}'.");

                    FormatObjectStylesExcelExport(excelFilePath);

                    // Show a TaskDialog to ask if the user wants to open the exported file
                    DoYouWantToOpenThisFile(excelFilePath);
                }
            }
        }
        // Define your method
        public static void FormatObjectStylesExcelExport(string filePath)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            // Load the Excel file using EPPlus
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                // Get the first worksheet in the Excel file
                var worksheet = package.Workbook.Worksheets[0];

                // Set the font size for the entire worksheet
                worksheet.Cells.Style.Font.Size = 9;
                // Set the font name to "Calibri" and make it light for the entire worksheet
                worksheet.Cells.Style.Font.Name = "Calibri Light";

                // Bold every cell in column A except those starting with " |--"
                foreach (var cell in worksheet.Cells[2, 1, worksheet.Dimension.End.Row, 1]) // Start from the second row, column A
                {
                    if (!cell.Text.StartsWith(" |--"))
                    {
                        cell.Style.Font.Bold = true;
                    }
                }

                // Set the background color of the first row cells to DEDEDE
                int row = 1; int col = 1; int rowRange = 1;
                worksheet.Cells[row, col, rowRange, worksheet.Dimension.End.Column].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[row, col, rowRange, worksheet.Dimension.End.Column].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DEDEDE"));


                // Auto-fit all columns
                worksheet.Cells.AutoFitColumns();

                // Freeze the first row (header)
                worksheet.View.FreezePanes(2, 1); // Freeze the first row except for the header

                // Format the header row (first row) with bold text
                var headerRow = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column]; // Select the entire first row
                headerRow.Style.Font.Bold = true;

                // Save the changes back to the Excel file
                package.Save();
            }
        }

        public static void ExportDimensionDataToEPPlusWithSaveAs(string fileName, List<DimensionData> dimensionTypes)
        {
            // Create a file save dialog
            var saveFileDialog = CreateSaveAsFileDialog(fileName);
            //System.Windows.Forms.SaveFileDialog saveFileDialog = CreateSaveAsFileDialog(fileName);

            // Show the file save dialog
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string excelFilePath = saveFileDialog.FileName;
                    // Set the license context to NonCommercial
                    ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                    // Create a new Excel package
                    using (var package = new ExcelPackage())
                    {
                        // Create a new worksheet
                        var worksheet = package.Workbook.Worksheets.Add("DimensionTypes");

                        // Set the headers
                        worksheet.Cells["A1"].Value = "Family";
                        worksheet.Cells["B1"].Value = "Type";

                        int row = 2;

                        // Write DimensionType data to the worksheet
                        foreach (var dimensionType in dimensionTypes)
                        {
                            worksheet.Cells[row, 1].Value = dimensionType.FamilyName;
                            worksheet.Cells[row, 2].Value = dimensionType.DimensionTypeName;
                            row++;
                        }

                        //// Write SpotDimensionType data to the worksheet
                        //foreach (var spotType in spotTypes)
                        //{
                        //    worksheet.Cells[row, 1].Value = spotType.FamilyName;
                        //    worksheet.Cells[row, 2].Value = spotType.SpotDimensionTypeName;
                        //    row++;
                        //}

                        // Save the Excel file with the specified file name
                        //package.SaveAs(new System.IO.FileInfo(fileName));
                        // Save the Excel file to the chosen location
                        File.WriteAllBytes(excelFilePath, package.GetAsByteArray());
                        Debug.Print($"Data exported to '{excelFilePath}'.");

                        // Format the Excel file using EPPlus
                        FormatDimensionsExcelExport(excelFilePath);

                        // Show a TaskDialog to ask if the user wants to open the exported file
                        DoYouWantToOpenThisFile(excelFilePath);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error exporting data to Excel: " + ex.Message);
                }
            }

        }

        public static System.Windows.Forms.SaveFileDialog CreateSaveAsFileDialog(string fileName)
        {
            return new System.Windows.Forms.SaveFileDialog
            {
                Title = "Save Exported Data As",
                FileName = fileName,
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                DefaultExt = "xlsx",
                AddExtension = true
            };
        }

        public static void DoYouWantToOpenThisFile(string excelFilePath)
        {
            TaskDialog _taskScheduleResult = new TaskDialog("Export Complete");
            _taskScheduleResult.TitleAutoPrefix = false; // Suppress The name of the document in the title
            _taskScheduleResult.MainContent = $"Do you want to open the exported file: \n{System.IO.Path.GetFileName(excelFilePath)} ?";
            _taskScheduleResult.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
            var TDResult = _taskScheduleResult.Show();

            if (TDResult == TaskDialogResult.Yes)
            {
                MyUtils.StartProcess(excelFilePath); // Open the exported file
                //Process.Start(excelFilePath); // Open the exported file
            }
        }

        public static void FormatDimensionsExcelExport(string excelFilePath)
        {
            using (var package = new ExcelPackage(new FileInfo(excelFilePath)))
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                var worksheet = package.Workbook.Worksheets[0];

                // Bold the header row and set the font size
                using (var range = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                {
                    range.Style.Font.Bold = true;
                    //range.Style.Font.Size = 9;
                }

                // Initialize the current family name
                string currentFamilyName = worksheet.Cells[2, 1].Text;

                // Iterate through rows and format
                for (int row = 3; row <= worksheet.Dimension.End.Row; row++)
                {
                    string familyName = worksheet.Cells[row, 1].Text;

                    if (familyName != currentFamilyName)
                    {
                        // Insert an empty row
                        worksheet.InsertRow(row, 1);

                        // Update the current family name and set it in the new row
                        currentFamilyName = familyName;
                        worksheet.Cells[row + 1, 1].Value = familyName;
                        row++; // Skip the row that was just added
                    }
                    else
                    {
                        // Clear the family name for subsequent rows in the same group
                        worksheet.Cells[row, 1].Clear();
                    }
                }
                // Bold everything in column 1
                for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
                {
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                }
                // Set the font size for the entire worksheet
                worksheet.Cells.Style.Font.Size = 9;
                // Set the font name to "Calibri" and make it light for the entire worksheet
                worksheet.Cells.Style.Font.Name = "Calibri Light";
                // Set the background color of the first row cells to DEDEDE
                int rowN = 1; int colN = 1; int rowRange = 1;
                worksheet.Cells[rowN, colN, rowRange, worksheet.Dimension.End.Column].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[rowN, colN, rowRange, worksheet.Dimension.End.Column].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#DEDEDE"));


                // Auto-fit all columns
                worksheet.Cells.AutoFitColumns();

                // Freeze the first row (header)
                worksheet.View.FreezePanes(2, 1); // Freeze the first row except for the header

                // Save the modified Excel file
                package.Save();
            }
        }


        // ==============   Classes ===============

        // Data structure to store export data
        public class ExportData
        {
            public string CategoryName { get; set; }
            public string LineWeight { get; set; }
            public string Cut { get; set; }
            public Autodesk.Revit.DB.Color LineColor { get; set; }
            public string LinePattern { get; set; }
            public string Material { get; set; }
        }

        public class DimensionData
        {
            public string FamilyName { get; set; }
            public string DimensionTypeName { get; set; }
        }

        public class SpotDimensionData
        {
            public string FamilyName { get; set; }
            public string SpotDimensionTypeName { get; set; }
        }

    }
}