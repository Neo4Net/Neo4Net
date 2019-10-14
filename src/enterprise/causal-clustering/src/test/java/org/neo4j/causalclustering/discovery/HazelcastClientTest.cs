using System;
using System.Collections;
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
	using Client = com.hazelcast.core.Client;
	using ClientService = com.hazelcast.core.ClientService;
	using Cluster = com.hazelcast.core.Cluster;
	using Endpoint = com.hazelcast.core.Endpoint;
	using EntryListener = com.hazelcast.core.EntryListener;
	using EntryView = com.hazelcast.core.EntryView;
	using ExecutionCallback = com.hazelcast.core.ExecutionCallback;
	using HazelcastInstance = com.hazelcast.core.HazelcastInstance;
	using IAtomicReference = com.hazelcast.core.IAtomicReference;
	using ICompletableFuture = com.hazelcast.core.ICompletableFuture;
	using IExecutorService = com.hazelcast.core.IExecutorService;
	using IMap = com.hazelcast.core.IMap;
	using ISet = com.hazelcast.core.ISet;
	using ItemListener = com.hazelcast.core.ItemListener;
	using Member = com.hazelcast.core.Member;
	using MemberSelector = com.hazelcast.core.MemberSelector;
	using MultiExecutionCallback = com.hazelcast.core.MultiExecutionCallback;
	using MultiMap = com.hazelcast.core.MultiMap;
	using EntryProcessor = com.hazelcast.map.EntryProcessor;
	using MapInterceptor = com.hazelcast.map.MapInterceptor;
	using MapListener = com.hazelcast.map.listener.MapListener;
	using MapPartitionLostListener = com.hazelcast.map.listener.MapPartitionLostListener;
	using JobTracker = com.hazelcast.mapreduce.JobTracker;
	using Aggregation = com.hazelcast.mapreduce.aggregation.Aggregation;
	using Supplier = com.hazelcast.mapreduce.aggregation.Supplier;
	using LocalExecutorStats = com.hazelcast.monitor.LocalExecutorStats;
	using LocalMapStats = com.hazelcast.monitor.LocalMapStats;
	using LocalMultiMapStats = com.hazelcast.monitor.LocalMultiMapStats;
	using Predicate = com.hazelcast.query.Predicate;
	using Test = org.junit.Test;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using OnDemandJobScheduler = Neo4Net.Test.OnDemandJobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.CLIENT_CONNECTOR_ADDRESSES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.MEMBER_DB_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.MEMBER_UUID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.RAFT_SERVER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.TRANSACTION_SERVER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class HazelcastClientTest
	{
		 private MemberId _myself = new MemberId( System.Guid.randomUUID() );
		 private static readonly System.Func<Dictionary<string, string>> _defaultSettings = () =>
		 {
		  Dictionary<string, string> settings = new Dictionary<string, string>();

		  settings.put( ( new BoltConnector( "bolt" ) ).type.name(), "BOLT" );
		  settings.put( ( new BoltConnector( "bolt" ) ).enabled.name(), "true" );
		  settings.put( ( new BoltConnector( "bolt" ) ).advertised_address.name(), "bolt:3001" );

		  settings.put( ( new BoltConnector( "http" ) ).type.name(), "HTTP" );
		  settings.put( ( new BoltConnector( "http" ) ).enabled.name(), "true" );
		  settings.put( ( new BoltConnector( "http" ) ).advertised_address.name(), "http:3001" );
		  return settings;
		 };

		 private Config Config( string key, string value )
		 {
			  Dictionary<string, string> defaults = _defaultSettings.get();
			  defaults[key] = value;
			  return Config.defaults( defaults );
		 }

		 private Config Config()
		 {
			  return Config.defaults( _defaultSettings.get() );
		 }

		 private HazelcastClient HzClient( OnDemandJobScheduler jobScheduler, Cluster cluster, Config config )
		 {
			  HazelcastConnector connector = mock( typeof( HazelcastConnector ) );

			  HazelcastClient client = new HazelcastClient( connector, jobScheduler, NullLogProvider.Instance, config, _myself );

			  HazelcastInstance hazelcastInstance = mock( typeof( HazelcastInstance ) );
			  when( connector.ConnectToHazelcast() ).thenReturn(hazelcastInstance);

			  when( hazelcastInstance.getSet( anyString() ) ).thenReturn(new HazelcastSet(this));
			  when( hazelcastInstance.getMultiMap( anyString() ) ).thenReturn(new HazelcastMultiMap(this));
			  when( hazelcastInstance.getMap( anyString() ) ).thenReturn(new HazelcastMap(this));

			  when( hazelcastInstance.Cluster ).thenReturn( cluster );
			  when( hazelcastInstance.getExecutorService( anyString() ) ).thenReturn(new StubExecutorService(this));

			  return client;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private HazelcastClient startedClientWithMembers(java.util.Set<com.hazelcast.core.Member> members, org.neo4j.kernel.configuration.Config config) throws Throwable
		 private HazelcastClient StartedClientWithMembers( ISet<Member> members, Config config )
		 {
			  OnDemandJobScheduler jobScheduler = new OnDemandJobScheduler();
			  Cluster cluster = mock( typeof( Cluster ) );

			  when( cluster.Members ).thenReturn( members );

			  HazelcastClient client = HzClient( jobScheduler, cluster, config );
			  client.Init();
			  client.Start();
			  jobScheduler.RunJob();

			  return client;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTopologyUsingHazelcastMembers() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnTopologyUsingHazelcastMembers()
		 {
			  // given
			  ISet<Member> members = asSet( MakeMember( 1 ), MakeMember( 2 ) );
			  HazelcastClient client = StartedClientWithMembers( members, Config() );

			  // when
			  CoreTopology topology = client.LocalCoreServers();

			  // then
			  assertEquals( members.Count, topology.Members().Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void localAndAllTopologiesShouldMatchForSingleDBName() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LocalAndAllTopologiesShouldMatchForSingleDBName()
		 {
			  // given
			  ISet<Member> members = asSet( MakeMember( 1 ), MakeMember( 2 ) );
			  HazelcastClient client = StartedClientWithMembers( members, Config() );

			  // then
			  string message = "Different local and global topologies reported despite single, default database name.";
			  assertEquals( message, client.AllCoreServers(), client.LocalCoreServers() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void localAndAllTopologiesShouldDifferForMultipleDBNames() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LocalAndAllTopologiesShouldDifferForMultipleDBNames()
		 {
			  // given
			  ISet<Member> members = asSet( MakeMember( 1, "foo" ), MakeMember( 2, "bar" ) );
			  HazelcastClient client = StartedClientWithMembers( members, Config( CausalClusteringSettings.database.name(), "foo" ) );

			  // then
			  string message = "Identical local and global topologies reported despite multiple, distinct database names.";
			  assertNotEquals( message, client.AllCoreServers(), client.LocalCoreServers() );
			  assertEquals( 1, client.LocalCoreServers().members().Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allTopologyShouldContainAllMembers() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AllTopologyShouldContainAllMembers()
		 {
			  // given
			  ISet<Member> members = asSet( MakeMember( 1, "foo" ), MakeMember( 2, "bar" ) );
			  HazelcastClient client = StartedClientWithMembers( members, Config( CausalClusteringSettings.database.name(), "foo" ) );

			  // then
			  string message = "Global topology should contain all Hazelcast Members despite different db names.";
			  assertEquals( message, members.Count, client.AllCoreServers().members().Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReconnectWhileHazelcastRemainsAvailable() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReconnectWhileHazelcastRemainsAvailable()
		 {
			  // given
			  HazelcastConnector connector = mock( typeof( HazelcastConnector ) );
			  OnDemandJobScheduler jobScheduler = new OnDemandJobScheduler();

			  HazelcastClient client = new HazelcastClient( connector, jobScheduler, NullLogProvider.Instance, Config(), _myself );

			  HazelcastInstance hazelcastInstance = mock( typeof( HazelcastInstance ) );
			  when( connector.ConnectToHazelcast() ).thenReturn(hazelcastInstance);

			  when( hazelcastInstance.getSet( anyString() ) ).thenReturn(new HazelcastSet(this));
			  when( hazelcastInstance.getMultiMap( anyString() ) ).thenReturn(new HazelcastMultiMap(this));
			  when( hazelcastInstance.getExecutorService( anyString() ) ).thenReturn(new StubExecutorService(this));
			  when( hazelcastInstance.getMap( anyString() ) ).thenReturn(new HazelcastMap(this));

			  Cluster cluster = mock( typeof( Cluster ) );
			  when( hazelcastInstance.Cluster ).thenReturn( cluster );

			  ISet<Member> members = asSet( MakeMember( 1 ), MakeMember( 2 ) );
			  when( cluster.Members ).thenReturn( members );

			  // when
			  client.Init();
			  client.Start();
			  jobScheduler.RunJob();

			  CoreTopology topology;
			  for ( int i = 0; i < 5; i++ )
			  {
					topology = client.AllCoreServers();
					assertEquals( members.Count, topology.Members().Count );
			  }

			  // then
			  verify( connector, times( 1 ) ).connectToHazelcast();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnEmptyTopologyIfUnableToConnectToHazelcast() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnEmptyTopologyIfUnableToConnectToHazelcast()
		 {
			  // given
			  HazelcastConnector connector = mock( typeof( HazelcastConnector ) );
			  LogProvider logProvider = mock( typeof( LogProvider ) );

			  Log log = mock( typeof( Log ) );
			  when( logProvider.getLog( any( typeof( Type ) ) ) ).thenReturn( log );

			  HazelcastInstance hazelcastInstance = mock( typeof( HazelcastInstance ) );
			  when( connector.ConnectToHazelcast() ).thenThrow(new System.InvalidOperationException());
			  IAtomicReference iAtomicReference = mock( typeof( IAtomicReference ) );
			  when( hazelcastInstance.getAtomicReference( anyString() ) ).thenReturn(iAtomicReference);
			  when( hazelcastInstance.getSet( anyString() ) ).thenReturn(new HazelcastSet(this));

			  OnDemandJobScheduler jobScheduler = new OnDemandJobScheduler();

			  HazelcastClient client = new HazelcastClient( connector, jobScheduler, logProvider, Config(), _myself );

			  Cluster cluster = mock( typeof( Cluster ) );
			  when( hazelcastInstance.Cluster ).thenReturn( cluster );

			  ISet<Member> members = asSet( MakeMember( 1 ), MakeMember( 2 ) );
			  when( cluster.Members ).thenReturn( members );

			  // when
			  client.Init();
			  client.Start();
			  jobScheduler.RunJob();
			  CoreTopology topology = client.AllCoreServers();

			  assertEquals( 0, topology.Members().Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRegisterReadReplicaInTopology() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRegisterReadReplicaInTopology()
		 {
			  // given
			  Cluster cluster = mock( typeof( Cluster ) );
			  ISet<Member> members = asSet( MakeMember( 1 ) );
			  when( cluster.Members ).thenReturn( members );

			  Endpoint endpoint = mock( typeof( Endpoint ) );
			  when( endpoint.Uuid ).thenReturn( "12345" );

			  Client client = mock( typeof( Client ) );
			  const string clientId = "12345";
			  when( client.Uuid ).thenReturn( clientId );

			  ClientService clientService = mock( typeof( ClientService ) );
			  when( clientService.ConnectedClients ).thenReturn( asSet( client ) );

			  HazelcastMap hazelcastMap = new HazelcastMap( this );
			  HazelcastMultiMap hazelcastMultiMap = new HazelcastMultiMap( this );

			  HazelcastInstance hazelcastInstance = mock( typeof( HazelcastInstance ) );
			  IAtomicReference iAtomicReference = mock( typeof( IAtomicReference ) );
			  when( hazelcastInstance.getAtomicReference( anyString() ) ).thenReturn(iAtomicReference);
			  when( hazelcastInstance.getMap( anyString() ) ).thenReturn(hazelcastMap);
			  when( hazelcastInstance.getMultiMap( anyString() ) ).thenReturn(hazelcastMultiMap);
			  when( hazelcastInstance.LocalEndpoint ).thenReturn( endpoint );
			  when( hazelcastInstance.getExecutorService( anyString() ) ).thenReturn(new StubExecutorService(this));
			  when( hazelcastInstance.Cluster ).thenReturn( cluster );
			  when( hazelcastInstance.ClientService ).thenReturn( clientService );

			  HazelcastConnector connector = mock( typeof( HazelcastConnector ) );
			  when( connector.ConnectToHazelcast() ).thenReturn(hazelcastInstance);

			  OnDemandJobScheduler jobScheduler = new OnDemandJobScheduler();
			  HazelcastClient hazelcastClient = new HazelcastClient( connector, jobScheduler, NullLogProvider.Instance, Config(), _myself );

			  // when
			  hazelcastClient.Init();
			  hazelcastClient.Start();
			  jobScheduler.RunJob();

			  // then
			  assertEquals( 1, hazelcastMap.Size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveReadReplicasOnGracefulShutdown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveReadReplicasOnGracefulShutdown()
		 {
			  // given
			  Cluster cluster = mock( typeof( Cluster ) );
			  ISet<Member> members = asSet( MakeMember( 1 ) );
			  when( cluster.Members ).thenReturn( members );

			  Endpoint endpoint = mock( typeof( Endpoint ) );
			  when( endpoint.Uuid ).thenReturn( "12345" );

			  Client client = mock( typeof( Client ) );
			  const string clientId = "12345";
			  when( client.Uuid ).thenReturn( clientId );

			  ClientService clientService = mock( typeof( ClientService ) );
			  when( clientService.ConnectedClients ).thenReturn( asSet( client ) );

			  HazelcastMap hazelcastMap = new HazelcastMap( this );

			  HazelcastInstance hazelcastInstance = mock( typeof( HazelcastInstance ) );
			  IAtomicReference iAtomicReference = mock( typeof( IAtomicReference ) );
			  when( hazelcastInstance.getAtomicReference( anyString() ) ).thenReturn(iAtomicReference);
			  when( hazelcastInstance.getMap( anyString() ) ).thenReturn(hazelcastMap);
			  when( hazelcastInstance.getMultiMap( anyString() ) ).thenReturn(new HazelcastMultiMap(this));
			  when( hazelcastInstance.LocalEndpoint ).thenReturn( endpoint );
			  when( hazelcastInstance.getExecutorService( anyString() ) ).thenReturn(new StubExecutorService(this));
			  when( hazelcastInstance.Cluster ).thenReturn( cluster );
			  when( hazelcastInstance.ClientService ).thenReturn( clientService );

			  HazelcastConnector connector = mock( typeof( HazelcastConnector ) );
			  when( connector.ConnectToHazelcast() ).thenReturn(hazelcastInstance);

			  OnDemandJobScheduler jobScheduler = new OnDemandJobScheduler();
			  HazelcastClient hazelcastClient = new HazelcastClient( connector, jobScheduler, NullLogProvider.Instance, Config(), _myself );

			  hazelcastClient.Init();
			  hazelcastClient.Start();

			  jobScheduler.RunJob();

			  // when
			  hazelcastClient.Stop();

			  // then
			  assertEquals( 0, hazelcastMap.Size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSwallowNPEFromHazelcast() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSwallowNPEFromHazelcast()
		 {
			  // given
			  Endpoint endpoint = mock( typeof( Endpoint ) );
			  when( endpoint.Uuid ).thenReturn( "12345" );

			  HazelcastInstance hazelcastInstance = mock( typeof( HazelcastInstance ) );
			  when( hazelcastInstance.LocalEndpoint ).thenReturn( endpoint );
			  when( hazelcastInstance.getMap( anyString() ) ).thenReturn(new HazelcastMap(this));
			  when( hazelcastInstance.getMultiMap( anyString() ) ).thenReturn(new HazelcastMultiMap(this));
			  doThrow( new System.NullReferenceException( "boom!!!" ) ).when( hazelcastInstance ).shutdown();

			  HazelcastConnector connector = mock( typeof( HazelcastConnector ) );
			  when( connector.ConnectToHazelcast() ).thenReturn(hazelcastInstance);

			  OnDemandJobScheduler jobScheduler = new OnDemandJobScheduler();

			  HazelcastClient hazelcastClient = new HazelcastClient( connector, jobScheduler, NullLogProvider.Instance, Config(), _myself );

			  hazelcastClient.Init();
			  hazelcastClient.Start();

			  jobScheduler.RunJob();

			  // when
			  hazelcastClient.Stop();

			  // then no NPE has been thrown
		 }

		 private Member MakeMember( int id )
		 {
			  return MakeMember( id, CausalClusteringSettings.database.DefaultValue );
		 }

		 private Member MakeMember( int id, string databaseName )
		 {
			  Member member = mock( typeof( Member ) );
			  when( member.getStringAttribute( MEMBER_UUID ) ).thenReturn( System.Guid.randomUUID().ToString() );
			  when( member.getStringAttribute( TRANSACTION_SERVER ) ).thenReturn( format( "host%d:%d", id, 7000 + id ) );
			  when( member.getStringAttribute( RAFT_SERVER ) ).thenReturn( format( "host%d:%d", id, 6000 + id ) );
			  when( member.getStringAttribute( CLIENT_CONNECTOR_ADDRESSES ) ).thenReturn( format( "bolt://host%d:%d,http://host%d:%d", id, 5000 + id, id, 5000 + id ) );
			  when( member.getStringAttribute( MEMBER_DB_NAME ) ).thenReturn( databaseName );
			  return member;
		 }

		 private class HazelcastMap : IMap<object, object>
		 {
			 private readonly HazelcastClientTest _outerInstance;

			 public HazelcastMap( HazelcastClientTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal Hashtable Delegate = new Hashtable();

			  public override int Size()
			  {
					return Delegate.Count;
			  }

			  public override bool Empty
			  {
				  get
				  {
						return Delegate.Count == 0;
				  }
			  }

			  public override object Get( object key )
			  {

					return Delegate[key];
			  }

			  public override bool ContainsKey( object key )
			  {
					return Delegate.ContainsKey( key );
			  }

			  public override object Put( object key, object value )
			  {
					return Delegate[key] = value;
			  }

			  public override void PutAll( System.Collections.IDictionary m )
			  {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
					Delegate.putAll( m );
			  }

			  public override object Remove( object key )
			  {
					return Delegate.Remove( key );
			  }

			  public override void Clear()
			  {
					Delegate.Clear();
			  }

			  public override ICompletableFuture<object> GetAsync( object key )
			  {
					return null;
			  }

			  public override ICompletableFuture<object> PutAsync( object key, object value )
			  {
					return null;
			  }

			  public override ICompletableFuture<object> PutAsync( object key, object value, long ttl, TimeUnit timeunit )
			  {
					return null;
			  }

			  public override ICompletableFuture<Void> SetAsync( object o, object o2 )
			  {
					return null;
			  }

			  public override ICompletableFuture<Void> SetAsync( object o, object o2, long l, TimeUnit timeUnit )
			  {
					return null;
			  }

			  public override ICompletableFuture<object> RemoveAsync( object key )
			  {
					return null;
			  }

			  public override bool TryRemove( object key, long timeout, TimeUnit timeunit )
			  {
					return false;
			  }

			  public override bool TryPut( object key, object value, long timeout, TimeUnit timeunit )
			  {
					return false;
			  }

			  public override object Put( object key, object value, long ttl, TimeUnit timeunit )
			  {
					return Delegate[key] = value;
			  }

			  public override void PutTransient( object key, object value, long ttl, TimeUnit timeunit )
			  {

			  }

			  public override bool ContainsValue( object value )
			  {
					return Delegate.ContainsValue( value );
			  }

			  public override ISet<object> KeySet()
			  {
					return Delegate.Keys;
			  }

			  public override ICollection<object> Values()
			  {
					return Delegate.Values;
			  }

			  public override ISet<Entry<object, object>> EntrySet()
			  {
					return Delegate.SetOfKeyValuePairs();
			  }

			  public override ISet<object> KeySet( Predicate predicate )
			  {
					return null;
			  }

			  public override ISet<KeyValuePair<object, object>> EntrySet( Predicate predicate )
			  {
					return null;
			  }

			  public override System.Collections.ICollection Values( Predicate predicate )
			  {
					return null;
			  }

			  public override ISet<object> LocalKeySet()
			  {
					return null;
			  }

			  public override ISet<object> LocalKeySet( Predicate predicate )
			  {
					return null;
			  }

			  public override void AddIndex( string attribute, bool ordered )
			  {

			  }

			  public override LocalMapStats LocalMapStats
			  {
				  get
				  {
						return null;
				  }
			  }

			  public override object ExecuteOnKey( object key, EntryProcessor entryProcessor )
			  {
					return null;
			  }

			  public override void SubmitToKey( object key, EntryProcessor entryProcessor, ExecutionCallback callback )
			  {

			  }

			  public override ICompletableFuture SubmitToKey( object key, EntryProcessor entryProcessor )
			  {
					return null;
			  }

			  public override IDictionary<object, object> ExecuteOnEntries( EntryProcessor entryProcessor )
			  {
					return null;
			  }

			  public override IDictionary<object, object> ExecuteOnEntries( EntryProcessor entryProcessor, Predicate predicate )
			  {
					return null;
			  }

			  public override object Aggregate( Supplier supplier, Aggregation aggregation, JobTracker jobTracker )
			  {
					return null;
			  }

			  public override object Aggregate( Supplier supplier, Aggregation aggregation )
			  {
					return null;
			  }

			  public override IDictionary<object, object> ExecuteOnKeys( ISet<object> keys, EntryProcessor entryProcessor )
			  {
					return null;
			  }

			  public override object GetOrDefault( object key, object defaultValue )
			  {
					return Delegate.getOrDefault( key, defaultValue );
			  }

			  public override object PutIfAbsent( object key, object value )
			  {
					return Delegate.putIfAbsent( key, value );
			  }

			  public override object PutIfAbsent( object key, object value, long ttl, TimeUnit timeunit )
			  {
					return null;
			  }

			  public override bool Remove( object key, object value )
			  {
					return Delegate.Remove( key, value );
			  }

			  public override void Delete( object key )
			  {

			  }

			  public override void Flush()
			  {

			  }

			  public override void LoadAll( bool replaceExistingValues )
			  {

			  }

			  public override void LoadAll( ISet<object> keys, bool replaceExistingValues )
			  {

			  }

			  public override System.Collections.IDictionary GetAll( ISet<object> keys )
			  {
					return null;
			  }

			  public override bool Replace( object key, object oldValue, object newValue )
			  {
					return Delegate.replace( key, oldValue, newValue );
			  }

			  public override object Replace( object key, object value )
			  {
					return Delegate.replace( key, value );
			  }

			  public override void Set( object key, object value )
			  {

			  }

			  public override void Set( object key, object value, long ttl, TimeUnit timeunit )
			  {

			  }

			  public override void Lock( object key )
			  {

			  }

			  public override void Lock( object key, long leaseTime, TimeUnit timeUnit )
			  {

			  }

			  public override bool IsLocked( object key )
			  {
					return false;
			  }

			  public override bool TryLock( object key )
			  {
					return false;
			  }

			  public override bool TryLock( object key, long time, TimeUnit timeunit )
			  {
					return false;
			  }

			  public override bool TryLock( object key, long time, TimeUnit timeunit, long leaseTime, TimeUnit leaseTimeunit )
			  {
					return false;
			  }

			  public override void Unlock( object key )
			  {

			  }

			  public override void ForceUnlock( object key )
			  {

			  }

			  public override string AddLocalEntryListener( MapListener listener )
			  {
					return null;
			  }

			  public override string AddLocalEntryListener( EntryListener listener )
			  {
					return null;
			  }

			  public override string AddLocalEntryListener( MapListener listener, Predicate predicate, bool includeValue )
			  {
					return null;
			  }

			  public override string AddLocalEntryListener( EntryListener listener, Predicate predicate, bool includeValue )
			  {
					return null;
			  }

			  public override string AddLocalEntryListener( MapListener listener, Predicate predicate, object key, bool includeValue )
			  {
					return null;
			  }

			  public override string AddLocalEntryListener( EntryListener listener, Predicate predicate, object key, bool includeValue )
			  {
					return null;
			  }

			  public override string AddInterceptor( MapInterceptor interceptor )
			  {
					return null;
			  }

			  public override void RemoveInterceptor( string id )
			  {

			  }

			  public override string AddEntryListener( MapListener listener, bool includeValue )
			  {
					return null;
			  }

			  public override string AddEntryListener( EntryListener listener, bool includeValue )
			  {
					return null;
			  }

			  public override bool RemoveEntryListener( string id )
			  {
					return false;
			  }

			  public override string AddPartitionLostListener( MapPartitionLostListener listener )
			  {
					return null;
			  }

			  public override bool RemovePartitionLostListener( string id )
			  {
					return false;
			  }

			  public override string AddEntryListener( MapListener listener, object key, bool includeValue )
			  {
					return null;
			  }

			  public override string AddEntryListener( EntryListener listener, object key, bool includeValue )
			  {
					return null;
			  }

			  public override string AddEntryListener( MapListener listener, Predicate predicate, bool includeValue )
			  {
					return null;
			  }

			  public override string AddEntryListener( EntryListener listener, Predicate predicate, bool includeValue )
			  {
					return null;
			  }

			  public override string AddEntryListener( MapListener listener, Predicate predicate, object key, bool includeValue )
			  {
					return null;
			  }

			  public override string AddEntryListener( EntryListener listener, Predicate predicate, object key, bool includeValue )
			  {
					return null;
			  }

			  public override EntryView GetEntryView( object key )
			  {
					return null;
			  }

			  public override bool Evict( object key )
			  {
					return false;
			  }

			  public override void EvictAll()
			  {

			  }

			  public override object ComputeIfAbsent( object key, System.Func mappingFunction )
			  {
					return Delegate.computeIfAbsent( key, mappingFunction );
			  }

			  public override object ComputeIfPresent( object key, System.Func remappingFunction )
			  {
					return Delegate.computeIfPresent( key, remappingFunction );
			  }

			  public override object Compute( object key, System.Func remappingFunction )
			  {
					return Delegate.compute( key, remappingFunction );
			  }

			  public override object Merge( object key, object value, System.Func remappingFunction )
			  {
					return Delegate.merge( key, value, remappingFunction );
			  }

			  public override void ForEach( System.Action action )
			  {
					Delegate.forEach( action );
			  }

			  public override void ReplaceAll( System.Func function )
			  {
					Delegate.replaceAll( function );
			  }

			  public override bool Equals( object o )
			  {
					return Delegate.Equals( o );
			  }

			  public override int GetHashCode()
			  {
					return Delegate.GetHashCode();
			  }

			  public override string ToString()
			  {
					return Delegate.ToString();
			  }

			  public override string PartitionKey
			  {
				  get
				  {
						return null;
				  }
			  }

			  public override string Name
			  {
				  get
				  {
						return "name";
				  }
			  }

			  public override string ServiceName
			  {
				  get
				  {
						return "serviceName";
				  }
			  }

			  public override void Destroy()
			  {

			  }
		 }

		 private class HazelcastMultiMap : MultiMap<object, object>
		 {
			 private readonly HazelcastClientTest _outerInstance;

			 public HazelcastMultiMap( HazelcastClientTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal IDictionary<object, object> Delegate = new Dictionary<object, object>();

			  public override string PartitionKey
			  {
				  get
				  {
						throw new System.NotSupportedException();
				  }
			  }

			  public override string Name
			  {
				  get
				  {
						throw new System.NotSupportedException();
				  }
			  }

			  public override string ServiceName
			  {
				  get
				  {
						throw new System.NotSupportedException();
				  }
			  }

			  public override void Destroy()
			  {
					throw new System.NotSupportedException();
			  }

			  public override bool Put( object key, object value )
			  {
					if ( Delegate[key] != null )
					{
						 throw new System.NotSupportedException( "This is not a true multimap" );
					}
					Delegate[key] = value;
					return true;
			  }

			  public override ICollection<object> Get( object key )
			  {
					return asSet( Delegate[key] );
			  }

			  public override bool Remove( object key, object value )
			  {
					return Delegate.Remove( key, value );
			  }

			  public override ICollection<object> Remove( object key )
			  {
					return asSet( Delegate.Remove( key ) );
			  }

			  public override ISet<object> LocalKeySet()
			  {
					throw new System.NotSupportedException();
			  }

			  public override ISet<object> KeySet()
			  {
					throw new System.NotSupportedException();
			  }

			  public override ICollection<object> Values()
			  {
					return Delegate.Values;
			  }

			  public override ISet<KeyValuePair<object, object>> EntrySet()
			  {
					return Delegate.SetOfKeyValuePairs();
			  }

			  public override bool ContainsKey( object key )
			  {
					return Delegate.ContainsKey( key );
			  }

			  public override bool ContainsValue( object value )
			  {
					return Delegate.ContainsValue( value );
			  }

			  public override bool ContainsEntry( object key, object value )
			  {
					throw new System.NotSupportedException();
			  }

			  public override int Size()
			  {
					return Delegate.Count;
			  }

			  public override void Clear()
			  {
					Delegate.Clear();
			  }

			  public override int ValueCount( object key )
			  {
					throw new System.NotSupportedException();
			  }

			  public override string AddLocalEntryListener( EntryListener<object, object> listener )
			  {
					throw new System.NotSupportedException();
			  }

			  public override string AddEntryListener( EntryListener<object, object> listener, bool includeValue )
			  {
					throw new System.NotSupportedException();
			  }

			  public override bool RemoveEntryListener( string registrationId )
			  {
					throw new System.NotSupportedException();
			  }

			  public override string AddEntryListener( EntryListener<object, object> listener, object key, bool includeValue )
			  {
					throw new System.NotSupportedException();
			  }

			  public override void Lock( object key )
			  {
					throw new System.NotSupportedException();
			  }

			  public override void Lock( object key, long leaseTime, TimeUnit timeUnit )
			  {
					throw new System.NotSupportedException();
			  }

			  public override bool IsLocked( object key )
			  {
					throw new System.NotSupportedException();
			  }

			  public override bool TryLock( object key )
			  {
					throw new System.NotSupportedException();
			  }

			  public override bool TryLock( object key, long time, TimeUnit timeunit )
			  {
					throw new System.NotSupportedException();
			  }

			  public override bool TryLock( object key, long time, TimeUnit timeunit, long leaseTime, TimeUnit leaseTimeunit )
			  {
					throw new System.NotSupportedException();
			  }

			  public override void Unlock( object key )
			  {
					throw new System.NotSupportedException();
			  }

			  public override void ForceUnlock( object key )
			  {
					throw new System.NotSupportedException();
			  }

			  public override LocalMultiMapStats LocalMultiMapStats
			  {
				  get
				  {
						throw new System.NotSupportedException();
				  }
			  }

			  public override Result Aggregate<SuppliedValue, Result>( Supplier<object, object, SuppliedValue> supplier, Aggregation<object, SuppliedValue, Result> aggregation )
			  {
					throw new System.NotSupportedException();
			  }

			  public override Result Aggregate<SuppliedValue, Result>( Supplier<object, object, SuppliedValue> supplier, Aggregation<object, SuppliedValue, Result> aggregation, JobTracker jobTracker )
			  {
					throw new System.NotSupportedException();
			  }
		 }

		 private class HazelcastSet : ISet<object>
		 {
			 private readonly HazelcastClientTest _outerInstance;

			  internal ISet<object> Delegate;

			  internal HazelcastSet( HazelcastClientTest outerInstance )
			  {
				  this._outerInstance = outerInstance;
					this.Delegate = new HashSet<object>();
			  }

			  public override string PartitionKey
			  {
				  get
				  {
						throw new System.InvalidOperationException();
				  }
			  }

			  public override string Name
			  {
				  get
				  {
						throw new System.InvalidOperationException();
				  }
			  }

			  public override string ServiceName
			  {
				  get
				  {
						throw new System.InvalidOperationException();
				  }
			  }

			  public override void Destroy()
			  {
					throw new System.InvalidOperationException();
			  }

			  public override string AddItemListener( ItemListener<object> listener, bool includeValue )
			  {
					throw new System.InvalidOperationException();
			  }

			  public override bool RemoveItemListener( string registrationId )
			  {
					throw new System.InvalidOperationException();
			  }

			  public override int Size()
			  {
					return Delegate.Count;
			  }

			  public override bool Empty
			  {
				  get
				  {
						return Delegate.Count == 0;
				  }
			  }

			  public override bool Contains( object o )
			  {
					return Delegate.Contains( o );
			  }

			  public override IEnumerator<object> Iterator()
			  {
					return Delegate.GetEnumerator();
			  }

			  public override object[] ToArray()
			  {
					return Delegate.ToArray();
			  }

			  public override T[] ToArray<T>( T[] a )
			  {
					return Delegate.toArray( a );
			  }

			  public override bool Add( object o )
			  {
					return Delegate.Add( o );
			  }

			  public override bool Remove( object o )
			  {
					return Delegate.remove( o );
			  }

			  public override bool ContainsAll<T1>( ICollection<T1> c )
			  {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
					return Delegate.containsAll( c );
			  }

			  public override bool AddAll<T1>( ICollection<T1> c )
			  {
					return Delegate.addAll( c );
			  }

			  public override bool RetainAll<T1>( ICollection<T1> c )
			  {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'retainAll' method:
					return Delegate.retainAll( c );
			  }

			  public override bool RemoveAll<T1>( ICollection<T1> c )
			  {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
					return Delegate.removeAll( c );
			  }

			  public override void Clear()
			  {
					Delegate.Clear();
			  }

			  public override bool Equals( object o )
			  {
					return Delegate.SetEquals( o );
			  }

			  public override int GetHashCode()
			  {
					return Delegate.GetHashCode();
			  }

			  public override Spliterator<object> Spliterator()
			  {
					return Delegate.spliterator();
			  }
		 }

		 private class StubExecutorService : IExecutorService
		 {
			 private readonly HazelcastClientTest _outerInstance;

			 public StubExecutorService( HazelcastClientTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal ExecutorService Executor = Executors.newSingleThreadExecutor();

			  public override void Execute( ThreadStart command, MemberSelector memberSelector )
			  {

			  }

			  public override void ExecuteOnKeyOwner( ThreadStart command, object key )
			  {

			  }

			  public override void ExecuteOnMember( ThreadStart command, Member member )
			  {

			  }

			  public override void ExecuteOnMembers( ThreadStart command, ICollection<Member> members )
			  {

			  }

			  public override void ExecuteOnMembers( ThreadStart command, MemberSelector memberSelector )
			  {

			  }

			  public override void ExecuteOnAllMembers( ThreadStart command )
			  {

			  }

			  public override Future<T> Submit<T>( Callable<T> task, MemberSelector memberSelector )
			  {
					return null;
			  }

			  public override Future<T> SubmitToKeyOwner<T>( Callable<T> task, object key )
			  {
					return null;
			  }

			  public override Future<T> SubmitToMember<T>( Callable<T> task, Member member )
			  {
					return null;
			  }

			  public override IDictionary<Member, Future<T>> SubmitToMembers<T>( Callable<T> task, ICollection<Member> members )
			  {
					return null;
			  }

			  public override IDictionary<Member, Future<T>> SubmitToMembers<T>( Callable<T> task, MemberSelector memberSelector )
			  {
					return null;
			  }

			  public override IDictionary<Member, Future<T>> SubmitToAllMembers<T>( Callable<T> task )
			  {
					return null;
			  }

			  public override void Submit<T>( ThreadStart task, ExecutionCallback<T> callback )
			  {

			  }

			  public override void Submit<T>( ThreadStart task, MemberSelector memberSelector, ExecutionCallback<T> callback )
			  {

			  }

			  public override void SubmitToKeyOwner<T>( ThreadStart task, object key, ExecutionCallback<T> callback )
			  {

			  }

			  public override void SubmitToMember<T>( ThreadStart task, Member member, ExecutionCallback<T> callback )
			  {

			  }

			  public override void SubmitToMembers( ThreadStart task, ICollection<Member> members, MultiExecutionCallback callback )
			  {

			  }

			  public override void SubmitToMembers( ThreadStart task, MemberSelector memberSelector, MultiExecutionCallback callback )
			  {

			  }

			  public override void SubmitToAllMembers( ThreadStart task, MultiExecutionCallback callback )
			  {

			  }

			  public override void Submit<T>( Callable<T> task, ExecutionCallback<T> callback )
			  {

			  }

			  public override void Submit<T>( Callable<T> task, MemberSelector memberSelector, ExecutionCallback<T> callback )
			  {

			  }

			  public override void SubmitToKeyOwner<T>( Callable<T> task, object key, ExecutionCallback<T> callback )
			  {

			  }

			  public override void SubmitToMember<T>( Callable<T> task, Member member, ExecutionCallback<T> callback )
			  {

			  }

			  public override void SubmitToMembers<T>( Callable<T> task, ICollection<Member> members, MultiExecutionCallback callback )
			  {

			  }

			  public override void SubmitToMembers<T>( Callable<T> task, MemberSelector memberSelector, MultiExecutionCallback callback )
			  {

			  }

			  public override void SubmitToAllMembers<T>( Callable<T> task, MultiExecutionCallback callback )
			  {

			  }

			  public override LocalExecutorStats LocalExecutorStats
			  {
				  get
				  {
						return null;
				  }
			  }

			  public override string PartitionKey
			  {
				  get
				  {
						return null;
				  }
			  }

			  public override string Name
			  {
				  get
				  {
						return null;
				  }
			  }

			  public override string ServiceName
			  {
				  get
				  {
						return null;
				  }
			  }

			  public override void Destroy()
			  {

			  }

			  public override void Shutdown()
			  {

			  }

			  public override IList<ThreadStart> ShutdownNow()
			  {
					return null;
			  }

			  public override bool Shutdown
			  {
				  get
				  {
						return false;
				  }
			  }

			  public override bool Terminated
			  {
				  get
				  {
						return false;
				  }
			  }

			  public override bool AwaitTermination( long timeout, TimeUnit unit )
			  {
					return false;
			  }

			  public override Future<T> Submit<T>( Callable<T> task )
			  {
					return Executor.submit( task );
			  }

			  public override Future<T> Submit<T>( ThreadStart task, T result )
			  {
					return null;
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public java.util.concurrent.Future<?> submit(Runnable task)
			  public override Future<object> Submit( ThreadStart task )
			  {
					return null;
			  }

			  public override IList<Future<T>> InvokeAll<T, T1>( ICollection<T1> tasks ) where T1 : java.util.concurrent.Callable<T>
			  {
					return null;
			  }

			  public override IList<Future<T>> InvokeAll<T, T1>( ICollection<T1> tasks, long timeout, TimeUnit unit ) where T1 : java.util.concurrent.Callable<T>
			  {
					return null;
			  }

			  public override T InvokeAny<T, T1>( ICollection<T1> tasks ) where T1 : java.util.concurrent.Callable<T>
			  {
					return null;
			  }

			  public override T InvokeAny<T, T1>( ICollection<T1> tasks, long timeout, TimeUnit unit ) where T1 : java.util.concurrent.Callable<T>
			  {
					return null;
			  }

			  public override void Execute( ThreadStart command )
			  {

			  }
		 }
	}

}