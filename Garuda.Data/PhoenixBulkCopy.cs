using Apache.Phoenix;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pbc = Google.Protobuf.Collections;

namespace Garuda.Data
{
    /// <summary>
    /// Lets you efficiently bulk load an Apache Phoenix Query Server table with data from another source.
    /// </summary>
    public class PhoenixBulkCopy
    {
        private PhoenixConnection _connection = null;

        /// <summary>
        /// Initializes a new instance of the SqlBulkCopy class using the specified open instance of PhoenixConnection.
        /// </summary>
        /// <param name="connection"></param>
        public PhoenixBulkCopy(PhoenixConnection connection)
        {
            if(null == connection)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            this._connection = connection;

            this.ColumnMappings = new Dictionary<string, PhoenixBulkCopyColumnMapping>();
        }

        /// <summary>
        /// Number of rows in each batch. At the end of each batch, the rows in the batch are sent to the server.
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// Name of the destination table on the server.
        /// </summary>
        public string DestinationTableName
        {
            get
            {
                return _destinationTableName;
            }

            set
            {
                if (null == value)
                {
                    throw new ArgumentNullException("DestinationTableName");
                }

                this._destinationTableName = value.ToUpper();
            }
        }
        private string _destinationTableName;

        /// <summary>
        /// Returns a dictionary of PhoenixBulkCopyColumnMapping items. The dictionary key
        /// is the column name.
        /// </summary>
        public Dictionary<string, PhoenixBulkCopyColumnMapping> ColumnMappings { get; private set; }

        /// <summary>
        /// Copies all rows in the supplied DataTable to a destination table specified by the DestinationTableName property of the PhoenixBulkCopy object.
        /// </summary>
        /// <param name="table"></param>
        public void WriteToServer(DataTable table)
        {
            pbc::RepeatedField<UpdateBatch> updates = new pbc::RepeatedField<UpdateBatch>();
            long batchCount = 0;
            string sql = BuildStatement();

            PrepareResponse pResp = _connection.InternalPrepareStatement(sql);

            foreach(DataRow row in table.Rows)
            {
                UpdateBatch anUpdate = new UpdateBatch();
                foreach(var val in row.ItemArray)
                {
                    anUpdate.ParameterValues.Add(PhoenixParameter.AsPhoenixTypedValue(val));
                }

                updates.Add(anUpdate);

                // If we reached the batch size, then send to server... ?
                batchCount++;
                if(batchCount >= this.BatchSize)
                {
                    // Send to server
                    _connection.InternalExecuteBatch(pResp.Statement.Id, updates);
                    updates.Clear();

                    // Reset batch counter
                    batchCount = 0;
                }
            }

            // Last batch   
            if(batchCount > 0)
            {
                // Send to server
                _connection.InternalExecuteBatch(pResp.Statement.Id, updates);
                updates.Clear();
            }
        }

        #region Private Methods

        private string BuildStatement()
        {
            ResultSetResponse resp = _connection.InternalColumnsRequest(string.Empty, string.Empty, this.DestinationTableName, string.Empty);
            string sqlFmt = "UPSERT INTO {0} ({1}) VALUES ({2})";
            StringBuilder colList = new StringBuilder();
            StringBuilder paramList = new StringBuilder();
            int colNameNdx = GetNdxOfColumn(resp, "COLUMN_NAME");
            int position = 1;
            bool bComma = false;

            // Build positional parameter list
            foreach(var r in resp.FirstFrame.Rows)
            {
                if(bComma)
                {
                    colList.Append(",");
                    paramList.Append(",");
                }

                // Column name into list.
                string colName = r.Value[colNameNdx].Value[0].StringValue;
                colList.Append(colName);

                // Parameter or Sequence?
                if (this.ColumnMappings.ContainsKey(colName))
                {
                    paramList.Append(this.ColumnMappings[colName].DefaultValue);
                }
                else
                {
                    paramList.AppendFormat(":{0}", position++);
                }

                bComma = true;
            }

            return string.Format(sqlFmt, this.DestinationTableName, colList, paramList);
        }

        private int GetNdxOfColumn(ResultSetResponse resp, string name)
        {
            for (int i = 0; i < resp.Signature.Columns.Count; i++)
            {
                if (name == resp.Signature.Columns[i].ColumnName)
                {
                    return i;
                }
            }

            throw new KeyNotFoundException(name);
        }

        #endregion
    }
}
