using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.IO;

namespace EEPRevitPlagin.EEPRPCommandModules.ModelCompare
{
    /// <summary>
    /// Interaction logic for AnalyzeSourceModelWPF.xaml
    /// </summary>
    public partial class AnalyzeSourceModelWPF : Window
    {
        public AnalyzeSourceModelWPF()
        {
            InitializeComponent();
            modelPath.Text = AnalyzeSourceModelCommand.doc.PathName.ToString();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void FolderB_Copy_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select a folder to export the models to:";
                DialogResult result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    saveToFolder.Text = fbd.SelectedPath;
                }
            }
        }

        private void exB1_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void cncl_Click(object sender, RoutedEventArgs e)
        {
            stat.Text = "stop";
        }

        private delegate void ProgressBarDelegate();
        private void UpdateProgress()
        {
            ProgBar.Value += 1;
        }
        private void exB_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlNode node = xmlDocument.CreateElement("Elements");
            xmlDocument.AppendChild(node);
            stat.Text = "start";
            exB.IsEnabled = false;
            cncl.IsEnabled = true;
            ProgBar.IsIndeterminate = false;
            ProgBar.Value = 0;
            BuiltInCategory[] cats = { BuiltInCategory.OST_Lines, BuiltInCategory.OST_MEPSpaces, BuiltInCategory.OST_GenericLines, BuiltInCategory.OST_AreaSchemeLines };
            List<BuiltInCategory> unwantedCats = new List<BuiltInCategory>();
            unwantedCats.AddRange(cats);
            List<Element> elements = new List<Element>();
            //Document sourceDoc = AnalyzeSourceModelCommand.uiapp.Application.OpenDocumentFile(modelPath.Text);
            if (allEle.IsChecked == true)
            {
                FilteredElementCollector collector = new FilteredElementCollector(AnalyzeSourceModelCommand.doc).WhereElementIsNotElementType();
                foreach (Element element in collector)
                {
                    if (element.Category != null && !unwantedCats.Contains((BuiltInCategory)element.Category.Id.IntegerValue) && element.Category.CategoryType == CategoryType.Model && element.get_Geometry(new Options()) != null) //&& element.Category.HasMaterialQuantities
                    {
                        elements.Add(element);
                    }
                }
            }
            else if (onlySelEle.IsChecked == true)
            {
                List<ElementId> selectedElements = AnalyzeSourceModelCommand.uidoc.Selection.GetElementIds().ToList();
                foreach (ElementId ele in selectedElements)
                {
                    Element element = AnalyzeSourceModelCommand.doc.GetElement(ele);
                    if (element.Category != null && !unwantedCats.Contains((BuiltInCategory)element.Category.Id.IntegerValue) && element.Category.CategoryType == CategoryType.Model && element.get_Geometry(new Options()) != null) //&& element.Category.HasMaterialQuantities
                    {
                        elements.Add(element);
                    }
                }
            }
            if (elements == null)
            {
                return;
            }
            ProgBar.Maximum = elements.Count;
            ProgBar.Minimum = 0;
            // read the original model
            for (int i = 0; i < ProgBar.Maximum; i++)
            {
                if (stat.Text == "stop")
                {
                    break;
                }
                ProgBar.Dispatcher.Invoke(new ProgressBarDelegate(UpdateProgress), System.Windows.Threading.DispatcherPriority.Background);
                //make the data from original model
                XmlNode xmlNode = xmlDocument.CreateElement("Element");
                XmlAttribute xmlAttr = xmlDocument.CreateAttribute("Id");
                xmlAttr.Value = elements[i].Id.ToString();
                xmlNode.Attributes.Append(xmlAttr);
                foreach (Parameter parameter in elements[i].Parameters)
                {
                    //XmlAttribute xmlAttribute = xmlDocument.CreateAttribute(parameter.Definition.Name.Replace(" ", "_19_").Replace("(", "_19_").Replace(")", "_19_").Replace(@"/", "_19_").Replace(@"\", "_19_").Replace(@",", "_19_"));
                    XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("N_" + parameter.Id);
                    // read the parameters value according to its type
                    if (parameter.StorageType == StorageType.String)
                    {
                        xmlAttribute.Value = parameter.AsString();
                    }
                    else
                    {
                        xmlAttribute.Value = parameter.AsValueString();
                    }
                    xmlNode.Attributes.Append(xmlAttribute);
                }
                Location eleLocation = elements[i].Location;
                if (eleLocation != null)
                {
                    if (eleLocation.GetType().Name == "LocationCurve" && (BuiltInCategory)elements[i].Category.Id.IntegerValue != BuiltInCategory.OST_FlexPipeCurves)
                    {
                        XmlAttribute locationl = xmlDocument.CreateAttribute("LocationCL");
                        XmlAttribute locationX = xmlDocument.CreateAttribute("LocationCX");
                        XmlAttribute locationY = xmlDocument.CreateAttribute("LocationCY");
                        XmlAttribute locationZ = xmlDocument.CreateAttribute("LocationCZ");
                        XmlAttribute locationDirX = xmlDocument.CreateAttribute("LocationCDirX");
                        XmlAttribute locationDirY = xmlDocument.CreateAttribute("LocationCDirY");
                        XmlAttribute locationDirZ = xmlDocument.CreateAttribute("LocationCDirZ");
                        LocationCurve locationPoint = (LocationCurve)eleLocation;
                        locationl.Value = locationPoint.Curve.Length.ToString();
                        Autodesk.Revit.DB.Line line = (Autodesk.Revit.DB.Line)locationPoint.Curve;
                        locationX.Value = line.Origin.X.ToString();
                        locationY.Value = line.Origin.Y.ToString();
                        locationZ.Value = line.Origin.Z.ToString();
                        locationDirX.Value = line.Direction.X.ToString();
                        locationDirY.Value = line.Direction.Y.ToString();
                        locationDirZ.Value = line.Direction.Z.ToString();
                        xmlNode.Attributes.Append(locationl);
                        xmlNode.Attributes.Append(locationX);
                        xmlNode.Attributes.Append(locationY);
                        xmlNode.Attributes.Append(locationZ);
                        xmlNode.Attributes.Append(locationDirX);
                        xmlNode.Attributes.Append(locationDirY);
                        xmlNode.Attributes.Append(locationDirZ);
                    }
                    else if (eleLocation.GetType().Name == "LocationPoint")
                    {
                        XmlAttribute locationX = xmlDocument.CreateAttribute("LocationPX");
                        XmlAttribute locationY = xmlDocument.CreateAttribute("LocationPY");
                        XmlAttribute locationZ = xmlDocument.CreateAttribute("LocationPZ");
                        XmlAttribute locationRot = xmlDocument.CreateAttribute("LocationPRot");
                        LocationPoint locationPoint = (LocationPoint)eleLocation;
                        locationX.Value = locationPoint.Point.X.ToString();
                        locationY.Value = locationPoint.Point.Y.ToString();
                        locationZ.Value = locationPoint.Point.Z.ToString();
                        try
                        {
                            locationRot.Value = locationPoint.Rotation.ToString();
                        }
                        catch
                        {
                            locationRot.Value = "NA";
                        }                        
                        xmlNode.Attributes.Append(locationX);
                        xmlNode.Attributes.Append(locationY);
                        xmlNode.Attributes.Append(locationZ);
                        xmlNode.Attributes.Append(locationRot);
                    }
                    else if (eleLocation.GetType().Name == "Location")
                    {
                        XmlAttribute locationX = xmlDocument.CreateAttribute("LocationX");
                        XmlAttribute locationY = xmlDocument.CreateAttribute("LocationY");
                        XmlAttribute locationZ = xmlDocument.CreateAttribute("LocationZ");
                        GeometryElement solid = elements[i].get_Geometry(new Options());
                        try
                        {
                            Solid solid1 = (Solid)solid.ElementAt(0);
                            locationX.Value = solid1.ComputeCentroid().X.ToString();
                            locationY.Value = solid1.ComputeCentroid().Y.ToString();
                            locationZ.Value = solid1.ComputeCentroid().Z.ToString();
                        }
                        catch
                        {
                            BoundingBoxXYZ boxXYZ = solid.GetBoundingBox();
                            locationX.Value = Convert.ToString((boxXYZ.Max.X + boxXYZ.Min.X) / 2);
                            locationY.Value = Convert.ToString((boxXYZ.Max.Y + boxXYZ.Min.Y) / 2);
                            locationZ.Value = Convert.ToString((boxXYZ.Max.Z + boxXYZ.Min.Z) / 2);
                        }
                        xmlNode.Attributes.Append(locationX);
                        xmlNode.Attributes.Append(locationY);
                        xmlNode.Attributes.Append(locationZ);
                    }
                }
                else
                {
                    XmlAttribute locationX = xmlDocument.CreateAttribute("LocationX");
                    XmlAttribute locationY = xmlDocument.CreateAttribute("LocationY");
                    XmlAttribute locationZ = xmlDocument.CreateAttribute("LocationZ");
                    GeometryElement solid = elements[i].get_Geometry(new Options());
                    try
                    {
                        Solid solid1 = (Solid)solid.ElementAt(0);
                        locationX.Value = solid1.ComputeCentroid().X.ToString();
                        locationY.Value = solid1.ComputeCentroid().Y.ToString();
                        locationZ.Value = solid1.ComputeCentroid().Z.ToString();
                    }
                    catch
                    {
                        BoundingBoxXYZ boxXYZ = solid.GetBoundingBox();// elements[i].get_BoundingBox(AnalyzeSourceModelCommand.doc.ActiveView); solid.GetBoundingBox();
                        locationX.Value = Convert.ToString((boxXYZ.Max.X + boxXYZ.Min.X) / 2);
                        locationY.Value = Convert.ToString((boxXYZ.Max.Y + boxXYZ.Min.Y) / 2);
                        locationZ.Value = Convert.ToString((boxXYZ.Max.Z + boxXYZ.Min.Z) / 2);
                    }
                    xmlNode.Attributes.Append(locationX);
                    xmlNode.Attributes.Append(locationY);
                    xmlNode.Attributes.Append(locationZ);
                }




                node.AppendChild(xmlNode);
                desc.Content = string.Format("Name: {0}, id: {1}, {2}%", elements[i].Name, elements[i].Id, Math.Round(ProgBar.Value * 100 / ProgBar.Maximum, 2));
            }
            string exportFileName = saveToFolder.Text + @"\" + System.IO.Path.GetFileNameWithoutExtension(AnalyzeSourceModelCommand.doc.PathName) + ".xml";
            xmlDocument.Save(exportFileName);
            exB.IsEnabled = true;
        }
    }
}
