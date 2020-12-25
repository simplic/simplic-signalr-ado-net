using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;

namespace Simplic.SignalR.Ado.Net.Client
{
    public class DataReaderSchema
    {
        public string Name { get; set; }
        public Type Type { get; set; }
    }

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

        private IList<DataReaderSchema> schema = new List<DataReaderSchema>();
        private IDictionary<int, JsonElement> row;
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

            var t = Stopwatch.StartNew();
            using (var stream = new MemoryStream(openDataReader.Schema))
            {
                table.ReadXmlSchema(stream);
            }

            using (var stream = new MemoryStream(openDataReader.SchemaData))
            {
                table.ReadXml(stream);
            }
            t.Stop();
            Console.WriteLine(t.ElapsedMilliseconds);
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

        public override string GetDataTypeName(int ordinal)
        {
            foreach (var row in table.Rows.OfType<DataRow>())
            {
                if ((int)row["ColumnOrdinal"] == ordinal)
                {
                    return row["DataType"].ToString();
                }
            }

            throw new ArgumentOutOfRangeException();
        }

        public override Type GetFieldType(int ordinal)
        {
            foreach (var row in table.Rows.OfType<DataRow>())
            {
                if ((int)row["ColumnOrdinal"] == ordinal)
                {
                    var typeName = row["DataType"].ToString();
                    var nullable = (bool)row["AllowDBNull"];

                    var type = Type.GetType(typeName);

                    if (nullable && type.IsValueType)
                    {
                        return Type.GetType($"System.Nullable`1[{typeName}]");
                    }

                    return type;
                }
            }

            throw new ArgumentOutOfRangeException();
        }

        public override string GetName(int ordinal)
        {
            foreach (var row in table.Rows.OfType<DataRow>())
            {
                if ((int)row["ColumnOrdinal"] == ordinal)
                {
                    return row["ColumnName"].ToString();
                }
            }

            throw new ArgumentOutOfRangeException();
        }

        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override int GetOrdinal(string name)
        {
            foreach (var row in table.Rows.OfType<DataRow>())
            {
                if (row["ColumnName"].ToString() == name)
                {
                    return (int)row["ColumnOrdinal"];
                }
            }

            throw new ArgumentOutOfRangeException();
        }

        public override DataTable GetSchemaTable()
        {
            // Maybe request the table here

            return table;
        }


        public override bool Read()
        {
            return true;
        }

        public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            return await base.ReadAsync(cancellationToken);
        }

        public override bool GetBoolean(int ordinal)
        {
            if (row.ContainsKey(ordinal))
            {
                return row[ordinal].GetBoolean();
            }
            throw new IndexOutOfRangeException();
        }

        public override byte GetByte(int ordinal)
        {
            if (row.ContainsKey(ordinal))
            {
                return row[ordinal].GetByte();
            }
            throw new IndexOutOfRangeException();
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            if (row.ContainsKey(ordinal))
            {
                var bytes = new byte[] { };//row[ordinal].GetString();
                for (int i = 0; i < length; i++)
                {
                    // TODO: What if int exceeds?
                    var c = bytes[i + (int)dataOffset];
                    buffer[i + bufferOffset] = c;
                }

                return bytes.Length;
            }
            throw new IndexOutOfRangeException();
        }

        public override char GetChar(int ordinal)
        {
            if (row.ContainsKey(ordinal))
            {
                return row[ordinal].GetString()[0];
            }
            throw new IndexOutOfRangeException();
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            if (row.ContainsKey(ordinal))
            {
                var str = row[ordinal].GetString();
                for (int i = 0; i < length; i++)
                {
                    // TODO: What if int exceeds?
                    var c = str[i + (int)dataOffset];
                    buffer[i + bufferOffset] = c;
                }

                return str.Length;
            }

            throw new IndexOutOfRangeException();
        }

        public override DateTime GetDateTime(int ordinal)
        {
            if (row.ContainsKey(ordinal))
            {
                return row[ordinal].GetDateTime();
            }
            throw new IndexOutOfRangeException();
        }

        public override decimal GetDecimal(int ordinal)
        {
            if (row.ContainsKey(ordinal))
            {
                return row[ordinal].GetDecimal();
            }
            throw new IndexOutOfRangeException();
        }

        public override double GetDouble(int ordinal)
        {
            if (row.ContainsKey(ordinal))
            {
                return row[ordinal].GetDouble();
            }
            throw new IndexOutOfRangeException();
        }

        public override float GetFloat(int ordinal)
        {
            if (row.ContainsKey(ordinal))
            {
                return (float)row[ordinal].GetDouble();
            }
            throw new IndexOutOfRangeException();
        }

        public override Guid GetGuid(int ordinal)
        {
            if (row.ContainsKey(ordinal))
            {
                return row[ordinal].GetGuid();
            }
            throw new IndexOutOfRangeException();
        }

        public override short GetInt16(int ordinal)
        {
            if (row.ContainsKey(ordinal))
            {
                return row[ordinal].GetInt16();
            }
            throw new IndexOutOfRangeException();
        }

        public override int GetInt32(int ordinal)
        {
            if (row.ContainsKey(ordinal))
            {
                return row[ordinal].GetInt32();
            }
            throw new IndexOutOfRangeException();
        }

        public override long GetInt64(int ordinal)
        {
            if (row.ContainsKey(ordinal))
            {
                return row[ordinal].GetInt64();
            }
            throw new IndexOutOfRangeException();
        }

        public override string GetString(int ordinal)
        {
            if (row.ContainsKey(ordinal))
            {
                return row[ordinal].GetString();
            }
            throw new IndexOutOfRangeException();
        }

        public byte[] GetByteArray(int ordinal)
        {
            if (row.ContainsKey(ordinal))
            {
                return row[ordinal].GetBytesFromBase64();
            }
            throw new IndexOutOfRangeException();
        }

        public override object GetValue(int ordinal)
        {
            if (row.ContainsKey(ordinal))
            {
                if (IsDBNull(ordinal))
                    return null;

                switch (GetDataTypeName(ordinal))
                {
                    case "System.String":
                        return GetString(ordinal);

                    case "System.Int32":
                        return GetInt32(ordinal);

                    case "System.Int64":
                        return GetInt32(ordinal);

                    case "System.Guid":
                        return GetGuid(ordinal);

                    case "System.Double":
                        return GetDouble(ordinal);

                    case "System.Byte[]":
                        return GetByteArray(ordinal);
                }

                return row[ordinal];
            }
            throw new IndexOutOfRangeException();
        }

        public override int GetValues(object[] values)
        {
            var min = Math.Min(values.Length, FieldCount);
            for (int i = 0; i < min; i++)
                values[i] = GetValue(i);

            return min;
        }

        public override bool IsDBNull(int ordinal)
        {
            if (row.ContainsKey(ordinal))
            {
                return row[ordinal].ValueKind == JsonValueKind.Null;
            }
            throw new IndexOutOfRangeException();
        }

        public override object this[string name] => null;

        #region Public Member
        public override object this[int ordinal] => GetValue(ordinal);

        public override int Depth => depth;

        public override int FieldCount => fieldCount;

        public override bool HasRows => hasRows;

        public override bool IsClosed => isClosed;

        public override int RecordsAffected => recordsAffected;
        public override int VisibleFieldCount => visibleFieldCount;
        #endregion
    }
}
