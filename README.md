# Raycynix.Extensions

**Raycynix.Extensions** is a robust infrastructure framework designed to accelerate the development of .NET microservices. It standardizes essential cross-cutting concerns—such as database management, observability, and resiliency—allowing you to focus on business logic rather than boilerplate code.

# 🚀 Key Features

## 👁️ Observability
- **Unified Logging**: Structured logging via Serilog (ECS compliant) with automatic `CorrelationId` injection.
- **Distributed Tracing**: Native support for `ActivitySource` (W3C standard) to track requests across service boundaries.
- **Metrics**: Automatic HTTP metrics collection for Prometheus.
- **Health Checks**: Standardized `/health` endpoints out-of-the-box.

## 💾 Database Context
- **Multi-Provider Support**: Seamless switching between `PostgreSQL`, `MS SQL Server`, `MySQL`, and `SQLite`.
- **Resiliency**: Built-in execution strategies with configurable retry policies for unstable network conditions.
- **High Performance**: Optimized with `DbContextPool` and configurable `ChangeTracker` behavior.
- **Fluent Configuration**: Use the `IConfigurator` pattern to modularize entity mapping and seeding logic.

# ⚡ Quick Start
1. **Installation**
Install the required packages in your microservice:
```bash
dotnet add package Raycynix.Extensions.Database
dotnet add package Raycynix.Extensions.Observability
```
2. **Configure** `Program.cs`
**Database & Observability Integration:**
```c#
var builder = WebApplication.CreateBuilder(args);

// 1. Setup Observability
builder.Services.AddRaycynixObservability();

// 2. Setup Database
builder.Services.AddRaycynixDatabase(builder.Configuration, options => {
    options.Provider = DatabaseProvider.PostgreSql;
    options.AutoCreate = true;
});

var app = builder.Build();

// 3. Configure Pipeline
app.UseRaycynixObservability();

app.Run();
```
# 🧩 Architectural Patterns

**The `IConfigurator` Pattern**
Instead of bloating your `DbContext` with massive `OnModelCreating` overrides, use the `IConfigurator` pattern to organize entity configurations.

**Implementation Example:**
```C#
public class UserConfigurator : IConfigurator
{
    public void Configure(ModelBuilder builder) 
    {
        builder.Entity<User>().HasKey(x => x.Id);
    }

    public void Seed(ModelBuilder builder) 
    {
        builder.Entity<User>().HasData(new User { Id = 1, Name = "Admin" });
    }
}
```

# 💡 Design Philosophy
1. **Convention over Configuration**: Sensible defaults are provided, but deep customization is always possible via `DatabaseConfiguration` and DI.
2. **Performance First**: By using `DbContextPool` and explicitly disabling unnecessary `ChangeTracker` overhead in read-heavy contexts, we ensure microservices stay snappy.
3. **Standardization**: Every microservice built with `Raycynix` shares the same logging structure, health checks, and database resiliency patterns.
