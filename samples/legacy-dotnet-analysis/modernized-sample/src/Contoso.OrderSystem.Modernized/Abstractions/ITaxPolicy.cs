namespace Contoso.OrderSystem.Modernized.Abstractions;

public interface ITaxPolicy
{
    decimal GetTaxRate(string state);
}
