namespace ClickSphere_API.Models
{
    public class CreateViewRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Database { get; set; }
        public string Query { get; set; }
    }
}
