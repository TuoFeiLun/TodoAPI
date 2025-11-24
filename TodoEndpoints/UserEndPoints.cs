using Microsoft.AspNetCore.Authorization;
using MyApi.Controller;
using MyApi.Database;
using MyApi.Model.ChangePasswordRequest;
namespace MinAPISeparateFile;

public static class UserEndPoints
{
    public static void MapUserEndpoints(WebApplication app)
    {
        var userItems = app.MapGroup("/users");

        userItems.MapGet("/", [Authorize("create_and_delete_user")] (UserDb db) =>
            UserController.GetAllUsers(db));
        userItems.MapGet("/{id}", [Authorize("create_and_delete_user")] (int id, UserDb db) =>
            UserController.GetUser(id, db));
        userItems.MapPost("/", [Authorize("create_and_delete_user")] (MyApi.Model.User.User user, UserDb db) =>
            UserController.CreateUser(user, db));
        userItems.MapPut("/{id}", [Authorize("editor_user")] (int id, MyApi.Model.User.User user, UserDb db) =>
            UserController.UpdateUser(id, user, db));
        userItems.MapDelete("/{id}", [Authorize("create_and_delete_user")] (int id, UserDb db) =>
            UserController.DeleteUser(id, db));

        // Authenticated user endpoints - use JWT to get current user
        userItems.MapPut("/change-password", [Authorize] (HttpContext context, ChangePasswordRequest request, UserDb db) =>
            UserController.ChangePassword(context, request, db));
        userItems.MapGet("/me", [Authorize("viewer_user")] (HttpContext context, UserDb db) =>
            UserController.GetCurrentUser(context, db));
    }
}