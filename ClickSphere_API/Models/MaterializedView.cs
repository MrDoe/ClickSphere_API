namespace ClickSphere_API.Models;

/// <summary>
/// Represents a materialized view in the ClickSphere API.
/// </summary>
public class MaterializedView : View
{
    /// <summary>
    /// Gets or sets the engine type used for the materialized view.
    /// </summary>
    public EngineType Engine { get; set; }

    /// <summary>
    /// Gets or sets the column used for partitioning the materialized view.
    /// </summary>
    public string? PartitionBy { get; set; }

    /// <summary>
    /// Gets or sets the column used for ordering the materialized view.
    /// </summary>
    public string? OrderBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the materialized view should be populated.
    /// </summary>
    public bool? Populate { get; set; }

    /// <summary>
    /// Gets or sets the target table for the materialized view.
    /// </summary>
    public string? TargetTable { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterializedView"/> class.
    /// </summary>
    public MaterializedView() : base()
    {
        Populate = true;
        Engine = EngineType.MergeTree;
        PartitionBy = null;
        TargetTable = null;
    }
}
