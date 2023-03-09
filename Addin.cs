using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using ENG.ParameterScanner.Revit.Addin.Views;

namespace ENG.ParameterScanner.Revit.Addin
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class ParameterScanner : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            try
            {
                var simpleForm = new MainForm(commandData);
                simpleForm.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Parameters", ex.ToString());
                return Result.Failed;
            }
        }
    }
}
