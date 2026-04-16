using Microsoft.Extensions.Options;

namespace Contoso.OrderSystem.Modernized.Configuration;

public sealed class PaymentGatewayOptionsValidator : IValidateOptions<PaymentGatewayOptions>
{
    public ValidateOptionsResult Validate(string? name, PaymentGatewayOptions options)
    {
        var failures = new List<string>();

        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var gatewayUri))
        {
            failures.Add("PaymentGateway:BaseUrl must be a valid absolute URI.");
        }
        else
        {
            if (!string.Equals(gatewayUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                failures.Add("PaymentGateway:BaseUrl must use HTTPS.");
            }

            if (options.AllowedHosts.Length == 0)
            {
                failures.Add("PaymentGateway:AllowedHosts must contain at least one host.");
            }
            else if (!options.AllowedHosts.Contains(gatewayUri.Host, StringComparer.OrdinalIgnoreCase))
            {
                failures.Add("Payment gateway host must appear in PaymentGateway:AllowedHosts.");
            }
        }

        if (string.IsNullOrWhiteSpace(options.MerchantId))
        {
            failures.Add("PaymentGateway:MerchantId is required.");
        }

        if (string.IsNullOrWhiteSpace(options.MerchantSecret))
        {
            failures.Add("PaymentGateway:MerchantSecret must come from secure configuration.");
        }

        return failures.Count > 0 ? ValidateOptionsResult.Fail(failures) : ValidateOptionsResult.Success;
    }
}
