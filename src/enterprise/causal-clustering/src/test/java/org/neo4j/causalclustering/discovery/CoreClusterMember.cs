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
	using CoreGraphDatabase = Neo4Net.causalclustering.core.CoreGraphDatabase;
	using RaftMachine = Neo4Net.causalclustering.core.consensus.RaftMachine;
	using FileNames = Neo4Net.causalclustering.core.consensus.log.segmented.FileNames;
	using ClusterStateDirectory = Neo4Net.causalclustering.core.state.ClusterStateDirectory;
	using RaftLogPruner = Neo4Net.causalclustering.core.state.RaftLogPruner;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using GraphDatabaseDependencies = Neo4Net.Graphdb.facade.GraphDatabaseDependencies;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using Config = Neo4Net.Kernel.configuration.Config;
	using HttpConnector = Neo4Net.Kernel.configuration.HttpConnector;
	using Encryption = Neo4Net.Kernel.configuration.HttpConnector.Encryption;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using EnterpriseEditionSettings = Neo4Net.Kernel.impl.enterprise.configuration.EnterpriseEditionSettings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Level = Neo4Net.Logging.Level;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.log.RaftLog_Fields.RAFT_LOG_DIRECTORY_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.AdvertisedSocketAddress.advertisedAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.ListenSocketAddress.listenAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	public class CoreClusterMember : ClusterMember<CoreGraphDatabase>
	{
		 private readonly File _neo4jHome;
		 protected internal readonly DiscoveryServiceFactory DiscoveryServiceFactory;
		 private readonly File _defaultDatabaseDirectory;
		 private readonly File _clusterStateDir;
		 private readonly File _raftLogDir;
		 private readonly IDictionary<string, string> _config = stringMap();
		 private readonly int _serverId;
		 private readonly string _boltAdvertisedSocketAddress;
		 private readonly int _discoveryPort;
		 private readonly string _raftListenAddress;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal CoreGraphDatabase DatabaseConflict;
		 private readonly Config _memberConfig;
		 private readonly ThreadGroup _threadGroup;
		 private readonly Monitors _monitors = new Monitors();
		 private readonly string _dbName;
		 private readonly File _databasesDirectory;

		 public CoreClusterMember( int serverId, int discoveryPort, int txPort, int raftPort, int boltPort, int httpPort, int backupPort, int clusterSize, IList<AdvertisedSocketAddress> addresses, DiscoveryServiceFactory discoveryServiceFactory, string recordFormat, File parentDir, IDictionary<string, string> extraParams, IDictionary<string, System.Func<int, string>> instanceExtraParams, string listenAddress, string advertisedAddress )
		 {
			  this._serverId = serverId;

			  this._discoveryPort = discoveryPort;

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  string initialMembers = addresses.Select( AdvertisedSocketAddress::toString ).collect( joining( "," ) );
			  _boltAdvertisedSocketAddress = advertisedAddress( advertisedAddress, boltPort );
			  _raftListenAddress = listenAddress( listenAddress, raftPort );

			  _config[EnterpriseEditionSettings.mode.name()] = EnterpriseEditionSettings.Mode.CORE.name();
			  _config[GraphDatabaseSettings.default_advertised_address.name()] = advertisedAddress;
			  _config[CausalClusteringSettings.initial_discovery_members.name()] = initialMembers;
			  _config[CausalClusteringSettings.discovery_listen_address.name()] = listenAddress(listenAddress, discoveryPort);
			  _config[CausalClusteringSettings.discovery_advertised_address.name()] = advertisedAddress(advertisedAddress, discoveryPort);
			  _config[CausalClusteringSettings.transaction_listen_address.name()] = listenAddress(listenAddress, txPort);
			  _config[CausalClusteringSettings.raft_listen_address.name()] = _raftListenAddress;
			  _config[CausalClusteringSettings.cluster_topology_refresh.name()] = "1000ms";
			  _config[CausalClusteringSettings.minimum_core_cluster_size_at_formation.name()] = clusterSize.ToString();
			  _config[CausalClusteringSettings.minimum_core_cluster_size_at_runtime.name()] = clusterSize.ToString();
			  _config[CausalClusteringSettings.leader_election_timeout.name()] = "500ms";
			  _config[CausalClusteringSettings.raft_messages_log_enable.name()] = Settings.TRUE;
			  _config[GraphDatabaseSettings.store_internal_log_level.name()] = Level.DEBUG.name();
			  _config[GraphDatabaseSettings.record_format.name()] = recordFormat;
			  _config[( new BoltConnector( "bolt" ) ).type.name()] = "BOLT";
			  _config[( new BoltConnector( "bolt" ) ).enabled.name()] = "true";
			  _config[( new BoltConnector( "bolt" ) ).listen_address.name()] = listenAddress(listenAddress, boltPort);
			  _config[( new BoltConnector( "bolt" ) ).advertised_address.name()] = _boltAdvertisedSocketAddress;
			  _config[( new HttpConnector( "http", HttpConnector.Encryption.NONE ) ).type.name()] = "HTTP";
			  _config[( new HttpConnector( "http", HttpConnector.Encryption.NONE ) ).enabled.name()] = "true";
			  _config[( new HttpConnector( "http", HttpConnector.Encryption.NONE ) ).listen_address.name()] = listenAddress(listenAddress, httpPort);
			  _config[( new HttpConnector( "http", HttpConnector.Encryption.NONE ) ).advertised_address.name()] = advertisedAddress(advertisedAddress, httpPort);
			  _config[OnlineBackupSettings.online_backup_server.name()] = listenAddress(listenAddress, backupPort);
			  _config[GraphDatabaseSettings.pagecache_memory.name()] = "8m";
			  _config[GraphDatabaseSettings.auth_store.name()] = (new File(parentDir, "auth")).AbsolutePath;
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  _config.putAll( extraParams );

			  foreach ( KeyValuePair<string, System.Func<int, string>> entry in instanceExtraParams.SetOfKeyValuePairs() )
			  {
					_config[entry.Key] = entry.Value.apply( serverId );
			  }

			  this._neo4jHome = new File( parentDir, "server-core-" + serverId );
			  _config[GraphDatabaseSettings.neo4j_home.name()] = _neo4jHome.AbsolutePath;
			  _config[GraphDatabaseSettings.logs_directory.name()] = (new File(_neo4jHome, "logs")).AbsolutePath;
			  _config[GraphDatabaseSettings.logical_logs_location.name()] = "core-tx-logs-" + serverId;

			  this.DiscoveryServiceFactory = discoveryServiceFactory;
			  File dataDir = new File( _neo4jHome, "data" );
			  _clusterStateDir = ClusterStateDirectory.withoutInitializing( dataDir ).get();
			  _raftLogDir = new File( _clusterStateDir, RAFT_LOG_DIRECTORY_NAME );
			  _databasesDirectory = new File( dataDir, "databases" );
			  _defaultDatabaseDirectory = new File( _databasesDirectory, GraphDatabaseSettings.DEFAULT_DATABASE_NAME );
			  _memberConfig = Config.defaults( _config );

			  this._dbName = _memberConfig.get( CausalClusteringSettings.database );

			  //noinspection ResultOfMethodCallIgnored
			  _defaultDatabaseDirectory.mkdirs();
			  _threadGroup = new ThreadGroup( ToString() );
		 }

		 public virtual string BoltAdvertisedAddress()
		 {
			  return _boltAdvertisedSocketAddress;
		 }

		 public virtual string RoutingURI()
		 {
			  return string.Format( "bolt+routing://{0}", _boltAdvertisedSocketAddress );
		 }

		 public virtual string DirectURI()
		 {
			  return string.Format( "bolt://{0}", _boltAdvertisedSocketAddress );
		 }

		 public virtual string RaftListenAddress()
		 {
			  return _raftListenAddress;
		 }

		 public override void Start()
		 {
			  DatabaseConflict = new CoreGraphDatabase( _databasesDirectory, _memberConfig, GraphDatabaseDependencies.newDependencies().monitors(_monitors), DiscoveryServiceFactory );
		 }

		 public override void Shutdown()
		 {
			  if ( DatabaseConflict != null )
			  {
					try
					{
						 DatabaseConflict.shutdown();
					}
					finally
					{
						 DatabaseConflict = null;
					}
			  }
		 }

		 public virtual bool Shutdown
		 {
			 get
			 {
				  return DatabaseConflict == null;
			 }
		 }

		 public override CoreGraphDatabase Database()
		 {
			  return DatabaseConflict;
		 }

		 public override File DatabaseDirectory()
		 {
			  return _defaultDatabaseDirectory;
		 }

		 public virtual File DatabasesDirectory()
		 {
			  return _databasesDirectory;
		 }

		 public virtual RaftLogPruner RaftLogPruner()
		 {
			  return DatabaseConflict.DependencyResolver.resolveDependency( typeof( RaftLogPruner ) );
		 }

		 public virtual RaftMachine Raft()
		 {
			  return DatabaseConflict.DependencyResolver.resolveDependency( typeof( RaftMachine ) );
		 }

		 public virtual MemberId Id()
		 {
			  return DatabaseConflict.DependencyResolver.resolveDependency( typeof( RaftMachine ) ).identity();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.SortedMap<long, java.io.File> getLogFileNames() throws java.io.IOException
		 public virtual SortedDictionary<long, File> LogFileNames
		 {
			 get
			 {
				  File logFilesDir = new File( _clusterStateDir, RAFT_LOG_DIRECTORY_NAME );
				  using ( DefaultFileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
				  {
						return ( new FileNames( logFilesDir ) ).getAllFiles( fileSystem, null );
				  }
			 }
		 }

		 public override File HomeDir()
		 {
			  return _neo4jHome;
		 }

		 public override string ToString()
		 {
			  return format( "CoreClusterMember{serverId=%d}", _serverId );
		 }

		 public override int ServerId()
		 {
			  return _serverId;
		 }

		 public virtual string DbName()
		 {
			  return _dbName;
		 }

		 public override ClientConnectorAddresses ClientConnectorAddresses()
		 {
			  return ClientConnectorAddresses.ExtractFromConfig( Config.defaults( this._config ) );
		 }

		 public override string SettingValue( string settingName )
		 {
			  return _config[settingName];
		 }

		 public override Config Config()
		 {
			  return _memberConfig;
		 }

		 public override ThreadGroup ThreadGroup()
		 {
			  return _threadGroup;
		 }

		 public override Monitors Monitors()
		 {
			  return _monitors;
		 }

		 public virtual File ClusterStateDirectory()
		 {
			  return _clusterStateDir;
		 }

		 public virtual File RaftLogDirectory()
		 {
			  return _raftLogDir;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void disableCatchupServer() throws Throwable
		 public virtual void DisableCatchupServer()
		 {
			  DatabaseConflict.disableCatchupServer();
		 }

		 internal virtual int DiscoveryPort()
		 {
			  return _discoveryPort;
		 }
	}

}