namespace ClickSphere_API.Models
{
    public class UserConfig
    {
        public required Guid UserId { get; set; }
        public required string UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? LDAP_User { get; set; }
    }
}