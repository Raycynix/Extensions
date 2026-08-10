using Microsoft.Extensions.Logging;

namespace Raycynix.Extensions.Logging.Example;

internal sealed class OrderProcessor(ILogger<OrderProcessor> logger)
{
    public async Task ProcessAsync(string orderId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Order accepted with {@Metadata}", new
        {
            OrderId = orderId,
            Amount = 149.90m,
            Currency = "USD"
        });

        await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);

        logger.LogWarning("Inventory is low for one of the order items test");

        try

        {
            throw new InvalidOperationException("Payment provider rejected the authorization.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Order processing failed test");
            logger.LogInformation("Tests");
        }
    }
}