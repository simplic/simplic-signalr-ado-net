using System.Text;
using System.Text.RegularExpressions;

namespace Simplic.SignalR.Ado.Net.Client
{
    /// <summary>
    /// Connection string parser utilities
    /// </summary>
    public static class ConnectionStringParser
    {
        /// <summary>
        /// Parse and analyze connection string
        /// </summary>
        /// <param name="connectionString">SignalR and DB-Connection string combined</param>
        /// <param name="parameter">Parameter object</param>
        public static bool ParseConnectionString(string connectionString, out ConnectionStringParameter parameter)
        {
            parameter = new ConnectionStringParameter { };

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                parameter.ErrorMessage = "Connection string must not be null or empty";
                return false;
            }

            connectionString = connectionString.Trim();

            if (!connectionString.StartsWith("{"))
            {
                parameter.ErrorMessage = "Connection string must start with SignalR endpoint url: `{http://your-key@sample.com/}<db-connection-string>`";
                return false;
            }

            var escapeChar = '\\';
            var skipChar = false;
            var signalRConnectionCompleted = false;

            var signalRStringBuilder = new StringBuilder();
            var dbStringBuilder = new StringBuilder();

            for (int i = 1; i < connectionString.Length; i++)
            {
                var currentChar = connectionString[i];

                if (signalRConnectionCompleted)
                {
                    dbStringBuilder.Append(currentChar);
                }
                else
                {
                    if (currentChar == escapeChar && !skipChar)
                    {
                        // Skip next char
                        skipChar = true;
                        continue;
                    }

                    if (currentChar == '}' && !skipChar)
                    {
                        signalRConnectionCompleted = true;
                        continue;
                    }

                    if(currentChar != '}' && skipChar)
                        signalRStringBuilder.Append(escapeChar);

                    signalRStringBuilder.Append(currentChar);

                    skipChar = false;
                }
            }

            var src = signalRStringBuilder.ToString();
            parameter.DbConnectionString = dbStringBuilder.ToString().TrimStart(';');

            string[] keyValuePairs = Regex.Split(src, @"(?<!($|[^\\])(\\\\)*?\\);");

            foreach (var pair in keyValuePairs)
            {
                if (string.IsNullOrWhiteSpace(pair))
                    continue;

                if (pair.Contains("="))
                {
                    // Get key and value, important! do not use string-split here
                    var key = pair.Substring(0, pair.IndexOf('='));
                    var value = pair.Substring(pair.IndexOf('=') + 1, pair.Length - (pair.IndexOf('=') + 1));

                    switch (key.ToLower())
                    {
                        case "url":
                            parameter.Url = value;
                            break;

                        case "driver":
                            parameter.Provider = value;
                            break;
                        default:
                            throw new System.Exception($"Invalid key in connection string: `{key}`. Value: `{value}`");
                    }
                }
                else
                {
                    throw new System.Exception("Connection-string parameter must be in the format `key=value;`");
                }
            }

            // TODO: make this better...
            if (!signalRConnectionCompleted || parameter.Url.Length == 0)
            {
                parameter.ErrorMessage = "Connection string must start with SignalR endpoint url: `{http://your-key@sample.com/}<db-connection-string>`";
                return false;
            }

            if (dbStringBuilder.Length == 0)
            {
                parameter.ErrorMessage = "Connection string must contain database-connection-string: `{http://your-key@sample.com/}<db-connection-string>`";
                return false;
            }

            return true;
        }
    }
}
