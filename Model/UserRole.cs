using System.Text.Json.Serialization;
namespace MyApi.Model.User;


[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    Admin,
    Editor,
    Viewer
}