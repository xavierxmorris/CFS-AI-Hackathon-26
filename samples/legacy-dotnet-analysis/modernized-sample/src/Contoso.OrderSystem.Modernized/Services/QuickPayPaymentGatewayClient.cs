using System.Net.Http.Headers;
using Contoso.OrderSystem.Modernized.Abstractions;
using Contoso.OrderSystem.Modernized.Configuration;
using Contoso.OrderSystem.Modernized.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Contoso.OrderSystem.Modernized.Services;

public sealed class QuickPayPaymentGatewayClient : IPaymentGatewayClient
{
    private readonly HttpClient _httpClient;
    private readonly PaymentGatewayOptions _options;
    private readonly ILogger<QuickPayPaymentGatewayClient> _logger;

    public QuickPayPaymentGatewayClient(
        HttpClient httpClient,
        IOptions<PaymentGatewayOptions> options,
        ILogger<QuickPayPaymentGatewayClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public Task<PaymentOperationResult> ChargeCustomerAsync(PaymentChargeRequest request, CancellationToken cancellationToken = default)
    {
        var values = new Dictionary<string, string>
        {
            ["merchant_id"] = _options.MerchantId,
            ["secret"] = _options.MerchantSecret,
            ["email"] = request.CustomerEmail,
            ["amount"] = request.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
            ["token"] = request.PaymentToken,
            ["currency"] = "USD"
        };

        return SendAsync(new Uri(_options.BaseUrl, UriKind.Absolute), values, "APPROVED", cancellationToken);
    }

    public Task<PaymentOperationResult> RefundChargeAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default)
    {
        var refundUri = new Uri(new Uri(_options.BaseUrl, UriKind.Absolute), "refund");
        var values = new Dictionary<string, string>
        {
            ["merchant_id"] = _options.MerchantId,
            ["secret"] = _options.MerchantSecret,
            ["transaction_id"] = transactionId,
            ["amount"] = amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
        };

        return SendAsync(refundUri, values, "REFUNDED", cancellationToken);
    }

    private async Task<PaymentOperationResult> SendAsync(Uri endpoint, IReadOnlyDictionary<string, string> body, string successPrefix, CancellationToken cancellationToken)
    {
        ValidateEndpoint(endpoint);

        using var message = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new FormUrlEncodedContent(body)
        };
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

        try
        {
            using var response = await _httpClient.SendAsync(message, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Payment gateway returned non-success status code {StatusCode} from host {GatewayHost}.",
                    (int)response.StatusCode,
                    endpoint.Host);
                return PaymentOperationResult.Failed($"gateway-status:{(int)response.StatusCode}");
            }

            return responseBody.StartsWith(successPrefix, StringComparison.OrdinalIgnoreCase)
                ? PaymentOperationResult.Approved()
                : PaymentOperationResult.Failed("gateway-declined");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Payment gateway request failed for host {GatewayHost}.", endpoint.Host);
            return PaymentOperationResult.Failed("gateway-error");
        }
    }

    private void ValidateEndpoint(Uri endpoint)
    {
        if (!string.Equals(endpoint.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Payment endpoints must use HTTPS.");
        }

        if (!_options.AllowedHosts.Contains(endpoint.Host, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Payment endpoint host is not allowlisted.");
        }
    }
}
