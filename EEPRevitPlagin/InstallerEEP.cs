using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using System.Net;

namespace EEPRevitPlagin
{
    [RunInstaller(true)]
    public partial class InstallerEEP : System.Configuration.Install.Installer
    {
        public InstallerEEP()
        {
            InitializeComponent();
        }

        string myAddinDLL = "EEPRevitPlagint";

        public override void Uninstall(System.Collections.IDictionary stateSaver)
        {
            string sDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Autodesk\\Revit\\Addins";
            bool exists = System.IO.Directory.Exists(sDir);

            if (exists)
            {
                try
                {
                    foreach (string d in Directory.GetDirectories(sDir))
                    {
                        //DirSearch.Add(d);
                        File.Delete(d + "\\" + myAddinDLL + ".addin");
                    }
                }
                catch (System.Exception excpt)
                {
                    System.Windows.Forms.MessageBox.Show(excpt.Message);
                }
            }
        }

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            string sDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Autodesk\\Revit\\Addins";
            bool exists = System.IO.Directory.Exists(sDir);

            if (!exists) System.IO.Directory.CreateDirectory(sDir);

            XElement XElementAddIn = new XElement("AddIn", new XAttribute("Type", "Application"));

            XElementAddIn.Add(new XElement("Name", myAddinDLL));
            XElementAddIn.Add(new XElement("Assembly", this.Context.Parameters["targetdir"].Trim() + myAddinDLL + ".dll"));
            XElementAddIn.Add(new XElement("AddInId", Guid.NewGuid().ToString()));
            XElementAddIn.Add(new XElement("FullClassName", myAddinDLL + ".ThisApplication"));
            XElementAddIn.Add(new XElement("VendorId", "EEP"));
            XElementAddIn.Add(new XElement("VendorDescription", "Pidlozhevich"));

            XElement XElementRevitAddIns = new XElement("RevitAddIns");
            XElementRevitAddIns.Add(XElementAddIn);

            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    new XDocument(XElementRevitAddIns).Save(d + "\\" + myAddinDLL + ".addin");
                }
            }
            catch (System.Exception excpt)
            {
                System.Windows.Forms.MessageBox.Show(excpt.Message);
            }
        }
    }
}
