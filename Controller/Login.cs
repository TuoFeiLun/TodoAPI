using Microsoft.EntityFrameworkCore;
using MyApi.Model.User;
using MyApi.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MyApi.Config;
using System.Text;
using MyApi.Services;
namespace MyApi.Controller;

public class LoginController
{


    // Helper function to generate JWT token
    private static string GenerateJwtToken(IEnumerable<Claim> claims)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(JwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: JwtSettings.Issuer,
            audience: JwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(JwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    // Login method - authenticate user with hashed password verification
    public static async Task<IResult> Login(User user, AppDbContext db)
    {
        var existingUser = await db.Users
            .Where(u => u.Email == user.Email)
            .FirstOrDefaultAsync();

        // Verify password using BCrypt hash comparison
        if (existingUser is not null && PasswordHashService.VerifyPassword(user.Password, existingUser.Password))
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, existingUser.Name),
                new Claim(ClaimTypes.Email, existingUser.Email ?? ""),
                new Claim(ClaimTypes.NameIdentifier, existingUser.Id.ToString()),
                new Claim(ClaimTypes.Role, existingUser.Role),
                new Claim("scope", "view_user"),
                new Claim("scope", "view_todoitem")
            };
            if (existingUser.Role == "admin")
            {
                claims.Add(new Claim("scope", "create_and_delete_user"));
                claims.Add(new Claim("scope", "update_user"));
                claims.Add(new Claim("scope", "change_user_role"));

            }
            else if (existingUser.Role == "editor")
            {
                claims.Add(new Claim("scope", "update_user"));
                claims.Add(new Claim("scope", "crud_todoitem"));
            }

            var token = GenerateJwtToken(claims);
            return Results.Ok(new
            {
                tokenType = "Bearer",
                token = token,
                name = existingUser.Name,
                email = existingUser.Email,
                isAdmin = existingUser.IsAdmin
            });
        }
        return Results.Unauthorized();
    }
}