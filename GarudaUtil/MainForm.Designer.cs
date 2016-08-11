namespace GarudaUtil
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this._tsbConnect = new System.Windows.Forms.ToolStripButton();
            this._treeView = new System.Windows.Forms.TreeView();
            this._imgListTree = new System.Windows.Forms.ImageList(this.components);
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this._rtbQuery = new System.Windows.Forms.RichTextBox();
            this._dataGridView1 = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this._tspExecute = new System.Windows.Forms.ToolStripButton();
            this._statusStrip = new System.Windows.Forms.StatusStrip();
            this._tsslCurrent = new System.Windows.Forms.ToolStripStatusLabel();
            this._tsslConnection = new System.Windows.Forms.ToolStripStatusLabel();
            this._tsslElapsed = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dataGridView1)).BeginInit();
            this.toolStrip2.SuspendLayout();
            this._statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel3);
            this.splitContainer1.Size = new System.Drawing.Size(858, 391);
            this.splitContainer1.SplitterDistance = 285;
            this.splitContainer1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.toolStrip1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this._treeView, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(285, 391);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._tsbConnect});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(285, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // _tsbConnect
            // 
            this._tsbConnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._tsbConnect.Image = global::GarudaUtil.Properties.Resources.Connected_64;
            this._tsbConnect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._tsbConnect.Name = "_tsbConnect";
            this._tsbConnect.Size = new System.Drawing.Size(23, 22);
            this._tsbConnect.Text = "Connect...";
            this._tsbConnect.Click += new System.EventHandler(this._tsbConnect_Click);
            // 
            // _treeView
            // 
            this._treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._treeView.ImageIndex = 0;
            this._treeView.ImageList = this._imgListTree;
            this._treeView.Location = new System.Drawing.Point(3, 28);
            this._treeView.Name = "_treeView";
            this._treeView.SelectedImageIndex = 0;
            this._treeView.ShowRootLines = false;
            this._treeView.Size = new System.Drawing.Size(279, 371);
            this._treeView.TabIndex = 1;
            this._treeView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this._treeView_NodeMouseDoubleClick);
            // 
            // _imgListTree
            // 
            this._imgListTree.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_imgListTree.ImageStream")));
            this._imgListTree.TransparentColor = System.Drawing.Color.Transparent;
            this._imgListTree.Images.SetKeyName(0, "Cloud Storage Filled-50.png");
            this._imgListTree.Images.SetKeyName(1, "Database-50.png");
            this._imgListTree.Images.SetKeyName(2, "Insert Table Filled-50.png");
            this._imgListTree.Images.SetKeyName(3, "Origami-50.png");
            this._imgListTree.Images.SetKeyName(4, "List-52.png");
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.tabControl1, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.toolStrip2, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(569, 391);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 28);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(563, 390);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainer2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(555, 364);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this._rtbQuery);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this._dataGridView1);
            this.splitContainer2.Size = new System.Drawing.Size(549, 358);
            this.splitContainer2.SplitterDistance = 179;
            this.splitContainer2.TabIndex = 2;
            // 
            // _rtbQuery
            // 
            this._rtbQuery.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rtbQuery.Location = new System.Drawing.Point(0, 0);
            this._rtbQuery.Name = "_rtbQuery";
            this._rtbQuery.Size = new System.Drawing.Size(549, 179);
            this._rtbQuery.TabIndex = 0;
            this._rtbQuery.Text = "";
            // 
            // _dataGridView1
            // 
            this._dataGridView1.AllowUserToAddRows = false;
            this._dataGridView1.AllowUserToDeleteRows = false;
            this._dataGridView1.AllowUserToOrderColumns = true;
            this._dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dataGridView1.Location = new System.Drawing.Point(0, 0);
            this._dataGridView1.Name = "_dataGridView1";
            this._dataGridView1.ReadOnly = true;
            this._dataGridView1.Size = new System.Drawing.Size(549, 175);
            this._dataGridView1.TabIndex = 1;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(555, 364);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // toolStrip2
            // 
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._tspExecute});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(569, 25);
            this.toolStrip2.TabIndex = 1;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // _tspExecute
            // 
            this._tspExecute.Image = global::GarudaUtil.Properties.Resources.Fire_Element_Filled_50;
            this._tspExecute.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._tspExecute.Name = "_tspExecute";
            this._tspExecute.Size = new System.Drawing.Size(67, 22);
            this._tspExecute.Text = "Execute";
            this._tspExecute.Click += new System.EventHandler(this._tspExecute_Click);
            // 
            // _statusStrip
            // 
            this._statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._tsslCurrent,
            this._tsslConnection,
            this._tsslElapsed});
            this._statusStrip.Location = new System.Drawing.Point(0, 391);
            this._statusStrip.Name = "_statusStrip";
            this._statusStrip.Size = new System.Drawing.Size(858, 22);
            this._statusStrip.TabIndex = 1;
            this._statusStrip.Text = "statusStrip1";
            // 
            // _tsslCurrent
            // 
            this._tsslCurrent.AutoSize = false;
            this._tsslCurrent.Name = "_tsslCurrent";
            this._tsslCurrent.Padding = new System.Windows.Forms.Padding(0, 0, 50, 0);
            this._tsslCurrent.Size = new System.Drawing.Size(100, 17);
            this._tsslCurrent.Text = "Ready";
            this._tsslCurrent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _tsslConnection
            // 
            this._tsslConnection.AutoSize = false;
            this._tsslConnection.AutoToolTip = true;
            this._tsslConnection.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this._tsslConnection.Name = "_tsslConnection";
            this._tsslConnection.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this._tsslConnection.Size = new System.Drawing.Size(350, 17);
            this._tsslConnection.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _tsslElapsed
            // 
            this._tsslElapsed.AutoSize = false;
            this._tsslElapsed.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this._tsslElapsed.Name = "_tsslElapsed";
            this._tsslElapsed.Size = new System.Drawing.Size(250, 17);
            this._tsslElapsed.Text = "00:00";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(858, 413);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this._statusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Garuda Query";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._dataGridView1)).EndInit();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this._statusStrip.ResumeLayout(false);
            this._statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.TreeView _treeView;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RichTextBox _rtbQuery;
        private System.Windows.Forms.DataGridView _dataGridView1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.StatusStrip _statusStrip;
        private System.Windows.Forms.ToolStripButton _tsbConnect;
        private System.Windows.Forms.ToolStripButton _tspExecute;
        private System.Windows.Forms.ToolStripStatusLabel _tsslCurrent;
        private System.Windows.Forms.ToolStripStatusLabel _tsslConnection;
        private System.Windows.Forms.ImageList _imgListTree;
        private System.Windows.Forms.ToolStripStatusLabel _tsslElapsed;
        private System.Windows.Forms.SplitContainer splitContainer2;
    }
}

