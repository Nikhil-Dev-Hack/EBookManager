using eBookManager.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static eBookManager.Helper.ExtensionMethods;
using static System.Math;

namespace eBookManager
{
    public partial class ImportBooks : Form
    {
        private string _jsonPath;
        private List<StorageSpace> _spaces;
        private enum _storageSpaceSelection
        {
            New = -9999,
            NoSelection = -1
        }
        private enum Extension
        {
            doc = 0,
            docx = 1,
            pdf = 2,
            epub = 3,
            lit = 4
        }
        private HashSet<string> AllowedExtensions => new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            ".doc",".docx",".pdf",".epub", ".lit"
        };
        public ImportBooks()
        {
            InitializeComponent();
            _jsonPath = Path.Combine(Application.StartupPath, "bookData.txt");
        }

        private async void ImportBooks_Load(object sender, EventArgs e)
        {
            _spaces = await _spaces.ReadFromDataStore(_jsonPath);
        }

        public void PopulateBookList(string paramDir, TreeNode paramNode)
        {
            DirectoryInfo dir = new DirectoryInfo(paramDir);
            foreach (DirectoryInfo dirInfo in dir.GetDirectories())
            {
                TreeNode node = new TreeNode(dir.Name);
                node.ImageIndex = 4;
                node.SelectedImageIndex = 5;
                if (paramNode != null)
                    paramNode.Nodes.Add(node);
                else
                    tvFoundBooks.Nodes.Add(node);
                PopulateBookList(dirInfo.FullName, node);
            }
            foreach (FileInfo fleInfo in dir.GetFiles().Where(x => AllowedExtensions.Contains(x.Extension)).ToList())
            {
                TreeNode node = new TreeNode(fleInfo.Name);
                node.Tag = fleInfo.FullName;
                int iconIndex = Enum.Parse(typeof(Extension), fleInfo.Extension.TrimStart('.'), true).GetHashCode();
                node.ImageIndex = iconIndex;
                node.SelectedImageIndex = iconIndex;
                if (paramNode != null)
                    paramNode.Nodes.Add(node);
                else
                    tvFoundBooks.Nodes.Add(node);
            }
        }
        private void btnSelectSourceFolder_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Select the location of your eBook and documents";
                DialogResult dlgResult = fbd.ShowDialog();
                if (dlgResult == DialogResult.OK)
                {
                    tvFoundBooks.Nodes.Clear();
                    string path = fbd.SelectedPath;
                    DirectoryInfo di = new DirectoryInfo(path);
                    TreeNode root = new TreeNode(di.Name);
                    root.ImageIndex = 4;
                    root.SelectedImageIndex = 5;
                    tvFoundBooks.Nodes.Add(root);
                    PopulateBookList(di.FullName, root);
                    tvFoundBooks.Sort();
                    root.Expand();
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        private void tvFoundBooks_AfterSelect(object sender, TreeViewCancelEventArgs e)
        {
            DocumentEngine engine = new DocumentEngine();
            string path = e.Node.Tag?.ToString() ?? "";
            if (File.Exists(path))
            {
                var (dateCreated, dateLastAccessed, fileName, fileExtention, fileLength, hasError) = engine.GetFileProperties(e.Node.Tag.ToString());
                if (!hasError)
                {
                    txtFileName.Text = fileName;
                    txtExtension.Text = fileExtention;
                    dtCreated.Value = dateCreated;
                    dtLastAccessed.Value = dateLastAccessed;
                    txtFilePath.Text = e.Node.Tag.ToString();
                    txtFileSize.Text = $"{Round(fileLength.ToMegabytes(), 2).ToString()} MB";

                }
            }
        }
        
    }
}