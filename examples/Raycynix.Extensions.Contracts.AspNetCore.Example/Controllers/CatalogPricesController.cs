using Microsoft.AspNetCore.Mvc;
using Raycynix.Extensions.Contracts.AspNetCore.Attributes;
using Raycynix.Extensions.Contracts.AspNetCore.Example.Models;
using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Contracts.AspNetCore.Example.Controllers;

[ApiController]
[Route("api/catalog/prices")]
[Contract("catalog.price", "1.2.0")]
internal sealed class CatalogPricesController : ControllerBase
{
    [HttpGet("{id}")]
    public IResult GetPrice(string id)
    {
        return this.VersionedContract(new CatalogPriceDto
        {
            ProductId = id,
            Price = new Money
            {
                Amount = 149.99m,
                Currency = "USD"
            },
            DiscountPrice = new Money
            {
                Amount = 129.99m,
                Currency = "USD"
            }
        });
    }
}