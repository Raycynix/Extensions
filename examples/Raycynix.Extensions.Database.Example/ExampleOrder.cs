namespace Raycynix.Extensions.Database.Example;

internal sealed class ExampleOrder
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}