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
    public partial class LoginForm : Form
    {
        public string Server { get; set; }

        public LoginForm()
        {
            InitializeComponent();
        }

        private void _btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                this.Server = _cbServer.Text;
                this.DialogResult = DialogResult.OK;
            }
            catch(Exception ex)
            {
                HandleException(ex);
            }
        }

        private void HandleException(Exception ex)
        {
            MessageBox.Show(this, ex.Message, ex.GetType().Name);
        }
    }
}
