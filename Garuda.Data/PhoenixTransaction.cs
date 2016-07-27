using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garuda.Data
{
    public sealed class PhoenixTransaction : IDbTransaction
    {
        private bool _hasCommitted = false;

        internal PhoenixTransaction(PhoenixConnection connection, IsolationLevel il)
        {
            if (null == connection)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            this._connection = connection;
            this._phoneixTxIsoLevel = PhoenixIsolationLevelMap.GetPhoenixLevel(il);
        }

        #region IDbTransction Interface

        /// <summary>
        /// Specifies the Connection object to associate with the transaction.
        /// </summary>
        public IDbConnection Connection { get { return _connection; } }
        private PhoenixConnection _connection = null;

        public IsolationLevel IsolationLevel
        {
            get { return PhoenixIsolationLevelMap.GetIsolationLevel(this._phoneixTxIsoLevel); }
        }

        private uint _phoneixTxIsoLevel = 0;

        public void Commit()
        {
            this._connection.CommitTransaction();
            this._hasCommitted = true;
        }

        public void Rollback()
        {
            this._connection.RollbackTransction();
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Rollback uncommited transaction statements.
                    if(!this._hasCommitted)
                    {
                        this.Rollback();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PhoenixTransaction() {
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
