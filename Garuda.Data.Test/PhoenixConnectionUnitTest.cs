using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace Garuda.Data.Test
{
    [TestClass]
    public class PhoenixConnectionUnitTest
    {
        [TestMethod]
        public void BasicOpenDisposeTest()
        {
            using (PhoenixConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();
            }
        }

        [TestMethod]
        public void BeginTransactionMethodTest()
        {
            using (IDbConnection c = new PhoenixConnection())
            {
                c.ConnectionString = this.ConnectionString();
                c.Open();

                using (IDbTransaction t = c.BeginTransaction())
                {
                    
                }
            }
        }


        private string ConnectionString()
        {
            return System.IO.File.ReadAllText(@"..\..\..\GarudaUtil\myconnection.txt");
        }
    }

}
