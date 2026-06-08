using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.AspNetCore.Identity.Example.Models;
using Raycynix.Extensions.Database.AspNetCore.Identity.Example.Models.Identity;
using Raycynix.Extensions.Database.Implementations;

namespace Raycynix.Extensions.Database.AspNetCore.Identity.Example.Configurators;

internal sealed class ExampleOrderConfigurator : GenericConfigurator<ExampleOrder>
{
    public override Type[] DependsOn => [typeof(ExampleUser)];

    public override void Configure(ModelBuilder modelBuilder)
    {
        var entity = ConfigureEntity(modelBuilder, "example_orders");

        entity.HasKey(current => current.Id);
        entity.HasIndex(current => current.Number).IsUnique();

        entity.Property(current => current.Number).HasMaxLength(64).IsRequired();
        entity.Property(current => current.Status).HasMaxLength(32).IsRequired();
        entity.Property(current => current.TotalAmount).HasPrecision(18, 2);

        entity.Property(current => current.CustomerId).IsRequired();
        entity
            .HasOne(current => current.Customer)
            .WithMany(current => current.Orders)
            .HasForeignKey(current => current.CustomerId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }

    public override void Seed(ModelBuilder modelBuilder)
    {
        ConfigureEntity(modelBuilder, "example_orders").HasData(new ExampleOrder
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Number = "ORD-2026-1001",
            CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            TotalAmount = 99.50m,
            Status = "Completed",
            CreatedAt = new DateTime(2026, 04, 14, 12, 30, 00)
        });
    }
}