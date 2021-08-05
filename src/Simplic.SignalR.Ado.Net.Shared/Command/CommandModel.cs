using System;
using System.Collections.Generic;

namespace Simplic.SignalR.Ado.Net
{
    public class CommandModel
    {
        public string CommandText { get; set; }
        public Guid? TransactionId { get; set; }
        public IList<CommandParameter> Parameters { get; set; } = new List<CommandParameter>();
        public Guid Id { get; set; }
    }
}
