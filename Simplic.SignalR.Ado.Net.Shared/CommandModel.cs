using System;

namespace Simplic.SignalR.Ado.Net
{
    public class CommandModel
    {
        public string CommandText { get; set; }
        public Guid? TransactionId { get; set; }
        public Guid Id { get; set; }
    }
}
