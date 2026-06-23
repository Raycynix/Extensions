using Raycynix.Extensions.Database.AspNetCore.Identity.Example.Models.Identity;

namespace Raycynix.Extensions.Database.AspNetCore.Identity.Example.Models;

/// <summary>
/// Represents an example application entity owned by an identity user.
/// </summary>
public class ExampleOrder
{
    /// <summary>
    /// Gets or sets the unique order identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the business order number shown to users.
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the user who owns the order.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the identity user navigation property for the order owner.
    /// </summary>
    public ExampleUser? Customer { get; set; }

    /// <summary>
    /// Gets or sets the total monetary amount of the order.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the current order status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC timestamp when the order was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

internal abstract record CreateExampleOrderRequest(Guid CustomerId, decimal TotalAmount);

internal sealed record ExampleOrderResponse(
    string Number,
    Guid CustomerId,
    decimal TotalAmount,
    string Status,
    DateTimeOffset CreatedAtUtc);

internal abstract class OrderEndpoints;