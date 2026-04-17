using Raycynix.Extensions.Common.Context;
using Raycynix.Extensions.Exceptions;
using Raycynix.Extensions.Exceptions.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IOperationContext>(_ => new OperationContext
{
    CorrelationId = "example-correlation-id",
    UserId = "example-user"
});
builder.Services.AddRaycynixExceptions(options =>
{
    options.Map<InvalidOperationException>(
        errorCode: "invalid_operation",
        message: "The operation is not valid.",
        statusCode: 409,
        category: Raycynix.Extensions.Exceptions.Abstractions.Enums.ErrorCategory.Conflict);
});

var app = builder.Build();

app.UseRaycynixExceptions();

app.MapGet("/", () => Results.Ok(new
{
    Service = "Raycynix.Extensions.Exceptions.AspNetCore.Example",
    Endpoints = new[]
    {
        "GET /not-found",
        "GET /validation",
        "GET /mapped",
        "GET /transient"
    }
}));

app.MapGet("/not-found", () =>
{
    throw new NotFoundException("Order was not found.");
});

app.MapGet("/validation", () =>
{
    throw new ValidationException("Validation failed.", new Dictionary<string, string[]>
    {
        ["email"] = ["Email is required."],
        ["quantity"] = ["Quantity must be greater than zero."]
    });
});

app.MapGet("/mapped", () =>
{
    throw new InvalidOperationException("This endpoint simulates a mapped domain error.");
});

app.MapGet("/transient", () =>
{
    throw new TransientFailureException(
        message: "Temporary dependency outage.",
        retryAfterSeconds: 5,
        operationName: "CallExternalService");
});

app.Run();
