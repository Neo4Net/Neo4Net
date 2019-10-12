/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.causalclustering.core.state
{

	using Org.Neo4j.causalclustering.core.state.storage;
	using Org.Neo4j.causalclustering.core.state.storage;
	using CoreTopologyService = Org.Neo4j.causalclustering.discovery.CoreTopologyService;
	using DiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.DiscoveryServiceFactory;
	using RemoteMembersResolver = Org.Neo4j.causalclustering.discovery.RemoteMembersResolver;
	using TopologyServiceMultiRetryStrategy = Org.Neo4j.causalclustering.discovery.TopologyServiceMultiRetryStrategy;
	using TopologyServiceRetryStrategy = Org.Neo4j.causalclustering.discovery.TopologyServiceRetryStrategy;
	using ClusterBinder = Org.Neo4j.causalclustering.identity.ClusterBinder;
	using ClusterId = Org.Neo4j.causalclustering.identity.ClusterId;
	using DatabaseName = Org.Neo4j.causalclustering.identity.DatabaseName;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using Clocks = Org.Neo4j.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Thread.sleep;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.server.CoreServerModule.CLUSTER_ID_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.server.CoreServerModule.DB_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.ResolutionResolverFactory.chooseResolver;

	public class ClusteringModule
	{
		 private readonly CoreTopologyService _topologyService;
		 private readonly ClusterBinder _clusterBinder;

		 public ClusteringModule( DiscoveryServiceFactory discoveryServiceFactory, MemberId myself, PlatformModule platformModule, File clusterStateDirectory, DatabaseLayout databaseLayout )
		 {
			  LifeSupport life = platformModule.Life;
			  Config config = platformModule.Config;
			  LogProvider logProvider = platformModule.Logging.InternalLogProvider;
			  LogProvider userLogProvider = platformModule.Logging.UserLogProvider;
			  Dependencies dependencies = platformModule.Dependencies;
			  Monitors monitors = platformModule.Monitors;
			  FileSystemAbstraction fileSystem = platformModule.FileSystem;
			  RemoteMembersResolver remoteMembersResolver = chooseResolver( config, platformModule.Logging );

			  _topologyService = discoveryServiceFactory.CoreTopologyService( config, myself, platformModule.JobScheduler, logProvider, userLogProvider, remoteMembersResolver, ResolveStrategy( config, logProvider ), monitors );

			  life.Add( _topologyService );

			  dependencies.SatisfyDependency( _topologyService ); // for tests

			  CoreBootstrapper coreBootstrapper = new CoreBootstrapper( databaseLayout, platformModule.PageCache, fileSystem, config, logProvider, platformModule.Monitors );

			  SimpleStorage<ClusterId> clusterIdStorage = new SimpleFileStorage<ClusterId>( fileSystem, clusterStateDirectory, CLUSTER_ID_NAME, new ClusterId.Marshal(), logProvider );

			  SimpleStorage<DatabaseName> dbNameStorage = new SimpleFileStorage<DatabaseName>( fileSystem, clusterStateDirectory, DB_NAME, new DatabaseName.Marshal(), logProvider );

			  string dbName = config.Get( CausalClusteringSettings.database );
			  int minimumCoreHosts = config.Get( CausalClusteringSettings.minimum_core_cluster_size_at_formation );

			  Duration clusterBindingTimeout = config.Get( CausalClusteringSettings.cluster_binding_timeout );
			  _clusterBinder = new ClusterBinder( clusterIdStorage, dbNameStorage, _topologyService, Clocks.systemClock(), () => sleep(100), clusterBindingTimeout, coreBootstrapper, dbName, minimumCoreHosts, platformModule.Monitors );
		 }

		 private static TopologyServiceRetryStrategy ResolveStrategy( Config config, LogProvider logProvider )
		 {
			  long refreshPeriodMillis = config.Get( CausalClusteringSettings.cluster_topology_refresh ).toMillis();
			  int pollingFrequencyWithinRefreshWindow = 2;
			  int numberOfRetries = pollingFrequencyWithinRefreshWindow + 1; // we want to have more retries at the given frequency than there is time in a refresh period
			  return new TopologyServiceMultiRetryStrategy( refreshPeriodMillis / pollingFrequencyWithinRefreshWindow, numberOfRetries, logProvider );
		 }

		 public virtual CoreTopologyService TopologyService()
		 {
			  return _topologyService;
		 }

		 public virtual System.Func<Optional<ClusterId>> ClusterIdentity()
		 {
			  return _clusterBinder;
		 }

		 public virtual ClusterBinder ClusterBinder()
		 {
			  return _clusterBinder;
		 }
	}

}