using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Collections.Generic;

namespace Garuda.Data.Test
{
    [TestClass]
    public class PhoenixUnitTest
    {
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Tests the construction of PhoenixConnection as well
        /// as setting of the connection string property and 
        /// opening/disposal of the connection.
        /// </summary>
        [TestMethod]
        public void ConnectionBasicOpenDisposeTest()
        {
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                Assert.AreEqual<ConnectionState>(ConnectionState.Open, c.State);
            }
        }

        /// <summary>
        /// Tests the construction of PhoenixConnection passing the
        /// connection string as a parameter and 
        /// opening/disposal of the connection.
        /// </summary>
        [TestMethod]
        public void ConstructorConnectionBasicOpenDisposeTest()
        {
            using (PhoenixConnection c = new PhoenixConnection(this.ConnectionString()))
            {
                c.Open();

                Assert.AreEqual<ConnectionState>(ConnectionState.Open, c.State);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectionStringNullTest()
        {
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = null;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorConnectionStringNullTest()
        {
            using (PhoenixConnection c = new PhoenixConnection(null))
            {
                // Shouldn't get here anyway
                Assert.AreEqual<ConnectionState>(ConnectionState.Closed, c.State);
            }
        }

        [TestMethod]
        public void ConstructorConnectionStringMalformedTest()
        {
            string cs = "Server=;UserID=";
            using (PhoenixConnection c = new PhoenixConnection(cs))
            {
                Assert.AreEqual<ConnectionState>(ConnectionState.Closed, c.State);
            }
        }

        [TestMethod]
        public void CommandCreateDisposeTest()
        {
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                using (IDbCommand cmd = c.CreateCommand())
                {
                    // Do nothing, just displose
                }
            }
        }

        [TestMethod]
        public void TransactionRollbackTest()
        {
            using (IDbConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                ReCreateTestTableIfNotExists(c);

                using (IDbCommand cmd = c.CreateCommand())
                using (IDbTransaction tx = c.BeginTransaction())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = string.Format("UPSERT INTO GARUDATEST (ID, AircraftIcaoNumber, MyInt, MyUint, MyUlong, MyTingInt, MyTime, MyDate, MyTimestamp, MyUnsignedTime, MyFloat) VALUES (NEXT VALUE FOR garuda.testsequence, 'NINTX1', 5, 4, 3, 2, CURRENT_TIME(), CURRENT_DATE(), '2016-07-25 22:28:00',  CURRENT_TIME(), 1.2 / .4)");
                    cmd.ExecuteNonQuery();
                    tx.Rollback();

                }

                // Query for data... should get zero rows.
                Assert.AreEqual(0, QueryAllRows(c));
            }
        }

        [TestMethod]
        public void TransactionCommitTest()
        {
            using (IDbConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                ReCreateTestTableIfNotExists(c);

                using (IDbCommand cmd = c.CreateCommand())
                using (IDbTransaction tx = c.BeginTransaction())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = string.Format("UPSERT INTO GARUDATEST (ID, AircraftIcaoNumber, MyInt, MyUint, MyUlong, MyTingInt, MyTime, MyDate, MyTimestamp, MyUnsignedTime, MyFloat) VALUES (NEXT VALUE FOR garuda.testsequence, 'NINTX1', 5, 4, 3, 2, CURRENT_TIME(), CURRENT_DATE(), '2016-07-25 22:28:00',  CURRENT_TIME(), 1.2 / .4)");
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }

                Assert.AreEqual(1, QueryAllRows(c));
            }
        }

        [TestMethod]
        public void TransactionReuseCommitTest()
        {
            int toInsert = 10;

            using (IDbConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                ReCreateTestTableIfNotExists(c);

                using (IDbTransaction tx = c.BeginTransaction())
                {
                    for (int i = 0; i < toInsert; i++)
                    {
                        using (IDbCommand cmd = c.CreateCommand())
                        {
                            cmd.Transaction = tx;
                            cmd.CommandText = string.Format("UPSERT INTO GARUDATEST (ID, AircraftIcaoNumber, MyInt, MyUint, MyUlong, MyTingInt, MyTime, MyDate, MyTimestamp, MyUnsignedTime, MyFloat) VALUES (NEXT VALUE FOR garuda.testsequence, 'NINTX1', 5, 4, 3, 2, CURRENT_TIME(), CURRENT_DATE(), '2016-07-25 22:28:00',  CURRENT_TIME(), 1.2 / .4)");
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }

                Assert.AreEqual(toInsert, QueryAllRows(c));
            }
        }

        [TestMethod]
        public void CommandPrepareTest()
        {
            int toInsert = 10;

            using (IDbConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                ReCreateTestTableIfNotExists(c);

                using (IDbTransaction tx = c.BeginTransaction())
                {
                    using (IDbCommand cmd = c.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = string.Format("UPSERT INTO GARUDATEST (ID, AircraftIcaoNumber, MyInt, MyUint, MyUlong, MyTingInt, MyTime, MyDate, MyTimestamp, MyUnsignedTime, MyFloat) VALUES (NEXT VALUE FOR garuda.testsequence, :1, 12, 14, 87, 45, CURRENT_TIME(), CURRENT_DATE(), '2016-07-25 22:28:00',  CURRENT_TIME(), 1.2 / .4)");
                        cmd.Prepare();

                        for (int i = 0; i < toInsert; i++)
                        {
                            // Create a parameter used in the query
                            var p1 = cmd.CreateParameter();
                            p1.Value = string.Format("N{0}", DateTime.Now.ToString("hmmss"));
                            cmd.Parameters.Add(p1);

                            cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                    }

                    tx.Commit();
                }

                Assert.AreEqual(toInsert, QueryAllRows(c));
            }
        }

        [TestMethod]
        public void CommandPrepareWith2StringParamsTest()
        {
            int rowsToInsert = 10;
            List<Func<object>> pFuncs = new List<Func<object>>();
            pFuncs.Add(() => string.Format("N{0}", DateTime.Now.ToString("hmmss")));
            pFuncs.Add(() => "2016-07-25 22:28:00");

            PreparedCmdParameterTest(rowsToInsert,
                "UPSERT INTO GARUDATEST (ID, AircraftIcaoNumber, MyInt, MyUint, MyUlong, MyTingInt, MyTime, MyDate, MyTimestamp, MyUnsignedTime, MyFloat) VALUES (NEXT VALUE FOR garuda.testsequence, :1, 12, 14, 87, 45, CURRENT_TIME(), CURRENT_DATE(), :2,  CURRENT_TIME(), 1.2 / .4)",
                pFuncs);
        }

        [TestMethod]
        public void CommandPrepareWith1String1IntParamsTest()
        {
            int rowsToInsert = 10;
            List<Func<object>> pFuncs = new List<Func<object>>();
            pFuncs.Add(() => string.Format("N{0}", DateTime.Now.ToString("hmmss")));
            pFuncs.Add(() => DateTime.Now.Millisecond);

            PreparedCmdParameterTest(rowsToInsert,
                "UPSERT INTO GARUDATEST (ID, AircraftIcaoNumber, MyInt, MyUint, MyUlong, MyTingInt, MyTime, MyDate, MyTimestamp, MyUnsignedTime, MyFloat) VALUES (NEXT VALUE FOR garuda.testsequence, :1, :2, 14, 87, 45, CURRENT_TIME(), CURRENT_DATE(), '2016-07-25 22:28:00',  CURRENT_TIME(), 1.2 / .4)",
                pFuncs);
        }

        [TestMethod]
        public void CommandPrepareWith1String1UlongParamsTest()
        {
            int rowsToInsert = 10;
            List<Func<object>> pFuncs = new List<Func<object>>();
            pFuncs.Add(() => string.Format("N{0}", DateTime.Now.ToString("hmmss")));
            pFuncs.Add(() => DateTime.Now.Ticks);

            PreparedCmdParameterTest(rowsToInsert,
                "UPSERT INTO GARUDATEST (ID, AircraftIcaoNumber, MyInt, MyUint, MyUlong, MyTingInt, MyTime, MyDate, MyTimestamp, MyUnsignedTime, MyFloat) VALUES (NEXT VALUE FOR garuda.testsequence, :1, 19, 14, :2, 45, CURRENT_TIME(), CURRENT_DATE(), '2016-07-25 22:28:00',  CURRENT_TIME(), 1.2 / .4)",
                pFuncs);
        }


        [TestMethod]
        public void CommandPrepareWith1String1FloatParamsTest()
        {
            int rowsToInsert = 10;
            List<Func<object>> pFuncs = new List<Func<object>>();
            pFuncs.Add(() => string.Format("N{0}", DateTime.Now.ToString("hmmss")));
            pFuncs.Add(() => Convert.ToSingle(0.2 / 0.14));

            PreparedCmdParameterTest(rowsToInsert, 
                "UPSERT INTO GARUDATEST (ID, AircraftIcaoNumber, MyInt, MyUint, MyUlong, MyTingInt, MyTime, MyDate, MyTimestamp, MyUnsignedTime, MyFloat) VALUES (NEXT VALUE FOR garuda.testsequence, :1, 19, 14, 87, 45, CURRENT_TIME(), CURRENT_DATE(), '2016-07-25 22:28:00',  CURRENT_TIME(), :2)",
                pFuncs);
        }

        private void PreparedCmdParameterTest(int rowsToInsert, string sql, List<Func<object>> pFunc)
        {
            using (IDbConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                ReCreateTestTableIfNotExists(c);

                using (IDbTransaction tx = c.BeginTransaction())
                {
                    using (IDbCommand cmd = c.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = sql;
                        cmd.Prepare();

                        for (int i = 0; i < rowsToInsert; i++)
                        {
                            // Parameters loop
                            foreach(var pf in pFunc)
                            {
                                // Create a parameter used in the query
                                var p = cmd.CreateParameter();
                                p.Value = pf();
                                cmd.Parameters.Add(p);
                            }

                            cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                    }

                    tx.Commit();
                }

                Assert.AreEqual(rowsToInsert, QueryAllRows(c));
            }
        }

        private long QueryAllRows(IDbConnection c)
        {
            long recCount = 0;
            object oVal = null;

            // Query for data... should get one rows.
            using (IDbCommand cmd = c.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM GARUDATEST";
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        recCount++;

                        for(int i = 0; i < reader.FieldCount; i++)
                        {
                            oVal = reader.GetValue(i);
                            this.TestContext.WriteLine(string.Format("{0}: {1} ({2})", reader.GetName(i), oVal, reader.GetDataTypeName(i)));
                        }
                    }
                }
            }

            return recCount;
        }

        private static void ReCreateTestTableIfNotExists(IDbConnection phConn)
        {
            using (IDbCommand cmd = phConn.CreateCommand())
            {
                cmd.CommandText = "DROP TABLE IF EXISTS GARUDATEST";
                cmd.ExecuteNonQuery();
            }

            using (IDbCommand cmd = phConn.CreateCommand())
            {
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS GARUDATEST (ID BIGINT PRIMARY KEY, AircraftIcaoNumber varchar(16), MyInt INTEGER, MyUint UNSIGNED_INT, MyUlong UNSIGNED_LONG, MyTingInt TINYINT, MyTime TIME, MyDate DATE, MyTimestamp TIMESTAMP, MyUnsignedTime UNSIGNED_TIME, MyFloat FLOAT, MyBinary BINARY(16), MyArray INTEGER[2] )";
                cmd.ExecuteNonQuery();
            }

            bool bCreateSequence = true;
            using (IDbCommand cmd = phConn.CreateCommand())
            {

                cmd.CommandText = "SELECT sequence_schema, sequence_name, start_with, increment_by, cache_size FROM SYSTEM.\"SEQUENCE\""; //  WHERE sequence_schema = 'garuda' AND sequence_name='testsequence'
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.GetString(1).Equals("testsequence", StringComparison.InvariantCultureIgnoreCase))
                        {
                            bCreateSequence = false;
                            break;
                        }

                    }
                }
            }

            if (bCreateSequence)
            {
                using (IDbCommand cmd = phConn.CreateCommand())
                {
                    cmd.CommandText = "CREATE SEQUENCE garuda.testsequence";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private string ConnectionString()
        {
            return System.IO.File.ReadAllText(@"..\..\..\GarudaUtil\myconnection.txt");
        }
    }

}
