namespace BuildVersionIncrement
{
    partial class AddInSettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			this.solutionTreeView = new System.Windows.Forms.TreeView();
			this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.copyToAllProjectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.copyToGlobalSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.propertyGrid = new System.Windows.Forms.PropertyGrid();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPageSolution = new System.Windows.Forms.TabPage();
			this.tabPageGlobalSettings = new System.Windows.Forms.TabPage();
			this.propertyGridGlobalSettings = new System.Windows.Forms.PropertyGrid();
			this.tabPageLog = new System.Windows.Forms.TabPage();
			this.textBoxLog = new System.Windows.Forms.TextBox();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOk = new System.Windows.Forms.Button();
			this.chkVersionIncrementEnabled = new System.Windows.Forms.CheckBox();
			this.chkVerboseLogEnabled = new System.Windows.Forms.CheckBox();
			this.contextMenu.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPageSolution.SuspendLayout();
			this.tabPageGlobalSettings.SuspendLayout();
			this.tabPageLog.SuspendLayout();
			this.SuspendLayout();
			// 
			// solutionTreeView
			// 
			this.solutionTreeView.ContextMenuStrip = this.contextMenu;
			this.solutionTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.solutionTreeView.HideSelection = false;
			this.solutionTreeView.ImageIndex = 0;
			this.solutionTreeView.ImageList = this.imageList;
			this.solutionTreeView.Indent = 20;
			this.solutionTreeView.ItemHeight = 18;
			this.solutionTreeView.Location = new System.Drawing.Point(0, 0);
			this.solutionTreeView.Name = "solutionTreeView";
			this.solutionTreeView.SelectedImageIndex = 0;
			this.solutionTreeView.Size = new System.Drawing.Size(255, 355);
			this.solutionTreeView.TabIndex = 0;
			this.solutionTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.solutionTreeView_AfterSelect);
			this.solutionTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.solutionTreeView_NodeMouseClick);
			// 
			// contextMenu
			// 
			this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToAllProjectsToolStripMenuItem,
            this.copyToGlobalSettingsToolStripMenuItem,
            this.toolStripSeparator1,
            this.clearToolStripMenuItem,
            this.undoToolStripMenuItem});
			this.contextMenu.Name = "contextMenu";
			this.contextMenu.Size = new System.Drawing.Size(187, 98);
			this.contextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
			// 
			// copyToAllProjectsToolStripMenuItem
			// 
			this.copyToAllProjectsToolStripMenuItem.Name = "copyToAllProjectsToolStripMenuItem";
			this.copyToAllProjectsToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.copyToAllProjectsToolStripMenuItem.Text = "Copy to all projects";
			this.copyToAllProjectsToolStripMenuItem.ToolTipText = "Copies the current settings to all projects";
			this.copyToAllProjectsToolStripMenuItem.Click += new System.EventHandler(this.copyToAllProjectsToolStripMenuItem_Click);
			// 
			// copyToGlobalSettingsToolStripMenuItem
			// 
			this.copyToGlobalSettingsToolStripMenuItem.Name = "copyToGlobalSettingsToolStripMenuItem";
			this.copyToGlobalSettingsToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.copyToGlobalSettingsToolStripMenuItem.Text = "Set as Global Settings";
			this.copyToGlobalSettingsToolStripMenuItem.ToolTipText = "Copies the current settings to the Global Settings";
			this.copyToGlobalSettingsToolStripMenuItem.Click += new System.EventHandler(this.copyToGlobalSettingsToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(183, 6);
			// 
			// clearToolStripMenuItem
			// 
			this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
			this.clearToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.clearToolStripMenuItem.Text = "Reset to defaults";
			this.clearToolStripMenuItem.ToolTipText = "Resets the current settings to the defaults";
			this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
			// 
			// undoToolStripMenuItem
			// 
			this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
			this.undoToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.undoToolStripMenuItem.Text = "Undo";
			this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
			// 
			// imageList
			// 
			this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageList.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(3, 3);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.solutionTreeView);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.propertyGrid);
			this.splitContainer1.Size = new System.Drawing.Size(617, 355);
			this.splitContainer1.SplitterDistance = 255;
			this.splitContainer1.TabIndex = 1;
			// 
			// propertyGrid
			// 
			this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyGrid.Enabled = false;
			this.propertyGrid.Location = new System.Drawing.Point(0, 0);
			this.propertyGrid.Name = "propertyGrid";
			this.propertyGrid.Size = new System.Drawing.Size(358, 355);
			this.propertyGrid.TabIndex = 0;
			this.propertyGrid.ToolbarVisible = false;
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl1.Controls.Add(this.tabPageSolution);
			this.tabControl1.Controls.Add(this.tabPageGlobalSettings);
			this.tabControl1.Controls.Add(this.tabPageLog);
			this.tabControl1.Location = new System.Drawing.Point(12, 12);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(631, 387);
			this.tabControl1.TabIndex = 1;
			// 
			// tabPageSolution
			// 
			this.tabPageSolution.Controls.Add(this.splitContainer1);
			this.tabPageSolution.Location = new System.Drawing.Point(4, 22);
			this.tabPageSolution.Name = "tabPageSolution";
			this.tabPageSolution.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageSolution.Size = new System.Drawing.Size(623, 361);
			this.tabPageSolution.TabIndex = 0;
			this.tabPageSolution.Text = "Solution";
			this.tabPageSolution.UseVisualStyleBackColor = true;
			// 
			// tabPageGlobalSettings
			// 
			this.tabPageGlobalSettings.Controls.Add(this.propertyGridGlobalSettings);
			this.tabPageGlobalSettings.Location = new System.Drawing.Point(4, 22);
			this.tabPageGlobalSettings.Name = "tabPageGlobalSettings";
			this.tabPageGlobalSettings.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageGlobalSettings.Size = new System.Drawing.Size(623, 361);
			this.tabPageGlobalSettings.TabIndex = 1;
			this.tabPageGlobalSettings.Text = "Global Settings";
			this.tabPageGlobalSettings.UseVisualStyleBackColor = true;
			// 
			// propertyGridGlobalSettings
			// 
			this.propertyGridGlobalSettings.Dock = System.Windows.Forms.DockStyle.Fill;
			this.propertyGridGlobalSettings.Location = new System.Drawing.Point(3, 3);
			this.propertyGridGlobalSettings.Name = "propertyGridGlobalSettings";
			this.propertyGridGlobalSettings.Size = new System.Drawing.Size(617, 355);
			this.propertyGridGlobalSettings.TabIndex = 1;
			this.propertyGridGlobalSettings.ToolbarVisible = false;
			// 
			// tabPageLog
			// 
			this.tabPageLog.Controls.Add(this.textBoxLog);
			this.tabPageLog.Location = new System.Drawing.Point(4, 22);
			this.tabPageLog.Name = "tabPageLog";
			this.tabPageLog.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageLog.Size = new System.Drawing.Size(623, 361);
			this.tabPageLog.TabIndex = 2;
			this.tabPageLog.Text = "Log";
			this.tabPageLog.UseVisualStyleBackColor = true;
			// 
			// textBoxLog
			// 
			this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxLog.BackColor = System.Drawing.SystemColors.Window;
			this.textBoxLog.Location = new System.Drawing.Point(6, 6);
			this.textBoxLog.Multiline = true;
			this.textBoxLog.Name = "textBoxLog";
			this.textBoxLog.ReadOnly = true;
			this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxLog.Size = new System.Drawing.Size(611, 349);
			this.textBoxLog.TabIndex = 0;
			this.textBoxLog.VisibleChanged += new System.EventHandler(this.textBoxLog_VisibleChanged);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(568, 405);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 1;
			this.buttonCancel.Text = "&Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOk
			// 
			this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOk.Location = new System.Drawing.Point(487, 405);
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.Size = new System.Drawing.Size(75, 23);
			this.buttonOk.TabIndex = 0;
			this.buttonOk.Text = "&Ok";
			this.buttonOk.UseVisualStyleBackColor = true;
			this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
			// 
			// chkVersionIncrementEnabled
			// 
			this.chkVersionIncrementEnabled.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.chkVersionIncrementEnabled.AutoSize = true;
			this.chkVersionIncrementEnabled.Checked = global::BuildVersionIncrement.GlobalAddinSettings.Default.IsEnabled;
			this.chkVersionIncrementEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkVersionIncrementEnabled.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::BuildVersionIncrement.GlobalAddinSettings.Default, "IsEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.chkVersionIncrementEnabled.Location = new System.Drawing.Point(12, 410);
			this.chkVersionIncrementEnabled.Name = "chkVersionIncrementEnabled";
			this.chkVersionIncrementEnabled.Size = new System.Drawing.Size(153, 17);
			this.chkVersionIncrementEnabled.TabIndex = 2;
			this.chkVersionIncrementEnabled.Text = "Version Increment Enabled";
			this.chkVersionIncrementEnabled.UseVisualStyleBackColor = true;
			// 
			// chkVerboseLogEnabled
			// 
			this.chkVerboseLogEnabled.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.chkVerboseLogEnabled.AutoSize = true;
			this.chkVerboseLogEnabled.Checked = global::BuildVersionIncrement.GlobalAddinSettings.Default.IsVerboseLogEnabled;
			this.chkVerboseLogEnabled.CheckState = System.Windows.Forms.CheckState.Unchecked;
			this.chkVerboseLogEnabled.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::BuildVersionIncrement.GlobalAddinSettings.Default, "IsVerboseLogEnabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.chkVerboseLogEnabled.Location = new System.Drawing.Point(171, 410);
			this.chkVerboseLogEnabled.Name = "chkVerboseLogEnabled";
			this.chkVerboseLogEnabled.Size = new System.Drawing.Size(139, 17);
			this.chkVerboseLogEnabled.TabIndex = 3;
			this.chkVerboseLogEnabled.Text = "Is Verbose Log Enabled";
			this.chkVerboseLogEnabled.UseVisualStyleBackColor = true;
			// 
			// AddInSettingsForm
			// 
			this.AcceptButton = this.buttonOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(655, 440);
			this.Controls.Add(this.chkVerboseLogEnabled);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.chkVersionIncrementEnabled);
			this.Controls.Add(this.buttonOk);
			this.Controls.Add(this.buttonCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "AddInSettingsForm";
			this.ShowIcon = false;
			this.Text = "Build Version Increment Settings";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AddInSettingsForm_FormClosed);
			this.contextMenu.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.tabPageSolution.ResumeLayout(false);
			this.tabPageGlobalSettings.ResumeLayout(false);
			this.tabPageLog.ResumeLayout(false);
			this.tabPageLog.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView solutionTreeView;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.CheckBox chkVersionIncrementEnabled;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem copyToAllProjectsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToGlobalSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageSolution;
        private System.Windows.Forms.TabPage tabPageGlobalSettings;
        private System.Windows.Forms.PropertyGrid propertyGridGlobalSettings;
        private System.Windows.Forms.TabPage tabPageLog;
        private System.Windows.Forms.TextBox textBoxLog;
		private System.Windows.Forms.CheckBox chkVerboseLogEnabled;
    }
}