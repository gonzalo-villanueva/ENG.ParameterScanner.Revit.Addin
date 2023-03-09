using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ENG.ParameterScanner.Revit.Addin.Views
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MainForm : Window
    {
        public ExternalCommandData _commandData;
        public string ProjectName = "Parameter Scanner";
        private System.Windows.Point lastMouseDownPoint;
        private System.Windows.Point lastWindowPosition;


        public MainForm(ExternalCommandData commanddata)
        {
            this.InitializeComponent();
            this._commandData = commanddata;
            this.DataContext = this;
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("CONFIG PAGE");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LogoButton_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("ADDIN DESCRIPTION");
        }

        private void handleResults(Label addinAlerts, ElementResult results, string ParameterName, string v)
        {
            string newTag = "ERROR";
            string newContent = "You can only use this add-in on the following view categories: Floor Plans, Reflected Ceiling Plans, and 3D Views.";
            if (results.ElementCount >= 0)
            {
                newTag = "INFO";
                newContent = results.Elements.Count().ToString();
                if (ParameterName == "")
                {
                    newTag = "ERROR";
                    newContent = "Please complete the Parameter Name Field.";
                }
                else if (results.ParameterExist == 0)
                {
                    newTag = "ERROR";
                    newContent = "No elements with parameter: " + ParameterName + ". Please check the parameter name and try again.";
                }
                else
                {
                    if (v == "Selection")
                    {
                        newContent = results.ParameterMatch.ToString() + " element/s selected.";
                    }
                    else
                    {
                        newContent = results.ParameterMatch.ToString() + " element/s isolated.";
                    }
                    if (results.ParameterMatch == 0) { newTag = "WARNING"; }
                }
            }
            addinAlerts.Tag = newTag;
            addinAlerts.Content = newContent;
        }

        private void SelectItemsButton_Click(object sender, RoutedEventArgs e)
        {
            // Get Active Document
            Document doc = _commandData.Application.ActiveUIDocument.Document;
            View activeView = doc.ActiveView;

            // Get all the elements that match the filter
            var results = GetElementsWithParameterValue(doc, activeView, this.ParameterName.Text, this.ParameterValue.Text);

            handleResults(this.AddinAlerts, results, this.ParameterName.Text, "Selection");

            if (results.ElementCount >= 0)
            {
                // Get ElementIds
                ICollection<ElementId> elementIds = results.Elements.Select(ee => ee.Id).ToList();

                // Get Selection
                Selection selection = _commandData.Application.ActiveUIDocument.Selection;

                // Applied new Selection
                selection.SetElementIds(elementIds);
            }
        }
        
        private void IsolateInViewButton_Click(object sender, RoutedEventArgs e)
        {
            // Get Active Document
            Document doc = _commandData.Application.ActiveUIDocument.Document;
            View activeView = doc.ActiveView;

            // Get all the elements that match the filter
            var results = GetElementsWithParameterValue(doc, activeView, this.ParameterName.Text, this.ParameterValue.Text);
            handleResults(this.AddinAlerts, results, this.ParameterName.Text, "Isolate");

           

            if (results.ElementCount >= 0)
            {
                // Get ElementIds
                ICollection<ElementId> elementIds = results.Elements.Select(ee => ee.Id).ToList();

                // Applied the Isolation
                using (Transaction transaction = new Transaction(doc, "PS - Isolate Elements"))
                {
                    transaction.Start();
                    try
                    {
                        activeView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                        if (results.ParameterMatch > 0)
                        {
                            activeView.IsolateElementsTemporary(elementIds);
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.RollBack();
                        TaskDialog.Show("Error", "No se han podido aislar los elementos en la vista activa.");
                    }
                }
            }
        }

        public class ElementResult
        {
            public List<Element> Elements { get; set; }
            public int ElementCount { get; set; }
            public int ParameterExist { get; set; }
            public int ParameterMatch { get; set; }
            public int ParameterValueEmpty { get; set; }
        }

        public static ElementResult GetElementsWithParameterValue(Document doc, View activeView, string parameterName, string parameterValue)
        {
            if (activeView.ViewType == ViewType.FloorPlan || activeView.ViewType == ViewType.CeilingPlan || activeView.ViewType == ViewType.ThreeD)
            {
                if (parameterValue == "") { parameterValue = null; }

                FilteredElementCollector collector = new FilteredElementCollector(doc, activeView.Id);

                List<Element> elements = collector.WhereElementIsNotElementType().ToList();

                List<Element> elementsWithParameterValue = new List<Element>();

                int ElementCount = 0;
                int ParameterExist = 0;
                int ParameterMatch = 0;
                int ParameterValueEmpty = 0;

                foreach (Element element in elements)
                {
                    ElementCount++;
                    Parameter parameter = element.LookupParameter(parameterName);
                    if (parameter != null)
                    {
                        ParameterExist++;
                        if (parameter.AsString() == parameterValue)
                        {
                            ParameterMatch++;
                            elementsWithParameterValue.Add(element);
                        }
                        if (!parameter.HasValue)
                        {
                            ParameterValueEmpty++;
                        }
                    }
                }

                return new ElementResult
                {
                    Elements = elementsWithParameterValue,
                    ElementCount = ElementCount,
                    ParameterExist = ParameterExist,
                    ParameterMatch = ParameterMatch,
                    ParameterValueEmpty = ParameterValueEmpty
                };
            }
            else
            {
                return new ElementResult
                {
                    Elements = null,
                    ElementCount = -1,
                    ParameterExist = 0,
                    ParameterMatch = 0,
                    ParameterValueEmpty = 0
                };
            }
        }

        private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point currentMousePosition = e.GetPosition(this);
                double deltaX = currentMousePosition.X - lastMouseDownPoint.X;
                double deltaY = currentMousePosition.Y - lastMouseDownPoint.Y;
                this.Left = lastWindowPosition.X + deltaX;
                this.Top = lastWindowPosition.Y + deltaY;
            }
        }

        private void Window_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.ReleaseMouseCapture();
            }
        }
    }
}
