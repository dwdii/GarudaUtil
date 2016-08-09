using Apache.Phoenix;
using Garuda.Data.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Garuda.Data
{
    public class PhoenixDataReader : System.Data.Common.DbDataReader
    {
        private PhoenixConnection _connection = null;

        private uint _statementId = uint.MaxValue;

        private List<GarudaResultSet> _resultSets = new List<GarudaResultSet>();

        private int _currentFrameRowNdx = -1;

        private ulong _currentRowCount = 0;

        private int _currentFrame = 0;

        private int _currentResultSet = 0;

        /// <summary>
        /// Gets the PhoenixConnection associated with the SqlDataReader.
        /// </summary>
        public PhoenixConnection Connection {  get { return _connection; } }

        internal PhoenixDataReader(PhoenixCommand cmd, GarudaExecuteResponse response)
        {
            if(null == cmd)
            {
                throw new ArgumentNullException("cmd");
            }
            if(null == response)
            {
                throw new ArgumentNullException("response");
            }

            _connection = (PhoenixConnection)cmd.Connection;
            _statementId = response.StatementId;

            //_response = response.Response.Results.ToList();
            _resultSets = new List<GarudaResultSet>();
            foreach(var res in response.Response.Results)
            {
                GarudaResultSet grs = new GarudaResultSet(res.Signature, res.FirstFrame);
                _resultSets.Add(grs);
            }
        }

        internal PhoenixDataReader(PhoenixConnection connection, ResultSetResponse response)
        {
            if (null == connection)
            {
                throw new ArgumentNullException(nameof(connection));
            }
            if (null == response)
            {
                throw new ArgumentNullException(nameof(response));
            }

            _connection = connection;

            GarudaResultSet grs = new GarudaResultSet(response.Signature, response.FirstFrame);
            _resultSets.Add(grs);
        }

        #region DbDataReader Class

        /// <summary>
        /// Gets the value of the specified column in its native format given the column name.(Overrides DbDataReader.Item[String].)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override object this[string name]
        {
            get
            {
                return this[GetOrdinal(name)];
            }
        }

        /// <summary>
        /// Gets the value of the specified column in its native format given the column ordinal.(Overrides DbDataReader.Item[Int32].)
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override object this[int ordinal]
        {
            get
            {
                return CurrentRowValue(ordinal);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override int Depth
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the number of columns in the current row.(Overrides DbDataReader.FieldCount.)
        /// </summary>
        public override int FieldCount
        {
            get
            {
                return CurrentResultSet().Signature.Columns.Count;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the SqlDataReader contains one or more rows.(Overrides DbDataReader.HasRows.)
        /// </summary>
        public override bool HasRows
        {
            get
            {
                return CurrentFrame().Rows.Count > 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the DbDataReader is closed.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override bool IsClosed
        {
            get
            {
                return !(CurrentFrame().Rows.Count > _currentFrameRowNdx);
            }
        }

        /// <summary>
        /// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public override int RecordsAffected
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns a DataTable that describes the column metadata of the DbDataReader.
        /// </summary>
        /// <returns></returns>
        public override DataTable GetSchemaTable()
        {
            DataTable dt = new DataTable();

            DataColumn dcNullable = dt.Columns.Add();
            dcNullable.ColumnName = "AllowDBNull";

            DataColumn dcColName = dt.Columns.Add();
            dcColName.ColumnName = "ColumnName";

            DataColumn dcColOrdinal = dt.Columns.Add();
            dcColOrdinal.ColumnName = "ColumnOrdinal";

            DataColumn dcColSize = dt.Columns.Add();
            dcColSize.ColumnName = "ColumnSize";


            DataColumn dcIsKey = dt.Columns.Add();
            dcIsKey.ColumnName = "IsKey";

            foreach (var col in CurrentResultSet().Signature.Columns)
            {
                DataRow row = dt.NewRow();

                row[dcColName] = col.ColumnName;
                row[dcColOrdinal] = col.Ordinal;
                row[dcNullable] = Convert.ToBoolean(col.Nullable);
                row[dcColSize] = col.DisplaySize;
                

                dt.Rows.Add(row);
            }

            

            return dt;
        }

        /// <summary>
        /// Gets the value of the specified column as a Boolean.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override bool GetBoolean(int ordinal)
        {
            return (bool)GetValue(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override char GetChar(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override string GetDataTypeName(int ordinal)
        {
            return CurrentResultSet().Signature.Columns[ordinal].Type.Name;
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return (DateTime)GetValue(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override double GetDouble(int ordinal)
        {
            return (double)GetValue(ordinal);
        }

        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the Type information corresponding to the type of Object that would be returned from GetValue.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override Type GetFieldType(int ordinal)
        {
            return PhoenixDataTypeMap.GetDotNetType(CurrentResultSet().Signature.Columns[ordinal].Type.Rep, 
                this.GetDataTypeName(ordinal));
        }

        /// <summary>
        /// Gets the single-precision floating point number of the specified field.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override float GetFloat(int ordinal)
        {
            return(float)GetValue(ordinal);
        }

        /// <summary>
        /// Returns the GUID value of the specified field.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override Guid GetGuid(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override short GetInt16(int ordinal)
        {
            return (short)GetValue(ordinal);
        }

        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override int GetInt32(int ordinal)
        {
            return (int)GetValue(ordinal);
        }

        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override long GetInt64(int ordinal)
        {
            return (long)GetValue(ordinal);
        }

        /// <summary>
        /// Gets the name for the field to find.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override string GetName(int ordinal)
        {
            return CurrentResultSet().Signature.Columns[ordinal].ColumnName;
        }

        /// <summary>
        /// Return the index of the named field.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override int GetOrdinal(string name)
        {
            int ordinal = -1;

            for(int i = 0; i < this.FieldCount; i++)
            {
                var c = CurrentResultSet().Signature.Columns[i];
                if (c.ColumnName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    ordinal = i;
                    break;
                }
            }

            return ordinal;
        }

        /// <summary>
        /// Gets the string value of the specified field.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override string GetString(int ordinal)
        {
            return GetValue(ordinal) as string;
        }

        /// <summary>
        /// Return the value of the specified field.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override object GetValue(int ordinal)
        {
            object o = null;
            TypedValue val = CurrentRowValue(ordinal);

            switch (val.Type)
            {
                case Rep.STRING:
                    o = val.StringValue;
                    break;

                case Rep.BYTE_STRING:
                    o = val.BytesValues.ToString();
                    break;

                case Rep.PRIMITIVE_DOUBLE:
                case Rep.DOUBLE:
                    o = val.DoubleValue;
                    break;

                case Rep.BOOLEAN:
                    o = val.BoolValue;
                    break;

                case Rep.BYTE:
                case Rep.SHORT:
                case Rep.PRIMITIVE_SHORT:
                case Rep.INTEGER:
                case Rep.LONG:
                case Rep.NUMBER:
                    o = val.NumberValue;
                    if(val.NumberValue == 0 && val.DoubleValue != 0)
                    {
                        // Saw this once... Rep.NUMBER but DoubleValue was 
                        // where the value was stored.
                        o = val.DoubleValue;
                    }
                    break;

                case Rep.NULL:
                    o = null;// DBNull.Value;
                    break;

                default:
                    o = val.NumberValue;
                    break;
            }

            switch(GetDataTypeName(ordinal))
            {
                case "DATE":
                    o = FromPhoenixDate(val.NumberValue);
                    break;

                case "TIME":
                    o = FromPhoenixTime(val.NumberValue);
                    break;

                case "TIMESTAMP":
                    o = FromPhoenixTimestamp(val.NumberValue);
                    break;
            }

            return o;
        }

        /// <summary>
        /// Populates an array of objects with the column values of the current record.
        /// </summary>
        /// <param name="values">An array of Object to copy the attribute fields into.</param>
        /// <returns>The number of instances of Object in the array.</returns>
        public override int GetValues(object[] values)
        {
            for(int i = 0; i < values.Length; i++)
            {
                values[i] = GetValue(i);
            }

            return values.Length;
        }

        /// <summary>
        /// Return whether the specified field is set to null.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override bool IsDBNull(int ordinal)
        {
            return CurrentRowValue(ordinal).Null;
        }

        /// <summary>
        /// Advances the data reader to the next result, when reading the results of batch SQL statements.
        /// </summary>
        /// <returns></returns>
        public override bool NextResult()
        {
            bool ok = this._resultSets.Count > _currentResultSet + 1;
            if (ok)
            {
                _currentResultSet++;
            }

            return ok;
        }

        /// <summary>
        /// Advances the IDataReader to the next record.
        /// </summary>
        /// <returns></returns>
        public override bool Read()
        {
            // Crude, but will do for now...
            _currentRowCount++;
            _currentFrameRowNdx++;

            // Load more data?
            bool bInCurrentFrame = CurrentFrame().Rows.Count > _currentFrameRowNdx;
            if (!bInCurrentFrame && !CurrentFrame().Done)
            {
                Task<FetchResponse> tResp = Task.Run(() => this.Connection.InternalFetchAsync(this._statementId, _currentRowCount - 1, 1000));
                tResp.Wait();

                CurrentResultSet().Frames.Add(tResp.Result.Frame);
                _currentFrame++;
                _currentFrameRowNdx = 0;
            }

            return CurrentFrame().Rows.Count > _currentFrameRowNdx;
        }

        /// <summary>
        /// Closes the IDataReader Object.
        /// </summary>
        public override void Close()
        {
            if(uint.MaxValue > _statementId)
            {
                this.Connection.InternalCloseStatement(_statementId);
            }

            base.Close();
        }

        #endregion

        private GarudaResultSet CurrentResultSet()
        {
            return _resultSets[_currentResultSet];
        }

        private Frame CurrentFrame()
        {
            return CurrentResultSet().Frames[_currentFrame];
        }

        private Row CurrentRow()
        {
            return CurrentFrame().Rows[_currentFrameRowNdx];
        }

        private TypedValue CurrentRowValue(int ordinal)
        {
            return CurrentRow().Value[ordinal].Value[0];
        }

        private TimeSpan FromPhoenixTime(long time)
        {
            var dtTime = PhoenixParameter.Constants.Epoch.AddMilliseconds(time);
            return dtTime.Subtract(PhoenixParameter.Constants.Epoch);
        }

        private DateTime FromPhoenixDate(long date)
        {
            return PhoenixParameter.Constants.Epoch.AddDays(date);
        }

        private DateTime FromPhoenixTimestamp(long timestamp)
        {
            return PhoenixParameter.Constants.Epoch.AddMilliseconds(timestamp);
        }
    }
}
