namespace ClickViews_API
{
    public class GetRowsRequest
    {
        public required string Database { get; set; }
        public required string Table { get; set; }
        public required string Columns { get; set; }
        public string? Limit { get; set; }
        public string? OrderBy { get; set; }
    }
}