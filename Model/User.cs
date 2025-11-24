namespace MyApi.Model.User;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }  // PropertyNameCaseInsensitive
    public string Email { get; set; }
    public string Password { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsAdmin { get; set; } = false;
    public string Role { get; set; }


    // Constructor for creating new users
    public User(string name, string password, string email, string role, bool isAdmin = false)
    {
        Name = name;
        Email = email;
        Password = password;
        CreatedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
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