namespace CoreKit.IAM.Models
{
    public class LogoutRequest
    {
        public string RefreshToken { get; set; } = default!;
    }
}