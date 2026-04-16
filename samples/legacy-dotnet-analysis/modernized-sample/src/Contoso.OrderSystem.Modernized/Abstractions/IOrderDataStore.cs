using Contoso.OrderSystem.Modernized.Models;

namespace Contoso.OrderSystem.Modernized.Abstractions;

public interface IOrderDataStore
{
    Task<Product?> GetProductByCodeAsync(string productCode, CancellationToken cancellationToken = default);
    Task<int> SaveConfirmedOrderAsync(ConfirmedOrder order, int loyaltyPointsEarned, CancellationToken cancellationToken = default);
}
