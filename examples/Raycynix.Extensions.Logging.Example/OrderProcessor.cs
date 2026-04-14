using Raycynix.Extensions.Logging.Abstractions;

namespace Raycynix.Extensions.Logging.Example;

internal sealed class OrderProcessor(ILogger<OrderProcessor> logger)
{
    public async Task ProcessAsync(string orderId, CancellationToken cancellationToken)
    {
        logger.Information("Order accepted", new
        {
            OrderId = orderId,
            Amount = 149.90m,
            Currency = "USD"
        });

        await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);

        logger.Warning("Inventory is low for one of the order items", metadata: new
        {
            OrderId = orderId,
            Sku = "SKU-RED-MUG",
            Remaining = 2
        });

        try
        {
            throw new InvalidOperationException("Payment provider rejected the authorization.");
        }
        catch (Exception exception)
        {
            logger.Error("Order processing failed", exception, new
            {
                OrderId = orderId,
                Retryable = false
            });
        }
    }
}