namespace Raycynix.Extensions.Exceptions.Abstractions.Interfaces;

public interface IExceptionDataMasker
{
    object? Mask(object? data);
}