namespace Contoso.OrderSystem.Modernized.Models;

public sealed record PaymentOperationResult(bool Success, string? FailureReason = null)
{
    public static PaymentOperationResult Approved() => new(true);
    public static PaymentOperationResult Failed(string? failureReason = null) => new(false, failureReason);
}
