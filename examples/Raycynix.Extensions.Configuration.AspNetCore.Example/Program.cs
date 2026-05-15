using Raycynix.Extensions.Configuration;
using Raycynix.Extensions.Configuration.Abstractions.Interfaces;
using Raycynix.Extensions.Configuration.AspNetCore;

Environment.CurrentDirectory = AppContext.BaseDirectory;

var builder = WebApplication.CreateBuilder(args);

builder.AddRaycynixAspNetCoreConfiguration();

builder.Services.AddRaycynixFeatureFlags(builder.Configuration, requireSection: true);
builder.Services.AddRaycynixConfiguration<DashboardOptions>(
    builder.Configuration,
    requireSection: true);
builder.Services.AddRaycynixConfigurationValidator<DashboardOptions>(
    options => options.RefreshIntervalSeconds > 0,
    "DashboardOptions.RefreshIntervalSeconds must be greater than zero.");
builder.Services.AddRaycynixConfigurationChangeHandler<DashboardOptions>(
    static (context, cancellationToken) =>
    {
        Console.WriteLine(
            $"Dashboard options changed at {context.ChangedAtUtc:O}. Theme={context.Current.Theme}, Refresh={context.Current.RefreshIntervalSeconds}s");

        return ValueTask.CompletedTask;
    });

var app = builder.Build();

app.UseRaycynixAspNetCoreConfiguration();

app.MapGet("/", (IApplicationEnvironment environment, IFeatureFlagAccessor featureFlags) => Results.Ok(new
{
    Service = "Raycynix.Extensions.Configuration.AspNetCore.Example",
    Environment = environment.Name,
    Features = featureFlags.GetAll()
}));

app.MapGet("/config", (IConfigurationAccessor<DashboardOptions> accessor) =>
{
    var options = accessor.Current;

    return Results.Ok(new
    {
        options.Theme,
        options.RefreshIntervalSeconds,
        options.EnableCaching
    });
});

app.MapGet("/features", (IFeatureFlagAccessor featureFlags) => Results.Ok(featureFlags.GetAll()));

app.MapGet("/dashboard", (IConfigurationAccessor<DashboardOptions> accessor) =>
{
    var options = accessor.Current;

    return Results.Ok(new
    {
        Message = "Dashboard endpoint is enabled.",
        options.Theme,
        options.RefreshIntervalSeconds
    });
}).RequireFeature("NewDashboard");

app.MapGet("/beta-or-admin", () => Results.Ok(new
{
    Message = "At least one gated feature is enabled."
})).RequireAnyFeature("BetaApi", "AdminApi");

app.Run();

internal sealed class DashboardOptions
{
    public string Theme { get; set; } = "light";

    public int RefreshIntervalSeconds { get; set; } = 30;

    public bool EnableCaching { get; set; } = true;
}
