namespace Simplic.SignalR.Ado.Net.Client
{
    public class ConnectionStringParameter
    {
        public string Url { get; set; }
        public string Provider { get; set; }
        public string DbConnectionString { get; set; }
        public string ErrorMessage { get; set; } = "";
        public bool Success { get; set; }
    }
}
