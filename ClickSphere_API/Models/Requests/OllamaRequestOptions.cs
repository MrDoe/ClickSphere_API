namespace ClickSphere_API.Models.Requests;
/// <summary>
/// Represents the options for an Ollama request.
/// </summary>
public class OllamaRequestOptions
{
    /// <summary>
    /// Gets or sets the number of items to keep.
    /// </summary>
    public int? num_keep { get; set; }

    /// <summary>
    /// Gets or sets the seed value.
    /// </summary>
    public int? seed { get; set; }

    /// <summary>
    /// Gets or sets the number of predictions.
    /// </summary>
    public int? num_predict { get; set; }

    /// <summary>
    /// Gets or sets the top k value.
    /// </summary>
    public int? top_k { get; set; }

    /// <summary>
    /// Gets or sets the top p value.
    /// </summary>
    public double? top_p { get; set; }

    /// <summary>
    /// Gets or sets the tfs z value.
    /// </summary>
    public double? tfs_z { get; set; }

    /// <summary>
    /// Gets or sets the typical p value.
    /// </summary>
    public double? typical_p { get; set; }

    /// <summary>
    /// Gets or sets the number of items to repeat last.
    /// </summary>
    public int? repeat_last_n { get; set; }

    /// <summary>
    /// Gets or sets the temperature value.
    /// </summary>
    public double? temperature { get; set; }

    /// <summary>
    /// Gets or sets the repeat penalty value.
    /// </summary>
    public double? repeat_penalty { get; set; }

    /// <summary>
    /// Gets or sets the presence penalty value.
    /// </summary>
    public double? presence_penalty { get; set; }

    /// <summary>
    /// Gets or sets the frequency penalty value.
    /// </summary>
    public double? frequency_penalty { get; set; }

    /// <summary>
    /// Gets or sets the mirostat value.
    /// </summary>
    public int? mirostat { get; set; }

    /// <summary>
    /// Gets or sets the mirostat tau value.
    /// </summary>
    public double? mirostat_tau { get; set; }

    /// <summary>
    /// Gets or sets the mirostat eta value.
    /// </summary>
    public double? mirostat_eta { get; set; }

    /// <summary>
    /// Gets or sets the penalize newline value.
    /// </summary>
    public bool? penalize_newline { get; set; }

    /// <summary>
    /// Gets or sets the stop words.
    /// </summary>
    public string[]? stop { get; set; }

    /// <summary>
    /// Gets or sets the numa value.
    /// </summary>
    public bool? numa { get; set; }

    /// <summary>
    /// Gets or sets the number of contexts.
    /// </summary>
    public int? num_ctx { get; set; }

    /// <summary>
    /// Gets or sets the number of batches.
    /// </summary>
    public int? num_batch { get; set; }

    /// <summary>
    /// Gets or sets the number of GQAs.
    /// </summary>
    public int? num_gqa { get; set; }

    /// <summary>
    /// Gets or sets the number of GPUs.
    /// </summary>
    public int? num_gpu { get; set; }

    /// <summary>
    /// Gets or sets the main GPU value.
    /// </summary>
    public int? main_gpu { get; set; }

    /// <summary>
    /// Gets or sets the low VRAM value.
    /// </summary>
    public bool? low_vram { get; set; }

    /// <summary>
    /// Gets or sets the f16 KV value.
    /// </summary>
    public bool? f16_kv { get; set; }

    /// <summary>
    /// Gets or sets the vocab only value.
    /// </summary>
    public bool? vocab_only { get; set; }

    /// <summary>
    /// Gets or sets the use mmap value.
    /// </summary>
    public bool? use_mmap { get; set; }

    /// <summary>
    /// Gets or sets the use mlock value.
    /// </summary>
    public bool? use_mlock { get; set; }

    /// <summary>
    /// Gets or sets the rope frequency base value.
    /// </summary>
    public double? rope_frequency_base { get; set; }

    /// <summary>
    /// Gets or sets the rope frequency scale value.
    /// </summary>
    public double? rope_frequency_scale { get; set; }

    /// <summary>
    /// Gets or sets the number of threads.
    /// </summary>
    public int? num_thread { get; set; }
}
