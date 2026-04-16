namespace Contoso.OrderSystem.Modernized.Models;

public sealed record OrderConfirmationMessage(
    string RecipientEmail,
    string RecipientName,
    int OrderId,
    decimal Total);
