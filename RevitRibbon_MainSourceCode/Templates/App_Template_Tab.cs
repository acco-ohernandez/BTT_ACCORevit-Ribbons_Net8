// Version 1.3.0 2023-06-20
#region Namespaces
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitRibbon_MainSourceCode;
#endregion

namespace RevitRibbon_MainSourceCode
{
    class App_Template_Tab : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)

        {
            //// Create ribbon
            //string assembly_Dll_File = @"C:\ACCORevit\ACCO\ACCORevit ADDINS\02-ACCORevit Ribbons\NAME_OF_FOLDER_FOR_DLLs_and_.Ribbon\RevitRibbon_MainSourceCode.dll";
            //string xml_Ribbon_File_Path = @"C:\ACCORevit\ACCO\ACCORevit ADDINS\02-ACCORevit Ribbons\NAME_OF_FOLDER_FOR_DLLs_and_.Ribbon";

            //or

            // Get the path of the executing assembly (DLL)
            string assembly_Dll_File = Assembly.GetExecutingAssembly().Location;

            // Get the directory path of the assembly
            string xml_Ribbon_File_Path = Path.GetDirectoryName(assembly_Dll_File);

            // call the RibbonBuilder Class and pass it the path of the dll and *.ribbon path.
            RibbonBuilder.build_ribbon(application, assembly_Dll_File, xml_Ribbon_File_Path);

            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            // Do nothing
            return Result.Succeeded;
        }
    }
}