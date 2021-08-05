using System.Data;

namespace Simplic.SignalR.Ado.Net
{
    public class CommandParameter
    {
        public DbType DbType { get; set; }
        public ParameterDirection Direction { get; set; } = ParameterDirection.Input;
        public bool IsNullable { get; set; } = false;
        public string ParameterName { get; set; }
        public int Size { get; set; }
        public string SourceColumn { get; set; }
        public bool SourceColumnNullMapping { get; set; } = false;
        public object Value { get; set; }
        public byte Precision { get; set; }
        public byte Scale { get; set; }
        public DataRowVersion SourceVersion { get; set; } = DataRowVersion.Current;
    }
}
