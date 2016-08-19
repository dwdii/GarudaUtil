namespace GarudaUtil
{
    partial class QueryView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this._rtbQuery = new System.Windows.Forms.RichTextBox();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this._tspExecute = new System.Windows.Forms.ToolStripButton();
            this._tsbExecutionPlan = new System.Windows.Forms.ToolStripButton();
            this._tabControl1 = new System.Windows.Forms.TabControl();
            this._tabResults = new System.Windows.Forms.TabPage();
            this._dataGridView1 = new System.Windows.Forms.DataGridView();
            this._tabMessages = new System.Windows.Forms.TabPage();
            this._txtMessages = new System.Windows.Forms.TextBox();
            this._statusStrip = new System.Windows.Forms.StatusStrip();
            this._tsslElapsed = new System.Windows.Forms.ToolStripStatusLabel();
            this._tsslRowCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this._tabControl1.SuspendLayout();
            this._tabResults.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dataGridView1)).BeginInit();
            this._tabMessages.SuspendLayout();
            this._statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this._rtbQuery);
            this.splitContainer2.Panel1.Controls.Add(this.toolStrip2);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this._tabControl1);
            this.splitContainer2.Size = new System.Drawing.Size(511, 381);
            this.splitContainer2.SplitterDistance = 189;
            this.splitContainer2.TabIndex = 3;
            // 
            // _rtbQuery
            // 
            this._rtbQuery.AcceptsTab = true;
            this._rtbQuery.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rtbQuery.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._rtbQuery.Location = new System.Drawing.Point(0, 25);
            this._rtbQuery.Name = "_rtbQuery";
            this._rtbQuery.ShowSelectionMargin = true;
            this._rtbQuery.Size = new System.Drawing.Size(511, 164);
            this._rtbQuery.TabIndex = 0;
            this._rtbQuery.Text = "";
            // 
            // toolStrip2
            // 
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._tspExecute,
            this._tsbExecutionPlan});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(511, 25);
            this.toolStrip2.TabIndex = 2;
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
            // _tsbExecutionPlan
            // 
            this._tsbExecutionPlan.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._tsbExecutionPlan.Image = global::GarudaUtil.Properties.Resources.Training_Filled_50;
            this._tsbExecutionPlan.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._tsbExecutionPlan.Name = "_tsbExecutionPlan";
            this._tsbExecutionPlan.Size = new System.Drawing.Size(23, 22);
            this._tsbExecutionPlan.Text = "Execution Plan";
            this._tsbExecutionPlan.Click += new System.EventHandler(this._tsbExecutionPlan_Click);
            // 
            // _tabControl1
            // 
            this._tabControl1.Controls.Add(this._tabResults);
            this._tabControl1.Controls.Add(this._tabMessages);
            this._tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tabControl1.Location = new System.Drawing.Point(0, 0);
            this._tabControl1.Name = "_tabControl1";
            this._tabControl1.SelectedIndex = 0;
            this._tabControl1.Size = new System.Drawing.Size(511, 188);
            this._tabControl1.TabIndex = 2;
            // 
            // _tabResults
            // 
            this._tabResults.Controls.Add(this._dataGridView1);
            this._tabResults.Location = new System.Drawing.Point(4, 22);
            this._tabResults.Name = "_tabResults";
            this._tabResults.Padding = new System.Windows.Forms.Padding(3);
            this._tabResults.Size = new System.Drawing.Size(503, 163);
            this._tabResults.TabIndex = 0;
            this._tabResults.Text = "Results";
            this._tabResults.UseVisualStyleBackColor = true;
            // 
            // _dataGridView1
            // 
            this._dataGridView1.AllowUserToAddRows = false;
            this._dataGridView1.AllowUserToDeleteRows = false;
            this._dataGridView1.AllowUserToOrderColumns = true;
            this._dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this._dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            this._dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dataGridView1.Location = new System.Drawing.Point(3, 3);
            this._dataGridView1.Name = "_dataGridView1";
            this._dataGridView1.ReadOnly = true;
            this._dataGridView1.Size = new System.Drawing.Size(497, 157);
            this._dataGridView1.TabIndex = 1;
            // 
            // _tabMessages
            // 
            this._tabMessages.Controls.Add(this._txtMessages);
            this._tabMessages.Location = new System.Drawing.Point(4, 22);
            this._tabMessages.Name = "_tabMessages";
            this._tabMessages.Padding = new System.Windows.Forms.Padding(3);
            this._tabMessages.Size = new System.Drawing.Size(503, 162);
            this._tabMessages.TabIndex = 1;
            this._tabMessages.Text = "Messages";
            this._tabMessages.UseVisualStyleBackColor = true;
            // 
            // _txtMessages
            // 
            this._txtMessages.BackColor = System.Drawing.SystemColors.Window;
            this._txtMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtMessages.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._txtMessages.Location = new System.Drawing.Point(3, 3);
            this._txtMessages.Multiline = true;
            this._txtMessages.Name = "_txtMessages";
            this._txtMessages.ReadOnly = true;
            this._txtMessages.Size = new System.Drawing.Size(497, 156);
            this._txtMessages.TabIndex = 0;
            // 
            // _statusStrip
            // 
            this._statusStrip.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this._statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this._tsslElapsed,
            this._tsslRowCount});
            this._statusStrip.Location = new System.Drawing.Point(0, 381);
            this._statusStrip.Name = "_statusStrip";
            this._statusStrip.Size = new System.Drawing.Size(511, 24);
            this._statusStrip.SizingGrip = false;
            this._statusStrip.TabIndex = 4;
            this._statusStrip.Text = "statusStrip1";
            // 
            // _tsslElapsed
            // 
            this._tsslElapsed.AutoSize = false;
            this._tsslElapsed.Name = "_tsslElapsed";
            this._tsslElapsed.Size = new System.Drawing.Size(250, 19);
            this._tsslElapsed.Text = "00:00";
            // 
            // _tsslRowCount
            // 
            this._tsslRowCount.AutoSize = false;
            this._tsslRowCount.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this._tsslRowCount.Name = "_tsslRowCount";
            this._tsslRowCount.Size = new System.Drawing.Size(122, 19);
            this._tsslRowCount.Text = "0 rows";
            this._tsslRowCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.AutoSize = false;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(200, 19);
            // 
            // QueryView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this._statusStrip);
            this.Name = "QueryView";
            this.Size = new System.Drawing.Size(511, 405);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this._tabControl1.ResumeLayout(false);
            this._tabResults.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._dataGridView1)).EndInit();
            this._tabMessages.ResumeLayout(false);
            this._tabMessages.PerformLayout();
            this._statusStrip.ResumeLayout(false);
            this._statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.RichTextBox _rtbQuery;
        private System.Windows.Forms.DataGridView _dataGridView1;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton _tspExecute;
        private System.Windows.Forms.ToolStripButton _tsbExecutionPlan;
        private System.Windows.Forms.TabControl _tabControl1;
        private System.Windows.Forms.TabPage _tabResults;
        private System.Windows.Forms.TabPage _tabMessages;
        private System.Windows.Forms.TextBox _txtMessages;
        private System.Windows.Forms.StatusStrip _statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel _tsslElapsed;
        private System.Windows.Forms.ToolStripStatusLabel _tsslRowCount;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    }
}
