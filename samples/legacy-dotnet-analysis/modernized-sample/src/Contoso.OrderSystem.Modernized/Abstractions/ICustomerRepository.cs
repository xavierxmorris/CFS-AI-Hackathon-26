using Contoso.OrderSystem.Modernized.Models;

namespace Contoso.OrderSystem.Modernized.Abstractions;

public interface ICustomerRepository
{
    Task<Customer?> GetCustomerByIdAsync(int customerId, CancellationToken cancellationToken = default);
    Task<Customer?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddLoyaltyPointsAsync(int customerId, int points, CancellationToken cancellationToken = default);
}
