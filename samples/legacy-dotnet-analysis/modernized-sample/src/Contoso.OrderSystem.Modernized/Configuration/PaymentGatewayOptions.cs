using System.ComponentModel.DataAnnotations;

namespace Contoso.OrderSystem.Modernized.Configuration;

public sealed class PaymentGatewayOptions
{
    public const string SectionName = "PaymentGateway";

    [Required]
    public string BaseUrl { get; init; } = string.Empty;

    [Required]
    public string MerchantId { get; init; } = string.Empty;

    [Required]
    public string MerchantSecret { get; init; } = string.Empty;

    public string[] AllowedHosts { get; init; } = Array.Empty<string>();
}
