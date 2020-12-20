using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simplic.SignalR.Ado.Net.Client
{
    public class SignalRDataReader : DbDataReader
    {
        #region Fields
        private SignalRDbCommand command;
        private SignalRDbConnection connection;
        private Guid id;
        private DataTable table;

        private int depth;
        private int fieldCount;
        private bool hasRows;
        private bool isClosed;
        private int recordsAffected;
        private int visibleFieldCount;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialize data reader
        /// </summary>
        /// <param name="openDataReader">Data reader instance</param>
        /// <param name="command">Command instance</param>
        internal SignalRDataReader(OpenDataReaderResponse openDataReader, SignalRDbCommand command)
        {
            this.command = command;
            this.connection = command.SignalRDbConnection;

            depth = openDataReader.Depth;
            fieldCount = openDataReader.FieldCount;
            isClosed = openDataReader.IsClosed;
            recordsAffected = openDataReader.RecordsAffected;
            hasRows = openDataReader.HasRows;
            visibleFieldCount = openDataReader.VisibleFieldCount;

            id = openDataReader.Id;

            table = new DataTable();

            using (var stream = new MemoryStream(openDataReader.Schema))
            {
                table.ReadXmlSchema(stream);
            }

            using (var stream = new MemoryStream(openDataReader.SchemaData))
            {
                table.ReadXml(stream);
            }
        }
        #endregion

        public override bool NextResult()
        {
            var response = connection.HubConnectionBuilder.InvokeAsync<ResponseObject<NextResultResponse>>("NextResultAsync", id).Result;

            // AssertResponse(response);

            depth = response.Object.Depth;
            fieldCount = response.Object.FieldCount;
            isClosed = response.Object.IsClosed;
            recordsAffected = response.Object.RecordsAffected;
            hasRows = response.Object.HasRows;
            visibleFieldCount = response.Object.VisibleFieldCount;

            return response.Object.Result;
        }

        public override async Task<bool> NextResultAsync(CancellationToken cancellationToken)
        {
            var response = await connection.HubConnectionBuilder.InvokeAsync<ResponseObject<NextResultResponse>>("NextResultAsync", id);

            // AssertResponse(response);

            depth = response.Object.Depth;
            fieldCount = response.Object.FieldCount;
            isClosed = response.Object.IsClosed;
            recordsAffected = response.Object.RecordsAffected;
            hasRows = response.Object.HasRows;
            visibleFieldCount = response.Object.VisibleFieldCount;

            return response.Object.Result;
        }

        public override bool Read()
        {
            throw new NotImplementedException();
        }

        public override bool GetBoolean(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override byte GetByte(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override char GetChar(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override string GetDataTypeName(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetDateTime(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override decimal GetDecimal(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override double GetDouble(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override Type GetFieldType(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override float GetFloat(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override Guid GetGuid(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override short GetInt16(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int GetInt32(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetInt64(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override string GetName(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public override string GetString(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public override bool IsDBNull(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override DataTable GetSchemaTable()
        {
            return table;
        }

        public override object this[int ordinal] => throw new NotImplementedException();

        public override object this[string name] => throw new NotImplementedException();

        public override int Depth => depth;

        public override int FieldCount => fieldCount;

        public override bool HasRows => hasRows;

        public override bool IsClosed => isClosed;

        public override int RecordsAffected => recordsAffected;
        public override int VisibleFieldCount => visibleFieldCount;
    }
}
