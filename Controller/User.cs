using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Http.HttpResults;
using MyApi.Model.User;
using MyApi.Database;
using System.Security.Claims;
using MyApi.Model.ChangePasswordRequest;
using MyApi.Services;
namespace MyApi.Controller;

public class UserController
{
    // RULE TEST OK
    // Get all users without password information
    public static async Task<IResult> GetAllUsers(UserDb db)
    {
        var users = await db.Users.Select(x => new UserResponseDto
        {
            Id = x.Id,
            Name = x.Name,
            Email = x.Email,
            Role = x.Role,
            IsAdmin = x.IsAdmin,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        }).ToArrayAsync();

        return TypedResults.Ok(users);
    }


    // Get single user without password information
    public static async Task<IResult> GetUser(int id, UserDb db)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
        {
            return TypedResults.NotFound();
        }

        var userDto = new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsAdmin = user.IsAdmin,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        return TypedResults.Ok(userDto);
    }

    // Create new user with hashed password - never store plain text passwords
    public static async Task<IResult> CreateUser(User user, UserDb db)
    {
        try
        {
            // Check name and email is not already in the database
            if (db.Users.Any(x => x.Name == user.Name || x.Email == user.Email))
            {
                return TypedResults.BadRequest(new { message = "Name or email already exists" });
            }

            // Hash the password before saving - security best practice
            user.Password = PasswordHashService.HashPassword(user.Password);
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;

            db.Users.Add(user);
            await db.SaveChangesAsync();

            // Return user info without password
            var userResponse = new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsAdmin = user.IsAdmin,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return TypedResults.Created($"/users/{user.Id}", userResponse);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return TypedResults.BadRequest(new { message = "Error creating user" });
        }
    }
    public static async Task<IResult> UpdateUser(int id, User user, UserDb db)
    {
        var existingUser = await db.Users.FindAsync(id);
        if (existingUser is null) return TypedResults.NotFound();
        if (user.Name != null)
        {
            existingUser.Name = user.Name;
        }
        // cannot change email
        if (user.Email != null && user.Email != existingUser.Email)
        {
            return TypedResults.BadRequest(new { message = "Email cannot be changed" });
        }
        // cannot change isAdmin
        if (user.IsAdmin != existingUser.IsAdmin)
        {
            return TypedResults.BadRequest(new { message = "IsAdmin cannot be changed" });
        }
        // cannot change role
        if (user.Role != null && user.Role != existingUser.Role) // role is lowercase letter. role is string.
        {
            return TypedResults.BadRequest(new { message = "Role cannot be changed" });
        }
        existingUser.UpdatedAt = DateTime.Now;
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    public static async Task<IResult> ChangeUserRole(int id, User user, UserDb db)
    {
        var existingUser = await db.Users.FindAsync(id);
        if (existingUser is null) return TypedResults.NotFound();
        // check user name and email is already in the database, if not, return bad request
        if (db.Users.Any(x => x.Name == user.Name && x.Email == user.Email))
        {
            if (existingUser.Role == user.Role)
            {
                return TypedResults.BadRequest(new { message = "User role is already " + user.Role });
            }
            else
            {
                existingUser.Role = user.Role;
                existingUser.UpdatedAt = DateTime.Now;
            }
            await db.SaveChangesAsync();
            return TypedResults.Ok(new { message = "User role changed successfully" });
        }
        return TypedResults.BadRequest(new { message = "User name or email not found" });
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

        // Verify old password using BCrypt hash comparison
        if (!PasswordHashService.VerifyPassword(request.OldPassword, user.Password))
        {
            return TypedResults.UnprocessableEntity(new { message = "Old password is incorrect" });
        }

        // Hash new password and update
        user.Password = PasswordHashService.HashPassword(request.NewPassword);
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
            role = user.Role,
            createdAt = user.CreatedAt,
            updatedAt = user.UpdatedAt
        });
    }
}


