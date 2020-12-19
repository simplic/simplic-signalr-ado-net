using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Simplic.SignalR.Ado.Net.Client
{
    public class SignalRDbParameter : DbParameter
    {
        private CommandParameter model;

        public override void ResetDbType()
        {
            model = new CommandParameter();
        }

        public override DbType DbType { get => model.DbType; set => model.DbType = value; }
        public override ParameterDirection Direction { get => model.Direction; set => model.Direction = value; }
        public override bool IsNullable { get => model.IsNullable; set => model.IsNullable = value; }
        public override string ParameterName { get => model.ParameterName; set => model.ParameterName = value; }
        public override int Size { get => model.Size; set => model.Size = value; }
        public override string SourceColumn { get => model.SourceColumn; set => model.SourceColumn = value; }
        public override bool SourceColumnNullMapping { get => model.SourceColumnNullMapping; set => model.SourceColumnNullMapping = value; }
        public override object Value { get => model.Value; set => model.Value = value; }
        internal bool IsDirty { get; set; } = true;
        internal CommandParameter Model { get => model; }
    }
}
