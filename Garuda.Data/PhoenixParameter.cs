using Apache.Phoenix;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garuda.Data
{
    public class PhoenixParameter : System.Data.Common.DbParameter
    {
        public override DbType DbType { get; set; }

        public override ParameterDirection Direction { get; set; }

        public override bool IsNullable { get; set; }

        public override string ParameterName { get; set; }

        public override int Size { get; set; }

        public override string SourceColumn { get; set; }

        public override bool SourceColumnNullMapping { get; set; }

        public override object Value { get; set; }

        public override void ResetDbType()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts this parameter to and returns the Apache Phoenix TypedValue structure.
        /// </summary>
        /// <returns></returns>
        public TypedValue AsPhoenixTypedValue()
        {
            var tv = new TypedValue();

            if(this.Value == null)
            {
                tv.Null = true;
            }
            else
            {
                Type pt = this.Value.GetType();
                if (pt == typeof(int) || pt == typeof(long))
                {
                    tv.NumberValue = Convert.ToInt64(this.Value);
                    tv.Type = Rep.BIG_INTEGER;
                }
                else if(pt == typeof(float) || pt == typeof(double))
                {
                    tv.DoubleValue = Convert.ToDouble(this.Value);
                    tv.Type = Rep.DOUBLE;
                }
                else if (pt == typeof(string))
                {
                    tv.StringValue = Convert.ToString(this.Value);
                    tv.Type = Rep.STRING;
                }
                else if (this.Value == DBNull.Value)
                {
                    tv.Null = true;
                    tv.Type = Rep.NULL;
                }
            }

            return tv;
        }
    }
}
