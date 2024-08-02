namespace Engineering_BIM_Team_Tab.Common
{
    internal static class Utils
    {
        internal static RibbonPanel? CreateRibbonPanel(UIControlledApplication app, string tabName, string panelName)
        {
            RibbonPanel? curPanel;

            if (GetRibbonPanelByName(app, tabName, panelName) == null)
                curPanel = app.CreateRibbonPanel(tabName, panelName);

            else
                curPanel = GetRibbonPanelByName(app, tabName, panelName);

            return curPanel;
        }

        internal static RibbonPanel? GetRibbonPanelByName(UIControlledApplication app, string tabName, string panelName)
        {
            foreach (RibbonPanel tmpPanel in app.GetRibbonPanels(tabName))
            {
                if (tmpPanel.Name == panelName)
                    return tmpPanel;
            }

            return null;
        }

        internal static string GetDeclaringTypeName()
        {
            return MethodBase.GetCurrentMethod()?.DeclaringType?.FullName ?? "Unknown";
        }

        internal static string GetDeclaringTypeName1()
        {
            // Get the stack trace and find the calling method
            StackTrace stackTrace = new StackTrace();
            // Index 1 is the caller of the current method
            MethodBase? methodBase = stackTrace.GetFrame(1)?.GetMethod();

            // Return the full name of the declaring type, or "Unknown" if null
            return methodBase?.DeclaringType?.FullName ?? "Unknown";
        }
    }
}
