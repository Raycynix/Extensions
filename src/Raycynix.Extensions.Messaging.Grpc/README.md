# Raycynix.Extensions.Messaging.Grpc

![TeamCity build status](https://ci.raycynix.com/app/rest/builds/buildType:id:RSX_Extensions_Building/statusIcon.svg)

`Raycynix.Extensions.Messaging.Grpc` contains the direct gRPC transport for Raycynix messaging.

## What it contains

- `AddGrpc(...)`
- `AddGrpcUnary<TGrpcClient, TRequest, TResponse>(...)`
- `GrpcDirectMessagingConfiguration`
- `IGrpcRequestClient`
- gRPC unary request/response transport over `Grpc.Net.Client`

## Usage

Register the transport and map logical destinations to unary gRPC calls:

```csharp
builder.Services.AddRaycynixMessaging(builder.Configuration)
    .AddGrpc(options =>
    {
        options.Address = "https://catalog-service";
    })
    .AddGrpcUnary<Catalog.CatalogClient, GetCatalogItemRequest, CatalogItemReply>(
        "catalog/get-item",
        static async (client, request, cancellationToken) =>
            await client.GetItemAsync(request, cancellationToken: cancellationToken));
```

Send a direct request:

```csharp
public class CatalogGateway(
    IRequestEnvelopeFactory envelopeFactory,
    IGrpcRequestClient requestClient)
{
    public async Task<CatalogItemReply> GetItemAsync(string sku, CancellationToken cancellationToken)
    {
        var request = envelopeFactory.Create(
            new GetCatalogItemRequest { Sku = sku },
            destination: "catalog/get-item",
            format: MessageFormat.Grpc);

        var response = await requestClient.SendAsync<GetCatalogItemRequest, CatalogItemReply>(
            request,
            cancellationToken);

        return response.Response;
    }
}
```

Notes:

- `destination` is a logical operation key, not a URL
- the registered unary mapping decides which generated gRPC client and method are called
- contract, correlation, trace, and security headers are added on the shared request envelope level
