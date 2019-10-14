using System;
using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.core
{

	using InFlightCacheFactory = Neo4Net.causalclustering.core.consensus.log.cache.InFlightCacheFactory;
	using Neo4Net.causalclustering.discovery;
	using Description = Neo4Net.Configuration.Description;
	using Internal = Neo4Net.Configuration.Internal;
	using LoadableConfig = Neo4Net.Configuration.LoadableConfig;
	using ReplacedBy = Neo4Net.Configuration.ReplacedBy;
	using Neo4Net.Graphdb.config;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocols.Implementations.GZIP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocols.Implementations.LZ4;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocols.Implementations.LZ4_HIGH_COMPRESSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocols.Implementations.LZ4_HIGH_COMPRESSION_VALIDATING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocols.Implementations.LZ_VALIDATING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocols.Implementations.SNAPPY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocols.Implementations.SNAPPY_VALIDATING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.logs_directory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.ADVERTISED_SOCKET_ADDRESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.BYTES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.DOUBLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.DURATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.INTEGER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.NO_DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.PATH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.STRING_LIST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.advertisedAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.buildSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.derivedSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.list;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.listenAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.optionsIgnoreCase;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.prefixSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.setting;

	[Description("Settings for Causal Clustering")]
	public class CausalClusteringSettings : LoadableConfig
	{
		 [Description("Time out for a new member to catch up")]
		 public static readonly Setting<Duration> JoinCatchUpTimeout = setting( "causal_clustering.join_catch_up_timeout", DURATION, "10m" );

		 [Description("The time limit within which a new leader election will occur if no messages are received.")]
		 public static readonly Setting<Duration> LeaderElectionTimeout = setting( "causal_clustering.leader_election_timeout", DURATION, "7s" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Configures the time after which we give up trying to bind to a cluster formed of the other initial discovery members.") public static final org.neo4j.graphdb.config.Setting<java.time.Duration> cluster_binding_timeout = setting("causal_clustering.cluster_binding_timeout", DURATION, "5m");
		 [Description("Configures the time after which we give up trying to bind to a cluster formed of the other initial discovery members.")]
		 public static readonly Setting<Duration> ClusterBindingTimeout = setting( "causal_clustering.cluster_binding_timeout", DURATION, "5m" );

		 [Description("Prevents the current instance from volunteering to become Raft leader. Defaults to false, and " + "should only be used in exceptional circumstances by expert users. Using this can result in reduced " + "availability for the cluster.")]
		 public static readonly Setting<bool> RefuseToBeLeader = setting( "causal_clustering.refuse_to_be_leader", BOOLEAN, FALSE );

		 [Description("The name of the database being hosted by this server instance. This configuration setting may be safely ignored " + "unless deploying a multicluster. Instances may be allocated to distinct sub-clusters by assigning them distinct database " + "names using this setting. For instance if you had 6 instances you could form 2 sub-clusters by assigning half " + "the database name \"foo\", half the name \"bar\". The setting value must match exactly between members of the same sub-cluster. " + "This setting is a one-off: once an instance is configured with a database name it may not be changed in future without using " + "neo4j-admin unbind.")]
		 public static readonly Setting<string> Database = setting( "causal_clustering.database", STRING, "default" );

		 [Description("Enable pre-voting extension to the Raft protocol (this is breaking and must match between the core cluster members)")]
		 public static readonly Setting<bool> EnablePreVoting = setting( "causal_clustering.enable_pre_voting", BOOLEAN, FALSE );

		 [Description("The maximum batch size when catching up (in unit of entries)")]
		 public static readonly Setting<int> CatchupBatchSize = setting( "causal_clustering.catchup_batch_size", INTEGER, "64" );

		 [Description("The maximum lag allowed before log shipping pauses (in unit of entries)")]
		 public static readonly Setting<int> LogShippingMaxLag = setting( "causal_clustering.log_shipping_max_lag", INTEGER, "256" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Maximum number of entries in the RAFT in-queue") public static final org.neo4j.graphdb.config.Setting<int> raft_in_queue_size = setting("causal_clustering.raft_in_queue_size", INTEGER, "1024");
		 [Description("Maximum number of entries in the RAFT in-queue")]
		 public static readonly Setting<int> RaftInQueueSize = setting( "causal_clustering.raft_in_queue_size", INTEGER, "1024" );

		 [Description("Maximum number of bytes in the RAFT in-queue")]
		 public static readonly Setting<long> RaftInQueueMaxBytes = setting( "causal_clustering.raft_in_queue_max_bytes", BYTES, "2G" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Largest batch processed by RAFT in number of entries") public static final org.neo4j.graphdb.config.Setting<int> raft_in_queue_max_batch = setting("causal_clustering.raft_in_queue_max_batch", INTEGER, "128");
		 [Description("Largest batch processed by RAFT in number of entries")]
		 public static readonly Setting<int> RaftInQueueMaxBatch = setting( "causal_clustering.raft_in_queue_max_batch", INTEGER, "128" );

		 [Description("Largest batch processed by RAFT in bytes")]
		 public static readonly Setting<long> RaftInQueueMaxBatchBytes = setting( "causal_clustering.raft_in_queue_max_batch_bytes", BYTES, "8M" );

		 [Description("Expected number of Core machines in the cluster before startup"), Obsolete, ReplacedBy("causal_clustering.minimum_core_cluster_size_at_formation and causal_clustering.minimum_core_cluster_size_at_runtime")]
		 public static readonly Setting<int> ExpectedCoreClusterSize = setting( "causal_clustering.expected_core_cluster_size", INTEGER, "3" );

		 [Description("Minimum number of Core machines in the cluster at formation. The expected_core_cluster size setting is used when bootstrapping the " + "cluster on first formation. A cluster will not form without the configured amount of cores and this should in general be configured to the" + " full and fixed amount. When using multi-clustering (configuring multiple distinct database names across core hosts), this setting is used " + "to define the minimum size of *each* sub-cluster at formation.")]
		 public static readonly Setting<int> MinimumCoreClusterSizeAtFormation = buildSetting( "causal_clustering.minimum_core_cluster_size_at_formation", INTEGER, ExpectedCoreClusterSize.DefaultValue ).constraint( min( 2 ) ).build();

		 [Description("Minimum number of Core machines required to be available at runtime. The consensus group size (core machines successfully voted into the " + "Raft) can shrink and grow dynamically but bounded on the lower end at this number. The intention is in almost all cases for users to leave this " + "setting alone. If you have 5 machines then you can survive failures down to 3 remaining, e.g. with 2 dead members. The three remaining can " + "still vote another replacement member in successfully up to a total of 6 (2 of which are still dead) and then after this, one of the " + "superfluous dead members will be immediately and automatically voted out (so you are left with 5 members in the consensus group, 1 of which " + "is currently dead). Operationally you can now bring the last machine up by bringing in another replacement or repairing the dead one. " + "When using multi-clustering (configuring multiple distinct database names across core hosts), this setting is used to define the minimum size " + "of *each* sub-cluster at runtime.")]
		 public static readonly Setting<int> MinimumCoreClusterSizeAtRuntime = buildSetting( "causal_clustering.minimum_core_cluster_size_at_runtime", INTEGER, "3" ).constraint( min( 2 ) ).build();

		 [Description("Network interface and port for the transaction shipping server to listen on. Please note that it is also possible to run the backup " + "client against this port so always limit access to it via the firewall and configure an ssl policy.")]
		 public static readonly Setting<ListenSocketAddress> TransactionListenAddress = listenAddress( "causal_clustering.transaction_listen_address", 6000 );

		 [Description("Advertised hostname/IP address and port for the transaction shipping server.")]
		 public static readonly Setting<AdvertisedSocketAddress> TransactionAdvertisedAddress = advertisedAddress( "causal_clustering.transaction_advertised_address", TransactionListenAddress );

		 [Description("Network interface and port for the RAFT server to listen on.")]
		 public static readonly Setting<ListenSocketAddress> RaftListenAddress = listenAddress( "causal_clustering.raft_listen_address", 7000 );

		 [Description("Advertised hostname/IP address and port for the RAFT server.")]
		 public static readonly Setting<AdvertisedSocketAddress> RaftAdvertisedAddress = advertisedAddress( "causal_clustering.raft_advertised_address", RaftListenAddress );

		 [Description("Host and port to bind the cluster member discovery management communication.")]
		 public static readonly Setting<ListenSocketAddress> DiscoveryListenAddress = listenAddress( "causal_clustering.discovery_listen_address", 5000 );

		 [Description("Advertised cluster member discovery management communication.")]
		 public static readonly Setting<AdvertisedSocketAddress> DiscoveryAdvertisedAddress = advertisedAddress( "causal_clustering.discovery_advertised_address", DiscoveryListenAddress );

		 [Description("A comma-separated list of other members of the cluster to join.")]
		 public static readonly Setting<IList<AdvertisedSocketAddress>> InitialDiscoveryMembers = setting( "causal_clustering.initial_discovery_members", list( ",", ADVERTISED_SOCKET_ADDRESS ), NO_DEFAULT );

		 [Description("Type of in-flight cache.")]
		 public static readonly Setting<InFlightCacheFactory.Type> InFlightCacheType = setting( "causal_clustering.in_flight_cache.type", optionsIgnoreCase( typeof( InFlightCacheFactory.Type ) ), InFlightCacheFactory.Type.CONSECUTIVE.name() );

		 [Description("The maximum number of entries in the in-flight cache.")]
		 public static readonly Setting<int> InFlightCacheMaxEntries = setting( "causal_clustering.in_flight_cache.max_entries", INTEGER, "1024" );

		 [Description("The maximum number of bytes in the in-flight cache.")]
		 public static readonly Setting<long> InFlightCacheMaxBytes = setting( "causal_clustering.in_flight_cache.max_bytes", BYTES, "2G" );

		 [Description("Address for Kubernetes API")]
		 public static readonly Setting<AdvertisedSocketAddress> KubernetesAddress = setting( "causal_clustering.kubernetes.address", ADVERTISED_SOCKET_ADDRESS, "kubernetes.default.svc:443" );

		 [Description("File location of token for Kubernetes API")]
		 public static readonly Setting<File> KubernetesToken = PathUnixAbsolute( "causal_clustering.kubernetes.token", "/var/run/secrets/kubernetes.io/serviceaccount/token" );

		 [Description("File location of namespace for Kubernetes API")]
		 public static readonly Setting<File> KubernetesNamespace = PathUnixAbsolute( "causal_clustering.kubernetes.namespace", "/var/run/secrets/kubernetes.io/serviceaccount/namespace" );

		 [Description("File location of CA certificate for Kubernetes API")]
		 public static readonly Setting<File> KubernetesCaCrt = PathUnixAbsolute( "causal_clustering.kubernetes.ca_crt", "/var/run/secrets/kubernetes.io/serviceaccount/ca.crt" );

		 /// <summary>
		 /// Creates absolute path on the first filesystem root. This will be `/` on Unix but arbitrary on Windows.
		 /// If filesystem roots cannot be listed then `//` will be used - this will be resolved to `/` on Unix and `\\` (a UNC network path) on Windows.
		 /// An absolute path is always needed for validation, even though we only care about a path on Linux.
		 /// </summary>
		 private static Setting<File> PathUnixAbsolute( string name, string path )
		 {
			  File[] roots = File.listRoots();
			  Path root = roots.Length > 0 ? roots[0].toPath() : Paths.get("//");
			  return setting( name, PATH, root.resolve( path ).ToString() );
		 }

		 [Description("LabelSelector for Kubernetes API")]
		 public static readonly Setting<string> KubernetesLabelSelector = setting( "causal_clustering.kubernetes.label_selector", STRING, NO_DEFAULT );

		 [Description("Service port name for discovery for Kubernetes API")]
		 public static readonly Setting<string> KubernetesServicePortName = setting( "causal_clustering.kubernetes.service_port_name", STRING, NO_DEFAULT );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("The polling interval when attempting to resolve initial_discovery_members from DNS and SRV records.") public static final org.neo4j.graphdb.config.Setting<java.time.Duration> discovery_resolution_retry_interval = setting("causal_clustering.discovery_resolution_retry_interval", DURATION, "5s");
		 [Description("The polling interval when attempting to resolve initial_discovery_members from DNS and SRV records.")]
		 public static readonly Setting<Duration> DiscoveryResolutionRetryInterval = setting( "causal_clustering.discovery_resolution_retry_interval", DURATION, "5s" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Configures the time after which we give up trying to resolve a DNS/SRV record into a list of initial discovery members.") public static final org.neo4j.graphdb.config.Setting<java.time.Duration> discovery_resolution_timeout = setting("causal_clustering.discovery_resolution_timeout", DURATION, "5m");
		 [Description("Configures the time after which we give up trying to resolve a DNS/SRV record into a list of initial discovery members.")]
		 public static readonly Setting<Duration> DiscoveryResolutionTimeout = setting( "causal_clustering.discovery_resolution_timeout", DURATION, "5m" );

		 [Description("Configure the discovery type used for cluster name resolution")]
		 public static readonly Setting<DiscoveryType> DiscoveryType = setting( "causal_clustering.discovery_type", optionsIgnoreCase( typeof( DiscoveryType ) ), DiscoveryType.List.name() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Select the middleware used for cluster topology discovery") public static final org.neo4j.graphdb.config.Setting<org.neo4j.causalclustering.discovery.DiscoveryServiceFactorySelector.DiscoveryImplementation> discovery_implementation = setting("causal_clustering.discovery_implementation", optionsIgnoreCase(org.neo4j.causalclustering.discovery.DiscoveryServiceFactorySelector.DiscoveryImplementation.class), org.neo4j.causalclustering.discovery.DiscoveryServiceFactorySelector.DEFAULT.name());
		 [Description("Select the middleware used for cluster topology discovery")]
		 public static readonly Setting<DiscoveryServiceFactorySelector.DiscoveryImplementation> DiscoveryImplementation = setting( "causal_clustering.discovery_implementation", optionsIgnoreCase( typeof( DiscoveryServiceFactorySelector.DiscoveryImplementation ) ), DiscoveryServiceFactorySelector.DEFAULT.name() );

		 [Description("Prevents the network middleware from dumping its own logs. Defaults to true.")]
		 public static readonly Setting<bool> DisableMiddlewareLogging = setting( "causal_clustering.disable_middleware_logging", BOOLEAN, TRUE );

		 [Description("The level of middleware logging")]
		 public static readonly Setting<int> MiddlewareLoggingLevel = setting( "causal_clustering.middleware_logging.level", INTEGER, Convert.ToString( Level.FINE.intValue() ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Hazelcast license key") public static final org.neo4j.graphdb.config.Setting<String> hazelcast_license_key = setting("hazelcast.license_key", STRING, NO_DEFAULT);
		 [Description("Hazelcast license key")]
		 public static readonly Setting<string> HazelcastLicenseKey = setting( "hazelcast.license_key", STRING, NO_DEFAULT );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Parallelism level of default dispatcher used by Akka based cluster topology discovery, including cluster, replicator, and discovery actors") public static final org.neo4j.graphdb.config.Setting<int> middleware_akka_default_parallelism_level = setting("causal_clustering.middleware.akka.default-parallelism", INTEGER, System.Convert.ToString(4));
		 [Description("Parallelism level of default dispatcher used by Akka based cluster topology discovery, including cluster, replicator, and discovery actors")]
		 public static readonly Setting<int> MiddlewareAkkaDefaultParallelismLevel = setting( "causal_clustering.middleware.akka.default-parallelism", INTEGER, Convert.ToString( 4 ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Parallelism level of dispatcher used for communication from Akka based cluster topology discovery ") public static final org.neo4j.graphdb.config.Setting<int> middleware_akka_sink_parallelism_level = setting("causal_clustering.middleware.akka.sink-parallelism", INTEGER, System.Convert.ToString(2));
		 [Description("Parallelism level of dispatcher used for communication from Akka based cluster topology discovery ")]
		 public static readonly Setting<int> MiddlewareAkkaSinkParallelismLevel = setting( "causal_clustering.middleware.akka.sink-parallelism", INTEGER, Convert.ToString( 2 ) );

		 /*
		     Begin akka failure detector
		     setting descriptions copied from reference.conf in akka-cluster
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Akka cluster phi accrual failure detector. " + "How often keep-alive heartbeat messages should be sent to each connection.") public static final org.neo4j.graphdb.config.Setting<java.time.Duration> akka_failure_detector_heartbeat_interval = setting("causal_clustering.middleware.akka.failure_detector.heartbeat_interval", DURATION, "1s");
		 [Description("Akka cluster phi accrual failure detector. " + "How often keep-alive heartbeat messages should be sent to each connection.")]
		 public static readonly Setting<Duration> AkkaFailureDetectorHeartbeatInterval = setting( "causal_clustering.middleware.akka.failure_detector.heartbeat_interval", DURATION, "1s" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Akka cluster phi accrual failure detector. " + "Defines the failure detector threshold. " + "A low threshold is prone to generate many wrong suspicions but ensures " + "a quick detection in the event of a real crash. Conversely, a high " + "threshold generates fewer mistakes but needs more time to detect actual crashes.") public static final org.neo4j.graphdb.config.Setting<double> akka_failure_detector_threshold = setting("causal_clustering.middleware.akka.failure_detector.threshold", DOUBLE, "10.0");
		 [Description("Akka cluster phi accrual failure detector. " + "Defines the failure detector threshold. " + "A low threshold is prone to generate many wrong suspicions but ensures " + "a quick detection in the event of a real crash. Conversely, a high " + "threshold generates fewer mistakes but needs more time to detect actual crashes.")]
		 public static readonly Setting<double> AkkaFailureDetectorThreshold = setting( "causal_clustering.middleware.akka.failure_detector.threshold", DOUBLE, "10.0" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Akka cluster phi accrual failure detector. " + "Number of the samples of inter-heartbeat arrival times to adaptively " + "calculate the failure timeout for connections.") public static final org.neo4j.graphdb.config.Setting<int> akka_failure_detector_max_sample_size = setting("causal_clustering.middleware.akka.failure_detector.max_sample_size", INTEGER, "1000");
		 [Description("Akka cluster phi accrual failure detector. " + "Number of the samples of inter-heartbeat arrival times to adaptively " + "calculate the failure timeout for connections.")]
		 public static readonly Setting<int> AkkaFailureDetectorMaxSampleSize = setting( "causal_clustering.middleware.akka.failure_detector.max_sample_size", INTEGER, "1000" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Akka cluster phi accrual failure detector. " + "Minimum standard deviation to use for the normal distribution in " + "AccrualFailureDetector. Too low standard deviation might result in " + "too much sensitivity for sudden, but normal, deviations in heartbeat inter arrival times.") public static final org.neo4j.graphdb.config.Setting<java.time.Duration> akka_failure_detector_min_std_deviation = setting("causal_clustering.middleware.akka.failure_detector.min_std_deviation", DURATION, "100ms");
		 [Description("Akka cluster phi accrual failure detector. " + "Minimum standard deviation to use for the normal distribution in " + "AccrualFailureDetector. Too low standard deviation might result in " + "too much sensitivity for sudden, but normal, deviations in heartbeat inter arrival times.")]
		 public static readonly Setting<Duration> AkkaFailureDetectorMinStdDeviation = setting( "causal_clustering.middleware.akka.failure_detector.min_std_deviation", DURATION, "100ms" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Akka cluster phi accrual failure detector. " + "Number of potentially lost/delayed heartbeats that will be " + "accepted before considering it to be an anomaly. " + "This margin is important to be able to survive sudden, occasional, " + "pauses in heartbeat arrivals, due to for example garbage collect or network drop.") public static final org.neo4j.graphdb.config.Setting<java.time.Duration> akka_failure_detector_acceptable_heartbeat_pause = setting("causal_clustering.middleware.akka.failure_detector.acceptable_heartbeat_pause", DURATION, "4s");
		 [Description("Akka cluster phi accrual failure detector. " + "Number of potentially lost/delayed heartbeats that will be " + "accepted before considering it to be an anomaly. " + "This margin is important to be able to survive sudden, occasional, " + "pauses in heartbeat arrivals, due to for example garbage collect or network drop.")]
		 public static readonly Setting<Duration> AkkaFailureDetectorAcceptableHeartbeatPause = setting( "causal_clustering.middleware.akka.failure_detector.acceptable_heartbeat_pause", DURATION, "4s" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Akka cluster phi accrual failure detector. " + "Number of member nodes that each member will send heartbeat messages to, " + "i.e. each node will be monitored by this number of other nodes.") public static final org.neo4j.graphdb.config.Setting<int> akka_failure_detector_monitored_by_nr_of_members = setting("causal_clustering.middleware.akka.failure_detector.monitored_by_nr_of_members", INTEGER, "5");
		 [Description("Akka cluster phi accrual failure detector. " + "Number of member nodes that each member will send heartbeat messages to, " + "i.e. each node will be monitored by this number of other nodes.")]
		 public static readonly Setting<int> AkkaFailureDetectorMonitoredByNrOfMembers = setting( "causal_clustering.middleware.akka.failure_detector.monitored_by_nr_of_members", INTEGER, "5" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Akka cluster phi accrual failure detector. " + "After the heartbeat request has been sent the first failure detection " + "will start after this period, even though no heartbeat message has been received.") public static final org.neo4j.graphdb.config.Setting<java.time.Duration> akka_failure_detector_expected_response_after = setting("causal_clustering.middleware.akka.failure_detector.expected_response_after", DURATION, "1s");
		 [Description("Akka cluster phi accrual failure detector. " + "After the heartbeat request has been sent the first failure detection " + "will start after this period, even though no heartbeat message has been received.")]
		 public static readonly Setting<Duration> AkkaFailureDetectorExpectedResponseAfter = setting( "causal_clustering.middleware.akka.failure_detector.expected_response_after", DURATION, "1s" );
		 /*
		     End akka failure detector
		  */

		 [Description("The maximum file size before the storage file is rotated (in unit of entries)")]
		 public static readonly Setting<int> LastFlushedStateSize = setting( "causal_clustering.last_applied_state_size", INTEGER, "1000" );

		 [Description("The maximum file size before the ID allocation file is rotated (in unit of entries)")]
		 public static readonly Setting<int> IdAllocStateSize = setting( "causal_clustering.id_alloc_state_size", INTEGER, "1000" );

		 [Description("The maximum file size before the membership state file is rotated (in unit of entries)")]
		 public static readonly Setting<int> RaftMembershipStateSize = setting( "causal_clustering.raft_membership_state_size", INTEGER, "1000" );

		 [Description("The maximum file size before the vote state file is rotated (in unit of entries)")]
		 public static readonly Setting<int> VoteStateSize = setting( "causal_clustering.raft_vote_state_size", INTEGER, "1000" );

		 [Description("The maximum file size before the term state file is rotated (in unit of entries)")]
		 public static readonly Setting<int> TermStateSize = setting( "causal_clustering.raft_term_state_size", INTEGER, "1000" );

		 [Description("The maximum file size before the global session tracker state file is rotated (in unit of entries)")]
		 public static readonly Setting<int> GlobalSessionTrackerStateSize = setting( "causal_clustering.global_session_tracker_state_size", INTEGER, "1000" );

		 [Description("The maximum file size before the replicated lock token state file is rotated (in unit of entries)")]
		 public static readonly Setting<int> ReplicatedLockTokenStateSize = setting( "causal_clustering.replicated_lock_token_state_size", INTEGER, "1000" );

		 [Description("The initial timeout until replication is retried. The timeout will increase exponentially.")]
		 public static readonly Setting<Duration> ReplicationRetryTimeoutBase = setting( "causal_clustering.replication_retry_timeout_base", DURATION, "10s" );

		 [Description("The upper limit for the exponentially incremented retry timeout.")]
		 public static readonly Setting<Duration> ReplicationRetryTimeoutLimit = setting( "causal_clustering.replication_retry_timeout_limit", DURATION, "60s" );

		 [Description("The number of operations to be processed before the state machines flush to disk")]
		 public static readonly Setting<int> StateMachineFlushWindowSize = setting( "causal_clustering.state_machine_flush_window_size", INTEGER, "4096" );

		 [Description("The maximum number of operations to be batched during applications of operations in the state machines")]
		 public static readonly Setting<int> StateMachineApplyMaxBatchSize = setting( "causal_clustering.state_machine_apply_max_batch_size", INTEGER, "16" );

		 [Description("RAFT log pruning strategy")]
		 public static readonly Setting<string> RaftLogPruningStrategy = setting( "causal_clustering.raft_log_prune_strategy", STRING, "1g size" );

		 [Description("RAFT log implementation")]
		 public static readonly Setting<string> RaftLogImplementation = setting( "causal_clustering.raft_log_implementation", STRING, "SEGMENTED" );

		 [Description("RAFT log rotation size")]
		 public static readonly Setting<long> RaftLogRotationSize = buildSetting( "causal_clustering.raft_log_rotation_size", BYTES, "250M" ).constraint( min( 1024L ) ).build();

		 [Description("RAFT log reader pool size")]
		 public static readonly Setting<int> RaftLogReaderPoolSize = setting( "causal_clustering.raft_log_reader_pool_size", INTEGER, "8" );

		 [Description("RAFT log pruning frequency")]
		 public static readonly Setting<Duration> RaftLogPruningFrequency = setting( "causal_clustering.raft_log_pruning_frequency", DURATION, "10m" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Enable or disable the dump of all network messages pertaining to the RAFT protocol") @Internal public static final org.neo4j.graphdb.config.Setting<bool> raft_messages_log_enable = setting("causal_clustering.raft_messages_log_enable", BOOLEAN, FALSE);
		 [Description("Enable or disable the dump of all network messages pertaining to the RAFT protocol")]
		 public static readonly Setting<bool> RaftMessagesLogEnable = setting( "causal_clustering.raft_messages_log_enable", BOOLEAN, FALSE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Path to RAFT messages log.") @Internal public static final org.neo4j.graphdb.config.Setting<java.io.File> raft_messages_log_path = derivedSetting("causal_clustering.raft_messages_log_path", logs_directory, logs -> new java.io.File(logs, "raft-messages.log"), PATH);
		 [Description("Path to RAFT messages log.")]
		 public static readonly Setting<File> RaftMessagesLogPath = derivedSetting( "causal_clustering.raft_messages_log_path", logs_directory, logs => new File( logs, "raft-messages.log" ), PATH );

		 [Description("Interval of pulling updates from cores.")]
		 public static readonly Setting<Duration> PullInterval = setting( "causal_clustering.pull_interval", DURATION, "1s" );

		 [Description("The catch up protocol times out if the given duration elapses with no network activity. " + "Every message received by the client from the server extends the time out duration.")]
		 public static readonly Setting<Duration> CatchUpClientInactivityTimeout = setting( "causal_clustering.catch_up_client_inactivity_timeout", DURATION, "10m" );

		 [Description("Maximum retry time per request during store copy. Regular store files and indexes are downloaded in separate requests during store copy." + " This configures the maximum time failed requests are allowed to resend. ")]
		 public static readonly Setting<Duration> StoreCopyMaxRetryTimePerRequest = setting( "causal_clustering.store_copy_max_retry_time_per_request", DURATION, "20m" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Maximum backoff timeout for store copy requests") @Internal public static final org.neo4j.graphdb.config.Setting<java.time.Duration> store_copy_backoff_max_wait = setting("causal_clustering.store_copy_backoff_max_wait", DURATION, "5s");
		 [Description("Maximum backoff timeout for store copy requests")]
		 public static readonly Setting<Duration> StoreCopyBackoffMaxWait = setting( "causal_clustering.store_copy_backoff_max_wait", DURATION, "5s" );

		 [Description("Throttle limit for logging unknown cluster member address")]
		 public static readonly Setting<Duration> UnknownAddressLoggingThrottle = setting( "causal_clustering.unknown_address_logging_throttle", DURATION, "10000ms" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Maximum transaction batch size for read replicas when applying transactions pulled from core " + "servers.") @Internal public static final org.neo4j.graphdb.config.Setting<int> read_replica_transaction_applier_batch_size = setting("causal_clustering.read_replica_transaction_applier_batch_size", INTEGER, "64");
		 [Description("Maximum transaction batch size for read replicas when applying transactions pulled from core " + "servers.")]
		 public static readonly Setting<int> ReadReplicaTransactionApplierBatchSize = setting( "causal_clustering.read_replica_transaction_applier_batch_size", INTEGER, "64" );

		 [Description("Time To Live before read replica is considered unavailable")]
		 public static readonly Setting<Duration> ReadReplicaTimeToLive = buildSetting( "causal_clustering.read_replica_time_to_live", DURATION, "1m" ).constraint( min( Duration.ofSeconds( 60 ) ) ).build();

		 [Description("How long drivers should cache the data from the `dbms.cluster.routing.getServers()` procedure.")]
		 public static readonly Setting<Duration> ClusterRoutingTtl = buildSetting( "causal_clustering.cluster_routing_ttl", DURATION, "300s" ).constraint( min( Duration.ofSeconds( 1 ) ) ).build();

		 [Description("Configure if the `dbms.cluster.routing.getServers()` procedure should include followers as read " + "endpoints or return only read replicas. Note: if there are no read replicas in the cluster, followers " + "are returned as read end points regardless the value of this setting. Defaults to true so that followers " + "are available for read-only queries in a typical heterogeneous setup.")]
		 public static readonly Setting<bool> ClusterAllowReadsOnFollowers = setting( "causal_clustering.cluster_allow_reads_on_followers", BOOLEAN, TRUE );

		 [Description("The size of the ID allocation requests Core servers will make when they run out of NODE IDs. " + "Larger values mean less frequent requests but also result in more unused IDs (and unused disk space) " + "in the event of a crash.")]
		 public static readonly Setting<int> NodeIdAllocationSize = setting( "causal_clustering.node_id_allocation_size", INTEGER, "1024" );

		 [Description("The size of the ID allocation requests Core servers will make when they run out " + "of RELATIONSHIP IDs. Larger values mean less frequent requests but also result in more unused IDs " + "(and unused disk space) in the event of a crash.")]
		 public static readonly Setting<int> RelationshipIdAllocationSize = setting( "causal_clustering.relationship_id_allocation_size", INTEGER, "1024" );

		 [Description("The size of the ID allocation requests Core servers will make when they run out " + "of PROPERTY IDs. Larger values mean less frequent requests but also result in more unused IDs " + "(and unused disk space) in the event of a crash.")]
		 public static readonly Setting<int> PropertyIdAllocationSize = setting( "causal_clustering.property_id_allocation_size", INTEGER, "1024" );

		 [Description("The size of the ID allocation requests Core servers will make when they run out " + "of STRING_BLOCK IDs. Larger values mean less frequent requests but also result in more unused IDs " + "(and unused disk space) in the event of a crash.")]
		 public static readonly Setting<int> StringBlockIdAllocationSize = setting( "causal_clustering.string_block_id_allocation_size", INTEGER, "1024" );

		 [Description("The size of the ID allocation requests Core servers will make when they run out " + "of ARRAY_BLOCK IDs. Larger values mean less frequent requests but also result in more unused IDs " + "(and unused disk space) in the event of a crash.")]
		 public static readonly Setting<int> ArrayBlockIdAllocationSize = setting( "causal_clustering.array_block_id_allocation_size", INTEGER, "1024" );

		 [Description("The size of the ID allocation requests Core servers will make when they run out " + "of PROPERTY_KEY_TOKEN IDs. Larger values mean less frequent requests but also result in more unused IDs " + "(and unused disk space) in the event of a crash.")]
		 public static readonly Setting<int> PropertyKeyTokenIdAllocationSize = setting( "causal_clustering.property_key_token_id_allocation_size", INTEGER, "32" );

		 [Description("The size of the ID allocation requests Core servers will make when they run out " + "of PROPERTY_KEY_TOKEN_NAME IDs. Larger values mean less frequent requests but also result in more " + "unused IDs (and unused disk space) in the event of a crash.")]
		 public static readonly Setting<int> PropertyKeyTokenNameIdAllocationSize = setting( "causal_clustering.property_key_token_name_id_allocation_size", INTEGER, "1024" );

		 [Description("The size of the ID allocation requests Core servers will make when they run out " + "of RELATIONSHIP_TYPE_TOKEN IDs. Larger values mean less frequent requests but also result in more " + "unused IDs (and unused disk space) in the event of a crash.")]
		 public static readonly Setting<int> RelationshipTypeTokenIdAllocationSize = setting( "causal_clustering.relationship_type_token_id_allocation_size", INTEGER, "32" );

		 [Description("The size of the ID allocation requests Core servers will make when they run out " + "of RELATIONSHIP_TYPE_TOKEN_NAME IDs. Larger values mean less frequent requests but also result in more " + "unused IDs (and unused disk space) in the event of a crash.")]
		 public static readonly Setting<int> RelationshipTypeTokenNameIdAllocationSize = setting( "causal_clustering.relationship_type_token_name_id_allocation_size", INTEGER, "1024" );

		 [Description("The size of the ID allocation requests Core servers will make when they run out " + "of LABEL_TOKEN IDs. Larger values mean less frequent requests but also result in more " + "unused IDs (and unused disk space) in the event of a crash.")]
		 public static readonly Setting<int> LabelTokenIdAllocationSize = setting( "causal_clustering.label_token_id_allocation_size", INTEGER, "32" );

		 [Description("The size of the ID allocation requests Core servers will make when they run out " + "of LABEL_TOKEN_NAME IDs. Larger values mean less frequent requests but also result in more " + "unused IDs (and unused disk space) in the event of a crash.")]
		 public static readonly Setting<int> LabelTokenNameIdAllocationSize = setting( "causal_clustering.label_token_name_id_allocation_size", INTEGER, "1024" );

		 [Description("The size of the ID allocation requests Core servers will make when they run out " + "of NEOSTORE_BLOCK IDs. Larger values mean less frequent requests but also result in more " + "unused IDs (and unused disk space) in the event of a crash.")]
		 public static readonly Setting<int> NeostoreBlockIdAllocationSize = setting( "causal_clustering.neostore_block_id_allocation_size", INTEGER, "1024" );

		 [Description("The size of the ID allocation requests Core servers will make when they run out " + "of SCHEMA IDs. Larger values mean less frequent requests but also result in more " + "unused IDs (and unused disk space) in the event of a crash.")]
		 public static readonly Setting<int> SchemaIdAllocationSize = setting( "causal_clustering.schema_id_allocation_size", INTEGER, "1024" );

		 [Description("The size of the ID allocation requests Core servers will make when they run out " + "of NODE_LABELS IDs. Larger values mean less frequent requests but also result in more " + "unused IDs (and unused disk space) in the event of a crash.")]
		 public static readonly Setting<int> NodeLabelsIdAllocationSize = setting( "causal_clustering.node_labels_id_allocation_size", INTEGER, "1024" );

		 [Description("The size of the ID allocation requests Core servers will make when they run out " + "of RELATIONSHIP_GROUP IDs. Larger values mean less frequent requests but also result in more " + "unused IDs (and unused disk space) in the event of a crash.")]
		 public static readonly Setting<int> RelationshipGroupIdAllocationSize = setting( "causal_clustering.relationship_group_id_allocation_size", INTEGER, "1024" );

		 [Description("Time between scanning the cluster to refresh current server's view of topology")]
		 public static readonly Setting<Duration> ClusterTopologyRefresh = buildSetting( "causal_clustering.cluster_topology_refresh", DURATION, "5s" ).constraint( min( Duration.ofSeconds( 1 ) ) ).build();

		 [Description("An ordered list in descending preference of the strategy which read replicas use to choose " + "the upstream server from which to pull transactional updates.")]
		 public static readonly Setting<IList<string>> UpstreamSelectionStrategy = setting( "causal_clustering.upstream_selection_strategy", list( ",", STRING ), "default" );

		 [Description("Configuration of a user-defined upstream selection strategy. " + "The user-defined strategy is used if the list of strategies (`causal_clustering.upstream_selection_strategy`) " + "includes the value `user_defined`. ")]
		 public static readonly Setting<string> UserDefinedUpstreamSelectionStrategy = setting( "causal_clustering.user_defined_upstream_strategy", STRING, "" );

		 [Description("Comma separated list of groups to be used by the connect-randomly-to-server-group selection strategy. " + "The connect-randomly-to-server-group strategy is used if the list of strategies (`causal_clustering.upstream_selection_strategy`) " + "includes the value `connect-randomly-to-server-group`. ")]
		 public static readonly Setting<IList<string>> ConnectRandomlyToServerGroupStrategy = setting( "causal_clustering.connect-randomly-to-server-group", list( ",", STRING ), "" );

		 [Description("A list of group names for the server used when configuring load balancing and replication policies.")]
		 public static readonly Setting<IList<string>> ServerGroups = setting( "causal_clustering.server_groups", list( ",", STRING ), "" );

		 [Description("The load balancing plugin to use.")]
		 public static readonly Setting<string> LoadBalancingPlugin = setting( "causal_clustering.load_balancing.plugin", STRING, "server_policies" );

		 [Description("Time out for protocol negotiation handshake")]
		 public static readonly Setting<Duration> HandshakeTimeout = setting( "causal_clustering.handshake_timeout", DURATION, "20s" );

		 [Description("The configuration must be valid for the configured plugin and usually exists" + "under matching subkeys, e.g. ..config.server_policies.*" + "This is just a top-level placeholder for the plugin-specific configuration.")]
		 public static readonly Setting<string> LoadBalancingConfig = prefixSetting( "causal_clustering.load_balancing.config", STRING, "" );

		 [Description("Enables shuffling of the returned load balancing result.")]
		 public static readonly Setting<bool> LoadBalancingShuffle = setting( "causal_clustering.load_balancing.shuffle", BOOLEAN, TRUE );

		 [Description("Require authorization for access to the Causal Clustering status endpoints.")]
		 public static readonly Setting<bool> StatusAuthEnabled = setting( "dbms.security.causal_clustering_status_auth_enabled", BOOLEAN, TRUE );

		 [Description("Enable multi-data center features. Requires appropriate licensing.")]
		 public static readonly Setting<bool> MultiDcLicense = setting( "causal_clustering.multi_dc_license", BOOLEAN, FALSE );

		 [Description("Name of the SSL policy to be used by the clustering, as defined under the dbms.ssl.policy.* settings." + " If no policy is configured then the communication will not be secured.")]
		 public static readonly Setting<string> SslPolicy = prefixSetting( "causal_clustering.ssl_policy", STRING, NO_DEFAULT );

		 [Description("Raft protocol implementation versions that this instance will allow in negotiation as a comma-separated list." + " Order is not relevant: the greatest value will be preferred. An empty list will allow all supported versions")]
		 public static readonly Setting<IList<int>> RaftImplementations = setting( "causal_clustering.protocol_implementations.raft", list( ",", INTEGER ), "" );

		 [Description("Catchup protocol implementation versions that this instance will allow in negotiation as a comma-separated list." + " Order is not relevant: the greatest value will be preferred. An empty list will allow all supported versions")]
		 public static readonly Setting<IList<int>> CatchupImplementations = setting( "causal_clustering.protocol_implementations.catchup", list( ",", INTEGER ), "" );

		 [Description("Network compression algorithms that this instance will allow in negotiation as a comma-separated list." + " Listed in descending order of preference for incoming connections. An empty list implies no compression." + " For outgoing connections this merely specifies the allowed set of algorithms and the preference of the " + " remote peer will be used for making the decision." + " Allowable values: [" + GZIP + "," + SNAPPY + "," + SNAPPY_VALIDATING + "," + LZ4 + "," + LZ4_HIGH_COMPRESSION + "," + LZ_VALIDATING + "," + LZ4_HIGH_COMPRESSION_VALIDATING + "]")]
		 public static readonly Setting<IList<string>> CompressionImplementations = setting( "causal_clustering.protocol_implementations.compression", STRING_LIST, "" );
	}

}