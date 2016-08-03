using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        public int BatchSize { get; set; }

        public void WriteToServer(DataTable table)
        {
                
        }
    }
}
