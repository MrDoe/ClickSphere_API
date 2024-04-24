namespace ClickSphere_API.Models;

/// <summary>
/// Represents the type of engine used in ClickSphere.
/// </summary>
public enum EngineType
{
    /// <summary>
    /// MergeTree engine.
    /// </summary>
    MergeTree,

    /// <summary>
    /// ReplacingMergeTree engine.
    /// </summary>
    ReplacingMergeTree,

    /// <summary>
    /// SummingMergeTree engine.
    /// </summary>
    SummingMergeTree,

    /// <summary>
    /// CollapsingMergeTree engine.
    /// </summary>
    CollapsingMergeTree,

    /// <summary>
    /// AggregatingMergeTree engine.
    /// </summary>
    AggregatingMergeTree,

    /// <summary>
    /// GraphiteMergeTree engine.
    /// </summary>
    GraphiteMergeTree,

    /// <summary>
    /// VersionedCollapsingMergeTree engine.
    /// </summary>
    VersionedCollapsingMergeTree,

    /// <summary>
    /// Log engine.
    /// </summary>
    Log,

    /// <summary>
    /// TinyLog engine.
    /// </summary>
    TinyLog,

    /// <summary>
    /// StripeLog engine.
    /// </summary>
    StripeLog,

    /// <summary>
    /// ODBC engine.
    /// </summary>
    ODBC,

    /// <summary>
    /// JDBC engine.
    /// </summary>
    JDBC,

    /// <summary>
    /// MySQL engine.
    /// </summary>
    MySQL,

    /// <summary>
    /// MongoDB engine.
    /// </summary>
    MongoDB,

    /// <summary>
    /// Redis engine.
    /// </summary>
    Redis,

    /// <summary>
    /// HDFS engine.
    /// </summary>
    HDFS,

    /// <summary>
    /// S3 engine.
    /// </summary>
    S3,

    /// <summary>
    /// Kafka engine.
    /// </summary>
    Kafka,

    /// <summary>
    /// RabbitMQ engine.
    /// </summary>
    RabbitMQ,

    /// <summary>
    /// EmbeddedRocksDB engine.
    /// </summary>
    EmbeddedRocksDB,

    /// <summary>
    /// PostgreSQL engine.
    /// </summary>
    PostgreSQL,

    /// <summary>
    /// S3Queue engine.
    /// </summary>
    S3Queue,

    /// <summary>
    /// Distributed engine.
    /// </summary>
    Distributed,

    /// <summary>
    /// Dictionary engine.
    /// </summary>
    Dictionary,

    /// <summary>
    /// Merge engine.
    /// </summary>
    Merge,

    /// <summary>
    /// File engine.
    /// </summary>
    File,

    /// <summary>
    /// Null engine.
    /// </summary>
    Null,

    /// <summary>
    /// Set engine.
    /// </summary>
    Set,

    /// <summary>
    /// Join engine.
    /// </summary>
    Join,

    /// <summary>
    /// URL engine.
    /// </summary>
    URL,

    /// <summary>
    /// View engine.
    /// </summary>
    View,

    /// <summary>
    /// Memory engine.
    /// </summary>
    Memory,

    /// <summary>
    /// Buffer engine.
    /// </summary>
    Buffer,

    /// <summary>
    /// KeeperMap engine.
    /// </summary>
    KeeperMap
}
