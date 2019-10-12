using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
namespace Neo4Net.net
{
	using After = org.junit.After;
	using BeforeClass = org.junit.BeforeClass;
	using ClassRule = org.junit.ClassRule;
	using Test = org.junit.Test;


	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using TransportTestUtil = Neo4Net.Bolt.v1.transport.integration.TransportTestUtil;
	using SocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SocketConnection;
	using TransportConnection = Neo4Net.Bolt.v1.transport.socket.client.TransportConnection;
	using Neo4jPackV2 = Neo4Net.Bolt.v2.messaging.Neo4jPackV2;
	using Predicates = Neo4Net.Function.Predicates;
	using Neo4Net.Function;
	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Lock = Neo4Net.Graphdb.Lock;
	using Node = Neo4Net.Graphdb.Node;
	using Result = Neo4Net.Graphdb.Result;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using EnterpriseNeo4jRule = Neo4Net.Harness.junit.EnterpriseNeo4jRule;
	using Neo4jRule = Neo4Net.Harness.junit.Neo4jRule;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using NetworkConnectionTracker = Neo4Net.Kernel.api.net.NetworkConnectionTracker;
	using TrackedNetworkConnection = Neo4Net.Kernel.api.net.TrackedNetworkConnection;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using KernelTransactions = Neo4Net.Kernel.Impl.Api.KernelTransactions;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgSuccess;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.runtime.spi.StreamMatchers.eqRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.auth_enabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Transaction.Terminated;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_enabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.net.ConnectionTrackingIT.TestConnector.BOLT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.net.ConnectionTrackingIT.TestConnector.HTTP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.net.ConnectionTrackingIT.TestConnector.HTTPS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.configuration.ServerSettings.webserver_max_threads;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;
	using static Neo4Net.Test.server.HTTP.RawPayload;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.quotedJson;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.rawPayload;
	using static Neo4Net.Test.server.HTTP.Response;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.withBasicAuth;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringOrNoValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	public class ConnectionTrackingIT
	{
		 private const string NEO4_J_USER_PWD = "test";
		 private const string OTHER_USER = "otherUser";
		 private const string OTHER_USER_PWD = "test";

		 private static readonly IList<string> _listConnectionsProcedureColumns = Arrays.asList( "connectionId", "connectTime", "connector", "username", "userAgent", "serverAddress", "clientAddress" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.harness.junit.Neo4jRule neo4j = new org.neo4j.harness.junit.EnterpriseNeo4jRule().withConfig(auth_enabled, "true").withConfig("dbms.connector.https.enabled", "true").withConfig(webserver_max_threads, "50").withConfig(online_backup_enabled, org.neo4j.kernel.configuration.Settings.FALSE);
		 public static readonly Neo4jRule Neo4j = new EnterpriseNeo4jRule().withConfig(auth_enabled, "true").withConfig("dbms.connector.https.enabled", "true").withConfig(webserver_max_threads, "50").withConfig(online_backup_enabled, Settings.FALSE);

		 private static long _dummyNodeId;

		 private readonly ExecutorService _executor = Executors.newCachedThreadPool();
		 private readonly ISet<TransportConnection> _connections = ConcurrentDictionary.newKeySet();
		 private readonly TransportTestUtil _util = new TransportTestUtil( new Neo4jPackV2() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void beforeAll()
		 public static void BeforeAll()
		 {
			  ChangeDefaultPasswordForUserNeo4j( NEO4_J_USER_PWD );
			  CreateNewUser( OTHER_USER, OTHER_USER_PWD );
			  _dummyNodeId = CreateDummyNode();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void afterEach() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AfterEach()
		 {
			  foreach ( TransportConnection connection in _connections )
			  {
					try
					{
						 connection.Disconnect();
					}
					catch ( Exception )
					{
					}
			  }
			  foreach ( TrackedNetworkConnection connection in AcceptedConnectionsFromConnectionTracker() )
			  {
					try
					{
						 connection.Close();
					}
					catch ( Exception )
					{
					}
			  }
			  _executor.shutdownNow();
			  TerminateAllTransactions();
			  AwaitNumberOfAcceptedConnectionsToBe( 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListNoConnectionsWhenIdle() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListNoConnectionsWhenIdle()
		 {
			  VerifyConnectionCount( HTTP, null, 0 );
			  VerifyConnectionCount( HTTPS, null, 0 );
			  VerifyConnectionCount( BOLT, null, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListUnauthenticatedHttpConnections() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListUnauthenticatedHttpConnections()
		 {
			  TestListingOfUnauthenticatedConnections( 5, 0, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListUnauthenticatedHttpsConnections() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListUnauthenticatedHttpsConnections()
		 {
			  TestListingOfUnauthenticatedConnections( 0, 2, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListUnauthenticatedBoltConnections() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListUnauthenticatedBoltConnections()
		 {
			  TestListingOfUnauthenticatedConnections( 0, 0, 4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListUnauthenticatedConnections() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListUnauthenticatedConnections()
		 {
			  TestListingOfUnauthenticatedConnections( 3, 2, 7 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAuthenticatedHttpConnections() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAuthenticatedHttpConnections()
		 {
			  LockNodeAndExecute(_dummyNodeId, () =>
			  {
				for ( int i = 0; i < 4; i++ )
				{
					 UpdateNodeViaHttp( _dummyNodeId, "neo4j", NEO4_J_USER_PWD );
				}
				for ( int i = 0; i < 3; i++ )
				{
					 UpdateNodeViaHttp( _dummyNodeId, OTHER_USER, OTHER_USER_PWD );
				}

				AwaitNumberOfAuthenticatedConnectionsToBe( 7 );
				VerifyAuthenticatedConnectionCount( HTTP, "neo4j", 4 );
				VerifyAuthenticatedConnectionCount( HTTP, OTHER_USER, 3 );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAuthenticatedHttpsConnections() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAuthenticatedHttpsConnections()
		 {
			  LockNodeAndExecute(_dummyNodeId, () =>
			  {
				for ( int i = 0; i < 4; i++ )
				{
					 UpdateNodeViaHttps( _dummyNodeId, "neo4j", NEO4_J_USER_PWD );
				}
				for ( int i = 0; i < 5; i++ )
				{
					 UpdateNodeViaHttps( _dummyNodeId, OTHER_USER, OTHER_USER_PWD );
				}

				AwaitNumberOfAuthenticatedConnectionsToBe( 9 );
				VerifyAuthenticatedConnectionCount( HTTPS, "neo4j", 4 );
				VerifyAuthenticatedConnectionCount( HTTPS, OTHER_USER, 5 );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAuthenticatedBoltConnections() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAuthenticatedBoltConnections()
		 {
			  LockNodeAndExecute(_dummyNodeId, () =>
			  {
				for ( int i = 0; i < 2; i++ )
				{
					 UpdateNodeViaBolt( _dummyNodeId, "neo4j", NEO4_J_USER_PWD );
				}
				for ( int i = 0; i < 5; i++ )
				{
					 UpdateNodeViaBolt( _dummyNodeId, OTHER_USER, OTHER_USER_PWD );
				}

				AwaitNumberOfAuthenticatedConnectionsToBe( 7 );
				VerifyAuthenticatedConnectionCount( BOLT, "neo4j", 2 );
				VerifyAuthenticatedConnectionCount( BOLT, OTHER_USER, 5 );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAuthenticatedConnections() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAuthenticatedConnections()
		 {
			  LockNodeAndExecute(_dummyNodeId, () =>
			  {
				for ( int i = 0; i < 4; i++ )
				{
					 UpdateNodeViaBolt( _dummyNodeId, OTHER_USER, OTHER_USER_PWD );
				}
				for ( int i = 0; i < 1; i++ )
				{
					 UpdateNodeViaHttp( _dummyNodeId, "neo4j", NEO4_J_USER_PWD );
				}
				for ( int i = 0; i < 5; i++ )
				{
					 UpdateNodeViaHttps( _dummyNodeId, "neo4j", NEO4_J_USER_PWD );
				}

				AwaitNumberOfAuthenticatedConnectionsToBe( 10 );
				VerifyConnectionCount( BOLT, OTHER_USER, 4 );
				VerifyConnectionCount( HTTP, "neo4j", 1 );
				VerifyConnectionCount( HTTPS, "neo4j", 5 );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKillHttpConnection() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKillHttpConnection()
		 {
			  TestKillingOfConnections( Neo4j.httpURI(), HTTP, 4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKillHttpsConnection() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKillHttpsConnection()
		 {
			  TestKillingOfConnections( Neo4j.httpsURI(), HTTPS, 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKillBoltConnection() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKillBoltConnection()
		 {
			  TestKillingOfConnections( Neo4j.boltURI(), BOLT, 3 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testListingOfUnauthenticatedConnections(int httpCount, int httpsCount, int boltCount) throws Exception
		 private void TestListingOfUnauthenticatedConnections( int httpCount, int httpsCount, int boltCount )
		 {
			  for ( int i = 0; i < httpCount; i++ )
			  {
					ConnectSocketTo( Neo4j.httpURI() );
			  }

			  for ( int i = 0; i < httpsCount; i++ )
			  {
					ConnectSocketTo( Neo4j.httpsURI() );
			  }

			  for ( int i = 0; i < boltCount; i++ )
			  {
					ConnectSocketTo( Neo4j.boltURI() );
			  }

			  AwaitNumberOfAcceptedConnectionsToBe( httpCount + httpsCount + boltCount );

			  VerifyConnectionCount( HTTP, null, httpCount );
			  VerifyConnectionCount( HTTPS, null, httpsCount );
			  VerifyConnectionCount( BOLT, null, boltCount );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testKillingOfConnections(java.net.URI uri, TestConnector connector, int count) throws Exception
		 private void TestKillingOfConnections( URI uri, TestConnector connector, int count )
		 {
			  IList<TransportConnection> socketConnections = new List<TransportConnection>();
			  for ( int i = 0; i < count; i++ )
			  {
					socketConnections.Add( ConnectSocketTo( uri ) );
			  }

			  AwaitNumberOfAcceptedConnectionsToBe( count );
			  VerifyConnectionCount( connector, null, count );

			  KillAcceptedConnectionViaBolt();
			  VerifyConnectionCount( connector, null, 0 );

			  foreach ( TransportConnection socketConnection in socketConnections )
			  {
					AssertConnectionBreaks( socketConnection );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.bolt.v1.transport.socket.client.TransportConnection connectSocketTo(java.net.URI uri) throws java.io.IOException
		 private TransportConnection ConnectSocketTo( URI uri )
		 {
			  SocketConnection connection = new SocketConnection();
			  _connections.Add( connection );
			  connection.Connect( new HostnamePort( uri.Host, uri.Port ) );
			  return connection;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void awaitNumberOfAuthenticatedConnectionsToBe(int n) throws InterruptedException
		 private static void AwaitNumberOfAuthenticatedConnectionsToBe( int n )
		 {
			  assertEventually( "Unexpected number of authenticated connections", ConnectionTrackingIT.authenticatedConnectionsFromConnectionTracker, hasSize( n ), 1, MINUTES );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void awaitNumberOfAcceptedConnectionsToBe(int n) throws InterruptedException
		 private static void AwaitNumberOfAcceptedConnectionsToBe( int n )
		 {
			  assertEventually( _connections => "Unexpected number of accepted connections: " + _connections, ConnectionTrackingIT.acceptedConnectionsFromConnectionTracker, hasSize( n ), 1, MINUTES );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void verifyConnectionCount(TestConnector connector, String username, int expectedCount) throws InterruptedException
		 private static void VerifyConnectionCount( TestConnector connector, string username, int expectedCount )
		 {
			  VerifyConnectionCount( connector, username, expectedCount, false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void verifyAuthenticatedConnectionCount(TestConnector connector, String username, int expectedCount) throws InterruptedException
		 private static void VerifyAuthenticatedConnectionCount( TestConnector connector, string username, int expectedCount )
		 {
			  VerifyConnectionCount( connector, username, expectedCount, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void verifyConnectionCount(TestConnector connector, String username, int expectedCount, boolean expectAuthenticated) throws InterruptedException
		 private static void VerifyConnectionCount( TestConnector connector, string username, int expectedCount, bool expectAuthenticated )
		 {
			  assertEventually( _connections => "Unexpected number of listed connections: " + _connections, () => ListMatchingConnection(connector, username, expectAuthenticated), hasSize(expectedCount), 1, MINUTES );
		 }

		 private static IList<IDictionary<string, object>> ListMatchingConnection( TestConnector connector, string username, bool expectAuthenticated )
		 {
			  Result result = Neo4j.GraphDatabaseService.execute( "CALL dbms.listConnections()" );
			  assertEquals( _listConnectionsProcedureColumns, result.Columns() );
			  IList<IDictionary<string, object>> records = result.ToList();

			  IList<IDictionary<string, object>> matchingRecords = new List<IDictionary<string, object>>();
			  foreach ( IDictionary<string, object> record in records )
			  {
					string actualConnector = record["connector"].ToString();
					assertNotNull( actualConnector );
					object actualUsername = record["username"];
					if ( Objects.Equals( connector.name, actualConnector ) && Objects.Equals( username, actualUsername ) )
					{
						 if ( expectAuthenticated )
						 {
							  assertEquals( connector.userAgent, record["userAgent"] );
						 }

						 matchingRecords.Add( record );
					}

					assertThat( record["connectionId"].ToString(), startsWith(actualConnector) );
					OffsetDateTime connectTime = ISO_OFFSET_DATE_TIME.parse( record["connectTime"].ToString(), OffsetDateTime.from );
					assertNotNull( connectTime );
					assertThat( record["serverAddress"], instanceOf( typeof( string ) ) );
					assertThat( record["clientAddress"], instanceOf( typeof( string ) ) );
			  }
			  return matchingRecords;
		 }

		 private static IList<TrackedNetworkConnection> AuthenticatedConnectionsFromConnectionTracker()
		 {
			  return AcceptedConnectionsFromConnectionTracker().Where(connection => connection.username() != null).ToList();
		 }

		 private static IList<TrackedNetworkConnection> AcceptedConnectionsFromConnectionTracker()
		 {
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) Neo4j.GraphDatabaseService;
			  NetworkConnectionTracker connectionTracker = Db.DependencyResolver.resolveDependency( typeof( NetworkConnectionTracker ) );
			  return connectionTracker.ActiveConnections();
		 }

		 private static void ChangeDefaultPasswordForUserNeo4j( string newPassword )
		 {
			  string changePasswordUri = Neo4j.httpURI().resolve("user/neo4j/password").ToString();
			  Response response = withBasicAuth( "neo4j", "neo4j" ).POST( changePasswordUri, quotedJson( "{'password':'" + newPassword + "'}" ) );

			  assertEquals( 200, response.status() );
		 }

		 private static void CreateNewUser( string username, string password )
		 {
			  string uri = TxCommitUri( false );

			  Response response1 = withBasicAuth( "neo4j", NEO4_J_USER_PWD ).POST( uri, Query( "CALL dbms.security.createUser(\\\"" + username + "\\\", \\\"" + password + "\\\", false)" ) );
			  assertEquals( 200, response1.status() );

			  Response response2 = withBasicAuth( "neo4j", NEO4_J_USER_PWD ).POST( uri, Query( "CALL dbms.security.addRoleToUser(\\\"admin\\\", \\\"" + username + "\\\")" ) );
			  assertEquals( 200, response2.status() );
		 }

		 private static long CreateDummyNode()
		 {
			  using ( Result result = Neo4j.GraphDatabaseService.execute( "CREATE (n:Dummy) RETURN id(n) AS i" ) )
			  {
					IDictionary<string, object> record = single( result );
					return ( long ) record["i"];
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void lockNodeAndExecute(long id, org.neo4j.function.ThrowingAction<Exception> action) throws Exception
		 private static void LockNodeAndExecute( long id, ThrowingAction<Exception> action )
		 {
			  GraphDatabaseService db = Neo4j.GraphDatabaseService;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.getNodeById( id );
					Lock @lock = tx.AcquireWriteLock( node );
					try
					{
						 action.Apply();
					}
					finally
					{
						 @lock.Release();
					}
					tx.Failure();
			  }
		 }

		 private Future<Response> UpdateNodeViaHttp( long id, string username, string password )
		 {
			  return UpdateNodeViaHttp( id, false, username, password );
		 }

		 private Future<Response> UpdateNodeViaHttps( long id, string username, string password )
		 {
			  return UpdateNodeViaHttp( id, true, username, password );
		 }

		 private Future<Response> UpdateNodeViaHttp( long id, bool encrypted, string username, string password )
		 {
			  string uri = TxCommitUri( encrypted );
			  string userAgent = encrypted ? HTTPS.userAgent : HTTP.userAgent;

			  return _executor.submit( () => withBasicAuth(username, password).withHeaders(HttpHeaders.USER_AGENT, userAgent).POST(uri, Query("MATCH (n) WHERE id(n) = " + id + " SET n.prop = 42")) );
		 }

		 private Future<Void> UpdateNodeViaBolt( long id, string username, string password )
		 {
			  return _executor.submit(() =>
			  {
				ConnectSocketTo( Neo4j.boltURI() ).send(_util.defaultAcceptedVersions()).send(_util.chunk(InitMessage(username, password))).send(_util.chunk(new RunMessage("MATCH (n) WHERE id(n) = " + id + " SET n.prop = 42"), PullAllMessage.INSTANCE));

				return null;
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void killAcceptedConnectionViaBolt() throws Exception
		 private void KillAcceptedConnectionViaBolt()
		 {
			  foreach ( TrackedNetworkConnection connection in AcceptedConnectionsFromConnectionTracker() )
			  {
					KillConnectionViaBolt( connection );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void killConnectionViaBolt(org.neo4j.kernel.api.net.TrackedNetworkConnection trackedConnection) throws Exception
		 private void KillConnectionViaBolt( TrackedNetworkConnection trackedConnection )
		 {
			  string id = trackedConnection.Id();
			  string user = trackedConnection.Username();

			  TransportConnection connection = ConnectSocketTo( Neo4j.boltURI() );
			  try
			  {
					connection.Send( _util.defaultAcceptedVersions() ).send(_util.chunk(InitMessage("neo4j", NEO4_J_USER_PWD))).send(_util.chunk(new RunMessage("CALL dbms.killConnection('" + id + "')"), PullAllMessage.INSTANCE));

					assertThat( connection, _util.eventuallyReceivesSelectedProtocolVersion() );
					assertThat( connection, _util.eventuallyReceives( msgSuccess(), msgSuccess(), msgRecord(eqRecord(any(typeof(Value)), equalTo(stringOrNoValue(user)), equalTo(stringValue("Connection found")))), msgSuccess() ) );
			  }
			  finally
			  {
					connection.Disconnect();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertConnectionBreaks(org.neo4j.bolt.v1.transport.socket.client.TransportConnection connection) throws java.util.concurrent.TimeoutException
		 private static void AssertConnectionBreaks( TransportConnection connection )
		 {
			  Predicates.await( () => ConnectionIsBroken(connection), 1, MINUTES );
		 }

		 private static bool ConnectionIsBroken( TransportConnection connection )
		 {
			  try
			  {
					connection.Send( new sbyte[]{ 1 } );
					connection.Recv( 1 );
					return false;
			  }
			  catch ( SocketException )
			  {
					return true;
			  }
			  catch ( IOException )
			  {
					return false;
			  }
			  catch ( InterruptedException e )
			  {
					Thread.CurrentThread.Interrupt();
					throw new Exception( e );
			  }
		 }

		 private static void TerminateAllTransactions()
		 {
			  DependencyResolver dependencyResolver = ( ( GraphDatabaseAPI ) Neo4j.GraphDatabaseService ).DependencyResolver;
			  KernelTransactions kernelTransactions = dependencyResolver.ResolveDependency( typeof( KernelTransactions ) );
			  kernelTransactions.ActiveTransactions().forEach(h => h.markForTermination(Terminated));
		 }

		 private static string TxCommitUri( bool encrypted )
		 {
			  URI baseUri = encrypted ? Neo4j.httpsURI() : Neo4j.httpURI();
			  return baseUri.resolve( "db/data/transaction/commit" ).ToString();
		 }

		 private static RawPayload Query( string statement )
		 {
			  return rawPayload( "{\"statements\":[{\"statement\":\"" + statement + "\"}]}" );
		 }

		 private static InitMessage InitMessage( string username, string password )
		 {
			  IDictionary<string, object> authToken = map( "scheme", "basic", "principal", username, "credentials", password );
			  return new InitMessage( BOLT.userAgent, authToken );
		 }

		 internal sealed class TestConnector
		 {
			  public static readonly TestConnector Http = new TestConnector( "Http", InnerEnum.Http, "http", "http-user-agent" );
			  public static readonly TestConnector Https = new TestConnector( "Https", InnerEnum.Https, "https", "https-user-agent" );
			  public static readonly TestConnector Bolt = new TestConnector( "Bolt", InnerEnum.Bolt, "bolt", "bolt-user-agent" );

			  private static readonly IList<TestConnector> valueList = new List<TestConnector>();

			  static TestConnector()
			  {
				  valueList.Add( Http );
				  valueList.Add( Https );
				  valueList.Add( Bolt );
			  }

			  public enum InnerEnum
			  {
				  Http,
				  Https,
				  Bolt
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Final String;
			  internal Final String;

			  internal TestConnector( string name, InnerEnum innerEnum, string name, string userAgent )
			  {
					this.Name = name;
					this.UserAgent = userAgent;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			 public static IList<TestConnector> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static TestConnector valueOf( string name )
			 {
				 foreach ( TestConnector enumInstance in TestConnector.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }
	}

}