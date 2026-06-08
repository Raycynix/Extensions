using Microsoft.AspNetCore.Identity;

namespace Raycynix.Extensions.Database.AspNetCore.Identity.Example.Models.Identity;

public class ExampleRole : IdentityRole<Guid>
{
    public ExampleRole()
    {
    }

    public ExampleRole(string name) : base(name)
    {
        
    }

    public ExampleRole(Guid id, string name)
    {
        Id = id;
        Name = name;
        NormalizedName = name.ToUpperInvariant();
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }
}