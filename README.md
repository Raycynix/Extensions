# Raycynix.Extensions

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions` is a set of infrastructure packages for .NET applications.

## Database split

The database module is now divided by responsibility:

- `Raycynix.Extensions.Database` contains the core EF Core registration and the `IDatabaseInitializer` implementation.
- `Raycynix.Extensions.Database.Hosting` contains generic-host startup extensions for `IServiceProvider` and `IHost`.
- `Raycynix.Extensions.Database.AspNetCore` contains the ASP.NET Core wrapper for `WebApplication`.

## How initialization works

`AddRaycynixDatabase(...)` only registers services. It does not initialize the database by itself.

Initialization is performed by `IDatabaseInitializer`, which:

1. Creates a scoped `DatabaseContext`
2. Runs `EnsureCreatedAsync` when `EnsureCreated` is enabled
3. Runs `MigrateAsync` when `UseMigrations` is enabled

The hosting and ASP.NET Core packages only decide when to call that initializer.

## Web application

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRaycynixDatabase(builder.Configuration, options =>
{
    options.Provider = DatabaseProvider.PostgreSql;
    options.EnsureCreated = false;
    options.UseMigrations = true;
});

var app = builder.Build();

await app.UseRaycynixDatabaseInitializationAsync();

app.Run();
```

## Console or worker application

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRaycynixDatabase(builder.Configuration, options =>
{
    options.Provider = DatabaseProvider.PostgreSql;
    options.UseMigrations = true;
});

var host = builder.Build();

await host.InitializeRaycynixDatabaseAsync();
await host.RunAsync();
```
