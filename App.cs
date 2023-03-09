using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace ENG.ParameterScanner.Revit.Addin
{
    public class App : IExternalApplication
    {
        public static string AddInPath = typeof(App).Assembly.Location;
        public static string AssetsFolder = Path.GetDirectoryName(AddInPath) + "\\Assets\\";

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                CreateRibbon(application);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Parameters", ex.ToString());
                return Result.Failed;
            }
        }

        private void CreateRibbon(UIControlledApplication application)
        {

            string ribbonName = "Parameters";
            string tabName = "Parameters";

            application.CreateRibbonTab(ribbonName);

            RibbonPanel ribbonPanel = application.CreateRibbonPanel(ribbonName, tabName);

            #region Button
            PushButtonData pushButton = new PushButtonData("ParameterScanner", "Parameter\nScanner", AddInPath, "ENG.ParameterScanner.Revit.Addin.ParameterScanner")
            {
                Image = new BitmapImage(new Uri(Path.Combine(AssetsFolder, "Icon_12x12.png"), UriKind.Absolute)),
                LargeImage = new BitmapImage(new Uri(Path.Combine(AssetsFolder, "Icon_24x24.png"), UriKind.Absolute)),
                ToolTip = "This add-in check if all required parameters were filled",
                
            };
            ribbonPanel.AddItem(pushButton);
            #endregion
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}