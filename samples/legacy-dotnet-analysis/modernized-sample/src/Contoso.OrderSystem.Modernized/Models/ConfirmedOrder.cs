namespace Contoso.OrderSystem.Modernized.Models;

public sealed record ConfirmedOrder(
    int CustomerId,
    string ProductCode,
    int Quantity,
    decimal Total,
    decimal Tax,
    DateTimeOffset OrderedAtUtc);
