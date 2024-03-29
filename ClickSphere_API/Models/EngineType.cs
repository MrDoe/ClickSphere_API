﻿namespace ClickSphere_API;

public enum EngineType
{
    MergeTree,
    ReplacingMergeTree,
    SummingMergeTree,
    CollapsingMergeTree,
    AggregatingMergeTree,
    GraphiteMergeTree,
    VersionedCollapsingMergeTree,
    Log,
    TinyLog,
    StripeLog,
    ODBC,
    JDBC,
    MySQL,
    MongoDB,
    Redis,
    HDFS,
    S3,
    Kafka,
    RabbitMQ,
    EmbeddedRocksDB,
    PostgreSQL,
    S3Queue,
    Distributed,
    Dictionary,
    Merge,
    File,
    Null,
    Set,
    Join,
    URL,
    View,
    Memory,
    Buffer,
    KeeperMap
}
