using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.AspNetCore.Identity.Example.Models.Identity;
using Raycynix.Extensions.Database.Implementations;

namespace Raycynix.Extensions.Database.AspNetCore.Identity.Example.Configurators;

/// <summary>
/// Configures the example identity user entity for the shared Raycynix database model.
/// </summary>
public class ExampleUserConfigurator : GenericConfigurator<ExampleUser>
{
    /// <inheritdoc />
    public override Type[] DependsOn => [];

    /// <inheritdoc />
    public override void Configure(ModelBuilder modelBuilder)
    {
        base.Configure(modelBuilder);

        var entity = ConfigureEntity(modelBuilder, "example_users");

        entity.Property(current => current.UserName).HasMaxLength(200)
            .IsRequired();

        entity.Property(current => current.Email).HasMaxLength(200);
        entity.Property(current => current.CreatedAt).HasDefaultValue(DateTime.UtcNow).IsRequired();
        entity.Property(current => current.LastLoginAt);
    }

    /// <inheritdoc />
    public override void Seed(ModelBuilder modelBuilder)
    {
        base.Seed(modelBuilder);
        ConfigureEntity(modelBuilder, "example_users").HasData(new ExampleUser
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                UserName = "seed_user",
                Email = "",
                EmailConfirmed = true,
                PasswordHash = "=",
                SecurityStamp = "11111111-1111-1111-1111-111111111111",
                PhoneNumberConfirmed = true,
                PhoneNumber = "1234567890"
            }
        );
    }
}
