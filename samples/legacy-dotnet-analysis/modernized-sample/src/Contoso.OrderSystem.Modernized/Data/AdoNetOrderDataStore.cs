using System.Data;
using System.Data.Common;
using Contoso.OrderSystem.Modernized.Abstractions;
using Contoso.OrderSystem.Modernized.Models;
using Microsoft.Extensions.Logging;

namespace Contoso.OrderSystem.Modernized.Data;

public sealed class AdoNetOrderDataStore : IOrderDataStore
{
    private readonly Func<DbConnection> _connectionFactory;
    private readonly ILogger<AdoNetOrderDataStore> _logger;

    public AdoNetOrderDataStore(Func<DbConnection> connectionFactory, ILogger<AdoNetOrderDataStore> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Product?> GetProductByCodeAsync(string productCode, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT ProductCode, Price, StockCount
            FROM Products
            WHERE ProductCode = @code
            """;
        AddParameter(command, "@code", DbType.String, productCode);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new Product(
            reader.GetString(0),
            reader.GetDecimal(1),
            reader.GetInt32(2));
    }

    public async Task<int> SaveConfirmedOrderAsync(ConfirmedOrder order, int loyaltyPointsEarned, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var orderId = await InsertOrderAsync(connection, transaction, order, cancellationToken);
            await ReduceStockAsync(connection, transaction, order, cancellationToken);
            await AddLoyaltyPointsAsync(connection, transaction, order.CustomerId, loyaltyPointsEarned, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return orderId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Persisting confirmed order failed for customer {CustomerId} and product {ProductCode}.", order.CustomerId, order.ProductCode);
            throw;
        }
    }

    private static async Task<int> InsertOrderAsync(DbConnection connection, DbTransaction transaction, ConfirmedOrder order, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO Orders (CustomerId, ProductCode, Quantity, Total, Tax, OrderDate, Status)
            VALUES (@customerId, @productCode, @quantity, @total, @tax, @orderDate, 'Confirmed');
            SELECT CAST(SCOPE_IDENTITY() AS int);
            """;
        AddParameter(command, "@customerId", DbType.Int32, order.CustomerId);
        AddParameter(command, "@productCode", DbType.String, order.ProductCode);
        AddParameter(command, "@quantity", DbType.Int32, order.Quantity);
        AddParameter(command, "@total", DbType.Decimal, order.Total);
        AddParameter(command, "@tax", DbType.Decimal, order.Tax);
        AddParameter(command, "@orderDate", DbType.DateTimeOffset, order.OrderedAtUtc);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static async Task ReduceStockAsync(DbConnection connection, DbTransaction transaction, ConfirmedOrder order, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE Products
            SET StockCount = StockCount - @quantity
            WHERE ProductCode = @productCode
              AND StockCount >= @quantity
            """;
        AddParameter(command, "@quantity", DbType.Int32, order.Quantity);
        AddParameter(command, "@productCode", DbType.String, order.ProductCode);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken);
        if (rows != 1)
        {
            throw new InvalidOperationException("Stock update failed due to insufficient inventory or a concurrent change.");
        }
    }

    private static async Task AddLoyaltyPointsAsync(DbConnection connection, DbTransaction transaction, int customerId, int points, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE Customers
            SET LoyaltyPoints = LoyaltyPoints + @points
            WHERE CustomerId = @customerId
            """;
        AddParameter(command, "@points", DbType.Int32, points);
        AddParameter(command, "@customerId", DbType.Int32, customerId);
        await command.ExecuteNonQueryAsync(cancellationToken);
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
