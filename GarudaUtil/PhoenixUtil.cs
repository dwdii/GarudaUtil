using Apache.Phoenix;
using PhoenixSharp;
using PhoenixSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pbc = Google.Protobuf.Collections;

namespace GarudaUtil
{
    class PhoenixUtil : IDisposable
    {
        public ClusterCredentials Credentials { get; set; }

        public string ConnectionId { get; private set; }

        public RequestOptions Options { get; set; }

        private PhoenixClient _client = null;

        private OpenConnectionResponse _openConnection = null;

        public PhoenixUtil()
        {
            this.ConnectionId = Guid.NewGuid().ToString();

            this.Options = RequestOptions.GetVNetDefaultOptions();

            this.Credentials = new ClusterCredentials(new Uri("http://localhost/"), "", "");
        }

        public void Open()
        {
            
            if (null != _client)
            {
                // Already allocated
            }
            else
            {
                pbc::MapField<string, string> info = new pbc::MapField<string, string>();

                _client = new PhoenixClient(this.Credentials);

                var tOpen = _client.OpenConnectionRequestAsync(this.ConnectionId,
                    info,
                    this.Options);
                tOpen.Wait();

                _openConnection = tOpen.Result;


                // Syncing connection
                ConnectionProperties connProperties = new ConnectionProperties
                {
                    HasAutoCommit = true,
                    AutoCommit = true,
                    HasReadOnly = true,
                    ReadOnly = false,
                    TransactionIsolation = 0,
                    Catalog = "",
                    Schema = "",
                    IsDirty = true
                };
                _client.ConnectionSyncRequestAsync(this.ConnectionId, connProperties, this.Options).Wait();
            }
        }

        public void SystemTables()
        {
            // List system tables
            pbc.RepeatedField<string> list = new pbc.RepeatedField<string>();
            list.Add("SYSTEM TABLE");
            ResultSetResponse tablesResponse = _client.TablesRequestAsync("", "", "", list, true, this.ConnectionId, this.Options).Result;
            //Assert.AreEqual(4, tablesResponse.FirstFrame.Rows.Count);

            foreach(var c in tablesResponse.Signature.Columns)
            {
                Console.Write(string.Format("{0} |", c.ColumnName));
            }
            Console.WriteLine();

            foreach (var t in tablesResponse.FirstFrame.Rows)
            {
                foreach(var v in t.Value)
                {
                    Console.Write(string.Format("{0} |", v.Value[0].ToString()));
                }
                Console.WriteLine();
            }

            // List all table types
            ResultSetResponse tableTypeResponse = _client.TableTypesRequestAsync(this.ConnectionId, this.Options).Result;
            //Assert.AreEqual(6, tableTypeResponse.FirstFrame.Rows.Count);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    if (_openConnection != null)
                    {
                        _client.CloseConnectionRequestAsync(this.ConnectionId, this.Options).Wait();
                        _openConnection = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PhoenixUtil() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
