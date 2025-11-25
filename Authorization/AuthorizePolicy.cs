using Microsoft.AspNetCore.Authorization;

namespace MyApi.Authorization;

// RULE TEST OK
// Centralized authorization policy configuration
public static class AuthorizePolicies
{
    // Policy names as constants for type safety
    public const string CreateAndDeleteUser = "create_and_delete_user";
    public const string ChangeUserRole = "change_user_role";
    public const string EditorUser = "editor_user";
    public const string ViewerUser = "viewer_user";
    public const string ViewerTodoItem = "viewer_todoitem";
    public const string CrudTodoItem = "crud_todoitem";

    // Configure all authorization policies - extension method for fluent API
    public static AuthorizationBuilder AddAuthorizationPolicies(this AuthorizationBuilder builder)
    {
        // User management policies
        builder.AddPolicy(CreateAndDeleteUser, policy =>
            policy.RequireRole("admin")
                  .RequireClaim("scope", CreateAndDeleteUser));

        builder.AddPolicy(ChangeUserRole, policy =>
            policy.RequireRole("admin")
                  .RequireClaim("scope", ChangeUserRole));

        builder.AddPolicy(EditorUser, policy =>
            policy.RequireRole("admin", "editor")
                  .RequireClaim("scope", "update_user"));

        builder.AddPolicy(ViewerUser, policy =>
            policy.RequireRole("admin", "editor", "viewer")
                  .RequireClaim("scope", "view_user"));

        // Todo item policies
        builder.AddPolicy(ViewerTodoItem, policy =>
            policy.RequireRole("admin", "editor", "viewer")
                  .RequireClaim("scope", "view_todoitem"));

        builder.AddPolicy(CrudTodoItem, policy =>
            policy.RequireRole("editor")
                  .RequireClaim("scope", "crud_todoitem"));

        return builder;
    }
}