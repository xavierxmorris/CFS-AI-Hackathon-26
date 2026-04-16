namespace Contoso.OrderSystem.Modernized.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
