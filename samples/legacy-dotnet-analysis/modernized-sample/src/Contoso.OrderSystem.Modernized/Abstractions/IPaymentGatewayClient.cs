using Contoso.OrderSystem.Modernized.Models;

namespace Contoso.OrderSystem.Modernized.Abstractions;

public interface IPaymentGatewayClient
{
    Task<PaymentOperationResult> ChargeCustomerAsync(PaymentChargeRequest request, CancellationToken cancellationToken = default);
    Task<PaymentOperationResult> RefundChargeAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default);
}
