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
    public class PhoenixConnection : IDbConnection
    {
        #region Private Data Members
        private PhoenixClient _client = null;

        private OpenConnectionResponse _openConnection = null;

        #endregion

        public ClusterCredentials Credentials { get; set; }

        public string ConnectionId { get; private set; }

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

        public int ConnectionTimeout
        {
            get { return this.Options.TimeoutMillis; }
        }

        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public string Database
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ConnectionState State { get; private set; }

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        /// <remarks>Once the transaction has completed, you must explicitly commit or roll back the transaction by using the Commit or Rollback methods.</remarks>
        /// <returns>An object representing the new transaction.</returns>
        public IDbTransaction BeginTransaction()
        {
            return new PhoenixTransaction(this, IsolationLevel.ReadCommitted);
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return new PhoenixTransaction(this, il);
        }

        public void Open()
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

                // Connected.
                this.State = ConnectionState.Open;
            }
        }

        public void Close()
        {
            if (_openConnection != null)
            {
                _client.CloseConnectionRequestAsync(this.ConnectionId, this.Options).Wait();
                _openConnection = null;
                _client = null;

                // Flag connection closed
                this.State = ConnectionState.Closed;
            }
        }

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


        public PhoenixConnection()
        {
            this.ConnectionId = Guid.NewGuid().ToString();

            this.Options = RequestOptions.GetVNetDefaultOptions();

            this.Credentials = null;

            this.State = ConnectionState.Closed;

            this.ConnectionProperties = DefaultConnectionProps();
        }

        public PhoenixConnection(string connectionString) : this()
        {
            this.ConnectionString = connectionString;
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

        internal GarudaExecuteResponse InternalExecuteRequest(PrepareResponse prepared, string sql, 
            PhoenixParameterCollection parameterValues)
        {
            Task<CreateStatementResponse> tStmt = null;
            Task<ExecuteResponse> tResp = null;
            GarudaExecuteResponse ourResp = new GarudaExecuteResponse();

            if(null == prepared)
            {
                try
                {
                    // Not prepared....
                    tStmt = _client.CreateStatementRequestAsync(this.ConnectionId, this.Options);
                    tStmt.Wait();
                    ourResp.StatementId = tStmt.Result.StatementId;

                    tResp = _client.PrepareAndExecuteRequestAsync(this.ConnectionId,
                        sql,
                        ulong.MaxValue,
                        ourResp.StatementId,
                        this.Options);
                }
                catch (Exception ex)
                {
                    if (tStmt.IsCompleted)
                    {
                        InternalCloseStatement(tStmt.Result.StatementId);
                    }

                    throw;
                }
            }
            else
            {
                // Prepared and possibly with parameters.
                pbc.RepeatedField<TypedValue> pbParamValues = parameterValues.AsRepeatedFieldTypedValue();

                tResp = _client.ExecuteRequestAsync(prepared.Statement, pbParamValues, 100,
                    parameterValues.Count > 0, this.Options);
            }

            tResp.Wait();
            ourResp.Response = tResp.Result;

            return ourResp;
        }

        internal PrepareResponse InternalPrepareStatement(string sql)
        {
            Task<PrepareResponse> tResp = _client.PrepareRequestAsync(this.ConnectionId, sql, ulong.MaxValue, this.Options);
            tResp.Wait();

            return tResp.Result;
        }

        internal ExecuteBatchResponse InternalExecuteBatch(uint statementId, pbc::RepeatedField<UpdateBatch> updates)
        {
            Task<ExecuteBatchResponse> tResp = _client.ExecuteBatchRequestAsync(this.ConnectionId, statementId, updates, this.Options);
            tResp.Wait();

            return tResp.Result;
        }

        internal void InternalCloseStatement(uint statementId)
        {
            _client.CloseStatementRequestAsync(this.ConnectionId, statementId, this.Options).Wait();
        }

        internal async Task<FetchResponse> InternalFetchAsync(uint statementId, ulong offset, int max)
        {
            FetchResponse resp = await _client.FetchRequestAsync(this.ConnectionId, statementId, offset, (uint)max, this.Options);

            return resp;
        }

        internal void InternalSyncConnectionProperties(bool autoCommit, uint isolationLevel)
        {
            this.ConnectionProperties.AutoCommit = autoCommit;
            this.ConnectionProperties.TransactionIsolation = isolationLevel;
            this.ConnectionProperties.IsDirty = true;
            Task<ConnectionSyncResponse> tResp = _client.ConnectionSyncRequestAsync(this.ConnectionId, 
                this.ConnectionProperties, 
                this.Options);
            tResp.Wait();
        }

        internal void CommitTransaction()
        {
            Task<CommitResponse> tResp = this._client.CommitRequestAsync(this.ConnectionId, this.Options);
            tResp.Wait();
        }

        internal void RollbackTransction()
        {
            Task<RollbackResponse> tResp = this._client.RollbackRequestAsync(this.ConnectionId, this.Options);
            tResp.Wait();
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
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
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
