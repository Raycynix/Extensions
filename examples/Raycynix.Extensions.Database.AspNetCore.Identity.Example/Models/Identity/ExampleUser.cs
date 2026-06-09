using Microsoft.AspNetCore.Identity;

namespace Raycynix.Extensions.Database.AspNetCore.Identity.Example.Models.Identity;

public class ExampleUser : IdentityUser<Guid>
{
    public ExampleUser()
    {
    }
    
    
    
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    public virtual List<ExampleOrder>? Orders { get; set; }
}