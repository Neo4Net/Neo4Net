using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.metrics
{

	using Description = Neo4Net.Configuration.Description;
	using LoadableConfig = Neo4Net.Configuration.LoadableConfig;
	using Neo4Net.Graphdb.config;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.BYTES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.DURATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.HOSTNAME_PORT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.INTEGER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.buildSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.pathSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.range;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.setting;

	/// <summary>
	/// Settings for the Neo4j Enterprise metrics reporting.
	/// </summary>
	[Description("Metrics settings")]
	public class MetricsSettings : LoadableConfig
	{
		 // Common settings
		 [Description("A common prefix for the reported metrics field names. By default, this is either be 'neo4j', " + "or a computed value based on the cluster and instance names, when running in an HA configuration.")]
		 public static readonly Setting<string> MetricsPrefix = setting( "metrics.prefix", STRING, "neo4j" );

		 // The below settings define what metrics to gather
		 // By default everything is on
		 [Description("The default enablement value for all the supported metrics. Set this to `false` to turn off all " + "metrics by default. The individual settings can then be used to selectively re-enable specific " + "metrics.")]
		 public static readonly Setting<bool> MetricsEnabled = setting( "metrics.enabled", BOOLEAN, TRUE );

		 [Description("The default enablement value for all Neo4j specific support metrics. Set this to `false` to turn " + "off all Neo4j specific metrics by default. The individual `metrics.neo4j.*` metrics can then be " + "turned on selectively.")]
		 public static readonly Setting<bool> NeoEnabled = buildSetting( "metrics.neo4j.enabled", BOOLEAN ).inherits( MetricsEnabled ).build();

		 [Description("Enable reporting metrics about transactions; number of transactions started, committed, etc.")]
		 public static readonly Setting<bool> NeoTxEnabled = buildSetting( "metrics.neo4j.tx.enabled", BOOLEAN ).inherits( NeoEnabled ).build();

		 [Description("Enable reporting metrics about the Neo4j page cache; page faults, evictions, flushes, exceptions, " + "etc.")]
		 public static readonly Setting<bool> NeoPageCacheEnabled = buildSetting( "metrics.neo4j.pagecache.enabled", BOOLEAN ).inherits( NeoEnabled ).build();

		 [Description("Enable reporting metrics about approximately how many entities are in the database; nodes, " + "relationships, properties, etc.")]
		 public static readonly Setting<bool> NeoCountsEnabled = buildSetting( "metrics.neo4j.counts.enabled", BOOLEAN ).inherits( NeoEnabled ).build();

		 [Description("Enable reporting metrics about the network usage.")]
		 public static readonly Setting<bool> NeoNetworkEnabled = buildSetting( "metrics.neo4j.network.enabled", BOOLEAN ).inherits( NeoEnabled ).build();

		 [Description("Enable reporting metrics about Causal Clustering mode.")]
		 public static readonly Setting<bool> CausalClusteringEnabled = buildSetting( "metrics.neo4j.causal_clustering.enabled", BOOLEAN ).inherits( NeoEnabled ).build();

		 [Description("Enable reporting metrics about Neo4j check pointing; when it occurs and how much time it takes to " + "complete.")]
		 public static readonly Setting<bool> NeoCheckPointingEnabled = buildSetting( "metrics.neo4j.checkpointing.enabled", BOOLEAN ).inherits( NeoEnabled ).build();

		 [Description("Enable reporting metrics about the Neo4j log rotation; when it occurs and how much time it takes to " + "complete.")]
		 public static readonly Setting<bool> NeoLogRotationEnabled = buildSetting( "metrics.neo4j.logrotation.enabled", BOOLEAN ).inherits( NeoEnabled ).build();

		 /// @deprecated high availability database/edition is deprecated in favour of causal clustering. It will be removed in next major release. 
		 [Description("Enable reporting metrics about HA cluster info."), Obsolete]
		 public static readonly Setting<bool> NeoClusterEnabled = buildSetting( "metrics.neo4j.cluster.enabled", BOOLEAN ).inherits( NeoEnabled ).build();

		 [Description("Enable reporting metrics about Server threading info.")]
		 public static readonly Setting<bool> NeoServerEnabled = buildSetting( "metrics.neo4j.server.enabled", BOOLEAN ).inherits( NeoEnabled ).build();

		 [Description("Enable reporting metrics about the duration of garbage collections")]
		 public static readonly Setting<bool> JvmGcEnabled = buildSetting( "metrics.jvm.gc.enabled", BOOLEAN ).inherits( NeoEnabled ).build();

		 [Description("Enable reporting metrics about the memory usage.")]
		 public static readonly Setting<bool> JvmMemoryEnabled = buildSetting( "metrics.jvm.memory.enabled", BOOLEAN ).inherits( NeoEnabled ).build();

		 [Description("Enable reporting metrics about the buffer pools.")]
		 public static readonly Setting<bool> JvmBuffersEnabled = buildSetting( "metrics.jvm.buffers.enabled", BOOLEAN ).inherits( NeoEnabled ).build();

		 [Description("Enable reporting metrics about the current number of threads running.")]
		 public static readonly Setting<bool> JvmThreadsEnabled = buildSetting( "metrics.jvm.threads.enabled", BOOLEAN ).inherits( NeoEnabled ).build();

		 [Description("Enable reporting metrics about number of occurred replanning events.")]
		 public static readonly Setting<bool> CypherPlanningEnabled = buildSetting( "metrics.cypher.replanning.enabled", BOOLEAN ).inherits( NeoEnabled ).build();

		 [Description("Enable reporting metrics about Bolt Protocol message processing.")]
		 public static readonly Setting<bool> BoltMessagesEnabled = buildSetting( "metrics.bolt.messages.enabled", BOOLEAN ).inherits( NeoEnabled ).build();

		 // CSV settings
		 [Description("Set to `true` to enable exporting metrics to CSV files")]
		 public static readonly Setting<bool> CsvEnabled = setting( "metrics.csv.enabled", BOOLEAN, TRUE );

		 [Description("The target location of the CSV files: a path to a directory wherein a CSV file per reported " + "field  will be written.")]
		 public static readonly Setting<File> CsvPath = pathSetting( "dbms.directories.metrics", "metrics" );

		 [Description("The reporting interval for the CSV files. That is, how often new rows with numbers are appended to " + "the CSV files.")]
		 public static readonly Setting<Duration> CsvInterval = setting( "metrics.csv.interval", DURATION, "3s" );

		 [Description("The file size in bytes at which the csv files will auto-rotate. If set to zero then no " + "rotation will occur. Accepts a binary suffix `k`, `m` or `g`.")]
		 public static readonly Setting<long> CsvRotationThreshold = buildSetting( "metrics.csv.rotation.size", BYTES, "10m" ).constraint( range( 0L, long.MaxValue ) ).build();

		 [Description("Maximum number of history files for the csv files.")]
		 public static readonly Setting<int> CsvMaxArchives = buildSetting( "metrics.csv.rotation.keep_number", INTEGER, "7" ).constraint( min( 1 ) ).build();

		 // Graphite settings
		 [Description("Set to `true` to enable exporting metrics to Graphite.")]
		 public static readonly Setting<bool> GraphiteEnabled = setting( "metrics.graphite.enabled", BOOLEAN, FALSE );

		 [Description("The hostname or IP address of the Graphite server")]
		 public static readonly Setting<HostnamePort> GraphiteServer = setting( "metrics.graphite.server", HOSTNAME_PORT, ":2003" );

		 [Description("The reporting interval for Graphite. That is, how often to send updated metrics to Graphite.")]
		 public static readonly Setting<Duration> GraphiteInterval = setting( "metrics.graphite.interval", DURATION, "3s" );

		 // Prometheus settings
		 [Description("Set to `true` to enable the Prometheus endpoint")]
		 public static readonly Setting<bool> PrometheusEnabled = setting( "metrics.prometheus.enabled", BOOLEAN, FALSE );

		 [Description("The hostname and port to use as Prometheus endpoint")]
		 public static readonly Setting<HostnamePort> PrometheusEndpoint = setting( "metrics.prometheus.endpoint", HOSTNAME_PORT, "localhost:2004" );
	}

}