using Raycynix.Extensions.Database.AspNetCore.Identity.Example.Models.Identity;

namespace Raycynix.Extensions.Database.AspNetCore.Identity.Example.Models;

public class ExampleOrder
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }
    public virtual ExampleUser? Customer { get; set; } 

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}