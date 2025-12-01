using System.Text.Json.Serialization;
namespace MyApi.Model.User;

/// <summary>
/// User roles for authorization. Use JsonNamingPolicy.CamelCase for case-insensitive matching.
/// Accepts: "admin", "Admin", "editor", "Editor", "viewer", "Viewer"
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<UserRole>))]
public enum UserRole
{
    Admin,
    Editor,
    Viewer
}