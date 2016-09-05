using Garuda.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garuda.Data.MetaData
{
    /// <summary>
    /// Represents a Phoenix index on a table and provides methods for discovering meta data
    /// about the index.
    /// </summary>
    public class GarudaPhoenixIndex : IGarudaPhoenixMetaData
    {
        const string SqlKeyColumnMetaData = "SELECT COLUMN_DEF, DATA_TYPE FROM SYSTEM.CATALOG WHERE COLUMN_NAME IS NOT NULL AND KEY_SEQ IS NOT NULL AND TABLE_NAME = :1 ORDER BY KEY_SEQ";


        DataRow _row = null;
        private DataTable _keyColumns;

        /// <summary>
        /// Constructs a new instance of the GarudaPhoenixIndex class.
        /// </summary>
        public GarudaPhoenixIndex()
        {

        }

        /// <summary>
        /// The DataRow which contains meta data about this index.
        /// </summary>
        public DataRow Row
        {
            get { return _row; }
            set
            {
                if(null == value)
                {
                    throw new ArgumentNullException();
                }

                _row = value;
            }
        }

        /// <summary>
        /// Gets the name of the table associated with this index.
        /// </summary>
        public string TableName
        {
            get { return _row["DATA_TABLE_NAME"].ToString(); }
        }

        /// <summary>
        /// Gets the name of this index.
        /// </summary>
        public string Name
        {
            get { return _row["INDEX_NAME"].ToString(); }
        }

        /// <summary>
        /// Gets a DataTable containing the columns of the table which comprise the key columns of this index. 
        /// This requies an additional trip to the Phoenix Query Server using the specified connection.
        /// </summary>
        /// <param name="c">The PhoenixConnection to use when querying additional index meta data.</param>
        /// <param name="refresh">If true and a cached copy exists, refresh the data from the PQS.</param>
        /// <returns>The DataTable containing the index meta data.</returns>
        public async Task<DataTable> GetKeyColumnsAsync(PhoenixConnection c, bool refresh)
        {
            if (null == this._keyColumns || refresh)
            {
                this._keyColumns = await Task.Factory.StartNew(() => GetKeyColumns(c));
            }

            return _keyColumns;
        }

        /// <summary>
        /// Gets a DataTable containing the columns of the table which comprise the key columns of this index. 
        /// This requies an additional trip to the Phoenix Query Server using the specified connection.
        /// </summary>
        /// <returns>The DataTable containing the index meta data.</returns>
        public DataTable GetKeyColumns(PhoenixConnection c)
        {
            if(null == c)
            {
                throw new ArgumentNullException(nameof(c));
            }

            DataTable dt = null;

            if (c.State != ConnectionState.Open)
            {
                c.Open();
            }

            using (IDbCommand cmd = c.CreateCommand())
            {
                cmd.CommandText = SqlKeyColumnMetaData;

                cmd.Parameters.Add(new PhoenixParameter(this.Name));
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
                    dt = new DataTable(string.Format("{0} Key Columns", this.Name));
                    dt.BeginLoadData();
                    dt.Load(dr);
                    dt.EndLoadData();
                }
            }

            return dt;
        }
    }
}
