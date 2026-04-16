using System.ComponentModel.DataAnnotations;

namespace Contoso.OrderSystem.Modernized.Configuration;

public sealed class LoyaltyOptions
{
    public const string SectionName = "Loyalty";

    public bool EnableDiscount { get; init; } = true;

    [Range(0, 1000)]
    public decimal MaxDiscountAmount { get; init; } = 50m;
}
