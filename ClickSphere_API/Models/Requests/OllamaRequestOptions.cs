namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents the options for an Ollama request.
/// </summary>
public class OllamaRequestOptions
{
    /// <summary>
    /// The number of generated responses to keep.
    /// </summary>
    public int? num_keep { get; set; }

    /// <summary>
    /// The random seed used for generation. Using the same seed will result in the same output for the same input.
    /// </summary>
    public int? seed { get; set; }

    /// <summary>
    /// The number of tokens to generate.
    /// </summary>
    public int? num_predict { get; set; }

    /// <summary>
    /// These parameters control the randomness of the generation. top_k limits the model to consider only the top k most likely next tokens.
    /// </summary>
    public int? top_k { get; set; }

    /// <summary>
    /// These parameters control the randomness of the generation. top_p makes the model consider just enough of the most likely next tokens that their cumulative probability exceeds p.
    /// </summary>
    public double? top_p { get; set; }

    /// <summary>
    /// The tfs z value. Controls the randomness of the generation.
    /// </summary>
    public double? tfs_z { get; set; }

    /// <summary>
    /// The typical p value. Controls the randomness of the generation.
    /// </summary>
    public double? typical_p { get; set; }

    /// <summary>
    /// The number of items to repeat last.
    /// </summary>
    public int? repeat_last_n { get; set; }

    /// <summary>
    /// The temperature value. Controls the randomness of the generation.
    /// </summary>
    public double? temperature { get; set; }

    /// <summary>
    /// The repeat penalty value. Controls the model’s tendency to repeat itself.
    /// </summary>
    public double? repeat_penalty { get; set; }

    /// <summary>
    /// The presence penalty value. Controls the model’s tendency to repeat itself.
    /// </summary>
    public double? presence_penalty { get; set; }

    /// <summary>
    /// The frequency penalty value. Controls the model’s tendency to repeat itself.
    /// </summary>
    public double? frequency_penalty { get; set; }

    /// <summary>
    /// The mirostat value. These parameter is related to a feature called Mirostat, which controls the model’s tendency to repeat itself.
    /// </summary>
    public int? mirostat { get; set; }

    /// <summary>
    /// The mirostat tau value. These parameter is related to a feature called Mirostat, which controls the model’s tendency to repeat itself.
    /// </summary>
    public double? mirostat_tau { get; set; }

    /// <summary>
    /// The mirostat eta value. These parameter is related to a feature called Mirostat, which controls the model’s tendency to repeat itself.
    /// </summary>
    public double? mirostat_eta { get; set; }

    /// <summary>
    /// If true, the model is discouraged from generating newlines.
    /// </summary>
    public bool? penalize_newline { get; set; }

    /// <summary>
    /// The model will stop generating when it produces one of these strings.
    /// </summary>
    public string[]? stop { get; set; }

    /// <summary>
    /// This option is related to the use of Non-Uniform Memory Access (NUMA) nodes. If true, the model will be placed on the NUMA node closest to the GPU it’s using.
    /// </summary>
    public bool? numa { get; set; }

    /// <summary>
    /// Number of tokens of context to use. The model will use the last num_ctx tokens as context.
    /// </summary>
    public int? num_ctx { get; set; }

    /// <summary>
    /// Batch size. The number of tokens to generate in parallel.
    /// </summary>
    public int? num_batch { get; set; }

    /// <summary>
    /// The number of GQAs to use. GQA stands for “Generative Query Answering”.
    /// </summary>
    public int? num_gqa { get; set; }

    /// <summary>
    /// The number of GPUs to use.
    /// </summary>
    public int? num_gpu { get; set; }

    /// <summary>
    /// The main GPU value. Determines which GPU to use as the main GPU.
    /// </summary>
    public int? main_gpu { get; set; }

    /// <summary>
    /// The low VRAM value. If true, the model will use less VRAM.
    /// </summary>
    public bool? low_vram { get; set; }

    /// <summary>
    /// The f16 KV value. If true, the model will use 16-bit floating point numbers.
    /// </summary>
    public bool? f16_kv { get; set; }

    /// <summary>
    /// The vocab only value. If true, the model will only use the vocabulary.
    /// </summary>
    public bool? vocab_only { get; set; }

    /// <summary>
    /// The use mmap value. If true, the model will use memory-mapped files.
    /// </summary>
    public bool? use_mmap { get; set; }

    /// <summary>
    /// The use mlock value. If true, the model will lock its memory in RAM.
    /// </summary>
    public bool? use_mlock { get; set; }

    /// <summary>
    /// The rope frequency base value. These options are related to a feature called ROPE, which adjusts the model’s behavior based on the frequency of the current token.
    /// </summary>
    public double? rope_frequency_base { get; set; }

    /// <summary>
    /// The rope frequency scale value. These options are related to a feature called ROPE, which adjusts the model’s behavior based on the frequency of the current token.
    /// </summary>
    public double? rope_frequency_scale { get; set; }

    /// <summary>
    /// The number of threads to use for generation.
    /// </summary>
    public int? num_thread { get; set; }
}
