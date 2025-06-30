namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents the options for an Ollama request.
/// </summary>
public class OllamaRequestOptions
{
    /// <summary>
    /// The number of generated responses to keep. Must be at least 1.
    /// Range: [1, ∞)
    /// </summary>
    public int? num_keep { get; set; }

    /// <summary>
    /// The random seed used for generation. Using the same seed and input yields the same output.
    /// Range: [0, 2^32−1]
    /// </summary>
    public int? seed { get; set; }

    /// <summary>
    /// The number of tokens to generate in the response.
    /// Range: [1, ∞)
    /// </summary>
    public int? num_predict { get; set; }

    /// <summary>
    /// Limits sampling to the top K most probable next tokens.
    /// Range: [0, ∞) (0 disables top-k filtering, i.e. no limit)
    /// </summary>
    public int? top_k { get; set; }

    /// <summary>
    /// Nucleus sampling threshold: consider the smallest set of tokens whose cumulative probability ≥ top_p.
    /// Range: [0.0, 1.0]
    /// </summary>
    public double? top_p { get; set; }

    /// <summary>
    /// Tail free sampling parameter. Controls how aggressively low-probability tails are pruned.
    /// Range: [0.0, 1.0]
    /// </summary>
    public double? tfs_z { get; set; }

    /// <summary>
    /// Typical sampling threshold. Filters tokens whose probability is below the typical_p-quantile of the distribution.
    /// Range: [0.0, 1.0]
    /// </summary>
    public double? typical_p { get; set; }

    /// <summary>
    /// Number of last tokens to apply repetition penalty to.
    /// Range: [0, ∞) (0 disables repetition penalty)
    /// </summary>
    public int? repeat_last_n { get; set; }

    /// <summary>
    /// Sampling temperature. Higher values produce more random outputs; lower values make output more deterministic.
    /// Range: (0.0, ∞)
    /// Typical: [0.1, 2.0]
    /// </summary>
    public double? temperature { get; set; }

    /// <summary>
    /// Penalty applied to tokens that have already appeared to discourage repetition.
    /// Range: (0.0, ∞)
    /// Typical: [1.0, 2.0]
    /// </summary>
    public double? repeat_penalty { get; set; }

    /// <summary>
    /// Penalty for introducing new tokens not seen in the context.
    /// Positive values discourage new tokens; negative values encourage them.
    /// Range: [-2.0, 2.0]
    /// </summary>
    public double? presence_penalty { get; set; }

    /// <summary>
    /// Penalty for repeated tokens based on their existing frequency in the context.
    /// Positive values penalize frequent tokens more heavily.
    /// Range: [-2.0, 2.0]
    /// </summary>
    public double? frequency_penalty { get; set; }

    /// <summary>
    /// The Mirostat algorithm version to use. 0 disables Mirostat; 1 or 2 select Mirostat 1.0 or 2.0 respectively.
    /// Range: [0, 2]
    /// </summary>
    public int? mirostat { get; set; }

    /// <summary>
    /// Target entropy (perplexity) for Mirostat. Lower values yield more focused output; higher values increase diversity.
    /// Range: (0.0, ∞)
    /// </summary>
    public double? mirostat_tau { get; set; }

    /// <summary>
    /// Learning rate for the Mirostat algorithm. Controls how quickly the model adjusts to reach the target entropy.
    /// Range: (0.0, 1.0]
    /// </summary>
    public double? mirostat_eta { get; set; }

    /// <summary>
    /// If true, discourages the model from emitting newline characters.
    /// </summary>
    public bool? penalize_newline { get; set; }

    /// <summary>
    /// List of stop sequences. Generation halts when any of these strings is produced.
    /// </summary>
    public string[]? stop { get; set; }

    /// <summary>
    /// If true, places the model on the NUMA node closest to the GPU in use.
    /// </summary>
    public bool? numa { get; set; }

    /// <summary>
    /// Maximum number of context tokens to keep. The model uses the last num_ctx tokens as context.
    /// Range: [1, ∞)
    /// </summary>
    public int? num_ctx { get; set; }

    /// <summary>
    /// Batch size for parallel token generation.
    /// Range: [1, ∞)
    /// </summary>
    public int? num_batch { get; set; }

    /// <summary>
    /// Number of GQA (grouped query attention) units to use.
    /// Range: [1, ∞)
    /// </summary>
    public int? num_gqa { get; set; }

    /// <summary>
    /// Number of GPUs to utilize for inference.
    /// Range: [1, ∞)
    /// </summary>
    public int? num_gpu { get; set; }

    /// <summary>
    /// Index of the primary GPU to use (0-based).
    /// Range: [0, ∞)
    /// </summary>
    public int? main_gpu { get; set; }

    /// <summary>
    /// If true, enables low-VRAM mode to reduce memory footprint.
    /// </summary>
    public bool? low_vram { get; set; }

    /// <summary>
    /// If true, uses 16-bit floating point (fp16) for key/value caches.
    /// </summary>
    public bool? f16_kv { get; set; }

    /// <summary>
    /// If true, loads only the vocabulary (no model weights).
    /// </summary>
    public bool? vocab_only { get; set; }

    /// <summary>
    /// If true, uses memory-mapped files (mmap) for model weights.
    /// </summary>
    public bool? use_mmap { get; set; }

    /// <summary>
    /// If true, locks model memory into RAM (mlock).
    /// </summary>
    public bool? use_mlock { get; set; }

    /// <summary>
    /// Base frequency parameter for ROPE (rotary positional embeddings).
    /// Range: [0.0, ∞)
    /// </summary>
    public double? rope_frequency_base { get; set; }

    /// <summary>
    /// Scale factor for ROPE frequency adjustments.
    /// Range: [0.0, ∞)
    /// </summary>
    public double? rope_frequency_scale { get; set; }

    /// <summary>
    /// Number of CPU threads to use for generation.
    /// Range: [1, ∞)
    /// </summary>
    public int? num_thread { get; set; }
}
