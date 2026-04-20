namespace Raycynix.Extensions.Logging.Example;

internal sealed class OrderProcessor(Abstractions.ILogger<OrderProcessor> logger)
{
    public async Task ProcessAsync(string orderId, CancellationToken cancellationToken)
    {
        logger.Information("Order accepted with {@Metadata}", new
        {
            OrderId = orderId,
            Amount = 149.90m,
            Currency = "USD"
        });

        await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);

        logger.Warning("Inventory is low for one of the order items test");
        
    try

    {
            throw new InvalidOperationException("Payment provider rejected the authorization.");
        }
        catch (Exception exception)
        {
            logger.Error(exception, "Order processing failed test");
            logger.Information("Tests");
        }
    }
}