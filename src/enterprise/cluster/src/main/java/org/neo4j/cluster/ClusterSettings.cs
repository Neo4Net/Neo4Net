using System.Collections.Generic;

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
namespace Neo4Net.cluster
{

	using Description = Neo4Net.Configuration.Description;
	using Internal = Neo4Net.Configuration.Internal;
	using LoadableConfig = Neo4Net.Configuration.LoadableConfig;
	using Neo4Net.GraphDb.config;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using Settings = Neo4Net.Kernel.configuration.Settings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.ANY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.DURATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.HOSTNAME_PORT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.INTEGER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.NO_DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.buildSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.illegalValueMessage;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.list;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.matches;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.setting;

	/// <summary>
	/// Settings for cluster members
	/// </summary>
	[Description("Cluster configuration settings")]
	public class ClusterSettings : LoadableConfig
	{
		 public static readonly System.Func<string, InstanceId> INSTANCE_ID = new FuncAnonymousInnerClass();

		 private class FuncAnonymousInnerClass : System.Func<string, InstanceId>
		 {
			 public override InstanceId apply( string value )
			 {
				  try
				  {
						return new InstanceId( int.Parse( value ) );
				  }
				  catch ( System.FormatException )
				  {
						throw new System.ArgumentException( "not a valid integer value" );
				  }
			 }

			 public override string ToString()
			 {
				  return "an instance id, which has to be a valid integer";
			 }
		 }

		 [Description("Id for a cluster instance. Must be unique within the cluster.")]
		 public static readonly Setting<InstanceId> ServerId = setting( "ha.server_id", INSTANCE_ID, NO_DEFAULT );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("The name of a cluster.") @Internal public static final org.Neo4Net.graphdb.config.Setting<String> cluster_name = buildSetting("unsupported.ha.cluster_name", STRING, "Neo4Net.ha").constraint(illegalValueMessage("must be a valid cluster name", matches(ANY))).build();
		 [Description("The name of a cluster.")]
		 public static readonly Setting<string> ClusterName = buildSetting( "unsupported.ha.cluster_name", STRING, "Neo4Net.ha" ).constraint( illegalValueMessage( "must be a valid cluster name", matches( ANY ) ) ).build();

		 [Description("A comma-separated list of other members of the cluster to join.")]
		 public static readonly Setting<IList<HostnamePort>> InitialHosts = setting( "ha.initial_hosts", list( ",", HOSTNAME_PORT ), NO_DEFAULT );

		 [Description("Host and port to bind the cluster management communication.")]
		 public static readonly Setting<HostnamePort> ClusterServer = setting( "ha.host.coordination", HOSTNAME_PORT, "0.0.0.0:5001-5099" );

		 [Description("Whether to allow this instance to create a cluster if unable to join.")]
		 public static readonly Setting<bool> AllowInitCluster = setting( "ha.allow_init_cluster", BOOLEAN, TRUE );

		 // Timeout settings

		 /*
		  * ha.heartbeat_interval
		  * ha.paxos_timeout
		  * ha.learn_timeout
		  */
		 [Description("Default timeout used for clustering timeouts. Override  specific timeout settings with proper" + " values if necessary. This value is the default value for the ha.heartbeat_interval," + " ha.paxos_timeout and ha.learn_timeout settings.")]
		 public static readonly Setting<Duration> DefaultTimeout = setting( "ha.default_timeout", DURATION, "5s" );

		 [Description("How often heartbeat messages should be sent. Defaults to ha.default_timeout.")]
		 public static readonly Setting<Duration> HeartbeatInterval = buildSetting( "ha.heartbeat_interval", DURATION ).inherits( DefaultTimeout ).build();

		 [Description("How long to wait for heartbeats from other instances before marking them as suspects for failure. " + "This value reflects considerations of network latency, expected duration of garbage collection pauses " + "and other factors that can delay message sending and processing. Larger values will result in more " + "stable masters but also will result in longer waits before a failover in case of master failure. This " + "value should not be set to less than twice the ha.heartbeat_interval value otherwise there is a high " + "risk of frequent master switches and possibly branched data occurrence.")]
		 public static readonly Setting<Duration> HeartbeatTimeout = setting( "ha.heartbeat_timeout", DURATION, "40s" );

		 /*
		  * ha.join_timeout
		  * ha.leave_timeout
		  */
		 [Description("Timeout for broadcasting values in cluster. Must consider end-to-end duration of Paxos algorithm." + " This value is the default value for the ha.join_timeout and ha.leave_timeout settings.")]
		 public static readonly Setting<Duration> BroadcastTimeout = setting( "ha.broadcast_timeout", DURATION, "30s" );

		 [Description("Timeout for joining a cluster. Defaults to ha.broadcast_timeout. " + "Note that if the timeout expires during cluster formation, the operator may have to restart the instance or instances.")]
		 public static readonly Setting<Duration> JoinTimeout = buildSetting( "ha.join_timeout", DURATION ).inherits( BroadcastTimeout ).build();

		 [Description("Timeout for waiting for configuration from an existing cluster member during cluster join.")]
		 public static readonly Setting<Duration> ConfigurationTimeout = setting( "ha.configuration_timeout", DURATION, "1s" );

		 [Description("Timeout for waiting for cluster leave to finish. Defaults to ha.broadcast_timeout.")]
		 public static readonly Setting<Duration> LeaveTimeout = buildSetting( "ha.leave_timeout", DURATION ).inherits( BroadcastTimeout ).build();

		 /*
		  *  ha.phase1_timeout
		  *  ha.phase2_timeout
		  *  ha.election_timeout
		  */
		 [Description("Default value for all Paxos timeouts. This setting controls the default value for the ha.phase1_timeout, " + "ha.phase2_timeout and ha.election_timeout settings. If it is not given a value it " + "defaults to ha.default_timeout and will implicitly change if ha.default_timeout changes. This is an " + "advanced parameter which should only be changed if specifically advised by Neo4Net Professional Services.")]
		 public static readonly Setting<Duration> PaxosTimeout = buildSetting( "ha.paxos_timeout", DURATION ).inherits( DefaultTimeout ).build();

		 [Description("Timeout for Paxos phase 1. If it is not given a value it defaults to ha.paxos_timeout and will " + "implicitly change if ha.paxos_timeout changes. This is an advanced parameter which should only be " + "changed if specifically advised by Neo4Net Professional Services. ")]
		 public static readonly Setting<Duration> Phase1Timeout = buildSetting( "ha.phase1_timeout", DURATION ).inherits( PaxosTimeout ).build();

		 [Description("Timeout for Paxos phase 2. If it is not given a value it defaults to ha.paxos_timeout and will " + "implicitly change if ha.paxos_timeout changes. This is an advanced parameter which should only be " + "changed if specifically advised by Neo4Net Professional Services. ")]
		 public static readonly Setting<Duration> Phase2Timeout = buildSetting( "ha.phase2_timeout", DURATION ).inherits( PaxosTimeout ).build();

		 [Description("Timeout for learning values. Defaults to ha.default_timeout.")]
		 public static readonly Setting<Duration> LearnTimeout = buildSetting( "ha.learn_timeout", DURATION ).inherits( DefaultTimeout ).build();

		 [Description("Timeout for waiting for other members to finish a role election. Defaults to ha.paxos_timeout.")]
		 public static readonly Setting<Duration> ElectionTimeout = buildSetting( "ha.election_timeout", DURATION ).inherits( PaxosTimeout ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.Neo4Net.graphdb.config.Setting<String> instance_name = setting("unsupported.ha.instance_name", STRING, org.Neo4Net.kernel.configuration.Settings.NO_DEFAULT);
		 public static readonly Setting<string> InstanceName = setting( "unsupported.ha.instance_name", STRING, Settings.NO_DEFAULT );

		 [Description("Maximum number of servers to involve when agreeing to membership changes. " + "In very large clusters, the probability of half the cluster failing is low, but protecting against " + "any arbitrary half failing is expensive. Therefore you may wish to set this parameter to a value less " + "than the cluster size.")]
		 public static readonly Setting<int> MaxAcceptors = buildSetting( "ha.max_acceptors", INTEGER, "21" ).constraint( min( 1 ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.Neo4Net.graphdb.config.Setting<bool> strict_initial_hosts = setting("ha.strict_initial_hosts", BOOLEAN, FALSE);
		 public static readonly Setting<bool> StrictInitialHosts = setting( "ha.strict_initial_hosts", BOOLEAN, FALSE );
	}

}