using ClickSphere_API.Models;
namespace ClickSphere_API;

public class MaterializedView : View
{
    public EngineType Engine { get; set; }
    public string? PartitionBy { get; set; }
    public string? OrderBy { get; set; }
    public bool? Populate { get; set; }
    public string? TargetTable { get; set; }

    public MaterializedView() : base()
    {
        Populate = true;
        Engine = EngineType.MergeTree;
        PartitionBy = null;
        TargetTable = null;
    }
}
