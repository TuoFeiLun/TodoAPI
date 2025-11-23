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

        // Admin-only endpoints
        userItems.MapGet("/", [Authorize("AdminsOnly")] () => UserController.GetAllUsers);
        userItems.MapGet("/{id}", [Authorize("AdminsOnly")] () => UserController.GetUser);
        userItems.MapPost("/", [Authorize("AdminsOnly")] () => UserController.CreateUser);
        userItems.MapPut("/{id}", [Authorize("AdminsOnly")] () => UserController.UpdateUser);
        userItems.MapDelete("/{id}", [Authorize("AdminsOnly")] () => UserController.DeleteUser);

        // Authenticated user endpoints - use JWT to get current user
        userItems.MapPut("/change-password", [Authorize] (HttpContext context, ChangePasswordRequest request, UserDb db) =>
            UserController.ChangePassword(context, request, db));
        userItems.MapGet("/me", [Authorize] (HttpContext context, UserDb db) =>
            UserController.GetCurrentUser(context, db));
    }
}