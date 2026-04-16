using Contoso.OrderSystem.Modernized.Abstractions;

namespace Contoso.OrderSystem.Modernized.Services;

public sealed class LegacyTaxPolicy : ITaxPolicy
{
    public decimal GetTaxRate(string state) => state switch
    {
        "CA" => 0.0725m,
        "NY" => 0.08m,
        "TX" => 0.0625m,
        _ => 0.05m
    };
}
