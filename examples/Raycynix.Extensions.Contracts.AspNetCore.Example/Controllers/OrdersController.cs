using Microsoft.AspNetCore.Mvc;
using Raycynix.Extensions.Contracts.AspNetCore.Attributes;
using Raycynix.Extensions.Contracts.AspNetCore.Example.Models;
using Raycynix.Extensions.Contracts.AspNetCore.Extensions;

namespace Raycynix.Extensions.Contracts.AspNetCore.Example.Controllers;

[ApiController]
[Route("api/orders")]
[Contract("orders.create", "1.0.0")]
internal sealed class OrdersController : ControllerBase
{
    [HttpPost]
    public IResult CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return this.Contract(
                ModelState.ToErrorContract(traceId: HttpContext.TraceIdentifier),
                StatusCodes.Status400BadRequest);
        }

        return this.Contract(new
        {
            Message = "Order accepted",
            request.OrderId,
            request.Quantity,
            request.Price
        }, StatusCodes.Status202Accepted);
    }
}