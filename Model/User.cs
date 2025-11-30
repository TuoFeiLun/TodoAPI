using System.ComponentModel.DataAnnotations;
namespace MyApi.Model.User;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsAdmin { get; set; } = false;

    // Role must be 'admin', 'editor', or 'viewer'. this method is triggered in ModelBinding when  [MapPost("/users", (User user)] using [FromBody] or [FromForm] attributes.
    // if you use jsonelement to bind the data by using patch data, you need to check the body property.

    //[AllowedValues(UserRole.Admin, UserRole.Editor, UserRole.Viewer, ErrorMessage = "Role must be 'admin', 'editor', or 'viewer'")]
    public UserRole Role { get; set; }

    // Parameterless constructor for EF Core
    public User() { }

    // Constructor for creating new users - use UTC time for PostgreSQL
    public User(string name, string password, string email, UserRole role, bool isAdmin = false)
    {
        Name = name;
        Email = email;
        Password = password;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsAdmin = isAdmin;
        Role = role;
    }
}

// DTO for returning user info without password
public class UserResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";

    public UserRole Role { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}