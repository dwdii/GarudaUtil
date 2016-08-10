using Apache.Phoenix;
using PhoenixSharp;
using PhoenixSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pbc = Google.Protobuf.Collections;

namespace Garuda.Data
{
    /// <summary>
    /// Represents an open connection to a Phoenix Query Server. This class cannot be inherited.
    /// </summary>
    public sealed class PhoenixConnection : IDbConnection
    {
        #region Private Data Members
        private PhoenixClient _client = null;

        private OpenConnectionResponse _openConnection = null;

        #endregion

        internal struct Constants
        {
            public const string SqlSelectTableMetaData = "SELECT TABLE_SCHEM, TABLE_NAME, TABLE_TYPE FROM SYSTEM.CATALOG WHERE TABLE_TYPE IS NOT NULL";

            public const string NamePhoenixTables = "Phoenix Tables";
        }

        /// <summary>
        /// Gets or sets the ClusterCredentials object for this connection.
        /// </summary>
        public ClusterCredentials Credentials { get; set; }

        /// <summary>
        /// The connection ID of the connection.
        /// </summary>
        public string ConnectionId { get; private set; }

        /// <summary>
        /// Gets or sets the Phoenix request options
        /// </summary>
        public RequestOptions Options { get; set; }

        internal ConnectionProperties ConnectionProperties { get; private set; }

        #region IDbConnection Interface

        /// <summary>
        /// Gets or sets the string used to open a database.
        /// 
        /// Server=myphoenixserver.domain.com;User ID=myuser;Password=mypwd;CredentialUri=http://myazurecredurl;Request Timeout=30000" 
        /// 
        /// Credentials are only used by the Microsoft.Phoenix.Client in gateway-mode (Azure).
        /// 
        /// Request Timeout is in milliseconds.
        /// </summary>
        /// <remarks>The ConnectionString property can be set only while the connection is closed.</remarks>
        /// <see cref="https://github.com/Azure/hdinsight-phoenix-sharp/blob/master/PhoenixSharp/PhoenixClient.cs"/>
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }

            set
            {
                if(this.State == ConnectionState.Closed)
                {
                    ParseConnectionString(value);
                    _connectionString = value;
                }
            }
        }

        private string _connectionString = null;

        /// <summary>
        /// Gets the time to wait while trying to establish a connection before terminating the attempt and generating an error.
        /// </summary>
        public int ConnectionTimeout
        {
            get { return this.Options.TimeoutMillis / 1000; }
        }

        /// <summary>
        /// Throws NotImplementedException.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public string Database
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Indicates the state of the PhoenixConnection during the most recent network operation performed on the connection.
        /// </summary>
        public ConnectionState State { get; private set; }

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        /// <remarks>Once the transaction has completed, you must explicitly commit or roll back the transaction by using the Commit or Rollback methods.</remarks>
        /// <returns>An object representing the new transaction.</returns>
        public IDbTransaction BeginTransaction()
        {
            return this.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Starts a database transaction with the specified isolation level.
        /// </summary>
        /// <param name="il"></param>
        /// <returns></returns>
        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return new PhoenixTransaction(this, il);
        }

        /// <summary>
        /// Opens a database connection with the property settings specified by the ConnectionString.
        /// </summary>
        public void Open()
        {
            Task.Run(() => PrivateOpenAsync()).Wait();
        }


        /// <summary>
        /// Closes the connection to the database. This is the preferred method of closing any open connection.
        /// </summary>
        public void Close()
        {
            if (_openConnection != null)
            {
                Task.Factory.StartNew(() =>_client.CloseConnectionRequestAsync(this.ConnectionId, this.Options)).Wait();
                _openConnection = null;
                _client = null;

                // Flag connection closed
                this.State = ConnectionState.Closed;
            }
        }

        /// <summary>
        /// Throws NotImplementedException.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <returns></returns>
        public IDbCommand CreateCommand()
        {
            return new PhoenixCommand(this); 
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the PhoenixConnection class.
        /// </summary>
        public PhoenixConnection()
        {
            this.ConnectionId = Guid.NewGuid().ToString();

            this.Options = RequestOptions.GetVNetDefaultOptions();

            this.Credentials = null;

            this.State = ConnectionState.Closed;

            this.ConnectionProperties = DefaultConnectionProps();
        }

        /// <summary>
        /// Initializes a new instance of the PhoenixConnection class when given a 
        /// string that contains the connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        public PhoenixConnection(string connectionString) : this()
        {
            this.ConnectionString = connectionString;
        }

        /// <summary>
        /// Returns a DataTable containing meta data about the available tables.
        /// </summary>
        /// <returns></returns>
        public DataTable GetTables()
        {
            if(this.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(string.Format("ConnectionState must be Open. Currently {0}.", this.State));
            }

            DataTable dt = new DataTable(Constants.NamePhoenixTables);
            using (IDbCommand cmd = this.CreateCommand())
            {
                cmd.CommandText = Constants.SqlSelectTableMetaData;
                using (IDataReader dr = cmd.ExecuteReader())
                {
                    dt.BeginLoadData();
                    dt.Load(dr);
                    dt.EndLoadData();
                }
            }

            return dt;
        }

        #region Internal Methods

        internal ResultSetResponse InternalColumnsRequest(string catalog, string schemaPattern, string tablePattern, string columnPattern)
        {
            Task<ResultSetResponse> tResp =_client.ColumnsRequestAsync(catalog, schemaPattern, tablePattern, columnPattern, this.ConnectionId, this.Options);
            tResp.Wait();

            return tResp.Result;
        }

        internal ResultSetResponse InternalTablesRequest(pbc.RepeatedField<string> list)
        {
            ResultSetResponse response = null;

            Task<ResultSetResponse> tResp = _client.TablesRequestAsync("", "", "", list, true, this.ConnectionId, this.Options);

            tResp.Wait();

            response = tResp.Result;

            return response;
        }

        internal async Task<GarudaExecuteResponse> InternalExecuteRequestAsync(PrepareResponse prepared, string sql, 
            PhoenixParameterCollection parameterValues)
        {
            CreateStatementResponse tStmt = null;
            ExecuteResponse tResp = null;
            GarudaExecuteResponse ourResp = new GarudaExecuteResponse();

            if(null == prepared)
            {
                try
                {
                    // Not prepared....
                    tStmt = await _client.CreateStatementRequestAsync(this.ConnectionId, this.Options);
                    
                    ourResp.StatementId = tStmt.StatementId;

                    tResp = await _client.PrepareAndExecuteRequestAsync(this.ConnectionId,
                        sql,
                        ulong.MaxValue,
                        ourResp.StatementId,
                        this.Options);
                }
                catch (Exception ex)
                {
                    InternalCloseStatementAsync(tStmt.StatementId);

                    throw;
                }
            }
            else
            {
                // Prepared and possibly with parameters.
                pbc.RepeatedField<TypedValue> pbParamValues = parameterValues.AsRepeatedFieldTypedValue();

                tResp = await _client.ExecuteRequestAsync(prepared.Statement, pbParamValues, 100,
                    parameterValues.Count > 0, this.Options);
            }

            ourResp.Response = tResp;

            return ourResp;
        }

        internal PrepareResponse InternalPrepareStatement(string sql)
        {
            Task<PrepareResponse> tResp = Task.Factory.StartNew(() => _client.PrepareRequestAsync(this.ConnectionId, sql, ulong.MaxValue, this.Options)).Result;

            return tResp.Result;
        }

        internal ExecuteBatchResponse InternalExecuteBatch(uint statementId, pbc::RepeatedField<UpdateBatch> updates)
        {
            Task<ExecuteBatchResponse> tResp = Task.Factory.StartNew(() => _client.ExecuteBatchRequestAsync(this.ConnectionId, statementId, updates, this.Options)).Result;
            
            return tResp.Result;
        }

        internal async void InternalCloseStatementAsync(uint statementId)
        {
            await _client.CloseStatementRequestAsync(this.ConnectionId, statementId, this.Options);
        }

        internal void InternalCloseStatement(uint statementId)
        {
            Task.Factory.StartNew(() => InternalCloseStatementAsync(statementId)).Wait();
        }

        internal async Task<FetchResponse> InternalFetchAsync(uint statementId, ulong offset, int max)
        {
            FetchResponse resp = await _client.FetchRequestAsync(this.ConnectionId, statementId, offset, (uint)max, this.Options);

            return resp;
        }

        internal ConnectionSyncResponse InternalSyncConnectionProperties(bool autoCommit, uint isolationLevel)
        {
            this.ConnectionProperties.AutoCommit = autoCommit;
            this.ConnectionProperties.TransactionIsolation = isolationLevel;
            this.ConnectionProperties.IsDirty = true;
            Task<ConnectionSyncResponse> tResp = Task.Factory.StartNew(() => _client.ConnectionSyncRequestAsync(this.ConnectionId, 
                this.ConnectionProperties, 
                this.Options)).Result;
            tResp.Wait();

            return tResp.Result;
        }

        internal void CommitTransaction()
        {
           Task.Factory.StartNew(() => this._client.CommitRequestAsync(this.ConnectionId, this.Options)).Wait();
        }

        internal void RollbackTransction()
        {
            Task.Factory.StartNew(() => this._client.RollbackRequestAsync(this.ConnectionId, this.Options)).Wait();
        }
        #endregion

        #region Private Methods
        private ConnectionProperties DefaultConnectionProps()
        {
            return new ConnectionProperties()
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
        }
        private void ParseConnectionString(string value)
        {
            if(null == value)
            {
                throw new ArgumentNullException("ConnectionString");
            }

            string[] parts = value.Split(';');
            string credsUser = null;
            string credsPasswd = null;
            string credsUri = null;

            foreach (string part in parts)
            {
                // Split the tuple
                string[] tuple = part.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                // Protect against missing values.
                if(tuple.Length == 2)
                {
                    switch (tuple[0].ToLower())
                    {
                        case ("data source"):
                        case ("server"):
                            string[] serverAndPort = tuple[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            this.Options.AlternativeHost = serverAndPort[0];
                            if(serverAndPort.Length > 1)
                            {
                                this.Options.Port = Convert.ToInt32(serverAndPort[1]);
                            }
                            break;

                        case ("uid"):
                        case ("userid"):
                        case ("username"):
                        case ("user id"):
                            credsUser = tuple[1];
                            break;

                        case ("password"):
                            credsPasswd = tuple[1];
                            break;

                        case ("credentialuri"):
                            credsUri = tuple[1];
                            break;

                        case ("requesttimeout"):
                        case ("request timeout"):
                            this.Options.TimeoutMillis = Convert.ToInt32(tuple[1]);
                            break;
                    }
                }
            }

            // Credentials
            if (null != credsUser && null != credsPasswd && null != credsUri)
            {
                this.Credentials = new ClusterCredentials(new Uri(credsUri), credsUser, credsPasswd);
            }
        }

        private async Task PrivateOpenAsync()
        {
            if (null != _client)
            {
                // Already allocated
            }
            else
            {
                // Update state....
                this.State = ConnectionState.Connecting;

                pbc::MapField<string, string> info = new pbc::MapField<string, string>();

                // Spin up Microsoft.Phoenix.Client
                _client = new PhoenixClient(this.Credentials);

                // Initiate connection
                var tOpen = await _client.OpenConnectionRequestAsync(this.ConnectionId,
                    info,
                    this.Options);
                //tOpen.Wait();
                _openConnection = tOpen;

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
                await _client.ConnectionSyncRequestAsync(this.ConnectionId, connProperties, this.Options);

                // Connected.
                this.State = ConnectionState.Open;
            }
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
                    // Dispose managed state (managed objects).
                    Close();
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

        /// <summary>
        /// Releases all resources used by the Component.
        /// </summary>
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
