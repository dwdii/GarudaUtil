using Garuda.Data;
using GarudaUtil.MetaData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
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

        public MainForm()
        {
            InitializeComponent();

            _tsslCurrent.Text = "Ready";
            _tsbNewQuery.Enabled = false;
            _tsbOpenFile.Enabled = false;
                

        }

        private async void _tsbConnect_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateBusyWaitState(true, "Connecting...");

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
                    root.ImageIndex = 0;

                    RefreshTreeTables(frmLogin);
                    _tsbNewQuery.Enabled = true;
                    _tsbOpenFile.Enabled = true;

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

        private void RefreshTreeTables(LoginForm frmLogin)
        {
            TreeNode root = _treeView.Nodes[0];
            root.Nodes.Clear();

            // Get list of tables and show in tree
            DataTable tables = _connection.GetTables();
            foreach (DataRow row in tables.Rows)
            {
                string schema = Convert.ToString(row["TABLE_SCHEM"]);
                string table = Convert.ToString(row["TABLE_NAME"]);

                string name;
                if (string.IsNullOrWhiteSpace(schema))
                {
                    name = table;
                }
                else
                {
                    name = string.Format("{0}.{1}", schema, table);
                }

                TreeNode t = root.Nodes.Add(name);
                t.Tag = new GarudaPhoenixTable(row);
                t.ImageIndex = 2;
                t.SelectedImageIndex = t.ImageIndex;
                t.ContextMenuStrip = _cmsTreeTableMenu;
            }

            root.Expand();

            // Show tables in grid view for now.
            //UpdateDataGrid(tables);
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
                _tsslCurrent.Text = "Ready";
            }

            this.Refresh();
        }

        public void UpdateElapsedStatus(Stopwatch sw, PhoenixCommand cmd)
        {
            _tsslElapsed.Text = string.Format("{0} [Cmd: {1}]", sw.Elapsed, cmd.Elapsed);
        }

        public void UpdateRowCountStatus(DataTable dt)
        {
            _tsslRowCount.Text = string.Format("{0} rows", dt.Rows.Count);
        }

        private void OnTreeTableDoubleClick(TreeNodeMouseClickEventArgs e)
        {
            var tmd = (GarudaPhoenixTable)e.Node.Tag;
            
            if (e.Node.Nodes.Count == 0)
            {
                var columns = tmd.GetColumns(_connection, false);
                foreach (DataRow row in columns.Rows)
                {
                    string col = Convert.ToString(row["COLUMN_NAME"]);
                    TreeNode t = e.Node.Nodes.Add(col);
                    t.Tag = row;
                    t.ImageIndex = 4;
                    t.SelectedImageIndex = t.ImageIndex;
                }

                e.Node.Expand();

                // Show columns in grid view for now.
                //UpdateDataGrid(columns);
            }
        }

        private void _treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                if (typeof(GarudaPhoenixTable) == e.Node.Tag.GetType())
                {
                    OnTreeTableDoubleClick(e);
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
                NewQueryViewTab(null);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private QueryView NewQueryViewTab(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                name = string.Format("Query{0}", _queryCounter++);
            }

            TabPage tp = new TabPage(name);
            tp.ToolTipText = name;

            var qv = new QueryView(this, _connection.ConnectionString);
            tp.Controls.Add(qv);
            _tabControl.TabPages.Add(tp);
            _tabControl.SelectedTab = tp;

            return qv;
        }

        private void _tsmiSelectTop1000_Click(object sender, EventArgs e)
        {
            try
            {
                QueryView qv = NewQueryViewTab(null);

                qv.Text = string.Format("SELECT * FROM {0} LIMIT 1000", _treeView.SelectedNode.Text);

                qv.ExecuteQuery();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void tabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            //This code will render a "x" mark at the end of the Tab caption. 
            
            Font bold = new Font(e.Font.FontFamily, e.Font.Size + 1, FontStyle.Bold);

            // Measure the tab text and if exceeds our max, then trim with ...
            SizeF s = e.Graphics.MeasureString(this._tabControl.TabPages[e.Index].Text, e.Font);
            //e.Bounds.Inflate(Convert.ToInt32(s.Width) + 50, 0);
            string text = this._tabControl.TabPages[e.Index].Text;
            if (s.Width > 65)
            {
                text = this._tabControl.TabPages[e.Index].Text.Substring(0, 7) + "...";
            }
            
            e.Graphics.DrawString("x", bold, Brushes.Black, e.Bounds.Right - 15, e.Bounds.Top + 1);
            e.Graphics.DrawString(text, 
                e.Font, Brushes.Black, e.Bounds.Left + 7, e.Bounds.Top + 4);
            
            e.DrawFocusRectangle();
        }

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

                ofd.Filter = "SQL Files (*.sql)|*.sql|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

                if(DialogResult.OK == ofd.ShowDialog(this))
                {
                    QueryView qv = NewQueryViewTab(System.IO.Path.GetFileName(ofd.FileName));

                    qv.Text = System.IO.File.ReadAllText(ofd.FileName);
                }
            }
            catch(Exception ex)
            {
                HandleException(ex);
            }
        }
    }
}
