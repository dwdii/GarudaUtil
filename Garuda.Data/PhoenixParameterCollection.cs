using Apache.Phoenix;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pbc = Google.Protobuf.Collections;

namespace Garuda.Data
{
    public class PhoenixParameterCollection : List<PhoenixParameter>, IDataParameterCollection
    {
        public object this[string parameterName]
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsFixedSize
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsSynchronized
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object SyncRoot
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool Contains(string parameterName)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(string parameterName)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(string parameterName)
        {
            throw new NotImplementedException();
        }

        public pbc.RepeatedField<TypedValue> AsRepeatedFieldTypedValue()
        {
            pbc.RepeatedField<TypedValue> pbParamValues = new pbc.RepeatedField<TypedValue>();
            foreach (var p in this)
            {
                pbParamValues.Add(p.AsPhoenixTypedValue());
            }

            return pbParamValues;
        }
    }
}
