namespace Raycynix.Extensions.Messaging.Grpc.Configurations;

/// <summary>
/// Configures the direct gRPC transport.
/// </summary>
public sealed class GrpcDirectMessagingConfiguration
{
    /// <summary>
    /// Gets the target service address.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Validates the gRPC configuration.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Address))
        {
            throw new InvalidOperationException("gRPC address cannot be empty.");
        }

        if (!Uri.TryCreate(Address, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("gRPC address must be an absolute URI.");
        }
    }
}
