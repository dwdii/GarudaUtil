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
        const string SqlColumnMetaData = "SELECT COLUMN_NAME, COLUMN_FAMILY, COLUMN_SIZE, COLUMN_DEF, ARRAY_SIZE, BUFFER_LENGTH, DATA_TYPE, IS_AUTOINCREMENT, IS_NULLABLE, NULLABLE, STORE_NULLS, IS_ROW_TIMESTAMP, ORDINAL_POSITION, PK_NAME, SQL_DATA_TYPE, SOURCE_DATA_TYPE, KEY_SEQ  FROM SYSTEM.CATALOG WHERE COLUMN_NAME IS NOT NULL AND TABLE_NAME = :1";
        const string SqlTableSchemaCriteria = " AND TABLE_SCHEM = :2";
        const string SqlTableSchemaNullCriteria = " AND TABLE_SCHEM IS NULL";

        const string SqlIndexMetaData = "SELECT TABLE_NAME as INDEX_NAME, DATA_TABLE_NAME FROM SYSTEM.CATALOG WHERE TABLE_TYPE = 'i' AND DATA_TABLE_NAME = :1";

        public DataRow Row { get; }

        private DataTable _columns = null;

        private DataTable _indexes = null;

        public GarudaPhoenixTable(DataRow row)
        {
            this.Row = row;
        }

        public string Name { get { return Row["TABLE_NAME"].ToString(); } }

        public async Task<DataTable> GetColumnsAsync(PhoenixConnection c, bool refresh)
        {
            if(null == this._columns || refresh)
            {
                if(c.State != ConnectionState.Open)
                {
                    c.Open();
                }

                using (IDbCommand cmd = c.CreateCommand())
                {
                    cmd.CommandText = SqlColumnMetaData;

                    cmd.Parameters.Add(new PhoenixParameter(this.Name));
                    if(DBNull.Value == Row["TABLE_SCHEM"])
                    {
                        cmd.CommandText += SqlTableSchemaNullCriteria;
                    }
                    else
                    {
                        cmd.CommandText += SqlTableSchemaCriteria;
                        cmd.Parameters.Add(new PhoenixParameter(Row["TABLE_SCHEM"]));
                    }

                    cmd.Prepare();
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        _columns = new DataTable(string.Format("{0} Columns", this.Name));
                        _columns.BeginLoadData();
                        _columns.Load(dr);
                        _columns.EndLoadData();
                    }
                }
            }

            return _columns;
        }

        public async Task<DataTable> GetIndexesAsync(PhoenixConnection c, bool refresh)
        {
            if (null == this._indexes || refresh)
            {
                this._indexes = await Task.Factory.StartNew(() => GetIndexes(c));
            }

            return _indexes;
        }

        public DataTable GetIndexes(PhoenixConnection c)
        {
            DataTable indexes = null;

            if (c.State != ConnectionState.Open)
            {
                c.Open();
            }

            using (IDbCommand cmd = c.CreateCommand())
            {
                cmd.CommandText = SqlIndexMetaData;

                cmd.Parameters.Add(new PhoenixParameter(this.Name));
                if (DBNull.Value == Row["TABLE_SCHEM"])
                {
                    cmd.CommandText += SqlTableSchemaNullCriteria;
                }
                else
                {
                    cmd.CommandText += SqlTableSchemaCriteria;
                    cmd.Parameters.Add(new PhoenixParameter(Row["TABLE_SCHEM"]));
                }

                cmd.Prepare();
                using (IDataReader dr = cmd.ExecuteReader())
                {
                    indexes = new DataTable(string.Format("{0} Indexes", this.Name));
                    indexes.BeginLoadData();
                    indexes.Load(dr);
                    indexes.EndLoadData();
                }
            }

            return indexes;
        }
    }
}
