namespace Contoso.OrderSystem.Modernized.Models;

public sealed record Product(
    string ProductCode,
    decimal Price,
    int StockCount);
