namespace TestApp
{
    partial class FileBrowserTest
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
            this.folderViewPanel = new System.Windows.Forms.Panel();
            this.dualPane = new System.Windows.Forms.SplitContainer();
            this.showBrowserButton = new System.Windows.Forms.Button();
            this.fileBrowser1 = new FileBrowser.Browser();
            this.pluginWrapper = new FileBrowser.BrowserPluginWrapper();
            this.shellBrowser = new ShellDll.ShellBrowser();
            this.hideBrowserButton = new System.Windows.Forms.Button();
            this.fileBrowser2 = new FileBrowser.Browser();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.helpInfoLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.seperator1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.currentDirInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.folderViewPanel.SuspendLayout();
            this.dualPane.Panel1.SuspendLayout();
            this.dualPane.Panel2.SuspendLayout();
            this.dualPane.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // folderViewPanel
            // 
            this.folderViewPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.folderViewPanel.Controls.Add(this.dualPane);
            this.folderViewPanel.Location = new System.Drawing.Point(0, 0);
            this.folderViewPanel.Name = "folderViewPanel";
            this.folderViewPanel.Size = new System.Drawing.Size(613, 487);
            this.folderViewPanel.TabIndex = 0;
            // 
            // dualPane
            // 
            this.dualPane.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dualPane.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.dualPane.Location = new System.Drawing.Point(0, 0);
            this.dualPane.Name = "dualPane";
            // 
            // dualPane.Panel1
            // 
            this.dualPane.Panel1.Controls.Add(this.showBrowserButton);
            this.dualPane.Panel1.Controls.Add(this.fileBrowser1);
            // 
            // dualPane.Panel2
            // 
            this.dualPane.Panel2.Controls.Add(this.hideBrowserButton);
            this.dualPane.Panel2.Controls.Add(this.fileBrowser2);
            this.dualPane.Size = new System.Drawing.Size(613, 487);
            this.dualPane.SplitterDistance = 340;
            this.dualPane.SplitterWidth = 10;
            this.dualPane.TabIndex = 1;
            this.dualPane.MouseDown += new System.Windows.Forms.MouseEventHandler(this.splitter_MouseDown);
            this.dualPane.MouseMove += new System.Windows.Forms.MouseEventHandler(this.splitter_MouseMove);
            this.dualPane.MouseUp += new System.Windows.Forms.MouseEventHandler(this.splitter_MouseUp);
            // 
            // showBrowserButton
            // 
            this.showBrowserButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.showBrowserButton.Image = global::TestApp.Properties.Resources.BrowseFolders;
            this.showBrowserButton.Location = new System.Drawing.Point(305, 452);
            this.showBrowserButton.Name = "showBrowserButton";
            this.showBrowserButton.Size = new System.Drawing.Size(32, 32);
            this.showBrowserButton.TabIndex = 1;
            this.showBrowserButton.UseVisualStyleBackColor = true;
            this.showBrowserButton.Visible = false;
            this.showBrowserButton.Click += new System.EventHandler(this.showBrowserButton_Click);
            // 
            // fileBrowser1
            // 
            this.fileBrowser1.AllowDrop = true;
            this.fileBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileBrowser1.Location = new System.Drawing.Point(0, 0);
            this.fileBrowser1.MinimumSize = new System.Drawing.Size(300, 200);
            this.fileBrowser1.Name = "fileBrowser1";
            this.fileBrowser1.PluginWrapper = this.pluginWrapper;            
            this.fileBrowser1.ShellBrowser = this.shellBrowser;
            this.fileBrowser1.Size = new System.Drawing.Size(340, 487);
            this.fileBrowser1.SplitterDistance = 162;
            this.fileBrowser1.TabIndex = 0;
            this.fileBrowser1.ContextMenuMouseHover += new FileBrowser.ContextMenuMouseHoverEventHandler(this.fileBrowser_ContextMenuMouseHover);
            this.fileBrowser1.SelectedFolderChanged += new FileBrowser.SelectedFolderChangedEventHandler(this.fileBrowser_SelectedFolderChanged);
            // 
            // hideBrowserButton
            // 
            this.hideBrowserButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.hideBrowserButton.Image = global::TestApp.Properties.Resources.BrowseFolders;
            this.hideBrowserButton.Location = new System.Drawing.Point(228, 452);
            this.hideBrowserButton.Name = "hideBrowserButton";
            this.hideBrowserButton.Size = new System.Drawing.Size(32, 32);
            this.hideBrowserButton.TabIndex = 2;
            this.hideBrowserButton.UseVisualStyleBackColor = true;
            this.hideBrowserButton.Click += new System.EventHandler(this.hideBrowserButton_Click);
            // 
            // fileBrowser2
            // 
            this.fileBrowser2.AllowDrop = true;
            this.fileBrowser2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileBrowser2.Location = new System.Drawing.Point(0, 0);
            this.fileBrowser2.Name = "fileBrowser2";
            this.fileBrowser2.PluginWrapper = this.pluginWrapper;            
            this.fileBrowser2.ShellBrowser = this.shellBrowser;
            this.fileBrowser2.ShowFolders = false;
            this.fileBrowser2.Size = new System.Drawing.Size(263, 487);
            this.fileBrowser2.SplitterDistance = 162;
            this.fileBrowser2.StartUpDirectory = FileBrowser.SpecialFolders.MyDocuments;
            this.fileBrowser2.TabIndex = 0;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpInfoLabel,
            this.seperator1,
            this.currentDirInfo});
            this.statusStrip.Location = new System.Drawing.Point(0, 490);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(613, 22);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // helpInfoLabel
            // 
            this.helpInfoLabel.Name = "helpInfoLabel";
            this.helpInfoLabel.Size = new System.Drawing.Size(594, 17);
            this.helpInfoLabel.Spring = true;
            this.helpInfoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // seperator1
            // 
            this.seperator1.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.seperator1.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
            this.seperator1.Name = "seperator1";
            this.seperator1.Size = new System.Drawing.Size(4, 17);
            // 
            // currentDirInfo
            // 
            this.currentDirInfo.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.currentDirInfo.Name = "currentDirInfo";
            this.currentDirInfo.Size = new System.Drawing.Size(0, 17);
            this.currentDirInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FileBrowserTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(613, 512);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.folderViewPanel);
            this.Name = "FileBrowserTest";
            this.Text = "FileBrowser Test";
            this.folderViewPanel.ResumeLayout(false);
            this.dualPane.Panel1.ResumeLayout(false);
            this.dualPane.Panel2.ResumeLayout(false);
            this.dualPane.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel folderViewPanel;
        private FileBrowser.Browser fileBrowser1;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel helpInfoLabel;
        private System.Windows.Forms.ToolStripStatusLabel currentDirInfo;
        private System.Windows.Forms.ToolStripStatusLabel seperator1;
        private System.Windows.Forms.SplitContainer dualPane;
        private ShellDll.ShellBrowser shellBrowser;
        private FileBrowser.Browser fileBrowser2;
        private System.Windows.Forms.Button showBrowserButton;
        private System.Windows.Forms.Button hideBrowserButton;
        private FileBrowser.BrowserPluginWrapper pluginWrapper;
    }
}

