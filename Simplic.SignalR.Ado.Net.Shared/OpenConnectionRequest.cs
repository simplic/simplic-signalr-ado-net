using System;
using System.Collections.Generic;
using System.Text;

namespace Simplic.SignalR.Ado.Net
{
    /// <summary>
    /// Object for requesting a new database connection
    /// </summary>
    public class OpenConnectionRequest
    {
        /// <summary>
        /// Gets or sets the ado.net provider name
        /// </summary>
        public string Provider { get; set; } 

        /// <summary>
        /// Gets or sets the database connection string
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
