using System.Data;
using System.Data.Common;
using Contoso.OrderSystem.Modernized.Abstractions;
using Contoso.OrderSystem.Modernized.Models;
using Microsoft.Extensions.Logging;

namespace Contoso.OrderSystem.Modernized.Data;

public sealed class AdoNetCustomerRepository : ICustomerRepository
{
    private readonly Func<DbConnection> _connectionFactory;
    private readonly ILogger<AdoNetCustomerRepository> _logger;

    public AdoNetCustomerRepository(Func<DbConnection> connectionFactory, ILogger<AdoNetCustomerRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Customer?> GetCustomerByIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT CustomerId, Name, Email, State, Status, LoyaltyPoints
            FROM Customers
            WHERE CustomerId = @id
            """;
        AddParameter(command, "@id", DbType.Int32, customerId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await ReadSingleCustomerAsync(reader, cancellationToken);
    }

    public async Task<Customer?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT CustomerId, Name, Email, State, Status, LoyaltyPoints
            FROM Customers
            WHERE Email = @email
            """;
        AddParameter(command, "@email", DbType.String, email);

        _logger.LogInformation("Looking up customer by email address using a parameterized query.");

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await ReadSingleCustomerAsync(reader, cancellationToken);
    }

    public async Task AddLoyaltyPointsAsync(int customerId, int points, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Customers
            SET LoyaltyPoints = LoyaltyPoints + @points
            WHERE CustomerId = @id
            """;
        AddParameter(command, "@points", DbType.Int32, points);
        AddParameter(command, "@id", DbType.Int32, customerId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<Customer?> ReadSingleCustomerAsync(DbDataReader reader, CancellationToken cancellationToken)
    {
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new Customer(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetInt32(5));
    }

    private static void AddParameter(DbCommand command, string name, DbType dbType, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.DbType = dbType;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
