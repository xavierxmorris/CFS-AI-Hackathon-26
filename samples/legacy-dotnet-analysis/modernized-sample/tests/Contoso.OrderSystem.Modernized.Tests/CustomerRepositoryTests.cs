using System.Collections;
using System.Data;
using System.Data.Common;
using Contoso.OrderSystem.Modernized.Data;
using Microsoft.Extensions.Logging.Abstractions;

#pragma warning disable CS8764, CS8765

namespace Contoso.OrderSystem.Modernized.Tests;

public sealed class CustomerRepositoryTests
{
    [Fact]
    public async Task FindByEmailAsync_UsesParameterizedQuery()
    {
        var command = new RecordingDbCommand();
        command.Results.Add(new object[] { 42, "Ada", "ada@example.com", "CA", "Active", 900 });
        var connection = new RecordingDbConnection(command);
        var repository = new AdoNetCustomerRepository(() => connection, NullLogger<AdoNetCustomerRepository>.Instance);

        var customer = await repository.FindByEmailAsync("ada@example.com' OR 1=1 --");

        Assert.NotNull(customer);
        Assert.Contains("WHERE Email = @email", command.CommandText, StringComparison.Ordinal);
        var parameter = Assert.Single(command.RecordedParameters);
        Assert.Equal("@email", parameter.ParameterName);
        Assert.Equal("ada@example.com' OR 1=1 --", parameter.Value);
    }

    private sealed class RecordingDbConnection : DbConnection
    {
        private readonly RecordingDbCommand _command;
        private ConnectionState _state = ConnectionState.Closed;

        public RecordingDbConnection(RecordingDbCommand command)
        {
            _command = command;
        }

        public override string ConnectionString { get; set; } = string.Empty;
        public override string Database => "ContosoOrders";
        public override string DataSource => "Fake";
        public override string ServerVersion => "1.0";
        public override ConnectionState State => _state;

        public override void ChangeDatabase(string databaseName) { }
        public override void Close() => _state = ConnectionState.Closed;
        public override void Open() => _state = ConnectionState.Open;
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => throw new NotSupportedException();
        protected override DbCommand CreateDbCommand() => _command;
        public override Task OpenAsync(CancellationToken cancellationToken) { _state = ConnectionState.Open; return Task.CompletedTask; }
    }

    private sealed class RecordingDbCommand : DbCommand
    {
        private readonly RecordingParameterCollection _parameters = new();

        public List<object[]> Results { get; } = [];
        public IReadOnlyList<DbParameter> RecordedParameters => _parameters.Items;

        public override string CommandText { get; set; } = string.Empty;
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; } = CommandType.Text;
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }
        protected override DbConnection? DbConnection { get; set; }
        protected override DbParameterCollection DbParameterCollection => _parameters;
        protected override DbTransaction? DbTransaction { get; set; }

        public override void Cancel() { }
        public override int ExecuteNonQuery() => 1;
        public override object? ExecuteScalar() => 1;
        public override void Prepare() { }
        protected override DbParameter CreateDbParameter() => new RecordingDbParameter();
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => new RecordingDbDataReader(Results);
    }

    private sealed class RecordingDbParameter : DbParameter
    {
        public override DbType DbType { get; set; }
        public override ParameterDirection Direction { get; set; } = ParameterDirection.Input;
        public override bool IsNullable { get; set; }
        public override string ParameterName { get; set; } = string.Empty;
        public override string? SourceColumn { get; set; }
        public override object? Value { get; set; }
        public override bool SourceColumnNullMapping { get; set; }
        public override int Size { get; set; }
        public override void ResetDbType() { }
    }

    private sealed class RecordingParameterCollection : DbParameterCollection
    {
        public List<DbParameter> Items { get; } = [];

        public override int Count => Items.Count;
        public override object SyncRoot { get; } = new();
        public override int Add(object value) { Items.Add((DbParameter)value); return Items.Count - 1; }
        public override void AddRange(Array values) { foreach (var value in values) { Add(value!); } }
        public override void Clear() => Items.Clear();
        public override bool Contains(string value) => Items.Any(parameter => parameter.ParameterName == value);
        public override bool Contains(object value) => Items.Contains((DbParameter)value);
        public override void CopyTo(Array array, int index) => Items.ToArray().CopyTo(array, index);
        public override IEnumerator GetEnumerator() => Items.GetEnumerator();
        protected override DbParameter GetParameter(string parameterName) => Items.Single(parameter => parameter.ParameterName == parameterName);
        protected override DbParameter GetParameter(int index) => Items[index];
        public override int IndexOf(string parameterName) => Items.FindIndex(parameter => parameter.ParameterName == parameterName);
        public override int IndexOf(object value) => Items.IndexOf((DbParameter)value);
        public override void Insert(int index, object value) => Items.Insert(index, (DbParameter)value);
        public override void Remove(object value) => Items.Remove((DbParameter)value);
        public override void RemoveAt(string parameterName) => Items.RemoveAll(parameter => parameter.ParameterName == parameterName);
        public override void RemoveAt(int index) => Items.RemoveAt(index);
        protected override void SetParameter(string parameterName, DbParameter value)
        {
            var index = IndexOf(parameterName);
            if (index >= 0) { Items[index] = value; }
            else { Items.Add(value); }
        }
        protected override void SetParameter(int index, DbParameter value) => Items[index] = value;
    }

    private sealed class RecordingDbDataReader : DbDataReader
    {
        private readonly IReadOnlyList<object[]> _rows;
        private int _index = -1;

        public RecordingDbDataReader(IReadOnlyList<object[]> rows)
        {
            _rows = rows;
        }

        public override int FieldCount => _rows.Count == 0 ? 0 : _rows[0].Length;
        public override bool HasRows => _rows.Count > 0;
        public override object this[int ordinal] => _rows[_index][ordinal];
        public override object this[string name] => throw new NotSupportedException();
        public override int Depth => 0;
        public override bool IsClosed => false;
        public override int RecordsAffected => 0;

        public override bool Read()
        {
            if (_index + 1 >= _rows.Count)
            {
                return false;
            }

            _index++;
            return true;
        }

        public override Task<bool> ReadAsync(CancellationToken cancellationToken) => Task.FromResult(Read());
        public override bool NextResult() => false;
        public override Task<bool> NextResultAsync(CancellationToken cancellationToken) => Task.FromResult(false);
        public override string GetName(int ordinal) => $"Column{ordinal}";
        public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;
        public override Type GetFieldType(int ordinal) => _rows[_index][ordinal].GetType();
        public override object GetValue(int ordinal) => _rows[_index][ordinal];
        public override int GetValues(object[] values) { _rows[_index].CopyTo(values, 0); return _rows[_index].Length; }
        public override int GetOrdinal(string name) => throw new NotSupportedException();
        public override bool GetBoolean(int ordinal) => (bool)GetValue(ordinal);
        public override byte GetByte(int ordinal) => (byte)GetValue(ordinal);
        public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) => throw new NotSupportedException();
        public override char GetChar(int ordinal) => (char)GetValue(ordinal);
        public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) => throw new NotSupportedException();
        public override Guid GetGuid(int ordinal) => (Guid)GetValue(ordinal);
        public override short GetInt16(int ordinal) => (short)GetValue(ordinal);
        public override int GetInt32(int ordinal) => (int)GetValue(ordinal);
        public override long GetInt64(int ordinal) => (long)GetValue(ordinal);
        public override float GetFloat(int ordinal) => (float)GetValue(ordinal);
        public override double GetDouble(int ordinal) => (double)GetValue(ordinal);
        public override string GetString(int ordinal) => (string)GetValue(ordinal);
        public override decimal GetDecimal(int ordinal) => (decimal)GetValue(ordinal);
        public override DateTime GetDateTime(int ordinal) => (DateTime)GetValue(ordinal);
        public override bool IsDBNull(int ordinal) => GetValue(ordinal) is DBNull;
        public override IEnumerator GetEnumerator() => _rows.GetEnumerator();
    }
}

#pragma warning restore CS8764, CS8765
