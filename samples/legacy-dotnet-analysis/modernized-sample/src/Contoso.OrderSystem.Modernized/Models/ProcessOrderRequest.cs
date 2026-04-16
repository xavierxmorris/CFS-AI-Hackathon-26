namespace Contoso.OrderSystem.Modernized.Models;

public sealed record ProcessOrderRequest(
    int CustomerId,
    string ProductCode,
    int Quantity,
    string PaymentToken);
