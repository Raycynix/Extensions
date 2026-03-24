# Raycynix.Extensions.Configuration

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Configuration` contains the core typed-configuration registration helpers for Raycynix applications.

## What it contains

- `AddRaycynixEnvironment()`
- `AddRaycynixEnvironment(string)`
- `AddRaycynixConfigurationSources(...)`
- `UseRaycynixConfigurationSources(...)`
- `AddRaycynixConfiguration<TOptions>(...)`
- `AddRaycynixConfigurationAccessor<TOptions>()`
- `AddRaycynixConfigurationValidator<TOptions, TValidator>()`
- `AddRaycynixConfigurationValidator<TOptions>(...)`
- `AddRaycynixConfigurationReloadPolicy<TOptions, TReloadPolicy>()`
- `AddRaycynixConfigurationReloadPolicy<TOptions>(...)`
- `AddRaycynixConfigurationChangeHandler<TOptions, THandler>()`
- `AddRaycynixConfigurationChangeHandler<TOptions>(...)`
- typed configuration binding based on the standard Options pipeline
- standard environment abstraction based on `IHostEnvironment`
- standard configuration source ordering
- support for default values through delegates and `IConfigurationDefaults<TOptions>`
- startup validation through standard `IValidateOptions<TOptions>` integration
- unified typed access through `IConfigurationAccessor<TOptions>`
- reload governance through `IConfigurationReloadPolicy<TOptions>`
- typed change notifications through `IOptionsMonitor<TOptions>`

## What it does not contain

- custom replacement for `IConfiguration`
- custom configuration providers
- feature flag infrastructure

## Usage

```csharp
builder.Configuration.UseRaycynixConfigurationSources(options =>
{
    options.BaseFileName = "appsettings";
    options.EnvironmentName = builder.Environment.EnvironmentName;
    options.IncludeUserSecrets = builder.Environment.IsDevelopment();
});

builder.Services.AddRaycynixEnvironment();

builder.Services.AddRaycynixConfiguration<MyOptions>(
    builder.Configuration,
    configureDefaults: options =>
    {
        options.TimeoutSeconds = 30;
    });

builder.Services.AddRaycynixConfigurationValidator<MyOptions, MyOptionsValidator>();

builder.Services.AddRaycynixConfigurationReloadPolicy<MyOptions>(context =>
{
    return ConfigurationReloadResult.Reject("MyOptions cannot be changed at runtime.");
});

builder.Services.AddRaycynixConfigurationChangeHandler<MyOptions>(
    static (context, cancellationToken) =>
    {
        Console.WriteLine($"Configuration changed: {context.ChangedAtUtc:O}");
        return ValueTask.CompletedTask;
    });
```

## Applying Runtime Reload Rules

Use runtime change handling in this order:

1. register the typed options model with `AddRaycynixConfiguration<TOptions>(...)`
2. register a validator if the options must be valid on startup
3. register a reload policy if runtime updates must be limited
4. register one or more change handlers for the updates that are actually allowed

Example:

```csharp
builder.Services.AddRaycynixConfiguration<CacheOptions>(builder.Configuration);

builder.Services.AddRaycynixConfigurationReloadPolicy<CacheOptions>(context =>
{
    if (context.Previous.ConnectionString != context.Current.ConnectionString)
    {
        return ConfigurationReloadResult.Reject(
            "CacheOptions.ConnectionString cannot be changed at runtime.");
    }

    return ConfigurationReloadResult.Apply();
});

builder.Services.AddRaycynixConfigurationChangeHandler<CacheOptions>(
    static (context, cancellationToken) =>
    {
        Console.WriteLine(
            $"Cache options reloaded at {context.ChangedAtUtc:O}. New TTL: {context.Current.DefaultTtlSeconds}");

        return ValueTask.CompletedTask;
    });
```

In this example:

- the options are still reloadable
- connection string changes are rejected
- allowed changes continue to flow through the registered handlers

By default, the package binds the section named after the options type, for example `MyOptions`.

You can override the section name explicitly when needed.

Registered validators run through the standard Options validation pipeline and are enforced on startup through `ValidateOnStart()`.

`AddRaycynixConfiguration<TOptions>(...)` also registers `IConfigurationAccessor<TOptions>` so application services can read the current typed configuration without directly depending on `IOptionsMonitor<TOptions>`.

Example:

```csharp
public class MyService(IConfigurationAccessor<MyOptions> configurationAccessor)
{
    public void Execute()
    {
        var options = configurationAccessor.Current;
        Console.WriteLine(options.TimeoutSeconds);
    }
}
```

Reload policies are evaluated before change handlers are notified. A policy can apply or reject a runtime change.

Configuration change handlers are triggered through the standard `IOptionsMonitor<TOptions>` pipeline when reloadable sources produce updated option values.

You can also declare simple runtime reload rules directly on properties:

```csharp
public class CacheOptions
{
    [ConfigurationReloadBehavior(ConfigurationReloadBehavior.Reject)]
    public string ConnectionString { get; set; } = string.Empty;

    public int DefaultTtlSeconds { get; set; }
}
```

When a property marked with `Reject` changes, the runtime update is rejected automatically before handlers are called.

The standard source order is:

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. user secrets when enabled
4. environment variables
5. command-line arguments

The standard environment names are:

- `Development`
- `Testing`
- `Staging`
- `Production`
