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
    /// <summary>
    /// Provides a way of reading a forward-only stream of rows from an Apache Phoenix Query Server. 
    /// This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// To create a PhoenixDataReader, you must call the ExecuteReader method of the PhoenixCommand object, 
    /// instead of directly using a constructor.
    /// </remarks>
    public sealed class PhoenixDataReader : System.Data.Common.DbDataReader
    {
        private PhoenixConnection _connection = null;

        private uint _statementId = uint.MaxValue;

        private List<GarudaResultSet> _resultSets = new List<GarudaResultSet>();

        private int _currentFrameRowNdx = -1;

        private ulong _currentRowCount = 0;

        private int _currentFrame = 0;

        private int _currentResultSet = 0;

        private bool _isClosed = false;

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
                GarudaResultSet grs = new GarudaResultSet(res.Signature, res.FirstFrame, res.UpdateCount);
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

            GarudaResultSet grs = new GarudaResultSet(response.Signature, response.FirstFrame, response.UpdateCount);
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
                return GetValue(ordinal);
            }
        }

        /// <summary>
        /// Not currently implemented.
        /// </summary>
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
                if (null == CurrentResultSet() || null == CurrentFrame())
                {
                    return 0;
                }

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
                if(null == CurrentResultSet() || null == CurrentFrame())
                {
                    return false;
                }
                
                return CurrentFrame().Rows.Count > 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the DbDataReader is closed.
        /// </summary>
        public override bool IsClosed
        {
            get
            {
                return _isClosed;
            }
        }

        /// <summary>
        /// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
        /// </summary>
        public override int RecordsAffected
        {
            get
            {
                ulong ra = this.CurrentResultSet().UpdateCount;
                if(ulong.MaxValue == ra)
                {
                    ra = 0;
                }
                else if(ra > int.MaxValue)
                {
                    ra = int.MaxValue;
                }

                return Convert.ToInt32(ra);
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
            dcNullable.DataType = typeof(bool);

            DataColumn dcColName = dt.Columns.Add();
            dcColName.ColumnName = "ColumnName";

            DataColumn dcColOrdinal = dt.Columns.Add();
            dcColOrdinal.ColumnName = "ColumnOrdinal";
            dcColOrdinal.DataType = typeof(int);

            DataColumn dcColSize = dt.Columns.Add();
            dcColSize.ColumnName = "ColumnSize";
            dcColSize.DataType = typeof(int);

            foreach (var col in CurrentResultSet().Signature.Columns)
            {
                DataRow row = dt.NewRow();

                row[dcColName] = col.ColumnName;
                row[dcColOrdinal] = col.Ordinal;
                row[dcNullable] = Convert.ToBoolean(col.Nullable);

                if( (col.Type.Rep == GarudaRep.String && col.ColumnName == "PLAN") || col.TableName == "SYSTEM.STATS" )
                {
                    row[dcColSize] = int.MaxValue;
                }
                else
                {
                    row[dcColSize] = col.DisplaySize;
                }



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

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override byte GetByte(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override char GetChar(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="dataOffset"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferOffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a string representing the data type of the specified column.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override string GetDataTypeName(int ordinal)
        {
            return CurrentResultSet().Signature.Columns[ordinal].Type.Name;
        }

        /// <summary>
        /// Gets the value of the specified column as a DateTime object.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override DateTime GetDateTime(int ordinal)
        {
            return (DateTime)GetValue(ordinal);
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override decimal GetDecimal(int ordinal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value of the specified column as a double-precision floating point number.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override double GetDouble(int ordinal)
        {
            return (double)GetValue(ordinal);
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
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
                case GarudaRep.String:
                    o = val.StringValue;
                    break;

                case GarudaRep.ByteString:
                    o = val.BytesValue.ToString();
                    break;

                case GarudaRep.PrimitiveDouble:
                case GarudaRep.Double:
                    o = val.DoubleValue;
                    break;

                case GarudaRep.Boolean:
                    o = val.BoolValue;
                    break;

                case GarudaRep.Byte:
                case GarudaRep.Short:
                case GarudaRep.PrimitiveShort:
                case GarudaRep.Integer:
                case GarudaRep.Long:
                case GarudaRep.Number:
                    o = val.NumberValue;
                    if(val.NumberValue == 0 && val.DoubleValue != 0)
                    {
                        // Saw this once... Rep.NUMBER but DoubleValue was 
                        // where the value was stored.
                        o = val.DoubleValue;
                    }
                    break;

                case GarudaRep.Null:
                    o = DBNull.Value;
                    break;

                case GarudaRep.Array:
                    o = DBNull.Value;
                    break;

                default:
                    o = val.NumberValue;
                    break;
            }

            // Chronotypes
            if(GarudaRep.Null != val.Type)
            {
                switch (GetDataTypeName(ordinal))
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
                _isClosed = true;
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
