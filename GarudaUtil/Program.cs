using Garuda.Data;
using PhoenixSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GarudaUtil
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool showException = true;

            try
            {
                if (args.Length == 0)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                }
                else
                {
                    ClusterCredentials creds = new ClusterCredentials(new Uri("https://ec2-50-112-194-207.us-west-2.compute.amazonaws.com/"),
                            "",
                            "");

                    // Command Line mode
                    using (IDbConnection phConn = new PhoenixConnection())
                    {
                        phConn.ConnectionString = string.Format("Server={0}", args[0]);
                        //phConn.Options.AlternativeHost = "ec2-50-112-194-207.us-west-2.compute.amazonaws.com";
                        //phConn.Credentials = null;

                        phConn.Open();

                        using (IDbCommand cmd = phConn.CreateCommand())
                        {
                            cmd.CommandText = "SYSTEM TABLE";
                            using (IDataReader reader = cmd.ExecuteReader())
                            {
                                while(reader.Read())
                                {
                                    for(int i = 0; i < reader.FieldCount; i++)
                                    {
                                        Console.WriteLine(string.Format("{0}: {1}", reader.GetName(i), reader.GetValue(i)));
                                    }
                                }
                            }
                        }

                        //util.SystemTables();
                    }





                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);

                if(showException)
                {
                    MessageBox.Show(ex.ToString(), ex.GetType().ToString());
                }
                
            }

        }
    }
}
