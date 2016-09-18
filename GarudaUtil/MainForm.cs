using Garuda.Data;
using Garuda.Data.MetaData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GarudaUtil
{
    public partial class MainForm : Form
    {
        PhoenixConnection _connection = null;
        int _queryCounter = 1;

        struct TreeImgNdx
        {
            internal const int Server = 0;
            internal const int Table = 2;
            internal const int Column = 4;
            internal const int Folder = 6;
            internal const int Key = 7;
            internal const int Index = 8;
            internal const int Schema = 6;

        }

        public MainForm()
        {
            InitializeComponent();

            _tsslCurrent.Text = Properties.Resources.StatusReady;
            _tsbNewQuery.Enabled = false;
            _tsbOpenFile.Enabled = false;
            _tsbSave.Enabled = false;
            _tsbRefreshTree.Enabled = false;
                

        }

        private async void _tsbConnect_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateBusyWaitState(true, Properties.Resources.StatusConnecting);

                LoginForm frmLogin = new LoginForm();
                if(DialogResult.OK == frmLogin.ShowDialog())
                {
                    _connection = frmLogin.Connection;
                    _tsslConnection.Text = frmLogin.Server;

                    // Add the server root to the tree
                    TreeNode root = null;
                    foreach (TreeNode tn in _treeView.Nodes)
                    {
                        var ph = tn.Tag as PhoenixConnection;
                        if (tn.Text == frmLogin.Server)
                        {
                            root = tn;
                            break;
                        }
                    }

                    // If not found, add new.
                    if (null == root)
                    {
                        root = _treeView.Nodes.Add(frmLogin.Server);
                    }

                    root.Tag = _connection;
                    root.ImageIndex = TreeImgNdx.Server;

                    RefreshTreeTables();
                    _tsbNewQuery.Enabled = true;
                    _tsbOpenFile.Enabled = true;
                    _tsbSave.Enabled = true;
                    _tsbRefreshTree.Enabled = true;

                    if(_tabControl.TabPages.Count == 0)
                    {
                        _tsbNewQuery_Click(_tsbNewQuery, new EventArgs());
                    }
                }
            }
            catch(Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                UpdateBusyWaitState(false, null);
            }
        }

        private void RefreshTreeTables()
        {
            if(_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            TreeNode root = _treeView.Nodes[0];
            root.Nodes.Clear();

            // Get list of tables and show in tree
            DataTable tables = _connection.GetTables();
            foreach (DataRow row in tables.Rows)
            {
                GarudaPhoenixTable table = new GarudaPhoenixTable(row);
                TreeNode nSchema = GetSchemaTreeNode(table.Schema);
                TreeNode t = nSchema.Nodes.Add(table.FullName);

                t.Tag = table;
                t.ImageIndex = TreeImgNdx.Table;
                t.SelectedImageIndex = t.ImageIndex;
                t.ContextMenuStrip = _cmsTreeTableMenu;
            }

            root.Expand();

            // Show tables in grid view for now.
            //UpdateDataGrid(tables);
        }

        private TreeNode GetSchemaTreeNode(string schema)
        {
            var root = _treeView.Nodes[0];
            var nSchema = root.Nodes[schema];

            if (null == nSchema)
            {
                if (string.IsNullOrWhiteSpace(schema))
                {
                    nSchema = GetSchemaTreeNode("(default)");
                }
                else
                {
                    nSchema = root.Nodes.Add(schema, schema);
                    nSchema.ImageIndex = TreeImgNdx.Schema;
                    nSchema.SelectedImageIndex = nSchema.ImageIndex;
                }
            }

            return nSchema;
        }

        private void HandleException(Exception ex)
        {
            if(typeof(AggregateException) == ex.GetType())
            {
                MessageBox.Show(this, ex.InnerException.Message, ex.InnerException.GetType().Name);
            }
            else
            {
                MessageBox.Show(this, ex.Message, ex.GetType().Name);
            }
        }

        

        public void UpdateBusyWaitState(bool useWaitCursor, string statusText)
        {
            this.UseWaitCursor = useWaitCursor;
            if (!string.IsNullOrWhiteSpace(statusText))
            {
                _tsslCurrent.Text = statusText;
            }
            else
            {
                _tsslCurrent.Text = Properties.Resources.StatusReady;
            }

            this.Refresh();
        }

        private async void OnTreeTableDoubleClick(TreeNodeMouseClickEventArgs e)
        {
            try
            {
                var tmd = (GarudaPhoenixTable)e.Node.Tag;
            
                if (e.Node.Nodes.Count == 0)
                {
                    TreeNode nTable = e.Node;

                    #region Columns Folder
                    TreeNode nColumnsFolder = e.Node.Nodes.Add(Properties.Resources.TreeFolderColumns);

                    nColumnsFolder.ImageIndex = TreeImgNdx.Folder;
                    nColumnsFolder.SelectedImageIndex = nColumnsFolder.ImageIndex;

                    var columns = await tmd.GetColumnsAsync(_connection, false);
                    foreach (DataRow row in columns.Rows)
                    {
                        string col = Convert.ToString(row["COLUMN_NAME"]);
                        bool isPK = DBNull.Value != row["KEY_SEQ"];

                        TreeNode t = nColumnsFolder.Nodes.Add(col);
                        t.Tag = row;
                        if(isPK)
                        {
                            t.ImageIndex = TreeImgNdx.Key;
                        }
                        else
                        {
                            t.ImageIndex = TreeImgNdx.Column;
                        }
                    
                        t.SelectedImageIndex = t.ImageIndex;
                    }
                    #endregion

                    #region Indexes Folder
                    var indexes = await tmd.GetIndexesAsync(_connection, false);
                    TreeNewSubFolder<GarudaPhoenixIndex>(e.Node, Properties.Resources.TreeFolderIndexes, indexes, TreeImgNdx.Index, null, "INDEX_NAME");
                    #endregion

                    e.Node.Expand();

                    // Show columns in grid view for now.
                    //UpdateDataGrid(columns);
                }
            }
            catch(Exception ex)
            {
                HandleException(ex);
            }
        }

        private void TreeNewSubFolder<T>(TreeNode parent, string subfolderName, DataTable items, int ndxFolderChildImg, ContextMenuStrip menu, string nameField)
            where T : IGarudaPhoenixMetaData, new()
        {
            TreeNode nSubFolder = parent.Nodes.Add(subfolderName);

            nSubFolder.ImageIndex = TreeImgNdx.Folder;
            nSubFolder.SelectedImageIndex = nSubFolder.ImageIndex;

            foreach (DataRow row in items.Rows)
            {
                string col = Convert.ToString(row[nameField]);
                TreeNode t = nSubFolder.Nodes.Add(col);
                IGarudaPhoenixMetaData md = new T()
                {
                    Row = row
                };

                t.Tag = md;
                t.ImageIndex = ndxFolderChildImg;
                t.SelectedImageIndex = t.ImageIndex;

                if(null != menu)
                {
                    t.ContextMenuStrip = menu;
                }
            }
        }

        private void _treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                if(null != e.Node.Tag)
                {
                    if (typeof(GarudaPhoenixTable) == e.Node.Tag.GetType())
                    {
                        OnTreeTableDoubleClick(e);
                    }
                    else if(typeof(GarudaPhoenixIndex) == e.Node.Tag.GetType())
                    {
                        PhoenixIndexForm frm = new PhoenixIndexForm();
                        GarudaPhoenixIndex ndx = e.Node.Tag as GarudaPhoenixIndex;
                        frm.TableName = ndx.TableName;
                        frm.IndexName = ndx.Name;
                        frm.KeyColumns = ndx.GetKeyColumns(_connection);
                        frm.ShowDialog(this);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void _tsbNewQuery_Click(object sender, EventArgs e)
        {
            try
            {
                NewQueryViewTab(null, null);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private QueryView NewQueryViewTab(string name, FileInfo fileInfo)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                name = string.Format("Query{0}", _queryCounter++);
            }

            TabPage tp = new TabPage(name);
            tp.ToolTipText = name;

            var qv = new QueryView(this, _connection.ConnectionString, fileInfo);
            tp.Controls.Add(qv);
            _tabControl.TabPages.Add(tp);
            _tabControl.SelectedTab = tp;

            return qv;
        }

        private void _tsmiSelectTop1000_Click(object sender, EventArgs e)
        {
            try
            {
                // Get location of context menu. This cooresponds to the point underwhich
                // is the node we care about.
                GarudaPhoenixTable table = GetTableFromTreeHitTest();
                if (null != table)
                {
                    // If there is a node at this location, use it's name for the query.
                    QueryView qv = NewQueryViewTab(null, null);
                    qv.Text = string.Format("SELECT * FROM {0} LIMIT 1000", table.FullName);
                    qv.ExecuteQuery();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private async void _tsmiTableScriptInsert_Click(object sender, EventArgs e)
        {
            try
            {
                // Get location of context menu. This cooresponds to the point underwhich
                // is the node we care about.
                GarudaPhoenixTable table = GetTableFromTreeHitTest();
                if (null != table)
                {
                    string strUpsert = await table.GenerateUpsertStatementAsync(this._connection);

                    // Open a new query view tab and set the text.
                    QueryView qv = NewQueryViewTab(null, null);
                    qv.Text = strUpsert;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }


        private async void _tsmiTableScriptCreate_Click(object sender, EventArgs e)
        {
            try
            {
                // Get location of context menu. This cooresponds to the point underwhich
                // is the node we care about.
                GarudaPhoenixTable table = GetTableFromTreeHitTest();
                if (null != table)
                {
                    DataTable columns = await table.GetColumnsAsync(_connection, true);

                    using (IDbCommand cmd = _connection.CreateCommand())
                    {
                        cmd.CommandText = string.Format("SELECT * FROM {0} LIMIT 0", table.FullName);
                        using (IDataReader dr = cmd.ExecuteReader())
                        {
                            DataTable schemaTable = dr.GetSchemaTable();

                            StringBuilder sbCreate = new StringBuilder();
                            sbCreate.AppendFormat("CREATE TABLE {0} (", table.FullName);
                            sbCreate.AppendLine();
                            for (int i = 0; i < schemaTable.Rows.Count; i++)
                            {
                                DataRow col = schemaTable.Rows[i];
                                string dataType = dr.GetDataTypeName(i);
                                string colName = col["ColumnName"].ToString();
                                bool isPK =  table.IsColumnPrimaryKey(columns, colName);

                                if (i > 0)
                                {
                                    sbCreate.AppendLine(",");
                                }

                                // Column name and data type, with size for varchars
                                sbCreate.AppendFormat("\t{0} {1}", colName, dataType);
                                if("VARCHAR" == dataType)
                                {
                                    sbCreate.AppendFormat("({0})", col["ColumnSize"]);
                                }

                                // Nullable?
                                if(!Convert.ToBoolean(col["AllowDBNull"]))
                                {
                                    sbCreate.AppendFormat(" NOT");
                                }
                                sbCreate.AppendFormat(" NULL");

                                // Primary key?
                                if(isPK)
                                {
                                    sbCreate.Append(" PRIMARY KEY");
                                }
                            }
                            sbCreate.AppendLine();
                            sbCreate.AppendFormat(")");

                            // Open a new query view tab and set the text.
                            QueryView qv = NewQueryViewTab(null, null);
                            qv.Text = sbCreate.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private GarudaPhoenixTable GetTableFromTreeHitTest()
        {
            GarudaPhoenixTable table = null;
            ToolStrip cms = _cmsTreeTableMenu;
            Point p = new Point(cms.Left, cms.Top);
            var hitTest = _treeView.HitTest(_treeView.PointToClient(p));
            if (hitTest.Node != null)
            {
                table = (GarudaPhoenixTable)hitTest.Node.Tag;
            }

            return table;
        }

         /// <summary>
        /// Custom handler for drawing the tab text including Close X. 
        /// </summary>
        /// <remarks>
        /// Location an dimensions of Close X must match with _tabControl_MouseDown logic.
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            //This code will render a "x" mark at the end of the Tab caption. 
            
            Font bold = new Font(e.Font.FontFamily, e.Font.Size + 1, FontStyle.Bold);

            #region Tab Background Color
            Brush backColorBrush = null;

            switch (e.State)
            {
                case DrawItemState.Selected:
                    backColorBrush = SystemBrushes.ControlLightLight;
                    break;

                default:
                    backColorBrush = SystemBrushes.FromSystemColor(_tabControl.Parent.BackColor);
                    break;
            }

            e.Graphics.FillRectangle(backColorBrush, e.Bounds);
            #endregion

            // Measure the tab text and if exceeds our max, then trim with ...
            SizeF s = e.Graphics.MeasureString(this._tabControl.TabPages[e.Index].Text, e.Font);

            //e.Bounds.Inflate(Convert.ToInt32(s.Width) + 50, 0);
            string text = this._tabControl.TabPages[e.Index].Text;
            if (s.Width > 80)
            {
                text = this._tabControl.TabPages[e.Index].Text.Substring(0, 10) + "...";
            }
            
            e.Graphics.DrawString("x", bold, Brushes.Black, e.Bounds.Right - 15, e.Bounds.Top + 1);
            e.Graphics.DrawString(text, 
                e.Font, Brushes.Black, e.Bounds.Left + 7, e.Bounds.Top + 4);
            
            e.DrawFocusRectangle();
        }

        /// <summary>
        /// Refer to tabControl_DrawItem for details.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _tabControl_MouseDown(object sender, MouseEventArgs e)
        {
            //Looping through the controls.
            for (int i = 0; i < this._tabControl.TabPages.Count; i++)
            {
                Rectangle r = _tabControl.GetTabRect(i);
                //Getting the position of the "x" mark.
                Rectangle closeButton = new Rectangle(r.Right - 15, r.Top + 4, 9, 7);
                if (closeButton.Contains(e.Location))
                {
                    if (MessageBox.Show(string.Format("Would you like to close tab \"{0}\"?", this._tabControl.TabPages[i].Text), 
                        "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        this._tabControl.TabPages.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private void _tsbOpenFile_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();

                ofd.Filter = GarudaUtil.Properties.Resources.OpenFileDialogFilter;

                if(DialogResult.OK == ofd.ShowDialog(this))
                {
                    QueryView qv = NewQueryViewTab(System.IO.Path.GetFileName(ofd.FileName), 
                        new FileInfo(ofd.FileName));
                }
            }
            catch(Exception ex)
            {
                HandleException(ex);
            }
        }

        private void _tsbSave_Click(object sender, EventArgs e)
        {
            try
            {
                if(null != _tabControl.SelectedTab)
                {
                    QueryView qv = _tabControl.SelectedTab.Controls[0] as QueryView;
                    if (null != qv)
                    {
                        qv.Save();
                    }
                }
            }
            catch(Exception ex)
            {
                HandleException(ex);
            }
        }

        private void _tsbRefreshTree_Click(object sender, EventArgs e)
        {
            try
            {
                RefreshTreeTables();
            }
            catch(Exception ex)
            {
                HandleException(ex);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            TabPage tp = _tabControl.SelectedTab;
            switch (keyData)
            {
                case (Keys.F5):
                    if(null != tp)
                    {
                        QueryView qv = tp.Controls[0] as QueryView;
                        qv.ExecuteQuery();
                    }
                    break;

                case (Keys.Control | Keys.N):
                    this._tsbNewQuery_Click(_tsbNewQuery, new EventArgs());
                    break;

                case (Keys.Control | Keys.O):
                    this._tsbOpenFile_Click(_tsbNewQuery, new EventArgs());
                    break;

                case (Keys.Control | Keys.S):
                    if (null != tp)
                    {
                        QueryView qv = tp.Controls[0] as QueryView;
                        qv.Save();
                    }
                    break;

            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            try
            {
                // Show the version/build in the title bar.
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                this.Text = string.Format("Garuda Query v{0}", fvi.FileVersion);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
    }
}
