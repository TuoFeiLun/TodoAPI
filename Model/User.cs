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
    public string Role { get; set; } = "";

    // Parameterless constructor for EF Core
    public User() { }

    // Constructor for creating new users - use UTC time for PostgreSQL
    public User(string name, string password, string email, string role, bool isAdmin = false)
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
    public string Role { get; set; } = "";
    public bool IsAdmin { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}