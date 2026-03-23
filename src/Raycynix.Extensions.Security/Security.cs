using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raycynix.Extensions.Security.Abstractions.Interfaces;
using Raycynix.Extensions.Security.Implementation;

namespace Raycynix.Extensions.Security;

public static class Security
{
    public static IServiceCollection AddRaycynixSecurity(this IServiceCollection services)
    {
        services.TryAddScoped<ISecurityContext, SecurityContext>();
        
        return services;
    }
}