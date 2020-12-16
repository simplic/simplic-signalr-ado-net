using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Simplic.SignalR.Ado.Net.Server
{
    public class DatabaseHub : Hub
    {
        private static IDictionary<string, DbConnectionCache> dbConnections = new Dictionary<string, DbConnectionCache>();

        #region [OpenAsync]
        public async Task<OpenConnectionResponse> OpenAsync(OpenConnectionRequest model)
        {
            var response = new OpenConnectionResponse();

            try
            {
                var dbProviderFactory = DbProviderFactories.GetFactory(model.Provider);
                var dbConnection = dbProviderFactory.CreateConnection();
                dbConnection.ConnectionString = model.ConnectionString;

                await dbConnection.OpenAsync();

                response.ConnectionTimeout = dbConnection.ConnectionTimeout;
                response.Database = dbConnection.Database;
                response.ServerVersion = dbConnection.ServerVersion;
                response.DataSource = dbConnection.DataSource;

                // dbConnection.StateChange += DbConnection_StateChange;

                dbConnections[Context.ConnectionId] = new DbConnectionCache
                {
                    ConnectionId = Context.ConnectionId,
                    DbConnection = dbConnection
                };

                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Exception = $"{ex.Message}\r\n{ex.InnerException?.Message ?? "<no-inner-exception>"}";
                response.Success = false;
            }

            return response;

            // DataTable table = DbProviderFactories.GetFactoryClasses();

            // Display each row and column value.
            //foreach (DataRow row in table.Rows)
            //{
            //    foreach (DataColumn column in table.Columns)
            //    {
            //        Console.WriteLine(row[column]);
            //    }
            //}
        }

        private void DbConnection_StateChange(object sender, StateChangeEventArgs e)
        {
            if (e.CurrentState == ConnectionState.Closed || e.CurrentState == ConnectionState.Broken)
            {
                // TODO: Cancel
            }
        }
        #endregion

        #region [ChangeDatabaseAsync]
        public async Task ChangeDatabaseAsync(string databaseName)
        {
            if (dbConnections.ContainsKey(Context.ConnectionId))
            {
                dbConnections[Context.ConnectionId].DbConnection.ChangeDatabase(databaseName);
            }
        }
        #endregion

        #region [Transaction]
        public async Task<Guid> BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            var transactionId = Guid.NewGuid();

            if (dbConnections.ContainsKey(Context.ConnectionId))
            {
                var connection = dbConnections[Context.ConnectionId];
                var transaction = connection.DbConnection.BeginTransaction(isolationLevel);

                connection.Transactions[transactionId] = transaction;
            }

            return transactionId;
        }

        public async Task CommitTransactionAsync(Guid id)
        {
            // TODO: Exception handling
            if (dbConnections.ContainsKey(Context.ConnectionId))
            {
                var connection = dbConnections[Context.ConnectionId];
                var transaction = connection.Transactions[id];
                transaction.Commit();

                connection.Transactions.Remove(id);
            }
        }

        public async Task RollbackTransactionAsync(Guid id)
        {
            // TODO: Exception handling
            if (dbConnections.ContainsKey(Context.ConnectionId))
            {
                var connection = dbConnections[Context.ConnectionId];
                var transaction = connection.Transactions[id];
                transaction.Rollback();

                connection.Transactions.Remove(id);
            }
        }
        #endregion

        public async Task<Guid> CreateCommandAsync()
        {
            var id = Guid.NewGuid();

            if (dbConnections.ContainsKey(Context.ConnectionId))
            {
                var connection = dbConnections[Context.ConnectionId];
                var command = connection.DbConnection.CreateCommand();

                connection.Commands[id] = command;
            }

            return id;
        }

        public async Task DisposeCommandAsync(Guid id)
        {
            if (dbConnections.ContainsKey(Context.ConnectionId))
            {
                var connection = dbConnections[Context.ConnectionId];
                var command = connection.Commands[id];
                command?.Dispose();

                connection.Commands.Remove(id);
            }
        }

        public async Task UpdateCommandAsync(CommandModel model)
        {
            if (dbConnections.ContainsKey(Context.ConnectionId))
            {
                var connection = dbConnections[Context.ConnectionId];
                var command = connection.Commands[model.Id];

                // Assert
                if (model.TransactionId != null && model.TransactionId != Guid.Empty)
                    command.Transaction = connection.Transactions[model.TransactionId.Value];

                command.CommandText = model.CommandText;
            }
        }

        public async Task PrepareCommandAsync(CommandModel model)
        {
            if (dbConnections.ContainsKey(Context.ConnectionId))
            {
                var connection = dbConnections[Context.ConnectionId];
                var command = connection.Commands[model.Id];
                command.Prepare();
            }
        }

        public async Task CancelCommandAsync(CommandModel model)
        {
            if (dbConnections.ContainsKey(Context.ConnectionId))
            {
                var connection = dbConnections[Context.ConnectionId];
                var command = connection.Commands[model.Id];
                command.Cancel();
            }
        }

        public async Task<int> ExecuteNonQueryAsync(Guid id)
        {
            // TODO: Assert connection

            if (dbConnections.ContainsKey(Context.ConnectionId))
            {
                var connection = dbConnections[Context.ConnectionId];
                var command = connection.Commands[id];

                return await command.ExecuteNonQueryAsync();
            }

            return -12;
        }

        public async Task<object> ExecuteScalarAsync(Guid id)
        {
            // TODO: Assert connection

            if (dbConnections.ContainsKey(Context.ConnectionId))
            {
                var connection = dbConnections[Context.ConnectionId];
                var command = connection.Commands[id];

                return await command.ExecuteScalarAsync();
            }

            return null;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (dbConnections.ContainsKey(Context.ConnectionId))
            {
                try
                {
                    var connection = dbConnections[Context.ConnectionId].DbConnection;

                    Console.WriteLine($"Disconnect: {Context.ConnectionId} (server={connection.DataSource}/databsae={connection.Database})");

                    connection.Dispose();
                }
                catch
                {
                    /* swallow */
                }

                dbConnections.Remove(Context.ConnectionId);
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}
