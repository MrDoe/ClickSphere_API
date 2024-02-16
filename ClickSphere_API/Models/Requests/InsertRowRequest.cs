namespace ClickSphere_API
{
    public class InsertRowRequest
    {
        public required string Database { get; set; }
        public required string Table { get; set; }
        public required string Values { get; set; }
    }
}