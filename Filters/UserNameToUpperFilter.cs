using MyApi.Model.User;

namespace MyApi.Filters.EndpointFilters;

/// <summary>
/// Endpoint filter that converts user name to uppercase before processing
/// </summary>
public class UserNameToUpperFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var user = context.GetArgument<User>(0);

        if (user != null && !string.IsNullOrEmpty(user.Name))
        {
            user.Name = user.Name.ToUpper();
        }

        return await next(context);
    }
}

