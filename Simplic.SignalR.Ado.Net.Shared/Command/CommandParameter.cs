using System.Data;

namespace Simplic.SignalR.Ado.Net
{
    public class CommandParameter
    {
        public DbType DbType { get; set; }
        public ParameterDirection Direction { get; set; }
        public bool IsNullable { get; set; }
        public string ParameterName { get; set; }
        public int Size { get; set; }
        public string SourceColumn { get; set; }
        public bool SourceColumnNullMapping { get; set; }
        public object Value { get; set; }
    }
}
