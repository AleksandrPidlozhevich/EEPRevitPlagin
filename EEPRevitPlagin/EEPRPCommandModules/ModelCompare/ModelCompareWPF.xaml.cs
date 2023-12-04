using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Data;

namespace EEPRevitPlagin.EEPRPCommandModules.ModelCompare
{
    /// <summary>
    /// Interaction logic for ModelCompareWPF.xaml
    /// </summary>
    public partial class ModelCompareWPF : Window
    {
        XmlDocument xmlDoc = new XmlDocument();
        DataSet dataSet = new DataSet();
        ExternalEvent externalEvent;
        private delegate void ProgressBarDelegate();
        private void UpdateProgress()
        {
            ProgBar.Value += 1;
        }
        public ModelCompareWPF(ExternalEvent e)
        {
            InitializeComponent();
            externalEvent = e;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        private void FolderB_Click(object sender, RoutedEventArgs e)
        {

        }

        private void openFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Filter = "Analysis file (*.xml)|*.xml";
                    dlg.Title = "Choose the Analysis xml file to compare";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        sourceFileLink.Text = dlg.FileName;
                    }
                }
                xmlDoc.Load(sourceFileLink.Text);
                XmlNode root = xmlDoc.ChildNodes[0];
                foreach (XmlNode node in root.ChildNodes)
                {
                    eleList.Items.Add(node.Attributes["Id"].Value);
                }
            }
            catch
            {

            }            
        }
        private void findEle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (string a in eleList.Items)
                {
                    if (a.ToString() == searchValue.Text)
                    {
                        eleList.SelectedItem = a;
                        break;
                    }
                }
            }
            catch
            {

            }
        }
        private void selEle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ModelCompareCommand.uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
                searchValue.Text = ModelCompareCommand.uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element).ElementId.ToString();
                foreach (string a in eleList.Items)
                {
                    if (a.ToString() == searchValue.Text)
                    {
                        eleList.SelectedItem = a;
                        break;
                    }
                }
            }
            catch
            {

            }
        }

        private void ex_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void rowAddToDataTable(DataTable dataTable, XmlAttribute attribute, Element element, bool showSameInTabel)
        {
            string diff = "The same";
            string val;
            bool showRow = true;
            Location eleLocation;
            LocationCurve locationPoint;
            Autodesk.Revit.DB.Line line;
            LocationPoint locationPoint1;
            GeometryElement solid;
            Solid solid1;
            BoundingBoxXYZ boxXYZ;
            switch (attribute.Name)
            {
                case "Id":
                    break;
                case "LocationCL":
                    eleLocation = element.Location;
                    locationPoint = (LocationCurve)eleLocation;
                    val = locationPoint.Curve.Length.ToString();
                    if (attribute.Value.ToString().Trim() != val.Trim())
                    {
                        diff = "Different";
                    }
                    if (showSameInTabel == false && diff == "The same")
                    {
                        showRow = false;
                    }
                    if (showRow)
                    {
                        dataTable.Rows.Add(attribute.Name.Replace("_19_", " "), attribute.Value, val, diff);
                    }

                    break;
                case "LocationCX":
                    eleLocation = element.Location;
                    locationPoint = (LocationCurve)eleLocation;
                    line = (Autodesk.Revit.DB.Line)locationPoint.Curve;
                    val = line.Origin.X.ToString();
                    if (attribute.Value.ToString().Trim() != val.Trim())
                    {
                        diff = "Different";
                    }
                    if (showSameInTabel == false && diff == "The same")
                    {
                        showRow = false;
                    }
                    if (showRow)
                    {
                        dataTable.Rows.Add(attribute.Name.Replace("_19_", " "), attribute.Value, val, diff);
                    }
                    break;
                case "LocationCY":
                    eleLocation = element.Location;
                    locationPoint = (LocationCurve)eleLocation;
                    line = (Autodesk.Revit.DB.Line)locationPoint.Curve;
                    val = line.Origin.Y.ToString();
                    if (attribute.Value.ToString().Trim() != val.Trim())
                    {
                        diff = "Different";
                    }
                    if (showSameInTabel == false && diff == "The same")
                    {
                        showRow = false;
                    }
                    if (showRow)
                    {
                        dataTable.Rows.Add(attribute.Name.Replace("_19_", " "), attribute.Value, val, diff);
                    }
                    break;
                case "LocationCZ":
                    eleLocation = element.Location;
                    locationPoint = (LocationCurve)eleLocation;
                    line = (Autodesk.Revit.DB.Line)locationPoint.Curve;
                    val = line.Origin.Z.ToString();
                    if (attribute.Value.ToString().Trim() != val.Trim())
                    {
                        diff = "Different";
                    }
                    if (showSameInTabel == false && diff == "The same")
                    {
                        showRow = false;
                    }
                    if (showRow)
                    {
                        dataTable.Rows.Add(attribute.Name.Replace("_19_", " "), attribute.Value, val, diff);
                    }
                    break;
                case "LocationCDirX":
                    eleLocation = element.Location;
                    locationPoint = (LocationCurve)eleLocation;
                    line = (Autodesk.Revit.DB.Line)locationPoint.Curve;
                    val = line.Direction.X.ToString();
                    if (attribute.Value.ToString().Trim() != val.Trim())
                    {
                        diff = "Different";
                    }
                    if (showSameInTabel == false && diff == "The same")
                    {
                        showRow = false;
                    }
                    if (showRow)
                    {
                        dataTable.Rows.Add(attribute.Name.Replace("_19_", " "), attribute.Value, val, diff);
                    }
                    break;
                case "LocationCDirY":
                    eleLocation = element.Location;
                    locationPoint = (LocationCurve)eleLocation;
                    line = (Autodesk.Revit.DB.Line)locationPoint.Curve;
                    val = line.Direction.Y.ToString();
                    if (attribute.Value.ToString().Trim() != val.Trim())
                    {
                        diff = "Different";
                    }
                    if (showSameInTabel == false && diff == "The same")
                    {
                        showRow = false;
                    }
                    if (showRow)
                    {
                        dataTable.Rows.Add(attribute.Name.Replace("_19_", " "), attribute.Value, val, diff);
                    }
                    break;
                case "LocationCDirZ":
                    eleLocation = element.Location;
                    locationPoint = (LocationCurve)eleLocation;
                    line = (Autodesk.Revit.DB.Line)locationPoint.Curve;
                    val = line.Direction.Z.ToString();
                    if (attribute.Value.ToString().Trim() != val.Trim())
                    {
                        diff = "Different";
                    }
                    if (showSameInTabel == false && diff == "The same")
                    {
                        showRow = false;
                    }
                    if (showRow)
                    {
                        dataTable.Rows.Add(attribute.Name.Replace("_19_", " "), attribute.Value, val, diff);
                    }
                    break;
                case "LocationPX":
                    eleLocation = element.Location;
                    locationPoint1 = (LocationPoint)eleLocation;
                    val = locationPoint1.Point.X.ToString();
                    if (attribute.Value.ToString().Trim() != val.Trim())
                    {
                        diff = "Different";
                    }
                    if (showSameInTabel == false && diff == "The same")
                    {
                        showRow = false;
                    }
                    if (showRow)
                    {
                        dataTable.Rows.Add(attribute.Name.Replace("_19_", " "), attribute.Value, val, diff);
                    }
                    break;
                case "LocationPY":
                    eleLocation = element.Location;
                    locationPoint1 = (LocationPoint)eleLocation;
                    val = locationPoint1.Point.Y.ToString();
                    if (attribute.Value.ToString().Trim() != val.Trim())
                    {
                        diff = "Different";
                    }
                    if (showSameInTabel == false && diff == "The same")
                    {
                        showRow = false;
                    }
                    if (showRow)
                    {
                        dataTable.Rows.Add(attribute.Name.Replace("_19_", " "), attribute.Value, val, diff);
                    }
                    break;
                case "LocationPZ":
                    eleLocation = element.Location;
                    locationPoint1 = (LocationPoint)eleLocation;
                    val = locationPoint1.Point.Z.ToString();
                    if (attribute.Value.ToString().Trim() != val.Trim())
                    {
                        diff = "Different";
                    }
                    if (showSameInTabel == false && diff == "The same")
                    {
                        showRow = false;
                    }
                    if (showRow)
                    {
                        dataTable.Rows.Add(attribute.Name.Replace("_19_", " "), attribute.Value, val, diff);
                    }
                    break;
                case "LocationPRot":
                    eleLocation = element.Location;
                    locationPoint1 = (LocationPoint)eleLocation;
                    try
                    {
                        val = locationPoint1.Rotation.ToString();
                    }
                    catch
                    {
                        val = "NA";
                    }                    
                    if (attribute.Value.ToString().Trim() != val.Trim())
                    {
                        diff = "Different";
                    }
                    if (showSameInTabel == false && diff == "The same")
                    {
                        showRow = false;
                    }
                    if (showRow)
                    {
                        dataTable.Rows.Add(attribute.Name.Replace("_19_", " "), attribute.Value, val, diff);
                    }
                    break;
                case "LocationX":
                    solid = element.get_Geometry(new Options());
                    try
                    {
                        solid1 = (Solid)solid.ElementAt(0);
                        val = solid1.ComputeCentroid().X.ToString();
                    }
                    catch
                    {
                        boxXYZ = solid.GetBoundingBox();
                        val = Convert.ToString((boxXYZ.Max.X + boxXYZ.Min.X) / 2);
                    }
                    if (attribute.Value.ToString().Trim() != val.Trim())
                    {
                        diff = "Different";
                    }
                    if (showSameInTabel == false && diff == "The same")
                    {
                        showRow = false;
                    }
                    if (showRow)
                    {
                        dataTable.Rows.Add(attribute.Name.Replace("_19_", " "), attribute.Value, val, diff);
                    }
                    break;
                case "LocationY":
                    solid = element.get_Geometry(new Options());
                    try
                    {
                        solid1 = (Solid)solid.ElementAt(0);
                        val = solid1.ComputeCentroid().Y.ToString();
                    }
                    catch
                    {
                        boxXYZ = solid.GetBoundingBox();
                        val = Convert.ToString((boxXYZ.Max.Y + boxXYZ.Min.Y) / 2);
                    }
                    if (attribute.Value.ToString().Trim() != val.Trim())
                    {
                        diff = "Different";
                    }
                    if (showSameInTabel == false && diff == "The same")
                    {
                        showRow = false;
                    }
                    if (showRow)
                    {
                        dataTable.Rows.Add(attribute.Name.Replace("_19_", " "), attribute.Value, val, diff);
                    }

                    break;
                case "LocationZ":
                    solid = element.get_Geometry(new Options());
                    try
                    {
                        solid1 = (Solid)solid.ElementAt(0);
                        val = solid1.ComputeCentroid().Z.ToString();
                    }
                    catch
                    {
                        boxXYZ = solid.GetBoundingBox();
                        val = Convert.ToString((boxXYZ.Max.Z + boxXYZ.Min.Z) / 2);
                    }
                    if (attribute.Value.ToString().Trim() != val.Trim())
                    {
                        diff = "Different";
                    }
                    if (showSameInTabel == false && diff == "The same")
                    {
                        showRow = false;
                    }
                    if (showRow)
                    {
                        dataTable.Rows.Add(attribute.Name.Replace("_19_", " "), attribute.Value, val, diff);
                    }
                    break;
                default:
                    ElementId id2 = new ElementId(Convert.ToInt32(attribute.Name.Replace("N_", "")));
                    string paraName = "";
                    Parameter parameter = null;
                    foreach (Parameter para in element.Parameters)
                    {
                        if (para.Id == id2)
                        {
                            paraName = para.Definition.Name;
                            parameter = para;
                            break;
                        }
                    }
                    if (parameter != null)
                    {

                        if (parameter.StorageType == StorageType.String)
                        {
                            if (parameter.AsString() != null)
                            {
                                val = parameter.AsString();
                            }
                            else
                            {
                                val = "";
                            }
                        }
                        else
                        {
                            if (parameter.AsValueString() != null)
                            {
                                val = parameter.AsValueString();
                            }
                            else
                            {
                                val = "";
                            }
                        }
                        if (attribute.Value.ToString().Trim() != val.Trim())
                        {
                            diff = "Different";
                        }
                        if (showSameInTabel == false && diff == "The same")
                        {
                            showRow = false;
                        }
                        if (showRow)
                        {
                            dataTable.Rows.Add(paraName, attribute.Value, val, diff);
                        }
                    }
                    break;
            }
        }
        private void eleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<ElementId> elementIds = new List<ElementId>();
            ElementId elementId = new ElementId(Convert.ToInt32(e.AddedItems[0].ToString()));
            Element ele = ModelCompareCommand.doc.GetElement(elementId);
            if (ele == null)
            {
                TaskDialog.Show("Info", "The element is deleted in this model");
            }
            else
            {
                elementIds.Add(elementId);
                ModelCompareCommand.uidoc.Selection.SetElementIds(elementIds);

                DataColumn properName = new DataColumn("Property Name");
                DataColumn properVS = new DataColumn("Source Value");
                DataColumn properVC = new DataColumn("Current Value");
                DataColumn different = new DataColumn("Different");

                DataTable dataTable = new DataTable();
                dataTable.Columns.Add(properName);
                dataTable.Columns.Add(properVS);
                dataTable.Columns.Add(properVC);
                dataTable.Columns.Add(different);
                XmlNode root = xmlDoc.ChildNodes[0];
                XmlNode wanted = null;
                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Attributes["Id"].Value == e.AddedItems[0].ToString())
                    {
                        wanted = node;
                        break;
                    }
                }
                foreach (XmlAttribute attribute in wanted.Attributes)
                {
                    rowAddToDataTable(dataTable, attribute, ele, !(bool)diff.IsChecked);
                }
                eleProperList.DataContext = dataTable.DefaultView;
            }
        }

        private void analyzeAll_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)useExView.IsChecked)
            {
                ModelCompareCommand.needToCreat = false;
            }
            else
            {
                ModelCompareCommand.needToCreat = true;
            }
            stat.Text = "start";
            ProgBar.Maximum = eleList.Items.Count;
            ProgBar.Minimum = 0;
            externalEvent.Raise();
            // read the original model
            DataTable dataTable = new DataTable();
            DataColumn elementId = new DataColumn("ElementId");
            DataColumn status = new DataColumn("Status");
            dataTable.Columns.Add(elementId);
            dataTable.Columns.Add(status);

            for (int i = 0; i < ProgBar.Maximum; i++)
            {
                if (stat.Text == "stop")
                {
                    break;
                }
                ProgBar.Dispatcher.Invoke(new ProgressBarDelegate(UpdateProgress), System.Windows.Threading.DispatcherPriority.Background);
                List<ElementId> elementIds = new List<ElementId>();
                ElementId elementId2 = new ElementId(Convert.ToInt32(eleList.Items[i].ToString()));
                Element ele = ModelCompareCommand.doc.GetElement(elementId2);
                if (ele == null)
                {
                    dataTable.Rows.Add(eleList.Items[i].ToString(), 1);
                }
                else
                {
                    XmlNode root = xmlDoc.ChildNodes[0];
                    XmlNode wanted = null;
                    foreach (XmlNode node in root.ChildNodes)
                    {
                        if (node.Attributes["Id"].Value == eleList.Items[i].ToString())
                        {
                            wanted = node;
                            break;
                        }
                    }
                    foreach (XmlAttribute attribute in wanted.Attributes)
                    {
                        string diff = "The same";
                        string val;
                        Location eleLocation;
                        LocationCurve locationPoint;
                        Autodesk.Revit.DB.Line line;
                        LocationPoint locationPoint1;
                        GeometryElement solid;
                        Solid solid1;
                        BoundingBoxXYZ boxXYZ;
                        if (onlyLocation.IsChecked == true)
                        {
                            switch (attribute.Name)
                            {
                                case "Id":
                                    break;
                                case "LocationCL":
                                    eleLocation = ele.Location;
                                    locationPoint = (LocationCurve)eleLocation;
                                    val = locationPoint.Curve.Length.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationCX":
                                    eleLocation = ele.Location;
                                    locationPoint = (LocationCurve)eleLocation;
                                    line = (Autodesk.Revit.DB.Line)locationPoint.Curve;
                                    val = line.Origin.X.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationCY":
                                    eleLocation = ele.Location;
                                    locationPoint = (LocationCurve)eleLocation;
                                    line = (Autodesk.Revit.DB.Line)locationPoint.Curve;
                                    val = line.Origin.Y.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationCZ":
                                    eleLocation = ele.Location;
                                    locationPoint = (LocationCurve)eleLocation;
                                    line = (Autodesk.Revit.DB.Line)locationPoint.Curve;
                                    val = line.Origin.Z.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationCDirX":
                                    eleLocation = ele.Location;
                                    locationPoint = (LocationCurve)eleLocation;
                                    line = (Autodesk.Revit.DB.Line)locationPoint.Curve;
                                    val = line.Direction.X.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationCDirY":
                                    eleLocation = ele.Location;
                                    locationPoint = (LocationCurve)eleLocation;
                                    line = (Autodesk.Revit.DB.Line)locationPoint.Curve;
                                    val = line.Direction.Y.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationCDirZ":
                                    eleLocation = ele.Location;
                                    locationPoint = (LocationCurve)eleLocation;
                                    line = (Autodesk.Revit.DB.Line)locationPoint.Curve;
                                    val = line.Direction.Z.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationPX":
                                    eleLocation = ele.Location;
                                    locationPoint1 = (LocationPoint)eleLocation;
                                    val = locationPoint1.Point.X.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationPY":
                                    eleLocation = ele.Location;
                                    locationPoint1 = (LocationPoint)eleLocation;
                                    val = locationPoint1.Point.Y.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationPZ":
                                    eleLocation = ele.Location;
                                    locationPoint1 = (LocationPoint)eleLocation;
                                    val = locationPoint1.Point.Z.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationPRot":
                                    eleLocation = ele.Location;
                                    locationPoint1 = (LocationPoint)eleLocation;
                                    try
                                    {
                                        val = locationPoint1.Rotation.ToString(); 
                                    }
                                    catch
                                    {
                                        val =  "NA";
                                    }                                    
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationX":
                                    solid = ele.get_Geometry(new Options());
                                    try
                                    {
                                        solid1 = (Solid)solid.ElementAt(0);
                                        val = solid1.ComputeCentroid().X.ToString();
                                    }
                                    catch
                                    {
                                        boxXYZ = solid.GetBoundingBox();
                                        val = Convert.ToString((boxXYZ.Max.X + boxXYZ.Min.X) / 2);
                                    }
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationY":
                                    solid = ele.get_Geometry(new Options());
                                    try
                                    {
                                        solid1 = (Solid)solid.ElementAt(0);
                                        val = solid1.ComputeCentroid().Y.ToString();
                                    }
                                    catch
                                    {
                                        boxXYZ = solid.GetBoundingBox();
                                        val = Convert.ToString((boxXYZ.Max.Y + boxXYZ.Min.Y) / 2);
                                    }
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }

                                    break;
                                case "LocationZ":
                                    solid = ele.get_Geometry(new Options());
                                    try
                                    {
                                        solid1 = (Solid)solid.ElementAt(0);
                                        val = solid1.ComputeCentroid().Z.ToString();
                                    }
                                    catch
                                    {
                                        boxXYZ = solid.GetBoundingBox();
                                        val = Convert.ToString((boxXYZ.Max.Z + boxXYZ.Min.Z) / 2);
                                    }
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;                                
                            }
                        }
                        else
                        {
                            switch (attribute.Name)
                            {
                                case "Id":
                                    break;
                                case "LocationCL":
                                    eleLocation = ele.Location;
                                    locationPoint = (LocationCurve)eleLocation;
                                    val = locationPoint.Curve.Length.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationCX":
                                    eleLocation = ele.Location;
                                    locationPoint = (LocationCurve)eleLocation;
                                    line = (Autodesk.Revit.DB.Line)locationPoint.Curve;
                                    val = line.Origin.X.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationCY":
                                    eleLocation = ele.Location;
                                    locationPoint = (LocationCurve)eleLocation;
                                    line = (Autodesk.Revit.DB.Line)locationPoint.Curve;
                                    val = line.Origin.Y.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationCZ":
                                    eleLocation = ele.Location;
                                    locationPoint = (LocationCurve)eleLocation;
                                    line = (Autodesk.Revit.DB.Line)locationPoint.Curve;
                                    val = line.Origin.Z.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationCDirX":
                                    eleLocation = ele.Location;
                                    locationPoint = (LocationCurve)eleLocation;
                                    line = (Autodesk.Revit.DB.Line)locationPoint.Curve;
                                    val = line.Direction.X.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationCDirY":
                                    eleLocation = ele.Location;
                                    locationPoint = (LocationCurve)eleLocation;
                                    line = (Autodesk.Revit.DB.Line)locationPoint.Curve;
                                    val = line.Direction.Y.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationCDirZ":
                                    eleLocation = ele.Location;
                                    locationPoint = (LocationCurve)eleLocation;
                                    line = (Autodesk.Revit.DB.Line)locationPoint.Curve;
                                    val = line.Direction.Z.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationPX":
                                    eleLocation = ele.Location;
                                    locationPoint1 = (LocationPoint)eleLocation;
                                    val = locationPoint1.Point.X.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationPY":
                                    eleLocation = ele.Location;
                                    locationPoint1 = (LocationPoint)eleLocation;
                                    val = locationPoint1.Point.Y.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationPZ":
                                    eleLocation = ele.Location;
                                    locationPoint1 = (LocationPoint)eleLocation;
                                    val = locationPoint1.Point.Z.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationPRot":
                                    eleLocation = ele.Location;
                                    locationPoint1 = (LocationPoint)eleLocation;
                                    val = locationPoint1.Rotation.ToString();
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationX":
                                    solid = ele.get_Geometry(new Options());
                                    try
                                    {
                                        solid1 = (Solid)solid.ElementAt(0);
                                        val = solid1.ComputeCentroid().X.ToString();
                                    }
                                    catch
                                    {
                                        boxXYZ = solid.GetBoundingBox();
                                        val = Convert.ToString((boxXYZ.Max.X + boxXYZ.Min.X) / 2);
                                    }
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                case "LocationY":
                                    solid = ele.get_Geometry(new Options());
                                    try
                                    {
                                        solid1 = (Solid)solid.ElementAt(0);
                                        val = solid1.ComputeCentroid().Y.ToString();
                                    }
                                    catch
                                    {
                                        boxXYZ = solid.GetBoundingBox();
                                        val = Convert.ToString((boxXYZ.Max.Y + boxXYZ.Min.Y) / 2);
                                    }
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }

                                    break;
                                case "LocationZ":
                                    solid = ele.get_Geometry(new Options());
                                    try
                                    {
                                        solid1 = (Solid)solid.ElementAt(0);
                                        val = solid1.ComputeCentroid().Z.ToString();
                                    }
                                    catch
                                    {
                                        boxXYZ = solid.GetBoundingBox();
                                        val = Convert.ToString((boxXYZ.Max.Z + boxXYZ.Min.Z) / 2);
                                    }
                                    if (attribute.Value.ToString().Trim() != val.Trim())
                                    {
                                        diff = "Different";
                                        dataTable.Rows.Add(ele.Id.ToString(), 0);
                                    }
                                    break;
                                default:
                                    ElementId id2 = new ElementId(Convert.ToInt32(attribute.Name.Replace("N_", "")));
                                    Parameter parameter = null;
                                    foreach (Parameter para in ele.Parameters)
                                    {
                                        if (para.Id == id2)
                                        {
                                            parameter = para;
                                            break;
                                        }
                                    }
                                    if (parameter != null)
                                    {
                                        if (parameter.StorageType == StorageType.String)
                                        {
                                            if (parameter.AsString() != null)
                                            {
                                                val = parameter.AsString();
                                            }
                                            else
                                            {
                                                val = "";
                                            }
                                        }
                                        else
                                        {
                                            if (parameter.AsValueString() != null)
                                            {
                                                val = parameter.AsValueString();
                                            }
                                            else
                                            {
                                                val = "";
                                            }
                                        }
                                        if (attribute.Value.ToString().Trim() != val.Trim())
                                        {
                                            diff = "Different";
                                            dataTable.Rows.Add(ele.Id.ToString(), 0);
                                        }
                                    }
                                    break;
                            }
                        }                        
                        if (diff == "Different")
                        {
                            break;
                        }
                    }
                }
                desc.Content = Math.Round(ProgBar.Value * 100 / ProgBar.Maximum, 2) + "%  ";
            }
            ProgBar.Value = 0;
            List<Element> elements = new List<Element>();
            FilteredElementCollector collector = new FilteredElementCollector(ModelCompareCommand.doc).WhereElementIsNotElementType();
            foreach (Element element in collector)
            {
                if ((element.Category != null && element.Category.HasMaterialQuantities) && element.Category.CategoryType == CategoryType.Model)
                {
                    elements.Add(element);
                }
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
                //Thread.Sleep(1000);
                if (!eleList.Items.Contains(elements[i].Id.ToString()))
                {
                    dataTable.Rows.Add(elements[i].Id.ToString(), 2);
                }
                desc.Content = Math.Round(ProgBar.Value * 100 / ProgBar.Maximum, 2) + " % ";
            }
            dataSet.Tables.Add(dataTable);
            dataSet.WriteXml(@exportTo.Text);
            ModelCompareCommand.exportTo = exportTo.Text;
        }

        private void exTo_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select a folder to export the models to:";
                DialogResult result = fbd.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    exportTo.Text = fbd.SelectedPath + @"\results.xml";
                }
            }
        }

        private void useExView_Checked(object sender, RoutedEventArgs e)
        {
            viewsList.Items.Clear();
            FilteredElementCollector viewCollector = new FilteredElementCollector(ModelCompareCommand.doc);
            viewCollector.OfClass(typeof(View3D));
            foreach (View3D view3D in viewCollector)
            {
                if (view3D.IsTemplate == false)
                {
                    viewsList.Items.Add(view3D.Name);
                }
            }
            viewsList.IsEnabled = true;
        }
        private void viewsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ModelCompareCommand.resViewName = (sender as System.Windows.Controls.ComboBox).SelectedItem as string;
        }

        private void StopB_Click(object sender, RoutedEventArgs e)
        {
            stat.Text = "stop";
        }
    }
}
