using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using System.Data;
using System.Collections.Generic;
using System;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Collections;

namespace EEPRevitPlagin.EEPRPCommandModules.ListNumbering
{
    /// <summary>
    /// Interaction logic for ListNumberingWPF.xaml
    /// </summary>
    public partial class ListNumberingWPF : Window
    {
        ExternalEvent externalEvent;
        bool canEdit = false;
        List<TreeViewCheckBoxItem> treeViewItemsL1 = new List<TreeViewCheckBoxItem>();
        List<TreeViewCheckBoxItem> treeViewItemsL2 = new List<TreeViewCheckBoxItem>();
        List<TreeViewCheckBoxItem> treeViewItemsL3 = new List<TreeViewCheckBoxItem>();
        List<TreeViewCheckBoxItem> treeViewItemsL4 = new List<TreeViewCheckBoxItem>();
        List<TreeViewCheckBoxItem> treeViewItemsL5 = new List<TreeViewCheckBoxItem>();
        public static Part[] parts = new Part[6];
        int currentPartIndex = 0;
        List<TreeViewCheckBoxItem> AllSheets = new List<TreeViewCheckBoxItem>();

        public ListNumberingWPF(ExternalEvent e)
        {
            InitializeComponent();
            ParamCB.Items.Clear();
            PrefParamCB.Items.Clear();
            SuffParamCB.Items.Clear();
            foreach (Parameter parameter in ListNumberingCommand.SheetParameters)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = parameter.Definition.Name;
                comboBoxItem.Tag = parameter.Id;
                ComboBoxItem comboBoxItem1 = new ComboBoxItem();
                comboBoxItem1.Content = parameter.Definition.Name;
                comboBoxItem1.Tag = parameter.Id;
                ComboBoxItem comboBoxItem2 = new ComboBoxItem();
                comboBoxItem2.Content = parameter.Definition.Name;
                comboBoxItem2.Tag = parameter.Id;
                if (parameter.Definition.Name == "Ш.НомерЛиста")
                {
                    ParamCB.Items.Add(comboBoxItem);
                    ParamCB.SelectedIndex = 0;
                }
                else
                {

                    PrefParamCB.Items.Add(comboBoxItem1);
                    SuffParamCB.Items.Add(comboBoxItem2);
                }
            }
            List<string> addedItems = new List<string>();
            FormatCB.SelectedIndex = 0;
            foreach (ListNumberingCommand.Sheet sheet in ListNumberingCommand.sheets)
            {
                if (!addedItems.Contains(sheet.Level1))
                {
                    TreeViewCheckBoxItem itemL1 = new TreeViewCheckBoxItem();
                    itemL1.Text = sheet.Level1;
                    itemL1.IsVisibled = "Hidden";
                    itemL1.Children = new List<TreeViewCheckBoxItem>();
                    addedItems.Add(sheet.Level1);
                    treeViewItemsL1.Add(itemL1);
                }
            }
            for (int i = 0; i < parts.GetLength(0); i++)
            {
                parts[i] = new Part()
                {
                    Format = 1,
                    Name = "part" + i.ToString(),
                    Order = i,
                    Prefix = "",
                    Suffix = "",
                    PrefixParameterName = ParamCB.Text,
                    PrefixParameterIndex = 0,
                    SuffixParameterName = ParamCB.Text,
                    SuffixParameterIndex = 0,
                    StartNumber = 1,
                    Step = 1,
                    IsActive = false
                };
            }
            foreach (TreeViewCheckBoxItem itemL1 in treeViewItemsL1)
            {
                foreach (ListNumberingCommand.Sheet sheet in ListNumberingCommand.sheets)
                {
                    if (!addedItems.Contains(sheet.Level1 + sheet.Level2) && sheet.Level1 == itemL1.Text)
                    {
                        TreeViewCheckBoxItem itemL2 = new TreeViewCheckBoxItem();
                        itemL2.Text = sheet.Level2;
                        itemL2.Parent = itemL1.Text;
                        itemL2.IsVisibled = "Hidden";
                        addedItems.Add(sheet.Level1 + sheet.Level2);
                        itemL2.Children = new List<TreeViewCheckBoxItem>();
                        itemL1.Children.Add(itemL2);
                        treeViewItemsL2.Add(itemL2);
                    }
                }
            }
            foreach (TreeViewCheckBoxItem itemL2 in treeViewItemsL2)
            {
                foreach (ListNumberingCommand.Sheet sheet in ListNumberingCommand.sheets)
                {
                    if (!addedItems.Contains(sheet.Level1 + sheet.Level2 + sheet.Level3) && sheet.Level2 == itemL2.Text && itemL2.Parent == sheet.Level1)
                    {
                        TreeViewCheckBoxItem itemL3 = new TreeViewCheckBoxItem();
                        itemL3.Text = sheet.Level3;
                        itemL3.Parent = sheet.Level1 + sheet.Level2;
                        itemL3.IsVisibled = "Hidden";
                        addedItems.Add(sheet.Level1 + sheet.Level2 + sheet.Level3);
                        itemL3.Children = new List<TreeViewCheckBoxItem>();
                        itemL2.Children.Add(itemL3);
                        treeViewItemsL3.Add(itemL3);
                    }
                }
            }
            foreach (TreeViewCheckBoxItem itemL3 in treeViewItemsL3)
            {
                foreach (ListNumberingCommand.Sheet sheet in ListNumberingCommand.sheets)
                {
                    if (!addedItems.Contains(sheet.Level1 + sheet.Level2 + sheet.Level3 + sheet.Level4) && sheet.Level3 == itemL3.Text && itemL3.Parent == sheet.Level1 + sheet.Level2)
                    {
                        TreeViewCheckBoxItem itemL4 = new TreeViewCheckBoxItem();
                        itemL4.Text = sheet.Level4;
                        itemL4.Parent = sheet.Level1 + sheet.Level2 + sheet.Level3;
                        itemL4.IsVisibled = "Hidden";
                        addedItems.Add(sheet.Level1 + sheet.Level2 + sheet.Level3 + sheet.Level4);
                        itemL4.Children = new List<TreeViewCheckBoxItem>();
                        itemL3.Children.Add(itemL4);
                        treeViewItemsL4.Add(itemL4);
                    }
                }
            }
            foreach (TreeViewCheckBoxItem itemL4 in treeViewItemsL4)
            {
                foreach (ListNumberingCommand.Sheet sheet in ListNumberingCommand.sheets)
                {
                    if (!addedItems.Contains(sheet.Level1 + sheet.Level2 + sheet.Level3 + sheet.Level4 + sheet.Level5) && sheet.Level4 == itemL4.Text && itemL4.Parent == sheet.Level1 + sheet.Level2 + sheet.Level3)
                    {
                        TreeViewCheckBoxItem itemL5 = new TreeViewCheckBoxItem();
                        itemL5.Text = sheet.Level5;
                        itemL5.Parent = sheet.Level1 + sheet.Level2 + sheet.Level3 + sheet.Level4;
                        itemL5.IsVisibled = "Hidden";
                        addedItems.Add(sheet.Level1 + sheet.Level2 + sheet.Level3 + sheet.Level4 + sheet.Level5);
                        itemL5.Children = new List<TreeViewCheckBoxItem>();
                        itemL4.Children.Add(itemL5);
                        treeViewItemsL5.Add(itemL5);
                    }
                }
            }
            foreach (ListNumberingCommand.Sheet sheet in ListNumberingCommand.sheets)
            {
                string all;
                TreeViewCheckBoxItem list = new TreeViewCheckBoxItem();
                switch (ListNumberingCommand.levelsCount)
                {
                    case 0:
                        list.Text = sheet.NumberText + "-" + sheet.Name;
                        list.IsVisibled = "Visible";
                        list.Id = sheet.Id;
                        Sheets.Items.Add(list);
                        AllSheets.Add(list);
                        break;
                    case 1:
                        foreach (TreeViewCheckBoxItem itemL1 in treeViewItemsL1)
                        {
                            if (sheet.Level1 == itemL1.Text)
                            {
                                list.Text = sheet.NumberText + "-" + sheet.Name;
                                list.IsVisibled = "Visible";
                                list.Id = sheet.Id;
                                itemL1.Children.Add(list);
                                AllSheets.Add(list);
                            }
                        }
                        break;
                    case 2:
                        all = sheet.Level1;
                        foreach (TreeViewCheckBoxItem itemL2 in treeViewItemsL2)
                        {
                            if (itemL2.Parent == all)
                            {
                                list.Text = sheet.NumberText + "-" + sheet.Name;
                                list.Id = sheet.Id;
                                list.IsVisibled = "Visible";
                                itemL2.Children.Add(list);
                                AllSheets.Add(list);
                            }
                        }
                        break;
                    case 3:
                        all = sheet.Level1 + sheet.Level2;
                        foreach (TreeViewCheckBoxItem itemL3 in treeViewItemsL3)
                        {
                            if (itemL3.Parent == all)
                            {
                                list.Text = sheet.NumberText + "-" + sheet.Name;
                                list.Id = sheet.Id;
                                list.IsVisibled = "Visible";
                                itemL3.Children.Add(list);
                                AllSheets.Add(list);
                            }
                        }
                        break;
                    case 4:
                        all = sheet.Level1 + sheet.Level2 + sheet.Level3;
                        foreach (TreeViewCheckBoxItem itemL4 in treeViewItemsL4)
                        {
                            if (itemL4.Parent == all)
                            {
                                list.Text = sheet.NumberText + "-" + sheet.Name;
                                list.Id = sheet.Id;
                                list.IsVisibled = "Visible";
                                itemL4.Children.Add(list);
                                AllSheets.Add(list);
                            }
                        }
                        break;
                    case 5:
                        all = sheet.Level1 + sheet.Level2 + sheet.Level3 + sheet.Level4;
                        foreach (TreeViewCheckBoxItem itemL5 in treeViewItemsL5)
                        {
                            if (itemL5.Parent == all)
                            {
                                list.Text = sheet.NumberText + "-" + sheet.Name;
                                list.Id = sheet.Id;
                                list.IsVisibled = "Visible";
                                itemL5.Children.Add(list);
                                AllSheets.Add(list);
                            }
                        }
                        break;
                    case 6:
                        break;
                }
            }
            FormatCB.SelectedIndex = 0;
            Sheets.ItemsSource = treeViewItemsL1;
            externalEvent = e;
            canEdit = true;

        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void exB1_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CheckB_Click(object sender, RoutedEventArgs e)
        {
            ListNumberingCommand.sheetsToAddNumber.Clear();
            foreach (ListBoxItem listBoxItem in OrderList.Items)
            {
                ListNumberingCommand.sheetsToAddNumber.Add(listBoxItem.Tag.ToString());
            }
            ListNumberingCommand.param = ParamCB.Text;
            if (ParamCB.Text == "Ш.НомерЛиста")
            {
                ListNumberingCommand.seperator = SeperatorTB.Text;
                ListNumberingCommand.seperatorIn = SeperatorInTB.Text;
                externalEvent.Raise();
            }
            else
            {
                TaskDialog.Show("error", "Add required Shared parameter");
            }

        }

        private void Item_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item)
            {
                DragDrop.DoDragDrop(item, item.Content, System.Windows.DragDropEffects.Move);
            }
        }

        private void Item_Drop(object sender, System.Windows.DragEventArgs e)
        {
            try
            {
                if (sender is ListBoxItem targetItem && e.Data.GetData(System.Windows.DataFormats.Text) is string source)
                {

                    ItemCollection itemCollection = OrderList.Items;
                    List<string> listl = new List<string>();
                    List<string> listr = new List<string>();
                    foreach (ListBoxItem item in itemCollection)
                    {
                        listr.Add(item.Tag.ToString().Split('$')[2].ToString());
                        listl.Add(item.Tag.ToString().Split('$')[0].ToString());
                    }
                    int sourceIndex = 0;
                    // get the index of source.text
                    for (int i = 0; i < OrderList.Items.Count; i++)
                    {
                        if (((ListBoxItem)OrderList.Items[i]).Content == source)
                        {
                            sourceIndex = i;
                        }
                    }
                    int targetIndex = OrderList.Items.IndexOf(targetItem);
                    if (targetIndex != sourceIndex)
                    {
                        var temp = OrderList.Items[sourceIndex];
                        OrderList.Items.RemoveAt(sourceIndex);
                        OrderList.Items.Insert(targetIndex, temp);
                    }
                    int ii = -1;


                    foreach (ListBoxItem item2 in OrderList.Items)
                    {
                        string rest;
                        if (leftTB.Text == "")
                        {
                            rest = item2.Content.ToString().Replace("-" + item2.Tag.ToString().Split('$')[2].ToString(), "").Replace(item2.Tag.ToString().Split('$')[0].ToString(), "");
                        }
                        else
                        {
                            rest = item2.Content.ToString().Replace("-" + item2.Tag.ToString().Split('$')[2].ToString(), "").Replace(leftTB.Text, "").Replace(item2.Tag.ToString().Split('$')[0].ToString(), "");
                        }
                        ii++;
                        //int p1 = Convert.ToInt32(item2.Tag.ToString().Split('$')[1].ToString());
                        //Element element = ListNumberingCommand.doc.GetElement(new ElementId(p1));

                        item2.Content = leftTB.Text + listl[ii] + rest;// + item2.Content.ToString().Replace(leftTB.Text,""). Replace(item2.Tag.ToString().Split('$')[0].ToString(), "");
                        item2.Tag = listl[ii] + "$" + item2.Tag.ToString().Split('$')[1].ToString() + "$" + listr[ii];
                    }
                }
                //edit the content & tags

                //SortTheList();
                OrderList.Items.Refresh();
            }
            catch
            {

            }



        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (TreeViewCheckBoxItem item in AllSheets)
                {
                    if (item.IsChecked == true)
                    {
                        ListBoxItem newItem = new ListBoxItem();
                        newItem.Content = item.Text;
                        newItem.Tag = item.Id;
                        bool found = false;
                        foreach (ListBoxItem item2 in OrderList.Items)
                        {
                            if (item2.Tag.ToString() == newItem.Tag.ToString() && item2.Content == newItem.Content)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            OrderList.Items.Add(newItem);
                        }

                    }
                }
                #region find mask and numbers automatically-Previously
                /*
                //find the general mask based on first 3 items
                List<int[]> ints = new List<int[]>();
                foreach (ListBoxItem item2 in OrderList.Items)
                {
                    ViewSheet element = (ViewSheet)ListNumberingCommand.doc.GetElement(new ElementId((int)item2.Tag));
                    int[] result = Regex
                          .Matches(element.SheetNumber, "[0-9]+") // groups of integer numbers
                          .OfType<Match>()
                          .Select(match => int.Parse(match.Value))
                          .ToArray();
                    ints.Add(result);
                }
                int ii = Math.Min(Math.Min(ints[0].Count(), ints[1].Count()), Math.Min(ints[1].Count(), ints[2].Count()));
                int iiReq = -1;
                for (int i = 0; i < ii; i++)
                {
                    if (Math.Abs(ints[0][i] - ints[1][i]) != 0 & Math.Abs(ints[1][i] - ints[2][i]) != 0)
                    {
                        iiReq = i;
                    }
                }
                //find the number
                string[] strings;
                ViewSheet element2 = (ViewSheet)ListNumberingCommand.doc.GetElement(new ElementId((int)((ListBoxItem)OrderList.Items[0]).Tag));
                if (Regex.Matches(element2.SheetNumber, ints[0][iiReq].ToString()).Count == 1)
                {
                    strings = element2.SheetNumber.Split(new string[] { ints[0][iiReq].ToString() }, StringSplitOptions.RemoveEmptyEntries);
                    if (strings.Length == 2)
                    {
                        if (element2.SheetNumber.IndexOf(strings[0]) < element2.SheetNumber.IndexOf(ints[0][iiReq].ToString()))
                        {
                            leftTB.Text = strings[0];
                            rightTB.Text = strings[1];
                        }
                        else
                        {
                            leftTB.Text = strings[1];
                            rightTB.Text = strings[0];
                        }
                    }
                    else if (strings.Length == 1)
                    {
                        if (element2.SheetNumber.IndexOf(strings[0]) < element2.SheetNumber.IndexOf(ints[0][iiReq].ToString()))
                        {
                            leftTB.Text = strings[0];
                            rightTB.Text = "";
                        }
                        else
                        {
                            leftTB.Text = "";
                            rightTB.Text = strings[0];
                        }
                    }


                }
                else
                {
                    ViewSheet element3 = (ViewSheet)ListNumberingCommand.doc.GetElement(new ElementId((int)((ListBoxItem)OrderList.Items[1]).Tag));
                    strings = element3.SheetNumber.Split(new string[] { ints[1][iiReq].ToString() }, StringSplitOptions.RemoveEmptyEntries);
                    if (strings.Length == 2)
                    {
                        if (element2.SheetNumber.IndexOf(strings[0]) < element2.SheetNumber.IndexOf(ints[1][iiReq].ToString()))
                        {
                            leftTB.Text = strings[0];
                            rightTB.Text = strings[1];
                        }
                        else
                        {
                            leftTB.Text = strings[1];
                            rightTB.Text = strings[0];
                        }
                    }
                    else if (strings.Length == 1)
                    {
                        if (element2.SheetNumber.IndexOf(strings[0]) < element2.SheetNumber.IndexOf(ints[1][iiReq].ToString()))
                        {
                            leftTB.Text = strings[0];
                            rightTB.Text = "";
                        }
                        else
                        {
                            leftTB.Text = "";
                            rightTB.Text = strings[0];
                        }
                    }
                }
                //check for others

                foreach (ListBoxItem item2 in OrderList.Items)
                {
                    ViewSheet element = (ViewSheet)ListNumberingCommand.doc.GetElement(new ElementId((int)item2.Tag));
                    bool isNumeric = true;
                    if (leftTB.Text != "")
                    {
                        if (rightTB.Text != "")
                        {
                            isNumeric = int.TryParse(element.SheetNumber.Replace(leftTB.Text, "").Replace(rightTB.Text, ""), out int n);
                        }
                        else
                        {
                            isNumeric = int.TryParse(element.SheetNumber.Replace(leftTB.Text, ""), out int n);
                        }
                    }
                    else
                    {
                        if (rightTB.Text != "")
                        {
                            isNumeric = int.TryParse(element.SheetNumber.Replace(rightTB.Text, ""), out int n);
                        }
                        else
                        {
                            isNumeric = int.TryParse(element.SheetNumber, out int n);
                        }
                    }
                    if (!isNumeric)
                    {
                        TaskDialog.Show("Error", "There is no mask");
                        leftTB.Text = "Need to edit";
                        rightTB.Text = "Need to edit";
                        break;
                    }
                }




                //add numbers to second part of tag
                foreach (ListBoxItem item2 in OrderList.Items)
                {
                    ViewSheet element = (ViewSheet)ListNumberingCommand.doc.GetElement(new ElementId((int)item2.Tag));
                    bool isNumeric = true;
                    if (leftTB.Text != "")
                    {
                        if (rightTB.Text != "")
                        {                        
                            isNumeric = int.TryParse(element.SheetNumber.Replace(leftTB.Text, "").Replace(rightTB.Text, ""), out int n);
                            item2.Tag = n.ToString() + "$" + item2.Tag;
                        }
                        else
                        {
                            isNumeric = int.TryParse(element.SheetNumber.Replace(leftTB.Text, ""), out int n);
                            item2.Tag = n.ToString() + "$" + item2.Tag;
                        }
                    }
                    else
                    {
                        if (rightTB.Text != "")
                        {
                            isNumeric = int.TryParse(element.SheetNumber.Replace(rightTB.Text, ""), out int n);
                            item2.Tag = n.ToString() + "$" + item2.Tag;
                        }
                        else
                        {
                            isNumeric = int.TryParse(element.SheetNumber, out int n);
                            item2.Tag = n.ToString() + "$" + item2.Tag;
                        }
                    }
                }
                */
                #endregion
                //add numbers to second part of tag
                foreach (ListBoxItem item2 in OrderList.Items)
                {
                    ViewSheet element = (ViewSheet)ListNumberingCommand.doc.GetElement(new ElementId((int)item2.Tag));
                    if (leftTB.Text != "")
                    {
                        string[] nums = Regex.Matches(element.SheetNumber.Replace(leftTB.Text, ""), "[0-9]+").OfType<Match>().Select(match => match.Value).ToArray();
                        //isNumeric = int.TryParse(element.SheetNumber.Replace(leftTB.Text, "").Replace(rightTB.Text, ""), out int n);
                        item2.Tag = nums[0].ToString() + "$" + item2.Tag + "$" + element.SheetNumber;
                    }
                    else
                    {
                        int[] nums = Regex.Matches(element.SheetNumber, "[0-9]+").OfType<Match>().Select(match => int.Parse(match.Value)).ToArray();
                        //isNumeric = int.TryParse(element.SheetNumber.Replace(rightTB.Text, ""), out int n);
                        item2.Tag = nums[0].ToString() + "$" + item2.Tag + "$" + element.SheetNumber;
                    }

                }
                //sort by mask
                SortTheList();
            }
            catch
            {

            }

        }
        private void SortTheList()
        {
            Dictionary<int, ListBoxItem> keyValuePairs = new Dictionary<int, ListBoxItem>();
            ItemCollection items = OrderList.Items;
            foreach (ListBoxItem item2 in OrderList.Items)
            {
                keyValuePairs.Add(Convert.ToInt32(item2.Tag.ToString().Split('$')[0]), item2);
            }

            var sortedDict = from entry in keyValuePairs orderby entry.Key ascending select entry;
            OrderList.Items.Clear();
            foreach (var ss in sortedDict)
            {
                OrderList.Items.Add(ss.Value);
            }
        }

        private void StackPanel_MouseDown(object sender, SelectionChangedEventArgs e)
        {
            StackPanel listBoxItem = (StackPanel)e.AddedItems[0];
            currentPartIndex = partsList.Items.IndexOf(listBoxItem);
            //show the information for the selected part
            canEdit = false;
            StartTB.Text = parts[currentPartIndex].StartNumber.ToString();
            StepTB.Text = parts[currentPartIndex].Step.ToString();
            PrefTB.Text = parts[currentPartIndex].Prefix.ToString();
            SuffTB.Text = parts[currentPartIndex].Suffix.ToString();
            PrefParamCB.SelectedIndex = parts[currentPartIndex].PrefixParameterIndex;
            SuffParamCB.SelectedIndex = parts[currentPartIndex].SuffixParameterIndex;
            FormatCB.SelectedIndex = parts[currentPartIndex].Format - 1;
            needToPrefixC.IsChecked = parts[currentPartIndex].NeedToPrefixparam;
            needToSuffixC.IsChecked = parts[currentPartIndex].NeedToSuffixparam;
            SeperatorInTB.Text = parts[currentPartIndex].PartSeperator;
            canEdit = true;
        }

        private void StartTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (canEdit)
            {
                try
                {
                    Part newPart = new Part()
                    {
                        PartSeperator = SeperatorInTB.Text,
                        Format = parts[currentPartIndex].Format,
                        Name = parts[currentPartIndex].Name,
                        Order = currentPartIndex,
                        Prefix = PrefTB.Text,
                        Suffix = SuffTB.Text,
                        PrefixParameterName = PrefParamCB.Text,
                        PrefixParameterIndex = PrefParamCB.SelectedIndex,
                        SuffixParameterName = SuffParamCB.Text,
                        SuffixParameterIndex = SuffParamCB.SelectedIndex,
                        StartNumber = Convert.ToInt32(StartTB.Text),
                        Step = Convert.ToInt32(StepTB.Text),
                        IsActive = parts[currentPartIndex].IsActive,
                        NeedToPrefixparam = parts[currentPartIndex].NeedToPrefixparam,
                        NeedToSuffixparam = parts[currentPartIndex].NeedToSuffixparam,
                    };
                    parts[currentPartIndex] = newPart;
                }
                catch
                {

                }
            }
        }

        private void FormatCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canEdit)
            {
                TextBlock comboBoxItem = (TextBlock)e.AddedItems[0];
                int itemIndex = FormatCB.Items.IndexOf(comboBoxItem);
                Part newPart = new Part()
                {
                    PartSeperator = SeperatorInTB.Text,
                    Format = itemIndex + 1,
                    Name = parts[currentPartIndex].Name,
                    Order = currentPartIndex,
                    Prefix = PrefTB.Text,
                    Suffix = SuffTB.Text,
                    PrefixParameterName = PrefParamCB.Text,
                    PrefixParameterIndex = PrefParamCB.SelectedIndex,
                    SuffixParameterName = SuffParamCB.Text,
                    SuffixParameterIndex = SuffParamCB.SelectedIndex,
                    StartNumber = Convert.ToInt32(StartTB.Text),
                    Step = Convert.ToInt32(StepTB.Text),
                    IsActive = parts[currentPartIndex].IsActive,
                    NeedToPrefixparam = parts[currentPartIndex].NeedToPrefixparam,
                    NeedToSuffixparam = parts[currentPartIndex].NeedToSuffixparam
                };
                parts[currentPartIndex] = newPart;
            }
        }

        private void PrefParamCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canEdit)
            {
                ComboBoxItem comboBoxItem = (ComboBoxItem)e.AddedItems[0];
                int itemIndex = PrefParamCB.Items.IndexOf(comboBoxItem);
                Part newPart = new Part()
                {
                    PartSeperator = SeperatorInTB.Text,
                    Format = FormatCB.Text.Length,
                    Name = parts[currentPartIndex].Name,
                    Order = currentPartIndex,
                    Prefix = PrefTB.Text,
                    Suffix = SuffTB.Text,
                    PrefixParameterName = comboBoxItem.Content.ToString(),
                    PrefixParameterIndex = itemIndex,
                    SuffixParameterName = SuffParamCB.Text,
                    SuffixParameterIndex = SuffParamCB.SelectedIndex,
                    StartNumber = Convert.ToInt32(StartTB.Text),
                    Step = Convert.ToInt32(StepTB.Text),
                    IsActive = parts[currentPartIndex].IsActive,
                    NeedToPrefixparam = parts[currentPartIndex].NeedToPrefixparam,
                    NeedToSuffixparam = parts[currentPartIndex].NeedToSuffixparam
                };
                parts[currentPartIndex] = newPart;
            }
        }

        private void SuffParamCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canEdit)
            {
                ComboBoxItem comboBoxItem = (ComboBoxItem)e.AddedItems[0];
                int itemIndex = SuffParamCB.Items.IndexOf(comboBoxItem);
                Part newPart = new Part()
                {
                    PartSeperator = SeperatorInTB.Text,
                    Format = FormatCB.Text.Length,
                    Name = parts[currentPartIndex].Name,
                    Order = currentPartIndex,
                    Prefix = PrefTB.Text,
                    Suffix = SuffTB.Text,
                    PrefixParameterName = PrefParamCB.Text,
                    PrefixParameterIndex = PrefParamCB.SelectedIndex,
                    SuffixParameterName = comboBoxItem.Content.ToString(),
                    SuffixParameterIndex = itemIndex,
                    StartNumber = Convert.ToInt32(StartTB.Text),
                    Step = Convert.ToInt32(StepTB.Text),
                    IsActive = parts[currentPartIndex].IsActive,
                    NeedToPrefixparam = parts[currentPartIndex].NeedToPrefixparam,
                    NeedToSuffixparam = parts[currentPartIndex].NeedToSuffixparam
                };
                parts[currentPartIndex] = newPart;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                parts[0].IsActive = true;
            }
            catch
            {

            }


        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            parts[1].IsActive = true;
        }

        private void CheckBox_Checked_2(object sender, RoutedEventArgs e)
        {
            parts[2].IsActive = true;
        }

        private void CheckBox_Checked_3(object sender, RoutedEventArgs e)
        {
            parts[3].IsActive = true;
        }

        private void CheckBox_Checked_4(object sender, RoutedEventArgs e)
        {
            parts[4].IsActive = true;
        }

        private void CheckBox_Checked_5(object sender, RoutedEventArgs e)
        {
            parts[5].IsActive = true;
        }

        private void CheckBox_Checked_6(object sender, RoutedEventArgs e)
        {
            parts[currentPartIndex].NeedToPrefixparam = true;
        }

        private void CheckBox_Checked_7(object sender, RoutedEventArgs e)
        {
            parts[currentPartIndex].NeedToSuffixparam = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            TreeViewCheckBoxItem checkBoxItem = Sheets.SelectedItem as TreeViewCheckBoxItem;
            if (checkBoxItem.IsVisibled == "Hidden")
            {
                try
                {
                    foreach (TreeViewCheckBoxItem item in checkBoxItem.Children)
                    {
                        ListBoxItem newItem = new ListBoxItem();
                        newItem.Content = item.Text;
                        newItem.Tag = item.Id;
                        bool found = false;
                        foreach (ListBoxItem item2 in OrderList.Items)
                        {
                            if (item2.Tag.ToString() == newItem.Tag.ToString() && item2.Content == newItem.Content)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            OrderList.Items.Add(newItem);
                        }
                    }
                    foreach (ListBoxItem item2 in OrderList.Items)
                    {
                        ViewSheet element = (ViewSheet)ListNumberingCommand.doc.GetElement(new ElementId((int)item2.Tag));
                        if (leftTB.Text != "")
                        {
                            string[] nums = Regex.Matches(element.SheetNumber.Replace(leftTB.Text, ""), "[0-9]+").OfType<Match>().Select(match => match.Value).ToArray();
                            //isNumeric = int.TryParse(element.SheetNumber.Replace(leftTB.Text, "").Replace(rightTB.Text, ""), out int n);
                            item2.Tag = nums[0].ToString() + "$" + item2.Tag + "$" + element.SheetNumber;
                        }
                        else
                        {
                            string[] nums = Regex.Matches(element.SheetNumber, "[0-9]+").OfType<Match>().Select(match => match.Value).ToArray();
                            //isNumeric = int.TryParse(element.SheetNumber.Replace(rightTB.Text, ""), out int n);
                            item2.Tag = nums[0].ToString() + "$" + item2.Tag + "$" + element.SheetNumber;
                        }

                    }
                    //sort by mask
                    SortTheList();
                }
                catch
                {

                }
            }
        }

        private void OrderList_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                try
                {
                    OrderList.Items.RemoveAt(OrderList.SelectedIndex);
                }
                catch
                {

                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            parts[0].IsActive = true;
            try
            {
                int i = -1;
                foreach (ListBoxItem item2 in OrderList.Items)
                {
                    i++;
                    //get the list
                    ElementId elementId = new ElementId(Convert.ToInt32(item2.Tag.ToString().Split('$')[1]));
                    ViewSheet sheet = (ViewSheet)ListNumberingCommand.doc.GetElement(elementId);
                    //get Value
                    string newListNumber = "";
                    for (int j = 0; j < 6; j++)
                    {
                        if (parts[j].IsActive)
                        {
                            string partNumber = "";
                            int t = 0;
                            for (int k = 0; k < 5; k++)
                            {

                                string partIn = "";
                                switch (k)
                                {
                                    case 0:
                                        if (parts[j].NeedToPrefixparam)
                                        {
                                            partIn = sheet.LookupParameter(parts[j].PrefixParameterName).AsString();
                                        }
                                        break;
                                    case 1:
                                        if (parts[j].Prefix != "")
                                        {
                                            partIn = parts[j].Prefix;
                                        }
                                        break;
                                    case 2:
                                        partIn = (parts[j].StartNumber + i * parts[j].Step).ToString("D" + parts[j].Format.ToString());

                                        break;
                                    case 3:
                                        if (parts[j].Suffix != "")
                                        {
                                            partIn = parts[j].Suffix;
                                        }
                                        break;
                                    case 4:
                                        if (parts[j].NeedToSuffixparam)
                                        {
                                            partIn = sheet.LookupParameter(parts[j].SuffixParameterName).AsString();
                                        }
                                        break;
                                }
                                if (k != 0)
                                {
                                    if (partIn != "")
                                    {
                                        if (t == 0)
                                        {
                                            partNumber = partIn;
                                            t++;
                                        }
                                        else
                                        {
                                            partNumber = partNumber + parts[j].PartSeperator + partIn;
                                            t++;
                                        }
                                    }
                                }
                                else
                                {
                                    if (partIn != "")
                                    {
                                        partNumber = partIn;
                                        t++;
                                    }
                                }
                            }
                            if (j != 0)
                            {
                                newListNumber = newListNumber + SeperatorTB.Text + partNumber;
                            }
                            else
                            {
                                newListNumber = partNumber;
                            }
                        }
                    }
                    //number on the list
                    if (leftTB.Text == "")
                    {
                        item2.Content = newListNumber + "-" + sheet.Name;
                        item2.Tag = newListNumber + "$" + item2.Tag.ToString().Split('$')[1] + "$" + newListNumber;
                    }
                    else
                    {
                        item2.Content = leftTB.Text + item2.Content.ToString().Replace(sheet.Name, "").Replace(leftTB.Text, "").Replace(item2.Tag.ToString().Split('$')[0], newListNumber) + sheet.Name;
                        item2.Tag = newListNumber + "$" + item2.Tag.ToString().Split('$')[1] + "$" + leftTB.Text + item2.Tag.ToString().Split('$')[2].Replace(leftTB.Text, "").Replace(item2.Tag.ToString().Split('$')[0], newListNumber);
                    }
                }
            }
            catch
            {

            }
            #region Old
            /*
                        try
                        {
                            int i = -1;
                            foreach (ListBoxItem item2 in OrderList.Items)
                            {
                                i++;
                                //get the list
                                ElementId elementId = new ElementId(Convert.ToInt32(item2.Tag.ToString().Split('$')[1]));
                                ViewSheet sheet = (ViewSheet)ListNumberingCommand.doc.GetElement(elementId);
                                //get Value
                                string newListNumber = "";
                                for (int j = 0; j < 6; j++)
                                {
                                    if (parts[j].IsActive)
                                    {
                                        string partNumber = "";
                                        int t = 0;
                                        for (int k = 0; k < 5; k++)
                                        {

                                            string partIn = "";
                                            switch (k)
                                            {
                                                case 0:
                                                    if (parts[j].NeedToPrefixparam)
                                                    {
                                                        partIn = sheet.LookupParameter(parts[j].PrefixParameterName).AsString();
                                                    }
                                                    break;
                                                case 1:
                                                    if (parts[j].Prefix != "")
                                                    {
                                                        partIn = parts[j].Prefix;
                                                    }
                                                    break;
                                                case 2:
                                                    partIn = (parts[j].StartNumber + i * parts[j].Step).ToString("D" + parts[j].Format.ToString());

                                                    break;
                                                case 3:
                                                    if (parts[j].Suffix != "")
                                                    {
                                                        partIn = parts[j].Suffix;
                                                    }
                                                    break;
                                                case 4:
                                                    if (parts[j].NeedToSuffixparam)
                                                    {
                                                        partIn = sheet.LookupParameter(parts[j].SuffixParameterName).AsString();
                                                    }
                                                    break;
                                            }
                                            if (k != 0)
                                            {
                                                if (partIn != "")
                                                {
                                                    if (t == 0)
                                                    {
                                                        partNumber = partIn;
                                                        t++;
                                                    }
                                                    else
                                                    {
                                                        partNumber = partNumber + parts[j].PartSeperator + partIn;
                                                        t++;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (partIn != "")
                                                {
                                                    partNumber = partIn;
                                                    t++;
                                                }
                                            }
                                        }
                                        if (j != 0)
                                        {
                                            newListNumber = newListNumber + SeperatorTB.Text + partNumber;
                                        }
                                        else
                                        {
                                            newListNumber = partNumber;
                                        }
                                    }
                                }
                                //number on the list

                                item2.Content = leftTB.Text + item2.Content.ToString().Replace(sheet.Name,""). Replace(leftTB.Text, "").Replace(item2.Tag.ToString().Split('$')[0], newListNumber)+sheet.Name;
                                item2.Tag = newListNumber+"$"+ item2.Tag.ToString().Split('$')[1]+"$"+ leftTB.Text+item2.Tag.ToString().Split('$')[2].Replace(leftTB.Text,"").Replace(item2.Tag.ToString().Split('$')[0], newListNumber);
                            }
                        }
                        catch
                        {

                        }
            */
            #endregion
        }

    }

    public class TreeViewCheckBoxItem
    {

        public string Text { get; set; }
        public bool IsChecked { get; set; }

        public int Id { get; set; }
        public string IsVisibled { get; set; }

        public string Parent { get; set; }
        public List<TreeViewCheckBoxItem> Children { get; set; }

        public bool IsSheet => IsSheetFunc();

        private bool IsSheetFunc()
        {
            if (Children == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    }
    public class Part
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public int StartNumber { get; set; }
        public int Step { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public int Format { get; set; }
        public bool NeedToPrefixparam { get; set; }
        public bool NeedToSuffixparam { get; set; }
        public string PrefixParameterName { get; set; }
        public string SuffixParameterName { get; set; }
        public int PrefixParameterIndex { get; set; }
        public int SuffixParameterIndex { get; set; }
        public string PartSeperator { get; set; }

    }

}
