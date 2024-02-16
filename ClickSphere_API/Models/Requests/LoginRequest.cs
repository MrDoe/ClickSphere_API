namespace ClickSphere_API.Models
{
    public class LoginRequest
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}
