namespace Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

public interface IExceptionMapper
{
    RaycynixException Map(Exception exception);
}