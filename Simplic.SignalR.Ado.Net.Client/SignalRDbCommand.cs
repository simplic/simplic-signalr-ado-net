using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.Json;

namespace Simplic.SignalR.Ado.Net.Client
{
    public class SignalRDbCommand : DbCommand, IDbCommand
    {
        private SignalRDbConnection dbConnection;
        private SignalRDbTransaction dbTransaction;

        private bool isDirty = true;

        private string commandText;
        private int commandTimeout;
        private CommandType commandType;
        private bool designTimeVisible;
        private UpdateRowSource updatedRowSource;

        public SignalRDbCommand()
        {

        }

        public SignalRDbCommand(string commandText)
        {
            CommandText = commandText;
        }

        public SignalRDbCommand(string commandText, SignalRDbConnection dbConnection)
            : this(commandText)
        {
            this.dbConnection = dbConnection;
            DbConnection = DbConnection;
        }

        public SignalRDbCommand(string commandText, SignalRDbConnection dbConnection, SignalRDbTransaction dbTransaction)
            : this(commandText, dbConnection)
        {
            this.dbTransaction = dbTransaction;
            Transaction = dbTransaction;
        }

        private void AssertResponse(Response response)
        {
            if (response?.Success == false)
                throw new Exception(response.Exception);
        }

        public override int ExecuteNonQuery()
        {
            Assert();
            CreateOrUpdate();

            var response = dbConnection.HubConnectionBuilder.InvokeAsync<ResponseObject<int>>("ExecuteNonQueryAsync", Id).Result;

            AssertResponse(response);

            return response.Object;
        }

        public override object ExecuteScalar()
        {
            Assert();
            CreateOrUpdate();

            var response = dbConnection.HubConnectionBuilder.InvokeAsync<ResponseObject<object>>("ExecuteScalarAsync", Id).Result;

            AssertResponse(response);

            if (response.Object is JsonElement element)
            {
                // TODO: Support all types
                if (element.ValueKind == JsonValueKind.Number)
                    return element.GetInt32();
                if (element.ValueKind == JsonValueKind.String)
                    return element.GetString();
            }

            return response.Object;
        }

        public override void Prepare()
        {
            Assert();
            CreateOrUpdate();

            var response = dbConnection.HubConnectionBuilder.InvokeAsync<Response>("PrepareCommandAsync").Result;
            AssertResponse(response);
        }

        public override void Cancel()
        {
            Assert();
            CreateOrUpdate();

            var response = dbConnection.HubConnectionBuilder.InvokeAsync<Response>("CancelCommandAsync").Result;
            AssertResponse(response);
        }

        protected override DbParameter CreateDbParameter()
        {
            Assert();
            CreateOrUpdate();

            var parameter = new SignalRDbParameter();
            DbParameterCollection.Add(parameter);

            isDirty = true;

            return parameter;
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            Assert();
            CreateOrUpdate();

            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            // dbConnection.HubConnectionBuilder.InvokeAsync<int>("DisposeCommandAsync", Id);

            base.Dispose(disposing);
        }

        private void Assert()
        {
            if (string.IsNullOrWhiteSpace(CommandText))
                throw new Exception("CommandText must not be null or whitespace.");

            if (DbConnection == null)
                throw new Exception("DbConnection not set in SignalRDbCommand");

            // TODO: Add some more assertation
        }

        private CommandModel GetModel()
        {
            return new CommandModel
            {
                CommandText = CommandText,
                TransactionId = dbTransaction?.Id,
                Id = Id
            };
        }

        private void CreateOrUpdate()
        {
            // Check whether any parameter is dirty
            foreach (var parameter in Parameters.OfType<SignalRDbParameter>())
            {
                if (parameter.IsDirty)
                {
                    isDirty = true;
                    parameter.IsDirty = false;
                }
            }

            if (Id == Guid.Empty)
            {
                // Create command
                var idResponse = dbConnection.HubConnectionBuilder.InvokeAsync<ResponseObject<Guid>>("CreateCommandAsync").Result;
                AssertResponse(idResponse);

                Id = idResponse.Object;

                var response = dbConnection.HubConnectionBuilder.InvokeAsync<Response>("UpdateCommandAsync", GetModel()).Result;
                AssertResponse(response);
            }
            else if (isDirty)
            {
                // Update command
                var response = dbConnection.HubConnectionBuilder.InvokeAsync<Response>("UpdateCommandAsync", GetModel()).Result;
                AssertResponse(response);
            }

            isDirty = false;
        }

        public override string CommandText
        {
            get => commandText;
            set
            {
                commandText = value;
                isDirty = true;
            }
        }

        public override int CommandTimeout
        {
            get => commandTimeout;
            set
            {
                commandTimeout = value;
                isDirty = true;
            }
        }

        public override CommandType CommandType
        {
            get => commandType;
            set
            {
                commandType = value;
                isDirty = true;
            }
        }

        public override bool DesignTimeVisible
        {
            get => designTimeVisible;
            set
            {
                designTimeVisible = value;
                isDirty = true;
            }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get => UpdatedRowSource;
            set
            {
                updatedRowSource = value;
                isDirty = true;
            }
        }

        // TODO: Make this better
        protected override DbConnection DbConnection { get => dbConnection; set => dbConnection = value as SignalRDbConnection; }

        protected override DbParameterCollection DbParameterCollection { get; } = new SignalRDbParameterCollection();

        protected override DbTransaction DbTransaction { get; set; }

        /// <summary>
        /// Gets or sets the command id.
        /// </summary>
        public Guid Id { get; set; }
    }
}
