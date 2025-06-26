using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClickSphere_API.Models;
using ClickSphere_API.Models.Requests;
using ClickSphere_API.Services;

/// <summary>
/// Service to preload models in Ollama.
/// This service is used to ensure that the model is loaded into memory before it is needed
/// </summary>
public class ModelPreloadService : IHostedService
{
    private readonly IDbService DbService;
    private AiConfig Text2SQLConfig { get; set; } = default!;
    private AiConfig RAGConfig { get; set; } = default!;
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// Constructor for the ModelPreloadService
    /// </summary>
    public ModelPreloadService(IDbService dbService)
    {
        DbService = dbService;
        Text2SQLConfig = DbService.GetAiConfig("Text2SQLConfig");
        RAGConfig = DbService.GetAiConfig("RAGConfig");
    }

    /// <summary>
    /// Starts the service and preloads the models.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns></returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Load Text2SQL model
            if (Text2SQLConfig.OllamaModel != null && Text2SQLConfig.OllamaUrl != null)
            {
                Console.WriteLine($"Loading Text2SQL model: {Text2SQLConfig.OllamaModel}");
                await LoadModel(Text2SQLConfig.OllamaModel, Text2SQLConfig, false);
                Console.WriteLine($"Text2SQL model loaded successfully: {Text2SQLConfig.OllamaModel}");
            }
            else
            {
                throw new Exception("Text2SQL model or URL is not configured in the database!");
            }

            // Load RAG model
            if (RAGConfig.OllamaModel != null && RAGConfig.OllamaUrl != null)
            {
                Console.WriteLine($"Loading RAG model: {RAGConfig.OllamaModel}");
                await LoadModel(RAGConfig.OllamaModel, RAGConfig, true);
                Console.WriteLine($"RAG model loaded successfully: {RAGConfig.OllamaModel}");
            }
            else
            {
                throw new Exception("RAG model or URL is not configured in the database!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error preloading models: {ex.Message}");
            // Don't rethrow - let the application start even if model preloading fails
        }
    }

    /// <summary>
    /// Preload a specific model in Ollama
    /// </summary>
    /// <param name="model">The model to load.</param>
    /// <param name="aiConfig">The AI configuration containing the Ollama URL.</param>
    /// <param name="isEmbedding">Whether the model is for embeddings (default: false).</param>
    /// <exception cref="Exception">Thrown if the model cannot be loaded.</exception>
    /// <returns></returns>
    public async Task LoadModel(string model, AiConfig aiConfig, bool isEmbedding = false)
    {
        using HttpClientHandler handler = new()
        {
            UseProxy = false
        };

        using HttpClient client = new(handler)
        {
            BaseAddress = new Uri(aiConfig.OllamaUrl!),
            Timeout = TimeSpan.FromSeconds(120)
        };

        // Add an Accept header for JSON format
        var mediaType = new MediaTypeWithQualityHeaderValue("application/json");
        client.DefaultRequestHeaders.Accept.Add(mediaType);

        // Create the request object
        OllamaRequest request = new()
        {
            model = model
        };
        
        // Create the JSON request content
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(request, jsonOptions),
            Encoding.UTF8,
            mediaType);

        string apiPath = isEmbedding ? "/api/embed" : "/api/generate";
        
        Console.WriteLine($"Sending request to Ollama API: {aiConfig.OllamaUrl}{apiPath}");
        Console.WriteLine($"Request body: {jsonContent.ReadAsStringAsync().Result}");

        // Send a POST request to the Ollama API
        HttpResponseMessage response = await client.PostAsync(apiPath, jsonContent);

        Console.WriteLine($"Received response with status: {response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error loading model: {response.StatusCode} - {errorContent}");
        }
        else
            Console.WriteLine($"Model {model} loaded successfully.");
    }

    /// <summary>
    /// Stops the service gracefully.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns></returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}