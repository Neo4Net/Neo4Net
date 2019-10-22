using System;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.ha
{

	using Description = Neo4Net.Configuration.Description;
	using Internal = Neo4Net.Configuration.Internal;
	using LoadableConfig = Neo4Net.Configuration.LoadableConfig;
	using Neo4Net.GraphDb.config;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using ConfigurationMigrator = Neo4Net.Kernel.configuration.ConfigurationMigrator;
	using Migrator = Neo4Net.Kernel.configuration.Migrator;
	using Settings = Neo4Net.Kernel.configuration.Settings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.BYTES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.DURATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.HOSTNAME_PORT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.INTEGER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.buildSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.optionsObeyCase;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.setting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.ha.HaSettings.BranchedDataCopyingStrategy.branch_then_copy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.ha.HaSettings.TxPushStrategy.fixed_ascending;

	/// <summary>
	/// Settings for High Availability mode
	/// </summary>
	[Description("High Availability configuration settings"), Obsolete]
	public class HaSettings : LoadableConfig
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Migrator private static final org.Neo4Net.kernel.configuration.ConfigurationMigrator migrator = new EnterpriseConfigurationMigrator();
		 private static readonly ConfigurationMigrator _migrator = new EnterpriseConfigurationMigrator();

		 [Description("How long a slave will wait for response from master before giving up."), Obsolete]
		 public static readonly Setting<Duration> ReadTimeout = setting( "ha.slave_read_timeout", DURATION, "20s" );

		 [Description("Timeout for request threads waiting for instance to become master or slave."), Obsolete]
		 public static readonly Setting<Duration> StateSwitchTimeout = setting( "ha.role_switch_timeout", DURATION, "120s" );

		 [Description("Timeout for waiting for internal conditions during state switch, like for transactions " + "to complete, before switching to master or slave."), Obsolete]
		 public static readonly Setting<Duration> InternalStateSwitchTimeout = setting( "ha.internal_role_switch_timeout", DURATION, "10s" );

		 [Description("Timeout for taking remote (write) locks on slaves. Defaults to ha.slave_read_timeout."), Obsolete]
		 public static readonly Setting<Duration> LockReadTimeout = buildSetting( "ha.slave_lock_timeout", DURATION ).inherits( ReadTimeout ).build();

		 [Description("Maximum number of connections a slave can have to the master."), Obsolete]
		 public static readonly Setting<int> MaxConcurrentChannelsPerSlave = buildSetting( "ha.max_channels_per_slave", INTEGER, "20" ).constraint( min( 1 ) ).build();

		 [Description("Hostname and port to bind the HA server."), Obsolete]
		 public static readonly Setting<HostnamePort> HaServer = setting( "ha.host.data", HOSTNAME_PORT, "0.0.0.0:6001-6011" );

		 [Description("Whether this instance should only participate as slave in cluster. " + "If set to `true`, it will never be elected as master."), Obsolete]
		 public static readonly Setting<bool> SlaveOnly = setting( "ha.slave_only", BOOLEAN, Settings.FALSE );

		 [Description("Policy for how to handle branched data."), Obsolete]
		 public static readonly Setting<BranchedDataPolicy> BranchedDataPolicy = setting( "ha.branched_data_policy", optionsObeyCase( typeof( BranchedDataPolicy ) ), "keep_all" );

		 [Description("Require authorization for access to the HA status endpoints."), Obsolete]
		 public static readonly Setting<bool> HaStatusAuthEnabled = setting( "dbms.security.ha_status_auth_enabled", BOOLEAN, Settings.TRUE );

		 [Description("Max size of the data chunks that flows between master and slaves in HA. Bigger size may increase " + "throughput, but may also be more sensitive to variations in bandwidth, whereas lower size increases " + "tolerance for bandwidth variations."), Obsolete]
		 public static readonly Setting<long> ComChunkSize = buildSetting( "ha.data_chunk_size", BYTES, "2M" ).constraint( min( 1024L ) ).build();

		 [Description("Interval of pulling updates from master."), Obsolete]
		 public static readonly Setting<Duration> PullInterval = setting( "ha.pull_interval", DURATION, "0s" );

		 [Description("The amount of slaves the master will ask to replicate a committed transaction. "), Obsolete]
		 public static readonly Setting<int> TxPushFactor = buildSetting( "ha.tx_push_factor", INTEGER, "1" ).constraint( min( 0 ) ).build();

		 [Description("Push strategy of a transaction to a slave during commit."), Obsolete]
		 public static readonly Setting<TxPushStrategy> TxPushStrategy = setting( "ha.tx_push_strategy", optionsObeyCase( typeof( TxPushStrategy ) ), fixed_ascending.name() );

		 [Description("Strategy for how to order handling of branched data on slaves and copying of the store from the" + " master. The default is copy_then_branch, which, when combined with the keep_last or keep_none branch" + " handling strategies results in a safer branching strategy, as there is always a store present so store" + " failure to copy a store (for example, because of network failure) does not leave the instance without" + " a store."), Obsolete]
		 public static readonly Setting<BranchedDataCopyingStrategy> BranchedDataCopyingStrategy = setting( "ha.branched_data_copying_strategy", optionsObeyCase( typeof( BranchedDataCopyingStrategy ) ), branch_then_copy.name() );

		 [Description("Size of batches of transactions applied on slaves when pulling from master"), Obsolete]
		 public static readonly Setting<int> PullApplyBatchSize = setting( "ha.pull_batch_size", INTEGER, "100" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Duration for which master will buffer ids and not reuse them to allow slaves read " + "consistently. Slaves will also terminate transactions longer than this duration, when " + "applying received transaction stream, to make sure they do not read potentially " + "inconsistent/reused records.") @Internal @Deprecated public static final org.Neo4Net.graphdb.config.Setting<java.time.Duration> id_reuse_safe_zone_time = setting("unsupported.dbms.id_reuse_safe_zone", org.Neo4Net.kernel.configuration.Settings.DURATION, "1h");
		 [Description("Duration for which master will buffer ids and not reuse them to allow slaves read " + "consistently. Slaves will also terminate transactions longer than this duration, when " + "applying received transaction stream, to make sure they do not read potentially " + "inconsistent/reused records."), Obsolete]
		 public static readonly Setting<Duration> IdReuseSafeZoneTime = setting( "unsupported.dbms.id_reuse_safe_zone", Settings.DURATION, "1h" );

		 [Obsolete]
		 public enum BranchedDataCopyingStrategy
		 {
			  [Description("First handles the branched store, then copies down a new store from the master and " + "replaces it. This strategy, when combined with the keep_last or keep_none branch handling " + "strategies results in less space used as the store is first removed and then the copy is fetched.")]
			  BranchThenCopy,

			  [Description("First copies down a new store from the master, then branches the existing store and " + "replaces it. This strategy uses potentially more space than branch_then_copy but it allows " + "for store copy failures to be recoverable as the original store is maintained until " + "the store copy finishes successfully.")]
			  CopyThenBranch
		 }

		 [Obsolete]
		 public enum TxPushStrategy
		 {
			  [Description("Round robin")]
			  RoundRobin,

			  [Description("Fixed, prioritized by server id in descending order. This strategy will push to the same set " + "of instances, as long as they remain available, and will prioritize available " + "instances with the highest instance ids.")]
			  FixedDescending,

			  [Description("Fixed, prioritized by server id in ascending order. This strategy will push to the same set of" + " instances, as long as they remain available, and will prioritize those available instances " + "with the lowest instance ids. This strategy makes it more likely that the most " + "up-to-date instance in a cluster will be an instance with a low id. This is consistent with the " + "master reelection tie-breaking strategy of letting the instance with the lowest id win an election if " + "several instances are equally up-to-date. Thus, using this strategy makes it very likely " + "that failover will happen in a low-id part of the cluster, which can be very helpful in " + "planning a multi-data center deployment.")]
			  FixedAscending
		 }
	}

}