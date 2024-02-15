namespace ClickViews_API.Models
{
    public class CreateViewRequest
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; }
        public required string Database { get; set; }
        public required string Query { get; set; }
    }
}
