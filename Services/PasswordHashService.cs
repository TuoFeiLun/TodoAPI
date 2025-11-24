namespace MyApi.Services;

// Password hashing service using BCrypt for secure password storage
public class PasswordHashService
{
    // Hash a plain text password
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    // Verify a plain text password against a hashed password
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}

