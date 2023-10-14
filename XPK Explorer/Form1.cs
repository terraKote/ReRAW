using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace XPK_Explorer
{
    public partial class Form1 : Form
    {
        private TreeNode _selectedNode;
        private MenuItem _menuItem;
        private ContextMenu _contextMenu;

        private List<Archive> _archives;

        public Form1()
        {
            _selectedNode = null;
            _menuItem = new MenuItem("Export");
            _contextMenu = new ContextMenu();

            _contextMenu.MenuItems.Add(_menuItem);
            _menuItem.Click += OnContextMenuClick;

            InitializeComponent();
        }

        private void OpenFolder(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();

            switch (result)
            {
                case DialogResult.None:
                    break;
                case DialogResult.OK:
                    PopulateTreeView();
                    break;
                case DialogResult.Cancel:
                    break;
                case DialogResult.Abort:
                    break;
                case DialogResult.Retry:
                    break;
                case DialogResult.Ignore:
                    break;
                case DialogResult.Yes:
                    break;
                case DialogResult.No:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PopulateTreeView()
        {
            var path = folderBrowserDialog1.SelectedPath;
            var root = new TreeNode(path);
            treeView1.Nodes.Add(root);
            treeView1.PathSeparator = "\\";

            var xpkFolder = new TreeNode("XPK");
            root.Nodes.Add(xpkFolder);

            var xpkFolderPath = Path.Combine(path, "XPK");
            var archives = Directory.GetFiles(xpkFolderPath).Select(Path.GetFileNameWithoutExtension);

            var archivePackages = new List<Archive>();
            var allowedArchives = new List<string>()
            {
                "TF"
            };

            foreach (var archiveFile in archives)
            {
                if (!allowedArchives.Contains(archiveFile))
                    continue;

                var archive = Archive.Open(path, archiveFile);

                var nodes = CreateNodesFromPathList(archive.FilePathEntries, archiveFile);
                xpkFolder.Nodes.Add(nodes);

                archivePackages.Add(archive);
            }

            _archives = archivePackages;
        }

        private TreeNode CreateNodesFromPathList(IEnumerable<string> filePathEntries, string rootNodeName)
        {
            var root = new TreeNode(rootNodeName);

            foreach (var path in filePathEntries.Where(x => !string.IsNullOrEmpty(x.Trim())))
            {
                var currentNode = root;
                var pathItems = path.Split('\\');

                foreach (var item in pathItems)
                {
                    var buffer = currentNode.Nodes.Cast<TreeNode>().Where(x => string.Equals(x.Text, item)).ToArray();
                    currentNode = buffer.Any() ? buffer.Single() : currentNode.Nodes.Add(item);
                }
            }

            return root;
        }

        private void OnNodeSelected(object sender, TreeNodeMouseClickEventArgs e)
        {
        }

        private void ExportSelectedItem(object sender, EventArgs e)
        {

        }

        private void OnNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            _selectedNode = e.Node;
            _contextMenu.Show(treeView1, e.Location);
        }

        private void OnContextMenuClick(object sender, EventArgs e)
        {
        }
    }
}
