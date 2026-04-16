using Contoso.OrderSystem.Modernized.Abstractions;

namespace Contoso.OrderSystem.Modernized.Services;

public sealed class TimeProviderClock : IClock
{
    private readonly TimeProvider _timeProvider;

    public TimeProviderClock(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public DateTimeOffset UtcNow => _timeProvider.GetUtcNow();
}
