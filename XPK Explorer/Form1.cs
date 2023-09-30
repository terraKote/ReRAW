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

            var xpkFolder = new TreeNode("XPK");
            root.Nodes.Add(xpkFolder);

            var xpkFolderPath = Path.Combine(path, "XPK");
            var archives = Directory.GetFiles(xpkFolderPath).Select(Path.GetFileNameWithoutExtension);

            var archivePackages = new List<Archive>();

            foreach (var archiveFile in archives)
            {
                var archive = Archive.Open(path, archiveFile);

                var tfArchive = new TreeNode(archiveFile);
                xpkFolder.Nodes.Add(tfArchive);

                foreach (var entry in archive.Entries)
                {
                    var node = new TreeNode(entry.Path);

                    if (entry.GetType() == typeof(ArchiveFolderEntry))
                    {
                        GeneratePath(entry as ArchiveFolderEntry, node);
                    }
                    tfArchive.Nodes.Add(node);
                }

                archivePackages.Add(archive);
            }

            _archives = archivePackages;
        }

        private static void GeneratePath(ArchiveFolderEntry entryParent, TreeNode parentNode)
        {
            foreach (var child in entryParent.ChildEntries)
            {
                var node = new TreeNode(child.Path);
                parentNode.Nodes.Add(node);

                if (child.GetType() == typeof(ArchiveFolderEntry))
                {
                    GeneratePath(child as ArchiveFolderEntry, node);
                }
            }
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
            var path = _selectedNode.FullPath;
            var index = path.IndexOf("XPK", StringComparison.Ordinal) + 4;
            path = path.Substring(index, path.Length - index);

            var archiveName = path.Split('\\').First();
            var archive = _archives.FirstOrDefault(x => string.Equals(x.Name, archiveName));

            if (archive != null)
            {
                var bytes = archive.GetFileBytes(path);

                if (bytes.Length > 0)
                {
                    var r = saveFileDialog1.ShowDialog();
                    if (r == DialogResult.OK)
                    {
                        File.WriteAllBytes(saveFileDialog1.FileName, bytes);
                    }
                }
            }
        }
    }
}
