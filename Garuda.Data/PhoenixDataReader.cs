using Apache.Phoenix;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garuda.Data
{
    public class PhoenixDataReader : System.Data.Common.DbDataReader
    {
        private PhoenixCommand _command = null;

        private uint _statementId = uint.MaxValue;

        private List<ResultSetResponse> _response = null;

        private int _currentRowNdx = -1;

        private int _currentResultSet = 0;

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

            _command = cmd;
            _response = response.Response.Results.ToList();
            _statementId = response.StatementId;
        }

        #region DbDataReader Class

        public override object this[string name]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override object this[int ordinal]
        {
            get
            {
                return _response[_currentResultSet].FirstFrame.Rows[_currentRowNdx].Value[ordinal];
            }
        }

        public override int Depth
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int FieldCount
        {
            get
            {
                return _response[_currentResultSet].Signature.Columns.Count;
            }
        }

        public override bool HasRows
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsClosed
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override int RecordsAffected
        {
            get
            {
                throw new NotImplementedException();
            }
        }

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
            throw new NotImplementedException();
        }

        public override DateTime GetDateTime(int ordinal)
        {
            throw new NotImplementedException();
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

        public override Type GetFieldType(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override float GetFloat(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override Guid GetGuid(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override short GetInt16(int ordinal)
        {
            return (short)GetValue(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            return (int)GetValue(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            return (long)GetValue(ordinal);
        }

        public override string GetName(int ordinal)
        {
            return _response[_currentResultSet].Signature.Columns[ordinal].ColumnName;
        }

        public override int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public override string GetString(int ordinal)
        {
            return GetValue(ordinal) as string;
        }

        public override object GetValue(int ordinal)
        {
            object o = null;
            TypedValue val = CurrentRowValue(ordinal);

            switch (val.Type)
            {
                case Rep.STRING:
                    o = val.StringValue;
                    break;

                case Rep.DOUBLE:
                    o = val.DoubleValue;
                    break;

                case Rep.BOOLEAN:
                    o = val.BoolValue;
                    break;

                case Rep.INTEGER:
                case Rep.LONG:
                case Rep.NUMBER:
                    o = val.NumberValue;
                    break;
            }

            return o;
        }

        public override int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public override bool IsDBNull(int ordinal)
        {
            return CurrentRowValue(ordinal).Null;
        }

        public override bool NextResult()
        {
            bool ok = this._response.Count > _currentResultSet;
            if (ok)
            {
                _currentResultSet++;
            }

            return ok;
        }

        public override bool Read()
        {
            // Crude, but will do for now...
            _currentRowNdx++;

            return _response[_currentResultSet].FirstFrame.Rows.Count > _currentRowNdx;
        }

        public override void Close()
        {
            (_command.Connection as PhoenixConnection).CloseStatement(_statementId);

            base.Close();
        }

        #endregion

        private Row CurrentRow()
        {
            return _response[_currentResultSet].FirstFrame.Rows[_currentRowNdx];
        }

        private TypedValue CurrentRowValue(int ordinal)
        {
            return CurrentRow().Value[ordinal].Value[0];
        }
    }
}
