namespace ClickViews_API.Models
{
    public class CreateViewRequest
    {
        public required string Database { get; set; }
        public required string View { get; set; }
        public required string Query { get; set; }
    }
}
