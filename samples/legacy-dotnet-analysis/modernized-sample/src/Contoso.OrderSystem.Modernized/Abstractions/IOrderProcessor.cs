using Contoso.OrderSystem.Modernized.Models;

namespace Contoso.OrderSystem.Modernized.Abstractions;

public interface IOrderProcessor
{
    Task<int> ProcessOrderAsync(ProcessOrderRequest request, CancellationToken cancellationToken = default);
}
