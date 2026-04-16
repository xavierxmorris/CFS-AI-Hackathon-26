using Contoso.OrderSystem.Modernized.Abstractions;
using Contoso.OrderSystem.Modernized.Configuration;
using Contoso.OrderSystem.Modernized.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Contoso.OrderSystem.Modernized.Services;

public sealed class OrderProcessor : IOrderProcessor
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderDataStore _orderDataStore;
    private readonly IPaymentGatewayClient _paymentGatewayClient;
    private readonly IEmailSender _emailSender;
    private readonly ITaxPolicy _taxPolicy;
    private readonly IClock _clock;
    private readonly LoyaltyOptions _loyaltyOptions;
    private readonly ILogger<OrderProcessor> _logger;

    public OrderProcessor(
        ICustomerRepository customerRepository,
        IOrderDataStore orderDataStore,
        IPaymentGatewayClient paymentGatewayClient,
        IEmailSender emailSender,
        ITaxPolicy taxPolicy,
        IClock clock,
        IOptions<LoyaltyOptions> loyaltyOptions,
        ILogger<OrderProcessor> logger)
    {
        _customerRepository = customerRepository;
        _orderDataStore = orderDataStore;
        _paymentGatewayClient = paymentGatewayClient;
        _emailSender = emailSender;
        _taxPolicy = taxPolicy;
        _clock = clock;
        _loyaltyOptions = loyaltyOptions.Value;
        _logger = logger;
    }

    public async Task<int> ProcessOrderAsync(ProcessOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Quantity <= 0)
        {
            _logger.LogWarning("Rejected order for customer {CustomerId} because quantity {Quantity} is invalid.", request.CustomerId, request.Quantity);
            return -1;
        }

        var customer = await _customerRepository.GetCustomerByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
        {
            _logger.LogWarning("Rejected order because customer {CustomerId} was not found.", request.CustomerId);
            return -1;
        }

        if (!string.Equals(customer.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Rejected order because customer {CustomerId} is in status {CustomerStatus}.", request.CustomerId, customer.Status);
            return -1;
        }

        var product = await _orderDataStore.GetProductByCodeAsync(request.ProductCode, cancellationToken);
        if (product is null)
        {
            _logger.LogWarning("Rejected order because product {ProductCode} was not found.", request.ProductCode);
            return -1;
        }

        if (request.Quantity > product.StockCount)
        {
            _logger.LogWarning(
                "Rejected order because product {ProductCode} has insufficient stock. Requested {RequestedQuantity}, available {AvailableQuantity}.",
                request.ProductCode,
                request.Quantity,
                product.StockCount);
            return -1;
        }

        var subtotal = product.Price * request.Quantity;
        var tax = subtotal * _taxPolicy.GetTaxRate(customer.State);
        var total = subtotal + tax;

        if (_loyaltyOptions.EnableDiscount && customer.LoyaltyPoints > 500)
        {
            var discount = Math.Min(total * 0.10m, _loyaltyOptions.MaxDiscountAmount);
            total -= discount;
        }

        var paymentResult = await _paymentGatewayClient.ChargeCustomerAsync(
            new PaymentChargeRequest(customer.Email, total, request.PaymentToken),
            cancellationToken);

        if (!paymentResult.Success)
        {
            _logger.LogWarning(
                "Rejected order because payment failed for customer {CustomerId}. Reason: {FailureReason}",
                request.CustomerId,
                paymentResult.FailureReason ?? "unknown");
            return -1;
        }

        var pointsEarned = (int)(total / 10m);
        var orderId = await _orderDataStore.SaveConfirmedOrderAsync(
            new ConfirmedOrder(request.CustomerId, request.ProductCode, request.Quantity, total, tax, _clock.UtcNow),
            pointsEarned,
            cancellationToken);

        try
        {
            await _emailSender.SendOrderConfirmationAsync(
                new OrderConfirmationMessage(customer.Email, customer.Name, orderId, total),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Order {OrderId} was created but confirmation email delivery failed.", orderId);
        }

        return orderId;
    }
}
