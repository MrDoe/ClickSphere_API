namespace ClickSphere_API.Models
{
    public class CreatUserRequest
    {
        public required Guid UserId { get; set; }
        public required string UserName { get; set; }
        public string? Password { get; set; }
    }
}