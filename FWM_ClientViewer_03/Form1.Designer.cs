//Firewall Monitor v04
//(c) 2026 - TimeWarpToaster

//https://www.gnu.org/licenses/gpl-3.0.html

namespace FWM_ClientViewer_03
{
    partial class Form1
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tabsMain = new System.Windows.Forms.TabControl();
            this.tabFirewallRules = new System.Windows.Forms.TabPage();
            this.lvFirewallRules = new System.Windows.Forms.ListView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnExpireFirewallRules = new System.Windows.Forms.Button();
            this.btnFirewallRefresh = new System.Windows.Forms.Button();
            this.tabDataView = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeDataView = new System.Windows.Forms.TreeView();
            this.lvTreeDataRows = new System.Windows.Forms.ListView();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnDataExportCSV = new System.Windows.Forms.Button();
            this.btnDataPageHigher = new System.Windows.Forms.Button();
            this.btnDataPageLower = new System.Windows.Forms.Button();
            this.lblDataOfPages = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblDataPageNumber = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnQuery = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dtpQueryEnd = new System.Windows.Forms.DateTimePicker();
            this.dtpQueryStart = new System.Windows.Forms.DateTimePicker();
            this.tabSettings = new System.Windows.Forms.TabPage();
            this.tabLogs = new System.Windows.Forms.TabPage();
            this.lbLogsOut = new System.Windows.Forms.ListBox();
            this.gbLogOptions = new System.Windows.Forms.GroupBox();
            this.btnClearLogs = new System.Windows.Forms.Button();
            this.saveFileDialogData = new System.Windows.Forms.SaveFileDialog();
            this.menuStrip1.SuspendLayout();
            this.tabsMain.SuspendLayout();
            this.tabFirewallRules.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabDataView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabLogs.SuspendLayout();
            this.gbLogOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1529, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(116, 26);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Location = new System.Drawing.Point(0, 769);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1529, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tabsMain
            // 
            this.tabsMain.Controls.Add(this.tabFirewallRules);
            this.tabsMain.Controls.Add(this.tabDataView);
            this.tabsMain.Controls.Add(this.tabSettings);
            this.tabsMain.Controls.Add(this.tabLogs);
            this.tabsMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabsMain.Location = new System.Drawing.Point(0, 28);
            this.tabsMain.Name = "tabsMain";
            this.tabsMain.SelectedIndex = 0;
            this.tabsMain.Size = new System.Drawing.Size(1529, 741);
            this.tabsMain.TabIndex = 2;
            // 
            // tabFirewallRules
            // 
            this.tabFirewallRules.Controls.Add(this.lvFirewallRules);
            this.tabFirewallRules.Controls.Add(this.groupBox1);
            this.tabFirewallRules.Location = new System.Drawing.Point(4, 25);
            this.tabFirewallRules.Name = "tabFirewallRules";
            this.tabFirewallRules.Padding = new System.Windows.Forms.Padding(3);
            this.tabFirewallRules.Size = new System.Drawing.Size(1521, 712);
            this.tabFirewallRules.TabIndex = 0;
            this.tabFirewallRules.Text = "Firewall Rules";
            this.tabFirewallRules.UseVisualStyleBackColor = true;
            // 
            // lvFirewallRules
            // 
            this.lvFirewallRules.AllowColumnReorder = true;
            this.lvFirewallRules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvFirewallRules.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvFirewallRules.HideSelection = false;
            this.lvFirewallRules.Location = new System.Drawing.Point(3, 103);
            this.lvFirewallRules.Name = "lvFirewallRules";
            this.lvFirewallRules.Size = new System.Drawing.Size(1515, 606);
            this.lvFirewallRules.TabIndex = 2;
            this.lvFirewallRules.UseCompatibleStateImageBehavior = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnExpireFirewallRules);
            this.groupBox1.Controls.Add(this.btnFirewallRefresh);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1515, 100);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Firewall Options:  ";
            // 
            // btnExpireFirewallRules
            // 
            this.btnExpireFirewallRules.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExpireFirewallRules.Location = new System.Drawing.Point(1359, 35);
            this.btnExpireFirewallRules.Name = "btnExpireFirewallRules";
            this.btnExpireFirewallRules.Size = new System.Drawing.Size(150, 33);
            this.btnExpireFirewallRules.TabIndex = 1;
            this.btnExpireFirewallRules.Text = "Deactivate Selected";
            this.btnExpireFirewallRules.UseVisualStyleBackColor = true;
            this.btnExpireFirewallRules.Click += new System.EventHandler(this.btnFirewallExpireRules_Click);
            // 
            // btnFirewallRefresh
            // 
            this.btnFirewallRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFirewallRefresh.Location = new System.Drawing.Point(1146, 35);
            this.btnFirewallRefresh.Name = "btnFirewallRefresh";
            this.btnFirewallRefresh.Size = new System.Drawing.Size(120, 33);
            this.btnFirewallRefresh.TabIndex = 0;
            this.btnFirewallRefresh.Text = "Refresh";
            this.btnFirewallRefresh.UseVisualStyleBackColor = true;
            this.btnFirewallRefresh.Click += new System.EventHandler(this.btnFirewallRefresh_Click);
            // 
            // tabDataView
            // 
            this.tabDataView.Controls.Add(this.splitContainer1);
            this.tabDataView.Location = new System.Drawing.Point(4, 25);
            this.tabDataView.Name = "tabDataView";
            this.tabDataView.Padding = new System.Windows.Forms.Padding(3);
            this.tabDataView.Size = new System.Drawing.Size(1521, 712);
            this.tabDataView.TabIndex = 6;
            this.tabDataView.Text = "Data View";
            this.tabDataView.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeDataView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lvTreeDataRows);
            this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer1.Size = new System.Drawing.Size(1515, 706);
            this.splitContainer1.SplitterDistance = 262;
            this.splitContainer1.TabIndex = 0;
            // 
            // treeDataView
            // 
            this.treeDataView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeDataView.Location = new System.Drawing.Point(0, 0);
            this.treeDataView.Name = "treeDataView";
            this.treeDataView.Size = new System.Drawing.Size(262, 706);
            this.treeDataView.TabIndex = 0;
            this.treeDataView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeDataView_AfterSelect);
            // 
            // lvTreeDataRows
            // 
            this.lvTreeDataRows.AllowColumnReorder = true;
            this.lvTreeDataRows.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvTreeDataRows.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvTreeDataRows.HideSelection = false;
            this.lvTreeDataRows.Location = new System.Drawing.Point(0, 103);
            this.lvTreeDataRows.Name = "lvTreeDataRows";
            this.lvTreeDataRows.Size = new System.Drawing.Size(1249, 603);
            this.lvTreeDataRows.TabIndex = 1;
            this.lvTreeDataRows.UseCompatibleStateImageBehavior = false;
            // 
            // groupBox2
            // 
            this.groupBox2.AutoSize = true;
            this.groupBox2.Controls.Add(this.btnDataExportCSV);
            this.groupBox2.Controls.Add(this.btnDataPageHigher);
            this.groupBox2.Controls.Add(this.btnDataPageLower);
            this.groupBox2.Controls.Add(this.lblDataOfPages);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.lblDataPageNumber);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.btnQuery);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.dtpQueryEnd);
            this.groupBox2.Controls.Add(this.dtpQueryStart);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1249, 103);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Select Dates:  ";
            // 
            // btnDataExportCSV
            // 
            this.btnDataExportCSV.Enabled = false;
            this.btnDataExportCSV.Location = new System.Drawing.Point(948, 26);
            this.btnDataExportCSV.Name = "btnDataExportCSV";
            this.btnDataExportCSV.Size = new System.Drawing.Size(111, 33);
            this.btnDataExportCSV.TabIndex = 11;
            this.btnDataExportCSV.Text = "Export CSV";
            this.btnDataExportCSV.UseVisualStyleBackColor = true;
            this.btnDataExportCSV.Click += new System.EventHandler(this.btnDataExportCSV_Click);
            // 
            // btnDataPageHigher
            // 
            this.btnDataPageHigher.Location = new System.Drawing.Point(146, 42);
            this.btnDataPageHigher.Name = "btnDataPageHigher";
            this.btnDataPageHigher.Size = new System.Drawing.Size(28, 28);
            this.btnDataPageHigher.TabIndex = 10;
            this.btnDataPageHigher.UseVisualStyleBackColor = true;
            this.btnDataPageHigher.Click += new System.EventHandler(this.btnDataPageHigher_Click);
            // 
            // btnDataPageLower
            // 
            this.btnDataPageLower.Location = new System.Drawing.Point(112, 42);
            this.btnDataPageLower.Name = "btnDataPageLower";
            this.btnDataPageLower.Size = new System.Drawing.Size(28, 28);
            this.btnDataPageLower.TabIndex = 9;
            this.btnDataPageLower.UseVisualStyleBackColor = true;
            this.btnDataPageLower.Click += new System.EventHandler(this.btnDataPageLower_Click);
            // 
            // lblDataOfPages
            // 
            this.lblDataOfPages.AutoSize = true;
            this.lblDataOfPages.Location = new System.Drawing.Point(72, 65);
            this.lblDataOfPages.Name = "lblDataOfPages";
            this.lblDataOfPages.Size = new System.Drawing.Size(16, 17);
            this.lblDataOfPages.TabIndex = 8;
            this.lblDataOfPages.Text = "1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(37, 65);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(28, 17);
            this.label4.TabIndex = 7;
            this.label4.Text = "of  ";
            // 
            // lblDataPageNumber
            // 
            this.lblDataPageNumber.AutoSize = true;
            this.lblDataPageNumber.Location = new System.Drawing.Point(72, 42);
            this.lblDataPageNumber.Name = "lblDataPageNumber";
            this.lblDataPageNumber.Size = new System.Drawing.Size(16, 17);
            this.lblDataPageNumber.TabIndex = 6;
            this.lblDataPageNumber.Text = "1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 17);
            this.label3.TabIndex = 5;
            this.label3.Text = "Page  ";
            // 
            // btnQuery
            // 
            this.btnQuery.Location = new System.Drawing.Point(677, 26);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(111, 33);
            this.btnQuery.TabIndex = 4;
            this.btnQuery.Text = "Query";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(261, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "End Date:  ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(256, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Start Date:  ";
            // 
            // dtpQueryEnd
            // 
            this.dtpQueryEnd.Location = new System.Drawing.Point(346, 60);
            this.dtpQueryEnd.Name = "dtpQueryEnd";
            this.dtpQueryEnd.Size = new System.Drawing.Size(252, 22);
            this.dtpQueryEnd.TabIndex = 1;
            // 
            // dtpQueryStart
            // 
            this.dtpQueryStart.Location = new System.Drawing.Point(346, 21);
            this.dtpQueryStart.Name = "dtpQueryStart";
            this.dtpQueryStart.Size = new System.Drawing.Size(252, 22);
            this.dtpQueryStart.TabIndex = 0;
            this.dtpQueryStart.Value = new System.DateTime(2024, 1, 1, 0, 0, 0, 0);
            // 
            // tabSettings
            // 
            this.tabSettings.AutoScroll = true;
            this.tabSettings.Location = new System.Drawing.Point(4, 25);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tabSettings.Size = new System.Drawing.Size(1521, 712);
            this.tabSettings.TabIndex = 5;
            this.tabSettings.Text = "Settings";
            this.tabSettings.UseVisualStyleBackColor = true;
            // 
            // tabLogs
            // 
            this.tabLogs.Controls.Add(this.lbLogsOut);
            this.tabLogs.Controls.Add(this.gbLogOptions);
            this.tabLogs.Location = new System.Drawing.Point(4, 25);
            this.tabLogs.Name = "tabLogs";
            this.tabLogs.Padding = new System.Windows.Forms.Padding(3);
            this.tabLogs.Size = new System.Drawing.Size(1521, 712);
            this.tabLogs.TabIndex = 4;
            this.tabLogs.Text = "Logs";
            this.tabLogs.UseVisualStyleBackColor = true;
            // 
            // lbLogsOut
            // 
            this.lbLogsOut.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbLogsOut.FormattingEnabled = true;
            this.lbLogsOut.ItemHeight = 16;
            this.lbLogsOut.Location = new System.Drawing.Point(3, 103);
            this.lbLogsOut.Name = "lbLogsOut";
            this.lbLogsOut.Size = new System.Drawing.Size(1515, 606);
            this.lbLogsOut.TabIndex = 1;
            // 
            // gbLogOptions
            // 
            this.gbLogOptions.Controls.Add(this.btnClearLogs);
            this.gbLogOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbLogOptions.Location = new System.Drawing.Point(3, 3);
            this.gbLogOptions.Name = "gbLogOptions";
            this.gbLogOptions.Size = new System.Drawing.Size(1515, 100);
            this.gbLogOptions.TabIndex = 0;
            this.gbLogOptions.TabStop = false;
            this.gbLogOptions.Text = "Log Options:  ";
            // 
            // btnClearLogs
            // 
            this.btnClearLogs.Location = new System.Drawing.Point(56, 38);
            this.btnClearLogs.Name = "btnClearLogs";
            this.btnClearLogs.Size = new System.Drawing.Size(120, 33);
            this.btnClearLogs.TabIndex = 1;
            this.btnClearLogs.Text = "Clear Logs";
            this.btnClearLogs.UseVisualStyleBackColor = true;
            this.btnClearLogs.Click += new System.EventHandler(this.btnClearLogs_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1529, 791);
            this.Controls.Add(this.tabsMain);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Firewall Monitor  -  Viewer";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabsMain.ResumeLayout(false);
            this.tabFirewallRules.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tabDataView.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabLogs.ResumeLayout(false);
            this.gbLogOptions.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.TabControl tabsMain;
        private System.Windows.Forms.TabPage tabFirewallRules;
        private System.Windows.Forms.ListView lvFirewallRules;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnFirewallRefresh;
        private System.Windows.Forms.TabPage tabLogs;
        private System.Windows.Forms.GroupBox gbLogOptions;
        private System.Windows.Forms.Button btnClearLogs;
        private System.Windows.Forms.ListBox lbLogsOut;
        private System.Windows.Forms.Button btnExpireFirewallRules;
        private System.Windows.Forms.TabPage tabSettings;
        private System.Windows.Forms.TabPage tabDataView;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpQueryEnd;
        private System.Windows.Forms.DateTimePicker dtpQueryStart;
        private System.Windows.Forms.Button btnQuery;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView treeDataView;
        private System.Windows.Forms.ListView lvTreeDataRows;
        private System.Windows.Forms.Button btnDataPageLower;
        private System.Windows.Forms.Label lblDataOfPages;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblDataPageNumber;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnDataPageHigher;
        private System.Windows.Forms.Button btnDataExportCSV;
        private System.Windows.Forms.SaveFileDialog saveFileDialogData;
    }
}

