using Contoso.OrderSystem.Modernized.Configuration;

namespace Contoso.OrderSystem.Modernized.Tests;

public sealed class PaymentGatewayOptionsValidatorTests
{
    private readonly PaymentGatewayOptionsValidator _validator = new();

    [Fact]
    public void Validate_Fails_WhenSecretIsMissing()
    {
        var result = _validator.Validate(null, new PaymentGatewayOptions
        {
            BaseUrl = "https://api.quickpay-legacy.com/v2/charge",
            MerchantId = "merchant",
            AllowedHosts = ["api.quickpay-legacy.com"]
        });

        Assert.False(result.Succeeded);
        Assert.Contains(result.Failures!, failure => failure.Contains("MerchantSecret", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_Fails_WhenBaseUrlIsNotHttps()
    {
        var result = _validator.Validate(null, new PaymentGatewayOptions
        {
            BaseUrl = "http://api.quickpay-legacy.com/v2/charge",
            MerchantId = "merchant",
            MerchantSecret = "secret",
            AllowedHosts = ["api.quickpay-legacy.com"]
        });

        Assert.False(result.Succeeded);
        Assert.Contains(result.Failures!, failure => failure.Contains("HTTPS", StringComparison.Ordinal));
    }
}
