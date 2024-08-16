namespace Engineering_BIM_Team_Tab
{
    [Transaction(TransactionMode.Manual)]
    public class Command1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            if (TaskDialog.Show("Info", "Copy ViewTemplates", TaskDialogCommonButtons.Ok) == TaskDialogResult.Ok)
            {
                new Cmd_CopyViewTemplates().Execute(commandData, ref message, elements);
                return Result.Succeeded;
            }

            if (TaskDialog.Show("Select", "1 - Run Create Bim Setup View", TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel) == TaskDialogResult.Ok)
            {
                new Cmd_CreateBimSetupView().Execute(commandData, ref message, elements);
            }

            if (TaskDialog.Show("Select", "2 - Run Create ScopeBoxGrid", TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel) == TaskDialogResult.Ok)
            {
                new Cmd_ScopeBoxGrid().Execute(commandData, ref message, elements);
            }

            if (TaskDialog.Show("Select", "3 - Run Rename Scope Boxes", TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel) == TaskDialogResult.Ok)
            {
                new Cmd_RenameScopeBoxes().Execute(commandData, ref message, elements);
            }

            if (TaskDialog.Show("Select", "4 - Run Create Dependent Scope View", TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel) == TaskDialogResult.Ok)
            {
                new Cmd_CreateDependentScopeView().Execute(commandData, ref message, elements);
            }

            if (TaskDialog.Show("Select", "5 - Run Create Grid Dimensions", TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel) == TaskDialogResult.Ok)
            {
                new Cmd_GridDimensions().Execute(commandData, ref message, elements);
            }

            if (TaskDialog.Show("Select", "6 - Run Create Matchlines guides", TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel) == TaskDialogResult.Ok)
            {
                new Cmd_CreateMatchlineReference().Execute(commandData, ref message, elements);
            }

            if (TaskDialog.Show("Select", "7 - Run Create View References", TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel) == TaskDialogResult.Ok)
            {
                new Cmd_CreateViewReferencesDuplicates().Execute(commandData, ref message, elements);
            }

            if (TaskDialog.Show("Select", "8 - Run Create Parent Views", TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel) == TaskDialogResult.Ok)
            {
                new Cmd_CreateParentPlotViews().Execute(commandData, ref message, elements);
            }

            if (TaskDialog.Show("Select", "9 - Run Update Applied Dependent Views", TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel) == TaskDialogResult.Ok)
            {
                new Cmd_UpdateAppliedDependentViews().Execute(commandData, ref message, elements);
            }

            if (TaskDialog.Show("Select", "10 - Copy Dims To Parent Views", TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel) == TaskDialogResult.Ok)
            {
                new Cmd_CopyDimsToParentViews().Execute(commandData, ref message, elements);
            }

            if (TaskDialog.Show("Select", "11 - Run Clean Dependent View Dims", TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel) == TaskDialogResult.Ok)
            {
                new Cmd_CleanDependentViewDims().Execute(commandData, ref message, elements);
            }

            return Result.Succeeded;
        }

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";
            string? methodBase = Utils.GetDeclaringTypeName1();

            if (methodBase == null)
            {
                throw new InvalidOperationException("MethodBase.GetCurrentMethod().DeclaringType?.FullName is null");
            }
            else
            {
                RevitRibbon_MainSourceCode.ButtonDataClass myButtonData1 = new RevitRibbon_MainSourceCode.ButtonDataClass(
                    buttonInternalName,
                    buttonTitle,
                    methodBase,
                    RevitRibbon_MainSourceCode_Resources.Properties.Resources.Blue_32,
                    RevitRibbon_MainSourceCode_Resources.Properties.Resources.Blue_16,
                    "This is a tooltip for Button 1");

                return myButtonData1.Data;
            }
        }
    }

}
