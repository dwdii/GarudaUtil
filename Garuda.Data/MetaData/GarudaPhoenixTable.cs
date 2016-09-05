using Garuda.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garuda.Data.MetaData
{
    /// <summary>
    /// Represents a Phoenix table and provides methods for discovering columns, indexes, and other meta data
    /// about the table.
    /// </summary>
    public class GarudaPhoenixTable
    {
        const string SqlColumnMetaData = "SELECT COLUMN_NAME, COLUMN_FAMILY, COLUMN_SIZE, COLUMN_DEF, ARRAY_SIZE, BUFFER_LENGTH, DATA_TYPE, IS_AUTOINCREMENT, IS_NULLABLE, NULLABLE, STORE_NULLS, IS_ROW_TIMESTAMP, ORDINAL_POSITION, PK_NAME, SQL_DATA_TYPE, SOURCE_DATA_TYPE, KEY_SEQ  FROM SYSTEM.CATALOG WHERE COLUMN_NAME IS NOT NULL AND TABLE_NAME = :1";
        const string SqlTableSchemaCriteria = " AND TABLE_SCHEM = :2";
        const string SqlTableSchemaNullCriteria = " AND TABLE_SCHEM IS NULL";

        const string SqlIndexMetaData = "SELECT TABLE_NAME as INDEX_NAME, DATA_TABLE_NAME FROM SYSTEM.CATALOG WHERE TABLE_TYPE = 'i' AND DATA_TABLE_NAME = :1";

        /// <summary>
        /// The DataRow which was used to initialize this instance.
        /// </summary>
        public DataRow Row { get; }

        private DataTable _columns = null;

        private DataTable _indexes = null;

        private string _fullName = null;

        /// <summary>
        /// Constructs a new instance of the GarudaPhoenixTable class according to the data in the specified
        /// Row object.
        /// </summary>
        /// <param name="row"></param>
        public GarudaPhoenixTable(DataRow row)
        {
            this.Row = row;
        }

        /// <summary>
        /// The name of the table.
        /// </summary>
        public string Name { get { return Row["TABLE_NAME"].ToString(); } }

        /// <summary>
        /// The name of the schema in which this table belongs.
        /// </summary>
        public string Schema { get { return Row["TABLE_SCHEM"].ToString(); } }

        /// <summary>
        /// The full name of the table include schema prefix if any.
        /// </summary>
        public string FullName
        {
            get
            {
                if(null == _fullName)
                {
                    if (string.IsNullOrWhiteSpace(this.Schema))
                    {
                        _fullName = this.Name;
                    }
                    else
                    {
                        _fullName = string.Format("{0}.{1}", this.Schema, this.Name);
                    }
                }

                return _fullName;
            }
        }

        public bool IsColumnPrimaryKey(DataTable dt, string columnName)
        {
            foreach(DataRow row in dt.Rows)
            {
                if(row["COLUMN_NAME"].ToString() == columnName)
                {
                    return DBNull.Value != row["KEY_SEQ"];
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a DataTable containing the columns of this table and associated meta data. 
        /// This requies an additional trip to the Phoenix Query Server using the specified connection.
        /// </summary>
        /// <param name="c">The PhoenixConnection to use when querying additional column meta data.</param>
        /// <param name="refresh">If true and a cached copy exists, refresh the data from the PQS.</param>
        /// <returns>The DataTable containing the column meta data.</returns>
        public async Task<DataTable> GetColumnsAsync(PhoenixConnection c, bool refresh)
        {
            if (null == this._columns || refresh)
            {
                this._columns = await Task.Factory.StartNew(() => GetColumns(c));
            }

            return _columns;
        }

        /// <summary>
        /// Gets a DataTable containing the columns of this table and associated meta data. 
        /// This requies an additional trip to the Phoenix Query Server using the specified connection.
        /// </summary>
        /// <returns>The DataTable containing the column meta data.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
        public DataTable GetColumns(PhoenixConnection c)
        {
            if (null == c)
            {
                throw new ArgumentNullException(nameof(c));
            }

            DataTable columns = null;
            StringBuilder sbSql = new StringBuilder(SqlColumnMetaData);

            if (c.State != ConnectionState.Open)
            {
                c.Open();
            }

            using (IDbCommand cmd = c.CreateCommand())
            {
                // Parameters for table name, and schema if not null.
                cmd.Parameters.Add(new PhoenixParameter(this.Name));
                if(DBNull.Value == Row["TABLE_SCHEM"])
                {
                    sbSql.Append(SqlTableSchemaNullCriteria);
                }
                else
                {
                    sbSql.Append(SqlTableSchemaCriteria);
                    cmd.Parameters.Add(new PhoenixParameter(Row["TABLE_SCHEM"]));
                }

                cmd.CommandText = sbSql.ToString();
                cmd.Prepare();
                using (IDataReader dr = cmd.ExecuteReader())
                {
                    columns = new DataTable(string.Format("{0} Columns", this.Name));
                    columns.BeginLoadData();
                    columns.Load(dr);
                    columns.EndLoadData();
                }
            }

            return columns;
        }

        /// <summary>
        /// Gets a DataTable containing the indexes of this table and associated meta data. 
        /// This requies an additional trip to the Phoenix Query Server using the specified connection.
        /// </summary>
        /// <param name="c">The PhoenixConnection to use when querying additional index meta data.</param>
        /// <param name="refresh">If true and a cached copy exists, refresh the data from the PQS.</param>
        /// <returns>The DataTable containing the index meta data.</returns>
        public async Task<DataTable> GetIndexesAsync(PhoenixConnection c, bool refresh)
        {
            if (null == this._indexes || refresh)
            {
                this._indexes = await Task.Factory.StartNew(() => GetIndexes(c));
            }

            return _indexes;
        }

        /// <summary>
        /// Gets a DataTable containing the indexes of this table and associated meta data. 
        /// This requies an additional trip to the Phoenix Query Server using the specified connection.
        /// </summary>
        /// <returns>The DataTable containing the index meta data.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
        public DataTable GetIndexes(PhoenixConnection c)
        {
            if (null == c)
            {
                throw new ArgumentNullException(nameof(c));
            }

            DataTable indexes = null;
            StringBuilder sbSql = new StringBuilder(SqlIndexMetaData);

            if (c.State != ConnectionState.Open)
            {
                c.Open();
            }

            using (IDbCommand cmd = c.CreateCommand())
            {
                cmd.Parameters.Add(new PhoenixParameter(this.Name));
                if (DBNull.Value == Row["TABLE_SCHEM"])
                {
                    sbSql.Append(SqlTableSchemaNullCriteria);
                }
                else
                {
                    sbSql.Append(SqlTableSchemaCriteria);
                    cmd.Parameters.Add(new PhoenixParameter(Row["TABLE_SCHEM"]));
                }

                cmd.CommandText = sbSql.ToString();
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

        /// <summary>
        /// Generates the Upsert statement associated with this table.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="refresh">If true and cached column meta data exists, it is refreshed from the PQS. Defaults to true.</param>
        /// <returns></returns>
        public async Task<string> GenerateUpsertStatementAsync(PhoenixConnection c, bool refresh = true)
        {
            if (null == c)
            {
                throw new ArgumentNullException(nameof(c));
            }

            DataTable columns = await this.GetColumnsAsync(c, true);
            StringBuilder sbUpsert = new StringBuilder();
            StringBuilder sbValues = new StringBuilder();

            sbUpsert.AppendFormat("UPSERT INTO {0} (", this.FullName);
            for (int i = 0; i < columns.Rows.Count; i++)
            {
                DataRow col = columns.Rows[i];

                if (i > 0)
                {
                    sbUpsert.Append(",");
                    sbValues.Append(",");
                }

                sbUpsert.Append(col["COLUMN_NAME"]);
                sbValues.Append("?");
            }
            sbUpsert.AppendFormat(") VALUES ({0})", sbValues.ToString());

            return sbUpsert.ToString();
        }
    }
}
