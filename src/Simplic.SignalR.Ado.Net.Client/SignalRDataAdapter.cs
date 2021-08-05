using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Simplic.SignalR.Ado.Net.Client
{
    public class SignalRDataAdapter : DbDataAdapter
    {
        public SignalRDataAdapter()
        {
        }

        public SignalRDataAdapter(DbDataAdapter adapter) : base(adapter)
        {
        }
    }
}
