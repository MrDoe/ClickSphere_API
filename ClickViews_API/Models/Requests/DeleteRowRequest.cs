namespace ClickViews_API
{
    public class DeleteRowRequest
    {
        public required string Database { get; set; }
        public required string Table { get; set; }
        public required string Condition { get; set; }
    }
}