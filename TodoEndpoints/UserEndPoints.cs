using Microsoft.AspNetCore.Authorization;
using MyApi.Controller;
using MyApi.Database;
using MyApi.Model.ChangePasswordRequest;
using static MyApi.Authorization.AuthorizePolicies;
namespace MinAPISeparateFile;

public static class UserEndPoints
{
    public static void MapUserEndpoints(WebApplication app)
    {
        var userItems = app.MapGroup("/users");

        // Admin-only user management endpoints
        userItems.MapGet("/", [Authorize(CreateAndDeleteUser)] (UserDb db) =>
            UserController.GetAllUsers(db));
        userItems.MapGet("/{id}", [Authorize(CreateAndDeleteUser)] (int id, UserDb db) =>
            UserController.GetUser(id, db));
        userItems.MapPost("/", [Authorize(CreateAndDeleteUser)] (MyApi.Model.User.User user, UserDb db) =>
            UserController.CreateUser(user, db));
        userItems.MapPut("/{id}", [Authorize(EditorUser)] (int id, MyApi.Model.User.User user, UserDb db) =>
            UserController.UpdateUser(id, user, db));
        userItems.MapPut("/role/{id}", [Authorize(ChangeUserRole)] (int id, MyApi.Model.User.User user, UserDb db) =>
            UserController.ChangeUserRole(id, user, db));
        userItems.MapDelete("/{id}", [Authorize(CreateAndDeleteUser)] (int id, UserDb db) =>
            UserController.DeleteUser(id, db));

        // Authenticated user endpoints - use JWT to get current user
        userItems.MapPut("/change-password", [Authorize] (HttpContext context, ChangePasswordRequest request, UserDb db) =>
            UserController.ChangePassword(context, request, db));
        userItems.MapGet("/me", [Authorize(ViewerUser)] (HttpContext context, UserDb db) =>
            UserController.GetCurrentUser(context, db));
    }
}