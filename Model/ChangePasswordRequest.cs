namespace MyApi.Model.ChangePasswordRequest;
// DTO for change password request
public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}