using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Forms;
using System.Data;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Navisworks.Api.Automation;



namespace EEPRevitPlagin.EEPRPCommandModules.RevitServerExport
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class RevitServerExportWPF : Window
    {
        DataTable dt = new DataTable();
        public RevitServerExportWPF()
        {
            InitializeComponent();
            ModelTable.CanUserAddRows = false;

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsValidRevision(revision.Text) && IsValidServer(server.Text))
                {
                    TreeNode treeNode1 = new TreeNode();
                    if (dt.Columns.Count == 0)
                    {
                        dt.Columns.Add(new DataColumn("Model Name", typeof(string)));
                        dt.Columns.Add(new DataColumn("Model Path", typeof(string)));
                        dt.Columns.Add(new DataColumn("Export Revit files", typeof(bool)));
                        dt.Columns.Add(new DataColumn("Export to Navisworks", typeof(bool)));
                        dt.Columns.Add(new DataColumn("Export IFC files", typeof(bool)));
                        ModelTable.DataContext = dt;
                    }
                    RevitServerTools.GetSpecificFolder(treeNode1, "/|", server.Text, revision.Text);
                    ModelTable.CanUserAddRows = false;
                    ModelTable.CanUserDeleteRows = false;
                    System.Windows.Forms.TreeView treeView1 = new System.Windows.Forms.TreeView();
                    treeView1.Nodes.Add(treeNode1);
                    foreach (TreeNode node in treeNode1.Nodes)
                    {
                        if (node.Tag.ToString() == "Folder")
                        {
                            combo1.Items.Add(node.Text);
                            addModelRow(dt, node, server.Text);
                        }

                    }
                    DataTable data = dt.Copy();
                    ModelTable.DataContext = dt;
                    B1.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    System.Windows.MessageBox.Show("The Server or the Revit revision are not true.\nPlease correct them and try again");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        private void addModelRow(DataTable dt, TreeNode tNode, string server)
        {
            foreach (TreeNode node in tNode.Nodes)
            {
                if (node.Tag.ToString() == "Model")
                {
                    var row = dt.NewRow();
                    row["Model Name"] = node.Text;
                    row["Model Path"] = GetModelPath(node.FullPath, server);
                    row["Export Revit files"] = false;
                    row["Export to Navisworks"] = false;
                    row["Export IFC files"] = false;
                    dt.Rows.Add(row);
                }
                else if (node.Tag.ToString() == "Folder")
                {
                    addModelRow(dt, node, server);
                }
            }
        }
        private void combo1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataTable newData = dt.Copy();
            ModelTable.DataContext = newData;
            for (int i = ModelTable.Items.Count - 1; i >= 0; i--)
            {
                DataRowView dataRow = (DataRowView)ModelTable.Items[i];
                string item = (string)dataRow.Row.ItemArray[1];
                if (!item.Contains(e.AddedItems[0].ToString()))
                {
                    dataRow.Delete();
                }
            }
        }
        private bool IsValidRevision(string str)
        {
            string[] validRevisions = { "2018", "2019", "2020", "2021", "2022", "2023", "2024" };
            if (validRevisions.Contains(str))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool IsValidServer(string str)
        {
            System.Net.IPAddress ipAddress = null;
            return System.Net.IPAddress.TryParse(str, out ipAddress);
        }
        private string GetModelPath(string str, string server)
        {
            string res = str.Replace('\\', '/');
            return res.Replace("Server", string.Format("RSN://{0}", server));
        }
        private void Export_Button(object sender, RoutedEventArgs e)
        {
            List<string> navisFiles = new List<string>();
            string nwcString = nwcTB.Text;
            string ifcString = ifcTB.Text;
            foreach (DataRowView row in ModelTable.Items)
            {
                if ((bool)row[2] || (bool)row[3] || (bool)row[4])
                {
                    ExportModels.ExportrvtModelFromServer(row[1].ToString(), folderTB.Text, RevitServerExportCommand.uiapp.Application);
                    string modelPath = folderTB.Text + @"\" + row[0].ToString();
                    if ((bool)row[3] || (bool)row[4])
                    {
                        Document doc = ExportModels.OpenModel(modelPath, RevitServerExportCommand.uiapp.Application);
                        if ((bool)row[3])
                        {
                            //export to navis
                            NavisworksExportOptions navisworksExportOptions = new NavisworksExportOptions();
                            navisworksExportOptions.ConvertElementProperties = true;
                            navisworksExportOptions.ConvertLinkedCADFormats = false;
                            navisworksExportOptions.Coordinates = NavisworksCoordinates.Shared;
                            navisworksExportOptions.DivideFileIntoLevels = false;
                            navisworksExportOptions.ExportElementIds = true;
                            navisworksExportOptions.ExportLinks = false;
                            // the host element export
                            navisworksExportOptions.ExportParts = false;
                            navisworksExportOptions.ExportRoomAsAttribute = false;
                            navisworksExportOptions.ExportRoomGeometry = false;
                            navisworksExportOptions.ExportScope = NavisworksExportScope.View;
                            if (ExportModels.GetNavisView(doc, nwcString) == null)
                            {
                                System.Windows.MessageBox.Show("No Navis view in " + (string)row[0]);
                                continue;
                            }
                            navisworksExportOptions.ViewId = ExportModels.GetNavisView(doc, nwcString);
                            navisworksExportOptions.ExportUrls = false;
                            navisworksExportOptions.FacetingFactor = 1;
                            navisworksExportOptions.FindMissingMaterials = false;
                            navisworksExportOptions.Parameters = NavisworksParameters.All;
                            doc.Export(folderTB.Text, row[0].ToString().Replace(".rvt", ".nwc"), navisworksExportOptions);
                            navisFiles.Add(folderTB.Text + @"\" + row[0].ToString().Replace(".rvt", ".nwc"));
                        }
                        if ((bool)row[4])
                        {
                            using (Transaction transaction = new Transaction(doc))
                            {
                                transaction.Start("IFC Export");
                                IFCExportOptions ifcExportOptions = new IFCExportOptions();
                                if (ExportModels.GetIFCView(doc, ifcString) == null)
                                {
                                    System.Windows.MessageBox.Show("No IFC view in " + (string)row[0]);
                                    continue;
                                }
                                ifcExportOptions.FilterViewId = ExportModels.GetIFCView(doc, ifcString);
                                ifcExportOptions.FileVersion = IFCVersion.IFC2x3;
                                ifcExportOptions.ExportBaseQuantities = true;
                                ifcExportOptions.WallAndColumnSplitting = true;
                                ifcExportOptions.SpaceBoundaryLevel = 0;
                                ifcExportOptions.AddOption("ExportAnnotations ", "false");
                                ifcExportOptions.AddOption("Export2DElements", "false");
                                ifcExportOptions.AddOption("ExportSpecificSchedules", "false");
                                ifcExportOptions.AddOption("TessellationLevelOfDetail", "0,5");
                                /*
                                ifcExportOptions.AddOption("ExportInternalRevitPropertySets", revitInternalPset);
                                ifcExportOptions.AddOption("ExportIFCCommonPropertySets", "true");
                                ifcExportOptions.AddOption("ExportAnnotations ", "true");
                                ifcExportOptions.AddOption("SpaceBoundaries ", "0");
                                ifcExportOptions.AddOption("ExportRoomsInView", "false");
                                ifcExportOptions.AddOption("Use2DRoomBoundaryForVolume ", "true");
                                ifcExportOptions.AddOption("UseFamilyAndTypeNameForReference ", "true");
                                ifcExportOptions.AddOption("Export2DElements", "false");
                                ifcExportOptions.AddOption("ExportPartsAsBuildingElements", "false");
                                ifcExportOptions.AddOption("ExportBoundingBox", "false");
                                ifcExportOptions.AddOption("ExportSolidModelRep", "true");
                                ifcExportOptions.AddOption("ExportSchedulesAsPsets", "false");
                                ifcExportOptions.AddOption("ExportSpecificSchedules", "false");
                                ifcExportOptions.AddOption("ExportLinkedFiles", "false");
                                ifcExportOptions.AddOption("IncludeSiteElevation", "true");
                                ifcExportOptions.AddOption("StoreIFCGUID", "true");
                                ifcExportOptions.AddOption("VisibleElementsOfCurrentView ", "true");
                                ifcExportOptions.AddOption("UseActiveViewGeometry", "true");
                                ifcExportOptions.AddOption("TessellationLevelOfDetail", "0,5");
                                ifcExportOptions.AddOption("ExportUserDefinedPsets", userDefPsetBool);
                                ifcExportOptions.AddOption("ExportUserDefinedPsetsFileName", userDefinedPset);
                                ifcExportOptions.AddOption("ActivePhase", phaseString);
                                ifcExportOptions.AddOption("SitePlacement", IN[4]);
                                ifcExportOptions.AddOption("ClassificationName","x");
                                */
                                doc.Export(folderTB.Text, row[0].ToString().Replace(".rvt", ".ifc"), ifcExportOptions);
                                transaction.Commit();
                            }
                        }
                        doc.Close(false);
                    }
                    if (!(bool)row[2])
                    {
                        File.Delete(modelPath);
                    }

                }
            }
            if (checkBox1.IsChecked == true && navisFiles != null)
            {
                NavisworksApplication navisworks = new NavisworksApplication();
                navisworks.DisableProgress();
                navisworks.Visible = false;
                navisworks.OpenFile(navisFiles[0]);
                for (int i = 1; i < navisFiles.Count; i++)
                {
                    navisworks.AppendFile(navisFiles[i]);
                }
                navisworks.SaveFile(folderTB.Text + @"\All.nwd");
                navisworks.Dispose();
            }



        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void B1_Copy_Click(object sender, RoutedEventArgs e)
        {
            string models = null;
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Project Models List (*.lpm)|*.lpm";
                dlg.Title = "Choose the text files with the models";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string filePath = dlg.FileName;
                    combo1.Items.Add(Path.GetFileNameWithoutExtension( filePath));
                    Stream fileStream = dlg.OpenFile();
                    StreamReader streamReader = new StreamReader(fileStream);
                    models = streamReader.ReadToEnd();
                }

            }
            string[] modelsPaths = models.Split('\n');

            foreach (string modelPath in modelsPaths)
            {
                if (dt.Columns.Count == 0)
                {
                    dt.Columns.Add(new DataColumn("Model Name", typeof(string)));
                    dt.Columns.Add(new DataColumn("Model Path", typeof(string)));
                    dt.Columns.Add(new DataColumn("Export Revit files", typeof(bool)));
                    dt.Columns.Add(new DataColumn("Export to Navisworks", typeof(bool)));
                    dt.Columns.Add(new DataColumn("Export IFC files", typeof(bool)));
                    DataTable data = dt.Copy();
                    ModelTable.DataContext = dt;
                }
                var row = dt.NewRow();
                row["Model Name"] = System.IO.Path.GetFileName(modelPath.Trim());
                row["Model Path"] = modelPath.Trim();
                row["Export Revit files"] = false;
                row["Export to Navisworks"] = false;
                row["Export IFC files"] = false;
                dt.Rows.Add(row);
            }
        }

        private void ModelTable_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string modelPath = @"C:\navis models\temp\a_2125_VARSHAVA_RD_AR.rvt";
            Document doc = ExportModels.OpenModel(modelPath, RevitServerExportCommand.uiapp.Application);
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            title.Text = "Revit Server - Выгузка Моделей";
            serIP.Content = "Сервер IP";
            revVer.Content = "Версия Revit";
            proj.Content = "Проект";
            navisView.Content= "Название вид Navis";
            ifcView.Content = "Название вид IFC";
            saveTo.Content = "Сохранить в:";
            B1.Content = "подключиться к серверу";
            B1_Copy.Content = "Добавить модели вручную";
            exB.Content = "Выгрузка выбранных мдоелей";
            exB1.Content = "Закрыть";
            checkBox1.Content = "Объединить все nwc в один файл nwd";
        }
    }
}
