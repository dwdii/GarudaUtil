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

                    if(tabControl1.TabPages.Count == 0)
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
                NewQueryViewTab();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private QueryView NewQueryViewTab()
        {
            TabPage tp = new TabPage(string.Format("Query{0}", _queryCounter++));
            var qv = new QueryView(this, _connection.ConnectionString);
            tp.Controls.Add(qv);
            tabControl1.TabPages.Add(tp);
            tabControl1.SelectedTab = tp;

            return qv;
        }

        private void _tsmiSelectTop1000_Click(object sender, EventArgs e)
        {
            try
            {
                QueryView qv = NewQueryViewTab();

                qv.Text = string.Format("SELECT * FROM {0} LIMIT 1000", _treeView.SelectedNode.Text);

                qv.ExecuteQuery();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
    }
}
