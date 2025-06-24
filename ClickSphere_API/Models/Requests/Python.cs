using System.Text.Json.Serialization;

// --- JSON Models for API Calls ---
public class OllamaEmbeddingRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; }
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; }
}

public class OllamaEmbeddingResponse
{
    [JsonPropertyName("embedding")]
    public float[] Embedding { get; set; }
}

public class PythonEmbeddingRequest
{
    [JsonPropertyName("sentences")]
    public List<string> Sentences { get; set; }
}

public class PythonEmbeddingResponse
{
    [JsonPropertyName("dense_embeddings")]
    public List<float[]> DenseEmbeddings { get; set; }
    [JsonPropertyName("sparse_embeddings")]
    public List<SparseVectorData> SparseEmbeddings { get; set; }
}

public class SparseVectorData
{
    [JsonPropertyName("indices")]
    public List<int> Indices { get; set; }
    [JsonPropertyName("values")]
    public List<float> Values { get; set; }
}
