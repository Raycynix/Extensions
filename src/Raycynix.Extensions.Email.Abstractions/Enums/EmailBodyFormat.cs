namespace Raycynix.Extensions.Email.Abstractions.Enums;

/// <summary>
/// Defines the preferred representation of an email body.
/// </summary>
public enum EmailBodyFormat
{
    /// <summary>
    /// No body content.
    /// </summary>
    Unspecified,
    
    /// <summary>
    /// Plain text body content.
    /// </summary>
    PlainText,

    /// <summary>
    /// HTML body content.
    /// </summary>
    Html
}
