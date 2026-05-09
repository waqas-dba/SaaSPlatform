namespace SaaSPlatform.Core.IAM.Models;

public class LoginRequest
{
    public string Phone { get; set; } = default!;
    public string Password { get; set; } = default!;
}