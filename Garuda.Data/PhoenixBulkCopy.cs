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
    public class PhoenixBulkCopy
    {
        private PhoenixConnection _connection = null;

        public PhoenixBulkCopy(PhoenixConnection connection)
        {
            if(null == connection)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            this._connection = connection;

            this.SequenceMappings = new Dictionary<string, string>();
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

        public Dictionary<string, string> SequenceMappings { get; private set; }

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
            }

            // Send to server
            _connection.InternalExecuteBatch(pResp.Statement.Id, updates);
            updates.Clear();

            // Reset batch counter.
            batchCount = 0;
        }

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
                if (this.SequenceMappings.ContainsKey(colName))
                {
                    paramList.Append(this.SequenceMappings[colName]);
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
    }
}
