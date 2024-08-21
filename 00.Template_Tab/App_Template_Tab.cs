using RevitRibbon_MainSourceCode;

using RevitRibbon_MainSourceCode_Resources;
//You can run MSBuild from the command line with the /pp (preprocessor) switch to see the evaluated values of properties in your project file:
//msbuild YourProject.csproj /pp:out.txt


namespace Template_Tab
{
    internal class App_Template_Tab : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication app)
        {
            // Set the license context to non-commercial to use EPPlus in non-commercial projects
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            //#if RELEASE_2020 || RELEASE_2021 || RELEASE_2022 || RELEASE_2023 || RELEASE_2024 || RELEASE_2025
            // Get the path of the executing assembly (DLL)
            string assembly_Dll_File = Assembly.GetExecutingAssembly().Location;

            // Get the directory path of the assembly
            string xml_Ribbon_File_Path = Path.GetDirectoryName(assembly_Dll_File);

            // call the RibbonBuilder Class and pass it the path of the dll and *.ribbon path.
            RibbonBuilder.build_ribbon(app, assembly_Dll_File, xml_Ribbon_File_Path);
            //#endif

#if REVIT2020
            Debug.Print("Revit2020");
#elif REVIT2021
                    Debug.Print("Revit2021");
#elif REVIT2022
                        Debug.Print("Revit2022");
#elif REVIT2023
                        Debug.Print("Revit2023");
#elif REVIT2024
                        Debug.Print("Revit2024");
#elif REVIT2025
            Debug.Print("Revit2025");
#endif

            ////=======================================================
            //// 1. Create ribbon tab
            //string tabName = "Addin_Testing";
            //string panelName = "Engineering_BIM_Team_Tab"; // This is the DLL name
            //try
            //{
            //    app.CreateRibbonTab(tabName);
            //}
            //catch (Exception)
            //{
            //    Debug.Print("Tab already exists.");
            //}

            //// 2. Create ribbon panel 
            //RibbonPanel? panel = Utils.CreateRibbonPanel(app, tabName, panelName);

            //if (panel is null)
            //{
            //    Debug.Print("Failed to create or retrieve the ribbon panel.");
            //    return Result.Failed;
            //}

            //// 3. Create button data instances
            //// 4. Create buttons
            //PushButtonData? btnData1 = Command1.GetButtonData();
            //PushButton? myButton1 = panel.AddItem(btnData1) as PushButton;

            //if (myButton1 is null)
            //{
            //    Debug.Print("Failed to create the button.");
            //    return Result.Failed;
            //}

            // Uncomment the following lines if needed
            //PushButtonData? btnData2 = Command2.GetButtonData();
            //PushButton? myButton2 = panel.AddItem(btnData2) as PushButton;

            // NOTE:
            // To create a new tool, copy lines 35 and 39 and rename the variables to "btnData3" and "myButton3". 
            // Change the name of the tool in the arguments of line 
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }

}
