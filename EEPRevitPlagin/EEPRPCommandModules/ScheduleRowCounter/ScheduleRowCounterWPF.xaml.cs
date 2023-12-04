using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace EEPRevitPlagin.EEPRPCommandModules.ScheduleRowCounter
{

    public partial class ScheduleRowCounterWPF : Window
    {
        static ViewSchedule viewSchedule;
        ExternalEvent externalEvent;
        public static string paraId;
        public static int startFrom;
        public static int step;
        public static int startFromRow;
        public static int endAtRow;
        public static string paraName;
        public static string paraType;
        public static int paraOrder;
        public static string prefix;
        public static string suffix;
        List<string> ids = new List<string>();
        List<string> paraNames = new List<string>();
        List<string> paraTypes = new List<string>();

        public ScheduleRowCounterWPF(ExternalEvent e)
        {
            InitializeComponent();
            ids = new List<string>();
            externalEvent = e;
            viewSchedule = (ViewSchedule)ScheduleRowCounterCommand.doc.ActiveView;
            EndAtRowT.Text = ScheduleRowCounterCommand.tableData.GetSectionData(SectionType.Body).NumberOfRows.ToString();
            int i = viewSchedule.Definition.GetFieldCount();
            for (int j = 0; j < i; j++)
            {
                var field = viewSchedule.Definition.GetField(j);
                ParaList.Items.Add(field.ColumnHeading);
                ids.Add(field.ParameterId.IntegerValue.ToString());
                paraNames.Add(field.GetName());
                paraTypes.Add(field.FieldType.ToString());
            }
        }
        private void exB1_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void StartB_Click(object sender, RoutedEventArgs e)
        {
            startFrom = Convert.ToInt16(StartFromT.Text);
            step = Convert.ToInt16(StepT.Text);
            startFromRow = Convert.ToInt16(StartFromRowT.Text);
            endAtRow = Convert.ToInt16(EndAtRowT.Text);
            if (startFrom != 0 && step != 0 && startFromRow != 0 && endAtRow != 0)
            {
                paraId = ids[ParaList.SelectedIndex];
                paraName = paraNames[ParaList.SelectedIndex];
                paraType = paraTypes[ParaList.SelectedIndex];
                paraOrder = ParaList.SelectedIndex;
                prefix = PrefixT.Text;
                suffix= SuffixT.Text;
                externalEvent.Raise();
            }
        }
    }
}
