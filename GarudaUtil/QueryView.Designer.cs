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
            this._dataGridView1 = new System.Windows.Forms.DataGridView();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this._tspExecute = new System.Windows.Forms.ToolStripButton();
            this._tsbExecutionPlan = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dataGridView1)).BeginInit();
            this.toolStrip2.SuspendLayout();
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
            this.splitContainer2.Panel2.Controls.Add(this._dataGridView1);
            this.splitContainer2.Size = new System.Drawing.Size(397, 347);
            this.splitContainer2.SplitterDistance = 173;
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
            this._rtbQuery.Size = new System.Drawing.Size(397, 148);
            this._rtbQuery.TabIndex = 0;
            this._rtbQuery.Text = "";
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
            this._dataGridView1.Location = new System.Drawing.Point(0, 0);
            this._dataGridView1.Name = "_dataGridView1";
            this._dataGridView1.ReadOnly = true;
            this._dataGridView1.Size = new System.Drawing.Size(397, 170);
            this._dataGridView1.TabIndex = 1;
            // 
            // toolStrip2
            // 
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._tspExecute,
            this._tsbExecutionPlan});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(397, 25);
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
            // QueryView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer2);
            this.Name = "QueryView";
            this.Size = new System.Drawing.Size(397, 347);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._dataGridView1)).EndInit();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.RichTextBox _rtbQuery;
        private System.Windows.Forms.DataGridView _dataGridView1;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton _tspExecute;
        private System.Windows.Forms.ToolStripButton _tsbExecutionPlan;
    }
}
