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
using System.IO;

namespace GarudaUtil
{
    public partial class QueryView : UserControl
    {
        MainForm _mainForm = null;
        string _connectionString = null;
        PhoenixConnection _connection = null;
        FileInfo _fileInfo = null;

        public QueryView(MainForm mainForm, string connectionString, FileInfo fileInfo)
        {
            InitializeComponent();

            _mainForm = mainForm;
            _connectionString = connectionString;
            _connection = new PhoenixConnection(_connectionString);
            _fileInfo = fileInfo;

            if(null != _fileInfo)
            {
                this.Text = System.IO.File.ReadAllText(this._fileInfo.FullName);
            }

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
            for(int i = 0; i < dt.Columns.Count; i++)
            {
                if(_dataGridView1.Columns[i].ValueType == typeof(DateTime))
                {
                    _dataGridView1.Columns[i].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss.fff";
                }
            }

            _tabControl1.SelectedTab = _tabResults;
            UpdateRowCountStatus(dt);
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

                // Clear the messages.
                _txtMessages.Clear();

                sw.Start();
                _mainForm.UpdateBusyWaitState(true, Properties.Resources.StatusExecuting);
                using (PhoenixCommand cmd = new PhoenixCommand(_connection))
                {
                    cmd.CommandText = _rtbQuery.Text;
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        if (0 < dr.FieldCount)
                        {
                            dt.Load(dr);
                        }

                        UpdateDataGrid(dt);
                        _txtMessages.AppendText(string.Format("{0} record(s) affected", dr.RecordsAffected));

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
                _mainForm.UpdateBusyWaitState(false, null);
            }
        }

        private void _tsbExecutionPlan_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();

            try
            {
                sw.Start();
                _mainForm.UpdateBusyWaitState(true, Properties.Resources.StatusExecuting);
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
                _mainForm.UpdateBusyWaitState(false, null);
            }
        }

        private void HandleException(Exception ex)
        {
            _txtMessages.AppendText(ex.ToString());
            _tabControl1.SelectedTab = _tabMessages;
            
            //if (typeof(AggregateException) == ex.GetType())
            //{
            //    MessageBox.Show(this, ex.InnerException.Message, ex.InnerException.GetType().Name);
            //}
            //else
            //{
            //    MessageBox.Show(this, ex.Message, ex.GetType().Name);
            //}
        }

        public void UpdateElapsedStatus(Stopwatch sw, PhoenixCommand cmd)
        {
            _tsslElapsed.Text = string.Format("{0} [Cmd: {1}]", sw.Elapsed, cmd.Elapsed);
        }

        public void UpdateRowCountStatus(DataTable dt)
        {
            _tsslRowCount.Text = string.Format("{0} rows", dt.Rows.Count);
        }

        internal void Save()
        {
            DialogResult dr = DialogResult.OK;
            if (null == _fileInfo)
            {
                SaveFileDialog sfd = new SaveFileDialog();

                sfd.OverwritePrompt = true;
                sfd.Filter = Properties.Resources.OpenFileDialogFilter;
                sfd.AddExtension = true;
                sfd.DefaultExt = "sql";

                dr = sfd.ShowDialog(this);
                if (DialogResult.OK == dr)
                {
                    _fileInfo = new FileInfo(sfd.FileName);
                    (this.Parent as TabPage).Text = _fileInfo.Name;
                }
            }

            if(DialogResult.OK == dr)
            {
                File.WriteAllText(_fileInfo.FullName, this.Text);
            }
        }
    }
}
