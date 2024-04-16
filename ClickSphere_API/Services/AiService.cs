using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
namespace ClickSphere_API.Services;
public class AiService
{
    public static async Task<string> Ask(string question)
    {
        using HttpClient client = new();
        client.BaseAddress = new Uri("http://localhost:11434");
        var mediaType = new MediaTypeWithQualityHeaderValue("application/json");

        // Add an Accept header for JSON format.
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(mediaType);

        string inject = "You are an expert for ClickHouse databases.\n" +
        "Source table definition: CREATE TABLE trips (trip_id UInt32, pickup_datetime DateTime, dropoff_datetime DateTime, pickup_longitude Nullable(Float64)," +
        "pickup_latitude Nullable(Float64), dropoff_longitude Nullable(Float64), dropoff_latitude Nullable(Float64), passenger_count UInt8," +
        "trip_distance Float32, fare_amount Float32, extra Float32, tip_amount Float32, tolls_amount Float32, total_amount Float32," +
        "payment_type Enum('CSH' = 1, 'CRE' = 2, 'NOC' = 3, 'DIS' = 4, 'UNK' = 5)," +
        "pickup_ntaname  LowCardinality(String)," +
        "dropoff_ntaname LowCardinality(String)) ENGINE = MergeTree PRIMARY KEY (pickup_datetime, dropoff_datetime); " +
        "Please write a SQL query for the following question:";

        // Create the JSON request content.
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(new { model = "codegemma", prompt = inject + question, stream = false }),
            Encoding.UTF8,
            mediaType);

        // Send a POST request to the Ollama API
        HttpResponseMessage response = await client.PostAsync("/api/generate", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            // Read the response as a string
            string responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }
        else
        {
            throw new Exception("Failed to call Ollama API");
        }
    }
}
