using Apache.Phoenix;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pbc = Google.Protobuf.Collections;

namespace Garuda.Data
{
    public class PhoenixCommand : IDbCommand
    {
        private GarudaExecuteResponse _response = null;

        private PrepareResponse _prepared = null;

        public PhoenixCommand(PhoenixConnection connection)
        {
            this.CommandType = System.Data.CommandType.Text;
            this.Connection = connection;
            this.Parameters = new PhoenixParameterCollection();
        }

        #region IDbCommand Interface

        public string CommandText { get; set; }

        public int CommandTimeout { get; set; }

        public CommandType CommandType { get; set; }

        public IDbConnection Connection
        {
            get { return _connection; }
            set { _connection = (PhoenixConnection)value; }
        }
        private PhoenixConnection _connection;

        IDataParameterCollection IDbCommand.Parameters { get { return this.Parameters; } }

        public PhoenixParameterCollection Parameters { get; private set; }

        public IDbTransaction Transaction
        {
            get { return _transaction; }

            set
            {
                _transaction = (PhoenixTransaction)value;

                // Update connection isolation level.
                bool autoCommit = _transaction == null;
                uint isoLevel = autoCommit ? 0 : PhoenixIsolationLevelMap.GetPhoenixLevel(_transaction.IsolationLevel);
                this._connection.InternalSyncConnectionProperties(autoCommit, isoLevel);
            }
        }

        private PhoenixTransaction _transaction = null;

        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public UpdateRowSource UpdatedRowSource
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

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public IDbDataParameter CreateParameter()
        {
            return new PhoenixParameter();
        }

        public int ExecuteNonQuery()
        {
            GarudaExecuteResponse resp = Execute();

            return -1;
        }

        public IDataReader ExecuteReader()
        {
            GarudaExecuteResponse resp = Execute();
            return new PhoenixDataReader(this, resp);
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            throw new NotImplementedException();
        }

        public object ExecuteScalar()
        {
            throw new NotImplementedException();
        }

        public void Prepare()
        {
            _prepared = this._connection.InternalPrepareStatement(this.CommandText);
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    if(null != _response)
                    {
                        _connection.InternalCloseStatement(_response.StatementId);
                        _response = null;

                        _connection = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PhoenixCommand() {
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

        private GarudaExecuteResponse Execute()
        {
            GarudaExecuteResponse response = _connection.InternalExecuteRequest(this._prepared, 
                this.CommandText, this.Parameters);

            return response;
        }

    }

}
