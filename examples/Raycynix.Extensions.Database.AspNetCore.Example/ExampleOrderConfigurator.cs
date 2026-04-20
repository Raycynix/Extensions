using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Implementations;

namespace Raycynix.Extensions.Database.AspNetCore.Example;

internal sealed class ExampleOrderConfigurator : GenericConfigurator<ExampleOrder>
{
    public override Type[] DependsOn => [];

    public override void Configure(ModelBuilder modelBuilder)
    {
        var entity = ConfigureEntity(modelBuilder, "example_orders");

        entity.HasKey(current => current.Id);
        entity.HasIndex(current => current.Number).IsUnique();

        entity.Property(current => current.Number).HasMaxLength(64).IsRequired();
        entity.Property(current => current.CustomerName).HasMaxLength(200).IsRequired();
        entity.Property(current => current.Status).HasMaxLength(32).IsRequired();
        entity.Property(current => current.TotalAmount).HasPrecision(18, 2);
    }

    public override void Seed(ModelBuilder modelBuilder)
    {
        ConfigureEntity(modelBuilder, "example_orders").HasData(new ExampleOrder
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Number = "ORD-2026-1001",
            CustomerName = "Seeded Web Customer",
            TotalAmount = 99.50m,
            Status = "Completed",
            CreatedAt = new DateTime(2026, 04, 14, 12, 30, 00)
        });
    }
}