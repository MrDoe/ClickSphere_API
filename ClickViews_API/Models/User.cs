namespace ClickViews_API.Models
{
    public class User
    {
        public required Guid UserId { get; set; }
        public required string UserName { get; set; }
        public string? Password { get; set; }
    }
}