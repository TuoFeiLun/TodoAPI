namespace MyApi.Config;
public class JwtSettings
{
    public static string SecretKey = "l2id!3p$-qu;3AP(@C(f-+.fz&}HUu:Vtr{R6CiASk:";
    public static string Issuer = "TodoAPI";
    public static string Audience = "TodoAPIUsers";
    public static int ExpirationMinutes = 60;
}
