using Microsoft.AspNetCore.Authorization;
using MyApi.Controller;
using MyApi.Database;
using MyApi.Model.ChangePasswordRequest;
using static MyApi.Authorization.AuthorizePolicies;
using System.Text.Json;

namespace MinAPISeparateFile;

public static class UserEndPoints
{
    public static void MapUserEndpoints(WebApplication app)
    {
        var userItems = app.MapGroup("/users");

        // Admin-only user management endpoints
        userItems.MapGet("/", [Authorize(CreateAndDeleteUser)] (AppDbContext db) =>
            UserController.GetAllUsers(db));
        userItems.MapGet("/{id}", [Authorize(CreateAndDeleteUser)] (int id, AppDbContext db) =>
            UserController.GetUser(id, db));
        userItems.MapPost("/", [Authorize(CreateAndDeleteUser)] (MyApi.Model.User.User user, AppDbContext db) =>
            UserController.CreateUser(user, db));
        userItems.MapPatch("/{id}", [Authorize(EditorUser)] (int id, JsonElement patchData, AppDbContext db) =>
            UserController.UpdateUser(id, patchData, db));
        userItems.MapPatch("/role/{id}", [Authorize(ChangeUserRole)] (int id, JsonElement patchData, AppDbContext db) =>
            UserController.ChangeUserRole(id, patchData, db));
        userItems.MapDelete("/{id}", [Authorize(CreateAndDeleteUser)] (int id, AppDbContext db) =>
            UserController.DeleteUser(id, db));

        // Authenticated user endpoints - use JWT to get current user
        userItems.MapPut("/change-password", [Authorize] (HttpContext context, ChangePasswordRequest request, AppDbContext db) =>
            UserController.ChangePassword(context, request, db));
        userItems.MapGet("/me", [Authorize(ViewerUser)] (HttpContext context, AppDbContext db) =>
            UserController.GetCurrentUser(context, db));
    }
}