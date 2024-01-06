using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using XPK_Explorer.FileManagement;
using XPK_Explorer.FileManagement.Loaders;

namespace XPK_Explorer
{
    public partial class Form1 : Form
    {
        private const string ROOT = "{root}";

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
                    CleanUp();
                    PopulateTreeView();
                    UpdateTitle();
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

        private void UpdateTitle()
        {
            Text = $"{folderBrowserDialog1.SelectedPath} | XPK Explorer";
        }

        private void CleanUp()
        {
            _archives?.Clear();
            treeView1.Nodes.Clear();
        }

        private void PopulateTreeView()
        {
            var path = folderBrowserDialog1.SelectedPath;
            var root = new TreeNode(ROOT);
            treeView1.Nodes.Add(root);
            treeView1.PathSeparator = "\\";

            var xpkFolderPath = Path.Combine(path, "XPK");
            var archives = Directory.GetFiles(xpkFolderPath).Select(Path.GetFileNameWithoutExtension);

            var archivePackages = new List<Archive>();

            foreach (var archiveFile in archives)
            {
                var archive = Archive.Open(path, archiveFile);

                var nodes = CreateNodesFromPathList(archive.FilePathEntries, archiveFile);
                root.Nodes.Add(nodes);

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
            var archiveName = GetArchiveName(e.Node.FullPath);
            var filePath = GetArchiveFilePath(e.Node.FullPath, archiveName);
            var extension = Path.GetExtension(filePath);

            // check if the file is selected
            if (string.IsNullOrEmpty(extension))
                return;

            var archive = _archives.FirstOrDefault(x => x.Name == archiveName);

            if (archive == null)
                return;

            var entry = archive.GetFileEntry(filePath);
            var entryBytes = archive.GetFileEntryBytes(entry);
            var bitmapLoader = new BitmapFileLoader();

            //var bitmap = bitmapLoader.Load(extension, entryBytes);
            //propertyGrid1.SelectedObject = new TextureFileDescription(filePath, entryBytes.LongLength, (uint)bitmap.Width, (uint)bitmap.Height);
            //pictureBox1.Image = bitmap;
        }
        private string GetArchiveName(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                throw new ArgumentNullException(nameof(fullPath), "Parameter cannot be empty!");
            var index = fullPath.IndexOf(ROOT, StringComparison.Ordinal) + ROOT.Length + 1;

            if (index >= fullPath.Length)
            {
                throw new Exception();
            }

            fullPath = fullPath.Substring(index);

            return fullPath.Split('\\').First();
        }

        private string GetArchiveFilePath(string fullPath, string archiveName)
        {
            if (string.IsNullOrEmpty(fullPath))
                throw new ArgumentNullException(nameof(fullPath), "Parameter cannot be empty!");

            var entries = new Queue<string>(fullPath.Split('\\'));
            var path = string.Empty;

            while (entries.Count > 0)
            {
                var entry = entries.Dequeue();

                if (string.Equals(entry, ROOT) || string.Equals(entry, archiveName))
                    continue;

                path = Path.Combine(path, entry);
            }

            return path;
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

            if (string.IsNullOrEmpty(path))
                return;

            var header = "XPK";

            var index = path.IndexOf(header, StringComparison.Ordinal) + header.Length + 1;

            if (index >= path.Length)
            {
                return;
            }

            path = path.Substring(index);

            var archiveName = path.Split('\\').First();

            index = path.IndexOf(archiveName, StringComparison.Ordinal) + archiveName.Length + 1;

            if (index >= path.Length)
            {
                return;
            }

            path = path.Substring(index);

            var archive = _archives.FirstOrDefault(x => string.Equals(x.Name, archiveName));

            if (archive == null)
            {
                MessageBox.Show($"Couldn't locate archive: {archiveName}");
                return;
            }

            var fileName = Path.GetFileName(path);

            if (string.IsNullOrEmpty(fileName) || !fileName.Contains('.'))
            {
                MessageBox.Show($"Exporting folders is currently unsupported");
                return;
            }

            var fileEntry = archive.GetFileEntry(path);
            var bytes = archive.GetFileEntryBytes(fileEntry);

            if (bytes.Length > 0)
            {
                saveFileDialog1.FileName = fileName;
                var saveDialog = saveFileDialog1.ShowDialog();

                if (saveDialog == DialogResult.OK)
                {
                    File.WriteAllBytes(saveFileDialog1.FileName, bytes);
                }
            }
        }
    }
}
