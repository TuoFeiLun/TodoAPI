using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Http.HttpResults;
using MyApi.Model.User;
using MyApi.Database;
using System.Security.Claims;
using MyApi.Model.ChangePasswordRequest;
namespace MyApi.Controller;

public class UserController
{
    public static async Task<IResult> GetAllUsers(UserDb db)
    {
        return TypedResults.Ok(await db.Users.Select(x => new User(x.Name, x.Password ?? "", x.Email ?? "", x.IsAdmin)).ToArrayAsync());
    }

    public static async Task<IResult> GetUser(int id, UserDb db)
    {
        return await db.Users.FindAsync(id)
            is User user
                ? TypedResults.Ok(user)
                : TypedResults.NotFound();
    }
    public static async Task<IResult> CreateUser(User user, UserDb db)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return TypedResults.Created($"/users/{user.Id}", user);
    }
    public static async Task<IResult> UpdateUser(int id, User user, UserDb db)
    {
        var existingUser = await db.Users.FindAsync(id);
        if (existingUser is null) return TypedResults.NotFound();
        existingUser.Name = user.Name;
        existingUser.Email = user.Email;
        existingUser.Password = user.Password;
        existingUser.IsAdmin = user.IsAdmin;
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    public static async Task<IResult> DeleteUser(int id, UserDb db)
    {
        if (await db.Users.FindAsync(id) is User user)
        {
            db.Users.Remove(user);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }
        return TypedResults.NotFound();
    }


    // Change password using JWT authentication - more secure approach
    public static async Task<IResult> ChangePassword(HttpContext context, ChangePasswordRequest request, UserDb db)
    {
        // Get user ID from JWT token
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return TypedResults.Unauthorized();
        }

        if (!int.TryParse(userIdClaim.Value, out int userId))
        {
            return TypedResults.BadRequest("Invalid user ID in token");
        }

        // Find user in database by ID from JWT
        var user = await db.Users.FindAsync(userId);
        if (user is null)
        {
            return TypedResults.NotFound("User not found");
        }

        // Verify old password
        if (user.Password != request.OldPassword)
        {
            return TypedResults.Unauthorized();
        }

        // Update password
        user.Password = request.NewPassword;
        user.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();

        return TypedResults.Ok(new
        {
            message = "Password changed successfully",
            email = user.Email,
            updatedAt = user.UpdatedAt
        });
    }


    // Get current user info from JWT token
    public static async Task<IResult> GetCurrentUser(HttpContext context, UserDb db)
    {
        // Get user ID from JWT token
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return TypedResults.Unauthorized();
        }

        if (!int.TryParse(userIdClaim.Value, out int userId))
        {
            return TypedResults.BadRequest("Invalid user ID in token");
        }

        // Find user in database
        var user = await db.Users.FindAsync(userId);
        if (user is null)
        {
            return TypedResults.NotFound("User not found");
        }

        // Return user info (without password)
        return TypedResults.Ok(new
        {
            id = user.Id,
            name = user.Name,
            email = user.Email,
            isAdmin = user.IsAdmin,
            createdAt = user.CreatedAt,
            updatedAt = user.UpdatedAt
        });
    }
}


