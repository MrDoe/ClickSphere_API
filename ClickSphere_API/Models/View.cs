namespace ClickSphere_API.Models
{
    /// <summary>
    /// Represents a view in the application.
    /// </summary>
    public class View
    {
        /// <summary>
        /// Gets or sets the ID of the view.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the view.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the view.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the database associated with the view.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Gets or sets the query used to retrieve data for the view.
        /// </summary>
        public string Query { get; set; }

        public View()
        {
            Id = "";
            Name = "";
            Database = "";
            Query = "";
        }
    }
}
