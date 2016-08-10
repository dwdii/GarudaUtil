using Garuda.Data;
using GarudaUtil.MetaData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
                LoginForm frmLogin = new LoginForm();

                if(DialogResult.OK == frmLogin.ShowDialog())
                {
                    this.UseWaitCursor = true;
                    _tsslCurrent.Text = "Connecting...";
                    this.Refresh();

                    _connection = new PhoenixConnection();
                    _connection.ConnectionString = string.Format("server={0};Request Timeout=15000", frmLogin.Server);
                    _connection.Open();

                    _tsslConnection.Text = frmLogin.Server;

                    // Add the server root to the tree
                    TreeNode root = _treeView.Nodes.Add(frmLogin.Server);
                    root.Tag = _connection;
                    root.ImageIndex = 0;

                    // Get list of tables and show in tree
                    DataTable tables = _connection.GetTables();
                    foreach (DataRow row in tables.Rows)
                    {
                        string schema = Convert.ToString(row["TABLE_SCHEM"]);
                        string table = Convert.ToString(row["TABLE_NAME"]);

                        string name;
                        if(string.IsNullOrWhiteSpace(schema))
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
                    dataGridView1.DataSource = tables;

                }
            }
            catch(Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                _tsslCurrent.Text = "Ready";
                this.UseWaitCursor = false;
            }
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
            try
            {
                using (IDbCommand cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = _rtbQuery.Text;
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(dr);

                        this.dataGridView1.AutoGenerateColumns = true;
                        this.dataGridView1.DataSource = dt;

                        // Automatically resize the visible rows.
                        dataGridView1.AutoSizeRowsMode =
                            DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;

                        // Set the DataGridView control's border.
                        dataGridView1.BorderStyle = BorderStyle.Fixed3D;

                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void _treeView_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                MouseEventArgs m = (MouseEventArgs)e;
                TreeViewHitTestInfo hit = _treeView.HitTest(m.Location);

                if(null != hit.Node)
                {
                    if(typeof(GarudaPhoenixTable) == hit.Node.Tag.GetType())
                    {
                        var tmd = (GarudaPhoenixTable)hit.Node.Tag;
                        if(hit.Node.Nodes.Count == 0)
                        {
                            var columns = tmd.GetColumns(_connection);
                            foreach (DataRow row in columns.Rows)
                            {
                                string col = Convert.ToString(row["COLUMN_NAME"]);
                                TreeNode t = hit.Node.Nodes.Add(col);
                                t.Tag = row;
                                t.ImageIndex = 4;
                                t.SelectedImageIndex = t.ImageIndex;
                            }

                            hit.Node.Expand();

                            // Show columns in grid view for now.
                            dataGridView1.DataSource = columns;
                        }

                    }
                }

                
            }
            catch(Exception ex)
            {
                HandleException(ex);
            }
        }
    }
}
