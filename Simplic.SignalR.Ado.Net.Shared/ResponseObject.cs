using System;
using System.Collections.Generic;
using System.Text;

namespace Simplic.SignalR.Ado.Net
{
    public class ResponseObject<T> : Response
    {
        public T Object { get; set; }
    }
}
