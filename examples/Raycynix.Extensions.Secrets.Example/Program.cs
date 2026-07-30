using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Secrets;
using Raycynix.Extensions.Secrets.Extensions;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

const string secretKey = "ConnectionStrings:Main";
const string requiredKey = "Api:Token";

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration[secretKey] = "Server=config;Database=main;";
builder.Configuration[requiredKey] = "config-token";

Environment.SetEnvironmentVariable(secretKey, "Server=env;Database=main;");
Environment.SetEnvironmentVariable("CONNECTIONSTRINGS_MAIN", "Server=github;Database=main;");
Environment.SetEnvironmentVariable("ConnectionStrings.Main", "Server=teamcity;Database=main;");

builder.Services.AddRaycynixSecrets(builder.Configuration);

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var resolver = scope.ServiceProvider.GetRequiredService<ISecretResolver>();
var diagnosticsResolver = (ISecretDiagnosticsResolver)resolver;

Console.WriteLine("Raycynix Secrets example");
Console.WriteLine();
Console.WriteLine("Available values");
Console.WriteLine($"Configuration[{secretKey}]: {builder.Configuration[secretKey]}");
Console.WriteLine($"Environment[{secretKey}]: {Environment.GetEnvironmentVariable(secretKey)}");
Console.WriteLine($"Environment[CONNECTIONSTRINGS_MAIN]: {Environment.GetEnvironmentVariable("CONNECTIONSTRINGS_MAIN")}");
Console.WriteLine($"Environment[ConnectionStrings.Main]: {Environment.GetEnvironmentVariable("ConnectionStrings.Main")}");
Console.WriteLine();

var resolved = await resolver.GetSecretAsync(secretKey);
var resolvedWithSource = await resolver.ResolveWithSourceAsync(secretKey);
var requiredSecret = await resolver.GetRequiredSecretAsync(requiredKey);
var attempts = await diagnosticsResolver.ExplainSecretResolutionAsync(secretKey);

Console.WriteLine("Default provider order");
Console.WriteLine("Configuration -> Environment -> GitHub -> TeamCity");
Console.WriteLine($"GetSecretAsync: {resolved}");
Console.WriteLine($"ResolveWithSourceAsync.ProviderName: {resolvedWithSource.ProviderName}");
Console.WriteLine($"GetRequiredSecretAsync({requiredKey}): {requiredSecret}");
Console.WriteLine("ExplainSecretResolutionAsync:");
foreach (var attempt in attempts)
{
    Console.WriteLine($"- {attempt.ProviderName}: {attempt.Succeeded}");
}

Console.WriteLine();
Console.WriteLine("Custom provider order");
Console.WriteLine("GitHub -> Configuration -> Environment -> TeamCity");

var customOrderBuilder = Host.CreateApplicationBuilder(args);
customOrderBuilder.Configuration[secretKey] = builder.Configuration[secretKey];
customOrderBuilder.Configuration[requiredKey] = builder.Configuration[requiredKey];
customOrderBuilder.Configuration["SecretOptions:ProviderOrder:0"] = SecretProviderNames.GitHub;
customOrderBuilder.Configuration["SecretOptions:ProviderOrder:1"] = SecretProviderNames.Configuration;
customOrderBuilder.Configuration["SecretOptions:ProviderOrder:2"] = SecretProviderNames.Environment;
customOrderBuilder.Configuration["SecretOptions:ProviderOrder:3"] = SecretProviderNames.TeamCity;
customOrderBuilder.Services.AddRaycynixSecrets(customOrderBuilder.Configuration);

using var customOrderHost = customOrderBuilder.Build();
using var customOrderScope = customOrderHost.Services.CreateScope();

var customOrderResolver = customOrderScope.ServiceProvider.GetRequiredService<ISecretResolver>();
var customOrderResult = await customOrderResolver.ResolveWithSourceAsync(secretKey);
var customOrderAttempts = await customOrderResolver.ExplainSecretResolutionAsync(secretKey);

Console.WriteLine($"ResolveWithSourceAsync.Value: {customOrderResult.Value}");
Console.WriteLine($"ResolveWithSourceAsync.ProviderName: {customOrderResult.ProviderName}");
Console.WriteLine("ExplainSecretResolutionAsync:");
foreach (var attempt in customOrderAttempts)
{
    Console.WriteLine($"- {attempt.ProviderName}: {attempt.Succeeded}");
}

Environment.SetEnvironmentVariable(secretKey, null);
Environment.SetEnvironmentVariable("CONNECTIONSTRINGS_MAIN", null);
Environment.SetEnvironmentVariable("ConnectionStrings.Main", null);
