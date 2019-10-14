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
namespace Neo4Net.causalclustering.discovery
{

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using RobustJobSchedulerWrapper = Neo4Net.causalclustering.helper.RobustJobSchedulerWrapper;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Config = Neo4Net.Kernel.configuration.Config;
	using SafeLifecycle = Neo4Net.Kernel.Lifecycle.SafeLifecycle;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.READ_REPLICAS_DB_NAME_MAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.READ_REPLICA_BOLT_ADDRESS_MAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.READ_REPLICA_MEMBER_ID_MAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.READ_REPLICA_TRANSACTION_SERVER_ADDRESS_MAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.extractCatchupAddressesMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.getCoreTopology;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.getReadReplicaTopology;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.refreshGroups;

	public class HazelcastClient : SafeLifecycle, TopologyService
	{
		 private readonly Log _log;
		 private readonly ClientConnectorAddresses _connectorAddresses;
		 private readonly RobustHazelcastWrapper _hzInstance;
		 private readonly RobustJobSchedulerWrapper _scheduler;
		 private readonly Config _config;
		 private readonly long _timeToLive;
		 private readonly long _refreshPeriod;
		 private readonly AdvertisedSocketAddress _transactionSource;
		 private readonly MemberId _myself;
		 private readonly IList<string> _groups;
		 private readonly TopologyServiceRetryStrategy _topologyServiceRetryStrategy;

		 //TODO: Work out error handling in case cluster hosts change their dbName unexpectedly
		 private readonly string _dbName;

		 private JobHandle _keepAliveJob;
		 private JobHandle _refreshTopologyJob;

		 /* cached data updated during each refresh */
		 private volatile CoreTopology _coreTopology = CoreTopology.Empty;
		 private volatile CoreTopology _localCoreTopology = CoreTopology.Empty;
		 private volatile ReadReplicaTopology _readReplicaTopology = ReadReplicaTopology.Empty;
		 private volatile ReadReplicaTopology _localReadReplicaTopology = ReadReplicaTopology.Empty;
		 private volatile IDictionary<MemberId, AdvertisedSocketAddress> _catchupAddressMap = new Dictionary<MemberId, AdvertisedSocketAddress>();
		 private volatile IDictionary<MemberId, RoleInfo> _coreRoles;

		 public HazelcastClient( HazelcastConnector connector, JobScheduler scheduler, LogProvider logProvider, Config config, MemberId myself )
		 {
			  this._hzInstance = new RobustHazelcastWrapper( connector );
			  this._config = config;
			  this._log = logProvider.getLog( this.GetType() );
			  this._scheduler = new RobustJobSchedulerWrapper( scheduler, _log );
			  this._connectorAddresses = ClientConnectorAddresses.ExtractFromConfig( config );
			  this._transactionSource = config.Get( CausalClusteringSettings.transaction_advertised_address );
			  this._timeToLive = config.Get( CausalClusteringSettings.read_replica_time_to_live ).toMillis();
			  this._refreshPeriod = config.Get( CausalClusteringSettings.cluster_topology_refresh ).toMillis();
			  this._myself = myself;
			  this._groups = config.Get( CausalClusteringSettings.server_groups );
			  this._topologyServiceRetryStrategy = ResolveStrategy( _refreshPeriod, logProvider );
			  this._dbName = config.Get( CausalClusteringSettings.database );
			  this._coreRoles = emptyMap();
		 }

		 private static TopologyServiceRetryStrategy ResolveStrategy( long refreshPeriodMillis, LogProvider logProvider )
		 {
			  int pollingFrequencyWithinRefreshWindow = 2;
			  int numberOfRetries = pollingFrequencyWithinRefreshWindow + 1; // we want to have more retries at the given frequency than there is time in a refresh period
			  return new TopologyServiceMultiRetryStrategy( refreshPeriodMillis / pollingFrequencyWithinRefreshWindow, numberOfRetries, logProvider );
		 }

		 public override IDictionary<MemberId, RoleInfo> AllCoreRoles()
		 {
			  return _coreRoles;
		 }

		 public override MemberId Myself()
		 {
			  return _myself;
		 }

		 public override string LocalDBName()
		 {
			  return _dbName;
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

		 /// <summary>
		 /// Caches the topology so that the lookups are fast.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void refreshTopology() throws HazelcastInstanceNotActiveException
		 private void RefreshTopology()
		 {
			  CoreTopology newCoreTopology = _hzInstance.apply( hz => getCoreTopology( hz, _config, _log ) );
			  _coreTopology = newCoreTopology;
			  _localCoreTopology = newCoreTopology.FilterTopologyByDb( _dbName );

			  ReadReplicaTopology newReadReplicaTopology = _hzInstance.apply( hz => getReadReplicaTopology( hz, _log ) );
			  _readReplicaTopology = newReadReplicaTopology;
			  _localReadReplicaTopology = newReadReplicaTopology.FilterTopologyByDb( _dbName );

			  _catchupAddressMap = extractCatchupAddressesMap( LocalCoreServers(), LocalReadReplicas() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void refreshRoles() throws HazelcastInstanceNotActiveException
		 private void RefreshRoles()
		 {
			  _coreRoles = _hzInstance.apply( hz => HazelcastClusterTopology.GetCoreRoles( hz, AllCoreServers().members().Keys ) );
		 }

		 public override void Init0()
		 {
			  // nothing to do
		 }

		 public override void Start0()
		 {
			  _keepAliveJob = _scheduler.scheduleRecurring( Group.HZ_TOPOLOGY_KEEP_ALIVE, _timeToLive / 3, this.keepReadReplicaAlive );
			  _refreshTopologyJob = _scheduler.scheduleRecurring(Group.HZ_TOPOLOGY_REFRESH, _refreshPeriod, () =>
			  {
			  this.RefreshTopology();
			  this.RefreshRoles();
			  });
		 }

		 public override void Stop0()
		 {
			  _keepAliveJob.cancel( true );
			  _refreshTopologyJob.cancel( true );
			  DisconnectFromCore();
		 }

		 public override void Shutdown0()
		 {
			  // nothing to do
		 }

		 private void DisconnectFromCore()
		 {
			  try
			  {
					string uuid = _hzInstance.apply( _hzInstance => _hzInstance.LocalEndpoint.Uuid );
					_hzInstance.apply( hz => hz.getMap( READ_REPLICA_BOLT_ADDRESS_MAP ).remove( uuid ) );
					_hzInstance.shutdown();
			  }
			  catch ( Exception e )
			  {
					// Hazelcast is not able to stop correctly sometimes and throws a bunch of different exceptions
					// let's simply log the current problem but go on with our shutdown
					_log.warn( "Unable to shutdown hazelcast cleanly", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void keepReadReplicaAlive() throws HazelcastInstanceNotActiveException
		 private void KeepReadReplicaAlive()
		 {
			  _hzInstance.perform(hazelcastInstance =>
			  {
				string hzId = hazelcastInstance.LocalEndpoint.Uuid;
				string addresses = _connectorAddresses.ToString();
				_log.debug( "Adding read replica into cluster (%s -> %s)", hzId, addresses );

				hazelcastInstance.getMap( READ_REPLICAS_DB_NAME_MAP ).put( hzId, _dbName, _timeToLive, MILLISECONDS );

				hazelcastInstance.getMap( READ_REPLICA_TRANSACTION_SERVER_ADDRESS_MAP ).put( hzId, _transactionSource.ToString(), _timeToLive, MILLISECONDS );

				hazelcastInstance.getMap( READ_REPLICA_MEMBER_ID_MAP ).put( hzId, _myself.Uuid.ToString(), _timeToLive, MILLISECONDS );

				refreshGroups( hazelcastInstance, hzId, _groups );

				// this needs to be last as when we read from it in HazelcastClusterTopology.readReplicas
				// we assume that all the other maps have been populated if an entry exists in this one
				hazelcastInstance.getMap( READ_REPLICA_BOLT_ADDRESS_MAP ).put( hzId, addresses, _timeToLive, MILLISECONDS );
			  });
		 }
	}

}