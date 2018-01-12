using Apache.Phoenix;
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
    public partial class PhoenixIndexForm : Form
    {
        DataTable _dtKeyColumns = null;

        public PhoenixIndexForm()
        {
            InitializeComponent();
        }

        public string TableName
        {
            get { return _txtTableName.Text; }
            set { _txtTableName.Text = value; }
        }

        public string IndexName
        {
            get { return _txtIndexName.Text; }
            set { _txtIndexName.Text = value; }
        }

        public DataTable KeyColumns
        {
            get { return _dtKeyColumns; }
            set { _dtKeyColumns = value; }
        }

        public bool NewMode { get; set; }

        private void PhoenixIndexForm_Shown(object sender, EventArgs e)
        {
            try
            {
                _txtTableName.ReadOnly = !NewMode;
                _txtIndexName.ReadOnly = !NewMode;

                foreach(DataRow r in this.KeyColumns.Rows)
                {
                    ListViewItem lvi = new ListViewItem();

                    lvi.Text = r["COLUMN_DEF"].ToString();

                    //ListViewItem.ListViewSubItem siDataType = new ListViewItem.ListViewSubItem();
                    //Rep dt = (Rep)Convert.ToInt32(r["DATA_TYPE"]);
                    //siDataType.Text = dt.ToString();
                    //lvi.SubItems.Add(siDataType);

                    ListViewItem.ListViewSubItem siKeySeq = new ListViewItem.ListViewSubItem();
                    siKeySeq.Text = Convert.ToInt32(r["KEY_SEQ"]).ToString();
                    lvi.SubItems.Add(siKeySeq);

                    _lvKeyCols.Items.Add(lvi);
                }
            }
            catch(Exception ex)
            {
                HandleException(ex);
            }
        }

        private void HandleException(Exception ex)
        {
            MessageBox.Show(this, ex.Message, ex.GetType().Name, MessageBoxButtons.OK);
        }

        private void _btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            catch(Exception ex)
            {
                HandleException(ex);
            }
        }
    }
}
