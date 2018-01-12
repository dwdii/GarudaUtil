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
        private const string RegPath_Connections = "SOFTWARE\\Garuda.Data\\Connections";
        private const string RegPath_LastConnection = "SOFTWARE\\Garuda.Data\\Connections\\Last";
        private const string LastServer = "LastServer";
        private const string LastMode = "LastMode";
        private const string LastUsername = "LastUsername";

        ModeItem _modeVnet = new ModeItem("VNET / Apache Phoenix", PhoenixConnectionModeStr.Vnet);
        ModeItem _modeHdiGateway = new ModeItem("HDInsight Gateway", PhoenixConnectionModeStr.HdiGateway);
        Dictionary<string, ModeItem> _modeOptions = new Dictionary<string, ModeItem>();

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

            _modeOptions.Add(_modeHdiGateway.Key, _modeHdiGateway);
            _modeOptions.Add(_modeVnet.Key, _modeVnet);

            foreach(ModeItem mi in _modeOptions.Values)
            {
                _cbMode.Items.Add(mi);

            }
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
                    SaveFormState();
                }

                this.DialogResult = DialogResult.OK;
            }
            catch(Exception ex)
            {
                HandleException(ex);
            }
        }

        private void LoginForm_Shown(object sender, EventArgs e)
        {
            try
            {
                LoadFormState();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void _cbMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                _txtUserId.Enabled = _cbMode.SelectedItem == _modeHdiGateway;
                _txtPasswd.Enabled = _cbMode.SelectedItem == _modeHdiGateway;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void LoadFormState()
        {
            // Load Server Strings
            using (RegistryKey rk = Registry.CurrentUser.CreateSubKey(RegPath_Connections))
            {
                foreach (string name in rk.GetValueNames())
                {
                    _cbServer.Items.Add(rk.GetValue(name));
                }
            }

            // Load Last Connection info
            using (RegistryKey rk = Registry.CurrentUser.CreateSubKey(RegPath_LastConnection))
            {
                object oLastMode = rk.GetValue(LastMode);
                ModeItem modeItem = null;
                
                // Server string
                _cbServer.SelectedItem = rk.GetValue(LastServer);
                if(null == _cbServer.SelectedItem && _cbServer.Items.Count > 0)
                {
                    _cbServer.SelectedItem = _cbServer.Items[0];
                }

                // Mode?
                if(null != oLastMode && _modeOptions.ContainsKey(oLastMode.ToString()))
                {
                    // Set Mode
                    modeItem = _modeOptions[oLastMode.ToString()];
                    _cbMode.SelectedItem = modeItem;
                    if (modeItem.Key == PhoenixConnectionModeStr.HdiGateway)
                    {
                        // For HDI, set user id.
                        _txtUserId.Text = rk.GetValue(LastUsername).ToString();
                        if (_txtUserId.Text.Length > 0)
                        {
                            _txtPasswd.Focus();
                        }
                    }
                }
            }
        }

        private void SaveFormState()
        {
            using (RegistryKey rk = Registry.CurrentUser.CreateSubKey(RegPath_Connections))
            {
                if (!_cbServer.Items.Contains(this.Server))
                {
                    string strName = string.Format("Connection{0}", rk.ValueCount);
                    rk.SetValue(strName, this.Server);
                }
            }

            using (RegistryKey rk = Registry.CurrentUser.CreateSubKey(RegPath_LastConnection))
            {
                rk.SetValue(LastServer, this.Server);
                rk.SetValue(LastMode, (_cbMode.SelectedItem as ModeItem).Key);
                rk.SetValue(LastUsername, this._txtUserId.Text);
            }
        }

        private void HandleException(Exception ex)
        {
            string msg = ex.Message;
            if (ex.InnerException != null)
            {
                msg = string.Format("{0}\r\n\r\n{1}", ex.Message, ex.InnerException.Message);
            }

            System.Diagnostics.Trace.WriteLine(ex);

            MessageBox.Show(this, msg, ex.GetType().Name);
        }

    }
}
