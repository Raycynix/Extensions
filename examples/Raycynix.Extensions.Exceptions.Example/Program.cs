using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Exceptions;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;
using Raycynix.Extensions.Exceptions.Abstractions.Options;
using Raycynix.Extensions.Exceptions.Example;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRaycynixExceptions(options =>
{
    options.Map<InvalidOperationException>(
        errorCode: "invalid_operation",
        message: "The requested operation is not allowed in the current state.",
        statusCode: 409,
        category: Raycynix.Extensions.Exceptions.Abstractions.Enums.ErrorCategory.Conflict);
});
builder.Services.AddHostedService<ExceptionsExampleWorker>();

await builder.Build().RunAsync();