namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents a document.
/// </summary>
public class Document
{
    /// <summary>
    /// Gets or sets the id of the document.
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the document.
    /// </summary>
    public string? Filename { get; set; }
    /// <summary>
    /// Gets or sets the content of the document.
    /// </summary>
    public string? Content { get; set; }
    public float[]? DenseEmbedding { get; set; }
    public List<int>? SparseIndices { get; set; }
    public List<float>? SparseValues { get; set; }

    // For search results
    public double DenseScore { get; set; }
    public double SparseScore { get; set; }
    public double HybridScore { get; set; }
}