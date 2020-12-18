using System;
using System.Collections.Generic;
using System.Text;

namespace Simplic.SignalR.Ado.Net
{
    /// <summary>
    /// Represents the open connection response
    /// </summary>
    public class OpenConnectionResponse
    {
        /// <summary>
        /// Gets or sets the database name
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Gets or sets the connection timeout
        /// </summary>
        public int ConnectionTimeout { get; set; }

        /// <summary>
        /// Gets or sets whether establishing the connection was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets an occured exception
        /// </summary>
        public string Exception { get; set; }

        /// <summary>
        /// Gets or sets the server-version
        /// </summary>
        public string ServerVersion { get; set; }

        /// <summary>
        /// Gets or sets the data source name
        /// </summary>
        public string DataSource { get; set; }
    }
}
