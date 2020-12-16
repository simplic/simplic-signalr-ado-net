using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplic.SignalR.Ado.Net.Server
{
    public class DbConnectionCache
    {
        public int CreateDateTime { get; set; }
        public DbConnection DbConnection { get; set; }
        public string ConnectionId { get; set; }
        public IDictionary<Guid, DbTransaction> Transactions { get; set; } = new Dictionary<Guid, DbTransaction>();
        public IDictionary<Guid, DbCommand> Commands { get; set; } = new Dictionary<Guid, DbCommand>();
    }
}
