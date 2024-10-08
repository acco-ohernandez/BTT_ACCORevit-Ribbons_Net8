﻿#region Namespaces
using Autodesk.Revit.DB.Structure;

using Microsoft.VisualBasic.FileIO;

using OfficeOpenXml;
#endregion

#region Begining of doc
namespace RevitRibbon_MainSourceCode
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_ScheduleImport : MyUtils, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Autodesk.Revit.DB.Document doc = uidoc.Document;

            #region FormStuff
            //// open form
            //MyForm currentForm = new MyForm()
            //{
            //    Width = 800,
            //    Height = 450,
            //    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
            //    Topmost = true,
            //};

            //currentForm.ShowDialog();

            // get form data and do something
            #endregion
            #endregion

            //Get the file paths selected by the user:
            string excelFilePath = M_GetExcelFilePath();
            if (excelFilePath == null)
            {
                M_MyTaskDialog("Info", "No file was selected.\nImport Cancelled!");
                return Result.Cancelled; // if no file is selected by the user, cancel the operation
            }

            // Set EPPlus license context
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;  // Set the license context for EPPlus to NonCommercial
#if REVIT2025
            // Register the CodePagesEncodingProvider to support additional encodings. This is required for .NET8 Revit 2025
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif
            using (var excelPackage = new ExcelPackage(new FileInfo(excelFilePath)))
            {
                List<ExcelWorksheet> excelWorksheetList = M_ReadExcelFile(excelPackage);
                if (excelWorksheetList == null)
                {
                    M_MyTaskDialog("Error", "Failed to read Excel file.");
                    return Result.Cancelled;
                }

                List<ViewSchedule> ScheduleNamesFoundInCurrentDoc = M_GetScheduleByUniqueIdFromExcelSheet(doc, excelWorksheetList);
                if (ScheduleNamesFoundInCurrentDoc.Count == 0)
                {
                    M_MyTaskDialog("Error", "The current Revit document does not contain any of the schedules from the Excel file.");
                    return Result.Cancelled;
                }

                // Open schedulesImport_Form1
                SchedulesImport_Form schedulesImport_Form1 = new SchedulesImport_Form()
                {
                    Width = 500,
                    Height = 600,
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                    Topmost = true,
                };

                schedulesImport_Form1.dataGrid.ItemsSource = ScheduleNamesFoundInCurrentDoc.Select(schedule => schedule.Name).ToList();

                // Update the Content property of lbl_Title
                schedulesImport_Form1.Dispatcher.Invoke(() =>
                {
                    schedulesImport_Form1.lbl_Title.Content = "Schedules Import";
                });

                schedulesImport_Form1.ShowDialog();
                int count = 0;
                if (schedulesImport_Form1.DialogResult == true)
                {
                    var selectedScheduleNames = schedulesImport_Form1.dataGrid.SelectedItems;
                    foreach (var scheduleName in selectedScheduleNames)
                    {
                        // Find the selected schedule by name
                        var selectedSchedule = ScheduleNamesFoundInCurrentDoc.FirstOrDefault(schedule => schedule.Name == scheduleName.ToString());
                        if (selectedSchedule != null)
                        {

                            ExcelWorksheet worksheet = M_GetWorksheetByCellA2(selectedSchedule.UniqueId, excelWorksheetList);
                            //var excelSheetData = GetScheduleDataFromSheet(excelWorksheetList[6]);
                            var excelSheetData = GetScheduleDataFromSheet(worksheet);
                            using (Transaction trans = new Transaction(doc, $"Imported: {selectedSchedule.Name}"))
                            {
                                trans.Start();
                                ImportSchedules(doc, excelSheetData);
                                count++;
                                trans.Commit();
                            }
                        }
                    }
                }
                M_MyTaskDialog("Info", $"{count} Schedule(s) Imported Successfully");
            }

            return Result.Succeeded;
        }

        public static String GetMethod()
        {
            var method = MethodBase.GetCurrentMethod().DeclaringType?.FullName;
            return method;
        }
    }
}