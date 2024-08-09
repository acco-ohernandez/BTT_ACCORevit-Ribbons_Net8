using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using Microsoft.VisualBasic.FileIO;

using OfficeOpenXml;

using RevitRibbon_MainSourceCode_Resources;

using TaskDialogIcon = Autodesk.Revit.UI.TaskDialogIcon;

//using OfficeOpenXml.Core.ExcelPackage;

//using View = Autodesk.Revit.DB.View;

namespace RevitRibbon_MainSourceCode
{
    [Transaction(TransactionMode.Manual)]
    public class MyUtils
    {
        #region export methods
        //###########################################################################################
        public static List<ViewSchedule> M_GetSchedulesList(Autodesk.Revit.DB.Document doc)
        {
            int count = 0;
            List<ViewSchedule> schedulesList = new List<ViewSchedule>();
            FilteredElementCollector schedulesCollector = new FilteredElementCollector(doc).OfClass(typeof(ViewSchedule));

            foreach (ViewSchedule viewSchedule in schedulesCollector)
            {
                if (viewSchedule.IsTitleblockRevisionSchedule)
                    continue;

                schedulesList.Add(doc.GetElement(viewSchedule.Id) as ViewSchedule);
            }

            // Sort the schedulesList by name
            schedulesList = schedulesList.OrderBy(schedule => schedule.Name).ToList();

            foreach (Element element in schedulesList)
            {
                Debug.Print($"Method: _GetSchedulesList : {count} - {element.Name}");
                count++;
            }

            return schedulesList;
        }

        public static List<ViewSchedule> M_GetSchedulesList1(Autodesk.Revit.DB.Document doc) // This method returns a list of all the schedule elements
        {
            int count = 0;
            List<ViewSchedule> _schedulesList = new List<ViewSchedule>();
            FilteredElementCollector _schedules = new FilteredElementCollector(doc).OfClass(typeof(ViewSchedule));
            foreach (ViewSchedule viewSchedule in _schedules)
            {
                if (viewSchedule.IsTitleblockRevisionSchedule) continue;
                _schedulesList.Add(doc.GetElement(viewSchedule.Id) as ViewSchedule);
            }
            foreach (Element element in _schedulesList) // Debug Print
            { Debug.Print($"Method: _GetSchedulesList : {count} - {element.Name}"); count++; } // 
            return _schedulesList;
        }
        //###########################################################################################


        //###########################################################################################
        /// <summary>
        /// _GetScheduleTableDataAsString Method: Takes in one ViewSchedule and the Autodesk.Revit.DB.Document
        /// </summary>
        /// <param name="_selectedSchedule"></param>
        /// <param name="doc"></param>
        /// <returns>Returns a comma delimited string list of all the schedule data</returns>
        /// 
        public static List<string> _GetScheduleTableDataAsString(ViewSchedule _selectedSchedule, Autodesk.Revit.DB.Document doc)
        {
            //---Gets the list of items/rows in the schedule---
            var _scheduleItemsCollector = new FilteredElementCollector(doc, _selectedSchedule.Id).WhereElementIsNotElementType();
            List<string> uid = new List<string>(); // List to hold all the UniqueIDs in the schedule
            foreach (var _scheduleRow in _scheduleItemsCollector)
            {
                Debug.Print($"{_scheduleRow.GetType()} - UID: {_scheduleRow.UniqueId}"); // only for Debug
                uid.Add(_scheduleRow.UniqueId); // adds each UniqueID to the uid list
            }

            // Get the data from the ViewSchedule object
            TableData tableData = _selectedSchedule.GetTableData();
            TableSectionData _sectionData = tableData.GetSectionData(SectionType.Body);

            // Concatenate the data into a list of strings, with each string representing a row
            List<string> _rowsList = new List<string>();
            for (int row = 0; row < _sectionData.NumberOfRows; row++)
            {
                int _adjestedRow = row - 2;
                StringBuilder sb = new StringBuilder();
                if (row == 0)
                {
                    sb.Append("UniqueId,"); // adds the "UniqueId" header
                }
                if (row == 1)
                {
                    sb.Append(","); // adds the "," on the second line. This is because schedules second row is empty.
                }
                for (int col = 0; col < _sectionData.NumberOfColumns; col++)
                {
                    string cellText = _sectionData.GetCellText(row, col).ToString();
                    var cellTextColIndex = _sectionData.GetCellText(row, col);

                    if (col == 0 && row >= 2) // if first column, add the UniqueID
                    {
                        string uniqueId = uid[_adjestedRow];
                        sb.Append($"{uniqueId},");
                    }
                    sb.Append(cellText + ",");
                }
                _rowsList.Add(sb.ToString());
            }
            // This line is only for debug, outputs all the rows in _scheduleTableDataAsString
            int lineN = 0; foreach (var line in _rowsList) { lineN++; Debug.Print($"Row {lineN}: {line}"); }

            return _rowsList;
        }
        //###########################################################################################

        //###########################################################################################

        public List<ViewSchedule> _GetViewScheduleBasedOnUniqueId(Autodesk.Revit.DB.Document doc, string _elementUniqueIdString) // still not working
        {
            // Assuming you have an active Revit document
            //string uniqueId = "dc86627d-cf12-49fe-bdad-488a619b34a1-00060aca";
            string uniqueId = _elementUniqueIdString;

            int count = 0;
            List<ViewSchedule> _schedulesList = new List<ViewSchedule>();
            FilteredElementCollector _schedules = new FilteredElementCollector(doc).OfClass(typeof(ViewSchedule));
            foreach (ViewSchedule viewSchedule in _schedules)
            {
                Debug.Print("Method: _GetElementBasedOnUniqueId =================");

                if (viewSchedule.IsTitleblockRevisionSchedule) continue;

                //_schedulesList.Add(doc.GetElement(viewSchedule.Id) as ViewSchedule);
                if (viewSchedule.UniqueId == uniqueId && viewSchedule != null)
                {
                    _schedulesList.Add(doc.GetElement(viewSchedule.Id) as ViewSchedule);
                }

            }
            foreach (Element element in _schedulesList) // Debug Print
            {
                Debug.Print($"Method: _GetElementBasedOnUniqueId : {count} - {element.Name} " +
                            $"Element UniqueId: {element.UniqueId}");
                count++;
            } // Debug Print

            return _schedulesList;
        }

        //###########################################################################################
        public static ViewSchedule M_GetViewScheduleByName(Document doc, string viewScheduleName)
        {
            FilteredElementCollector _schedules = new FilteredElementCollector(doc).OfClass(typeof(ViewSchedule));
            ViewSchedule _viewScheduleNotFound = null;
            foreach (ViewSchedule curViewScheduleInDoc in _schedules)
            {
                if (curViewScheduleInDoc.Name == viewScheduleName)
                {
                    return curViewScheduleInDoc;
                }

            }
            return _viewScheduleNotFound;
        }
        public static ViewSchedule M_GetViewScheduleByUniqueId(Document doc, string viewScheduleUniqueId)
        {
            FilteredElementCollector _schedules = new FilteredElementCollector(doc).OfClass(typeof(ViewSchedule));
            ViewSchedule _viewScheduleNotFound = null;
            foreach (ViewSchedule curViewScheduleInDoc in _schedules)
            {
                if (curViewScheduleInDoc.UniqueId == viewScheduleUniqueId)
                {
                    return curViewScheduleInDoc;
                }

            }
            return _viewScheduleNotFound;
        }
        //###########################################################################################
        public static List<Element> _GetElementsOnScheduleRow(Document doc, ViewSchedule selectedSchedule)
        {
            // Got the idea from this Youtube video
            // https://www.youtube.com/watch?v=H1Z3f1pgyPE

            Debug.Print($"=========== Biginning GetElementsOnScheduleRow Method");

            TableData tabledata = selectedSchedule.GetTableData();
            TableSectionData tableSectionData = tabledata.GetSectionData(SectionType.Body);
            List<ElementId> elemIds = new FilteredElementCollector(doc, selectedSchedule.Id).ToElementIds().ToList();
            List<Element> elementOnRow = new List<Element>();

            foreach (var elemId in elemIds)
            {
                var elem = doc.GetElement(elemId);
                Debug.Print($"===========Row ElementID: {elem.Id} : Element Name: {elem.Name}");
                elementOnRow.Add(elem);
            }
            Debug.Print($"=========== End of GetElementsOnScheduleRow Method");

            return elementOnRow;
        }

        //###########################################################################################
        /// <summary>
        /// Takes a ViewSchedule Element and gets the TableData from it.
        /// </summary>
        /// <param name="curSchedule"></param>
        /// <returns>viewSchedule TableData</returns>
        public TableData _GetSchedulesTablesList(Element viewScheduleElement)
        {
            ViewSchedule _curViewSchedule = viewScheduleElement as ViewSchedule;
            TableData _scheduleTableData = _curViewSchedule.GetTableData() as TableData;
            return _scheduleTableData;
        }
        //###########################################################################################

        public static List<string> _listOfUniqueIdsInScheduleView(Document doc, ViewSchedule _selectedSchedule)
        {
            //---Gets the list of items/rows in the schedule---
            var _scheduleItemsCollector = new FilteredElementCollector(doc, _selectedSchedule.Id).WhereElementIsNotElementType();
            List<string> uid = new List<string>(); // List to hold all the UniqueIDs in the schedule
            foreach (var _scheduleRow in _scheduleItemsCollector)
            {
                Debug.Print($"{_scheduleRow.GetType()} - UID: {_scheduleRow.UniqueId}"); // only for Debug
                uid.Add(_scheduleRow.UniqueId); // adds each UniqueID to the uid list
            }
            return uid;
        }


        //###########################################################################################
        public static Element _getElementOnViewScheduleRowByUniqueId(Document doc, ViewSchedule _selectedSchedule, string _uniqueId)
        {
            //---Gets the list of items/rows in the schedule---
            var _scheduleItemsCollector = new FilteredElementCollector(doc, _selectedSchedule.Id).Where(e => e.UniqueId == _uniqueId);

            if (_scheduleItemsCollector.Any())
            {
                int count = _scheduleItemsCollector.Count();
                Element _scheduleRow = _scheduleItemsCollector.FirstOrDefault() as Element;
                Debug.Print($"Method _getElementOnViewScheduleRowByUniqueId: {_scheduleRow.GetType()} - UID: {_scheduleRow.UniqueId} === ELEMENT RETURNED!");
                return _scheduleRow;
            }
            else
            {
                Debug.Print($"Method _getElementOnViewScheduleRowByUniqueId: Element with UID {_uniqueId} not found!");
                return null;
            }
        }


        //###########################################################################################
        public static string[] GetCsvFilePath()
        {
            // Create a new OpenFileDialog object.
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();

            // Set the dialog's filter to CSV files.
            dialog.Filter = "CSV Files (*.csv)|*.csv";
            dialog.Multiselect = true;
            dialog.Title = "Select CSV File";
            dialog.RestoreDirectory = false;

            // Show the dialog to the user.
            dialog.ShowDialog();
            var _fileNames = dialog.FileNames;
            // If the user selected a file, return its path.
            if (_fileNames.Count() > 0)
            {
                return _fileNames;
            }

            // Otherwise, return null.
            return null;
        }
        //###########################################################################################
        public static List<string[]> ImportCSVToStringList2(string csvFilePath)
        {
            var dataList = new List<string[]>();

            // Create a TextFieldParser to parse the CSV file
            using (TextFieldParser parser = new TextFieldParser(csvFilePath)) // Requires the Microsoft.VisualBasic namespace reference and using Microsoft.VisualBasic.FileIO;
            {
                // Set the delimiter to comma
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                // Set the option to handle fields enclosed in quotes
                parser.HasFieldsEnclosedInQuotes = true;

                // Skip the first two lines
                if (!parser.EndOfData)
                {
                    parser.ReadLine();
                }

                if (!parser.EndOfData)
                {
                    parser.ReadLine();
                }

                // Read each line of the CSV file
                while (!parser.EndOfData)
                {
                    // Read the fields of the current line
                    string[] fields = parser.ReadFields();

                    // Add the fields to the dataList collection
                    dataList.Add(fields);
                }
            }

            // Return the parsed data as a list of string arrays
            return dataList;
        }

        //###########################################################################################


        public static string[] M_GetLinesFromCSV(string csvFilePath, int lineNumber)
        {
            string[] lineFields = null;

            try
            {
                using (StreamReader reader = new StreamReader(csvFilePath))
                {
                    // Read lines until we reach the specified line number
                    for (int i = 1; i <= lineNumber; i++)
                    {
                        string line = reader.ReadLine();

                        if (line == null)
                        {
                            // Reached the end of the file before reaching the specified line number
                            Debug.Print("Specified line number is out of range.");
                            return null;
                        }

                        if (i == lineNumber)
                        {
                            lineFields = line.Split(',');

                            // Remove quotes from each field
                            for (int j = 0; j < lineFields.Length; j++)
                            {
                                //string skipDoubleQuoteinText = SkipDoubleQuoteInText(lineFields[j]);
                                lineFields[j] = lineFields[j].Trim('"');
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"An error occurred while reading the CSV file: \n{csvFilePath}");
                Debug.Print(ex.ToString());
                lineFields = null; // Set lineFields to null in case of an error
            }

            return lineFields;
        }

        //###########################################################################################

        public static List<string> GetAllScheduleNames(Document doc)
        {
            var _curDocScheduleNames = new List<string>();
            foreach (var vs in M_GetSchedulesList(doc))
            {
                _curDocScheduleNames.Add(vs.Name); // Get all the Schedules in the current Document
            }
            return _curDocScheduleNames;
        }
        public static List<string> GetAllScheduleUniqueIds(Document doc)
        {
            var _curDocScheduleUniqueIds = new List<string>();
            foreach (var vs in M_GetSchedulesList(doc))
            {
                _curDocScheduleUniqueIds.Add($"{vs.UniqueId}"); // Get all the Schedules UniqueIds in the current Document
            }
            return _curDocScheduleUniqueIds;
        }
        //###########################################################################################
        public static string _UpdateViewSchedule(Autodesk.Revit.DB.Document doc, string viewScheduleUniqueIdFromCSV, string[] headersFromCSV, List<string[]> viewScheduledataRows)
        {
            string _updatesResult = null;
            ViewSchedule _viewScheduleToUpdate = M_GetViewScheduleByUniqueId(doc, viewScheduleUniqueIdFromCSV);
            if (_viewScheduleToUpdate != null)
            {
                _updatesResult += $"\n=== Updating ViewSchedule:{_viewScheduleToUpdate.Name} ===\n";

                List<Element> _rowsElementsOnViewSchedule =
                _GetElementsOnScheduleRow(doc, _viewScheduleToUpdate); // Get the list of Elements on Rows of the _viewScheduleToUpdate

                foreach (var _viewScheduledataRow in viewScheduledataRows) // Loop through the dataRows from viewScheduledataRows
                {
                    string _curCsvRowUniqueId = _viewScheduledataRow[0];  // Get the Unique Id from the current Row

                    Element _curRowElement =
                        _getElementOnViewScheduleRowByUniqueId(doc, _viewScheduleToUpdate, _curCsvRowUniqueId); // Get Element on ViewSchedule Row by UniqueId

                    // if the Element from the _curCsvRowUniqueId does not exist in the current schedule, skip it.
                    if (_curRowElement != null)
                    {
                        ParameterSet paramSet = _curRowElement.Parameters; // Get the parameters of the current row element in the _viewScheduleToUpdate

                        int headerCount = headersFromCSV.Count();
                        for (int i = 1; i < headerCount; i++)
                        {
                            int _headerColumnNumber = i;
                            string _curCsvHeaderName = headersFromCSV[_headerColumnNumber];
                            _curCsvHeaderName = _curCsvHeaderName.Trim();
                            Debug.Print(_curCsvHeaderName);

                            foreach (Parameter parameter in paramSet)
                            {
                                string paramName = null;
                                paramName = parameter.Definition.Name; // Get the name of the parameter


                                if (parameter.IsShared)// test
                                {
                                    var paramGuid = parameter.GUID; // Get the name of the parameter
                                }

                                Debug.Print(paramName);

                                if (paramName == _curCsvHeaderName && parameter != null && parameter.StorageType == StorageType.String && !parameter.IsReadOnly)
                                {

                                    string _valueFromCsv = _viewScheduledataRow[_headerColumnNumber];
                                    parameter.Set(_valueFromCsv);
                                    //// Check if the CSV value is not empty before updating the parameter
                                    //if (!string.IsNullOrEmpty(_viewScheduledataRow[_headerColumnNumber]))
                                    //{
                                    //    string _valueFromCsv = _viewScheduledataRow[_headerColumnNumber];
                                    //    parameter.Set(_valueFromCsv);
                                    //}
                                    //else
                                    //{
                                    //    if (parameter.HasValue && parameter.IsShared)
                                    //        parameter.ClearValue();
                                    //}

                                }

                            }
                        }

                    }

                }
            }

            return _updatesResult;
        }
        //###########################################################################################

        public static void M_MyTaskDialog(string Title, string MainContent)
        {
            TaskDialog _taskScheduleResult = new TaskDialog(Title);
            _taskScheduleResult.TitleAutoPrefix = false;
            _taskScheduleResult.MainContent = MainContent;
            _taskScheduleResult.Show();
        }
        public static void M_MyTaskDialog(string Title, string MainContent, string icon)
        {
            TaskDialog _taskScheduleResult = new TaskDialog(Title);
            _taskScheduleResult.TitleAutoPrefix = false;
            _taskScheduleResult.MainContent = MainContent;
            if (icon == "Error")
            { _taskScheduleResult.MainIcon = TaskDialogIcon.TaskDialogIconError; }
            else if (icon == "Warning")
            { _taskScheduleResult.MainIcon = TaskDialogIcon.TaskDialogIconWarning; }
            else if (icon == "Information")
            { _taskScheduleResult.MainIcon = TaskDialogIcon.TaskDialogIconInformation; }
            else if (icon == "Shield")
            {
                _taskScheduleResult.MainIcon = TaskDialogIcon.TaskDialogIconShield;
            }
            _taskScheduleResult.Show();
        }
        public static void M_MyTaskDialog(string Title, string MainInstructions, bool mainContentIsOn, string mainContentString = "")
        {
            if (mainContentIsOn)
            {
                TaskDialog _taskScheduleResult1 = new TaskDialog(Title);
                _taskScheduleResult1.TitleAutoPrefix = false;
                _taskScheduleResult1.MainInstruction = MainInstructions;
                _taskScheduleResult1.MainContent = mainContentString;

                _taskScheduleResult1.Show();
            }
            else
            {
                TaskDialog _taskScheduleResult2 = new TaskDialog(Title);
                _taskScheduleResult2.TitleAutoPrefix = false;
                _taskScheduleResult2.MainInstruction = MainInstructions;
                _taskScheduleResult2.Show();
            }
        }
        //###########################################################################################

        public static string _CreateFolderOnDesktopByName(string name)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string folderPath = System.IO.Path.Combine(desktopPath, name);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Debug.Print("Directory created successfully.");
            }
            else
            {
                Debug.Print("Directory already exists.");
            }
            return folderPath;
        }

        //###########################################################################################
        //add shared parameter to schedule
        public static void _AddSharedParameterToSchedule(Document doc, string _viewScheduleName)
        {
            var viewSchedule = M_GetViewScheduleByName(doc, _viewScheduleName);
            var field = viewSchedule.Definition.GetSchedulableFields().FirstOrDefault(x => IsSharedParameterSchedulableField(viewSchedule.Document, x.ParameterId, new Guid("<your guid>")));
            //.FirstOrDefault(x => x.ParameterId == destParameterId);


        }
        public static bool IsSharedParameterSchedulableField(Document document, ElementId parameterId, Guid sharedParameterId)
        {
            var sharedParameterElement = document.GetElement(parameterId) as SharedParameterElement;

            return sharedParameterElement?.GuidValue == sharedParameterId;
        }


        public static ScheduleField GetScheduleFieldByName(Document doc, ViewSchedule viewSchedule, string fieldName)
        {
            ScheduleDefinition definition = viewSchedule.Definition;
            var scheduleFieldIds = definition.GetFieldOrder();

            // Loop through the schedule fields and find the one with matching name
            foreach (ScheduleFieldId fieldId in scheduleFieldIds)
            {
                ScheduleField field = definition.GetField(fieldId); ;
                var ParamId = field.ParameterId;
                if (ParamId.ToString() == fieldName)
                {
                    return field as ScheduleField;
                }
            }

            // If no matching field is found, return null
            return null;
        }



        //public static ScheduleField M_AddByNameAvailableFieldToSchedule(Document doc, string scheduleName, string fieldName)
        public static void M_AddByNameAvailableFieldToSchedule(Document doc, string scheduleName, string fieldName)
        {
            // Get the schedule by name.
            ViewSchedule schedule = M_GetViewScheduleByName(doc, scheduleName);

            // Get the definition of the schedule.
            ScheduleDefinition definition = schedule.Definition;

            // Get the list of available fields.
            IList<SchedulableField> availableFields = definition.GetSchedulableFields();

            // Find the field you want to add.
            SchedulableField field = availableFields.FirstOrDefault(f => f.GetName(doc) == fieldName);

            // Add the field to the schedule.
            ScheduleField fieldAdded = null;
            try
            {
                fieldAdded = definition.AddField(field);
            }
            catch
            {
                Debug.Print("====    Did not add the dev_Text_1 field because it already existed.    ====");

                //var fields =  GetScheduleFieldByName(doc, schedule, "dev_Text_1");
                // fieldAdded = definition.GetField(fields.FieldId);
            }


            //return fieldAdded;
        }
        //###########################################################################################
        public static void _UpdateMyUniqueIDColumn(Document doc, string _viewScheduleName)
        {
            ViewSchedule _viewScheduleToUpdate = M_GetViewScheduleByName(doc, _viewScheduleName); // Get the schedule by name
            List<Element> _rowsElementsOnViewSchedule = _GetElementsOnScheduleRow(doc, _viewScheduleToUpdate); // Get the list of Elements on Rows of the _viewScheduleToUpdate
            foreach (Element _rowElement in _rowsElementsOnViewSchedule)
            {
                // Get the parameter set for the current row element.
                ParameterSet paramSet = _rowElement.Parameters; // Get the parameters of the current row element in the _viewScheduleToUpdate

                // paramSet.
                //paramSet.Where(p => p.Definition.Name == "MyUniqueId");


                // Iterate through the parameters in the parameter set.
                foreach (Parameter param in paramSet)
                {
                    // Check if the parameter's name is equal to MyUniqueId.
                    //if (param.Definition.Name == "MyUniqueId")
                    if (param.Definition.Name == "Dev_Text_1")
                    {
                        // Get the parameter's value.
                        param.Set($"{_rowElement.UniqueId}");
                        break;
                    }
                }

            }


            //TaskDialog.Show("Info", "Added UniqueIDs to MyUniqueId Column");
            Debug.Print("Added UniqueIDs to MyUniqueId Column");
        }
        //###########################################################################################
        private void ShowDefinitionFileInfo(DefinitionFile myDefinitionFile)
        {
            StringBuilder fileInformation = new StringBuilder(500);

            // get the file name 
            fileInformation.AppendLine("File Name: " + myDefinitionFile.Filename);

            // iterate the Definition groups of this file
            foreach (DefinitionGroup myGroup in myDefinitionFile.Groups)
            {
                // get the group name
                fileInformation.AppendLine("Group Name: " + myGroup.Name);

                // iterate the difinitions
                foreach (Definition definition in myGroup.Definitions)
                {
                    // get definition name
                    fileInformation.AppendLine("Definition Name: " + definition.Name);
                }
            }
            //TaskDialog.Show("Revit", fileInformation.ToString());
            M_MyTaskDialog("Revit", fileInformation.ToString(), "Information");
        }
        //###########################################################################################

        public static void AddNewParameterToSchedule(Document doc, string _viewScheduleName, string parameterName)
        {
            // Get the schedule by name.
            ViewSchedule schedule = M_GetViewScheduleByName(doc, _viewScheduleName); // Get the schedule by name

            // Get the definition of the schedule.
            ScheduleDefinition definition = schedule.Definition;

            // Create a new parameter.



            // Update the schedule.
            // schedule.Update();
        }
        public static void M_GetSharedParameterFile(Autodesk.Revit.ApplicationServices.Application app)
        {
            var originalSharedParametersFilename = app.SharedParametersFilename;
            // Create a new Revit application object.
            //Application app = new Application();

            // Set the SharedParametersFilename property to the path of the shared parameters file.
            //app.SharedParametersFilename = @"C:\Users\ohernandez\Desktop\Revit_Exports\SharedParams\ACCO -- Dev_Revit Shared Parameters.txt";
            app.SharedParametersFilename = M_CreateSharedParametersFile();

            // Open the shared parameters file.
            DefinitionFile definitionFile = app.OpenSharedParameterFile();

            // Get the DefinitionGroups collection for the DefinitionFile object.
            DefinitionGroups groups = definitionFile.Groups;

            // Iterate through the DefinitionGroups collection to get the DefinitionGroup objects.
            foreach (DefinitionGroup group in groups)
            {
                // Iterate through the DefinitionGroup objects to get the Definition objects.
                foreach (Definition definition in group.Definitions)
                {
                    // Use the Definition objects to access the shared parameters.
                    Console.WriteLine(definition.Name);
                }
            }

            // Close the shared parameters file.
            definitionFile.Dispose(); //Close();

            // Dispose the Revit application object.
            app.Dispose();

            app.SharedParametersFilename = originalSharedParametersFilename;
        }

        public static Definition GetParameterDefinitionFromFile(DefinitionFile defFile, string groupName, string paramName)
        {
            // iterate the Definition groups of this file
            foreach (DefinitionGroup group in defFile.Groups)
            {
                if (group.Name == groupName)
                {
                    // iterate the difinitions
                    foreach (Definition definition in group.Definitions)
                    {
                        if (definition.Name == paramName)
                            return definition;
                    }
                }
            }
            return null;
        }

        public static string M_MyAddNewParameterToSchedule(Autodesk.Revit.ApplicationServices.Application app)
        {
            string originalSharedParametersFile = app.SharedParametersFilename;

            // Set the SharedParametersFilename property to the path of the shared parameters file.
            //app.SharedParametersFilename = @"C:\Users\ohernandez\Desktop\Revit_Exports\SharedParams\ACCO -- Dev_Revit Shared Parameters.txt";
            app.SharedParametersFilename = M_CreateSharedParametersFile();

            // Open the shared parameters file.
            DefinitionFile definitionFile = app.OpenSharedParameterFile();
            Definition paramDef = null;
            // iterate the Definition groups of this file
            foreach (DefinitionGroup group in definitionFile.Groups)
            {
                if (group.Name == "Dev_Group_Common")
                {
                    // iterate the difinitions
                    foreach (Definition definition in group.Definitions)
                    {
                        if (definition.Name == "Dev_Text_1")
                            paramDef = definition;
                    }
                }
            }

            return originalSharedParametersFile;
        }

        public static void M_Add_Dev_Text_4(Autodesk.Revit.ApplicationServices.Application app, Document doc, ViewSchedule _curSchedule, CategorySet myCatSet)
        {
            string originalSharedParametersFilename = null;
            try
            {
                originalSharedParametersFilename = app.SharedParametersFilename;
                app.SharedParametersFilename = M_CreateSharedParametersFile();
                DefinitionFile sharedParameterFile = app.OpenSharedParameterFile();
                var curDef = MyUtils.GetParameterDefinitionFromFile(sharedParameterFile, "Dev_Group_Common", "Dev_Text_1");
                ElementBinding curBinding = doc.Application.Create.NewInstanceBinding(myCatSet);
#if REVIT2020 || REVIT2021 || REVIT2022 || REVIT2023
                var paramAdded = doc.ParameterBindings.Insert(curDef, curBinding, BuiltInParameterGroup.PG_IDENTITY_DATA);
#else // REVIT2024 || REVIT2025
                var paramAdded = doc.ParameterBindings.Insert(curDef, curBinding, GroupTypeId.IdentityData);
#endif

                M_AddByNameAvailableFieldToSchedule(doc, _curSchedule.Name, "Dev_Text_1");
                _UpdateMyUniqueIDColumn(doc, _curSchedule.Name);

                app.SharedParametersFilename = originalSharedParametersFilename;
                _curSchedule.Document.Regenerate();
            }
            catch (Exception ex)
            {
                app.SharedParametersFilename = originalSharedParametersFilename;
                Debug.Print("Error occurred in M_Add_Dev_Text_4: " + ex.Message);
            }
        }

        //public static void M_Add_Dev_Text_4(Autodesk.Revit.ApplicationServices.Application app, Document doc, ViewSchedule _curSchedule, CategorySet myCatSet)
        //{
        //    string originalSharedParametersFilename = null;
        //    try
        //    {
        //        // Define category for shared parameter

        //        // Save the original shared parameters filename
        //        originalSharedParametersFilename = app.SharedParametersFilename;

        //        // Set the path of the shared parameters file
        //        app.SharedParametersFilename = M_CreateSharedParametersFile();

        //        // Open the shared parameters file
        //        DefinitionFile sharedParameterFile = app.OpenSharedParameterFile();

        //        // Get the parameter definition from the shared parameters file
        //        var curDef = MyUtils.GetParameterDefinitionFromFile(sharedParameterFile, "Dev_Group_Common", "Dev_Text_1");

        //        // Create the binding
        //        ElementBinding curBinding = doc.Application.Create.NewInstanceBinding(myCatSet);

        //        // Insert the parameter into the document
        //        var paramAdded = doc.ParameterBindings.Insert(curDef, curBinding, BuiltInParameterGroup.PG_IDENTITY_DATA);


        //        // Add the available field to the schedule
        //        M_AddByNameAvailableFieldToSchedule(doc, _curSchedule.Name, "Dev_Text_1");

        //        // Update the "Dev_Text_1" column with the UniqueID 
        //        _UpdateMyUniqueIDColumn(doc, _curSchedule.Name);

        //        // Restore the original shared parameters filename
        //        app.SharedParametersFilename = originalSharedParametersFilename;

        //        // Regenerate the document
        //        _curSchedule.Document.Regenerate();
        //    }
        //    catch (Exception ex)
        //    {
        //        app.SharedParametersFilename = originalSharedParametersFilename;

        //        // Handle the exception
        //        Debug.Print("Error occurred in M_Add_Dev_Text_4: " + ex.Message);
        //    }
        //}

        public static BuiltInCategory _GetScheduleBuiltInCategory(ViewSchedule schedule)
        {
            Category scheduleCategory = schedule.Category;

#if REVIT2024 || REVIT2025
            BuiltInCategory builtInCategory = (BuiltInCategory)scheduleCategory.Id.Value;
#else
            BuiltInCategory builtInCategory = (BuiltInCategory)scheduleCategory.Id.IntegerValue;
#endif

            if (Enum.IsDefined(typeof(BuiltInCategory), builtInCategory))
            {
                return builtInCategory;
            }
            else
            {
                // Return a default value or throw an exception, based on your requirements
                return BuiltInCategory.INVALID;
            }
        }

        //public static BuiltInCategory _GetScheduleBuiltInCategory(ViewSchedule schedule)
        //{
        //    Category scheduleCategory = schedule.Category;
        //    BuiltInCategory builtInCategory = (BuiltInCategory)scheduleCategory.Id.IntegerValue;

        //    if (Enum.IsDefined(typeof(BuiltInCategory), builtInCategory))
        //    {
        //        return builtInCategory;
        //    }
        //    else
        //    {
        //        // Return a default value or throw an exception, based on your requirements
        //        return BuiltInCategory.INVALID;
        //    }
        //}

        public static BuiltInCategory _GetElementBuiltInCategory(Element element)
        {
            Document doc = element.Document;
            Category category = doc.GetElement(element.GetTypeId()).Category;

#if REVIT2024 || REVIT2025
            BuiltInCategory builtInCategory = (BuiltInCategory)category.Id.Value;
#else
            BuiltInCategory builtInCategory = (BuiltInCategory)category.Id.IntegerValue;
#endif

            if (Enum.IsDefined(typeof(BuiltInCategory), builtInCategory))
            {
                return builtInCategory;
            }
            else
            {
                return BuiltInCategory.INVALID;
            }
        }

        //public static BuiltInCategory _GetElementBuiltInCategory(Element element)
        //{
        //    Document doc = element.Document;
        //    Category category = doc.GetElement(element.GetTypeId()).Category;
        //    BuiltInCategory builtInCategory = (BuiltInCategory)category.Id.IntegerValue;

        //    if (Enum.IsDefined(typeof(BuiltInCategory), builtInCategory))
        //    {
        //        return builtInCategory;
        //    }
        //    else
        //    {
        //        // Return a default value or throw an exception, based on your requirements
        //        return BuiltInCategory.INVALID;
        //    }
        //}

        public static Category _GetScheduleCategory(Document doc, ViewSchedule schedule)
        {
            var CatID = schedule.Definition.CategoryId;
            Category _scheduleCategory = null;
            foreach (Category c in doc.Settings.Categories)
            {
                if (c.Id == CatID)
                {
                    Debug.Print($"CategoryName:{c.Name} ID:{c.Id} CategoryType:{c.CategoryType} ");
                    _scheduleCategory = c;
                    return _scheduleCategory;
                }
            }
            return null;
        }


        public List<BuiltInCategory> _GetAllBuiltInCategories()
        {
            List<BuiltInCategory> builtInCategories = new List<BuiltInCategory>();

            foreach (BuiltInCategory category in Enum.GetValues(typeof(BuiltInCategory)))
            {
                if (category != BuiltInCategory.INVALID)
                {
                    builtInCategories.Add(category);
                }
            }
            return builtInCategories;
        }


        public static BuiltInCategory _GetBuiltInCategoryFromCategory(Category category)
        {
            if (category != null && category.Id != null)
            {
#if REVIT2024 || REVIT2025
                long idValue = category.Id.Value;
#else
                int idValue = category.Id.IntegerValue;
#endif
                if (idValue >= 0)
                {
                    BuiltInCategory builtInCategory = (BuiltInCategory)idValue;
                    if (Enum.IsDefined(typeof(BuiltInCategory), builtInCategory))
                    {
                        return builtInCategory;
                    }
                }
            }

            return BuiltInCategory.INVALID;
        }

        //public static BuiltInCategory _GetBuiltInCategoryFromCategory(Category category)
        //{
        //    if (category != null && category.Id != null && category.Id.IntegerValue >= 0)
        //    {
        //        BuiltInCategory builtInCategory = (BuiltInCategory)category.Id.IntegerValue;
        //        if (Enum.IsDefined(typeof(BuiltInCategory), builtInCategory))
        //        {
        //            return builtInCategory;
        //        }
        //    }

        //    // Return a default value or throw an exception, based on your requirements
        //    return BuiltInCategory.INVALID;
        //}

        public static BuiltInCategory _GetBuiltInCategoryById(int categoryId)
        {
            BuiltInCategory builtInCategory = (BuiltInCategory)categoryId;

            if (Enum.IsDefined(typeof(BuiltInCategory), builtInCategory))
            {
                return builtInCategory;
            }
            else
            {
                // Return a default value or throw an exception, based on your requirements
                return BuiltInCategory.INVALID;
            }
        }

        //public void AddUniqueIdColumnToCsv(string filePath, string[] uniqueIds)
        public static void AddUniqueIdColumnToViewScheduleCsv(string filePath, List<string> uniqueIds)
        {
            // Wait for 1 second
            Thread.Sleep(200);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file does not exist.", filePath);
            }

            var csvLines = File.ReadAllLines(filePath);
            if (csvLines.Length < 3)
            {
                throw new InvalidOperationException("The specified file does not have the correct format of rows.");
            }

            // Add the UniqueID header to the first row
            csvLines[0] = csvLines[0] + ",";       // Update Row 0
            csvLines[1] = "UniqueID," + csvLines[1];  // Update Row 1
            csvLines[2] = csvLines[2] + ",";        // Update Row 2

            // Add the UniqueID values to each subsequent row
            for (int i = 3; i < csvLines.Length; i++)
            {
                csvLines[i] = uniqueIds[i - 3] + "," + csvLines[i];
            }

            // Write the modified CSV data to the same file
            File.WriteAllLines(filePath, csvLines, Encoding.UTF8);
        }

        //###########################################################################################
        public static CategorySet M_GetAllowBoundParamCategorySet(Document doc, ViewSchedule schedule)
        {
            CategorySet newCatSet = new CategorySet();
            foreach (Category _settingsCategory in doc.Settings.Categories)
            {
                if (_settingsCategory.AllowsBoundParameters)
                {
                    newCatSet.Insert(_settingsCategory);
                }
            }

            return newCatSet;
        }

        public static BuiltInCategory M_GetScheduleBuiltInCategory(Document doc, ViewSchedule schedule)
        {
            ElementId _scheduleDefinitionCategoryId = schedule.Definition.CategoryId;

            BuiltInCategory _scheduleBuiltInCategory = BuiltInCategory.INVALID;
            foreach (Category _settingsCategory in doc.Settings.Categories)
            {
                var curC = _GetBuiltInCategoryFromCategory(_settingsCategory);
                if (_settingsCategory.Id == _scheduleDefinitionCategoryId)
                {
#if REVIT2024 || REVIT2025
                    //_scheduleBuiltInCategory = _GetBuiltInCategoryById(_settingsCategory.Id.Value);
                    // Assuming _GetBuiltInCategoryById expects an int argument.
                    _scheduleBuiltInCategory = _GetBuiltInCategoryById((int)_settingsCategory.Id.Value);

#else
                    _scheduleBuiltInCategory = _GetBuiltInCategoryById(_settingsCategory.Id.IntegerValue);
#endif
                    break;
                }
            }

            return _scheduleBuiltInCategory;
        }

        //public static BuiltInCategory M_GetScheduleBuiltInCategory(Document doc, ViewSchedule schedule)
        //{
        //    ElementId _scheduleDefinitionCategoryId = schedule.Definition.CategoryId;

        //    BuiltInCategory _scheduleBuiltInCategory = new BuiltInCategory();
        //    foreach (Category _settingsCategory in doc.Settings.Categories)
        //    {
        //        var curC = _GetBuiltInCategoryFromCategory(_settingsCategory);
        //        //if (c.Id.IntegerValue == cId.IntegerValue)
        //        if (_settingsCategory.Id == _scheduleDefinitionCategoryId)
        //        {
        //            _scheduleBuiltInCategory = _GetBuiltInCategoryById(_settingsCategory.Id.IntegerValue);
        //            break;
        //        }
        //    }

        //    return _scheduleBuiltInCategory;
        //}
        //###########################################################################################
        public static void M_MoveCsvLastColumnToFirst(string filePath)
        {
            // Read all lines from the CSV file
            string[] lines = File.ReadAllLines(filePath);
            //string[] lines = File.ReadAllLines(@"C:\Users\ohernandez\Desktop\Revit_Exports\Mechanical Equipment Schedule.csv");

            // Get the header line
            string headerLine = lines[0];

            // Get the data lines excluding the header line
            string[] dataLines = lines.Skip(1).ToArray();

            // Split the header line and data lines by comma
            string[] headerColumns = headerLine.Split(',');
            string[][] dataColumns = dataLines.Select(line => line.Split(',')).ToArray();

            // Move the last column to column A
            for (int i = 0; i < dataColumns.Length; i++)
            {
                string lastColumn = dataColumns[i][dataColumns[i].Length - 1];

                for (int j = dataColumns[i].Length - 1; j > 0; j--)
                {
                    dataColumns[i][j] = dataColumns[i][j - 1];
                }

                dataColumns[i][0] = lastColumn;
            }

            // Merge the updated header and data columns
            string[] updatedLines = new string[dataColumns.Length + 1];
            updatedLines[0] = string.Join(",", headerColumns);

            for (int i = 0; i < dataColumns.Length; i++)
            {
                updatedLines[i + 1] = string.Join(",", dataColumns[i]);
            }

            // Write the updated lines back to the file
            File.WriteAllLines(filePath, updatedLines, Encoding.UTF8);
        }

        public static void M_MoveCsvLastColumnToFirst_temp(string filePath)
        {
            // Read all lines from the CSV file
            string[] lines = File.ReadAllLines(filePath);

            // Get the header line
            string headerLine = lines[0];

            // Get the data lines excluding the header line
            string[] dataLines = lines.Skip(1).ToArray();

            // Split the header line and data lines by comma
            string[] headerColumns = headerLine.Split(',');

            // Split each data line into columns
            string[][] dataColumns = dataLines.Select(line => SplitLine(line)).ToArray();

            // Move the last column to the first column
            for (int i = 0; i < dataColumns.Length; i++)
            {
                string lastColumn = dataColumns[i][dataColumns[i].Length - 1];

                for (int j = dataColumns[i].Length - 1; j > 0; j--)
                {
                    dataColumns[i][j] = dataColumns[i][j - 1];
                }

                dataColumns[i][0] = lastColumn;
            }

            // Merge the updated header and data columns
            string[] updatedLines = new string[dataColumns.Length + 1];
            updatedLines[0] = string.Join(",", EscapeColumns(headerColumns));

            for (int i = 0; i < dataColumns.Length; i++)
            {
                updatedLines[i + 1] = string.Join(",", EscapeColumns(dataColumns[i]));
            }

            // Write the updated lines back to the file
            File.WriteAllLines(filePath, updatedLines, Encoding.UTF8);
        }
        private static string[] SplitLine(string line)
        {
            var columns = new List<string>();
            StringBuilder columnBuilder = new StringBuilder();
            bool inQuotes = false;

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    columns.Add(columnBuilder.ToString().Trim().Trim('"'));
                    columnBuilder.Clear();
                }
                else
                {
                    columnBuilder.Append(c);
                }
            }

            columns.Add(columnBuilder.ToString().Trim().Trim('"'));
            return columns.ToArray();
        }

        private static string[] SplitLine_Old(string line)
        {
            // Custom implementation to split a CSV line while handling quotes and special characters
            // This implementation assumes the CSV follows the standard CSV format

            var columns = new List<string>();
            StringBuilder columnBuilder = new StringBuilder();
            bool inQuotes = false;

            foreach (char c in line)
            {
                if (c == '"')
                {
                    // Toggle inQuotes flag when encountering a quote
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    // Add column to the list when encountering a comma outside quotes
                    columns.Add(columnBuilder.ToString().Trim());
                    columnBuilder.Clear();
                }
                else
                {
                    // Append character to the current column
                    columnBuilder.Append(c);
                }
            }

            // Add the last column to the list
            columns.Add(columnBuilder.ToString().Trim());

            return columns.ToArray();
        }

        private static string[] EscapeColumns(string[] columns)
        {
            // Custom implementation to escape quotes and special characters in CSV columns
            // Modify this method according to your specific escaping requirements

            for (int i = 0; i < columns.Length; i++)
            {
                // Escape double quotes by doubling them
                columns[i] = columns[i].Replace("\"", "\"\"");

                // Add additional escaping logic for special characters if needed
                // Modify the escaping rules based on your specific use case
                // For example, you may need to escape newlines, tabs, or specific symbols
            }

            return columns;
        }

        //###########################################################################################

        public void M_AddScheduleUniqueIdToCellA1(string filePath, string newText)
        {
            // Read the existing CSV file
            string[] lines = File.ReadAllLines(filePath);

            // Move the existing text from A1 to B1 and insert new text in A1
            for (int i = 0; i < lines.Length; i++)
            {
                string[] cells = lines[i].Split(',');

                // Move the existing value from A1 to B1
                if (i == 0 && cells.Length > 1)
                {
                    cells[1] = cells[0];
                    // Insert the new text in A1
                    cells[0] = $"\"{newText}\"";
                }

                // Join cells back into a line
                lines[i] = string.Join(",", cells);
            }

            // Write the modified lines back to the CSV file
            File.WriteAllLines(filePath, lines, Encoding.UTF8);
        }

        //###########################################################################################
        /// <summary>
        /// This updated version takes into account quoted text fields by parsing each line character by character and keeping track of whether the current character is within quotes or not. It correctly handles commas within quoted text fields and ensures that the fields are split and joined correctly when moving the last column to the first position.

        ///Please note that this approach assumes that quoted text fields do not contain any escaped quotes.If your CSV file contains escaped quotes (e.g., "This is a ""quoted"" field"), additional handling may be required.
        /// </summary>
        /// <param name="csvFilePath"></param>
        public static void M_MoveCsvLastColumnToFirst2(string csvFilePath)
        {
            // Read all lines from the CSV file
            string[] lines = System.IO.File.ReadAllLines(csvFilePath);

            if (lines.Length > 0)
            {
                // Split the first line to get the column headers
                string[] headers = lines[0].Split(',');

                // Find the index of the last column
                int lastColumnIndex = headers.Length - 1;

                // Move the last column to the first position
                string lastColumnHeader = headers[lastColumnIndex];
                for (int i = lastColumnIndex; i > 0; i--)
                {
                    headers[i] = headers[i - 1];
                }
                headers[0] = lastColumnHeader;

                // Modify each line by moving the last column to the first position
                for (int i = 1; i < lines.Length; i++) // Start from index 1 to skip the header row
                {
                    List<string> values = new List<string>();
                    StringBuilder fieldValue = new StringBuilder();
                    bool withinQuotes = false;

                    foreach (char c in lines[i])
                    {
                        if (c == '"' && !withinQuotes)
                        {
                            withinQuotes = true;
                            fieldValue.Append(c);
                        }
                        else if (c == '"' && withinQuotes)
                        {
                            withinQuotes = false;
                            fieldValue.Append(c);
                        }
                        else if (c == ',' && !withinQuotes)
                        {
                            values.Add(fieldValue.ToString());
                            fieldValue.Clear();
                        }
                        else
                        {
                            fieldValue.Append(c);
                        }
                    }

                    values.Add(fieldValue.ToString());

                    string lastValue = values[lastColumnIndex];
                    for (int j = lastColumnIndex; j > 0; j--)
                    {
                        values[j] = values[j - 1];
                    }
                    values[0] = lastValue;

                    lines[i] = string.Join(",", values);
                }

                // Write the modified lines back to the CSV file
                System.IO.File.WriteAllLines(csvFilePath, lines, Encoding.UTF8);
            }
        }
        //##############################################################

        public static string M_CreateSharedParametersFile()
        {
            string data = @"# This is a Revit shared parameter file.
# Do not edit manually.
*META	VERSION	MINVERSION
META	2	1
*GROUP	ID	NAME
GROUP	1	Dev_Group_Common
*PARAM	GUID	NAME	DATATYPE	DATACATEGORY	GROUP	VISIBLE	DESCRIPTION	USERMODIFIABLE	HIDEWHENNOVALUE
PARAM	31fa72f6-6cd4-4ea8-9998-8923afa881e3	Dev_Text_1	TEXT		1	1		1	0";

            //33
            string outputFileName = @"ACCO -- Dev_Revit Shared Parameters.txt";
            string tempDirectory = System.IO.Path.GetTempPath();
            string tempFilePath = System.IO.Path.Combine(tempDirectory, outputFileName);

            try
            {
                // Write the data to the temp file
                System.IO.File.WriteAllText(tempFilePath, data);
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during file writing
                Console.WriteLine("An error occurred while writing the temp file: " + ex.Message);
                return null;
            }

            return tempFilePath;
        }

        public static ScheduleDefinition M_ShowHeadersAndTileOnSchedule(ViewSchedule curViewSchedule)
        {
            // get the current definitions of the schedule
            ScheduleDefinition curScheduleDefinition = curViewSchedule.Definition;

            // set the ShowTitle to True
            curScheduleDefinition.ShowTitle = true;

            // set the ShowHeaders to True
            curScheduleDefinition.ShowHeaders = true;

            // Return the original definitions
            return curScheduleDefinition;
        }

        public static void CheckAndPromptToCloseExcel(string filePath)
        {
            FileStream stream = null;

            string fileName = System.IO.Path.GetFileName(filePath);

            try
            {
                Debug.Print($"Checking if file: {fileName} is currently locked by excel");
                // Attempt to open the CSV file using a FileStream to check for exclusive access
                stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException ex)
            {
                // The file is locked, prompt the user to close Excel before continuing
                M_MyTaskDialog("Warning", $"The CSV file below is currently locked by Excel: " +
                                           $"\n{fileName}\n{ex} " +
                                           $"\n\nPlease close Excel, \nthen click CLOSE to continue!", "Warning");
                //return;
            }
            finally
            {
                stream?.Dispose();
            }

            // Continue with processing the CSV file
            // ...
        }
        public bool CheckIfFileIsOpen(string filePath)
        {
            bool fileIsOpen = false;
            if (File.Exists(filePath))
            {
                FileStream stream = null;

                string fileName = System.IO.Path.GetFileName(filePath);

                try
                {
                    Debug.Print($"Checking if file: {fileName} is currently locked by excel");
                    // Attempt to open the CSV file using a FileStream to check for exclusive access
                    stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    stream?.Dispose();
                    fileIsOpen = false;
                }
                catch (IOException)
                {
                    // The file is locked, prompt the user to close Excel before continuing
                    stream?.Dispose();
                    fileIsOpen = true;
                }
            }
            return fileIsOpen;
        }
        public void CheckAndPromptToCloseExcel(string filePath, string MessageToShow)
        {
            if (File.Exists(filePath))
            {
                FileStream stream = null;

                string fileName = System.IO.Path.GetFileName(filePath);

                try
                {
                    Debug.Print($"Checking if file: {fileName} is currently locked by excel");
                    // Attempt to open the CSV file using a FileStream to check for exclusive access
                    stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                }
                catch (IOException)
                {
                    // The file is locked, prompt the user to close Excel before continuing
                    M_MyTaskDialog("Warning", MessageToShow, "Warning");
                    //return;
                }
                finally
                {
                    stream?.Dispose();
                }

                // Continue with processing the CSV file
                // ...
            }

        }

        public void ExportViewScheduleBasic(ViewSchedule schedule, ExcelWorksheet worksheet) // bookmark CTRL + K K  . next K N
        {
            string scheduleUniqueId = schedule.UniqueId;
            string scheduleName = schedule.Name;
            var dt = CreateDataTable(schedule);

            if (dt.Rows.Count > 0)
            {
                var dtRearranged = RearrangeColumns(dt);
                dtRearranged = InsertUniqueId(dtRearranged, scheduleUniqueId, scheduleName);
                //var dtRearranged = InsertUniqueId(dt, scheduleUniqueId);

                worksheet.Cells.LoadFromDataTable(dtRearranged, true);
                WorksheetFormatting(worksheet);
            }
        }

        private void AutoFitColumns(ExcelWorksheet worksheet, int rowIndexToFitTo)
        {
            for (int columnIndex = 1; columnIndex <= worksheet.Dimension.Columns; columnIndex++)
            {
                var cellValue = worksheet.Cells[rowIndexToFitTo, columnIndex].Value;
                if (cellValue != null)
                {
                    var cellTextLength = cellValue.ToString().Length;
                    var column = worksheet.Column(columnIndex);
                    column.Width = cellTextLength + 2; // Adjust the value as needed
                }
            }
        }
        public DataTable CreateDataTable(ViewSchedule schedule)
        {
            var dt = new DataTable();

            // if schedule is not Itemized, change to itemized every instance
            if (!schedule.Definition.IsItemized)
                schedule.Definition.IsItemized = true;

            // Definition of columns
            var fieldsCount = schedule.Definition.GetFieldCount();
            for (var fieldIndex = 0; fieldIndex < fieldsCount; fieldIndex++)
            {
                var field = schedule.Definition.GetField(fieldIndex);
                if (field.IsHidden) continue;
                var columnName = field.GetName(); // Parameter names
                var fieldType = typeof(string);

                // Ensure column names are unique by appending a number if necessary
                var i = 1;
                while (dt.Columns.Contains(columnName))
                {
                    columnName = $"{field.GetName()}({i})";
                    i++;
                }

                dt.Columns.Add(columnName, fieldType);
            }

            // Content display
            var viewSchedule = schedule;
            var table = viewSchedule.GetTableData();
            var section = table.GetSectionData(SectionType.Body);
            var nRows = section.NumberOfRows;
            var nColumns = section.NumberOfColumns;

            if (nRows > 1)
            {
                // Set the values of the first row to the column headings
                var columnNameRow = dt.NewRow();

                int actualIndex = 0;
                for (var j = 0; j < nColumns; j++)
                {
                    //var field = viewSchedule.Definition.GetField(j);
                    var field = viewSchedule.Definition.GetField(actualIndex);
                    actualIndex++;
                    if (field.IsHidden)
                    {
                        j--;
                        continue;
                    }
                    columnNameRow[j] = field.ColumnHeading;
                }
                dt.Rows.Add(columnNameRow);

                // Populate data rows
                for (var i = 2; i < nRows; i++) // start at row index 2. to skip schedule title and header rows
                {
                    var dataRow = dt.NewRow();
                    for (var j = 0; j < nColumns; j++)
                    {
                        // Retrieve the cell value for each column
                        object val = viewSchedule.GetCellText(SectionType.Body, i, j); // Gets the displayed schedule data
                        if (val.ToString() != "")
                            dataRow[j] = val;
                    }
                    dt.Rows.Add(dataRow);
                }
            }

            return dt;
        }
        public DataTable RearrangeColumns(DataTable dt)
        {
            var lastColumnIndex = dt.Columns.Count - 1;
            var lastColumn = dt.Columns[lastColumnIndex];

            // Create a new DataTable with the columns rearranged
            var dtRearranged = new DataTable();
            dtRearranged.Columns.Add(lastColumn.ColumnName, lastColumn.DataType);
            foreach (DataColumn column in dt.Columns)
            {
                if (column != lastColumn)
                    dtRearranged.Columns.Add(column.ColumnName, column.DataType);
            }

            // Copy the data rows with the columns rearranged
            foreach (DataRow row in dt.Rows)
            {
                var newRow = dtRearranged.NewRow();
                newRow[0] = row[lastColumnIndex];
                for (var i = 0; i < lastColumnIndex; i++)
                    newRow[i + 1] = row[i];
                dtRearranged.Rows.Add(newRow);
            }

            return dtRearranged;
        }
        public DataTable InsertUniqueId(DataTable dt, string scheduleUniqueId, string scheduleName)
        {
            var newRowAtIndex0 = dt.NewRow();
            newRowAtIndex0[0] = scheduleUniqueId;
            newRowAtIndex0[1] = scheduleName;
            dt.Rows.InsertAt(newRowAtIndex0, 0);

            return dt;
        }
        #region Excel Formatting methods
        public void WorksheetFormatting(ExcelWorksheet worksheet)
        {
            FormatRow3Style(worksheet);
            AutoFitColumns(worksheet, 3);
            HideFirstTwoRows(worksheet);
            HideFirstColumn(worksheet);
            FreezeFirstThreeRows(worksheet);
        }
        public void HideFirstTwoRows(ExcelWorksheet worksheet)
        {
            // Hide the first two rows
            worksheet.Row(1).Hidden = true;
            //worksheet.Row(2).Hidden = true;
        }
        public void HideFirstColumn(ExcelWorksheet worksheet)
        {
            // Hide the first column
            worksheet.Column(1).Hidden = true;
        }
        public void FreezeFirstThreeRows(ExcelWorksheet worksheet)
        {
            // Freeze the first three rows
            worksheet.View.FreezePanes(4, 1);
        }
        public void FormatRow3Style(ExcelWorksheet worksheet)
        {
            var lastColumnIndex = worksheet.Dimension.End.Column;

            // Apply bold font to the row
            worksheet.Cells[3, 1, 3, lastColumnIndex].Style.Font.Bold = true;

            // Set the background color to light gray for cells with content
            for (int columnIndex = 1; columnIndex <= lastColumnIndex; columnIndex++)
            {
                var cell = worksheet.Cells[3, columnIndex];
                if (cell.Value != null)
                {
                    cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }
            }
        }
        #endregion

        public void M_AutoFitColumns(ExcelWorksheet worksheet, int rowIndex)
        {
            for (int columnIndex = 1; columnIndex <= worksheet.Dimension.Columns; columnIndex++)
            {
                var cellValue = worksheet.Cells[rowIndex, columnIndex].Value;

                if (cellValue != null)
                {
                    var cellTextLength = cellValue.ToString().Length;
                    var column = worksheet.Column(columnIndex);
                    column.Width = cellTextLength + 2; // Adjust the value as needed
                }
            }
        }


        public static ExcelPackage Create_ExcelFile(string filePath)
        {
            // Set EPPlus license context
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;  // Set the license context for EPPlus to NonCommercial

            // Open Excel file using EPPlus library
            ExcelPackage excelFile = new ExcelPackage(filePath);  // Create an instance of ExcelPackage by providing the file path
                                                                  //ExcelWorkbook workbook = excelFile.Workbook;  // Get the workbook from the Excel package
                                                                  // ExcelWorksheet worksheet = workbook.Worksheets[1];  // Get the first worksheet (index 0) from the workbook
                                                                  //ExcelWorksheet worksheet = workbook.Worksheets.Add("Sheet1");

            return excelFile;
        }
        #endregion
        //==================================================Importer methods
        public static List<ExcelWorksheet> M_ReadExcelFile(ExcelPackage excelPackage)
        {
            List<ExcelWorksheet> sheets = new List<ExcelWorksheet>();

            // Load the Excel package and retrieve the worksheets
            ExcelWorkbook workbook = excelPackage.Workbook;
            foreach (ExcelWorksheet worksheet in workbook.Worksheets)
            {
                sheets.Add(worksheet);
            }

            return sheets;
        }

        public List<ExcelWorksheet> M_ReadExcelFile_1(string filePath)
        {
            // Set EPPlus license context
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;  // Set the license context for EPPlus to NonCommercial

            List<ExcelWorksheet> sheets = new List<ExcelWorksheet>();

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                // Handle file not found error
                // You can throw an exception or handle it in a way that suits your needs
                M_MyTaskDialog("Error", $"The Excel file does not exist: {filePath}", "Error");
                return null;
            }

            // Load the Excel file using EPPlus
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                // Loop through each worksheet in the Excel file
                foreach (var sheet in package.Workbook.Worksheets)
                {
                    sheets.Add(sheet);
                }
            }

            return sheets;
        }

        public ScheduleData GetScheduleDataFromSheet(ExcelWorksheet sheet)
        {
            ScheduleData scheduleData = new ScheduleData();

            // Retrieve the schedule's unique ID from cell A2
            string uniqueId = sheet.Cells["A2"].GetValue<string>();

            // Retrieve parameter names starting from cell B1
            int parameterNameRow = 1;
            int parameterNameColumn = 2;
            while (!string.IsNullOrEmpty(sheet.Cells[parameterNameRow, parameterNameColumn].GetValue<string>()))
            {
                string parameterName = sheet.Cells[parameterNameRow, parameterNameColumn].GetValue<string>();
                scheduleData.ParameterNames.Add(parameterName);
                parameterNameColumn++;
            }

            // Retrieve scheduled element unique IDs starting from cell A4
            int uniqueIdRow = 4;
            while (!string.IsNullOrEmpty(sheet.Cells[uniqueIdRow, 1].GetValue<string>()))
            {
                string elementUniqueId = sheet.Cells[uniqueIdRow, 1].GetValue<string>();
                scheduleData.ElementUniqueIds.Add(elementUniqueId);
                uniqueIdRow++;
            }

            // Retrieve parameter values starting from cell B4
            int parameterValueRow = 4;
            foreach (string elementUniqueId in scheduleData.ElementUniqueIds)
            {
                List<string> parameterValues = new List<string>();
                for (int column = 2; column <= scheduleData.ParameterNames.Count + 1; column++)
                {
                    string parameterValue = sheet.Cells[parameterValueRow, column].GetValue<string>();
                    parameterValues.Add(parameterValue);
                }
                scheduleData.ParameterValues.Add(elementUniqueId, parameterValues);
                parameterValueRow++;
            }

            // Set the schedule's unique ID in the scheduleData object
            scheduleData.UniqueId = uniqueId;

            return scheduleData;
        }
        public static void ImportSchedules(Document doc, ScheduleData scheduleData)
        {
            // Get the view schedule by unique ID
            ViewSchedule schedule = M_GetViewScheduleByUniqueId(doc, scheduleData.UniqueId);

            if (schedule == null)
            {
                // Schedule not found, handle the error or return
                return;
            }

            foreach (string elementUniqueId in scheduleData.ElementUniqueIds)
            {
                // Find the element by UniqueId using a suitable method for your case
                Element element = M_GetElementByUniqueId(doc, schedule, elementUniqueId);
                if (element == null)
                {
                    // If element is not found, print to debug
                    Debug.Print($"Schedule element UniqueId not found: {elementUniqueId}");
                    continue;
                }

                // Set parameter values
                List<string> parameterValues;
                if (scheduleData.ParameterValues.TryGetValue(elementUniqueId, out parameterValues))
                {
                    ScheduleDefinition scheduleDef = schedule.Definition;

                    for (int i = 0; i < scheduleData.ParameterNames.Count; i++)
                    {
                        string parameterName = scheduleData.ParameterNames[i];    // Parameter name from excel sheet
                        Parameter parameter = element.LookupParameter(parameterName); // Find the parameter in the current schedule
                        if (parameter != null && parameter.IsReadOnly == false && parameter.StorageType == StorageType.String)
                        {
                            string parameterValue = parameterValues[i];
                            parameter.Set(parameterValue);
                        }
                        else // This else statement can be commented out if no logging is disired.
                        {
                            // Create a log file with the current document name and time in the user's temp folder
                            string logFileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{doc.Title}_ImportLog.txt");

                            // Get the current timestamp
                            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                            // Write the schedule name, the name of the parameter, the reason why the parameter did not get set, and the timestamp
                            string logEntry = $"{timeStamp}: Schedule: {schedule.Name}, Parameter: {parameterName}, Reason: ";

                            if (parameter == null)
                                logEntry += "Parameter not found";
                            else if (parameter.IsReadOnly)
                                logEntry += "Parameter is read-only";
                            else if (parameter.StorageType != StorageType.String)
                                logEntry += "Parameter is not of string type";

                            // Append the log entry to the log file
                            File.AppendAllText(logFileName, logEntry + Environment.NewLine);
                        }
                    }
                }
            }
        }

        public static void ImportSchedules1(Document doc, ScheduleData scheduleData)
        {
            // Get the view schedule by unique ID
            ViewSchedule schedule = M_GetViewScheduleByUniqueId(doc, scheduleData.UniqueId);

            if (schedule == null)
            {
                // Schedule not found, handle the error or return
                return;
            }

            foreach (string elementUniqueId in scheduleData.ElementUniqueIds)
            {
                // Find the element by UniqueId using a suitable method for your case
                Element element = M_GetElementByUniqueId(doc, schedule, elementUniqueId);
                if (element == null)
                {
                    // If element is not found, print to debug
                    Debug.Print($"Schedule element UniqueId not found: {elementUniqueId}");
                    continue;
                }

                // Set parameter values
                List<string> parameterValues;
                if (scheduleData.ParameterValues.TryGetValue(elementUniqueId, out parameterValues))
                {
                    ScheduleDefinition scheduleDef = schedule.Definition;

                    for (int i = 0; i < scheduleData.ParameterNames.Count; i++)
                    {
                        string parameterName = scheduleData.ParameterNames[i];    // Parameter name from excel sheet
                        Parameter parameter = element.LookupParameter(parameterName); // Find the parameter in the current schedule
                        if (parameter != null && parameter.IsReadOnly == false && parameter.StorageType == StorageType.String)
                        {
                            string parameterValue = parameterValues[i];
                            parameter.Set(parameterValue);
                        }
                        else
                        {
                            // Chat GPT. Add a method that creates a log file with the current document name and time in the user's tempfolder 
                            // write the schedule name, the name of the parameter and reason why the parameter did not get set
                        }
                    }
                }
            }
        }

        private static Element M_GetElementByUniqueId(Document doc, ViewSchedule schedule, string elementUniqueId)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc, schedule.Id);
            ICollection<Element> elements = collector.WhereElementIsNotElementType().ToElements();

            foreach (Element element in elements)
            {
                var parameterUniqueId = element.UniqueId;
                if (parameterUniqueId != null && parameterUniqueId == elementUniqueId)
                {
                    return element;
                }
            }

            return null;
        }

        private static Parameter GetParameterByName(Element element, string parameterName)
        {
            foreach (Parameter parameter in element.Parameters)
            {
                if (parameter.Definition.Name == parameterName)
                {
                    return parameter;
                }
            }
            return null;
        }

        private static Element M_GetElementById(Document doc, string _elementId)
        {
#if REVIT2024 || REVIT2025
            ElementId elementId = new ElementId(long.Parse(_elementId));
#else
            ElementId elementId = new ElementId(int.Parse(_elementId));
#endif
            return doc.GetElement(elementId);
        }

        //private static Element M_GetElementById(Document doc, string _elementId)
        //{
        //    ElementId elementId = new ElementId(int.Parse(_elementId));
        //    return doc.GetElement(elementId);
        //}

        private static ScheduleFieldId M_GetScheduleFieldIdByName(ScheduleDefinition scheduleDef, string parameterName)
        {
            foreach (ScheduleFieldId fieldId in scheduleDef.GetFieldOrder())
            {
                ScheduleField field = scheduleDef.GetField(fieldId);
                if (field != null && field.GetName() == parameterName)
                {
                    return fieldId;
                }
            }

            return null;
        }

        private static ScheduleSheetInstance M_GetScheduleSheetInstanceByUniqueId(Document doc, ViewSchedule schedule, string elementUniqueId)
        {
            FilteredElementCollector instances = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_ScheduleGraphics)
                .OfClass(typeof(ScheduleSheetInstance));

            foreach (ScheduleSheetInstance instance in instances)
            {
                if (instance.OwnerViewId == schedule.Id && instance.UniqueId == elementUniqueId)
                {
                    return instance;
                }
            }

            return null;
        }


        // ========================= Graphical interface methods
        public static string M_GetExcelFilePath()
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Filter = "Excel Files|*.xlsx;*.xls";
                openFileDialog.Multiselect = false;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = "Select Exported Excel file with Schedule(s) to Import";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return openFileDialog.FileName;
                }
            }

            return null;
        }
        public List<ViewSchedule> M_GetScheduleByUniqueIdFromExcelSheet(Document doc, List<ExcelWorksheet> excelFile)
        {
            List<ViewSchedule> schedulesList = new List<ViewSchedule>();

            foreach (ExcelWorksheet sheet in excelFile)
            {
                string uniqueId = sheet.Cells["A2"].GetValue<string>();
                if (!string.IsNullOrEmpty(uniqueId))
                {
                    ViewSchedule schedule = M_GetViewScheduleByUniqueId(doc, uniqueId);
                    if (schedule != null)
                    {
                        schedulesList.Add(schedule);
                    }
                }
            }

            return schedulesList;
        }
        public List<string> M_GetScheduleNameByUniqueIdFromExcelSheet(Document doc, List<ExcelWorksheet> excelFile)
        {
            List<string> schedulesNameList = new List<string>();

            foreach (ExcelWorksheet sheet in excelFile)
            {
                string uniqueId = sheet.Cells["A2"].GetValue<string>();
                if (!string.IsNullOrEmpty(uniqueId))
                {
                    ViewSchedule schedule = M_GetViewScheduleByUniqueId(doc, uniqueId);
                    if (schedule != null)
                    {
                        schedulesNameList.Add(schedule.Name);
                    }
                }
            }

            return schedulesNameList;
        }

        public ExcelWorksheet M_GetWorksheetByCellA2(string uniqueId, List<ExcelWorksheet> worksheets)
        {
            //This method iterates through the list of ExcelWorksheet objects and checks the value in cell A2.
            //If the value matches the provided uniqueId, it returns the corresponding worksheet.If no matching worksheet is found, it returns null.
            foreach (ExcelWorksheet worksheet in worksheets)
            {
                var cellA2Value = worksheet.Cells["A2"].GetValue<string>();
                if (!string.IsNullOrEmpty(cellA2Value) && cellA2Value == uniqueId)
                {
                    return worksheet;
                }
            }
            return null; // Worksheet not found
        }
        public static void M_ShowCurrentFormForNSeconds(RevitRibbon_MainSourceCode_Resources.MyForm currentForm, int NumOfSeconds)
        {
            currentForm.Show();
            Task.Run(async () =>
            {
                await Task.Delay(NumOfSeconds * 1000);

                // Close the form on the UI thread using Dispatcher.Invoke
                currentForm.Dispatcher.Invoke(() =>
                {
                    currentForm.Close();
                });
            });
        }
        public static void M_ShowCurrentFormForNSeconds2(RevitRibbon_MainSourceCode_Resources.MyForm currentForm, int NumOfSeconds)
        {
            currentForm.Show();
            Task.Run(async () =>
            {
                await Task.Delay(NumOfSeconds * 1000);

                // Close the form on the UI thread using Dispatcher.Invoke
                currentForm.Dispatcher.Invoke(() =>
                {
                    currentForm.Close();
                });
            });
        }

        public static TaskDialogResult TaskDialogNotifyUserFileAlreadyExists(string excelFilePath, string messageTitle, string textMessage)
        {
            if (File.Exists(excelFilePath))
            {
                var taskDialog = new TaskDialog(messageTitle);
                taskDialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.Cancel;
                taskDialog.MainContent = textMessage;
                var result = taskDialog.Show();
                return result;
            }
            return TaskDialogResult.Yes;
        }
        //==================================
        public bool M_TellTheUserIfFileExistsOrIsOpen(string _excelFilePath)
        {
            CheckAndPromptToCloseExcel(_excelFilePath, $"The existing Excel file is open. \nPlease close the file before you proceed to close this prompt!"); // Promt the user if the file is open

            if (CheckIfFileIsOpen(_excelFilePath)) // if the file is open return true to cancel the export
            {
                M_MyTaskDialog("Info", "The existing Excel file was not closed.\nExport Cancelled!", "Information");

                return true;
            }
            else
            {
                if (File.Exists(_excelFilePath))
                {
                    File.Delete(_excelFilePath);// If the file exists, delete it.
                }
                return false;
            }
        }
        public bool M_TellTheUserIfFileExistsOrIsOpen1(string _excelFilePath)
        {
            var tdialogresult = TaskDialogNotifyUserFileAlreadyExists(_excelFilePath, "Warning", $"{_excelFilePath}\n\nThe file already exists. Do you want to overwrite it?");
            if (tdialogresult == TaskDialogResult.Yes)
            {
                CheckAndPromptToCloseExcel(_excelFilePath, $"The existing file is opened. \nPlease close the file before you proceed to close this prompt!"); // Promt the user if the file is open
                File.Delete(_excelFilePath);// If the file exists, delete it.
                return false;
            }
            else { return true; }
        }
        public bool M_TellTheUserIfFileExistsOrIsOpen2(string _excelFilePath)
        {
            var tdialogresult = TaskDialogNotifyUserFileAlreadyExists(_excelFilePath, "Warning", $"{_excelFilePath}\n\nThe file already exists. Do you want to overwrite it?");
            if (tdialogresult == TaskDialogResult.Yes)
            {
                CheckAndPromptToCloseExcel(_excelFilePath, $"The existing file is opened. \nPlease close the file before you proceed to close this prompt!"); // Promt the user if the file is open
                File.Delete(_excelFilePath);// If the file exists, delete it.
                return false;
            }
            else { return true; }
        }
        public string M_ExportPickedFolder()
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                DialogResult result = folderDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
                {
                    return folderDialog.SelectedPath;
                }

                return null; // Return null if no folder was selected or the dialog was canceled
            }
        }

        public string M_ExcelSaveAs(string defaultFileName)
        {
            using (var saveDialog = new System.Windows.Forms.SaveFileDialog())
            {
                saveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx;*.xls"; // Set the file filters
                saveDialog.DefaultExt = "xlsx"; // Set the default file extension
                saveDialog.FileName = defaultFileName; // Set the default file name

                DialogResult result = saveDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(saveDialog.FileName))
                {
                    return saveDialog.FileName;
                }

                return null; // Return null if no file was selected or the dialog was canceled
            }
        }

        /// <summary>
        /// Zooms to fit the active view in Autodesk Revit.
        /// This is like using ZoomExtents in AutoCad
        /// </summary>
        /// <param name="uiDoc">The UIDocument representing the active document.</param>
        internal static void ZoomToFitActiveView(UIDocument uiDoc)
        {
            Autodesk.Revit.DB.View curView = uiDoc.Document.ActiveView;

#if REVIT2024 || REVIT2025
            UIView activeUIView = uiDoc.GetOpenUIViews()
                                       .FirstOrDefault(uiv => uiv.ViewId.Value == curView.Id.Value);
#else
            UIView activeUIView = uiDoc.GetOpenUIViews()
                                       .FirstOrDefault(uiv => uiv.ViewId.IntegerValue == curView.Id.IntegerValue);
#endif

            if (activeUIView != null && activeUIView.IsValidObject)
            {
                activeUIView.ZoomToFit();
            }
            else
            {
                M_MyTaskDialog("Info", "The ZoomToFit of Current view was unable to process.", "Information");
            }
        }

        //internal static void ZoomToFitActiveView(UIDocument uiDoc)
        //{
        //    // Get the active view from the UIDocument
        //    Autodesk.Revit.DB.View curView = uiDoc.Document.ActiveView;

        //    // Use LINQ to find the UIView for the active view
        //    UIView activeUIView = uiDoc.GetOpenUIViews()
        //                               .FirstOrDefault(uiv => uiv.ViewId.IntegerValue == curView.Id.IntegerValue);

        //    // Check if the UIView is valid and zoom to fit
        //    if (activeUIView != null && activeUIView.IsValidObject)
        //    {
        //        activeUIView.ZoomToFit();  // Zoom the UIView to fit its contents <===================
        //    }
        //    else
        //    {
        //        // Handle the case where the UIView is not valid
        //        // You can throw an exception or log an error message here
        //        MyUtils.M_MyTaskDialog("Info", "The ZoomToFit of Current view was unable to process.");
        //    }
        //}

        // Scope Boxes
        public static Element GetSelectedScopeBox(Document doc, UIApplication uiapp)
        {
            // Get the current selection
            Selection selection = uiapp.ActiveUIDocument.Selection;

            // Retrieve the selected elements
            ICollection<ElementId> selectedElementIds = selection.GetElementIds();

            // Ensure that one and only one element is selected
            if (selectedElementIds.Count != 1)
            {
                //TaskDialog.Show("Error", "You must pre-select an already drawn scope boxe. \nTry again.");
                M_MyTaskDialog("Selection Required", "Please select a single Scope Box before proceeding.", "Warning");
                return null;
            }

            // Get the selected Scope Box
            var selectedElement = doc.GetElement(selectedElementIds.First());

            // Ensure that the selected element is a Scope Box
            if (selectedElement.Category.Name != "Scope Boxes")
            {
                //TaskDialog.Show("Error", "You did not select a Scope Box.");
                M_MyTaskDialog("Selection Required", "Please select a single Scope Box before proceeding.", "Warning");
                return null;
            }
            return selectedElement;
        }

        /// <summary>
        /// Must pass in a View and the required distance in feet at a known scale.
        /// Example 1: Revit view scale = 48 => 2 feet
        /// Example 2: offSet = MyUtils.GetViewScaleMultipliedValue(doc.ActiveView, 48, 2);  used for ScopeBox Overlap
        /// Example 3: offSet = MyUtils.GetViewScaleMultipliedValue(doc.ActiveView, 48, 3.5);  used for View Reference Insert Point
        /// This will returned the value needed at the current view scale
        /// </summary>
        /// <param name="currentView"></param>
        /// <param name="baseNum"></param>
        /// <returns></returns>
        public static double GetViewScaleMultipliedValue(Autodesk.Revit.DB.View currentView, double baseScaleNum, double baseNum)
        {
            double viewScale = currentView.Scale;
            //double baseScaleNum = 48;
            double multiplier = baseScaleNum / viewScale;
            double calculatedDistance = baseNum / multiplier;
            return calculatedDistance;
        }

        public static string GetViewScaleString(Autodesk.Revit.DB.View currentView)
        {
            int CurViewScale = currentView.Scale;
            // Define the scale mappings based on the provided CSV data
            var scaleMappings =
            new (int scaleNum, string viewScaleString)[]
                {
                    (1,"12\" = 1'-0\""),
                    (2,"6\" = 1'-0\""),
                    (4,"3\" = 1'-0\""),
                    (8,"1-1/2\" = 1'-0\""),
                    (12,"1\" = 1'-0\""),
                    (16,"3/4\" = 1'-0\""),
                    (24,"1/2\" = 1'-0\""),
                    (32,"3/8\" = 1'-0\""),
                    (48,"1/4\" = 1'-0\""),
                    (64,"3/16\" = 1'-0\""),
                    (96,"1/8\" = 1'-0\""),
                    (120,"1\" = 10'-0\""),
                    (128,"3/32\" = 1'-0\""),
                    (192,"1/16\" = 1'-0\""),
                    (240,"1\" = 20'-0\""),
                    (256,"3/64\" = 1'-0\""),
                    (360,"1\" = 30'-0\""),
                    (384,"1/32\" = 1'-0\""),
                    (480,"1\" = 40'-0\""),
                    (600,"1\" = 50'-0\""),
                    (720,"1\" = 60'-0\""),
                    (768,"1/64\" = 1'-0\""),
                    (960,"1\" = 80'-0\""),
                    (1200,"1\" = 100'-0\""),
                    (1920,"1\" = 160'-0\""),
                    (2400,"1\" = 200'-0\""),
                    (3600,"1\" = 300'-0\""),
                    (4800,"1\" = 400'-0\""),
                };


            string ViewScaleString = "-";
            // Find the corresponding scale mapping
            foreach (var mapping in scaleMappings)
            {
                if (CurViewScale == mapping.scaleNum)
                {
                    ViewScaleString = mapping.viewScaleString;
                    break;
                }
                else
                {
                    ViewScaleString = "Custome Scale";
                }
            }

            //// Handle the case where no matching scale is found
            //throw new ArgumentException("Unsupported view scale.");

            return ViewScaleString;
        }

        /// <summary>
        /// Sets the value of a specified parameter on an element to a given value.
        /// </summary>
        /// <typeparam name="T">The type of the value being set. Supported types are string, double, and ElementId.</typeparam>
        /// <param name="curElem">The element whose parameter is being set.</param>
        /// <param name="paramName">The name of the parameter to set.</param>
        /// <param name="value">The value to set the parameter to. The method checks the type of this value against the parameter's storage type.</param>
        /// <exception cref="ArgumentException">Thrown when the type of T is unsupported or does not match the parameter's storage type.</exception>
        /// <remarks>
        /// This method iterates through all parameters of the provided element and sets the value of the parameter
        /// matching the provided name. The method determines what Set overload to call based on the type of the value provided:
        /// - For string values, the method expects the parameter's storage type to be StorageType.String.
        /// - For double values, the method expects the parameter's storage type to be StorageType.Double.
        /// - For ElementId values, the method expects the parameter's storage type to be StorageType.ElementId.
        /// If the parameter is found but the types do not align (e.g., trying to set a string value to a parameter that
        /// expects a double), an ArgumentException is thrown.
        /// </remarks>
        public bool SetParameterValue<T>(Element curElem, string paramName, T value)
        {
            foreach (Parameter curParam in curElem.Parameters)
            {
                if (curParam.Definition.Name == paramName)
                {
                    try
                    {
                        if (typeof(T) == typeof(string) && curParam.StorageType == StorageType.String)
                        {
                            curParam.Set(value as string);
                            return true;  // Value set successfully, exit the method.
                        }
                        else if (typeof(T) == typeof(double) && curParam.StorageType == StorageType.Double)
                        {
                            curParam.Set(Convert.ToDouble(value));
                            return true;  // Value set successfully, exit the method.
                        }
                        else if (typeof(T) == typeof(ElementId) && curParam.StorageType == StorageType.ElementId)
                        {
                            curParam.Set(value as ElementId);
                            return true;  // Value set successfully, exit the method.
                        }
                        else
                        {
                            // Unsupported type or storage type mismatch
                            throw new ArgumentException($"Type {typeof(T).Name} is not supported or does not match the parameter's storage type.");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error or handle it as needed.
                        Debug.WriteLine($"Error setting parameter '{paramName}': {ex.Message}");
                        return false;  // Indicate failure.
                    }
                }
            }

            // Parameter with the given name not found or value not set; return false.
            return false;
        }

        public static List<Level> GetAllLevels(Document doc)
        {
            // Get all the levels
            return new FilteredElementCollector(doc)
                         .OfClass(typeof(Level))
                         .Cast<Level>()
                         .OrderBy(l => l.Elevation)
                         .ToList();
        }
        /// <summary>
        /// This method returns a Dictionary with the Mapped out View Scales
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, string> ScalesList()
        {
            var scaleMappings = new (int scaleNum, string ViewScaleString)[]
            {
                (1, "12\" = 1'-0\""),
                (2, "6\" = 1'-0\""),
                (4, "3\" = 1'-0\""),
                (8, "1-1/2\" = 1'-0\""),
                (12, "1\" = 1'-0\""),
                (16, "3/4\" = 1'-0\""),
                (24, "1/2\" = 1'-0\""),
                (32, "3/8\" = 1'-0\""),
                (48, "1/4\" = 1'-0\""),
                (64, "3/16\" = 1'-0\""),
                (96, "1/8\" = 1'-0\""),
                (120, "1\" = 10'-0\""),
                (128, "3/32\" = 1'-0\""),
                (192, "1/16\" = 1'-0\""),
                (240, "1\" = 20'-0\""),
                (256, "3/64\" = 1'-0\""),
                (360, "1\" = 30'-0\""),
                (384, "1/32\" = 1'-0\""),
                (480, "1\" = 40'-0\""),
                (600, "1\" = 50'-0\""),
                (720, "1\" = 60'-0\""),
                (768, "1/64\" = 1'-0\""),
                (960, "1\" = 80'-0\""),
                (1200, "1\" = 100'-0\""),
                (1920, "1\" = 160'-0\""),
                (2400, "1\" = 200'-0\""),
                (3600, "1\" = 300'-0\""),
                (4800, "1\" = 400'-0\""),
            };
            var scaleDictionary = new Dictionary<int, string>();

            foreach (var (scaleNum, viewScaleString) in scaleMappings)
            {
                scaleDictionary[scaleNum] = viewScaleString;
            }

            return scaleDictionary;
        }

        public static string ConvertSpaceToAlt255(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), "Input string cannot be null");
            }

            // Replace spaces with Alt+255 " " (non-breaking space)
            string result = input.Replace(' ', ' ');
            //string result = input.Replace(' ', '\u00A0');
            //string result = input.Replace(' ', '8');


            return result;
        }
        public static int GetUnicodeInt(char character)
        {
            return (int)character;
        }
        public static string GetUnicodeValue(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Input string cannot be null or empty", nameof(input));
            }

            // Get the first character of the input string
            char character = input[0];
            int unicodeValue = (int)character;
            string unicodeString = $"\\u{unicodeValue:X4}";

            string ReturnString = $"Character: '{character}', Decimal: {unicodeValue}, Unicode: {unicodeString}";
            //TaskDialog.Show("Character to Unicode", $"{ReturnString}");
            M_MyTaskDialog("Character to Unicode", $"{ReturnString}", "Information");
            return ReturnString;
        }

        public static List<View> GetDependentViewsFromParentView(View parentView)
        {
            // Get the document from the parent view
            Document doc = parentView.Document;

            // Get the IDs of the dependent elements
            ICollection<ElementId> dependentElementIds = parentView.GetDependentElements(null);

            // Filter the dependent elements to include only views and exclude the parent view
            List<View> dependentViews = new List<View>();
            foreach (ElementId id in dependentElementIds)
            {
                Element element = doc.GetElement(id);
                if (element is View dependentView && dependentView.Id != parentView.Id)
                {
                    dependentViews.Add(dependentView);
                }
            }

            return dependentViews;
        }

        public static Result GetBIMSetupView(Document doc, out View BIMSetupView)
        {
            BIMSetupView = Cmd_DependentViewsBrowserTree.GetAllViews(doc)
                                                        .Where(v => v.Name.StartsWith("BIM Setup View") && !v.IsTemplate && v.GetPrimaryViewId() == ElementId.InvalidElementId)
                                                        .FirstOrDefault();
            if (BIMSetupView == null)
            {
                MyUtils.M_MyTaskDialog("Action Required", "Please create the 'BIM Setup View' before proceeding.", "Warning");
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }

        public static string GetUniqueViewName(Document doc, string baseName)
        {
            if (!ViewNameExists(doc, baseName))
            {
                return baseName;
            }

            int suffix = 1;
            string newName;

            do
            {
                newName = $"{baseName}({suffix})";
                suffix++;
            }
            while (ViewNameExists(doc, newName));

            return newName;
        }

        public static bool ViewNameExists(Document doc, string viewName)
        {
            return new FilteredElementCollector(doc)
                   .OfClass(typeof(View))
                   .Cast<View>()
                   .Any(v => v.Name.Equals(viewName, StringComparison.OrdinalIgnoreCase));
        }

        public static ViewPlan CreateFloorPlanView(Document doc, string viewName, Level level = null)
        {
            // Ensure a valid document and view name are provided
            if (doc == null || string.IsNullOrWhiteSpace(viewName))
            {
                throw new ArgumentNullException("Document and view name cannot be null or empty.");
            }

            try
            {
                // Find the appropriate view family type for the floor plan view
                ViewFamilyType viewFamilyType = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewFamilyType))
                    .Cast<ViewFamilyType>()
                    .FirstOrDefault(vft => vft.ViewFamily == ViewFamily.FloorPlan);

                if (viewFamilyType == null)
                {
                    throw new InvalidOperationException("No ViewFamilyType found for FloorPlan.");
                }

                // Get the first level in the document to create the floor plan view
                if (level == null)
                {
                    level = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .FirstOrDefault() as Level;
                }

                if (level == null)
                {
                    throw new InvalidOperationException("No levels found in the document.");
                }

                // Create the new floor plan view
                ViewPlan newFloorPlanView = ViewPlan.Create(doc, viewFamilyType.Id, level.Id);

                // Assign a name to the newly created floor plan view
                if (newFloorPlanView != null)
                {
                    newFloorPlanView.Name = viewName;
                }


                return newFloorPlanView;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create floor plan view.", ex);
            }
        }
        public static void SetViewBrowserCategory(ViewPlan view)
        {
            if (view != null)
            {
                // Assuming "Browser Category" and "Browser Sub-Category" are shared parameters
                Parameter browserCategoryParam = view.LookupParameter("Browser Category");
                Parameter browserSubCategoryParam = view.LookupParameter("Browser Sub-Category");
                if (browserCategoryParam != null)
                {
                    browserCategoryParam.Set("__BIM Setup__");
                    if (browserSubCategoryParam != null)
                    {
                        browserSubCategoryParam.Set("BIM FloorPlan");
                    }
                }
                else
                {
                    // TaskDialog.Show("Info", $"The 'Browser Category' Parameter was not found. \nThe View '{view.Name}' will be placed in the default 'Floor Plans' Category on the project browser.");
                    M_MyTaskDialog("Info", $"The 'Browser Category' Parameter was not found. \nThe View '{view.Name}' will be placed in the default 'Floor Plans' Category on the project browser.", "Information");
                }
            }
        }

        public static List<Element> GetAllScopeBoxesInView(View view)
        {
            List<Element> scopeBoxes = new List<Element>();
            Document doc = view.Document;

            // Create a filtered element collector for the view
            FilteredElementCollector collector = new FilteredElementCollector(doc, view.Id);

            // Filter for elements that are scope boxes
            ElementCategoryFilter scopeBoxFilter = new ElementCategoryFilter(BuiltInCategory.OST_VolumeOfInterest);

            // Collect all scope boxes in the view
            foreach (Element element in collector.WherePasses(scopeBoxFilter))
            {
                if (IsScopeBox(element))
                {
                    scopeBoxes.Add(element);
                }
            }

            return scopeBoxes;
        }

        public static bool IsScopeBox(Element element)
        {
            // Check if the element is a scope box
            return element != null && element.Category != null && element.Category.Name == "Scope Boxes";
        }

        public static View CreateDependentViewByScopeBox(Document doc, View parentView, Element scopeBox)
        {
            // Check if the view can have dependent views
            if (!parentView.CanViewBeDuplicated(ViewDuplicateOption.AsDependent))
            {
                M_MyTaskDialog("Cannot Proceed", "The specified view cannot have Dependent views.", "Error");
                return null;
                //throw new InvalidOperationException("The specified view cannot have dependent views.");
            }

            var parentViewName = parentView.Name;
            var scopeBoxName = scopeBox.Name;

            // Create the dependent view
            ElementId dependentViewId = parentView.Duplicate(ViewDuplicateOption.AsDependent);
            View dependentView = doc.GetElement(dependentViewId) as View;
            //dependentView.Name = $"{parentViewName} - {scopeBoxName}";
            try
            {
                dependentView.Name = $"{parentViewName} - {scopeBoxName}";
            }
            catch (Exception)
            {
                M_MyTaskDialog("Cannot Proceed", "Dependent views already exist for this view.", "Error");
                return null;
            }

            // assign the scopeBox to the DependentView
            //dependentView.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).Set(scopeBox.Id);
            AssignScopeBoxToView(dependentView, scopeBox);

            return dependentView;
        }

        public static void AssignScopeBoxToView(View view, Element scopeBox)
        {
            // assign the scopeBox to the DependentView
            view.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).Set(scopeBox.Id);
        }

        // Get only parent views, no dependent nor templates
        public static List<View> GetAllParentViews(Document doc)
        {
            List<View> parentViewsList = new FilteredElementCollector(doc)
                                            .OfClass(typeof(View))
                                            .Cast<View>()
                                            .Where(v => !v.IsTemplate && IsParentView(v))
                                            .ToList();

            return parentViewsList;
        }
        private static bool IsParentView(View view)
        {
            return view.GetPrimaryViewId() == ElementId.InvalidElementId;
        }

        public static Parameter GetAssignedScopeBox(View view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view), "View cannot be null.");
            }

            // Get the assigned Scope Box of the View
            Parameter assignedScopeBox = view.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP);
            return assignedScopeBox;
        }


        public static ElementId GetViewSheetIdByName(Document doc, string viewSheetName)
        {
            // Create a filtered element collector for ViewSheets
            FilteredElementCollector collector = new FilteredElementCollector(doc)
                                                 .OfClass(typeof(ViewSheet));

            // Find the ViewSheet by its name
            ViewSheet viewSheet = collector
                                    .Cast<ViewSheet>()
                                    .FirstOrDefault(vs => vs.Name.Equals(viewSheetName, StringComparison.OrdinalIgnoreCase));

            // Return the Id of the ViewSheet, or ElementId.InvalidElementId if not found
            return viewSheet?.Id ?? ElementId.InvalidElementId;
        }


        public static ViewSheet GetViewSheetByName(Document doc, string viewSheetName)
        {
            // Create a filtered element collector for ViewSheets
            FilteredElementCollector collector = new FilteredElementCollector(doc)
                                                 .OfClass(typeof(ViewSheet));

            // Find the ViewSheet by its name
            ViewSheet viewSheet = collector
                                    .Cast<ViewSheet>()
                                    .FirstOrDefault(vs => vs.Name.Equals(viewSheetName, System.StringComparison.OrdinalIgnoreCase));

            return viewSheet;
        }

        public static ViewSheet GetViewSheetById(Document doc, ElementId viewSheetId)
        {
            // Get the element from the document using the provided ElementId
            Element element = doc.GetElement(viewSheetId);

            // Cast the element to ViewSheet and return it
            ViewSheet viewSheet = element as ViewSheet;

            return viewSheet;
        }

        ///// <summary>
        ///// This method returns a true if the current view is in focus.
        ///// and false if the current view is the Project Browser
        ///// </summary>
        ///// <param name="doc"></param>
        ///// <returns></returns>
        //public static bool CheckCurrentViewFocus(Document doc)
        //{
        //    var currentView = doc.ActiveView;
        //    if (currentView.ViewType == ViewType.ProjectBrowser)
        //    {
        //        M_MyTaskDialog("Action Required", "Please double click your view in the Project Browser before proceeding.", "Warning");
        //        return false;
        //    }
        //    return true;
        //}

        /// <summary>
        /// This method returns a true if the current view is in focus.
        /// and false if the current view is the Project Browser
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="dialogIcon"></param>
        /// <returns></returns>
        public static bool CheckCurrentViewFocus(Document doc, string title = "Information", string message = "Warning", string dialogIcon = "Warning")
        {
            var currentView = doc.ActiveView;
            if (currentView.ViewType == ViewType.ProjectBrowser)
            {
                M_MyTaskDialog(title, message, dialogIcon);
                return false;
            }
            return true;
        }
    }

    public class ScheduleData
    {
        public string UniqueId { get; set; }
        public List<string> ParameterNames { get; set; }
        public List<string> ElementUniqueIds { get; set; }
        public Dictionary<string, List<string>> ParameterValues { get; set; }

        public ScheduleData()
        {
            ParameterNames = new List<string>();
            ElementUniqueIds = new List<string>();
            ParameterValues = new Dictionary<string, List<string>>();
        }
    }



    public class ViewsTreeNode : TreeNode
    {
        public ViewsTreeNode(string viewType, List<View> views)
        {
            Header = viewType;
            foreach (var view in views)
            {
                var viewNode = new TreeNode
                {
                    Header = view.Name,
                    ViewId = view.Id
                };
                Children.Add(viewNode);
            }
        }
    }

    //// Usage example:
    //var allParentFloorPlanViewsExceptBIMSetUpView = MyUtils.GetAllParentViews(doc)
    //    .Where(v => v.ViewType == ViewType.FloorPlan && !v.Name.StartsWith("BIM Setup View"))
    //    .ToList();

    //List<ViewsTreeNode> viewsTreeNodes = new ViewsTreeNode(allParentFloorPlanViewsExceptBIMSetUpView);
    public class TreeNode : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isEnabled = true;
        private bool _isExpanded = true;

        public string Header { get; set; }
        public List<TreeNode> Children { get; set; } = new List<TreeNode>();
        public ElementId ViewId { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    foreach (var child in Children)
                    {
                        child.IsSelected = value;
                    }
                }
            }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}