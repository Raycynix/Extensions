using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;
using Raycynix.Extensions.Exceptions.Abstractions.Options;

namespace Raycynix.Extensions.Exceptions.Example;

internal sealed class ExceptionsExampleWorker(
    IExceptionMapper exceptionMapper,
    IExceptionDataMasker exceptionDataMasker,
    IRetryExecutor retryExecutor,
    IBackgroundTaskRunner backgroundTaskRunner,
    IHostApplicationLifetime applicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Raycynix exceptions example");

        var mapped = exceptionMapper.Map(new InvalidOperationException("Order is already closed."));
        Console.WriteLine($"Mapped exception: {mapped.ErrorCode} ({mapped.StatusCode}) - {mapped.Message}");

        var masked = exceptionDataMasker.Mask(new
        {
            ApiKey = "super-secret-key",
            Password = "qwerty",
            SafeValue = "visible"
        });
        Console.WriteLine($"Masked details type: {masked?.GetType().Name}");

        var retryAttempts = 0;
        var retryResult = await retryExecutor.ExecuteAsync(
            async cancellationToken =>
            {
                retryAttempts++;

                if (retryAttempts < 3)
                {
                    throw new TimeoutException("Temporary upstream timeout.");
                }

                await Task.Delay(10, cancellationToken);
                return $"Succeeded on attempt {retryAttempts}";
            },
            new RetryExecutionOptions
            {
                MaxRetries = 3,
                Delay = TimeSpan.FromMilliseconds(50),
                UseExponentialBackoff = false,
                UseJitter = false
            },
            operationName: "LoadCatalog",
            cancellationToken: stoppingToken);

        Console.WriteLine(retryResult);

        try
        {
            await backgroundTaskRunner.RunAsync(
                cancellationToken => throw new InvalidOperationException("Background import failed."),
                "ImportProducts",
                stoppingToken);
        }
        catch (Exception exception)
        {
            var mappedBackgroundException = exceptionMapper.Map(exception);
            Console.WriteLine(
                $"Background failure mapped as: {mappedBackgroundException.ErrorCode} ({mappedBackgroundException.Category})");
        }

        try
        {
            throw new ValidationException(
                "The request is invalid.",
                new Dictionary<string, string[]>
                {
                    ["customerId"] = ["Customer is required."],
                    ["amount"] = ["Amount must be greater than zero."]
                });
        }
        catch (ValidationException exception)
        {
            Console.WriteLine($"Validation exception contains {exception.ValidationErrors.Count} field groups.");
        }

        applicationLifetime.StopApplication();
    }
}
