using System.Collections.ObjectModel;
using System.ComponentModel;

using Microsoft.VisualBasic;


namespace RevitRibbon_MainSourceCode
{
    // Transaction required when using IExternalCommand for Revit
    [Transaction(TransactionMode.Manual)] // Starts a new transaction manually for every process that modifies the Revit database
    [Regeneration(RegenerationOption.Manual)]
    public class Cmd_CopyViewTemplates : IExternalCommand // Implementing IExternalCommand gets Revit to recognize our plugins
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            // Referenced similar code: https://boostyourbim.wordpress.com/2013/07/26/transferring-just-one-view-template-from-project-to-project/

            try
            {
                // Get all the open documents except the current active one.
                List<Document> openDocs = GetOtherOpenDocuments(doc, uiapp);

                if (openDocs.Count == 0)
                {
                    MyUtils.M_MyTaskDialog("Info", "No other opened documents.");
                    return Result.Cancelled;
                }

                // Get all the ViewTemplate IDs
                var allViewTemplates = GetAllViewTemplates(doc);
                if (allViewTemplates == null)
                    return Result.Cancelled;


                // Gui for list of templates to get the selected templates list
                var ViewTemplatesList = new ObservableCollection<RevitRibbon_MainSourceCode_Resources.ViewTemplateData>(
                    allViewTemplates.Select(vt => new RevitRibbon_MainSourceCode_Resources.ViewTemplateData(vt.Name, false)));

                // Open ViewTemplateListForm
                ListForm1 ViewTemplateListForm = new ListForm1(ViewTemplatesList)
                {
                    Width = 600,
                    Height = 650,
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                    Topmost = true,
                };
                ViewTemplateListForm.lbl_Info.Content = "Select View Templates from the current Model.";
                // Show the View Templates for the user to select them
                ViewTemplateListForm.ShowDialog();

                // Check if the user confirmed the selection if not return Result.Cancelled
                if (ViewTemplateListForm.DialogResult != true)
                    return Result.Cancelled;


                // Get the selected templates where IsSelected is true
                var selectedViewTemplates = ViewTemplateListForm.selectedViewTemplates.Where(vtData => vtData.IsSelected);

                // Collect the IDs of the selected templates
                var viewTemplateIDsList = new List<ElementId>();
                viewTemplateIDsList = selectedViewTemplates
                    .Select(vtData => allViewTemplates.FirstOrDefault(vt => vt.Name == vtData.TemplateName))
                    .Where(viewTemplate => viewTemplate != null)
                    .Select(viewTemplate => viewTemplate.Id)
                    .ToList();

                if (viewTemplateIDsList.Count == 0)
                {
                    MyUtils.M_MyTaskDialog("Info", "No View Templates Selected \nNothing copied");
                    return Result.Cancelled;
                }

                // Get the selected documents list (User GUI/Form)
                var selectedDocsList = GetListOfSelectedDocs(openDocs);
                if (selectedDocsList == null)
                    return Result.Cancelled;
                else if (selectedDocsList.Count == 0)
                {
                    MyUtils.M_MyTaskDialog("INFO", "You did not select any Model(s).\nNo Models where updated.");
                    return Result.Cancelled;
                }


                // Create a default CopyPasteOptions
                CopyPasteOptions cpOpts = new CopyPasteOptions();
                cpOpts.SetDuplicateTypeNamesHandler(new MyCopyHandler()); // Set the option to Skip duplicates and use the existing type

                var titlesOfDocList = new List<string>();

                //foreach (Document otherDoc in openDocs)
                foreach (Document otherDoc in selectedDocsList)
                {
                    // Get the name of the current document from openDocs. This can be use to report the updated documents
                    titlesOfDocList.Add(otherDoc.Title);

                    // Create a new transaction in each of the other documents and copy the template

                    using (Transaction t = new Transaction(otherDoc, "Copy View Template(s)"))
                    {
                        t.Start();

                        // Perform the copy into the other document using ElementTransformUtils
                        ElementTransformUtils.CopyElements(doc, viewTemplateIDsList, otherDoc, Autodesk.Revit.DB.Transform.Identity, cpOpts);

                        t.Commit();
                    }
                }

                string result = string.Format("View Templates copied = {0}\nRevit Model(s) updated = {1}",
                                   viewTemplateIDsList.Count(),
                                   titlesOfDocList.Count());
                // Tell the user the results
                MyUtils.M_MyTaskDialog("Results", result);


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

        private static List<Document> GetOtherOpenDocuments(Document doc, UIApplication uiapp)
        {
            var allOtherOpenDocs = uiapp.Application.Documents.Cast<Document>()
                .Where(d => d.ActiveView == null)
                .ToList();
            return allOtherOpenDocs;
#if REVIT2020
#elif REVIT2021
#elif REVIT2022
#endif

        }

        private List<Document> GetListOfSelectedDocs(List<Document> openDocs)
        {
            List<Document> selectedDocs = new List<Document>();
            // Gui for list of templates to get the selected templates list
            var ViewTemplatesList = new ObservableCollection<RevitRibbon_MainSourceCode_Resources.ViewTemplateData>(
                openDocs.Select(vt => new RevitRibbon_MainSourceCode_Resources.ViewTemplateData(vt.Title, false)));

            // Open the DocsListForm
            var DocsListForm = new ListForm1(ViewTemplatesList)
            {
                Width = 600,
                Height = 650,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                Topmost = true,
            };
            DocsListForm.lbl_Info.Content = "Select the Revit Model(s) to be updated.";
            DocsListForm.ShowDialog();

            if (DocsListForm.DialogResult != true)
                return null;

            // Get the selected Documents where IsSelected is true
            var formSelectedDocs = DocsListForm.selectedViewTemplates.Where(formDoc => formDoc.IsSelected);

            // Collect the Selected Documents from the openDocs list
            selectedDocs = formSelectedDocs
                .Select(vtData => openDocs.FirstOrDefault(vt => vt.Title == vtData.TemplateName))
                .Where(viewTemplate => viewTemplate != null)
                .Select(viewTemplate => viewTemplate)
                .ToList();


            return selectedDocs;
        }

        private List<View> GetAllViewTemplates(Document doc)
        {
            // Get all View Templates in the current document
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(View));

            // Filter for View Templates (they are of type View)
            var viewTemplates = collector.OfType<View>().Where(view => view.IsTemplate).OrderBy(v => v.Name).ToList();

            // Check if there are any View Templates
            if (viewTemplates.Count == 0)
            {
                MyUtils.M_MyTaskDialog("Error", "There are no View Templates in the current document.");
                return null;
            }

            // Now viewTemplates is a collection of View Templates in the current document
            return viewTemplates;
        }
    }
    // Other Classes used by this command
    public class MyCopyHandler : IDuplicateTypeNamesHandler
    {
        public DuplicateTypeAction OnDuplicateTypeNamesFound(DuplicateTypeNamesHandlerArgs args)
        {
            // You can decide how to handle duplicate types here.
            // For example, to skip duplicates and use the existing type, you can do:
            return DuplicateTypeAction.UseDestinationTypes;
        }
    }
    //public class ViewTemplateData : INotifyPropertyChanged
    //{
    //    private bool isSelected;
    //    public bool IsSelected
    //    {
    //        get { return isSelected; }
    //        set
    //        {
    //            if (isSelected != value)
    //            {
    //                isSelected = value;
    //                OnPropertyChanged(nameof(IsSelected));
    //            }
    //        }
    //    }

    //    private string templateName;
    //    public string TemplateName
    //    {
    //        get { return templateName; }
    //        set
    //        {
    //            if (templateName != value)
    //            {
    //                templateName = value;
    //                OnPropertyChanged(nameof(TemplateName));
    //            }
    //        }
    //    }

    //    public event PropertyChangedEventHandler PropertyChanged;

    //    protected virtual void OnPropertyChanged(string propertyName)
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //    }

    //    public ViewTemplateData(string templateName, bool isSelected)
    //    {
    //        TemplateName = templateName;
    //        IsSelected = isSelected;
    //    }
    //}
}

