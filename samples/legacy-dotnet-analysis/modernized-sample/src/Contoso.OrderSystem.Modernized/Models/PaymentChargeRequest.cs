namespace Contoso.OrderSystem.Modernized.Models;

public sealed record PaymentChargeRequest(
    string CustomerEmail,
    decimal Amount,
    string PaymentToken);
