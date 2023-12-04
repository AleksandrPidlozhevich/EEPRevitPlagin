using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows.Forms;



namespace EEPRevitPlagin.SecondaryСommand
{
    internal class RevitServerTools
    {
        public static TreeView GetFoldersAndFiles(string sreverNameOrIP, String revitVersion)
        {
            //Example:
            //TreeNode treeNode = EEPRevitPlagin.SecondaryСommand.RevitServerTools.GetFoldersAndFiles("192.168.1.10", "2021").Nodes[0];
            //treeView1.Nodes.Add((TreeNode)treeNode.Clone());


            try
            {
                TreeView treeView = new TreeView();
                TreeNode root = treeView.Nodes.Add("Server");
                AddContents(root, "/|", sreverNameOrIP, revitVersion);
                return treeView;
            }
            catch
            {
                return null;
            }
        }
        private static void AddContents(TreeNode parentNode, string path, string sreverNameOrIP, string revitVersion)
        {
            XmlDictionaryReader reader = GetResponse(path + "/contents", sreverNameOrIP, revitVersion);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Folders")
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Folders")
                            break;
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "Name")
                        {
                            reader.Read();
                            string content = reader.ReadContentAsString();
                            TreeNode node = parentNode.Nodes.Add(content);
                            AddContents(node, path + "|" + content, sreverNameOrIP, revitVersion);
                        }
                    }
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "Models")
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Models")
                            break;
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "Name")
                        {
                            reader.Read();
                            TreeNode node = parentNode.Nodes.Add(reader.Value);
                        }
                    }
                }
            }
            reader.Close();
        }
        private static XmlDictionaryReader GetResponse(string info, string sreverNameOrIP, string revitVersion)
        {
            WebRequest request = WebRequest.Create("http://" + sreverNameOrIP + string.Format("/RevitServerAdminRESTService{0}/AdminRESTService.svc", revitVersion) + info);
            request.Method = "GET";
            request.Headers.Add("User-Name", Environment.UserName);
            request.Headers.Add("User-Machine-Name", Environment.MachineName);
            request.Headers.Add("Operation-GUID", Guid.NewGuid().ToString());
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            XmlDictionaryReader jsonReader = JsonReaderWriterFactory.CreateJsonReader(request.GetResponse().GetResponseStream(), quotas);
            return jsonReader;
        }
        public static void GetSpecificFolder(TreeNode parentNode, string path, string sreverNameOrIP, string revitVersion)
        {

            //Example:
            //TreeNode treeNode1 = new TreeNode();
            //EEPRevitPlagin.SecondaryСommand.RevitServerTools.GetSpecificFolder(treeNode1, "/|21.16 НДЦ|", "192.168.1.10", "2021");
            //treeView1.Nodes.Add((TreeNode)treeNode1.Clone());



            if (path == "/|")
            {
                parentNode.Text = "Server";

            }
            else
            {
                parentNode.Text = path.Replace('|', '/');
            }
            XmlDictionaryReader reader = GetResponse(path + "/contents", sreverNameOrIP, revitVersion);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Folders")
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Folders")
                            break;
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "Name")
                        {
                            reader.Read();
                            string content = reader.ReadContentAsString();
                            TreeNode node = parentNode.Nodes.Add(content);
                            AddContents(node, path + "|" + content, sreverNameOrIP, revitVersion);
                        }
                    }
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "Models")
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Models")
                            break;
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "Name")
                        {
                            reader.Read();
                            TreeNode node = parentNode.Nodes.Add(reader.Value);
                        }
                    }
                }
            }
            reader.Close();
        }
    }
}
