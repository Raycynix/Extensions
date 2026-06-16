using System.ComponentModel.DataAnnotations;

namespace Raycynix.Extensions.Email.Abstractions.Models;

/// <summary>
/// Represents an email mailbox address and optional display name.
/// </summary>
/// <param name="Address">The mailbox address.</param>
/// <param name="DisplayName">The optional display name.</param>
public sealed record EmailAddress(
    [Required]
    [EmailAddress]
    string Address,
    
    [MaxLength(255)]
    string? DisplayName = null);
