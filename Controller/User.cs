using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Http.HttpResults;
using MyApi.Model.User;
using MyApi.Database;
using System.Security.Claims;
using MyApi.Model.ChangePasswordRequest;
using MyApi.Services;
using System.Text.Json;

namespace MyApi.Controller;

public class UserController
{
    // Get all users without password information
    public static async Task<IResult> GetAllUsers(AppDbContext db)
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
    public static async Task<IResult> GetUser(int id, AppDbContext db)
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

    // Create new user with hashed password
    public static async Task<IResult> CreateUser(User user, AppDbContext db)
    {
        try
        {
            // Check name and email is not already in the database
            // You dont need to use Toupper (but still use ToUpper()), because have used  UserNameToUpperFilter to convert user name to uppercase
            if (db.Users.Any(x => x.Name.ToUpper() == user.Name.ToUpper() || x.Email == user.Email))
            {
                return TypedResults.BadRequest(new { message = "Name or email already exists" });
            }

            // checke password must be at least 6 characters long and contain at least one uppercase letter, one lowercase letter, and one number
            if (user.Password.Length < 6 || !user.Password.Any(char.IsUpper) || !user.Password.Any(char.IsLower) || !user.Password.Any(char.IsDigit))
            {
                return TypedResults.BadRequest(new { message = "Password must be at least 6 characters long and contain at least one uppercase letter, one lowercase letter, and one number" });
            }

            // Hash the password before saving
            user.Password = PasswordHashService.HashPassword(user.Password);
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

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
    public static async Task<IResult> UpdateUser(int id, JsonElement patchData, AppDbContext db)
    {
        var existingUser = await db.Users.FindAsync(id);
        if (existingUser is null) return TypedResults.NotFound();

        // Only update fields that are present in the request
        if (patchData.TryGetProperty("name", out JsonElement nameElement))
        {
            existingUser.Name = nameElement.GetString() ?? existingUser.Name;
        }

        // Cannot change password via this endpoint
        if (patchData.TryGetProperty("password", out _))
        {
            return TypedResults.BadRequest(new { message = "Password cannot be changed via this endpoint. Use /change-password instead." });
        }

        // Cannot change email
        if (patchData.TryGetProperty("email", out _))
        {
            return TypedResults.BadRequest(new { message = "Email cannot be changed" });
        }

        // Cannot change isAdmin
        if (patchData.TryGetProperty("isAdmin", out _))
        {
            return TypedResults.BadRequest(new { message = "IsAdmin cannot be changed via this endpoint" });
        }

        // Cannot change role via this endpoint
        if (patchData.TryGetProperty("role", out _))
        {
            return TypedResults.BadRequest(new { message = "Role cannot be changed via this endpoint. Use /role/{id} instead." });
        }

        existingUser.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return TypedResults.Ok(new UserResponseDto
        {
            Id = existingUser.Id,
            Name = existingUser.Name,
            Email = existingUser.Email,
            Role = existingUser.Role,
            IsAdmin = existingUser.IsAdmin,
            CreatedAt = existingUser.CreatedAt,
            UpdatedAt = existingUser.UpdatedAt
        });
    }

    public static async Task<IResult> ChangeUserRole(int id, JsonElement patchData, AppDbContext db)
    {
        var existingUser = await db.Users.FindAsync(id);
        if (existingUser is null) return TypedResults.NotFound();

        if (!patchData.TryGetProperty("role", out JsonElement roleElement))
        {
            return TypedResults.BadRequest(new { message = "Role field is required" });
        }

        var newRole = roleElement.GetString();
        if (string.IsNullOrEmpty(newRole))
        {
            return TypedResults.BadRequest(new { message = "Role cannot be empty" });
        }
        // Parse string to UserRole enum (case-insensitive)
        if (!Enum.TryParse<UserRole>(newRole, ignoreCase: true, out UserRole newRoleEnum))
        {
            return TypedResults.BadRequest(new
            {
                message = $"Invalid role: '{newRole}'",
                allowedValues = Enum.GetNames<UserRole>()  // ["Admin", "Editor", "Viewer"]
            });
        }

        if (existingUser.Role == newRoleEnum)
        {
            return TypedResults.BadRequest(new { message = $"User role is already {newRole}" });
        }

        existingUser.Role = newRoleEnum;
        existingUser.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return TypedResults.Ok(new UserResponseDto
        {
            Id = existingUser.Id,
            Name = existingUser.Name,
            Email = existingUser.Email,
            Role = existingUser.Role,
            IsAdmin = existingUser.IsAdmin,
            CreatedAt = existingUser.CreatedAt,
            UpdatedAt = existingUser.UpdatedAt
        });
    }

    public static async Task<IResult> DeleteUser(int id, AppDbContext db)
    {
        if (await db.Users.FindAsync(id) is User user)
        {
            db.Users.Remove(user);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }
        return TypedResults.NotFound();
    }


    // Change password using JWT authentication
    public static async Task<IResult> ChangePassword(HttpContext context, ChangePasswordRequest request, AppDbContext db)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return TypedResults.Unauthorized();
        }

        if (!int.TryParse(userIdClaim.Value, out int userId))
        {
            return TypedResults.BadRequest("Invalid user ID in token");
        }

        var user = await db.Users.FindAsync(userId);
        if (user is null)
        {
            return TypedResults.NotFound("User not found");
        }

        if (!PasswordHashService.VerifyPassword(request.OldPassword, user.Password))
        {
            return TypedResults.UnprocessableEntity(new { message = "Old password is incorrect" });
        }

        user.Password = PasswordHashService.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return TypedResults.Ok(new
        {
            message = "Password changed successfully",
            email = user.Email,
            updatedAt = user.UpdatedAt
        });
    }

    // Get current user info from JWT token
    public static async Task<IResult> GetCurrentUser(HttpContext context, AppDbContext db)
    {
        ClaimsPrincipal currentUser = context.User;

        // Method 1: Find by Id (using FindAsync) - get Id from JWT claims
        var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return TypedResults.Unauthorized();
        }
        var user = await db.Users.FindAsync(userId);

        // Method 2: Find by Email (using FirstOrDefaultAsync)
        // var email = currentUser.FindFirst(ClaimTypes.Email)?.Value;
        // var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Method 3: Find by Name (using FirstOrDefaultAsync)
        // var name = currentUser.Identity?.Name;
        // var user = await db.Users.FirstOrDefaultAsync(u => u.Name == name);

        if (user is null)
        {
            return TypedResults.NotFound("User not found");
        }

        return TypedResults.Ok(new
        {
            id = user.Id,
            name = user.Name,
            email = user.Email,
            role = user.Role,
            createdAt = user.CreatedAt,
            updatedAt = user.UpdatedAt,
            isAuthenticated = currentUser.Identity?.IsAuthenticated,
            authenticationType = currentUser.Identity?.AuthenticationType
        });
    }
}


