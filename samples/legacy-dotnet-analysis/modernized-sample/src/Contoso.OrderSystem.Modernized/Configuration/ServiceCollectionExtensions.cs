using Contoso.OrderSystem.Modernized.Abstractions;
using Contoso.OrderSystem.Modernized.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Contoso.OrderSystem.Modernized.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddModernizedOrderSystem(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<Microsoft.Extensions.Options.IValidateOptions<PaymentGatewayOptions>, PaymentGatewayOptionsValidator>();

        services
            .AddOptions<PaymentGatewayOptions>()
            .Bind(configuration.GetSection(PaymentGatewayOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<LoyaltyOptions>()
            .Bind(configuration.GetSection(LoyaltyOptions.SectionName))
            .ValidateDataAnnotations();

        services
            .AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddHttpClient<IPaymentGatewayClient, QuickPayPaymentGatewayClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<PaymentGatewayOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddSingleton<ITaxPolicy, LegacyTaxPolicy>();
        services.AddSingleton<IClock>(_ => new TimeProviderClock(TimeProvider.System));
        services.TryAddSingleton<IEmailSender, LoggingEmailSender>();
        services.AddTransient<IOrderProcessor, OrderProcessor>();

        return services;
    }
}
