using Contoso.OrderSystem.Modernized.Models;

namespace Contoso.OrderSystem.Modernized.Abstractions;

public interface IEmailSender
{
    Task SendOrderConfirmationAsync(OrderConfirmationMessage message, CancellationToken cancellationToken = default);
}
