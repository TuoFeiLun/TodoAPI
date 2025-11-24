namespace MyApi.Model.User;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } // PropertyNameCaseInsensitive
    public string Email { get; set; }
    public string Password { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsAdmin { get; set; }


    // Constructor with default parameters - if null/empty, use default values
    public User(string name, string password, string email, bool isAdmin = false)
    {
        Name = name;
        Email = email;
        Password = password;
        CreatedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
        IsAdmin = isAdmin;
    }
}