using Raycynix.Extensions.Contracts.AspNetCore;
using Raycynix.Extensions.Contracts.AspNetCore.Extensions;
using Raycynix.Extensions.Contracts.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixContractsAspNetCore();
builder.Services.AddControllers();

var app = builder.Build();

app.UseRaycynixContracts();

app.MapGet("/", () => Results.Ok(new
{
    Service = "Raycynix.Extensions.Contracts.AspNetCore.Example",
    Endpoints = new[]
    {
        "GET /contracts/plain",
        "GET /contracts/versioned",
        "GET /contracts/request-metadata",
        "GET /api/catalog/prices/{id}",
        "POST /api/orders"
    }
}));

app.MapGet("/contracts/plain", (HttpContext httpContext) =>
        httpContext.Contract(new Money
        {
            Amount = 149.99m,
            Currency = "USD"
        }))
    .WithContract("catalog.prices", "1.2.0");

app.MapGet("/contracts/versioned", (HttpContext httpContext) =>
        httpContext.VersionedContract(new PagedResult<Money>
        {
            Items =
            [
                new Money
                {
                    Amount = 149.99m,
                    Currency = "USD"
                }
            ],
            PageInfo = new PageInfo
            {
                Page = 1,
                PageSize = 20,
                TotalCount = 1,
                TotalPages = 1,
                HasPreviousPage = false,
                HasNextPage = false
            }
        }))
    .WithContract("catalog.prices.page", "1.2.0");

app.MapGet("/contracts/request-metadata", (HttpContext httpContext) =>
{
    if (!httpContext.TryGetRequestContractMetadata(out var metadata))
    {
        return Results.BadRequest(new ErrorContract
        {
            Code = "missing_contract_metadata",
            Message = "Request headers X-Contract-Name and X-Contract-Version are required."
        });
    }

    return Results.Ok(new
    {
        metadata!.Name,
        Version = metadata.Version.ToString()
    });
});

app.MapControllers();

app.Run();