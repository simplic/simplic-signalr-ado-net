using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Simplic.SignalR.Ado.Net.Client
{
    public class SignalRDbTransaction : DbTransaction
    {
        private SignalRDbConnection dbConnection;

        public SignalRDbTransaction(SignalRDbConnection dbConnection, IsolationLevel isolationLevel, Guid id)
        {
            DbConnection = dbConnection;
            this.dbConnection = dbConnection;

            IsolationLevel = isolationLevel;
            Id = id;
        }

        public override void Commit()
        {
            dbConnection.HubConnectionBuilder.InvokeAsync("CommitTransactionAsync", Id).Wait();
        }

        public override void Rollback()
        {
            dbConnection.HubConnectionBuilder.InvokeAsync("RollbackTransactionAsync", Id).Wait();
        }
        
        public override IsolationLevel IsolationLevel { get; }

        protected override DbConnection DbConnection { get; }

        public Guid Id { get; }
    }
}
