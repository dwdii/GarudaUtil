using Garuda.Data;
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
        }

        private async void _tsbConnect_Click(object sender, EventArgs e)
        {
            try
            {
                LoginForm frmLogin = new LoginForm();

                if(DialogResult.OK == frmLogin.ShowDialog())
                {
                    _connection = new PhoenixConnection();
                    _connection.ConnectionString = string.Format("server={0};Request Timeout=15000", frmLogin.Server);
                    _connection.Open();
                }
            }
            catch(Exception ex)
            {
                HandleException(ex);
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
    }
}
