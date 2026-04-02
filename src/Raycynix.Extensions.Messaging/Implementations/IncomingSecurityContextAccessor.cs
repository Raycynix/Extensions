using System.Threading;
using Raycynix.Extensions.Security.Abstractions.Interfaces;

namespace Raycynix.Extensions.Messaging.Implementations;

/// <summary>
/// Stores the current inbound messaging security context for the active asynchronous execution flow.
/// </summary>
internal sealed class IncomingSecurityContextAccessor
{
    private readonly AsyncLocal<ISecurityContext?> current = new();

    /// <summary>
    /// Gets the current inbound messaging security context.
    /// </summary>
    public ISecurityContext Current => current.Value ?? new MessagingSecurityContext();

    /// <summary>
    /// Pushes a security context for the active asynchronous flow and returns a scope that restores the previous value.
    /// </summary>
    /// <param name="securityContext">The security context to apply.</param>
    /// <returns>A disposable scope that restores the previous context.</returns>
    public IDisposable Push(ISecurityContext securityContext)
    {
        ArgumentNullException.ThrowIfNull(securityContext);

        var previous = current.Value;
        current.Value = securityContext;
        return new RestoreScope(current, previous);
    }

    private sealed class RestoreScope(AsyncLocal<ISecurityContext?> state, ISecurityContext? previous) : IDisposable
    {
        public void Dispose()
        {
            state.Value = previous;
        }
    }
}
