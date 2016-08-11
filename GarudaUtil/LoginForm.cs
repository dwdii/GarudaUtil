using Garuda.Data;
using Microsoft.Win32;
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
        private const string RegPath = "SOFTWARE\\Garuda.Data\\Connections";

        public string Server { get; set; }

        public PhoenixConnection Connection { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
        }

        private void _btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                this.Server = _cbServer.Text;

                this.Connection = new PhoenixConnection();
                this.Connection.ConnectionString = string.Format("server={0};Request Timeout=15000", this.Server);
                this.Connection.Open();

                if(this.Connection.State == ConnectionState.Open)
                {
                    AddServerToListIfNotExists();
                }

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

        private void LoginForm_Shown(object sender, EventArgs e)
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.CreateSubKey(RegPath);
                foreach(string name in rk.GetValueNames())
                {
                    _cbServer.Items.Add(rk.GetValue(name));
                }
            }
            catch(Exception ex)
            {
                HandleException(ex);
            }
        }

        private void AddServerToListIfNotExists()
        {
            if(!_cbServer.Items.Contains(this.Server))
            {
                RegistryKey rk = Registry.CurrentUser.CreateSubKey(RegPath);
                string strName = string.Format("Connection{0}", rk.ValueCount);
                rk.SetValue(strName, this.Server);
            }
        }
    }
}
