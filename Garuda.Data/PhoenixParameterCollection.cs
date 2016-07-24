using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public int Add(object value)
        {
            return this.Add(value as PhoenixParameter);
        }
        
        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public bool Contains(string parameterName)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }


        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(string parameterName)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            this.Insert(index, value as PhoenixParameter);
        }

        public void Remove(object value)
        {
            this.Remove(value as PhoenixParameter);
        }

        public void RemoveAt(string parameterName)
        {
            throw new NotImplementedException();
        }
    }
}
