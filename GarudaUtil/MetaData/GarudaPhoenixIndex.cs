using Garuda.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarudaUtil.MetaData
{
    class GarudaPhoenixIndex : IGarudaPhoenixMetaData
    {
        const string SqlKeyColumnMetaData = "SELECT COLUMN_DEF, DATA_TYPE FROM SYSTEM.CATALOG WHERE COLUMN_NAME IS NOT NULL AND KEY_SEQ IS NOT NULL AND TABLE_NAME = :1 ORDER BY KEY_SEQ";


        DataRow _row = null;
        private DataTable _keyColumns;

        public GarudaPhoenixIndex()
        {

        }

        public DataRow Row
        {
            get
            {
                return _row;
            }

            set
            {
                _row = value;
            }
        }

        public string TableName
        {
            get { return _row["DATA_TABLE_NAME"].ToString(); }
        }

        public string IndexName
        {
            get { return _row["INDEX_NAME"].ToString(); }
        }

        public async Task<DataTable> GetIndexesAsync(PhoenixConnection c, bool refresh)
        {
            if (null == this._keyColumns || refresh)
            {
                this._keyColumns = await Task.Factory.StartNew(() => GetKeyColumns(c));
            }

            return _keyColumns;
        }

        public DataTable GetKeyColumns(PhoenixConnection c)
        {
            DataTable dt = null;

            if (c.State != ConnectionState.Open)
            {
                c.Open();
            }

            using (IDbCommand cmd = c.CreateCommand())
            {
                cmd.CommandText = SqlKeyColumnMetaData;

                cmd.Parameters.Add(new PhoenixParameter(this.IndexName));
                //if (DBNull.Value == Row["TABLE_SCHEM"])
                //{
                //    cmd.CommandText += SqlTableSchemaNullCriteria;
                //}
                //else
                //{
                //    cmd.CommandText += SqlTableSchemaCriteria;
                //    cmd.Parameters.Add(new PhoenixParameter(Row["TABLE_SCHEM"]));
                //}

                cmd.Prepare();
                using (IDataReader dr = cmd.ExecuteReader())
                {
                    dt = new DataTable(string.Format("{0} Key Columns", this.IndexName));
                    dt.BeginLoadData();
                    dt.Load(dr);
                    dt.EndLoadData();
                }
            }

            return dt;
        }
    }
}
