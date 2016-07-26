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
            GarudaUtilCmdLineArgs cmdLine = new GarudaUtilCmdLineArgs(args);

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

                    // Command Line mode
                    using (IDbConnection phConn = new PhoenixConnection())
                    {
                        phConn.ConnectionString = cmdLine.ConnectionString;

                        phConn.Open();

                        (phConn as PhoenixConnection).SystemTables();

                        using (IDbCommand cmd = phConn.CreateCommand())
                        {
                            cmd.CommandText = "DROP TABLE IF EXISTS GARUDATEST";
                            cmd.ExecuteNonQuery();

                            cmd.CommandText = "CREATE TABLE IF NOT EXISTS GARUDATEST (ID BIGINT PRIMARY KEY, AircraftIcaoNumber varchar(16), MyInt INTEGER, MyUint UNSIGNED_INT, MyUlong UNSIGNED_LONG, MyTingInt TINYINT, MyTime TIME, MyDate DATE, MyTimestamp TIMESTAMP, MyUnsignedTime UNSIGNED_TIME, MyFloat FLOAT, MyBinary BINARY(16), MyArray INTEGER[2] )";
                            cmd.ExecuteNonQuery();

                            bool bCreateSequence = true;
                            cmd.CommandText = "SELECT sequence_schema, sequence_name, start_with, increment_by, cache_size FROM SYSTEM.\"SEQUENCE\""; //  WHERE sequence_schema = 'garuda' AND sequence_name='testsequence'
                            using (IDataReader reader = cmd.ExecuteReader())
                            {
                                while(reader.Read())
                                {
                                    if (reader.GetString(1).Equals("testsequence", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        bCreateSequence = false;
                                        break;
                                    }

                                }
                            }

                            if(bCreateSequence)
                            {
                                cmd.CommandText = "CREATE SEQUENCE garuda.testsequence";
                                cmd.ExecuteNonQuery();
                            }
                            
                            // Insert a bunch of data...
                            for(int i = 0; i < 10; i++)
                            {
                                cmd.CommandText = string.Format("UPSERT INTO GARUDATEST (ID, AircraftIcaoNumber, MyInt, MyUint, MyUlong, MyTingInt, MyTime, MyDate, MyTimestamp, MyUnsignedTime, MyFloat) VALUES (NEXT VALUE FOR garuda.testsequence, 'N{0}', 5, 4, 3, 2, CURRENT_TIME(), CURRENT_DATE(), '2016-07-25 22:28:00',  CURRENT_TIME(), 1.2 / .4)", DateTime.Now.ToString("hmmss"));
                                cmd.ExecuteNonQuery();
                            }

                            cmd.CommandText = "SELECT * FROM GARUDATEST";
                            using (IDataReader reader = cmd.ExecuteReader())
                            {
                                while(reader.Read())
                                {
                                    for(int i = 0; i < reader.FieldCount; i++)
                                    {
                                        Console.WriteLine(string.Format("{0}: {1} ({2})", reader.GetName(i), reader.GetValue(i), reader.GetDataTypeName(i)));
                                    }
                                }
                            }
                        }

                        
                    }
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);

                if(cmdLine.ShowException)
                {
                    MessageBox.Show(ex.ToString(), ex.GetType().ToString());
                }
                
            }

        }
    }
}
