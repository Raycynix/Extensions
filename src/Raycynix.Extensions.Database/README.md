# Raycynix.Extensions.Database

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Database` is the core database package.

## What it contains

- `AddRaycynixDatabase(...)`
- `DatabaseContext`
- `DatabaseConfiguration`
- provider-specific EF Core setup
- `IDatabaseInitializer`
- `DatabaseInitializer`

## What it does not contain

- `WebApplication` extensions
- ASP.NET Core startup integration
- generic-host startup integration

## Usage

```csharp
builder.Services.AddRaycynixDatabase(builder.Configuration, options =>
{
    options.Provider = DatabaseProvider.PostgreSql;
    options.UseMigrations = true;
});
```

If you want to run initialization during startup, use one of these packages:

- `Raycynix.Extensions.Database.Hosting`
- `Raycynix.Extensions.Database.AspNetCore`
