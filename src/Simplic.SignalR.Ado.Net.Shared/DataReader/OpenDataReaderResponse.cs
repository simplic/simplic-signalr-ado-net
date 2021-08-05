using System;
using System.Collections.Generic;
using System.Text;

namespace Simplic.SignalR.Ado.Net
{
    public class OpenDataReaderResponse
    {
        public Guid Id { get; set; }
        public int Depth { get; set; }

        public bool IsClosed { get; set; }

        public int RecordsAffected { get; set; }

        public int FieldCount { get; set; }
        public bool HasRows { get; set; }
        public int VisibleFieldCount { get; set; }
        public byte[] Schema { get; set; }
        public byte[] SchemaData { get; set; }
    }
}
