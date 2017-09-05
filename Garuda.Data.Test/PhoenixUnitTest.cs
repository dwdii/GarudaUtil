﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Data.SqlClient;
using System.Threading.Tasks;

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

                TestContext.WriteLine("ConnectionId: {0}", c.ConnectionId);

                Assert.AreEqual<ConnectionState>(ConnectionState.Open, c.State);
                Assert.IsNotNull(c.ConnectionId);
                Assert.IsNotNull(c.ConnectionString);
            }
        }

        /// <summary>
        /// Tests the construction of PhoenixConnection passing the
        /// connection string as a parameter and 
        /// opening/disposal of the connection.
        /// </summary>
        [TestMethod]
        public void ConnectionConstructorBasicOpenDisposeTest()
        {
            using (PhoenixConnection c = new PhoenixConnection(this.ConnectionString()))
            {
                c.Open();

                TestContext.WriteLine("ConnectionId: {0}", c.ConnectionId);

                Assert.AreEqual<ConnectionState>(ConnectionState.Open, c.State);
                Assert.IsNotNull(c.ConnectionId);
                Assert.IsNotNull(c.ConnectionString);
            }
        }

        [TestMethod]
        public void ConnectionConstructorBasicOpenCloseTest()
        {
            using (PhoenixConnection c = new PhoenixConnection(this.ConnectionString()))
            {
                c.Open();

                TestContext.WriteLine("ConnectionId: {0}", c.ConnectionId);

                Assert.AreEqual<ConnectionState>(ConnectionState.Open, c.State);
                Assert.IsNotNull(c.ConnectionId);
                Assert.IsNotNull(c.ConnectionString);

                c.Close();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void ConnectionChangeDatabase()
        {
            using (PhoenixConnection c = new PhoenixConnection(this.ConnectionString()))
            {
                c.ChangeDatabase("whatever");
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
        public void CommandExecuteNonQueryElapsedGreaterThanZero()
        {
            using (IDbConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                ReCreateTestTableIfNotExists(c);

                using (IDbCommand cmd = c.CreateCommand())
                {
                    cmd.CommandText = string.Format("UPSERT INTO GARUDATEST (ID, AircraftIcaoNumber, MyInt, MyUint, MyUlong, MyTingInt, MyTime, MyDate, MyTimestamp, MyUnsignedTime, MyFloat) VALUES (NEXT VALUE FOR garuda.testsequence, 'NINTX1', 5, 4, 3, 2, CURRENT_TIME(), CURRENT_DATE(), '2016-07-25 22:28:00',  CURRENT_TIME(), 1.2 / .4)");
                    cmd.ExecuteNonQuery();

                    // Confirm PhoenixCommand.Elapsed is working a bit.
                    PhoenixCommand phCmd = cmd as PhoenixCommand;
                    Assert.IsNotNull(phCmd);
                    Assert.IsNotNull(phCmd.Elapsed);
                    Assert.AreNotEqual(0, phCmd.Elapsed.TotalMilliseconds, nameof(phCmd.Elapsed.TotalMilliseconds));
                    this.TestContext.WriteLine("PhoenixCommand.Elapsed: {0}", phCmd.Elapsed);

                }

                Assert.AreEqual(1, QueryAllRows(c));
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
        public void CommandPrepareCreateIndexBigTableTest()
        {
            using (IDbConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                CreateBigTestTableIfNotExists(c, false);

                // DROP INDEX NDX_BIGTBL_TEST ON BIGTABLE
                DropIndexIfExists(c, "NDX_BigTbl_Test", "bigtable");

                using (IDbTransaction tx = c.BeginTransaction())
                {
                    using (IDbCommand cmd = c.CreateCommand())
                    {
                        cmd.Transaction = tx;
                        cmd.CommandText = string.Format("CREATE INDEX NDX_BigTbl_Test ON bigtable (LruFlightKey)");
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }

                // Should really verify the index was created.
                Assert.IsTrue(true);
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

        //[TestMethod]
        //public void CommandPrepareWith1String1UlongMaxParamsTest()
        //{
        //    int rowsToInsert = 10;
        //    List<Func<object>> pFuncs = new List<Func<object>>();
        //    pFuncs.Add(() => string.Format("N{0}", DateTime.Now.ToString("hmmss")));
        //    pFuncs.Add(() => ulong.MaxValue);

        //    PreparedCmdParameterTest(rowsToInsert,
        //        "UPSERT INTO GARUDATEST (ID, AircraftIcaoNumber, MyInt, MyUint, MyUlong, MyTingInt, MyTime, MyDate, MyTimestamp, MyUnsignedTime, MyFloat) VALUES (NEXT VALUE FOR garuda.testsequence, :1, 19, 14, :2, 45, CURRENT_TIME(), CURRENT_DATE(), '2016-07-25 22:28:00',  CURRENT_TIME(), 1.2 / .4)",
        //        pFuncs);
        //}

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

        [TestMethod]
        public void CommandPrepareWithDateTimeParamsTest()
        {
            int rowsToInsert = 10;
            List<Func<object>> pFuncs = new List<Func<object>>();
            pFuncs.Add(() => DateTime.Now);

            PreparedCmdParameterTest(rowsToInsert,
                "UPSERT INTO GARUDATEST (ID, AircraftIcaoNumber, MyInt, MyUint, MyUlong, MyTingInt, MyTime, MyDate, MyTimestamp, MyUnsignedTime, MyFloat) VALUES (NEXT VALUE FOR garuda.testsequence, 'N123JB', 19, 14, 87, 45, CURRENT_TIME(), CURRENT_DATE(), :1,  CURRENT_TIME(), 3.14159)",
                pFuncs);
        }

        [TestMethod]
        public void CommandPrepareWithParamsStringIntUintUlongShortFloatTest()
        {
            int rowsToInsert = 10;
            List<Func<object>> pFuncs = new List<Func<object>>();
            pFuncs.Add(() => string.Format("N{0}", DateTime.Now.ToString("hmmss")));
            pFuncs.Add(() => DateTime.Now.Millisecond);
            pFuncs.Add(() => Convert.ToUInt32(DateTime.Now.Millisecond));
            pFuncs.Add(() => Convert.ToUInt64(DateTime.Now.Ticks));
            pFuncs.Add(() => Convert.ToInt16(DateTime.Now.Second));
            pFuncs.Add(() => Convert.ToSingle(0.2 / 0.14));

            PreparedCmdParameterTest(rowsToInsert,
                "UPSERT INTO GARUDATEST (ID, AircraftIcaoNumber, MyInt, MyUint, MyUlong, MyTingInt, MyTime, MyDate, MyTimestamp, MyUnsignedTime, MyFloat) VALUES (NEXT VALUE FOR garuda.testsequence, :1, :2, :3, :4, :5, CURRENT_TIME(), CURRENT_DATE(), '2016-07-25 22:28:00',  CURRENT_TIME(), :6)",
                pFuncs);
        }

        [TestMethod]
        public void ExecuteNonQueryUpsert1KBigTable()
        {
            int rowsToInsert = 1000;
            List<Func<object>> pFuncs = new List<Func<object>>();
            pFuncs.Add(() => string.Format("N{0}", DateTime.Now.ToString("hmmss")));
            pFuncs.Add(() => Guid.NewGuid().ToString());
            pFuncs.Add(() => DateTime.Now);

            using (IDbConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();
                CreateBigTestTableIfNotExists(c, false);
            }

            PreparedCmdParameterTest(rowsToInsert,
                "UPSERT INTO bigtable (ID, AircraftIcaoNumber, LruFlightKey, MyTimestamp) VALUES (NEXT VALUE FOR garuda.bigtableSequence, :1, :2, :3)",
                pFuncs, false);
        }

        [TestMethod]
        public void ExecuteQueryBigTable()
        {
            Stopwatch sw = new Stopwatch();
            using (IDbConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                CreateBigTestTableIfNotExists(c, false);

                // Query the table and measure performance
                sw.Start();
                long rows = QueryAllRows(c, "bigtable", 10);
                sw.Stop();
                WriteQueryRowsPerf(rows, sw.ElapsedMilliseconds);

                // How many rows did we get back?
                this.TestContext.WriteLine("Queried Rows: {0}", rows);

                // More than zero?
                Assert.IsTrue(rows > 0);
            }
        }

        [TestMethod]
        public void BulkCopyTest1()
        {
            Stopwatch sw = new Stopwatch();
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                CreateBulkCopyTableIfNotExists(c, false);

                PhoenixBulkCopy bc = new PhoenixBulkCopy(c);
                DataTable dt = ConvertCSVtoDataTable(System.Configuration.ConfigurationManager.AppSettings["BulkCopyCsvTestFile"]);

                // Query the table and measure performance
                sw.Start();

                bc.DestinationTableName = "BulkCopyTest";
                bc.ColumnMappings.Add("ID", new PhoenixBulkCopyColumnMapping("NEXT VALUE FOR garuda.BulkCopyTestSequence"));
                bc.BatchSize = 100;
                bc.WriteToServer(dt);

                sw.Stop();
                WriteBulkCopyPerf(dt.Rows.Count, sw.ElapsedMilliseconds);

                // How many rows did we get back?
                this.TestContext.WriteLine("Bulk Copy Rows: {0}", dt.Rows.Count);
                this.TestContext.WriteLine("Bulk Copy Time: {0}ms", sw.ElapsedMilliseconds);

                // More than zero?
                Assert.IsTrue(dt.Rows.Count > 0);
            }
        }

        [TestMethod]
        public void BulkCopyTest2_Salted3Table()
        {
            Stopwatch sw = new Stopwatch();
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                string tableName = CreateBulkCopyTableIfNotExists(c, false, 3);
                string nextValueForSequence = string.Format("NEXT VALUE FOR garuda.{0}", SequenceNameForTable(tableName));
                PhoenixBulkCopy bc = new PhoenixBulkCopy(c);
                DataTable dt = ConvertCSVtoDataTable(System.Configuration.ConfigurationManager.AppSettings["BulkCopyCsvTestFile"]);

                // Query the table and measure performance
                sw.Start();

                bc.DestinationTableName = tableName;
                bc.ColumnMappings.Add("ID", new PhoenixBulkCopyColumnMapping(nextValueForSequence));
                bc.BatchSize = 100;
                bc.WriteToServer(dt);

                sw.Stop();
                WriteBulkCopyPerf(dt.Rows.Count, sw.ElapsedMilliseconds, "BulkCopySalted3PerfFile");

                // How many rows did we get back?
                this.TestContext.WriteLine("Bulk Copy Rows: {0}", dt.Rows.Count);
                this.TestContext.WriteLine("Bulk Copy Time: {0}ms", sw.ElapsedMilliseconds);

                // More than zero?
                Assert.IsTrue(dt.Rows.Count > 0);
            }
        }

        [TestMethod]
        public void DataTableLoadFromPhoenixDataReader()
        {
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                using (IDbCommand cmd = c.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM BIGTABLE";
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(dr);

                        Assert.IsTrue(dt.Rows.Count > 0);
                        Assert.IsTrue(dt.Columns.Count > 0);
                    }
                }
            }
        }

        [TestMethod]
        public void DataReaderItemInt32Indexer()
        {
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                using (IDbCommand cmd = c.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM BIGTABLE";
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        while(dr.Read())
                        {
                            for(int i = 0; i < dr.FieldCount; i++)
                            {
                                object o = dr[i];

                                object o2 = dr.GetValue(i);

                                Assert.AreEqual(o2, o, "Mismatch on field {0}", i);
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void DataReaderItemStringIndexer()
        {
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                using (IDbCommand cmd = c.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM BIGTABLE";
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            for (int i = 0; i < dr.FieldCount; i++)
                            {
                                string name = dr.GetName(i);

                                object o = dr[name];

                                object o2 = dr.GetValue(i);

                                Assert.AreEqual(o2, o, "Mismatch on field {0}", name);
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void DataReaderGetSchemaTable()
        {
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                using (IDbCommand cmd = c.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM BIGTABLE";
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        DataTable dt = dr.GetSchemaTable();

                        Assert.IsTrue(dt.Columns.Count > 0);

                        DataColumn dcColName = dt.Columns["ColumnName"];
                        Assert.IsNotNull(dcColName, "ColumnName");

                        DataColumn dcColSize = dt.Columns["ColumnSize"];
                        Assert.IsNotNull(dcColSize, "ColumnSize");
                        Assert.AreEqual(dcColSize.DataType, typeof(int));
                        
                        DataColumn dcColOrdinal = dt.Columns["ColumnOrdinal"];
                        Assert.IsNotNull(dcColOrdinal, "ColumnOrdinal");
                        Assert.AreEqual(dcColOrdinal.DataType, typeof(int));

                        DataColumn dcNullable = dt.Columns["AllowDBNull"];
                        Assert.IsNotNull(dcNullable, "AllowDBNull");
                        Assert.AreEqual(dcNullable.DataType, typeof(bool));
                    }
                }
            }
        }

        [TestMethod]
        public void DataReaderGetFieldType()
        {
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                using (IDbCommand cmd = c.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM BIGTABLE";
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        dr.Read();

                        for(int i = 0; i < dr.FieldCount; i++)
                        {
                            Type t = dr.GetFieldType(i);
                            Assert.IsNotNull(t, "GetFieldType returned null!");
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void DataReaderFieldCountZero()
        {
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                using (IDbCommand cmd = c.CreateCommand())
                {
                    cmd.CommandText = "UPSERT INTO BIGTABLE (ID, MYTIMESTAMP) VALUES (502, NOW())";
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        Assert.AreEqual(0, dr.FieldCount);
                    }
                }
            }
        }

        [TestMethod]
        public void DataReaderHasRowsFalse()
        {
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                using (IDbCommand cmd = c.CreateCommand())
                {
                    cmd.CommandText = "UPSERT INTO BIGTABLE (ID, MYTIMESTAMP) VALUES (502, NOW())";
                    using (PhoenixDataReader dr = cmd.ExecuteReader() as PhoenixDataReader)
                    {
                        Assert.AreEqual(false, dr.HasRows);
                    }
                }
            }
        }

        [TestMethod]
        public void DataReaderRecordsAffectedOne()
        {
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                using (IDbCommand cmd = c.CreateCommand())
                {
                    cmd.CommandText = "UPSERT INTO BIGTABLE (ID, MYTIMESTAMP) VALUES (502, NOW())";
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        Assert.AreEqual(1, dr.RecordsAffected);
                    }
                }
            }
        }

        [TestMethod]
        public void ConnectionTablesDataTable()
        {
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                DataTable tables = c.GetTables();
                Assert.IsTrue(tables.Rows.Count > 0);
            }
        }

        [TestMethod]
        public void CommandExplain()
        {
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                using (PhoenixCommand cmd = new PhoenixCommand(c))
                {
                    cmd.CommandText = "SELECT * FROM BIGTABLE WHERE ID < 1000";

                    DataTable dt = cmd.Explain();

                    Assert.IsNotNull(dt);
                }

            }
        }

        [TestMethod]
        public void CommandExecuteScalar()
        {
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                using (PhoenixCommand cmd = new PhoenixCommand(c))
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM BIGTABLE WHERE ID < 1000";

                    object oVal = cmd.ExecuteScalar();

                    Assert.IsNotNull(oVal);
                    Assert.IsInstanceOfType(oVal, typeof(long));
                }

            }
        }


        [TestMethod]
        public void DataReaderGetValues()
        {
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                ReCreateTestTableIfNotExists(c, "DataReaderGetValuesTest",
                    "CREATE TABLE IF NOT EXISTS DataReaderGetValuesTest (ID BIGINT PRIMARY KEY, FirstCol varchar(16), SecondCol varchar(64))",
                    true, true);

                List<Func<object>> pFuncs = new List<Func<object>>();
                pFuncs.Add(() => "First");
                pFuncs.Add(() => "Second");

                PreparedCmdParameterTest(2,
                "UPSERT INTO DataReaderGetValuesTest (ID, FirstCol, SecondCol) VALUES (NEXT VALUE FOR garuda.DataReaderGetValuesTestSequence, :1, :2)",
                pFuncs, false);

                using (IDbCommand cmd = c.CreateCommand())
                {
                    
                    cmd.CommandText = "SELECT * FROM DataReaderGetValuesTest";
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        object[] values = new object[dr.FieldCount];

                        while(dr.Read())
                        {
                            dr.GetValues(values);
                            Assert.AreEqual("First", values[1]);
                            Assert.AreEqual("Second", values[2]);
                        }

                    }
                }
            }
        }

        [TestMethod]
        public void MetaDataGarudaPhoenixTableConstructor()
        {
            string expectedTable = "BIGTABLE";
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();
               
                DataTable dt = c.GetTables();
                foreach(DataRow row in dt.Rows)
                {
                    var tbl = new Garuda.Data.MetaData.GarudaPhoenixTable(row);
                    if(row["TABLE_NAME"].ToString() == expectedTable)
                    {
                        Assert.IsNotNull(tbl.Row);
                        Assert.IsInstanceOfType(tbl.Row, typeof(DataRow));

                        Assert.IsNotNull(tbl.Name);
                        Assert.AreEqual(expectedTable, tbl.Name);

                        Assert.IsNotNull(tbl.FullName);
                        Assert.AreEqual(expectedTable, tbl.FullName);

                        Assert.IsNotNull(tbl.Schema);
                        Assert.AreEqual(string.Empty, tbl.Schema);
                        break;
                    }
                }
            }
        }

        [TestMethod]
        public void MetaDataGarudaPhoenixTableGetColumns()
        {
            string expectedTable = "BIGTABLE";
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                DataTable dt = c.GetTables();
                foreach (DataRow row in dt.Rows)
                {
                    if (row["TABLE_NAME"].ToString() == expectedTable)
                    {
                        var tbl = new Garuda.Data.MetaData.GarudaPhoenixTable(row);

                        DataTable dtCols = tbl.GetColumns(c);
                        Assert.IsNotNull(dtCols);
                        Assert.IsNotNull(dtCols.Rows);
                        Assert.IsTrue(dtCols.Rows.Count > 0);

                        Assert.IsNotNull(dtCols.Columns);
                        break;
                    }
                }
            }
        }


        [TestMethod]
        public void MetaDataGarudaPhoenixTableGetIndexes()
        {
            string expectedTable = "BIGTABLE";
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                DataTable dt = c.GetTables();
                foreach (DataRow row in dt.Rows)
                {
                    if (row["TABLE_NAME"].ToString() == expectedTable)
                    {
                        var tbl = new Garuda.Data.MetaData.GarudaPhoenixTable(row);

                        DataTable dtCols = tbl.GetIndexes(c);
                        Assert.IsNotNull(dtCols);
                        Assert.IsNotNull(dtCols.Rows);
                        Assert.IsTrue(dtCols.Rows.Count > 0);

                        Assert.IsNotNull(dtCols.Columns);
                        break;
                    }
                }
            }
        }

        [TestMethod]
        public void MetaDataGarudaPhoenixTableGenerateUpsertStatement()
        {
            string expectedTable = "BIGTABLE";
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                DataTable dt = c.GetTables();
                foreach (DataRow row in dt.Rows)
                {
                    if (row["TABLE_NAME"].ToString() == expectedTable)
                    {
                        var tbl = new Garuda.Data.MetaData.GarudaPhoenixTable(row);

                        Task<string> tUpsert = tbl.GenerateUpsertStatementAsync(c);
                        tUpsert.Wait();

                        Assert.IsNotNull(tUpsert.Result);
                        Assert.IsTrue(tUpsert.Result.StartsWith("UPSERT INTO"));
                        TestContext.WriteLine(tUpsert.Result);
                        break;
                    }
                }
            }
        }

        [TestMethod]
        public void CollectionItemGet()
        {
            PhoenixParameterCollection c = new PhoenixParameterCollection();
            PhoenixParameter pp = new PhoenixParameter();
            string expectedValue = "test";
            string paramName = "1";

            pp.ParameterName = paramName;
            pp.Value = expectedValue;
            c.Add(pp);

            PhoenixParameter pp1 = c[paramName] as PhoenixParameter;

            Assert.IsNotNull(pp1);
            Assert.AreEqual(expectedValue, pp1.Value);
        }

        [TestMethod]
        public void CollectionItemSet()
        {
            PhoenixParameterCollection c = new PhoenixParameterCollection();
            PhoenixParameter pp = new PhoenixParameter();
            string expectedValue = "test";
            string expectedValue2 = "aDiffValue";
            string paramName = "1";

            pp.ParameterName = paramName;
            pp.Value = expectedValue;
            c.Add(pp);

            c[paramName] = expectedValue2;

            Assert.IsNotNull(pp);
            Assert.AreEqual(expectedValue2, pp.Value);
        }


        #region Private Methods

        /// <summary>
        /// Care of: http://stackoverflow.com/questions/1050112/how-to-read-a-csv-file-into-a-net-datatable
        /// </summary>
        /// <param name="strFilePath"></param>
        /// <returns></returns>
        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    dt.Rows.Add(dr);
                }

            }


            return dt;
        }

        private void WriteQueryRowsPerf(long rows, long milliseconds)
        {
            string file = System.Configuration.ConfigurationManager.AppSettings["QueryRowsPerfFile"];
            if(!File.Exists(file))
            {
                File.AppendAllText(file, "Timestamp,Rows,Duration(ms)\r\n");
            }

            File.AppendAllText(file, string.Format("{0},{1},{2}\r\n", DateTime.Now.ToString(), rows, milliseconds));
        }

        private void WriteBulkCopyPerf(long rows, long milliseconds, string filenameSetting = "BulkCopyPerfFile")
        {
            string file = System.Configuration.ConfigurationManager.AppSettings[filenameSetting];
            if (!File.Exists(file))
            {
                File.AppendAllText(file, "Timestamp,Rows,Duration(ms)\r\n");
            }

            File.AppendAllText(file, string.Format("{0},{1},{2}\r\n", DateTime.Now.ToString(), rows, milliseconds));
        }

        private void PreparedCmdParameterTest(int rowsToInsert, string sql, List<Func<object>> pFunc, bool assertTotalRows = true)
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

                if(assertTotalRows)
                {
                    Assert.AreEqual(rowsToInsert, QueryAllRows(c));
                }
                
            }
        }


        private long QueryAllRows(IDbConnection c)
        {
            return QueryAllRows(c, "GARUDATEST", int.MaxValue);
        }

        private long QueryAllRows(IDbConnection c, string table, int logResults)
        {
            long recCount = 0;
            StringBuilder line = new StringBuilder();

            // Query for data... should get one rows.
            using (IDbCommand cmd = c.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM {0}", table);
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    #region Log Column Headers
                    if (logResults > 0)
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                           if(i > 0)
                            {
                                line.Append(",");
                            }
                            line.AppendFormat("{0} ({1})", reader.GetName(i), reader.GetDataTypeName(i)); 
                        }
                        this.TestContext.WriteLine(line.ToString());
                        line.Clear();
                    }
                    #endregion

                    while (reader.Read())
                    {
                        recCount++;

                        #region Log Column Row
                        if (logResults > recCount)
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                if (i > 0)
                                {
                                    line.Append(",");
                                }
                                line.Append(reader.GetValue(i));
                            }
                            this.TestContext.WriteLine(line.ToString());
                            line.Clear();
                        }
                        #endregion

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

        private void DropIndexIfExists(IDbConnection phConn, string index, string table)
        {
            using (IDbCommand cmd = phConn.CreateCommand())
            {
                try
                {
                    cmd.CommandText = string.Format("DROP INDEX {0} ON {1}", index, table);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception) { }
            }
        }

        private static void CreateBigTestTableIfNotExists(IDbConnection phConn, bool dropIfExists)
        {
            ReCreateTestTableIfNotExists(phConn, "bigtable",
                "CREATE TABLE IF NOT EXISTS bigtable (ID BIGINT PRIMARY KEY, AircraftIcaoNumber varchar(16), LruFlightKey varchar(64), MyTimestamp TIMESTAMP )",
                true, dropIfExists);
        }

        private static string CreateBulkCopyTableIfNotExists(IDbConnection phConn, bool dropIfExists, int saltBuckets = 0)
        {
            string tableName = "BulkCopyTest";
            string createTable = "CREATE TABLE IF NOT EXISTS {0} (ID BIGINT PRIMARY KEY, AircraftIcaoNumber varchar(16), LruFlightKey varchar(64), MyTimestamp TIMESTAMP )";

            if (saltBuckets > 0)
            {
                tableName += string.Format("_{0}", saltBuckets);
                createTable += string.Format(" SALT_BUCKETS = {0}", saltBuckets);
            }

            createTable = string.Format(createTable, tableName);

            ReCreateTestTableIfNotExists(phConn, tableName,
                createTable,
                true, dropIfExists);

            return tableName;
        }

        private static void ReCreateTestTableIfNotExists(IDbConnection phConn, string table, string createStmt, bool createSequence, bool dropIfExists)
        {
            if(dropIfExists)
            {
                using (IDbCommand cmd = phConn.CreateCommand())
                {
                    cmd.CommandText = string.Format("DROP TABLE IF EXISTS {0}", table);
                    cmd.ExecuteNonQuery();
                }
            }


            using (IDbCommand cmd = phConn.CreateCommand())
            {
                cmd.CommandText = createStmt;
                cmd.ExecuteNonQuery();
            }

            bool bCreateSequence = true;
            string seqName = SequenceNameForTable(table);
            using (IDbCommand cmd = phConn.CreateCommand())
            {

                cmd.CommandText = "SELECT sequence_schema, sequence_name, start_with, increment_by, cache_size FROM SYSTEM.\"SEQUENCE\""; //  WHERE sequence_schema = 'garuda' AND sequence_name='testsequence'
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.GetString(1).Equals(seqName, StringComparison.InvariantCultureIgnoreCase))
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
                    cmd.CommandText = string.Format("CREATE SEQUENCE garuda.{0}", seqName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static string SequenceNameForTable(string tableName)
        {
            return string.Format("{0}Sequence", tableName);
        }

        private string ConnectionString()
        {
            return System.IO.File.ReadAllText(@"..\..\..\GarudaUtil\myconnection.txt");
        }

        #endregion
    }

}
