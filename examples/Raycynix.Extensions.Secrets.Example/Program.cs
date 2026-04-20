using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Secrets;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRaycynixSecrets();

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var resolver = scope.ServiceProvider.GetRequiredService<ISecretResolver>();
const string secretKey = "ConnectionStrings:Main";

Environment.SetEnvironmentVariable(secretKey, "Server=env;Database=main;");
Environment.SetEnvironmentVariable("CONNECTIONSTRINGS_MAIN", "Server=github;Database=main;");
Environment.SetEnvironmentVariable("env.ConnectionStrings.Main", "Server=teamcity;Database=main;");

var resolved = await resolver.GetSecretAsync(secretKey);
var fallback = await resolver.GetSecretAsync("Api:Token");

Console.WriteLine("Raycynix Secrets example");
Console.WriteLine($"Requested key: {secretKey}");
Console.WriteLine($"Exact environment variable: {Environment.GetEnvironmentVariable(secretKey)}");
Console.WriteLine($"GitHub-style variable: {Environment.GetEnvironmentVariable("CONNECTIONSTRINGS_MAIN")}");
Console.WriteLine($"TeamCity-style variable: {Environment.GetEnvironmentVariable("env.ConnectionStrings.Main")}");
Console.WriteLine($"Resolved secret: {resolved}");
Console.WriteLine();
Console.WriteLine("Fallback lookup");
Console.WriteLine("Requested key: Api:Token");
Console.WriteLine($"Resolved secret: {fallback ?? "<null>"}");

Environment.SetEnvironmentVariable(secretKey, null);
Environment.SetEnvironmentVariable("CONNECTIONSTRINGS_MAIN", null);
Environment.SetEnvironmentVariable("env.ConnectionStrings.Main", null);
