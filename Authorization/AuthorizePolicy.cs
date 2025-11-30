using Microsoft.AspNetCore.Authorization;
using MyApi.Model.User;

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

    // Role names from UserRole enum for consistency
    private static readonly string Admin = nameof(UserRole.Admin);
    private static readonly string Editor = nameof(UserRole.Editor);
    private static readonly string Viewer = nameof(UserRole.Viewer);

    // Configure all authorization policies - extension method for fluent API
    public static AuthorizationBuilder AddAuthorizationPolicies(this AuthorizationBuilder builder)
    {
        // User management policies
        builder.AddPolicy(CreateAndDeleteUser, policy =>
            policy.RequireRole(Admin)
                  .RequireClaim("scope", CreateAndDeleteUser));

        builder.AddPolicy(ChangeUserRole, policy =>
            policy.RequireRole(Admin)
                  .RequireClaim("scope", ChangeUserRole));

        builder.AddPolicy(EditorUser, policy =>
            policy.RequireRole(Admin, Editor)
                  .RequireClaim("scope", "update_user"));

        builder.AddPolicy(ViewerUser, policy =>
            policy.RequireRole(Admin, Editor, Viewer)
                  .RequireClaim("scope", "view_user"));

        // Todo item policies
        builder.AddPolicy(ViewerTodoItem, policy =>
            policy.RequireRole(Admin, Editor, Viewer)
                  .RequireClaim("scope", "view_todoitem"));

        builder.AddPolicy(CrudTodoItem, policy =>
            policy.RequireRole(Editor)
                  .RequireClaim("scope", "crud_todoitem"));

        return builder;
    }
}