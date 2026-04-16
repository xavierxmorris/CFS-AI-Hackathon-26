using System.ComponentModel.DataAnnotations;

namespace Contoso.OrderSystem.Modernized.Configuration;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    [Required]
    [EmailAddress]
    public string FromAddress { get; init; } = string.Empty;
}
