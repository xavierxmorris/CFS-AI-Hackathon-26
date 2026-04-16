using Contoso.OrderSystem.Modernized.Abstractions;
using Contoso.OrderSystem.Modernized.Configuration;
using Contoso.OrderSystem.Modernized.Models;
using Contoso.OrderSystem.Modernized.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Contoso.OrderSystem.Modernized.Tests;

public sealed class OrderProcessorTests
{
    [Fact]
    public async Task ProcessOrderAsync_ReturnsNegativeOne_WhenCustomerIsMissing()
    {
        var sut = CreateSubject(hasCustomer: false);

        var result = await sut.ProcessOrderAsync(new ProcessOrderRequest(42, "WIDGET", 2, "token"));

        Assert.Equal(-1, result);
    }

    [Fact]
    public async Task ProcessOrderAsync_ReturnsNegativeOne_WhenPaymentFails()
    {
        var paymentGateway = new FakePaymentGatewayClient(PaymentOperationResult.Failed("declined"));
        var sut = CreateSubject(paymentGateway: paymentGateway);

        var result = await sut.ProcessOrderAsync(new ProcessOrderRequest(42, "WIDGET", 2, "token"));

        Assert.Equal(-1, result);
        Assert.Single(paymentGateway.ChargeRequests);
    }

    [Fact]
    public async Task ProcessOrderAsync_AppliesLegacyDiscountAndPersistsOrder()
    {
        var orderStore = new FakeOrderDataStore();
        var emailSender = new FakeEmailSender();
        var sut = CreateSubject(
            customer: new Customer(42, "Ada", "ada@example.com", "CA", "Active", 900),
            product: new Product("WIDGET", 100m, 5),
            orderStore: orderStore,
            emailSender: emailSender,
            clock: new FixedClock(new DateTimeOffset(2026, 04, 16, 1, 0, 0, TimeSpan.Zero)));

        var result = await sut.ProcessOrderAsync(new ProcessOrderRequest(42, "WIDGET", 2, "token-123"));

        Assert.Equal(7001, result);
        var savedOrder = Assert.Single(orderStore.SavedOrders);
        Assert.Equal(193.05m, savedOrder.Order.Total);
        Assert.Equal(14.50m, savedOrder.Order.Tax);
        Assert.Equal(19, savedOrder.PointsEarned);
        var confirmation = Assert.Single(emailSender.Messages);
        Assert.Equal("ada@example.com", confirmation.RecipientEmail);
    }

    [Fact]
    public async Task ProcessOrderAsync_DoesNotFailOrder_WhenEmailDeliveryThrows()
    {
        var orderStore = new FakeOrderDataStore();
        var sut = CreateSubject(orderStore: orderStore, emailSender: new ThrowingEmailSender());

        var result = await sut.ProcessOrderAsync(new ProcessOrderRequest(42, "WIDGET", 1, "token"));

        Assert.Equal(7001, result);
        Assert.Single(orderStore.SavedOrders);
    }

    private static OrderProcessor CreateSubject(
        bool hasCustomer = true,
        Customer? customer = null,
        Product? product = null,
        FakeOrderDataStore? orderStore = null,
        IEmailSender? emailSender = null,
        IPaymentGatewayClient? paymentGateway = null,
        IClock? clock = null)
    {
        var resolvedCustomer = hasCustomer ? customer ?? new Customer(42, "Ada", "ada@example.com", "CA", "Active", 100) : null;
        var customerRepository = new FakeCustomerRepository(resolvedCustomer);
        var dataStore = orderStore ?? new FakeOrderDataStore(product ?? new Product("WIDGET", 100m, 10));

        return new OrderProcessor(
            customerRepository,
            dataStore,
            paymentGateway ?? new FakePaymentGatewayClient(PaymentOperationResult.Approved()),
            emailSender ?? new FakeEmailSender(),
            new LegacyTaxPolicy(),
            clock ?? new FixedClock(new DateTimeOffset(2026, 04, 16, 1, 0, 0, TimeSpan.Zero)),
            Options.Create(new LoyaltyOptions { EnableDiscount = true, MaxDiscountAmount = 50m }),
            NullLogger<OrderProcessor>.Instance);
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        private readonly Customer? _customer;

        public FakeCustomerRepository(Customer? customer)
        {
            _customer = customer;
        }

        public Task<Customer?> GetCustomerByIdAsync(int customerId, CancellationToken cancellationToken = default)
            => Task.FromResult(_customer);

        public Task<Customer?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(_customer);

        public Task AddLoyaltyPointsAsync(int customerId, int points, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeOrderDataStore : IOrderDataStore
    {
        private readonly Product _product;

        public FakeOrderDataStore(Product? product = null)
        {
            _product = product ?? new Product("WIDGET", 100m, 10);
        }

        public List<(ConfirmedOrder Order, int PointsEarned)> SavedOrders { get; } = [];

        public Task<Product?> GetProductByCodeAsync(string productCode, CancellationToken cancellationToken = default)
            => Task.FromResult<Product?>(_product with { ProductCode = productCode });

        public Task<int> SaveConfirmedOrderAsync(ConfirmedOrder order, int loyaltyPointsEarned, CancellationToken cancellationToken = default)
        {
            SavedOrders.Add((order, loyaltyPointsEarned));
            return Task.FromResult(7001);
        }
    }

    private sealed class FakePaymentGatewayClient : IPaymentGatewayClient
    {
        private readonly PaymentOperationResult _result;

        public FakePaymentGatewayClient(PaymentOperationResult result)
        {
            _result = result;
        }

        public List<PaymentChargeRequest> ChargeRequests { get; } = [];

        public Task<PaymentOperationResult> ChargeCustomerAsync(PaymentChargeRequest request, CancellationToken cancellationToken = default)
        {
            ChargeRequests.Add(request);
            return Task.FromResult(_result);
        }

        public Task<PaymentOperationResult> RefundChargeAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default)
            => Task.FromResult(_result);
    }

    private sealed class FakeEmailSender : IEmailSender
    {
        public List<OrderConfirmationMessage> Messages { get; } = [];

        public Task SendOrderConfirmationAsync(OrderConfirmationMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingEmailSender : IEmailSender
    {
        public Task SendOrderConfirmationAsync(OrderConfirmationMessage message, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("smtp unavailable");
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTimeOffset utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTimeOffset UtcNow { get; }
    }
}
