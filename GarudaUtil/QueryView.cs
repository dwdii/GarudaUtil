using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Garuda.Data;

namespace GarudaUtil
{
    public partial class QueryView : UserControl
    {
        MainForm _mainForm = null;
        string _connectionString = null;
        PhoenixConnection _connection = null;

        public QueryView(MainForm mainForm, string connectionString)
        {
            InitializeComponent();

            _mainForm = mainForm;
            _connectionString = connectionString;
            _connection = new PhoenixConnection(_connectionString);

            this.Dock = DockStyle.Fill;
        }

        public string Text
        {
            get { return _rtbQuery.Text; }
            set { _rtbQuery.Text = value; }
        }

        public void ExecuteQuery()
        {
            _tspExecute_Click(_tspExecute, new EventArgs());
        }

        
        private void UpdateDataGrid(DataTable dt)
        {
            _dataGridView1.AutoGenerateColumns = true;
            _dataGridView1.DataSource = dt;
            _mainForm.UpdateRowCountStatus(dt);
        }

        private void _tspExecute_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();

            try
            {
                if(_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }

                sw.Start();
                _mainForm.UpdateBusyWaitState(true, "Executing...");
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
                        _mainForm.UpdateElapsedStatus(sw, cmd);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                _mainForm.UpdateBusyWaitState(false, null);
            }
        }

        private void _tsbExecutionPlan_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();

            try
            {
                sw.Start();
                _mainForm.UpdateBusyWaitState(true, "Executing...");
                using (PhoenixCommand cmd = new PhoenixCommand(_connection))
                {
                    cmd.CommandText = _rtbQuery.Text;
                    DataTable dt = cmd.Explain();

                    UpdateDataGrid(dt);

                    // How long did the command take?
                    sw.Stop();
                    _mainForm.UpdateElapsedStatus(sw, cmd);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                _mainForm.UpdateBusyWaitState(false, null);
            }
        }

        private void HandleException(Exception ex)
        {
            if (typeof(AggregateException) == ex.GetType())
            {
                MessageBox.Show(this, ex.InnerException.Message, ex.InnerException.GetType().Name);
            }
            else
            {
                MessageBox.Show(this, ex.Message, ex.GetType().Name);
            }
        }
    }
}
