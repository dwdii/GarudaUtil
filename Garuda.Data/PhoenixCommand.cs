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
    /// <summary>
    /// The PhoenixCommand class encapsulates an SQL command that can be executed on a Phoenix query server.
    /// </summary>
    public class PhoenixCommand : IDbCommand
    {
        private GarudaExecuteResponse _response = null;

        private PrepareResponse _prepared = null;

        /// <summary>
        /// Constructs a new instance of a PhoenixCommand class associated with the
        /// specified PhoenixConnection.
        /// </summary>
        /// <param name="connection"></param>
        public PhoenixCommand(PhoenixConnection connection)
        {
            this.CommandType = System.Data.CommandType.Text;
            this.Connection = connection;
            this.Parameters = new PhoenixParameterCollection();
        }

        #region IDbCommand Interface

        /// <summary>
        /// Gets or sets the text command to run against the data source.
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
        /// </summary>
        public int CommandTimeout { get; set; }

        /// <summary>
        /// Indicates or specifies how the CommandText property is interpreted.
        /// </summary>
        public CommandType CommandType { get; set; }

        /// <summary>
        /// Gets or sets the IDbConnection used by this instance of the IDbCommand.
        /// </summary>
        public IDbConnection Connection
        {
            get { return _connection; }
            set { _connection = (PhoenixConnection)value; }
        }
        private PhoenixConnection _connection;

        /// <summary>
        /// Gets the IDataParameterCollection.
        /// </summary>
        IDataParameterCollection IDbCommand.Parameters { get { return this.Parameters; } }

        /// <summary>
        /// Gets the PhoenixParameterCollection.
        /// </summary>
        public PhoenixParameterCollection Parameters { get; private set; }

        /// <summary>
        /// Gets or sets the transaction within which the Command object of a .NET Framework data provider executes.
        /// </summary>
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

        /// <summary>
        /// Gets or sets how command results are applied to the DataRow when used by the Update method of a DbDataAdapter.
        /// 
        /// This property is not implemented currently.
        /// </summary>
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

        /// <summary>
        /// Attempts to cancels the execution of an IDbCommand.
        /// 
        /// This property is not implemented currently.
        /// </summary>
        public void Cancel()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new instance of an IDbDataParameter object.
        /// </summary>
        /// <returns></returns>
        public IDbDataParameter CreateParameter()
        {
            return new PhoenixParameter();
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object of a .NET Framework data provider, and returns the number of rows affected.
        /// </summary>
        /// <returns></returns>
        public int ExecuteNonQuery()
        {
            GarudaExecuteResponse resp = Execute();

            return -1;
        }

        /// <summary>
        /// Executes the CommandText against the Connection and builds an IDataReader.
        /// </summary>
        /// <returns></returns>
        public IDataReader ExecuteReader()
        {
            GarudaExecuteResponse resp = Execute();
            return new PhoenixDataReader(this, resp);
        }

        /// <summary>
        /// Executes the CommandText against the Connection, and builds an IDataReader using one of the CommandBehavior values.
        /// 
        /// Curren the CommandBehavior is ignored.
        /// </summary>
        /// <param name="behavior"></param>
        /// <returns></returns>
        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            return ExecuteReader();
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the resultset returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <returns></returns>
        public object ExecuteScalar()
        {
            object oVal = null;
            using (IDataReader dr = this.ExecuteReader())
            {
                if(dr.Read())
                {
                    oVal = dr.GetValue(0);
                }
            }

            return oVal;
        }

        /// <summary>
        /// Creates a prepared (or compiled) version of the command on the data source.
        /// </summary>
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
