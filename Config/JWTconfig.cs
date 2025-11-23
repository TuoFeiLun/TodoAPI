using System.Reflection.Metadata;

namespace MyApi.Config;
public class JwtSettings
{
    public const string SecretKey = "l2id!3p$-qu;3AP(@C(f-+.fz&}HUu:Vtr{R6CiASk:";
    public const string Issuer = "TodoAPI";
    public const string Audience = "TodoAPIUsers";
    public const int ExpirationMinutes = 60;
}
