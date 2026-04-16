using Contoso.OrderSystem.Modernized.Abstractions;
using Contoso.OrderSystem.Modernized.Models;
using Microsoft.Extensions.Logging;

namespace Contoso.OrderSystem.Modernized.Services;

public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendOrderConfirmationAsync(OrderConfirmationMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Queued order confirmation email for order {OrderId} to recipient {RecipientEmail}.",
            message.OrderId,
            message.RecipientEmail);

        return Task.CompletedTask;
    }
}
