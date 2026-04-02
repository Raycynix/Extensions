using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Raycynix.Extensions.Security.AspNetCore.Authorization.Conventions;

/// <summary>
/// Applies Raycynix security attributes declared on MVC controllers and actions as standard ASP.NET Core authorization filters.
/// </summary>
public sealed class RaycynixAuthorizationApplicationModelConvention : IApplicationModelConvention
{
    /// <inheritdoc />
    public void Apply(ApplicationModel application)
    {
        ArgumentNullException.ThrowIfNull(application);

        foreach (var controller in application.Controllers)
        {
            ApplyPolicies(controller.Attributes, controller.Filters);

            foreach (var action in controller.Actions)
            {
                ApplyPolicies(action.Attributes, action.Filters);
            }
        }
    }

    private static void ApplyPolicies(IReadOnlyList<object> attributes, IList<IFilterMetadata> filters)
    {
        var policies = SecurityPolicies.FromAttributes(attributes);
        foreach (var policy in policies)
        {
            filters.Add(new AuthorizeFilter(policy));
        }
    }
}
