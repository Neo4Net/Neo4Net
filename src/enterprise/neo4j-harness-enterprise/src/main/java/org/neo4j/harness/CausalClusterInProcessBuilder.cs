using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Harness
{

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using Neo4Net.causalclustering.discovery;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using EnterpriseInProcessServerBuilder = Neo4Net.Harness.Internal.EnterpriseInProcessServerBuilder;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using HttpConnector = Neo4Net.Kernel.configuration.HttpConnector;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using EnterpriseEditionSettings = Neo4Net.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;

	public class CausalClusterInProcessBuilder
	{

		 public static WithCores Init()
		 {
			  return new Builder();
		 }

		 /// <summary>
		 /// Step Builder to ensure that Cluster has all the required pieces
		 /// TODO: Add mapping methods to allow for core hosts and replicas to be unevenly distributed  between databases
		 /// </summary>
		 public class Builder : WithCores, WithReplicas, WithLogger, WithPath, WithOptionalDatabasesAndPorts
		 {

			  internal int NumCoreHosts;
			  internal int NumReadReplicas;
			  internal Log Log;
			  internal Path Path;
			  internal PortPickingFactory PortFactory = PortPickingFactory.Default;
			  internal readonly IDictionary<string, string> Config = new Dictionary<string, string>();
			  internal IList<string> Databases = new List<string>( Collections.singletonList( "default" ) );
			  internal DiscoveryServiceFactorySelector.DiscoveryImplementation DiscoveryServiceFactory = DiscoveryServiceFactorySelector.DEFAULT;

			  public override WithReplicas WithCores( int n )
			  {
					NumCoreHosts = n;
					return this;
			  }

			  public override WithLogger WithReplicas( int n )
			  {
					NumReadReplicas = n;
					return this;
			  }

			  public override WithPath WithLogger( LogProvider l )
			  {
					Log = l.GetLog( "Neo4Net.harness.CausalCluster" );
					return this;
			  }

			  public virtual Builder WithConfig( string settingName, string value )
			  {
					Config[settingName] = value;
					return this;
			  }

			  public override Builder AtPath( Path p )
			  {
					Path = p;
					return this;
			  }

			  public override Builder WithOptionalPortsStrategy( PortPickingStrategy s )
			  {
					PortFactory = new PortPickingFactory( s );
					return this;
			  }

			  public override Builder WithOptionalDatabases( IList<string> databaseNames )
			  {
					if ( databaseNames.Count > 0 )
					{
						 Databases = databaseNames;
					}
					return this;
			  }

			  public override Builder WithDiscoveryServiceFactory( DiscoveryServiceFactorySelector.DiscoveryImplementation discoveryServiceFactory )
			  {
					this.DiscoveryServiceFactory = discoveryServiceFactory;
					return this;
			  }

			  public virtual CausalCluster Build()
			  {
					int nDatabases = Databases.Count;
					if ( nDatabases > NumCoreHosts )
					{
						 throw new System.ArgumentException( "You cannot have more databases than core hosts. Each database in the cluster must have at least 1 core " + "host. You have provided " + nDatabases + " databases and " + NumCoreHosts + " core hosts." );
					}
					return new CausalCluster( this );
			  }
		 }

		 /// <summary>
		 /// Builder step interfaces
		 /// </summary>
		 public interface WithCores
		 {
			  WithReplicas WithCores( int n );
		 }

		 public interface WithReplicas
		 {
			  WithLogger WithReplicas( int n );
		 }

		 public interface WithLogger
		 {
			  WithPath WithLogger( LogProvider l );
		 }

		 public interface WithPath
		 {
			  Builder AtPath( Path p );
		 }

		 internal interface WithOptionalDatabasesAndPorts
		 {
			  Builder WithOptionalPortsStrategy( PortPickingStrategy s );

			  Builder WithOptionalDatabases( IList<string> databaseNames );

			  Builder WithDiscoveryServiceFactory( DiscoveryServiceFactorySelector.DiscoveryImplementation discoveryServiceFactory );
		 }

		 /// <summary>
		 /// Port picker functional interface
		 /// </summary>
		 public interface PortPickingStrategy
		 {
			  int Port( int offset, int id );
		 }

		 /// <summary>
		 /// Port picker factory
		 /// </summary>
		 public sealed class PortPickingFactory
		 {
			  public static readonly PortPickingFactory Default = new PortPickingFactory( ( offset, id ) => offset + id );

			  internal readonly PortPickingStrategy St;

			  public PortPickingFactory( PortPickingStrategy st )
			  {
					this.St = st;
			  }

			  internal int HazelcastPort( int coreId )
			  {
					return St.port( 55000, coreId );
			  }

			  internal int TxCorePort( int coreId )
			  {
					return St.port( 56000, coreId );
			  }

			  internal int RaftCorePort( int coreId )
			  {
					return St.port( 57000, coreId );
			  }

			  internal int BoltCorePort( int coreId )
			  {
					return St.port( 58000, coreId );
			  }

			  internal int HttpCorePort( int coreId )
			  {
					return St.port( 59000, coreId );
			  }

			  internal int HttpsCorePort( int coreId )
			  {
					return St.port( 60000, coreId );
			  }

			  internal int TxReadReplicaPort( int replicaId )
			  {
					return St.port( 56500, replicaId );
			  }

			  internal int BoltReadReplicaPort( int replicaId )
			  {
					return St.port( 58500, replicaId );
			  }

			  internal int HttpReadReplicaPort( int replicaId )
			  {
					return St.port( 59500, replicaId );
			  }

			  internal int HttpsReadReplicaPort( int replicaId )
			  {
					return St.port( 60500, replicaId );
			  }
		 }

		 /// <summary>
		 /// Implementation of in process Cluster
		 /// </summary>
		 public class CausalCluster
		 {
			  internal readonly int NCores;
			  internal readonly int NReplicas;
			  internal readonly IList<string> DatabaseNames;
			  internal readonly Path ClusterPath;
			  internal readonly Log Log;
			  internal readonly PortPickingFactory PortFactory;
			  internal readonly IDictionary<string, string> Config;
			  internal readonly DiscoveryServiceFactorySelector.DiscoveryImplementation DiscoveryServiceFactory;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal IList<ServerControls> CoreControlsConflict = synchronizedList( new List<ServerControls>() );
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal IList<ServerControls> ReplicaControlsConflict = synchronizedList( new List<ServerControls>() );

			  internal CausalCluster( CausalClusterInProcessBuilder.Builder builder )
			  {
					this.NCores = builder.NumCoreHosts;
					this.NReplicas = builder.NumReadReplicas;
					this.ClusterPath = builder.Path;
					this.Log = builder.Log;
					this.PortFactory = builder.PortFactory;
					this.DatabaseNames = builder.Databases;
					this.Config = builder.Config;
					this.DiscoveryServiceFactory = builder.DiscoveryServiceFactory;
			  }

			  internal virtual IDictionary<int, string> DistributeHostsBetweenDatabases( int nHosts, IList<string> databases )
			  {
					//Max number of hosts per database is (nHosts / nDatabases) or (nHosts / nDatabases) + 1
					int nDatabases = databases.Count;
					int maxCapacity = ( nHosts % nDatabases == 0 ) ? ( nHosts / nDatabases ) : ( nHosts / nDatabases ) + 1;

					IList<string> repeated = databases.stream().flatMap(db => IntStream.range(0, maxCapacity).mapToObj(ignored => db)).collect(Collectors.toList());

					IDictionary<int, string> mapping = new Dictionary<int, string>( nHosts );

					for ( int hostId = 0; hostId < nHosts; hostId++ )
					{
						 mapping[hostId] = repeated[hostId];
					}
					return mapping;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void boot() throws InterruptedException
			  public virtual void Boot()
			  {
					IList<string> initialMembers = new List<string>( NCores );

					IDictionary<int, string> initialMembersToDatabase = DistributeHostsBetweenDatabases( NCores, DatabaseNames );

					for ( int coreId = 0; coreId < NCores; coreId++ )
					{
						 int hazelcastPort = PortFactory.hazelcastPort( coreId );
						 initialMembers.Add( "localhost:" + hazelcastPort );
					}

					IList<Thread> coreThreads = new List<Thread>();
					IList<Thread> replicaThreads = new List<Thread>();

					for ( int coreId = 0; coreId < NCores; coreId++ )
					{
						 int hazelcastPort = PortFactory.hazelcastPort( coreId );
						 int txPort = PortFactory.txCorePort( coreId );
						 int raftPort = PortFactory.raftCorePort( coreId );
						 int boltPort = PortFactory.boltCorePort( coreId );
						 int httpPort = PortFactory.httpCorePort( coreId );
						 int httpsPort = PortFactory.httpsCorePort( coreId );

						 string homeDir = "core-" + coreId;
						 EnterpriseInProcessServerBuilder builder = new EnterpriseInProcessServerBuilder( ClusterPath.toFile(), homeDir );

						 string homePath = Paths.get( ClusterPath.ToString(), homeDir ).toAbsolutePath().ToString();
						 builder.WithConfig( GraphDatabaseSettings.Neo4Net_home.name(), homePath );
						 builder.WithConfig( GraphDatabaseSettings.pagecache_memory.name(), "8m" );

						 builder.withConfig( EnterpriseEditionSettings.mode.name(), EnterpriseEditionSettings.Mode.CORE.name() );
						 builder.WithConfig( CausalClusteringSettings.multi_dc_license.name(), "true" );
						 builder.withConfig( CausalClusteringSettings.initial_discovery_members.name(), string.join(",", initialMembers) );

						 builder.WithConfig( CausalClusteringSettings.discovery_listen_address.name(), SpecifyPortOnly(hazelcastPort) );
						 builder.WithConfig( CausalClusteringSettings.transaction_listen_address.name(), SpecifyPortOnly(txPort) );
						 builder.WithConfig( CausalClusteringSettings.raft_listen_address.name(), SpecifyPortOnly(raftPort) );

						 builder.WithConfig( CausalClusteringSettings.database.name(), initialMembersToDatabase[coreId] );

						 builder.WithConfig( CausalClusteringSettings.minimum_core_cluster_size_at_formation.name(), NCores.ToString() );
						 builder.WithConfig( CausalClusteringSettings.minimum_core_cluster_size_at_runtime.name(), NCores.ToString() );
						 builder.WithConfig( CausalClusteringSettings.server_groups.name(), "core," + "core" + coreId );
						 ConfigureConnectors( boltPort, httpPort, httpsPort, builder );

						 builder.WithConfig( ServerSettings.jmx_module_enabled.name(), Settings.FALSE );

						 builder.WithConfig( OnlineBackupSettings.online_backup_enabled, Settings.FALSE );

						 Config.forEach( builder.withConfig );

						 builder.WithConfig( CausalClusteringSettings.discovery_implementation, DiscoveryServiceFactory.ToString() );

						 int finalCoreId = coreId;
						 Thread coreThread = new Thread(() =>
						 {
						  CoreControlsConflict.Add( builder.NewServer() );
						  Log.info( "Core " + finalCoreId + " started." );
						 });
						 coreThreads.Add( coreThread );
						 coreThread.Start();
					}

					foreach ( Thread coreThread in coreThreads )
					{
						 coreThread.Join();
					}

					IDictionary<int, string> replicasToDatabase = DistributeHostsBetweenDatabases( NReplicas, DatabaseNames );

					for ( int replicaId = 0; replicaId < NReplicas; replicaId++ )
					{
						 int txPort = PortFactory.txReadReplicaPort( replicaId );
						 int boltPort = PortFactory.boltReadReplicaPort( replicaId );
						 int httpPort = PortFactory.httpReadReplicaPort( replicaId );
						 int httpsPort = PortFactory.httpsReadReplicaPort( replicaId );

						 string homeDir = "replica-" + replicaId;
						 EnterpriseInProcessServerBuilder builder = new EnterpriseInProcessServerBuilder( ClusterPath.toFile(), homeDir );

						 string homePath = Paths.get( ClusterPath.ToString(), homeDir ).toAbsolutePath().ToString();
						 builder.WithConfig( GraphDatabaseSettings.Neo4Net_home.name(), homePath );
						 builder.WithConfig( GraphDatabaseSettings.pagecache_memory.name(), "8m" );

						 builder.withConfig( EnterpriseEditionSettings.mode.name(), EnterpriseEditionSettings.Mode.READ_REPLICA.name() );
						 builder.withConfig( CausalClusteringSettings.initial_discovery_members.name(), string.join(",", initialMembers) );
						 builder.WithConfig( CausalClusteringSettings.transaction_listen_address.name(), SpecifyPortOnly(txPort) );

						 builder.WithConfig( CausalClusteringSettings.database.name(), replicasToDatabase[replicaId] );

						 builder.WithConfig( CausalClusteringSettings.server_groups.name(), "replica," + "replica" + replicaId );
						 ConfigureConnectors( boltPort, httpPort, httpsPort, builder );

						 builder.WithConfig( ServerSettings.jmx_module_enabled.name(), Settings.FALSE );

						 builder.WithConfig( OnlineBackupSettings.online_backup_enabled, Settings.FALSE );

						 builder.WithConfig( CausalClusteringSettings.discovery_implementation, DiscoveryServiceFactory.ToString() );

						 Config.forEach( builder.withConfig );

						 int finalReplicaId = replicaId;
						 Thread replicaThread = new Thread(() =>
						 {
						  ReplicaControlsConflict.Add( builder.NewServer() );
						  Log.info( "Read replica " + finalReplicaId + " started." );
						 });
						 replicaThreads.Add( replicaThread );
						 replicaThread.Start();
					}

					foreach ( Thread replicaThread in replicaThreads )
					{
						 replicaThread.Join();
					}
			  }

			  internal static string SpecifyPortOnly( int port )
			  {
					return ":" + port;
			  }

			  internal static void ConfigureConnectors( int boltPort, int httpPort, int httpsPort, TestServerBuilder builder )
			  {
					builder.WithConfig( ( new BoltConnector( "bolt" ) ).type.name(), "BOLT" );
					builder.WithConfig( ( new BoltConnector( "bolt" ) ).enabled.name(), "true" );
					builder.WithConfig( ( new BoltConnector( "bolt" ) ).listen_address.name(), SpecifyPortOnly(boltPort) );
					builder.WithConfig( ( new BoltConnector( "bolt" ) ).advertised_address.name(), SpecifyPortOnly(boltPort) );

					builder.WithConfig( ( new HttpConnector( "http", HttpConnector.Encryption.NONE ) ).type.name(), "HTTP" );
					builder.WithConfig( ( new HttpConnector( "http", HttpConnector.Encryption.NONE ) ).enabled.name(), "true" );
					builder.WithConfig( ( new HttpConnector( "http", HttpConnector.Encryption.NONE ) ).listen_address.name(), SpecifyPortOnly(httpPort) );
					builder.WithConfig( ( new HttpConnector( "http", HttpConnector.Encryption.NONE ) ).advertised_address.name(), SpecifyPortOnly(httpPort) );

					builder.WithConfig( ( new HttpConnector( "https", HttpConnector.Encryption.TLS ) ).type.name(), "HTTP" );
					builder.WithConfig( ( new HttpConnector( "https", HttpConnector.Encryption.TLS ) ).enabled.name(), "true" );
					builder.WithConfig( ( new HttpConnector( "https", HttpConnector.Encryption.TLS ) ).listen_address.name(), SpecifyPortOnly(httpsPort) );
					builder.WithConfig( ( new HttpConnector( "https", HttpConnector.Encryption.TLS ) ).advertised_address.name(), SpecifyPortOnly(httpsPort) );
			  }

			  public virtual IList<ServerControls> CoreControls
			  {
				  get
				  {
						return CoreControlsConflict;
				  }
			  }

			  public virtual IList<ServerControls> ReplicaControls
			  {
				  get
				  {
						return ReplicaControlsConflict;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws InterruptedException
			  public virtual void Shutdown()
			  {
					ShutdownControls( ReplicaControlsConflict );
					ShutdownControls( CoreControlsConflict );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shutdownControls(Iterable<? extends ServerControls> controls) throws InterruptedException
			  internal virtual void ShutdownControls<T1>( IEnumerable<T1> controls ) where T1 : ServerControls
			  {
					ICollection<Thread> threads = new List<Thread>();
					foreach ( ServerControls control in controls )
					{
						 Thread thread = new Thread( control.close );
						 threads.Add( thread );
						 thread.Start();
					}

					foreach ( Thread thread in threads )
					{
						 thread.Join();
					}
			  }
		 }
	}

}