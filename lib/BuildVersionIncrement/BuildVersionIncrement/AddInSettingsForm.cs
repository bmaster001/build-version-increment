using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Etier.IconHelper;
using System.Collections;
using System.Reflection;
using Qreed.Reflection;

namespace BuildVersionIncrement
{
    /// <summary>
    /// AddIn settings dialog form
    /// </summary>
    internal partial class AddInSettingsForm : Form
    {
        private class TreeNodeSort : IComparer
        {
            public int Compare(object x, object y)
            {
                SolutionItem xItem = (SolutionItem)((TreeNode)x).Tag;
                SolutionItem yItem = (SolutionItem)((TreeNode)y).Tag;

                if (xItem != null && yItem != null)
                {
                    if (xItem.ItemType == SolutionItemType.Folder)
                    {
                        if (yItem.ItemType == SolutionItemType.Folder)
                        {
                            return string.Compare(xItem.Name, yItem.Name);
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    else if (yItem.ItemType == SolutionItemType.Folder)
                    {
                        return 1;
                    }
                }
                else
                {
                    if (yItem == null) return 1;
                    if (yItem == null) return -1;
                }

                return string.Compare(xItem.Name, yItem.Name);
            }
        }

        private SolutionItem _solution;
        private GlobalIncrementSettings _globalSettings;
        private IconListManager _iconListManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddInSettings"/> class.
        /// </summary>
        public AddInSettingsForm()
        {
            InitializeComponent();

            Logger.WriteEvent += new EventHandler<Logger.WriteEventArgs>(Logger_WriteEvent);

            _iconListManager = new IconListManager(imageList, IconSize.Small);
            _globalSettings = new GlobalIncrementSettings();

			// Added this to implement fix the sorting of "Versioning Style.
			propertyGrid.PropertySort = PropertySort.Categorized;
			propertyGridGlobalSettings.PropertySort = PropertySort.Categorized;

            try
            {
                string version = ReflectionHelper.GetAssemblyAttribute<AssemblyFileVersionAttribute>(Assembly.GetExecutingAssembly()).Version;
                string config = ReflectionHelper.GetAssemblyAttribute<AssemblyConfigurationAttribute>(Assembly.GetExecutingAssembly()).Configuration;

                this.Text = string.Format("{0} v{1} [{2}]", this.Text, version, config);
            }
            catch
            {
                // swallow any exception
            }
        }

        private Connect _connect;
        /// <summary>
        /// Gets or sets the connect.
        /// </summary>
        /// <value>The connect.</value>
        public Connect Connect
        {
            get { return _connect; }
            set { _connect = value; }
        }

        /// <summary>
        /// Gets the selected increment settings.
        /// </summary>
        /// <value>The selected increment settings.</value>
        /// <remarks>
        /// This property will be <c>null</c> if there's no selected node or if the selection isn't 
        /// a project, a solution or global settings node.
        /// </remarks>
        public BaseIncrementSettings SelectedIncrementSettings
        {
            get
            {
                BaseIncrementSettings ret = null;

                if (solutionTreeView.SelectedNode != null)
                {
                    if (solutionTreeView.SelectedNode.Tag is GlobalIncrementSettings)
                        ret = solutionTreeView.SelectedNode.Tag as BaseIncrementSettings;
                    else if (solutionTreeView.SelectedNode.Tag is SolutionItem)
                    {
                        SolutionItem item = solutionTreeView.SelectedNode.Tag as SolutionItem;

                        if (item.ItemType == SolutionItemType.Project || item.ItemType == SolutionItemType.Solution)
                            ret = item.IncrementSettings;
                    }
                }

                return ret;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            // Calculate the center of our parent IDE and position this window
            EnvDTE.Window parentIDE = Connect.ApplicationObject.MainWindow;
            Int32 newX = parentIDE.Left + Convert.ToInt32(parentIDE.Width / 2) - Convert.ToInt32(this.Width / 2);
            Int32 newY = parentIDE.Top + Convert.ToInt32(parentIDE.Height / 2) - Convert.ToInt32(this.Height / 2);
            this.Location = new Point(newX, newY);
            
            try
            {
                _globalSettings.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occured while loading the global settings:\n" + ex.ToString(), 
                                "Global Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                _globalSettings = new GlobalIncrementSettings(); // Discard
            }

            BuildTree();

            propertyGridGlobalSettings.SelectedObject = _globalSettings;

            textBoxLog.AppendText(Logger.Instance.Contents);

            base.OnLoad(e);
        }

        void Logger_WriteEvent(object sender, Logger.WriteEventArgs e)
        {
            textBoxLog.AppendText(e.Message);
        }

        private void BuildTree()
        {
            try
            {
                _solution = new SolutionItem(Connect, Connect.ApplicationObject.Solution);

                TreeNode root = solutionTreeView.Nodes.Add(_solution.Name);
                root.SelectedImageIndex = root.ImageIndex = _iconListManager.AddFileIcon(_solution.Filename);
                root.Tag = _solution;

                BuildTree(root, _solution);

                root.Expand();
                solutionTreeView.SelectedNode = root;

                solutionTreeView.TreeViewNodeSorter = new TreeNodeSort();
                solutionTreeView.Sort();

                solutionTreeView.AfterCollapse += new TreeViewEventHandler(solutionTreeView_AfterCollapse);
                solutionTreeView.AfterExpand += new TreeViewEventHandler(solutionTreeView_AfterExpand);
                /*
                // Add the default settings node
                TreeNode globalNode = solutionTreeView.Nodes.Insert(0, "Global Settings", "Global Settings", 0, 0);
                globalNode.Tag = _globalSettings;
                globalNode.SelectedImageIndex = globalNode.ImageIndex = _iconListManager.AddFileIcon(".AddIn");*/
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occored while building solution tree.\n" + ex.ToString(), "Error");
            }
        }

        void solutionTreeView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            e.Node.ImageIndex = _iconListManager.AddFolderIcon(FolderType.Open);
        }

        void solutionTreeView_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            e.Node.ImageIndex = _iconListManager.AddFolderIcon(FolderType.Closed);
        }

        private void BuildTree(TreeNode currentNode, SolutionItem currentItem)
        {
            foreach (SolutionItem child in currentItem.SubItems)
            {
                TreeNode node = currentNode.Nodes.Add(child.Name);

                if (child.ItemType == SolutionItemType.Folder)
                {
                    node.ImageIndex = _iconListManager.AddFolderIcon(FolderType.Closed);
                    node.SelectedImageIndex = _iconListManager.AddFolderIcon(FolderType.Open);
                }
                else
                {
                    node.SelectedImageIndex = node.ImageIndex = _iconListManager.AddFileIcon(child.Filename);
                }

                node.Tag = child;

                BuildTree(node, child);
            }

            // Remove empty folders
            if (currentItem.ItemType == SolutionItemType.Folder && currentNode.Nodes.Count == 0)
                solutionTreeView.Nodes.Remove(currentNode);
        }

        private void solutionTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Name == "Global Settings")
            {
                propertyGrid.Enabled = true;
                propertyGrid.SelectedObject = e.Node.Tag;
            }
            else
            {
                SolutionItem solutionItem = (SolutionItem)e.Node.Tag;

                if ((solutionItem.ItemType == SolutionItemType.Project ||
                    solutionItem.ItemType == SolutionItemType.Solution) &&
                    solutionItem.Globals != null) // Not supported project type
                {
                    propertyGrid.Enabled = true;
                    propertyGrid.SelectedObject = solutionItem.IncrementSettings;
                }
                else
                {
                    propertyGrid.Enabled = false;
                    propertyGrid.SelectedObject = null;
                }
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (_globalSettings.Apply == GlobalIncrementSettings.ApplyGlobalSettings.Always)
                MessageBox.Show(Resources.GlobalMessage_alwaysApplyGlobalSettings, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            try
            {
                _globalSettings.Save();
            }
            catch (Exception ex)
            {
                string message = "Failed saving default settings:\n" + ex.ToString();
                Logger.Write(message, LogLevel.Error);
                MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try
            {
                GlobalAddinSettings.Default.Save();
            }
            catch (Exception ex)
            {
                string message = "Failed saving global settings:\n" + ex.ToString();
                Logger.Write(message, LogLevel.Error);
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try
            {
                if (_solution != null)
                    _solution.SetGlobalVariables();

                Close();
            }
            catch (Exception ex)
            {
                string message = "Failed storing global variables:\n" + ex.ToString();
                Logger.Write(message, LogLevel.Error);
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void contextMenu_Opening(object sender, CancelEventArgs e)
        {
            BaseIncrementSettings settings = SelectedIncrementSettings;

            if (settings == null)
            {
                e.Cancel = true;
            }

            // Only allow the copy to global settings if the current node isn't the global settings self
            
            copyToGlobalSettingsToolStripMenuItem.Enabled = settings is SolutionItemIncrementSettings;
        }

        private void copyToAllProjectsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BaseIncrementSettings settings = SelectedIncrementSettings;

            if (settings != null)
            {
                string name = "Global Settings";

                if (settings is SolutionItemIncrementSettings)
                    name = ((SolutionItemIncrementSettings)settings).Name;

                DialogResult r = MessageBox.Show(this, "Copy the increment settings of \"" + name + "\" to all other items?", "Copy to all", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (r == System.Windows.Forms.DialogResult.Yes)
                {
                    CopySettingsToAll(_solution, settings);
                }
            }
        }

        private static void CopySettingsToAll(SolutionItem item, BaseIncrementSettings settings)
        {
            if (item.ItemType == SolutionItemType.Solution || item.ItemType == SolutionItemType.Project)
            {
                Logger.Write("Copying IncrementSettings to \"" + item.Name + "\"", LogLevel.Debug);
                item.IncrementSettings.CopyFrom(settings);
            }
            
            foreach (SolutionItem child in item.SubItems)
                    CopySettingsToAll(child, settings);
        }

        private void copyToGlobalSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SolutionItemIncrementSettings settings = SelectedIncrementSettings as SolutionItemIncrementSettings;

            if (settings != null)
            {
                DialogResult r = MessageBox.Show(this, "Set the increment settings of \"" + settings.Name + "\" as global settings?", "Set as global settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (r == System.Windows.Forms.DialogResult.Yes)
                {
                    Logger.Write("Copying from \"" + solutionTreeView.SelectedNode.Text + "\" to global settings", LogLevel.Debug);

                    _globalSettings.CopyFrom(settings);
                }
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BaseIncrementSettings toReset = SelectedIncrementSettings;

            if (toReset != null)
            {
                DialogResult r = MessageBox.Show(this, "Reset the increment settings of \"" + solutionTreeView.SelectedNode.Text + "\" to the defaults?", "Reset settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (r == System.Windows.Forms.DialogResult.Yes)
                {
                    Logger.Write("Resetting increment settings  of \"" + solutionTreeView.SelectedNode.Text + "\"", LogLevel.Debug);

                    toReset.Reset();
                    propertyGrid.Refresh();
                }
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BaseIncrementSettings toReload = SelectedIncrementSettings;

            if (toReload != null)
            {
                DialogResult r = MessageBox.Show(this, "Discard changes to \"" + solutionTreeView.SelectedNode.Text + "\"?", "Undo changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (r == System.Windows.Forms.DialogResult.Yes)
                {
                    Logger.Write("Discard changes to \"" + solutionTreeView.SelectedNode.Text + "\"", LogLevel.Debug);

                    toReload.Load();
                    propertyGrid.Refresh();
                }
            }
        }

        private void solutionTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // Make sure that a right click also selects a node (otherwise the context menu will perform actions on the wrong node)

            solutionTreeView.SelectedNode = e.Node;
        }

        private void AddInSettingsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Logger.WriteEvent -= new EventHandler<Logger.WriteEventArgs>(Logger_WriteEvent);
        }

        private void textBoxLog_VisibleChanged(object sender, EventArgs e)
        {
            textBoxLog.Select(textBoxLog.Text.Length, 1);
            textBoxLog.ScrollToCaret();
        }
    }
}