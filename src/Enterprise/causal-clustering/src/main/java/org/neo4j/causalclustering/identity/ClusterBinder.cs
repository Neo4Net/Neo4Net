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
namespace Neo4Net.causalclustering.identity
{
	using OperationTimeoutException = com.hazelcast.core.OperationTimeoutException;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using CoreBootstrapper = Neo4Net.causalclustering.core.state.CoreBootstrapper;
	using CoreSnapshot = Neo4Net.causalclustering.core.state.snapshot.CoreSnapshot;
	using Neo4Net.causalclustering.core.state.storage;
	using CoreTopology = Neo4Net.causalclustering.discovery.CoreTopology;
	using CoreTopologyService = Neo4Net.causalclustering.discovery.CoreTopologyService;
	using Neo4Net.Functions;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;

	public class ClusterBinder : System.Func<Optional<ClusterId>>
	{
		 public interface Monitor
		 {
			  void WaitingForCoreMembers( int minimumCount );

			  void WaitingForBootstrap();

			  void Bootstrapped( CoreSnapshot snapshot, ClusterId clusterId );

			  void BoundToCluster( ClusterId clusterId );
		 }

		 private readonly SimpleStorage<ClusterId> _clusterIdStorage;
		 private readonly SimpleStorage<DatabaseName> _dbNameStorage;
		 private readonly CoreTopologyService _topologyService;
		 private readonly CoreBootstrapper _coreBootstrapper;
		 private readonly Monitor _monitor;
		 private readonly Clock _clock;
		 private readonly ThrowingAction<InterruptedException> _retryWaiter;
		 private readonly Duration _timeout;
		 private readonly string _dbName;
		 private readonly int _minCoreHosts;

		 private ClusterId _clusterId;

		 public ClusterBinder( SimpleStorage<ClusterId> clusterIdStorage, SimpleStorage<DatabaseName> dbNameStorage, CoreTopologyService topologyService, Clock clock, ThrowingAction<InterruptedException> retryWaiter, Duration timeout, CoreBootstrapper coreBootstrapper, string dbName, int minCoreHosts, Monitors monitors )
		 {
			  this._monitor = monitors.NewMonitor( typeof( Monitor ) );
			  this._clusterIdStorage = clusterIdStorage;
			  this._dbNameStorage = dbNameStorage;
			  this._topologyService = topologyService;
			  this._coreBootstrapper = coreBootstrapper;
			  this._clock = clock;
			  this._retryWaiter = retryWaiter;
			  this._timeout = timeout;
			  this._dbName = dbName;
			  this._minCoreHosts = minCoreHosts;
		 }

		 /// <summary>
		 /// This method verifies if the local topology being returned by the discovery service is a viable cluster
		 /// and should be bootstrapped by this host.
		 /// 
		 /// If true, then a) the topology is sufficiently large to form a cluster; & b) this host can bootstrap for
		 /// its configured database.
		 /// </summary>
		 /// <param name="coreTopology"> the present state of the local topology, as reported by the discovery service. </param>
		 /// <returns> Whether or not coreTopology, in its current state, can form a viable cluster </returns>
		 private bool HostShouldBootstrapCluster( CoreTopology coreTopology )
		 {
			  int memberCount = coreTopology.Members().Count;
			  if ( memberCount < _minCoreHosts )
			  {
					_monitor.waitingForCoreMembers( _minCoreHosts );
					return false;
			  }
			  else if ( !coreTopology.CanBeBootstrapped() )
			  {
					_monitor.waitingForBootstrap();
					return false;
			  }
			  else
			  {
					return true;
			  }
		 }

		 /// <summary>
		 /// The cluster binding process tries to establish a common cluster ID. If there is no common cluster ID
		 /// then a single instance will eventually create one and publish it through the underlying topology service.
		 /// </summary>
		 /// <exception cref="IOException"> If there is an issue with I/O. </exception>
		 /// <exception cref="InterruptedException"> If the process gets interrupted. </exception>
		 /// <exception cref="TimeoutException"> If the process times out. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public BoundState bindToCluster() throws Throwable
		 public virtual BoundState BindToCluster()
		 {
			  DatabaseName newName = new DatabaseName( _dbName );

			  _dbNameStorage.writeOrVerify(newName, existing =>
			  {
			  if ( !newName.Equals( existing ) )
			  {
				  throw new System.InvalidOperationException( format( "Your configured database name has changed. Found %s but expected %s in %s.", _dbName, existing.name(), CausalClusteringSettings.database.name() ) );
			  }
			  });

			  long endTime = _clock.millis() + _timeout.toMillis();
			  bool shouldRetryPublish = false;

			  if ( _clusterIdStorage.exists() )
			  {
					_clusterId = _clusterIdStorage.readState();
					do
					{
						 shouldRetryPublish = PublishClusterId( _clusterId );
					} while ( shouldRetryPublish && _clock.millis() < endTime );
					_monitor.boundToCluster( _clusterId );
					return new BoundState( _clusterId );
			  }

			  CoreSnapshot snapshot = null;
			  CoreTopology topology;

			  do
			  {
					topology = _topologyService.localCoreServers();

					if ( topology.ClusterId() != null )
					{
						 _clusterId = topology.ClusterId();
						 _monitor.boundToCluster( _clusterId );
					}
					else if ( HostShouldBootstrapCluster( topology ) )
					{
						 _clusterId = new ClusterId( System.Guid.randomUUID() );
						 snapshot = _coreBootstrapper.bootstrap( topology.Members().Keys );
						 _monitor.bootstrapped( snapshot, _clusterId );
						 shouldRetryPublish = PublishClusterId( _clusterId );
					}

					_retryWaiter.apply();

			  } while ( ( _clusterId == null || shouldRetryPublish ) && _clock.millis() < endTime );

			  if ( _clusterId == null || shouldRetryPublish )
			  {
					throw new TimeoutException( format( "Failed to join a cluster with members %s. Another member should have published " + "a clusterId but none was detected. Please restart the cluster.", topology ) );
			  }

			  _clusterIdStorage.writeState( _clusterId );
			  return new BoundState( _clusterId, snapshot );
		 }

		 public override Optional<ClusterId> Get()
		 {
			  return Optional.ofNullable( _clusterId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean publishClusterId(ClusterId localClusterId) throws BindingException, InterruptedException
		 private bool PublishClusterId( ClusterId localClusterId )
		 {
			  bool shouldRetry = false;
			  try
			  {
					bool success = _topologyService.setClusterId( localClusterId, _dbName );

					if ( !success )
					{
						 throw new BindingException( "Failed to publish: " + localClusterId );
					}
			  }
			  catch ( OperationTimeoutException )
			  {
					shouldRetry = true;
			  }

			  return shouldRetry;
		 }
	}

}