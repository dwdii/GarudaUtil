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

        ModeItem _modeVnet = new ModeItem("VNET / Apache Phoenix", PhoenixConnectionModeStr.Vnet);
        ModeItem _modeHdiGateway = new ModeItem("HDInsight Gateway", PhoenixConnectionModeStr.HdiGateway);

        public string Server { get; set; }

        public PhoenixConnection Connection { get; private set; }

        class ModeItem
        {


            public ModeItem(string displayName, string key)
            {
                this.DisplayName = displayName;
                this.Key = key;
            }

            public string DisplayName { get; set; }

            public string Key { get; set; }

            public override string ToString()
            {
                return this.DisplayName;
            }
        }

        public LoginForm()
        {
            InitializeComponent();

            _cbMode.Items.Add(_modeVnet);
            _cbMode.Items.Add(_modeHdiGateway);
            _cbMode.SelectedItem = _cbMode.Items[0];
        }

        private void _btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                StringBuilder conStr = new StringBuilder();

                conStr.AppendFormat("server={0};", _cbServer.Text);
                switch((_cbMode.SelectedItem as ModeItem).Key)
                {
                    case PhoenixConnectionModeStr.HdiGateway:
                        conStr.AppendFormat("User Id={0};", _txtUserId.Text);
                        conStr.AppendFormat("Password={0};", _txtPasswd.Text);
                        conStr.AppendFormat("Mode={0};", PhoenixConnectionModeStr.HdiGateway);
                        break;

                    case PhoenixConnectionModeStr.Vnet:
                        //conStr.AppendFormat("Mode:{0};", "VNET")
                        break;


                }

                conStr.Append("Request Timeout=60000;");


                this.Server = _cbServer.Text;

                this.Connection = new PhoenixConnection();
                this.Connection.ConnectionString = conStr.ToString();
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
            string msg = ex.Message;
            if(ex.InnerException != null)
            {
                msg = string.Format("{0}\r\n\r\n{1}", ex.Message, ex.InnerException.Message);
            }

            System.Diagnostics.Trace.WriteLine(ex);

            MessageBox.Show(this, msg, ex.GetType().Name);
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

        private void _cbMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                _txtUserId.Enabled = _cbMode.SelectedItem == _modeHdiGateway;
                _txtPasswd.Enabled = _cbMode.SelectedItem == _modeHdiGateway;
            }
            catch(Exception ex)
            {
                HandleException(ex);
            }
        }
    }
}
