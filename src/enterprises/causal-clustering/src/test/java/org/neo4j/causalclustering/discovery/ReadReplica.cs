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

	using CatchupPollingProcess = Neo4Net.causalclustering.catchup.tx.CatchupPollingProcess;
	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using ReadReplicaGraphDatabase = Neo4Net.causalclustering.readreplica.ReadReplicaGraphDatabase;
	using GraphDatabaseDependencies = Neo4Net.Graphdb.facade.GraphDatabaseDependencies;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using Config = Neo4Net.Kernel.configuration.Config;
	using HttpConnector = Neo4Net.Kernel.configuration.HttpConnector;
	using Encryption = Neo4Net.Kernel.configuration.HttpConnector.Encryption;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Level = Neo4Net.Logging.Level;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.AdvertisedSocketAddress.advertisedAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.ListenSocketAddress.listenAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public class ReadReplica implements ClusterMember<org.neo4j.causalclustering.readreplica.ReadReplicaGraphDatabase>
	public class ReadReplica : ClusterMember<ReadReplicaGraphDatabase>
	{
		 protected internal readonly DiscoveryServiceFactory DiscoveryServiceFactory;
		 private readonly File _neo4jHome;
		 protected internal readonly File DefaultDatabaseDirectory;
		 private readonly int _serverId;
		 private readonly string _boltAdvertisedSocketAddress;
		 private readonly Config _memberConfig;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal ReadReplicaGraphDatabase DatabaseConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal Monitors MonitorsConflict;
		 private readonly ThreadGroup _threadGroup;
		 protected internal readonly File DatabasesDirectory;

		 public ReadReplica( File parentDir, int serverId, int boltPort, int httpPort, int txPort, int backupPort, int discoveryPort, DiscoveryServiceFactory discoveryServiceFactory, IList<AdvertisedSocketAddress> coreMemberDiscoveryAddresses, IDictionary<string, string> extraParams, IDictionary<string, System.Func<int, string>> instanceExtraParams, string recordFormat, Monitors monitors, string advertisedAddress, string listenAddress )
		 {
			  this._serverId = serverId;

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  string initialHosts = coreMemberDiscoveryAddresses.Select( AdvertisedSocketAddress::toString ).collect( joining( "," ) );
			  _boltAdvertisedSocketAddress = advertisedAddress( advertisedAddress, boltPort );

			  IDictionary<string, string> config = stringMap();
			  config["dbms.mode"] = "READ_REPLICA";
			  config[CausalClusteringSettings.initial_discovery_members.name()] = initialHosts;
			  config[CausalClusteringSettings.discovery_listen_address.name()] = listenAddress(listenAddress, discoveryPort);
			  config[CausalClusteringSettings.discovery_advertised_address.name()] = advertisedAddress(advertisedAddress, discoveryPort);
			  config[GraphDatabaseSettings.store_internal_log_level.name()] = Level.DEBUG.name();
			  config[GraphDatabaseSettings.record_format.name()] = recordFormat;
			  config[GraphDatabaseSettings.pagecache_memory.name()] = "8m";
			  config[GraphDatabaseSettings.auth_store.name()] = (new File(parentDir, "auth")).AbsolutePath;
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  config.putAll( extraParams );

			  foreach ( KeyValuePair<string, System.Func<int, string>> entry in instanceExtraParams.SetOfKeyValuePairs() )
			  {
					config[entry.Key] = entry.Value.apply( serverId );
			  }

			  config[( new BoltConnector( "bolt" ) ).type.name()] = "BOLT";
			  config[( new BoltConnector( "bolt" ) ).enabled.name()] = "true";
			  config[( new BoltConnector( "bolt" ) ).listen_address.name()] = listenAddress(listenAddress, boltPort);
			  config[( new BoltConnector( "bolt" ) ).advertised_address.name()] = _boltAdvertisedSocketAddress;
			  config[( new HttpConnector( "http", HttpConnector.Encryption.NONE ) ).type.name()] = "HTTP";
			  config[( new HttpConnector( "http", HttpConnector.Encryption.NONE ) ).enabled.name()] = "true";
			  config[( new HttpConnector( "http", HttpConnector.Encryption.NONE ) ).listen_address.name()] = listenAddress(listenAddress, httpPort);
			  config[( new HttpConnector( "http", HttpConnector.Encryption.NONE ) ).advertised_address.name()] = advertisedAddress(advertisedAddress, httpPort);

			  this._neo4jHome = new File( parentDir, "read-replica-" + serverId );
			  config[GraphDatabaseSettings.neo4j_home.name()] = _neo4jHome.AbsolutePath;

			  config[CausalClusteringSettings.transaction_listen_address.name()] = listenAddress(listenAddress, txPort);
			  config[OnlineBackupSettings.online_backup_server.name()] = listenAddress(listenAddress, backupPort);
			  config[GraphDatabaseSettings.logs_directory.name()] = (new File(_neo4jHome, "logs")).AbsolutePath;
			  config[GraphDatabaseSettings.logical_logs_location.name()] = "replica-tx-logs-" + serverId;
			  _memberConfig = Config.defaults( config );

			  this.DiscoveryServiceFactory = discoveryServiceFactory;
			  File dataDirectory = new File( _neo4jHome, "data" );
			  DatabasesDirectory = new File( dataDirectory, "databases" );
			  DefaultDatabaseDirectory = new File( DatabasesDirectory, GraphDatabaseSettings.DEFAULT_DATABASE_NAME );

			  //noinspection ResultOfMethodCallIgnored
			  DefaultDatabaseDirectory.mkdirs();

			  this.MonitorsConflict = monitors;
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

		 public override void Start()
		 {
			  DatabaseConflict = new ReadReplicaGraphDatabase( DatabasesDirectory, _memberConfig, GraphDatabaseDependencies.newDependencies().monitors(MonitorsConflict), DiscoveryServiceFactory, MemberId() );
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

		 public virtual CatchupPollingProcess TxPollingClient()
		 {
			  return DatabaseConflict.DependencyResolver.resolveDependency( typeof( CatchupPollingProcess ) );
		 }

		 public override ReadReplicaGraphDatabase Database()
		 {
			  return DatabaseConflict;
		 }

		 public override ClientConnectorAddresses ClientConnectorAddresses()
		 {
			  return ClientConnectorAddresses.ExtractFromConfig( _memberConfig );
		 }

		 public override string SettingValue( string settingName )
		 {
			  return _memberConfig.Raw[settingName];
		 }

		 public override ThreadGroup ThreadGroup()
		 {
			  return _threadGroup;
		 }

		 public override Monitors Monitors()
		 {
			  return MonitorsConflict;
		 }

		 public override File DatabaseDirectory()
		 {
			  return DefaultDatabaseDirectory;
		 }

		 public override string ToString()
		 {
			  return format( "ReadReplica{serverId=%d}", _serverId );
		 }

		 public virtual string DirectURI()
		 {
			  return string.Format( "bolt://{0}", _boltAdvertisedSocketAddress );
		 }

		 public override File HomeDir()
		 {
			  return _neo4jHome;
		 }

		 public virtual string UpstreamDatabaseSelectionStrategy
		 {
			 set
			 {
				  UpdateConfig( CausalClusteringSettings.upstream_selection_strategy, value );
			 }
		 }

		 public virtual MemberId MemberId()
		 {
			  return new MemberId( new System.Guid( ( ( long ) _serverId ) << 32, 0 ) );
		 }

		 public override int ServerId()
		 {
			  return _serverId;
		 }

		 public override Config Config()
		 {
			  return _memberConfig;
		 }
	}

}