using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

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
namespace Neo4Net.Kernel.impl.ha
{

	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using InstanceId = Neo4Net.cluster.InstanceId;
	using Cluster = Neo4Net.cluster.client.Cluster;
	using ClusterClient = Neo4Net.cluster.client.ClusterClient;
	using ClusterClientModule = Neo4Net.cluster.client.ClusterClientModule;
	using NetworkReceiver = Neo4Net.cluster.com.NetworkReceiver;
	using NetworkSender = Neo4Net.cluster.com.NetworkSender;
	using ClusterMemberEvents = Neo4Net.cluster.member.ClusterMemberEvents;
	using ClusterMemberListener = Neo4Net.cluster.member.ClusterMemberListener;
	using NotElectableElectionCredentialsProvider = Neo4Net.cluster.protocol.election.NotElectableElectionCredentialsProvider;
	using StoreAssertions = Neo4Net.Consistency.store.StoreAssertions;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using TransactionFailureException = Neo4Net.Graphdb.TransactionFailureException;
	using Neo4Net.Graphdb.config;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using HighlyAvailableGraphDatabaseFactory = Neo4Net.Graphdb.factory.HighlyAvailableGraphDatabaseFactory;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using MapUtil = Neo4Net.Helpers.Collection.MapUtil;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using StoreLayout = Neo4Net.Io.layout.StoreLayout;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using HaSettings = Neo4Net.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using UpdatePuller = Neo4Net.Kernel.ha.UpdatePuller;
	using HighAvailabilityMemberState = Neo4Net.Kernel.ha.cluster.HighAvailabilityMemberState;
	using ClusterMember = Neo4Net.Kernel.ha.cluster.member.ClusterMember;
	using ClusterMembers = Neo4Net.Kernel.ha.cluster.member.ClusterMembers;
	using ObservedClusterMembers = Neo4Net.Kernel.ha.cluster.member.ObservedClusterMembers;
	using HighAvailabilityModeSwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;
	using Slaves = Neo4Net.Kernel.ha.com.master.Slaves;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using Neo4Net.Kernel.impl.util;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.@internal.LogService;
	using NullLogService = Neo4Net.Logging.@internal.NullLogService;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using StorageEngine = Neo4Net.Storageengine.Api.StorageEngine;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.ArrayUtil.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.fs.FileUtils.copyRecursively;

	/// <summary>
	/// Utility for spinning up an HA cluster inside the same JVM. Only intended for being used in tests
	/// as well as other tools that may need a cluster conveniently within the same JVM.
	/// </summary>
	public class ClusterManager : LifecycleAdapter
	{
		 /*
		  * The following ports are used by cluster instances to setup listening sockets. It is important that the range
		  * used remains below 30000, since that is where the ephemeral ports start in some linux kernels. If they are
		  * used, they can conflict with ephemeral sockets and result in address already in use errors, resulting in
		  * false failures.
		  */
		 public const int FIRST_SERVER_ID = 1;

		 /// <summary>
		 /// Network Flags for passing into <seealso cref="ManagedCluster.fail(HighlyAvailableGraphDatabase, NetworkFlag...)"/>
		 /// </summary>
		 public enum NetworkFlag
		 {
			  /// <summary>
			  /// Fail outgoing cluster network traffic.
			  /// </summary>
			  Out,
			  /// <summary>
			  /// Fail incoming cluster network traffic.
			  /// </summary>
			  In
		 }

		 private const long DEFAULT_TIMEOUT_SECONDS = 600L;
		 public static readonly IDictionary<string, string> ConfigForSingleJvmCluster = unmodifiableMap( stringMap( GraphDatabaseSettings.pagecache_memory.name(), "8m", GraphDatabaseSettings.shutdown_transaction_end_timeout.name(), "1s", new BoltConnector("bolt").type.name(), "BOLT", new BoltConnector("bolt").enabled.name(), "false" ) );

		 public interface StoreDirInitializer
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void initializeStoreDir(int serverId, java.io.File storeDir) throws java.io.IOException;
			  void InitializeStoreDir( int serverId, File storeDir );
		 }

		 public interface RepairKit
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.kernel.ha.HighlyAvailableGraphDatabase repair() throws Throwable;
			  HighlyAvailableGraphDatabase Repair();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Func<int, String> constant(final String value)
		 public static System.Func<int, string> Constant( string value )
		 {
			  return ignored => value;
		 }

		 private readonly string _localAddress;
		 private readonly File _root;
		 private readonly IDictionary<string, System.Func<int, string>> _commonConfig;
		 private readonly System.Func<Cluster> _clustersSupplier;
		 private readonly HighlyAvailableGraphDatabaseFactory _dbFactory;
		 private readonly StoreDirInitializer _storeDirInitializer;
		 private readonly Listener<GraphDatabaseService> _initialDatasetCreator;
		 private readonly IList<System.Predicate<ManagedCluster>> _availabilityChecks;
		 private ManagedCluster _managedCluster;
		 private readonly bool _consistencyCheck;
		 private readonly int _firstInstanceId;
		 private LifeSupport _life;
		 private bool _boltEnabled;
		 private readonly Monitors _commonMonitors;

		 private ClusterManager( Builder builder )
		 {
			  this._localAddress = LocalAddress;
			  this._clustersSupplier = builder.Supplier;
			  this._root = builder.Root;
			  this._commonConfig = WithDefaults( builder.CommonConfig );
			  this._dbFactory = builder.Factory;
			  this._storeDirInitializer = builder.Initializer;
			  this._initialDatasetCreator = builder.InitialDatasetCreator;
			  this._availabilityChecks = builder.AvailabilityChecks;
			  this._consistencyCheck = builder.ConsistencyCheck;
			  this._firstInstanceId = builder.FirstInstanceId;
			  this._boltEnabled = builder.BoltEnabled;
			  this._commonMonitors = builder.Monitors;
		 }

		 private static IDictionary<string, System.Func<int, string>> WithDefaults( IDictionary<string, System.Func<int, string>> commonConfig )
		 {
			  IDictionary<string, System.Func<int, string>> result = new Dictionary<string, System.Func<int, string>>();
			  foreach ( KeyValuePair<string, string> conf in ConfigForSingleJvmCluster.SetOfKeyValuePairs() )
			  {
					result[conf.Key] = Constant( conf.Value );
			  }
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  result.putAll( commonConfig );
			  return result;
		 }

		 private static string LocalAddress
		 {
			 get
			 {
				  try
				  {
						// Null corresponds to localhost
						return InetAddress.getByName( null ).HostAddress;
				  }
				  catch ( UnknownHostException e )
				  {
						// Fetching the localhost address won't throw this exception, so this should never happen, but if it
						// were, then the computer doesn't even have a loopback interface, so crash now rather than later
						throw new AssertionError( e );
				  }
			 }
		 }

		 /// <summary>
		 /// Provides a cluster specification with default values
		 /// </summary>
		 /// <param name="memberCount"> the total number of members in the cluster to start. </param>
		 public static System.Func<Cluster> ClusterOfSize( int memberCount )
		 {
			  return ClusterOfSize( LocalAddress, memberCount );
		 }

		 /// <summary>
		 /// Provides a cluster specification with default values on specified hostname
		 /// </summary>
		 /// <param name="hostname"> the hostname/ip-address to bind to </param>
		 /// <param name="memberCount"> the total number of members in the cluster to start. </param>
		 public static System.Func<Cluster> ClusterOfSize( string hostname, int memberCount )
		 {
			  return () =>
			  {
				Cluster cluster = new Cluster();

				for ( int i = 0; i < memberCount; i++ )
				{
					 int port = PortAuthority.allocatePort();
					 cluster.Members.add( new Cluster.Member( hostname + ":" + port, true ) );
				}
				return cluster;
			  };
		 }

		 /// <summary>
		 /// Provides a cluster specification with default values and unique ports
		 /// </summary>
		 /// <param name="haMemberCount"> the total number of members in the cluster to start. </param>
		 public static System.Func<Cluster> ClusterWithAdditionalClients( int haMemberCount, int additionalClientCount )
		 {
			  return () =>
			  {
				Cluster cluster = new Cluster();

				for ( int i = 0; i < haMemberCount; i++ )
				{
					 int port = PortAuthority.allocatePort();
					 cluster.Members.add( new Cluster.Member( port, true ) );
				}
				for ( int i = 0; i < additionalClientCount; i++ )
				{
					 int port = PortAuthority.allocatePort();
					 cluster.Members.add( new Cluster.Member( port, false ) );
				}

				return cluster;
			  };
		 }

		 /// <summary>
		 /// The current master sees this many slaves as available.
		 /// </summary>
		 /// <param name="count"> number of slaves to see as available. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Predicate<ManagedCluster> masterSeesSlavesAsAvailable(final int count)
		 public static System.Predicate<ManagedCluster> MasterSeesSlavesAsAvailable( int count )
		 {
			  return new PredicateAnonymousInnerClass( count );
		 }

		 private class PredicateAnonymousInnerClass : System.Predicate<ClusterManager.ManagedCluster>
		 {
			 private int _count;

			 public PredicateAnonymousInnerClass( int count )
			 {
				 this._count = count;
			 }

			 public override bool test( ManagedCluster cluster )
			 {
				  return _count( cluster.Master.DependencyResolver.resolveDependency( typeof( Slaves ) ).Slaves ) >= _count;
			 }

			 public override string ToString()
			 {
				  return "Master should see " + _count + " slaves as available";
			 }
		 }

		 /// <summary>
		 /// There must be a master available. Optionally exceptions, useful for when awaiting a
		 /// re-election of a different master.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Predicate<ManagedCluster> masterAvailable(final org.neo4j.kernel.ha.HighlyAvailableGraphDatabase... except)
		 public static System.Predicate<ManagedCluster> MasterAvailable( params HighlyAvailableGraphDatabase[] except )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<org.neo4j.kernel.ha.HighlyAvailableGraphDatabase> excludedNodes = asList(except);
			  ICollection<HighlyAvailableGraphDatabase> excludedNodes = asList( except );
			  return new PredicateAnonymousInnerClass2( excludedNodes );
		 }

		 private class PredicateAnonymousInnerClass2 : System.Predicate<ClusterManager.ManagedCluster>
		 {
			 private ICollection<HighlyAvailableGraphDatabase> _excludedNodes;

			 public PredicateAnonymousInnerClass2( ICollection<HighlyAvailableGraphDatabase> excludedNodes )
			 {
				 this._excludedNodes = excludedNodes;
			 }

			 public override bool test( ManagedCluster cluster )
			 {
				  System.Predicate<HighlyAvailableGraphDatabase> filterMasterPredicate = node => !_excludedNodes.Contains( node ) && node.isAvailable( 0 ) && node.Master;
				  return Iterables.filter( filterMasterPredicate, cluster.AllMembers ).GetEnumerator().hasNext();
			 }

			 public override string ToString()
			 {
				  return "There's an available master";
			 }
		 }

		 /// <summary>
		 /// The current master sees this many members (including itself).
		 /// </summary>
		 /// <param name="count"> number of members to see. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Predicate<ManagedCluster> masterSeesMembers(final int count)
		 public static System.Predicate<ManagedCluster> MasterSeesMembers( int count )
		 {
			  return new PredicateAnonymousInnerClass3( count );
		 }

		 private class PredicateAnonymousInnerClass3 : System.Predicate<ClusterManager.ManagedCluster>
		 {
			 private int _count;

			 public PredicateAnonymousInnerClass3( int count )
			 {
				 this._count = count;
			 }

			 public override bool test( ManagedCluster cluster )
			 {
				  ClusterMembers members = cluster.Master.DependencyResolver.resolveDependency( typeof( ClusterMembers ) );
				  return _count( members.Members ) == _count;
			 }

			 public override string ToString()
			 {
				  return "Master should see " + _count + " members";
			 }
		 }

		 public static System.Predicate<ManagedCluster> AllSeesAllAsAvailable()
		 {
			  return new PredicateAnonymousInnerClass4();
		 }

		 private class PredicateAnonymousInnerClass4 : System.Predicate<ManagedCluster>
		 {
			 public override bool test( ManagedCluster cluster )
			 {
				  if ( !AllSeesAllAsJoined().test(cluster) )
				  {
						return false;
				  }

				  int clusterMembersChecked = 0;
				  foreach ( HighlyAvailableGraphDatabase database in cluster.AllMembers )
				  {
						clusterMembersChecked++;
						ClusterMembers members = database.DependencyResolver.resolveDependency( typeof( ClusterMembers ) );

						foreach ( ClusterMember clusterMember in members.Members )
						{
							 if ( !cluster.IsAvailable( clusterMember ) )
							 {
								  return false;
							 }
						}
				  }
				  if ( clusterMembersChecked == 0 )
				  {
						return false;
				  }

				  // Everyone sees everyone else as available!
				  foreach ( HighlyAvailableGraphDatabase database in cluster.AllMembers )
				  {
						Log log = database.DependencyResolver.resolveDependency( typeof( LogService ) ).getInternalLog( this.GetType() );
						log.Debug( this.ToString() );
				  }
				  return true;
			 }

			 public override string ToString()
			 {
				  return "All instances should see all others as available";
			 }
		 }

		 public static System.Predicate<ManagedCluster> AllSeesAllAsJoined()
		 {
			  return new PredicateAnonymousInnerClass5();
		 }

		 private class PredicateAnonymousInnerClass5 : System.Predicate<ManagedCluster>
		 {
			 public override bool test( ManagedCluster cluster )
			 {
				  int clusterSize = cluster.Size();
				  int clusterMembersChecked = 0;

				  foreach ( HighlyAvailableGraphDatabase database in cluster.AllMembers )
				  {
						clusterMembersChecked++;
						ClusterMembers members = database.DependencyResolver.resolveDependency( typeof( ClusterMembers ) );

						if ( count( members.Members ) < clusterSize )
						{
							 return false;
						}
				  }

				  foreach ( ObservedClusterMembers arbiter in cluster.Arbiters )
				  {
						clusterMembersChecked++;
						if ( count( arbiter.Members ) < clusterSize )
						{
							 return false;
						}
				  }

				  // Everyone sees everyone else as joined!
				  return clusterMembersChecked > 0;
			 }

			 public override string ToString()
			 {
				  return "All instances should see all others as joined";
			 }
		 }

		 public static System.Predicate<ManagedCluster> AllAvailabilityGuardsReleased()
		 {
			  return item =>
			  {
				int clusterMembersChecked = 0;
				foreach ( HighlyAvailableGraphDatabase member in item.AllMembers )
				{
					 clusterMembersChecked++;
					 try
					 {
						  member.beginTx().close();
					 }
					 catch ( TransactionFailureException )
					 {
						  return false;
					 }
					 clusterMembersChecked++;
				}
				return clusterMembersChecked > 0;
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Predicate<ClusterManager.ManagedCluster> instanceEvicted(final org.neo4j.kernel.ha.HighlyAvailableGraphDatabase instance)
		 public static System.Predicate<ClusterManager.ManagedCluster> InstanceEvicted( HighlyAvailableGraphDatabase instance )
		 {
			  return _managedCluster =>
			  {
				InstanceId instanceId = _managedCluster.getServerId( instance );

				IEnumerable<HighlyAvailableGraphDatabase> members = _managedCluster.AllMembers;
				foreach ( HighlyAvailableGraphDatabase member in members )
				{
					 if ( instanceId.Equals( _managedCluster.getServerId( member ) ) )
					 {
						  if ( member.role().Equals("UNKNOWN") )
						  {
								return true;
						  }
					 }
				}
				return false;
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Predicate<ManagedCluster> memberSeesOtherMemberAsFailed(final org.neo4j.kernel.ha.HighlyAvailableGraphDatabase observer, final org.neo4j.kernel.ha.HighlyAvailableGraphDatabase observed)
		 public static System.Predicate<ManagedCluster> MemberSeesOtherMemberAsFailed( HighlyAvailableGraphDatabase observer, HighlyAvailableGraphDatabase observed )
		 {
			  return cluster =>
			  {
				InstanceId observedServerId = observed.DependencyResolver.resolveDependency( typeof( Config ) ).get( ClusterSettings.server_id );
				foreach ( ClusterMember member in observer.DependencyResolver.resolveDependency( typeof( ClusterMembers ) ).Members )
				{
					 if ( member.InstanceId.Equals( observedServerId ) )
					 {
						  return !member.Alive;
					 }
				}
				throw new System.InvalidOperationException( observed + " not a member according to " + observer );
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Predicate<ManagedCluster> entireClusterSeesMemberAsNotAvailable(final org.neo4j.kernel.ha.HighlyAvailableGraphDatabase observed)
		 public static System.Predicate<ManagedCluster> EntireClusterSeesMemberAsNotAvailable( HighlyAvailableGraphDatabase observed )
		 {
			  return cluster =>
			  {
				InstanceId observedServerId = observed.DependencyResolver.resolveDependency( typeof( Config ) ).get( ClusterSettings.server_id );
				int clusterMembersChecked = 0;

				foreach ( HighlyAvailableGraphDatabase observer in cluster.getAllMembers( observed ) )
				{
					 clusterMembersChecked++;
					 foreach ( ClusterMember member in observer.DependencyResolver.resolveDependency( typeof( ClusterMembers ) ).Members )
					 {
						  if ( member.InstanceId.Equals( observedServerId ) )
						  {
								if ( cluster.isAvailable( member ) )
								{
									 return false;
								} // else, keep looping to see if anyone else sees it as alive
						  }
					 }
				}
				// No one could see it as alive
				return clusterMembersChecked > 0;
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Predicate<ManagedCluster> memberThinksItIsRole(final org.neo4j.kernel.ha.HighlyAvailableGraphDatabase member, final String role)
		 public static System.Predicate<ManagedCluster> MemberThinksItIsRole( HighlyAvailableGraphDatabase member, string role )
		 {
			  return cluster => role.Equals( member.Role() );
		 }

		 private static string StateToString( ManagedCluster cluster )
		 {
			  StringBuilder buf = new StringBuilder( "\n" );
			  foreach ( HighlyAvailableGraphDatabase database in cluster.AllMembers )
			  {
					ClusterClient client = database.DependencyResolver.resolveDependency( typeof( ClusterClient ) );
					buf.Append( "Instance " ).Append( client.ServerId ).Append( ":State " ).Append( database.InstanceState ).Append( " (" ).Append( client.ClusterServer ).Append( "):" ).Append( "\n" );

					ClusterMembers members = database.DependencyResolver.resolveDependency( typeof( ClusterMembers ) );
					buf.Append( members );
			  }

			  return buf.ToString();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  Cluster cluster = _clustersSupplier.get();

			  _life = new LifeSupport();

			  // Started so instances added here will be started immediately, and in case of exceptions they can be
			  // shutdown() or stop()ped properly
			  _life.start();

			  _managedCluster = new ManagedCluster( this, cluster );
			  _life.add( _managedCluster );

			  _availabilityChecks.ForEach( _managedCluster.await );

			  if ( _initialDatasetCreator != null )
			  {
					_initialDatasetCreator.receive( _managedCluster.Master );
					_managedCluster.sync();
			  }
		 }

		 public override void Stop()
		 {
			  _life.stop();
		 }

		 public override void Shutdown()
		 {
			  _life.shutdown();
		 }

		 /// <summary>
		 /// Shutdown the cluster and catch any exceptions which might be thrown as a result. If an exception is thrown,
		 /// the stacktrace is printed.
		 /// 
		 /// This is intended for unit tests where a failure in cluster shutdown might mask the actual error in the test.
		 /// </summary>
		 public virtual void SafeShutdown()
		 {
			  try
			  {
					Shutdown();
			  }
			  catch ( Exception throwable )
			  {
					Console.WriteLine( throwable.ToString() );
					Console.Write( throwable.StackTrace );
			  }
		 }

		 public virtual ManagedCluster Cluster
		 {
			 get
			 {
				  return _managedCluster;
			 }
		 }

		 public interface ClusterBuilder<SELF>
		 {
			  SELF WithRootDirectory( File root );

			  SELF WithSeedDir( File seedDir );

			  SELF WithStoreDirInitializer( StoreDirInitializer initializer );

			  SELF WithDbFactory( HighlyAvailableGraphDatabaseFactory dbFactory );

			  SELF WithCluster( System.Func<Cluster> supplier );

			  /// <summary>
			  /// Supplies configuration where config values, as opposed to <seealso cref="withSharedConfig(System.Collections.IDictionary)"/>,
			  /// are a function of (one-based) server id. The function may return {@code null} which means
			  /// that the particular member doesn't have that config value, or at least not specifically
			  /// set, such that any default value would be used.
			  /// </summary>
			  SELF WithInstanceConfig( IDictionary<string, System.Func<int, string>> commonConfig );

			  /// <summary>
			  /// Enables bolt across the cluster, which is off by default.
			  /// </summary>
			  SELF WithBoltEnabled();

			  /// <summary>
			  /// Like <seealso cref="withInstanceConfig(System.Collections.IDictionary)"/>, but for individual settings, conveniently using
			  /// <seealso cref="Setting"/> instance as key as well.
			  /// </summary>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: SELF withInstanceSetting(org.neo4j.graphdb.config.Setting<?> setting, System.Func<int, String> valueFunction);
			  SELF withInstanceSetting<T1>( Setting<T1> setting, System.Func<int, string> valueFunction );

			  /// <summary>
			  /// Supplies configuration where config values are shared with all instances in the cluster.
			  /// </summary>
			  SELF WithSharedConfig( IDictionary<string, string> commonConfig );

			  /// <summary>
			  /// Like <seealso cref="withInstanceSetting(Setting, IntFunction)"/>, but for individual settings, conveniently using
			  /// <seealso cref="Setting"/> instance as key as well.
			  /// </summary>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: SELF withSharedSetting(org.neo4j.graphdb.config.Setting<?> setting, String value);
			  SELF withSharedSetting<T1>( Setting<T1> setting, string value );

			  /// <summary>
			  /// Initial dataset to be created once the cluster is up and running.
			  /// </summary>
			  /// <param name="transactor"> the <seealso cref="Listener"/> receiving a call to create the dataset on the master. </param>
			  SELF WithInitialDataset( Listener<GraphDatabaseService> transactor );

			  /// <summary>
			  /// Checks that must pass before cluster is considered to be up.
			  /// </summary>
			  /// <param name="checks"> availability checks that must pass before considering the cluster online. </param>
			  SELF WithAvailabilityChecks( params System.Predicate<ManagedCluster>[] checks );

			  /// <summary>
			  /// Runs consistency checks on the databases after cluster has been shut down.
			  /// </summary>
			  SELF WithConsistencyCheckAfterwards();

			  /// <summary>
			  /// Sets the instance id of the first cluster member to be started. The rest of the cluster members will have
			  /// instance ids incremented by one, sequentially. Default is 1.
			  /// </summary>
			  /// <param name="firstInstanceId"> The lowest instance id that will be used in the cluster </param>
			  SELF WithFirstInstanceId( int firstInstanceId );
		 }

		 public class Builder : ClusterBuilder<Builder>
		 {
			  internal File Root;
			  internal System.Func<Cluster> Supplier = ClusterOfSize( 3 );
			  internal readonly IDictionary<string, System.Func<int, string>> CommonConfig = new Dictionary<string, System.Func<int, string>>();
			  internal HighlyAvailableGraphDatabaseFactory Factory = new HighlyAvailableGraphDatabaseFactory();
			  internal Monitors Monitors;
			  internal StoreDirInitializer Initializer;
			  internal Listener<GraphDatabaseService> InitialDatasetCreator;
			  internal IList<System.Predicate<ManagedCluster>> AvailabilityChecks = Collections.emptyList();
			  internal bool ConsistencyCheck;
			  internal int FirstInstanceId = FIRST_SERVER_ID;
			  internal bool BoltEnabled;

			  public Builder( File root ) : this()
			  {
					this.Root = root;
			  }

			  public Builder()
			  {
					// We want this, at least in the ClusterRule case where we fill this Builder instances
					// with all our behavior, but we don't know about the root directory until we evaluate the rule.
					CommonConfig[ClusterSettings.heartbeat_interval.name()] = Constant("500ms");
					CommonConfig[ClusterSettings.heartbeat_timeout.name()] = Constant("2s");
					CommonConfig[ClusterSettings.leave_timeout.name()] = Constant("5s");
					CommonConfig[GraphDatabaseSettings.pagecache_memory.name()] = Constant("8m");
			  }

			  public override Builder WithRootDirectory( File root )
			  {
					this.Root = root;
					return this;
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Builder withSeedDir(final java.io.File seedDir)
			  public override Builder WithSeedDir( File seedDir )
			  {
					return WithStoreDirInitializer( ( serverId, storeDir ) => copyRecursively( seedDir, storeDir ) );
			  }

			  public override Builder WithStoreDirInitializer( StoreDirInitializer initializer )
			  {
					this.Initializer = initializer;
					return this;
			  }

			  public override Builder WithDbFactory( HighlyAvailableGraphDatabaseFactory dbFactory )
			  {
					this.Factory = dbFactory;
					return this;
			  }

			  public virtual Builder WithMonitors( Monitors monitors )
			  {
					this.Monitors = monitors;
					return this;
			  }

			  public override Builder WithCluster( System.Func<Cluster> supplier )
			  {
					this.Supplier = supplier;
					return this;
			  }

			  public override Builder WithInstanceConfig( IDictionary<string, System.Func<int, string>> commonConfig )
			  {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
					this.CommonConfig.putAll( commonConfig );
					return this;
			  }

			  public override Builder WithBoltEnabled()
			  {
					this.BoltEnabled = true;
					return this;
			  }

			  public override Builder WithInstanceSetting<T1>( Setting<T1> setting, System.Func<int, string> valueFunction )
			  {
					this.CommonConfig[setting.Name()] = valueFunction;
					return this;
			  }

			  public override Builder WithSharedConfig( IDictionary<string, string> commonConfig )
			  {
					IDictionary<string, System.Func<int, string>> dynamic = new Dictionary<string, System.Func<int, string>>();
					foreach ( KeyValuePair<string, string> entry in commonConfig.SetOfKeyValuePairs() )
					{
						 dynamic[entry.Key] = Constant( entry.Value );
					}
					return WithInstanceConfig( dynamic );
			  }

			  public override Builder WithSharedSetting<T1>( Setting<T1> setting, string value )
			  {
					return WithInstanceSetting( setting, Constant( value ) );
			  }

			  public override Builder WithInitialDataset( Listener<GraphDatabaseService> transactor )
			  {
					this.InitialDatasetCreator = transactor;
					return this;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SafeVarargs public final Builder withAvailabilityChecks(System.Predicate<ManagedCluster>... checks)
			  public override Builder WithAvailabilityChecks( params System.Predicate<ManagedCluster>[] checks )
			  {
					this.AvailabilityChecks = Arrays.asList( checks );
					return this;
			  }

			  public override Builder WithConsistencyCheckAfterwards()
			  {
					this.ConsistencyCheck = true;
					return this;
			  }

			  public override Builder WithFirstInstanceId( int firstInstanceId )
			  {
					this.FirstInstanceId = firstInstanceId;
					return this;
			  }

			  public virtual ClusterManager Build()
			  {
					if ( Supplier == null )
					{
						 Supplier = ClusterOfSize( 3 );
					}
					return new ClusterManager( this );
			  }
		 }

		 /// <summary>
		 /// Represent one cluster. It can retrieve the current master, random slave
		 /// or all members. It can also temporarily fail an instance or shut it down.
		 /// </summary>
		 public class ManagedCluster : LifecycleAdapter
		 {
			 private readonly ClusterManager _outerInstance;

			  internal readonly Cluster Spec;
			  internal readonly string Name;
			  internal readonly IDictionary<InstanceId, HighlyAvailableGraphDatabase> Members = new ConcurrentDictionary<InstanceId, HighlyAvailableGraphDatabase>();
			  internal readonly IDictionary<HighlyAvailableGraphDatabase, Monitors> MonitorsMap = new ConcurrentDictionary<HighlyAvailableGraphDatabase, Monitors>();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IList<ObservedClusterMembers> ArbitersConflict = new List<ObservedClusterMembers>();
			  internal readonly ISet<RepairKit> PendingRepairs = Collections.synchronizedSet( new HashSet<RepairKit>() );
			  internal readonly ParallelLifecycle ParallelLife = new ParallelLifecycle( DEFAULT_TIMEOUT_SECONDS, SECONDS );
			  internal readonly string InitialHosts;
			  internal readonly File Parent;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ManagedCluster(org.neo4j.cluster.client.Cluster spec) throws java.net.URISyntaxException
			  internal ManagedCluster( ClusterManager outerInstance, Cluster spec )
			  {
				  this._outerInstance = outerInstance;
					this.Spec = spec;
					this.Name = spec.Name;
					InitialHosts = BuildInitialHosts();
					Parent = new File( outerInstance.root, Name );
					for ( int i = 0; i < spec.Members.Count; i++ )
					{
						 StartMember( new InstanceId( outerInstance.firstInstanceId + i ) );
					}
			  }

			  public virtual string InitialHostsConfigString
			  {
				  get
				  {
						StringBuilder result = new StringBuilder();
						foreach ( HighlyAvailableGraphDatabase member in AllMembers )
						{
							 result.Append( result.Length > 0 ? "," : "" ).Append( outerInstance.localAddress ).Append( ":" ).Append( member.DependencyResolver.resolveDependency( typeof( ClusterClient ) ).ClusterServer.Port );
						}
						return result.ToString();
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init() throws Throwable
			  public override void Init()
			  {
					ParallelLife.init();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
			  public override void Start()
			  {
					ParallelLife.start();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
			  public override void Stop()
			  {
					IList<HighlyAvailableGraphDatabase> dbs = new IList<HighlyAvailableGraphDatabase> { AllMembers };

					// Shut down the dbs in parallel
					ParallelLife.stop();

					foreach ( HighlyAvailableGraphDatabase db in dbs )
					{
						 if ( outerInstance.consistencyCheck )
						 {
							  ConsistencyCheck( Db.databaseLayout() );
						 }
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
			  public override void Shutdown()
			  {
					ParallelLife.shutdown();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void consistencyCheck(org.neo4j.io.layout.DatabaseLayout databaseLayout) throws Throwable
			  internal virtual void ConsistencyCheck( DatabaseLayout databaseLayout )
			  {
					StoreAssertions.assertConsistentStore( databaseLayout );
			  }

			  /// <returns> all started members in this cluster. </returns>
			  public virtual IEnumerable<HighlyAvailableGraphDatabase> GetAllMembers( params HighlyAvailableGraphDatabase[] except )
			  {
					ISet<HighlyAvailableGraphDatabase> exceptSet = new HashSet<HighlyAvailableGraphDatabase>( asList( except ) );
					return Members.Values.Where( db => !exceptSet.Contains( db ) ).ToList();
			  }

			  public virtual IEnumerable<ObservedClusterMembers> Arbiters
			  {
				  get
				  {
						return ArbitersConflict;
				  }
			  }

			  public virtual bool IsArbiter( ClusterMember clusterMember )
			  {
					foreach ( ObservedClusterMembers arbiter in ArbitersConflict )
					{
						 if ( arbiter.CurrentMember.InstanceId.Equals( clusterMember.InstanceId ) )
						 {
							  return true;
						 }
					}
					return false;
			  }

			  public virtual bool IsAvailable( ClusterMember clusterMember )
			  {
					if ( IsArbiter( clusterMember ) )
					{
						 return clusterMember.Alive;
					}
					else
					{
						 return clusterMember.Alive && !clusterMember.HARole.Equals( HighAvailabilityModeSwitcher.UNKNOWN );
					}
			  }

			  public virtual string GetBoltAddress( HighlyAvailableGraphDatabase db )
			  {
					return "bolt://" + Db.DependencyResolver.resolveDependency( typeof( Config ) ).get( ( new GraphDatabaseSettings.BoltConnector( "bolt" ) ).advertised_address ).ToString();
			  }

			  public virtual HostnamePort GetBackupAddress( HighlyAvailableGraphDatabase hagdb )
			  {
					return hagdb.DependencyResolver.resolveDependency( typeof( Config ) ).get( OnlineBackupSettings.online_backup_server );
			  }

			  /// <returns> the current master in the cluster. </returns>
			  /// <exception cref="IllegalStateException"> if there's no current master. </exception>
			  public virtual HighlyAvailableGraphDatabase Master
			  {
				  get
				  {
						foreach ( HighlyAvailableGraphDatabase graphDatabaseService in AllMembers )
						{
							 if ( graphDatabaseService.IsAvailable( 0 ) && graphDatabaseService.Master )
							 {
								  return graphDatabaseService;
							 }
						}
						throw new System.InvalidOperationException( "No master found in cluster " + Name + StateToString( this ) );
				  }
			  }

			  /// <param name="except"> do not return any of the dbs found in this array </param>
			  /// <returns> a slave in this cluster. </returns>
			  /// <exception cref="IllegalStateException"> if no slave was found in this cluster. </exception>
			  public virtual HighlyAvailableGraphDatabase GetAnySlave( params HighlyAvailableGraphDatabase[] except )
			  {
					ISet<HighlyAvailableGraphDatabase> exceptSet = new HashSet<HighlyAvailableGraphDatabase>( asList( except ) );
					foreach ( HighlyAvailableGraphDatabase graphDatabaseService in AllMembers )
					{
						 if ( graphDatabaseService.InstanceState == HighAvailabilityMemberState.SLAVE && !exceptSet.Contains( graphDatabaseService ) )
						 {
							  return graphDatabaseService;
						 }
					}
					throw new System.InvalidOperationException( "No slave found in cluster " + Name + StateToString( this ) );
			  }

			  /// <param name="serverId"> the server id to return the db for. </param>
			  /// <returns> the <seealso cref="HighlyAvailableGraphDatabase"/> with the given server id. </returns>
			  /// <exception cref="IllegalStateException"> if that db isn't started or no such
			  /// db exists in the cluster. </exception>
			  public virtual HighlyAvailableGraphDatabase GetMemberByServerId( InstanceId serverId )
			  {
					HighlyAvailableGraphDatabase db = Members[serverId];
					if ( db == null )
					{
						 throw new System.InvalidOperationException( "Db " + serverId + " not found at the moment in " + Name + StateToString( this ) );
					}
					return db;
			  }

			  /// <summary>
			  /// Returns the global monitor for a particular <seealso cref="InstanceId"/>.
			  /// </summary>
			  /// <param name="database"> the database to get the global <seealso cref="Monitors"/> from. </param>
			  /// <returns> the global <seealso cref="Monitors"/>. </returns>
			  /// <exception cref="IllegalStateException"> if no monitor is registered, this might imply that the
			  /// server is not started yet. </exception>
			  public virtual Monitors GetMonitorsByDatabase( HighlyAvailableGraphDatabase database )
			  {
					Monitors monitors = MonitorsMap[database];
					if ( monitors == null )
					{
						 throw new System.InvalidOperationException( "Monitors for db " + database + " not found" );
					}
					return monitors;
			  }

			  /// <summary>
			  /// Shuts down a member of this cluster. A <seealso cref="RepairKit"/> is returned
			  /// which is able to restore the instance (i.e. start it again). This method
			  /// does not return until the rest of the cluster sees the member as not available.
			  /// </summary>
			  /// <param name="db"> the <seealso cref="HighlyAvailableGraphDatabase"/> to shut down. </param>
			  /// <returns> a <seealso cref="RepairKit"/> which can start it again. </returns>
			  /// <exception cref="IllegalArgumentException"> if the given db isn't a member of this cluster. </exception>
			  public virtual RepairKit Shutdown( HighlyAvailableGraphDatabase db )
			  {
					AssertMember( db );
					InstanceId serverId = Db.DependencyResolver.resolveDependency( typeof( Config ) ).get( ClusterSettings.server_id );
					ShutdownMember( serverId );
					Await( EntireClusterSeesMemberAsNotAvailable( db ) );
					return Wrap( new StartDatabaseAgainKit( _outerInstance, this, serverId ) );
			  }

			  internal virtual void ShutdownMember( InstanceId serverId )
			  {
					HighlyAvailableGraphDatabase db = Members.Remove( serverId );
					Db.shutdown();
			  }

			  internal virtual void AssertMember( HighlyAvailableGraphDatabase db )
			  {
					foreach ( HighlyAvailableGraphDatabase existingMember in Members.Values )
					{
						 if ( existingMember.Equals( db ) )
						 {
							  return;
						 }
					}
					throw new System.ArgumentException( "Db " + db + " not a member of this cluster " + Name + StateToString( this ) );
			  }

			  /// <summary>
			  /// Fails a member of this cluster by making it not receive/respond to heartbeats.
			  /// A <seealso cref="RepairKit"/> is returned which is able to repair the instance  (i.e start the network) again. This
			  /// method does not return until the rest of the cluster sees the member as not available.
			  /// </summary>
			  /// <param name="db"> the <seealso cref="HighlyAvailableGraphDatabase"/> to fail. </param>
			  /// <returns> a <seealso cref="RepairKit"/> which can repair the failure. </returns>
			  public virtual RepairKit Fail( HighlyAvailableGraphDatabase db )
			  {
					return fail( db, Enum.GetValues( typeof( NetworkFlag ) ) );
			  }

			  /// <summary>
			  /// Fails a member of this cluster by making it either not respond to heartbeats, or not receive them, or both.
			  /// A <seealso cref="RepairKit"/> is returned which is able to repair the instance  (i.e start the network) again.
			  /// This method does not return until the rest of the cluster sees the member as not available.
			  /// </summary>
			  /// <param name="db"> the <seealso cref="HighlyAvailableGraphDatabase"/> to fail. </param>
			  /// <param name="flags"> which part of networking to fail (IN/OUT/BOTH) </param>
			  /// <returns> a <seealso cref="RepairKit"/> which can repair the failure. </returns>
			  /// <exception cref="IllegalArgumentException"> if the given db isn't a member of this cluster. </exception>
			  public virtual RepairKit Fail( HighlyAvailableGraphDatabase db, params NetworkFlag[] flags )
			  {
					return fail( db, true, flags );
			  }

			  /// <summary>
			  /// WARNING: beware of hacks.
			  /// <para>
			  /// Fails a member of this cluster by making it either not respond to heartbeats, or not receive them, or both.
			  /// A <seealso cref="RepairKit"/> is returned which is able to repair the instance  (i.e start the network) again.
			  /// This method optionally does not return until the rest of the cluster sees the member as not available.
			  /// 
			  /// </para>
			  /// </summary>
			  /// <param name="db"> the <seealso cref="HighlyAvailableGraphDatabase"/> to fail. </param>
			  /// <param name="waitUntilDown"> if true, will not return until rest of cluster reports member as not available </param>
			  /// <param name="flags"> which part of networking to fail (IN/OUT) </param>
			  /// <returns> a <seealso cref="RepairKit"/> which can repair the failure. </returns>
			  /// <exception cref="IllegalArgumentException"> if the given db isn't a member of this cluster. </exception>
			  public virtual RepairKit Fail( HighlyAvailableGraphDatabase db, bool waitUntilDown, params NetworkFlag[] flags )
			  {
					AssertMember( db );

					NetworkReceiver networkReceiver = Db.DependencyResolver.resolveDependency( typeof( NetworkReceiver ) );
					NetworkSender networkSender = Db.DependencyResolver.resolveDependency( typeof( NetworkSender ) );

					if ( contains( flags, NetworkFlag.In ) )
					{
						 networkReceiver.Paused = true;
					}
					if ( contains( flags, NetworkFlag.Out ) )
					{
						 networkSender.Paused = true;
					}

					if ( waitUntilDown )
					{
						 Await( EntireClusterSeesMemberAsNotAvailable( db ) );
					}
					return Wrap( new StartNetworkAgainKit( _outerInstance, db, networkReceiver, networkSender, flags ) );
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private RepairKit wrap(final RepairKit actual)
			  internal virtual RepairKit Wrap( RepairKit actual )
			  {
					PendingRepairs.Add( actual );
					return () =>
					{
					 try
					 {
						  return actual.Repair();
					 }
					 finally
					 {
						  PendingRepairs.remove( actual );
					 }
					};
			  }

			  internal virtual AdvertisedSocketAddress SocketAddressForServer( string advertisedAddress, int listenPort )
			  {
					return new AdvertisedSocketAddress( advertisedAddress, listenPort );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.ha.HighlyAvailableGraphDatabase startMemberNow(org.neo4j.cluster.InstanceId serverId) throws java.io.IOException, java.net.URISyntaxException
			  internal virtual HighlyAvailableGraphDatabase StartMemberNow( InstanceId serverId )
			  {
					Cluster.Member member = MemberSpec( serverId );
					URI clusterUri = clusterUri( member );
					int clusterPort = clusterUri.Port;
					int haPort = PortAuthority.allocatePort();
					StoreLayout storeLayout = StoreLayout.of( new File( Parent, "server" + serverId ) );
					DatabaseLayout databaseLayout = storeLayout.DatabaseLayout( GraphDatabaseSettings.DEFAULT_DATABASE_NAME );
					if ( outerInstance.storeDirInitializer != null )
					{
						 outerInstance.storeDirInitializer.InitializeStoreDir( serverId.ToIntegerIndex(), databaseLayout.DatabaseDirectory() );
					}

					Monitors monitors = DatabaseMonitors;
					GraphDatabaseBuilder builder = outerInstance.dbFactory.setMonitors( monitors ).NewEmbeddedDatabaseBuilder( databaseLayout.DatabaseDirectory() );
					builder.SetConfig( ClusterSettings.cluster_name, Name );
					builder.SetConfig( ClusterSettings.initial_hosts, InitialHosts );
					builder.setConfig( ClusterSettings.server_id, serverId + "" );
					builder.SetConfig( ClusterSettings.cluster_server, "0.0.0.0:" + clusterPort );
					builder.setConfig( HaSettings.ha_server, clusterUri.Host + ":" + haPort );
					builder.SetConfig( OnlineBackupSettings.online_backup_enabled, Settings.FALSE );
					foreach ( KeyValuePair<string, System.Func<int, string>> conf in outerInstance.commonConfig.SetOfKeyValuePairs() )
					{
						 builder.setConfig( conf.Key, conf.Value.apply( serverId.ToIntegerIndex() ) );
					}

					if ( outerInstance.boltEnabled )
					{
						 string listenAddress = "127.0.0.1";
						 int boltPort = PortAuthority.allocatePort();
						 AdvertisedSocketAddress advertisedSocketAddress = SocketAddressForServer( listenAddress, boltPort );
						 string advertisedAddress = advertisedSocketAddress.Hostname;
						 string boltAdvertisedAddress = advertisedAddress + ":" + boltPort;

						 builder.SetConfig( ( new GraphDatabaseSettings.BoltConnector( "bolt" ) ).type, "BOLT" );
						 builder.SetConfig( ( new GraphDatabaseSettings.BoltConnector( "bolt" ) ).enabled, "true" );
						 builder.SetConfig( ( new GraphDatabaseSettings.BoltConnector( "bolt" ) ).listen_address, listenAddress + ":" + boltPort );
						 builder.SetConfig( ( new GraphDatabaseSettings.BoltConnector( "bolt" ) ).advertised_address, boltAdvertisedAddress );
					}

					HighlyAvailableGraphDatabase graphDatabase = ( HighlyAvailableGraphDatabase ) builder.NewGraphDatabase();
					Members[serverId] = graphDatabase;
					MonitorsMap[graphDatabase] = monitors;
					return graphDatabase;
			  }

			  internal virtual Monitors DatabaseMonitors
			  {
				  get
				  {
						return outerInstance.commonMonitors != null ? outerInstance.commonMonitors : new Monitors();
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.net.URI clusterUri(org.neo4j.cluster.client.Cluster.Member member) throws java.net.URISyntaxException
			  internal virtual URI ClusterUri( Cluster.Member member )
			  {
					return new URI( "cluster://" + member.Host );
			  }

			  internal virtual Cluster.Member MemberSpec( InstanceId serverId )
			  {
					return Spec.Members[serverId.ToIntegerIndex() - outerInstance.firstInstanceId];
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void startMember(org.neo4j.cluster.InstanceId serverId) throws java.net.URISyntaxException
			  internal virtual void StartMember( InstanceId serverId )
			  {
					Cluster.Member member = MemberSpec( serverId );
					if ( member.FullHaMember )
					{
						 ParallelLife.add( new LifecycleAdapterAnonymousInnerClass( this, serverId ) );
					}
					else
					{
						 URI clusterUri = clusterUri( member );
						 Config config = Config.defaults( MapUtil.stringMap( ClusterSettings.cluster_name.name(), Name, ClusterSettings.initial_hosts.name(), InitialHosts, ClusterSettings.server_id.name(), serverId + "", ClusterSettings.cluster_server.name(), "0.0.0.0:" + clusterUri.Port, OnlineBackupSettings.online_backup_enabled.name(), Settings.FALSE ) );
						 LifeSupport clusterClientLife = new LifeSupport();
						 LogService logService = NullLogService.Instance;
						 ClusterClientModule clusterClientModule = new ClusterClientModule( clusterClientLife, new Dependencies(), new Monitors(), config, logService, new NotElectableElectionCredentialsProvider() );

						 ArbitersConflict.add(new ObservedClusterMembers(logService.InternalLogProvider, clusterClientModule.ClusterClient, clusterClientModule.ClusterClient, new ClusterMemberEventsAnonymousInnerClass(this)
						, clusterClientModule.ClusterClient.ServerId));

						 ParallelLife.add( clusterClientLife );
					}
			  }

			  private class LifecycleAdapterAnonymousInnerClass : LifecycleAdapter
			  {
				  private readonly ManagedCluster _outerInstance;

				  private InstanceId _serverId;

				  public LifecycleAdapterAnonymousInnerClass( ManagedCluster outerInstance, InstanceId serverId )
				  {
					  this.outerInstance = outerInstance;
					  this._serverId = serverId;
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
				  public override void start()
				  {
						outerInstance.startMemberNow( _serverId );
				  }

				  public override void stop()
				  {
						HighlyAvailableGraphDatabase db = _outerInstance.members.Remove( _serverId );
						if ( db != null )
						{
							 Db.shutdown();
						}
				  }
			  }

			  private class ClusterMemberEventsAnonymousInnerClass : ClusterMemberEvents
			  {
				  private readonly ManagedCluster _outerInstance;

				  public ClusterMemberEventsAnonymousInnerClass( ManagedCluster outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

				  public void addClusterMemberListener( ClusterMemberListener listener )
				  {
						// noop
				  }

				  public void removeClusterMemberListener( ClusterMemberListener listener )
				  {
						// noop
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String buildInitialHosts() throws java.net.URISyntaxException
			  internal virtual string BuildInitialHosts()
			  {
					StringBuilder initialHosts = new StringBuilder();
					for ( int i = 0; i < Spec.Members.Count; i++ )
					{
						 if ( i > 0 )
						 {
							  initialHosts.Append( "," );
						 }
						 // the host might be 0.0.0.0:PORT, or :PORT, if so, replace with a valid address.
						 URI uri = new URI( "cluster://" + Spec.Members[i].Host );
						 if ( uri.Host == null || uri.Host.Empty || uri.Host.Equals( "0.0.0.0" ) )
						 {
							  initialHosts.Append( outerInstance.localAddress ).Append( ":" ).Append( uri.Port );
						 }
						 else
						 {
							  initialHosts.Append( uri.Host ).Append( ":" ).Append( uri.Port );
						 }
					}
					return initialHosts.ToString();
			  }

			  /// <summary>
			  /// Will await a condition for the default max time.
			  /// </summary>
			  /// <param name="predicate"> <seealso cref="Predicate"/> that should return true
			  /// signalling that the condition has been met. </param>
			  /// <exception cref="IllegalStateException"> if the condition wasn't met
			  /// during within the max time. </exception>
			  public virtual void Await( System.Predicate<ManagedCluster> predicate )
			  {
					Await( predicate, DEFAULT_TIMEOUT_SECONDS );
			  }

			  /// <summary>
			  /// Will await a condition for the given max time.
			  /// </summary>
			  /// <param name="predicate"> <seealso cref="Predicate"/> that should return true
			  /// signalling that the condition has been met. </param>
			  /// <exception cref="IllegalStateException"> if the condition wasn't met
			  /// during within the max time. </exception>
			  public virtual void Await( System.Predicate<ManagedCluster> predicate, long maxSeconds )
			  {
					long end = DateTimeHelper.CurrentUnixTimeMillis() + TimeUnit.SECONDS.toMillis(maxSeconds);
					while ( DateTimeHelper.CurrentUnixTimeMillis() < end )
					{
						 if ( predicate( this ) )
						 {
							  return;
						 }
						 try
						 {
							  Thread.Sleep( 100 );
						 }
						 catch ( InterruptedException )
						 {
							  // Ignore
						 }
					}
					string state = StateToString( this );
					throw new System.InvalidOperationException( format( "Awaited condition never met, waited %s seconds for %s:%n%s", maxSeconds, predicate, state ) );
			  }

			  /// <summary>
			  /// The total number of members of the cluster.
			  /// </summary>
			  public virtual int Size()
			  {
					return Spec.Members.Count;
			  }

			  public virtual InstanceId GetServerId( HighlyAvailableGraphDatabase member )
			  {
					AssertMember( member );
					return member.DependencyResolver.resolveDependency( typeof( Config ) ).get( ClusterSettings.server_id );
			  }

			  public virtual File GetDatabaseDir( HighlyAvailableGraphDatabase member )
			  {
					AssertMember( member );
					return member.DatabaseLayout().databaseDirectory();
			  }

			  public virtual void Sync( params HighlyAvailableGraphDatabase[] except )
			  {
					ISet<HighlyAvailableGraphDatabase> exceptSet = new HashSet<HighlyAvailableGraphDatabase>( asList( except ) );
					foreach ( HighlyAvailableGraphDatabase db in AllMembers )
					{
						 if ( !exceptSet.Contains( db ) )
						 {
							  UpdatePuller updatePuller = Db.DependencyResolver.resolveDependency( typeof( UpdatePuller ) );
							  try
							  {
									if ( Db.isAvailable( 60000 ) ) // wait for 1 min for db to become available
									{
										 updatePuller.PullUpdates();
									}
							  }
							  catch ( Exception e )
							  {
									throw new System.InvalidOperationException( StateToString( this ), e );
							  }
						 }
					}
			  }

			  public virtual void Force( params HighlyAvailableGraphDatabase[] except )
			  {
					ISet<HighlyAvailableGraphDatabase> exceptSet = new HashSet<HighlyAvailableGraphDatabase>( asList( except ) );
					foreach ( HighlyAvailableGraphDatabase db in AllMembers )
					{
						 if ( !exceptSet.Contains( db ) )
						 {
							  IOLimiter limiter = Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited;
							  Db.DependencyResolver.resolveDependency( typeof( StorageEngine ) ).flushAndForce( limiter );
						 }
					}
			  }

			  public virtual void Info( string message )
			  {
					foreach ( HighlyAvailableGraphDatabase db in AllMembers )
					{
						 LogService logService = Db.DependencyResolver.resolveDependency( typeof( LogService ) );
						 Log messagesLog = logService.GetInternalLog( typeof( HighlyAvailableGraphDatabase ) );
						 messagesLog.Info( message );
					}
			  }

		 }

		 private class StartNetworkAgainKit : RepairKit
		 {
			 private readonly ClusterManager _outerInstance;

			  internal readonly HighlyAvailableGraphDatabase Db;
			  internal readonly NetworkReceiver NetworkReceiver;
			  internal readonly NetworkSender NetworkSender;
			  internal readonly NetworkFlag[] Flags;

			  internal StartNetworkAgainKit( ClusterManager outerInstance, HighlyAvailableGraphDatabase db, NetworkReceiver networkReceiver, NetworkSender networkSender, params NetworkFlag[] flags )
			  {
				  this._outerInstance = outerInstance;
					this.Db = db;
					this.NetworkReceiver = networkReceiver;
					this.NetworkSender = networkSender;
					this.Flags = flags;
			  }

			  public override HighlyAvailableGraphDatabase Repair()
			  {
					if ( contains( Flags, NetworkFlag.Out ) )
					{
						 NetworkSender.Paused = false;
					}
					if ( contains( Flags, NetworkFlag.In ) )
					{
						 NetworkReceiver.Paused = false;
					}

					return Db;
			  }
		 }

		 private class StartDatabaseAgainKit : RepairKit
		 {
			 private readonly ClusterManager _outerInstance;

			  internal readonly InstanceId ServerId;
			  internal readonly ManagedCluster Cluster;

			  internal StartDatabaseAgainKit( ClusterManager outerInstance, ManagedCluster cluster, InstanceId serverId )
			  {
				  this._outerInstance = outerInstance;
					this.Cluster = cluster;
					this.ServerId = serverId;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.ha.HighlyAvailableGraphDatabase repair() throws Throwable
			  public override HighlyAvailableGraphDatabase Repair()
			  {
					return Cluster.startMemberNow( ServerId );
			  }
		 }
	}

}