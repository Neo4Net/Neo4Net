using System;
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
namespace Neo4Net.causalclustering.scenarios
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CatchupPollingProcess = Neo4Net.causalclustering.catchup.tx.CatchupPollingProcess;
	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using Role = Neo4Net.causalclustering.core.consensus.roles.Role;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using ReadReplica = Neo4Net.causalclustering.discovery.ReadReplica;
	using JULogging = Neo4Net.driver.Internal.logging.JULogging;
	using AccessMode = Neo4Net.driver.v1.AccessMode;
	using AuthTokens = Neo4Net.driver.v1.AuthTokens;
	using Config = Neo4Net.driver.v1.Config;
	using Driver = Neo4Net.driver.v1.Driver;
	using GraphDatabase = Neo4Net.driver.v1.GraphDatabase;
	using Record = Neo4Net.driver.v1.Record;
	using Session = Neo4Net.driver.v1.Session;
	using StatementResult = Neo4Net.driver.v1.StatementResult;
	using Transaction = Neo4Net.driver.v1.Transaction;
	using TransactionWork = Neo4Net.driver.v1.TransactionWork;
	using Values = Neo4Net.driver.v1.Values;
	using ClientException = Neo4Net.driver.v1.exceptions.ClientException;
	using ServiceUnavailableException = Neo4Net.driver.v1.exceptions.ServiceUnavailableException;
	using SessionExpiredException = Neo4Net.driver.v1.exceptions.SessionExpiredException;
	using ServerInfo = Neo4Net.driver.v1.summary.ServerInfo;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.driver.v1.Values.parameters;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.assertion.Assert.assertEventually;

	public class BoltCausalClusteringIT
	{
		 private const long DEFAULT_TIMEOUT_MS = 15_000;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.causalclustering.ClusterRule clusterRule = new org.Neo4Net.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.SuppressOutput suppressOutput = org.Neo4Net.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExecuteReadAndWritesWhenDriverSuppliedWithAddressOfLeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExecuteReadAndWritesWhenDriverSuppliedWithAddressOfLeader()
		 {
			  // given
			  _cluster = ClusterRule.withNumberOfReadReplicas( 0 ).startCluster();
			  _cluster.coreTx((db, tx) =>
			  {
									 Iterators.count( Db.execute( "CREATE CONSTRAINT ON (p:Person) ASSERT p.name is UNIQUE" ) );
									 tx.success();
			  });

			  // when
			  int count = ExecuteWriteAndReadThroughBolt( _cluster.awaitLeader() );

			  // then
			  assertEquals( 1, count );
			  _cluster.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExecuteReadAndWritesWhenDriverSuppliedWithAddressOfFollower() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExecuteReadAndWritesWhenDriverSuppliedWithAddressOfFollower()
		 {
			  // given
			  _cluster = ClusterRule.withNumberOfReadReplicas( 0 ).startCluster();
			  _cluster.coreTx((db, tx) =>
			  {
									 Iterators.count( Db.execute( "CREATE CONSTRAINT ON (p:Person) ASSERT p.name is UNIQUE" ) );
									 tx.success();
			  });

			  // when
			  int count = ExecuteWriteAndReadThroughBolt( _cluster.getMemberWithRole( Role.FOLLOWER ) );

			  // then
			  assertEquals( 1, count );
			  _cluster.shutdown();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static int executeWriteAndReadThroughBolt(org.Neo4Net.causalclustering.discovery.CoreClusterMember core) throws java.util.concurrent.TimeoutException
		 private static int ExecuteWriteAndReadThroughBolt( CoreClusterMember core )
		 {
			  using ( Driver driver = GraphDatabase.driver( core.RoutingURI(), AuthTokens.basic("Neo4Net", "Neo4Net") ) )
			  {

					return InExpirableSession(driver, d => d.session(AccessMode.WRITE), session =>
					{
					 // when
					 session.run( "MERGE (n:Person {name: 'Jim'})" ).consume();
					 Record record = session.run( "MATCH (n:Person) RETURN COUNT(*) AS count" ).next();
					 return record.get( "count" ).asInt();
					});
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToWriteOnAReadSession() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBeAbleToWriteOnAReadSession()
		 {
			  // given
			  _cluster = ClusterRule.withNumberOfReadReplicas( 0 ).startCluster();

			  assertEventually("Failed to execute write query on read server", () =>
			  {
				SwitchLeader( _cluster.awaitLeader() );
				CoreClusterMember leader = _cluster.awaitLeader();
				Driver driver = GraphDatabase.driver( leader.routingURI(), AuthTokens.basic("Neo4Net", "Neo4Net") );

				try
				{
					using ( Session session = driver.session( AccessMode.READ ) )
					{
						 // when
						 session.run( "CREATE (n:Person {name: 'Jim'})" ).consume();
						 return false;
					}
				}
				catch ( ClientException ex )
				{
					 assertEquals( "Write queries cannot be performed in READ access mode.", ex.Message );
					 return true;
				}
				finally
				{
					 driver.close();
				}
			  }, @is( true ), 30, SECONDS);
			  _cluster.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sessionShouldExpireOnLeaderSwitch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SessionShouldExpireOnLeaderSwitch()
		 {
			  // given
			  _cluster = ClusterRule.withNumberOfReadReplicas( 0 ).startCluster();

			  CoreClusterMember leader = _cluster.awaitLeader();

			  Driver driver = GraphDatabase.driver( leader.RoutingURI(), AuthTokens.basic("Neo4Net", "Neo4Net") );
			  try
			  {
					  using ( Session session = driver.session() )
					  {
						session.run( "CREATE (n:Person {name: 'Jim'})" ).consume();
      
						// when
						SwitchLeader( leader );
      
						session.run( "CREATE (n:Person {name: 'Mark'})" ).consume();
      
						fail( "Should have thrown exception" );
					  }
			  }
			  catch ( SessionExpiredException sep )
			  {
					// then
					assertEquals( string.Format( "Server at {0} no longer accepts writes", leader.BoltAdvertisedAddress() ), sep.Message );
			  }
			  finally
			  {
					driver.close();
			  }

			  _cluster.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToGetClusterOverview() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToGetClusterOverview()
		 {
			  // given
			  _cluster = ClusterRule.withNumberOfReadReplicas( 0 ).startCluster();

			  CoreClusterMember leader = _cluster.awaitLeader();

			  Driver driver = GraphDatabase.driver( leader.RoutingURI(), AuthTokens.basic("Neo4Net", "Neo4Net") );
			  try
			  {
					  using ( Session session = driver.session() )
					  {
						StatementResult overview = session.run( "CALL dbms.cluster.overview" );
						assertThat( overview.list(), hasSize(3) );
					  }
			  }
			  finally
			  {
					driver.close();
			  }

			  _cluster.shutdown();
		 }

		 /// <summary>
		 /// Keeps the leader different than the initial leader.
		 /// </summary>
		 private class LeaderSwitcher : ThreadStart
		 {
			 private readonly BoltCausalClusteringIT _outerInstance;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.Neo4Net.causalclustering.discovery.Cluster<?> cluster;
			  internal readonly Cluster<object> Cluster;
			  internal readonly System.Threading.CountdownEvent SwitchCompleteLatch;
			  internal CoreClusterMember InitialLeader;
			  internal CoreClusterMember CurrentLeader;

			  internal Thread Thread;
			  internal bool Stopped;
			  internal Exception Throwable;

			  internal LeaderSwitcher<T1>( BoltCausalClusteringIT outerInstance, Cluster<T1> cluster, System.Threading.CountdownEvent switchCompleteLatch )
			  {
				  this._outerInstance = outerInstance;
					this.Cluster = cluster;
					this.SwitchCompleteLatch = switchCompleteLatch;
			  }

			  public override void Run()
			  {
					try
					{
						 InitialLeader = Cluster.awaitLeader();

						 while ( !Stopped )
						 {
							  CurrentLeader = Cluster.awaitLeader();
							  if ( CurrentLeader == InitialLeader )
							  {
									outerInstance.switchLeader( InitialLeader );
									CurrentLeader = Cluster.awaitLeader();
							  }
							  else
							  {
									SwitchCompleteLatch.Signal();
							  }

							  Thread.Sleep( 100 );
						 }
					}
					catch ( Exception e )
					{
						 Throwable = e;
					}
			  }

			  internal virtual void Start()
			  {
					if ( Thread == null )
					{
						 Thread = new Thread( this );
						 Thread.Start();
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void stop() throws Throwable
			  internal virtual void Stop()
			  {
					if ( Thread != null )
					{
						 Stopped = true;
						 Thread.Join();
					}

					AssertNoException();
			  }

			  internal virtual bool HadLeaderSwitch()
			  {
					return CurrentLeader != InitialLeader;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void assertNoException() throws Throwable
			  internal virtual void AssertNoException()
			  {
					if ( Throwable != null )
					{
						 throw Throwable;
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickANewServerToWriteToOnLeaderSwitch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPickANewServerToWriteToOnLeaderSwitch()
		 {
			  // given
			  _cluster = ClusterRule.withNumberOfReadReplicas( 0 ).startCluster();

			  CoreClusterMember leader = _cluster.awaitLeader();

			  System.Threading.CountdownEvent leaderSwitchLatch = new System.Threading.CountdownEvent( 1 );

			  LeaderSwitcher leaderSwitcher = new LeaderSwitcher( this, _cluster, leaderSwitchLatch );

			  Config config = Config.build().withLogging(new JULogging(Level.OFF)).toConfig();
			  ISet<string> seenAddresses = new HashSet<string>();
			  try
			  {
					  using ( Driver driver = GraphDatabase.driver( leader.RoutingURI(), AuthTokens.basic("Neo4Net", "Neo4Net"), config ) )
					  {
						bool success = false;
      
						long deadline = DateTimeHelper.CurrentUnixTimeMillis() + (30 * 1000);
      
						while ( !success )
						{
							 if ( DateTimeHelper.CurrentUnixTimeMillis() > deadline )
							 {
								  fail( "Failed to write to the new leader in time. Addresses seen: " + seenAddresses );
							 }
      
							 try
							 {
									 using ( Session session = driver.session( AccessMode.WRITE ) )
									 {
									  StatementResult result = session.run( "CREATE (p:Person)" );
									  ServerInfo server = result.summary().server();
									  seenAddresses.Add( server.address() );
									  success = seenAddresses.Count >= 2;
									 }
							 }
							 catch ( Exception )
							 {
								  Thread.Sleep( 100 );
							 }
      
							 /*
							  * Having the latch release here ensures that we've done at least one pass through the loop, which means
							  * we've completed a connection before the forced master switch.
							  */
							 if ( seenAddresses.Count > 0 && !success )
							 {
								  leaderSwitcher.Start();
								  leaderSwitchLatch.await();
							 }
						}
					  }
			  }
			  finally
			  {
					leaderSwitcher.Stop();
					assertTrue( leaderSwitcher.HadLeaderSwitch() );
					assertThat( seenAddresses.Count, greaterThanOrEqualTo( 2 ) );
			  }

			  _cluster.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sessionCreationShouldFailIfCallingDiscoveryProcedureOnEdgeServer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SessionCreationShouldFailIfCallingDiscoveryProcedureOnEdgeServer()
		 {
			  // given
			  _cluster = ClusterRule.withNumberOfReadReplicas( 1 ).startCluster();

			  ReadReplica readReplica = _cluster.getReadReplicaById( 0 );
			  try
			  {
					GraphDatabase.driver( readReplica.RoutingURI(), AuthTokens.basic("Neo4Net", "Neo4Net") );
					fail( "Should have thrown an exception using a read replica address for routing" );
			  }
			  catch ( ServiceUnavailableException ex )
			  {
					// then
					assertThat( ex.Message, startsWith( "Failed to run" ) );
			  }

			  _cluster.shutdown();
		 }

		 /*
		    Create a session with empty arg list (no AccessMode arg), in a driver that was initialized with a bolt+routing
		    URI, and ensure that it
		    a) works against the Leader for reads and writes before a leader switch, and
		    b) receives a SESSION EXPIRED after a leader switch, and
		    c) keeps working if a new session is created after that exception, again with no access mode specified.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadAndWriteToANewSessionCreatedAfterALeaderSwitch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadAndWriteToANewSessionCreatedAfterALeaderSwitch()
		 {
			  // given
			  _cluster = ClusterRule.withNumberOfReadReplicas( 1 ).startCluster();
			  CoreClusterMember leader = _cluster.awaitLeader();

			  using ( Driver driver = GraphDatabase.driver( leader.RoutingURI(), AuthTokens.basic("Neo4Net", "Neo4Net") ) )
			  {
					InExpirableSession(driver, Driver.session, session =>
					{
					 // execute a write/read query
					 session.run( "CREATE (p:Person {name: {name} })", Values.parameters( "name", "Jim" ) );
					 Record record = session.run( "MATCH (n:Person) RETURN COUNT(*) AS count" ).next();
					 assertEquals( 1, record.get( "count" ).asInt() );

					 // change leader

					 try
					 {
						  SwitchLeader( leader );
						  session.run( "CREATE (p:Person {name: {name} })", Values.parameters( "name", "Mark" ) ).consume();
						  fail( "Should have thrown an exception as the leader went away mid session" );
					 }
					 catch ( SessionExpiredException sep )
					 {
						  // then
						  assertEquals( string.Format( "Server at {0} no longer accepts writes", leader.BoltAdvertisedAddress() ), sep.Message );
					 }
					 catch ( InterruptedException )
					 {
						  // ignored
					 }
					 return null;
					});

					InExpirableSession(driver, Driver.session, session =>
					{
					 // execute a write/read query
					 session.run( "CREATE (p:Person {name: {name} })", Values.parameters( "name", "Jim" ) );
					 Record record = session.run( "MATCH (n:Person) RETURN COUNT(*) AS count" ).next();
					 assertEquals( 2, record.get( "count" ).asInt() );
					 return null;
					});

					_cluster.shutdown();
			  }
		 }

		 // Ensure that Bookmarks work with single instances using a driver created using a bolt[not+routing] URI.
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void bookmarksShouldWorkWithDriverPinnedToSingleServer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BookmarksShouldWorkWithDriverPinnedToSingleServer()
		 {
			  // given
			  _cluster = ClusterRule.withNumberOfReadReplicas( 1 ).startCluster();
			  CoreClusterMember leader = _cluster.awaitLeader();

			  using ( Driver driver = GraphDatabase.driver( leader.DirectURI(), AuthTokens.basic("Neo4Net", "Neo4Net") ) )
			  {
					string bookmark = InExpirableSession(driver, Driver.session, session =>
					{
					 using ( Transaction tx = session.BeginTransaction() )
					 {
						  tx.run( "CREATE (p:Person {name: {name} })", Values.parameters( "name", "Alistair" ) );
						  tx.success();
					 }

					 return session.lastBookmark();
					});

					assertNotNull( bookmark );

					using ( Session session = driver.session( bookmark ), Transaction tx = session.BeginTransaction() )
					{
						 Record record = tx.run( "MATCH (n:Person) RETURN COUNT(*) AS count" ).next();
						 assertEquals( 1, record.get( "count" ).asInt() );
						 tx.success();
					}
			  }
			  _cluster.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseBookmarkFromAReadSessionInAWriteSession() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseBookmarkFromAReadSessionInAWriteSession()
		 {
			  // given
			  _cluster = ClusterRule.withNumberOfReadReplicas( 1 ).startCluster();
			  CoreClusterMember leader = _cluster.awaitLeader();

			  using ( Driver driver = GraphDatabase.driver( leader.DirectURI(), AuthTokens.basic("Neo4Net", "Neo4Net") ) )
			  {
					InExpirableSession(driver, d => d.session(AccessMode.WRITE), session =>
					{
					 session.run( "CREATE (p:Person {name: {name} })", Values.parameters( "name", "Jim" ) );
					 return null;
					});

					string bookmark;
					using ( Session session = driver.session( AccessMode.READ ) )
					{
						 using ( Transaction tx = session.BeginTransaction() )
						 {
							  tx.run( "MATCH (n:Person) RETURN COUNT(*) AS count" ).next();
							  tx.success();
						 }

						 bookmark = session.lastBookmark();
					}

					assertNotNull( bookmark );

					InExpirableSession(driver, d => d.session(AccessMode.WRITE, bookmark), session =>
					{
					 using ( Transaction tx = session.BeginTransaction() )
					 {
						  tx.run( "CREATE (p:Person {name: {name} })", Values.parameters( "name", "Alistair" ) );
						  tx.success();
					 }

					 return null;
					});

					using ( Session session = driver.session() )
					{
						 Record record = session.run( "MATCH (n:Person) RETURN COUNT(*) AS count" ).next();
						 assertEquals( 2, record.get( "count" ).asInt() );
					}
			  }
			  _cluster.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseBookmarkFromAWriteSessionInAReadSession() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseBookmarkFromAWriteSessionInAReadSession()
		 {
			  // given
			  _cluster = ClusterRule.withNumberOfReadReplicas( 1 ).startCluster();

			  CoreClusterMember leader = _cluster.awaitLeader();
			  ReadReplica readReplica = _cluster.getReadReplicaById( 0 );

			  readReplica.TxPollingClient().stop();

			  Driver driver = GraphDatabase.driver( leader.DirectURI(), AuthTokens.basic("Neo4Net", "Neo4Net") );

			  string bookmark = InExpirableSession(driver, d => d.session(AccessMode.WRITE), session =>
			  {
				using ( Transaction tx = session.BeginTransaction() )
				{
					 tx.run( "CREATE (p:Person {name: {name} })", Values.parameters( "name", "Jim" ) );
					 tx.run( "CREATE (p:Person {name: {name} })", Values.parameters( "name", "Alistair" ) );
					 tx.run( "CREATE (p:Person {name: {name} })", Values.parameters( "name", "Mark" ) );
					 tx.run( "CREATE (p:Person {name: {name} })", Values.parameters( "name", "Chris" ) );
					 tx.success();
				}

				return session.lastBookmark();
			  });

			  assertNotNull( bookmark );
			  readReplica.TxPollingClient().start();

			  driver = GraphDatabase.driver( readReplica.DirectURI(), AuthTokens.basic("Neo4Net", "Neo4Net") );

			  using ( Session session = driver.session( AccessMode.READ, bookmark ) )
			  {
					using ( Transaction tx = session.BeginTransaction() )
					{
						 Record record = tx.run( "MATCH (n:Person) RETURN COUNT(*) AS count" ).next();
						 tx.success();
						 assertEquals( 4, record.get( "count" ).asInt() );
					}
			  }
			  _cluster.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendRequestsToNewlyAddedReadReplicas() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendRequestsToNewlyAddedReadReplicas()
		 {
			  // given
			  _cluster = ClusterRule.withNumberOfReadReplicas( 1 ).withSharedCoreParams( stringMap( CausalClusteringSettings.cluster_routing_ttl.name(), "1s" ) ).startCluster();

			  CoreClusterMember leader = _cluster.awaitLeader();
			  Driver driver = GraphDatabase.driver( leader.RoutingURI(), AuthTokens.basic("Neo4Net", "Neo4Net") );

			  string bookmark = InExpirableSession(driver, d => d.session(AccessMode.WRITE), session =>
			  {
				using ( Transaction tx = session.BeginTransaction() )
				{
					 tx.run( "CREATE (p:Person {name: {name} })", Values.parameters( "name", "Jim" ) );
					 tx.success();
				}

				return session.lastBookmark();
			  });

			  // when
			  ISet<string> readReplicas = new HashSet<string>();

			  foreach ( ReadReplica readReplica in _cluster.readReplicas() )
			  {
					readReplicas.Add( readReplica.BoltAdvertisedAddress() );
			  }

			  for ( int i = 10; i <= 13; i++ )
			  {
					ReadReplica newReadReplica = _cluster.addReadReplicaWithId( i );
					readReplicas.Add( newReadReplica.BoltAdvertisedAddress() );
					newReadReplica.Start();
			  }

			  assertEventually("Failed to send requests to all servers", () =>
			  {
				for ( int i = 0; i < _cluster.readReplicas().Count; i++ ) // don't care about cores
				{
					 try
					 {
						 using ( Session session = driver.session( AccessMode.READ, bookmark ) )
						 {
							  ExecuteReadQuery( session );
   
							  session.readTransaction((TransactionWork<Void>) tx =>
							  {
								StatementResult result = tx.run( "MATCH (n:Person) RETURN COUNT(*) AS count" );

								assertEquals( 1, result.next().get("count").asInt() );

								readReplicas.remove( result.summary().server().address() );

								return null;
							  });
						 }
					 }
					 catch ( Exception )
					 {
						  return false;
					 }
				}

				return readReplicas.Count == 0; // have sent something to all replicas
			  }, @is( true ), 30, SECONDS);

			  _cluster.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleLeaderSwitch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleLeaderSwitch()
		 {
			  // given
			  _cluster = ClusterRule.startCluster();

			  CoreClusterMember leader = _cluster.awaitLeader();

			  using ( Driver driver = GraphDatabase.driver( leader.RoutingURI(), AuthTokens.basic("Neo4Net", "Neo4Net") ) )
			  {
					// when
					using ( Session session = driver.session() )
					{
						 try
						 {
								 using ( Transaction tx = session.BeginTransaction() )
								 {
								  SwitchLeader( leader );
      
								  tx.run( "CREATE (person:Person {name: {name}, title: {title}})", parameters( "name", "Webber", "title", "Mr" ) );
								  tx.success();
								 }
						 }
						 catch ( SessionExpiredException )
						 {
							  // expected
						 }
					}

					string bookmark = InExpirableSession(driver, Driver.session, s =>
					{
					 using ( Transaction tx = s.BeginTransaction() )
					 {
						  tx.run( "CREATE (person:Person {name: {name}, title: {title}})", parameters( "name", "Webber", "title", "Mr" ) );
						  tx.success();
					 }
					 return s.lastBookmark();
					});

					// then
					using ( Session session = driver.session( AccessMode.READ, bookmark ) )
					{
						 using ( Transaction tx = session.BeginTransaction() )
						 {
							  Record record = tx.run( "MATCH (n:Person) RETURN COUNT(*) AS count" ).next();
							  tx.success();
							  assertEquals( 1, record.get( "count" ).asInt() );
						 }
					}
			  }

			  _cluster.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transactionsShouldNotAppearOnTheReadReplicaWhilePollingIsPaused() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TransactionsShouldNotAppearOnTheReadReplicaWhilePollingIsPaused()
		 {
			  // given
			  IDictionary<string, string> @params = stringMap( GraphDatabaseSettings.keep_logical_logs.name(), "keep_none", GraphDatabaseSettings.logical_log_rotation_threshold.name(), "1M", GraphDatabaseSettings.check_point_interval_time.name(), "100ms", CausalClusteringSettings.cluster_allow_reads_on_followers.name(), "false" );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.causalclustering.discovery.Cluster<?> cluster = clusterRule.withSharedCoreParams(params).withNumberOfReadReplicas(1).startCluster();
			  Cluster<object> cluster = ClusterRule.withSharedCoreParams( @params ).withNumberOfReadReplicas( 1 ).startCluster();

			  Driver driver = GraphDatabase.driver( cluster.AwaitLeader().routingURI(), AuthTokens.basic("Neo4Net", "Neo4Net") );

			  using ( Session session = driver.session() )
			  {
					session.writeTransaction(tx =>
					{
														tx.run( "MERGE (n:Person {name: 'Jim'})" );
														return null;
					});
			  }

			  ReadReplica replica = cluster.FindAnyReadReplica();

			  CatchupPollingProcess pollingClient = replica.Database().DependencyResolver.resolveDependency(typeof(CatchupPollingProcess));

			  pollingClient.Stop();

			  string lastBookmark = null;
			  int iterations = 5;
			  const int nodesToCreate = 20000;
			  for ( int i = 0; i < iterations; i++ )
			  {
					using ( Session writeSession = driver.session() )
					{
						 writeSession.writeTransaction(tx =>
						 {

																	tx.run( "UNWIND range(1, {nodesToCreate}) AS i CREATE (n:Person {name: 'Jim'})", Values.parameters( "nodesToCreate", nodesToCreate ) );
																	return null;
						 });

						 lastBookmark = writeSession.lastBookmark();
					}
			  }

			  // when the poller is resumed, it does make it to the read replica
			  pollingClient.Start();

			  pollingClient.UpToDateFuture().get();

			  int happyCount = 0;
			  int numberOfRequests = 1_000;
			  for ( int i = 0; i < numberOfRequests; i++ ) // don't care about cores
			  {
					using ( Session session = driver.session( lastBookmark ) )
					{
						 happyCount += session.readTransaction(tx =>
						 {
																			  tx.run( "MATCH (n:Person) RETURN COUNT(*) AS count" );
																			  return 1;
						 });
					}
			  }

			  assertEquals( numberOfRequests, happyCount );

			  cluster.Shutdown();
		 }

		 private static void ExecuteReadQuery( Session session )
		 {
			  using ( Transaction tx = session.BeginTransaction() )
			  {
					Record record = tx.run( "MATCH (n:Person) RETURN COUNT(*) AS count" ).next();
					assertEquals( 1, record.get( "count" ).asInt() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static <T> T inExpirableSession(org.Neo4Net.driver.v1.Driver driver, System.Func<org.Neo4Net.driver.v1.Driver,org.Neo4Net.driver.v1.Session> acquirer, System.Func<org.Neo4Net.driver.v1.Session,T> op) throws java.util.concurrent.TimeoutException
		 private static T InExpirableSession<T>( Driver driver, System.Func<Driver, Session> acquirer, System.Func<Session, T> op )
		 {
			  long endTime = DateTimeHelper.CurrentUnixTimeMillis() + DEFAULT_TIMEOUT_MS;

			  do
			  {
					try
					{
							using ( Session session = acquirer( driver ) )
							{
							 return op( session );
							}
					}
					catch ( SessionExpiredException )
					{
						 // role might have changed; try again;
					}
			  } while ( DateTimeHelper.CurrentUnixTimeMillis() < endTime );

			  throw new TimeoutException( "Transaction did not succeed in time" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void switchLeader(org.Neo4Net.causalclustering.discovery.CoreClusterMember initialLeader) throws InterruptedException
		 private void SwitchLeader( CoreClusterMember initialLeader )
		 {
			  long deadline = DateTimeHelper.CurrentUnixTimeMillis() + (30 * 1000);

			  Role role = initialLeader.Database().Role;
			  while ( role != Role.FOLLOWER )
			  {
					if ( DateTimeHelper.CurrentUnixTimeMillis() > deadline )
					{
						 throw new Exception( "Failed to switch leader in time" );
					}

					try
					{
						 TriggerElection( initialLeader );
					}
					catch ( Exception e ) when ( e is IOException || e is TimeoutException )
					{
						 // keep trying
					}
					finally
					{
						 role = initialLeader.Database().Role;
						 Thread.Sleep( 100 );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void triggerElection(org.Neo4Net.causalclustering.discovery.CoreClusterMember initialLeader) throws java.io.IOException, java.util.concurrent.TimeoutException
		 private void TriggerElection( CoreClusterMember initialLeader )
		 {
			  foreach ( CoreClusterMember coreClusterMember in _cluster.coreMembers() )
			  {
					if ( !coreClusterMember.Equals( initialLeader ) )
					{
						 coreClusterMember.Raft().triggerElection(Clock.systemUTC());
						 _cluster.awaitLeader();
					}
			  }
		 }
	}

}