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

        public MainForm()
        {
            InitializeComponent();

            _tsslCurrent.Text = "Ready";

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
                    TreeNode root = _treeView.Nodes.Add(frmLogin.Server);
                    root.Tag = _connection;
                    root.ImageIndex = 0;

                    RefreshTreeTables(frmLogin);

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
            }

            root.Expand();

            // Show tables in grid view for now.
            UpdateDataGrid(tables);
        }

        private void UpdateBusyWaitState(bool useWaitCursor, string statusText)
        {
            this.UseWaitCursor = useWaitCursor;
            if(!string.IsNullOrWhiteSpace(statusText))
            {
                _tsslCurrent.Text = statusText;
            }
            else
            {
                _tsslCurrent.Text = "Ready";
            }
            
            this.Refresh();
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

        private void _tspExecute_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();

            try
            {
                sw.Start();
                UpdateBusyWaitState(true, "Executing...");
                using (PhoenixCommand cmd = new PhoenixCommand(_connection))
                {
                    cmd.CommandText = _rtbQuery.Text;
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(dr);

                        UpdateDataGrid(dt);

                        // How long did the command take?
                        sw.Stop();
                        UpdateElapsedStatus(sw, cmd);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                UpdateBusyWaitState(false, null);
            }
        }

        private void _tsbExecutionPlan_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();

            try
            {
                sw.Start();
                UpdateBusyWaitState(true, "Executing...");
                using (PhoenixCommand cmd = new PhoenixCommand(_connection))
                {
                    cmd.CommandText = _rtbQuery.Text;
                    DataTable dt = cmd.Explain();

                    UpdateDataGrid(dt);

                    // How long did the command take?
                    sw.Stop();
                    UpdateElapsedStatus(sw, cmd);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                UpdateBusyWaitState(false, null);
            }
        }

        private void UpdateElapsedStatus(Stopwatch sw, PhoenixCommand cmd)
        {
            _tsslElapsed.Text = string.Format("{0} [Cmd: {1}]", sw.Elapsed, cmd.Elapsed);
        }

        private void UpdateDataGrid(DataTable dt)
        {
            _dataGridView1.AutoGenerateColumns = true;
            _dataGridView1.DataSource = dt;
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
                UpdateDataGrid(columns);
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
    }
}
