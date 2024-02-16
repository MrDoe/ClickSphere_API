namespace ClickSphere_API
{
    public class CreateTableRequest
    {
        public required string Database { get; set; }
        public required string Table { get; set; }
        public required string Columns { get; set; }
        public string? Engine { get; set; }
        public string? OrderBy { get; set; }
    }
}