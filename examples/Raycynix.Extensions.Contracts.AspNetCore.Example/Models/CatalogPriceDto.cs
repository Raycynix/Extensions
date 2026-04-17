using Raycynix.Extensions.Contracts.Attributes;
using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Contracts.AspNetCore.Example.Models;

internal sealed class CatalogPriceDto
{
    [ContractIntroduced("1.0.0")]
    public string ProductId { get; set; } = string.Empty;

    [ContractIntroduced("1.0.0")]
    public Money Price { get; set; } = new();

    [ContractIntroduced("1.2.0")]
    public Money? DiscountPrice { get; set; }
}