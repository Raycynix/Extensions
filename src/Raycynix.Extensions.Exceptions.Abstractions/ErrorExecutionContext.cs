using Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

namespace Raycynix.Extensions.Exceptions.Abstractions;

/// <summary>
/// Represents the execution context in which an error occurred.
/// </summary>
public sealed record ErrorExecutionContext(
    string Source,
    string? OperationName = null,
    string? TraceId = null,
    string? SpanId = null,
    string? CorrelationId = null,
    string? UserId = null,
    string? Path = null,
    string? Method = null,
    string? Endpoint = null,
    string? QueryString = null,
    int? Attempt = null,
    int? MaxAttempts = null,
    bool IsTransient = false) : IErrorExecutionContext;
