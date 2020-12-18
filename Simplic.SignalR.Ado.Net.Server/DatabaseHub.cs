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

        /// <summary>
        /// Tries to find a command in a cached db-connection
        /// </summary>
        /// <param name="dbConnectionCache">Connection cache instance</param>
        /// <param name="id">Unique command id</param>
        /// <param name="command">Command instance if found</param>
        /// <returns></returns>
        private bool TryGetCommand(DbConnectionCache dbConnectionCache, Guid id, out DbCommand command)
        {
            // For security reason, we do not allow empty guids
            if (id == Guid.Empty)
            {
                command = null;
                return false;
            }

            if (dbConnectionCache.Commands.ContainsKey(id))
            {
                command = dbConnectionCache.Commands[id];
                return true;
            }

            command = null;
            return false;
        }

        public async Task<ResponseObject<Guid>> CreateCommandAsync()
        {
            try
            {
                var id = Guid.NewGuid();

                if (TryGetConnection(out DbConnectionCache connection))
                {
                    var command = connection.DbConnection.CreateCommand();
                    connection.Commands[id] = command;
                }
                else
                    return GetConnectionNotFoundReponse<Guid>();

                return GetSuccessReponse(id);
            }
            catch (Exception ex)
            {
                return new ResponseObject<Guid> { Exception = ex.Message, Success = false };
            }
        }

        public async Task<Response> DisposeCommandAsync(Guid id)
        {
            try
            {
                if (TryGetConnection(out DbConnectionCache connection))
                {
                    if (TryGetCommand(connection, id, out DbCommand command))
                    {
                        command.Dispose();
                        connection.Commands.Remove(id);
                    }
                    else
                    {
                        return GetCommandNotFoundReponse(id);
                    }
                }
                else
                    return GetConnectionNotFoundReponse<Guid>();

                return GetSuccessReponse(id);
            }
            catch (Exception ex)
            {
                return new Response { Exception = ex.Message, Success = false };
            }
        }

        public async Task<Response> UpdateCommandAsync(CommandModel model)
        {
            try
            {
                if (TryGetConnection(out DbConnectionCache connection))
                {
                    if (TryGetCommand(connection, model.Id, out DbCommand command))
                    {
                        // Assert
                        if (model.TransactionId != null && model.TransactionId != Guid.Empty)
                            command.Transaction = connection.Transactions[model.TransactionId.Value];

                        command.CommandText = model.CommandText;
                    }
                    else
                    {
                        return GetCommandNotFoundReponse(model.Id);
                    }
                }
                else
                    return GetConnectionNotFoundReponse();

                return GetSuccessReponse();
            }
            catch (Exception ex)
            {
                return new Response { Exception = ex.Message, Success = false };
            }
        }

        public async Task<Response> PrepareCommandAsync(CommandModel model)
        {
            try
            {
                if (TryGetConnection(out DbConnectionCache connection))
                {
                    if (TryGetCommand(connection, model.Id, out DbCommand command))
                    {
                        command.Prepare();
                    }
                    else
                    {
                        return GetCommandNotFoundReponse(model.Id);
                    }
                }
                else
                    return GetConnectionNotFoundReponse();

                return GetSuccessReponse();
            }
            catch (Exception ex)
            {
                return new Response { Exception = ex.Message, Success = false };
            }
        }

        public async Task<Response> CancelCommandAsync(CommandModel model)
        {
            try
            {
                if (TryGetConnection(out DbConnectionCache connection))
                {
                    if (TryGetCommand(connection, model.Id, out DbCommand command))
                    {
                        command.Cancel();
                    }
                    else
                    {
                        return GetCommandNotFoundReponse(model.Id);
                    }
                }
                else
                    return GetConnectionNotFoundReponse();

                return GetSuccessReponse();
            }
            catch (Exception ex)
            {
                return new Response { Exception = ex.Message, Success = false };
            }
        }

        public async Task<ResponseObject<int>> ExecuteNonQueryAsync(Guid id)
        {
            try
            {
                if (TryGetConnection(out DbConnectionCache connection))
                {
                    if (TryGetCommand(connection, id, out DbCommand command))
                    {
                        return GetSuccessReponse(await command.ExecuteNonQueryAsync());
                    }
                    else
                    {
                        return GetCommandNotFoundReponse<int>(id);
                    }
                }
                else
                    return GetConnectionNotFoundReponse<int>();
            }
            catch (Exception ex)
            {
                return new ResponseObject<int> { Exception = ex.Message, Success = false };
            }
        }

        public async Task<ResponseObject<object>> ExecuteScalarAsync(Guid id)
        {
            try
            {
                if (TryGetConnection(out DbConnectionCache connection))
                {
                    if (TryGetCommand(connection, id, out DbCommand command))
                    {
                        return GetSuccessReponse(await command.ExecuteScalarAsync());
                    }
                    else
                    {
                        return GetCommandNotFoundReponse<object>(id);
                    }
                }
                else
                    return GetConnectionNotFoundReponse<object>();
            }
            catch (Exception ex)
            {
                return new ResponseObject<object> { Exception = ex.Message, Success = false };
            }
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


        #region Connection cache
        /// <summary>
        /// Tries to find a cached database connection by the SingalR connection id
        /// </summary>
        /// <param name="dbConnectionCache">Returns the connection if found</param>
        /// <returns>True if the connection was found, in any other case false</returns>
        private bool TryGetConnection(out DbConnectionCache dbConnectionCache)
        {
            if (dbConnections.ContainsKey(Context.ConnectionId))
            {
                dbConnectionCache = dbConnections[Context.ConnectionId];
                return true;
            }

            dbConnectionCache = null;
            return false;
        }
        #endregion

        #region Reponse
        /// <summary>
        /// Creates a connection not found response
        /// </summary>
        /// <returns>Reponse with success false</returns>
        private Response GetConnectionNotFoundReponse()
        {
            return new Response { Exception = $"Could not find connection {Context.ConnectionId}", Success = false };
        }


        /// <summary>
        /// Creates a generic connection not found response
        /// </summary>
        /// <returns>Reponse with success false</returns>
        private ResponseObject<T> GetConnectionNotFoundReponse<T>()
        {
            return new ResponseObject<T> { Exception = $"Could not find connection {Context.ConnectionId}", Success = false };
        }

        /// <summary>
        /// Creates a command not found response
        /// </summary>
        /// <param name="commandId">Unique command id</param>
        /// <returns>Reponse with success false</returns>
        private Response GetCommandNotFoundReponse(Guid commandId)
        {
            return new Response { Exception = $"Could not find command {Context.ConnectionId}/{commandId}", Success = false };
        }


        /// <summary>
        /// Creates a generic command not found response
        /// </summary>
        /// <param name="commandId">Unique command id</param>
        /// <returns>Reponse with success false</returns>
        private ResponseObject<T> GetCommandNotFoundReponse<T>(Guid commandId)
        {
            return new ResponseObject<T> { Exception = $"Could not find connection {Context.ConnectionId}/{commandId}", Success = false };
        }

        /// <summary>
        /// Creates a success response
        /// </summary>
        /// <returns>Reponse with success true</returns>
        private Response GetSuccessReponse()
        {
            return new Response { Success = true, Exception = "" };
        }


        /// <summary>
        /// Creates a generic success response
        /// </summary>
        /// <param name="obj">Object to pass to the client</param>
        /// <returns>Reponse with success true</returns>
        private ResponseObject<T> GetSuccessReponse<T>(T obj)
        {
            return new ResponseObject<T> { Success = true, Exception = "", Object = obj };
        }
        #endregion
    }
}
