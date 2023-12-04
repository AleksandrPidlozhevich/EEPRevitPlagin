using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

namespace EEPRevitPlagin.EEPRPCommandModules.DuctInsulationCheck
{
    /// <summary>
    /// Interaction logic for DuctInsulationCheckWPF.xaml
    /// </summary>
    public partial class DuctInsulationCheckWPF : Window
    {
        DataTable dt = new DataTable();
        public DuctInsulationCheckWPF()
        {
            InitializeComponent();
        }

        private void CheckB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dt.Columns.Add(new DataColumn("ELement Id", typeof(int)));
                dt.Columns.Add(new DataColumn("Type", typeof(string)));
                dt.Columns.Add(new DataColumn("Insulation Added", typeof(bool)));
                dt.Columns.Add(new DataColumn("Thickness", typeof(double)));
                ElementTable.DataContext = dt;
                List<ElementId> eles = new List<ElementId>();
                if (selOnly.IsChecked == false)
                {
                    FilteredElementCollector f1 = new FilteredElementCollector(DuctInsulationCheckCommand.doc);
                    f1.OfCategory(BuiltInCategory.OST_DuctCurves);
                    f1.WhereElementIsNotElementType();
                    List<ElementId> ducts = (List<ElementId>)f1.ToElementIds();
                    eles.AddRange(ducts);
                    FilteredElementCollector f2 = new FilteredElementCollector(DuctInsulationCheckCommand.doc);
                    f2.OfCategory(BuiltInCategory.OST_DuctFitting);
                    f2.WhereElementIsNotElementType();
                    List<ElementId> fitting = (List<ElementId>)f2.ToElementIds();
                    eles.AddRange(fitting);
                }
                else
                {
                    IList<ElementId> selElems = (IList<ElementId>)DuctInsulationCheckCommand.uidoc.Selection.GetElementIds();
                    foreach (ElementId ele in selElems)
                    {
                        Element element = DuctInsulationCheckCommand.doc.GetElement(ele);
                        if (element.Category.Id.IntegerValue == Convert.ToInt32(BuiltInCategory.OST_DuctCurves) || element.Category.Id.IntegerValue == Convert.ToInt32(BuiltInCategory.OST_DuctFitting))
                        {
                            eles.Add(ele);
                        }
                    }
                }

                string insulType;
                if ((bool)inner.IsChecked)
                {
                    insulType = "Autodesk.Revit.DB.Mechanical.DuctInsulation";
                }
                else
                {
                    insulType = "Autodesk.Revit.DB.Mechanical.DuctLining";
                }
                foreach (ElementId ele in eles)
                {
                    Element element = DuctInsulationCheckCommand.doc.GetElement(ele);
                    IList<ElementId> deps = element.GetDependentElements(null);
                    int i = 0;
                    foreach (ElementId dep in deps)
                    {
                        Element element2 = DuctInsulationCheckCommand.doc.GetElement(dep);
                        if (element2.GetType().ToString() == insulType)
                        {
                            i++;
                            DuctInsulation ductInsulation = element2 as DuctInsulation;
                            dt.Rows.Add(ele.ToString(), element2.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsValueString().ToString(), true, ductInsulation.Thickness * 304.8);
                        }
                    }
                    if (i == 0)
                    {
                        dt.Rows.Add(ele.ToString(), "-", false, 0);
                    }
                }
                CheckB.IsEnabled = false;
            }
            catch
            {

            }            
        }

        private void exB1_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ElementTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<ElementId> elementIds = new List<ElementId>();
            DataRowView dataRow = (DataRowView)e.AddedItems[0];
            ElementId elementId = new ElementId(Convert.ToInt32(dataRow[0]));
            Element ele = DuctInsulationCheckCommand.doc.GetElement(elementId);
            if (ele == null)
            {

            }
            else
            {
                elementIds.Add(elementId);
                DuctInsulationCheckCommand.uidoc.Selection.SetElementIds(elementIds);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }

}
