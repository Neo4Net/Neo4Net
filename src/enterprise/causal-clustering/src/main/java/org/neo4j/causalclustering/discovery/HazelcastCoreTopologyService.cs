using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.causalclustering.discovery
{
	using InterfacesConfig = com.hazelcast.config.InterfacesConfig;
	using JoinConfig = com.hazelcast.config.JoinConfig;
	using ListenerConfig = com.hazelcast.config.ListenerConfig;
	using MemberAttributeConfig = com.hazelcast.config.MemberAttributeConfig;
	using NetworkConfig = com.hazelcast.config.NetworkConfig;
	using TcpIpConfig = com.hazelcast.config.TcpIpConfig;
	using Hazelcast = com.hazelcast.core.Hazelcast;
	using HazelcastException = com.hazelcast.core.HazelcastException;
	using HazelcastInstance = com.hazelcast.core.HazelcastInstance;
	using MemberAttributeEvent = com.hazelcast.core.MemberAttributeEvent;
	using MembershipEvent = com.hazelcast.core.MembershipEvent;
	using MembershipListener = com.hazelcast.core.MembershipListener;
	using Address = com.hazelcast.nio.Address;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using LeaderInfo = Neo4Net.causalclustering.core.consensus.LeaderInfo;
	using RobustJobSchedulerWrapper = Neo4Net.causalclustering.helper.RobustJobSchedulerWrapper;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using SocketAddress = Neo4Net.Helpers.SocketAddress;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.hazelcast.spi.properties.GroupProperty.INITIAL_MIN_CLUSTER_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.hazelcast.spi.properties.GroupProperty.LOGGING_TYPE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.hazelcast.spi.properties.GroupProperty.MERGE_FIRST_RUN_DELAY_SECONDS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.hazelcast.spi.properties.GroupProperty.MERGE_NEXT_RUN_DELAY_SECONDS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.hazelcast.spi.properties.GroupProperty.OPERATION_CALL_TIMEOUT_MILLIS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.hazelcast.spi.properties.GroupProperty.PREFER_IPv4_STACK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.disable_middleware_logging;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.discovery_listen_address;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.extractCatchupAddressesMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.getCoreTopology;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.getReadReplicaTopology;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.refreshGroups;

	public class HazelcastCoreTopologyService : AbstractCoreTopologyService
	{
		 public interface Monitor
		 {
			  void DiscoveredMember( SocketAddress socketAddress );

			  void LostMember( SocketAddress socketAddress );
		 }

		 private static readonly long _hazelcastIsHealthyTimeoutMs = TimeUnit.MINUTES.toMillis( 10 );
		 private const int HAZELCAST_MIN_CLUSTER = 2;

		 private readonly RobustJobSchedulerWrapper _scheduler;
		 private readonly long _refreshPeriod;
		 private readonly RemoteMembersResolver _remoteMembersResolver;
		 private readonly TopologyServiceRetryStrategy _topologyServiceRetryStrategy;
		 private readonly Monitor _monitor;
		 private readonly string _localDBName;

		 private JobHandle _refreshJob;

		 private readonly AtomicReference<LeaderInfo> _leaderInfo = new AtomicReference<LeaderInfo>( LeaderInfo.INITIAL );
		 private readonly AtomicReference<Optional<LeaderInfo>> _stepDownInfo = new AtomicReference<Optional<LeaderInfo>>( null );

		 private volatile HazelcastInstance _hazelcastInstance;

		 private volatile IDictionary<MemberId, AdvertisedSocketAddress> _catchupAddressMap = new Dictionary<MemberId, AdvertisedSocketAddress>();
		 private volatile IDictionary<MemberId, RoleInfo> _coreRoles = Collections.emptyMap();

		 private volatile ReadReplicaTopology _readReplicaTopology = ReadReplicaTopology.Empty;
		 private volatile CoreTopology _coreTopology = CoreTopology.Empty;
		 private volatile CoreTopology _localCoreTopology = CoreTopology.Empty;
		 private volatile ReadReplicaTopology _localReadReplicaTopology = ReadReplicaTopology.Empty;

		 private Thread _startingThread;
		 private volatile bool _stopped;

		 public HazelcastCoreTopologyService( Config config, MemberId myself, IJobScheduler jobScheduler, LogProvider logProvider, LogProvider userLogProvider, RemoteMembersResolver remoteMembersResolver, TopologyServiceRetryStrategy topologyServiceRetryStrategy, Monitors monitors ) : base( config, myself, logProvider, userLogProvider )
		 {
			  this._localDBName = config.Get( CausalClusteringSettings.database );
			  this._scheduler = new RobustJobSchedulerWrapper( jobScheduler, Log );
			  this._refreshPeriod = config.Get( CausalClusteringSettings.cluster_topology_refresh ).toMillis();
			  this._remoteMembersResolver = remoteMembersResolver;
			  this._topologyServiceRetryStrategy = topologyServiceRetryStrategy;
			  this._monitor = monitors.NewMonitor( typeof( Monitor ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean setClusterId(org.neo4j.causalclustering.identity.ClusterId clusterId, String dbName) throws InterruptedException
		 public override bool SetClusterId( ClusterId clusterId, string dbName )
		 {
			  WaitOnHazelcastInstanceCreation();
			  return HazelcastClusterTopology.CasClusterId( _hazelcastInstance, clusterId, dbName );
		 }

		 public override LeaderInfo Leader0
		 {
			 set
			 {
				  _leaderInfo.set( value );
			 }
		 }

		 public override LeaderInfo Leader
		 {
			 get
			 {
				  return _leaderInfo.get();
			 }
		 }

		 public override void HandleStepDown0( LeaderInfo steppingDown )
		 {
			  _stepDownInfo.set( steppingDown );
		 }

		 public override IDictionary<MemberId, RoleInfo> AllCoreRoles()
		 {
			  return _coreRoles;
		 }

		 public override void Init0()
		 {
			  // nothing to do
		 }

		 public override void Start0()
		 {
			  /*
			   * We will start hazelcast in its own thread. Hazelcast blocks until the minimum cluster size is available
			   * and during that block it ignores interrupts. This blocks the whole startup process and since it is the
			   * main thread that controls lifecycle and the main thread is not daemon, it will block ignoring signals
			   * and any shutdown attempts. The solution is to start hazelcast instance creation in its own thread which
			   * we set as daemon. All subsequent uses of hazelcastInstance in this class will still block on it being
			   * available (see waitOnHazelcastInstanceCreation() ) but they do so while checking for interrupt and
			   * exiting if one happens. This provides us with a way to exit before hazelcastInstance creation completes.
			   */
			  _startingThread = new Thread(() =>
			  {
				Log.info( "Cluster discovery service starting" );
				_hazelcastInstance = CreateHazelcastInstance();
				// We may be interrupted by the stop method after hazelcast returns. This is courtesy and not really
				// necessary
				if ( Thread.CurrentThread.Interrupted )
				{
					 return;
				}
				_refreshJob = _scheduler.scheduleRecurring( Group.HZ_TOPOLOGY_REFRESH, _refreshPeriod, HazelcastCoreTopologyService.this.refreshTopology );
				Log.info( "Cluster discovery service started" );
			  });
			  _startingThread.Daemon = true;
			  _startingThread.Name = "HZ Starting Thread";
			  _startingThread.Start();
		 }

		 public override void Stop0()
		 {
			  Log.info( string.Format( "HazelcastCoreTopologyService stopping and unbinding from {0}", Config.get( discovery_listen_address ) ) );

			  // Interrupt the starting thread. Not really necessary, just cleaner exit
			  _startingThread.Interrupt();
			  // Flag to notify waiters
			  _stopped = true;

			  if ( _refreshJob != null )
			  {
					_refreshJob.cancel( true );
			  }

			  if ( _hazelcastInstance != null )
			  {
					try
					{
						 _hazelcastInstance.LifecycleService.shutdown();
					}
					catch ( Exception e )
					{
						 Log.warn( "Failed to stop Hazelcast", e );
					}
			  }
		 }

		 public override void Shutdown0()
		 {
			  // nothing to do
		 }

		 private HazelcastInstance CreateHazelcastInstance()
		 {
			  JoinConfig joinConfig = new JoinConfig();
			  joinConfig.MulticastConfig.Enabled = false;
			  TcpIpConfig tcpIpConfig = joinConfig.TcpIpConfig;
			  tcpIpConfig.Enabled = true;

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  ICollection<string> initialMembers = _remoteMembersResolver.resolve( SocketAddress::toString );

			  initialMembers.forEach( tcpIpConfig.addMember );

			  ListenSocketAddress hazelcastAddress = Config.get( discovery_listen_address );
			  NetworkConfig networkConfig = new NetworkConfig();

			  if ( !hazelcastAddress.Wildcard )
			  {
					InterfacesConfig interfaces = new InterfacesConfig();
					interfaces.addInterface( hazelcastAddress.Hostname );
					interfaces.Enabled = true;
					networkConfig.Interfaces = interfaces;
			  }

			  networkConfig.Port = hazelcastAddress.Port;
			  networkConfig.Join = joinConfig;
			  networkConfig.PortAutoIncrement = false;

			  // We'll use election_timeout as a base value to calculate HZ timeouts. We multiply by 1.5
			  long? electionTimeoutMillis = Config.get( CausalClusteringSettings.leader_election_timeout ).toMillis();
			  long? baseHazelcastTimeoutMillis = ( 3 * electionTimeoutMillis ) / 2;
			  /*
			   * Some HZ settings require the value in seconds. Adding the divider and subtracting 1 is equivalent to the
			   * ceiling function for integers ( Math.ceil() returns double ). Anything < 0 will return 0, any
			   * multiple of 1000 returns the result of the division by 1000, any non multiple of 1000 returns the result
			   * of the division + 1. In other words, values in millis are rounded up.
			   */
			  long baseHazelcastTimeoutSeconds = ( baseHazelcastTimeoutMillis + 1000 - 1 ) / 1000;

			  com.hazelcast.config.Config c = new com.hazelcast.config.Config();
			  c.setProperty( OPERATION_CALL_TIMEOUT_MILLIS.Name, baseHazelcastTimeoutMillis.ToString() );
			  c.setProperty( MERGE_NEXT_RUN_DELAY_SECONDS.Name, baseHazelcastTimeoutSeconds.ToString() );
			  c.setProperty( MERGE_FIRST_RUN_DELAY_SECONDS.Name, baseHazelcastTimeoutSeconds.ToString() );
			  c.setProperty( INITIAL_MIN_CLUSTER_SIZE.Name, HAZELCAST_MIN_CLUSTER.ToString() );

			  if ( Config.get( disable_middleware_logging ) )
			  {
					c.setProperty( LOGGING_TYPE.Name, "none" );
			  }

			  if ( hazelcastAddress.IPv6 )
			  {
					c.setProperty( PREFER_IPv4_STACK.Name, "false" );
			  }

			  c.NetworkConfig = networkConfig;

			  MemberAttributeConfig memberAttributeConfig = HazelcastClusterTopology.BuildMemberAttributesForCore( MyselfConflict, Config );

			  c.MemberAttributeConfig = memberAttributeConfig;
			  LogConnectionInfo( initialMembers );
			  c.addListenerConfig( new ListenerConfig( new OurMembershipListener( this ) ) );

			  JobHandle logJob = _scheduler.schedule( Group.HZ_TOPOLOGY_HEALTH, _hazelcastIsHealthyTimeoutMs, () => Log.warn("The server has not been able to connect in a timely fashion to the " + "cluster. Please consult the logs for more details. Rebooting the server may " + "solve the problem.") );

			  try
			  {
					_hazelcastInstance = Hazelcast.newHazelcastInstance( c );
					logJob.Cancel( true );
			  }
			  catch ( HazelcastException e )
			  {
					string errorMessage = string.Format( "Hazelcast was unable to start with setting: {0} = {1}", discovery_listen_address.name(), Config.get(discovery_listen_address) );
					UserLog.error( errorMessage );
					Log.error( errorMessage, e );
					throw new Exception( e );
			  }

			  IList<string> groups = Config.get( CausalClusteringSettings.server_groups );
			  refreshGroups( _hazelcastInstance, MyselfConflict.Uuid.ToString(), groups );

			  return _hazelcastInstance;
		 }

		 private void LogConnectionInfo( ICollection<string> initialMembers )
		 {
			  UserLog.info( "My connection info: " + "[\n\tDiscovery:   listen=%s, advertised=%s," + "\n\tTransaction: listen=%s, advertised=%s, " + "\n\tRaft:        listen=%s, advertised=%s, " + "\n\tClient Connector Addresses: %s" + "\n]", Config.get( discovery_listen_address ), Config.get( CausalClusteringSettings.discovery_advertised_address ), Config.get( CausalClusteringSettings.transaction_listen_address ), Config.get( CausalClusteringSettings.transaction_advertised_address ), Config.get( CausalClusteringSettings.raft_listen_address ), Config.get( CausalClusteringSettings.raft_advertised_address ), ClientConnectorAddresses.ExtractFromConfig( Config ) );
			  UserLog.info( "Discovering other core members in initial members set: " + initialMembers );
		 }

		 public override string LocalDBName()
		 {
			  return _localDBName;
		 }

		 public override CoreTopology AllCoreServers()
		 {
			  return _coreTopology;
		 }

		 public override CoreTopology LocalCoreServers()
		 {
			  return _localCoreTopology;
		 }

		 public override ReadReplicaTopology AllReadReplicas()
		 {
			  return _readReplicaTopology;
		 }

		 public override ReadReplicaTopology LocalReadReplicas()
		 {
			  return _localReadReplicaTopology;
		 }

		 public override Optional<AdvertisedSocketAddress> FindCatchupAddress( MemberId memberId )
		 {
			  return _topologyServiceRetryStrategy.apply( memberId, this.retrieveSocketAddress, Optional.isPresent );
		 }

		 private Optional<AdvertisedSocketAddress> RetrieveSocketAddress( MemberId memberId )
		 {
			  return Optional.ofNullable( _catchupAddressMap[memberId] );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void refreshRoles() throws InterruptedException
		 private void RefreshRoles()
		 {
			  WaitOnHazelcastInstanceCreation();
			  LeaderInfo localLeaderInfo = _leaderInfo.get();
			  Optional<LeaderInfo> localStepDownInfo = _stepDownInfo.get();

			  if ( localStepDownInfo.Present )
			  {
					HazelcastClusterTopology.CasLeaders( _hazelcastInstance, localStepDownInfo.get(), _localDBName, Log );
					_stepDownInfo.compareAndSet( localStepDownInfo, null );
			  }
			  else if ( localLeaderInfo.MemberId() != null && localLeaderInfo.MemberId().Equals(MyselfConflict) )
			  {
					HazelcastClusterTopology.CasLeaders( _hazelcastInstance, localLeaderInfo, _localDBName, Log );
			  }

			  _coreRoles = HazelcastClusterTopology.GetCoreRoles( _hazelcastInstance, AllCoreServers().members().Keys );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private synchronized void refreshTopology() throws InterruptedException
		 private void RefreshTopology()
		 {
			 lock ( this )
			 {
				  RefreshCoreTopology();
				  RefreshReadReplicaTopology();
				  RefreshRoles();
				  _catchupAddressMap = extractCatchupAddressesMap( LocalCoreServers(), LocalReadReplicas() );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void refreshCoreTopology() throws InterruptedException
		 private void RefreshCoreTopology()
		 {
			  WaitOnHazelcastInstanceCreation();

			  CoreTopology newCoreTopology = getCoreTopology( _hazelcastInstance, Config, Log );
			  TopologyDifference difference = _coreTopology.difference( newCoreTopology );

			  _coreTopology = newCoreTopology;
			  _localCoreTopology = newCoreTopology.FilterTopologyByDb( _localDBName );

			  if ( difference.HasChanges() )
			  {
					Log.info( "Core topology changed %s", difference );
					ListenerService.notifyListeners( _coreTopology );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void refreshReadReplicaTopology() throws InterruptedException
		 private void RefreshReadReplicaTopology()
		 {
			  WaitOnHazelcastInstanceCreation();

			  ReadReplicaTopology newReadReplicaTopology = getReadReplicaTopology( _hazelcastInstance, Log );
			  TopologyDifference difference = _readReplicaTopology.difference( newReadReplicaTopology );

			  this._readReplicaTopology = newReadReplicaTopology;
			  this._localReadReplicaTopology = newReadReplicaTopology.FilterTopologyByDb( _localDBName );

			  if ( difference.HasChanges() )
			  {
					Log.info( "Read replica topology changed %s", difference );
			  }
		 }

		 /*
		  * Waits for hazelcastInstance to be set. It also checks for the stopped flag which is probably not really
		  * necessary. Nevertheless, since hazelcastInstance is created and set by a separate thread to avoid blocking
		  * ( see start() ), all accesses to it must be guarded by this method.
		  */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void waitOnHazelcastInstanceCreation() throws InterruptedException
		 private void WaitOnHazelcastInstanceCreation()
		 {
			  while ( _hazelcastInstance == null && !_stopped )
			  {
					Thread.Sleep( 200 );
			  }
		 }

		 private class OurMembershipListener : MembershipListener
		 {
			 private readonly HazelcastCoreTopologyService _outerInstance;

			 public OurMembershipListener( HazelcastCoreTopologyService outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void MemberAdded( MembershipEvent membershipEvent )
			  {
					if ( !membershipEvent.Member.localMember() )
					{
						 Address address = membershipEvent.Member.Address;
						 outerInstance.monitor.DiscoveredMember( new SocketAddress( address.Host, address.Port ) );
					}

					try
					{
						 outerInstance.refreshTopology();
					}
					catch ( InterruptedException e )
					{
						 throw new Exception( e );
					}
			  }

			  public override void MemberRemoved( MembershipEvent membershipEvent )
			  {
					if ( !membershipEvent.Member.localMember() )
					{
						 Address address = membershipEvent.Member.Address;
						 outerInstance.monitor.LostMember( new SocketAddress( address.Host, address.Port ) );
					}

					try
					{
						 outerInstance.refreshTopology();
					}
					catch ( InterruptedException e )
					{
						 throw new Exception( e );
					}
			  }

			  public override void MemberAttributeChanged( MemberAttributeEvent memberAttributeEvent )
			  {
					outerInstance.Log.info( "Core member attribute changed %s", memberAttributeEvent );
			  }
		 }
	}

}