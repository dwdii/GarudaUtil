using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace Garuda.Data.Test
{
    [TestClass]
    public class PhoenixConnectionUnitTest
    {
        [TestMethod]
        public void ConnectionBasicOpenDisposeTest()
        {
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();
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

                CreateTestTableIfNotExists(c);

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

                CreateTestTableIfNotExists(c);

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

                CreateTestTableIfNotExists(c);

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

        private static long QueryAllRows(IDbConnection c)
        {
            long recCount = 0;

            // Query for data... should get one rows.
            using (IDbCommand cmd = c.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM GARUDATEST";
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        recCount++;
                    }
                }
            }

            return recCount;
        }

        private static void CreateTestTableIfNotExists(IDbConnection phConn)
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
