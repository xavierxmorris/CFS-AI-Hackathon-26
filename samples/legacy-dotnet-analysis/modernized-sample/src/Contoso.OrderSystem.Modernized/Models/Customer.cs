namespace Contoso.OrderSystem.Modernized.Models;

public sealed record Customer(
    int CustomerId,
    string Name,
    string Email,
    string State,
    string Status,
    int LoyaltyPoints);
