using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Simplic.SignalR.Ado.Net.Client
{
    /// <summary>
    /// Represents a SignalR-ADO.Net connection
    /// </summary>
    public class SignalRDbConnection : DbConnection, IDbConnection
    {
        #region Fields
        private HubConnection hubConnectionBuilder;
        private int connectionTimeout;
        private string database;
        private ConnectionState state;
        private string dataSource;
        private string serverVersion;
        #endregion

        #region [Open]
        /// <summary>
        /// Opens a new connection and establish a connection to the signalr client
        /// </summary>
        public override void Open()
        {
            try
            {
                if (State == ConnectionState.Connecting || State == ConnectionState.Open)
                    throw new Exception($"Client already in connecting or open state: {State}");

                state = ConnectionState.Connecting;

                var result = ConnectionStringParser.ParseConnectionString(ConnectionString, out ConnectionStringParameter parameter);

                var url = parameter.Url;

                if (!result)
                    throw new Exception(parameter.ErrorMessage);

                url = url.Trim();
                if (!url.EndsWith("/"))
                    url = $"{url}/";

                hubConnectionBuilder = new HubConnectionBuilder()
                    .WithUrl($"{url}database")
                    .WithAutomaticReconnect()
                    .Build();

                hubConnectionBuilder.StartAsync().Wait();

                // Get database and timeout information
                var connectionResult = hubConnectionBuilder.InvokeAsync<OpenConnectionResponse>("OpenAsync", new OpenConnectionRequest
                {
                    ConnectionString = parameter.DbConnectionString,
                    Provider = parameter.Provider
                }).Result;

                if (connectionResult.Success)
                {
                    SetConnectionDetails(connectionResult);
                }
                else
                {
                    throw new Exception(connectionResult.Exception);
                }
            }
            catch (Exception)
            {
                state = ConnectionState.Closed;
                throw;
            }

            state = ConnectionState.Open;
        }

        public override async Task OpenAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (State == ConnectionState.Connecting || State == ConnectionState.Open)
                    throw new Exception($"Client already in connecting or open state: {State}");

                state = ConnectionState.Connecting;

                var result = ConnectionStringParser.ParseConnectionString(ConnectionString, out ConnectionStringParameter parameter);

                var url = parameter.Url;

                if (!result)
                    throw new Exception(parameter.ErrorMessage);

                url = url.Trim();
                if (!url.EndsWith("/"))
                    url = $"{url}/";

                hubConnectionBuilder = new HubConnectionBuilder()
                    .WithUrl($"{url}database")
                    .WithAutomaticReconnect()
                    .Build();

                await hubConnectionBuilder.StartAsync();

                // Get database and timeout information
                var connectionResult = await hubConnectionBuilder.InvokeAsync<OpenConnectionResponse>("OpenAsync", new OpenConnectionRequest
                {
                    ConnectionString = parameter.DbConnectionString,
                    Provider = parameter.Provider
                });

                if (connectionResult.Success)
                {
                    SetConnectionDetails(connectionResult);
                }
                else
                {
                    throw new Exception(connectionResult.Exception);
                }
            }
            catch (Exception)
            {
                state = ConnectionState.Closed;
                throw;
            }

            state = ConnectionState.Open;
        }

        private void SetConnectionDetails(OpenConnectionResponse response)
        {
            database = response.Database;
            connectionTimeout = response.ConnectionTimeout;
            serverVersion = response.ServerVersion;
            dataSource = response.DataSource;
        }
        #endregion

        #region [Close]
        /// <summary>
        /// Close the current connection
        /// </summary>
        public override void Close()
        {
            try
            {
                if (hubConnectionBuilder != null)
                    hubConnectionBuilder.StopAsync().Wait();
            }
            catch
            {
                /* swallow */
            }

            state = ConnectionState.Closed;
        }
        #endregion

        #region [ChangeDatabase]
        /// <summary>
        /// Change the currently selected database
        /// </summary>
        /// <param name="databaseName">Database name</param>
        public override void ChangeDatabase(string databaseName)
        {
            if (State != ConnectionState.Open)
                throw new Exception($"Could not change database, invalid connection state {State}");

            hubConnectionBuilder.InvokeAsync<OpenConnectionResponse>("ChangeDatabaseAsync", databaseName).Wait();
        }
        #endregion


        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            var id = hubConnectionBuilder.InvokeAsync<Guid>("BeginTransactionAsync", isolationLevel).Result;
            return new SignalRDbTransaction(this, isolationLevel, id);
        }
        
        #region [CreateCommand]
        /// <summary>
        /// Creates a new <see cref="SignalRDbCommand"/>
        /// </summary>
        /// <returns>Command instance</returns>
        protected override DbCommand CreateDbCommand()
        {
            return new SignalRDbCommand
            {
                Connection = this
            };
        }
        #endregion

        #region [Dispose]
        /// <summary>
        /// Close and dispose the current connection
        /// </summary>
        public new void Dispose()
        {
            try
            {
                Close();
            }
            catch
            {
                /* swallow */
            }
        }
        #endregion

        #region Public Member
        /// <summary>
        /// Gets or sets the SignalR-ADO.net combined connection string. E.g. {http://sample.com}key=value;server=srv;dbn=db
        /// </summary>
        public override string ConnectionString { get; set; }

        /// <summary>
        /// Gets the connection timeout
        /// </summary>
        public override int ConnectionTimeout => connectionTimeout;

        /// <summary>
        /// Gets the database name
        /// </summary>
        public override string Database => database;

        /// <summary>
        /// Gets the actual connection state
        /// </summary>
        public override ConnectionState State => state;

        /// <summary>
        /// Gets the data source
        /// </summary>
        public override string DataSource => dataSource;

        /// <summary>
        /// Gets the server version
        /// </summary>
        public override string ServerVersion => serverVersion;

        /// <summary>
        /// Gets the current hub connection
        /// </summary>
        internal HubConnection HubConnectionBuilder => hubConnectionBuilder;
        #endregion
    }
}
