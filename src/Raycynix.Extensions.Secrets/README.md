# Raycynix.Extensions.Secrets

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Secrets` contains the core secret resolution services for Raycynix applications.

## What it contains

- `AddRaycynixSecrets(...)`
- `ISecretProvider` registrations
- `ISecretResolver`
- environment, GitHub Actions, and TeamCity-style secret providers

## What it does not contain

- cloud-specific secret storage integrations
- UI or interactive secret management
- secret values in configuration files

## Usage

```csharp
builder.Services.AddRaycynixSecrets();
```

```csharp
public sealed class GitHubTokenLoader(ISecretResolver secrets)
{
    public async Task<string?> LoadAsync(CancellationToken cancellationToken)
    {
        return await secrets.GetSecretAsync("GitHub:Token", cancellationToken);
    }
}
```

The package resolves secrets through a provider chain and returns the first available value.

Secrets should be injected through providers and resolvers instead of being stored in `appsettings.json`.
