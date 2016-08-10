using Garuda.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarudaUtil.MetaData
{
    class GarudaPhoenixTable
    {
        public DataRow Row { get; }

        public GarudaPhoenixTable(DataRow row)
        {
            this.Row = row;
        }

        public DataTable GetColumns(PhoenixConnection c)
        {
            DataTable dt = new DataTable("Phoenix Columns");
            using (IDbCommand cmd = c.CreateCommand())
            {
                cmd.CommandText = "SELECT COLUMN_NAME FROM SYSTEM.CATALOG WHERE COLUMN_NAME IS NOT NULL AND TABLE_NAME = :1";
                cmd.Parameters.Add(new PhoenixParameter(Row["TABLE_NAME"]));
                cmd.Prepare();
                using (IDataReader dr = cmd.ExecuteReader())
                {
                    dt.BeginLoadData();
                    dt.Load(dr);
                    dt.EndLoadData();
                }
            }

            return dt;
        }
    }
}
