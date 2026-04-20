using System.ComponentModel.DataAnnotations;
using Raycynix.Extensions.Contracts.Models;

namespace Raycynix.Extensions.Contracts.AspNetCore.Example.Models;

internal sealed class CreateOrderRequest
{
    [Required]
    public string OrderId { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    public Money Price { get; set; } = new();
}