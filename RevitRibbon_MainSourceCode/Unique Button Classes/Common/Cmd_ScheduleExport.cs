#region Namespaces
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using Autodesk.Revit.Creation;

using OfficeOpenXml;

using RevitRibbon_MainSourceCode_Resources;
#endregion

namespace RevitRibbon_MainSourceCode
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_ScheduleExport : MyUtils, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Autodesk.Revit.DB.Document doc = uidoc.Document;

            #region FormStuff
            // open form
            MyForm currentForm = new MyForm()
            {
                Width = 400,
                Height = 200,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                Topmost = true,
            };

            //Get form data and do something
            #endregion
            string docName = doc.Title; // Name of the revit document
            string _excelFilePath = null; // To be used for the path of the excel file to be output          

            // ================= Get All Schedules =================
            var _schedulesList = M_GetSchedulesList(doc); // Get all the Schedules into a list

            // Open schedulesImport_Form1
            SchedulesImport_Form schedulesImport_Form1 = new SchedulesImport_Form()
            {
                Width = 500,
                Height = 600,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                Topmost = true,
            };
            // Pass the list schedules to the form data grid
            schedulesImport_Form1.dataGrid.ItemsSource = _schedulesList.Select(schedule => schedule.Name).ToList();

            // Update the Content property of lbl_Title. This dynamically updates the form title
            schedulesImport_Form1.Dispatcher.Invoke(() =>
            {
                schedulesImport_Form1.lbl_Title.Content = "Schedules Export";
                schedulesImport_Form1.btn_Import.Content = "Export";
            });
            // show the list to the user
            schedulesImport_Form1.ShowDialog();
            // If the user clicked the export button
            if (schedulesImport_Form1.DialogResult == true)
            {
                // Get the list of selected schedules to be exported
                var selectedSchedulenames = schedulesImport_Form1.dataGrid.SelectedItems.Cast<string>().ToList();
                // If no schedules selected, Cancel the export
                if (selectedSchedulenames.Count <= 0)
                {
                    M_MyTaskDialog("Info", "No schedules were selected");
                    return Result.Cancelled;
                }
                // pick a forder and file name for the excel export 
                string _path = M_ExcelSaveAs($"{docName}_Schedules.xlsx");
                if (_path == null) { M_MyTaskDialog("Info", "You did not select an output path"); return Result.Cancelled; }
                _excelFilePath = _path; // Path of the excel file to be output

                // populate the selectedSchedules list to be exported
                List<ViewSchedule> selectedSchedules = new List<ViewSchedule>();
                foreach (var scheduleName in selectedSchedulenames)
                {
                    // Find the selected schedule by name
                    selectedSchedules.Add(_schedulesList.FirstOrDefault(sch => sch.Name == scheduleName));
                }
                _schedulesList = selectedSchedules;
            }
            else
            {   // if the form schedulesImport_Form1 was closed, Cancel the export
                return Result.Cancelled;
            }

            // Handle file exists and file is open
            bool isFileOpen = M_TellTheUserIfFileExistsOrIsOpen(_excelFilePath);
            // if the file is open, cancel the export
            if (isFileOpen) { return Result.Cancelled; }

            ////M_ShowCurrentFormForNSeconds(currentForm, 5); // This is a background job
            //currentForm.Dispatcher.Invoke(() =>
            //{
            //    currentForm.lbl_ExportCount.Content = $"Exporting {_schedulesList.Count} Schedule(s), Please wait...";
            //});
            //M_ShowCurrentFormForNSeconds(currentForm, 5); // This is a background job

            ExcelPackage excelFile = Create_ExcelFile(_excelFilePath);
            ExcelWorkbook workbook = excelFile.Workbook;  // Get the workbook from the Excel package
            int prefix = 1;
            using (Transaction t = new Transaction(doc, "Exported Schedules"))
            {
                t.Start();
                foreach (ViewSchedule schedule in _schedulesList)
                {
                    // set the schedule to show tile and headers returns de original schedule definition
                    ScheduleDefinition curScheduleDefinition = MyUtils.M_ShowHeadersAndTileOnSchedule(schedule);
                    // Get all the categegories that allow AllowsBoundParameters as a set 
                    CategorySet _scheduleBuiltInCategory = M_GetAllowBoundParamCategorySet(doc, schedule);
                    // Add the "Dev_Text_1" parameter to be used for the UniqueID of the row element during export.
                    M_Add_Dev_Text_4(app, doc, schedule, _scheduleBuiltInCategory);

                    // create excel sheet based on schedule name and number prefix to avoid duplicates
                    ExcelWorksheet worksheet = workbook.Worksheets.Add($"{prefix}_{schedule.Name}");
                    // load current schedule to its own excel sheet
                    ExportViewScheduleBasic(schedule, worksheet);
                    prefix++;
                }
                t.RollBack();
            }
#if REVIT2025
            // Register the CodePagesEncodingProvider to support additional encodings. This is required for .NET8 Revit 2025
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif
            excelFile.Save();
            excelFile.Dispose();
            StartProcess(_excelFilePath);
            //M_MyTaskDialog("Info", $"Process complete.\n{prefix - 1} schedule(s) exported.");

            return Result.Succeeded;
        }



        public static String GetMethod()
        {
            var method = MethodBase.GetCurrentMethod().DeclaringType?.FullName;
            return method;
        }
    }

}
