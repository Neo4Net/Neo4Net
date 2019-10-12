using System;
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
namespace Neo4Net.causalclustering.scenarios
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CoreGraphDatabase = Neo4Net.causalclustering.core.CoreGraphDatabase;
	using Role = Neo4Net.causalclustering.core.consensus.roles.Role;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using WriteOperationsNotAllowedException = Neo4Net.Graphdb.security.WriteOperationsNotAllowedException;
	using PageCacheCounters = Neo4Net.Io.pagecache.monitoring.PageCacheCounters;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.Cluster.dataMatchesEventually;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.helpers.DataCreator.countNodes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Predicates.await;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class CoreReplicationIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0).withTimeout(1000, SECONDS);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0).withTimeout(1000, SECONDS);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _cluster = ClusterRule.startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReplicateTransactionsToCoreMembers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReplicateTransactionsToCoreMembers()
		 {
			  // when
			  CoreClusterMember leader = _cluster.coreTx((db, tx) =>
			  {
				Node node = Db.createNode( label( "boo" ) );
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  // then
			  assertEquals( 1, countNodes( leader ) );
			  dataMatchesEventually( leader, _cluster.coreMembers() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowWritesFromAFollower() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowWritesFromAFollower()
		 {
			  // given
			  _cluster.awaitLeader();

			  CoreGraphDatabase follower = _cluster.getMemberWithRole( Role.FOLLOWER ).database();

			  // when
			  try
			  {
					  using ( Transaction tx = follower.BeginTx() )
					  {
						follower.CreateNode();
						tx.Success();
						fail( "Should have thrown exception" );
					  }
			  }
			  catch ( WriteOperationsNotAllowedException ex )
			  {
					// expected
					assertThat( ex.Message, containsString( "No write operations are allowed" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pageFaultsFromReplicationMustCountInMetrics() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PageFaultsFromReplicationMustCountInMetrics()
		 {
			  // Given initial pin counts on all members
			  System.Func<CoreClusterMember, PageCacheCounters> getPageCacheCounters = ccm => ccm.database().DependencyResolver.resolveDependency(typeof(PageCacheCounters));
			  IList<PageCacheCounters> countersList = _cluster.coreMembers().Select(getPageCacheCounters).ToList();
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  long[] initialPins = countersList.Select( PageCacheCounters::pins ).ToArray();

			  // when the leader commits a write transaction,
			  _cluster.coreTx((db, tx) =>
			  {
				Node node = Db.createNode( label( "boo" ) );
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  // then the replication should cause pins on a majority of core members to increase.
			  // However, the commit returns as soon as the transaction has been replicated through the Raft log, which
			  // happens before the transaction is applied on the members. Therefor we are racing with the followers
			  // transaction application, so we have to spin.
			  int minimumUpdatedMembersCount = countersList.Count / 2 + 1;
			  assertEventually("Expected followers to eventually increase pin counts", () =>
			  {
				long[] pinsAfterCommit = countersList.Select( PageCacheCounters.pins ).ToArray();
				int membersWithIncreasedPinCount = 0;
				for ( int i = 0; i < initialPins.Length; i++ )
				{
					 long before = initialPins[i];
					 long after = pinsAfterCommit[i];
					 if ( before < after )
					 {
						  membersWithIncreasedPinCount++;
					 }
				}
				return membersWithIncreasedPinCount;
			  }, @is( greaterThanOrEqualTo( minimumUpdatedMembersCount ) ), 10, SECONDS);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowSchemaChangesFromAFollower() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowSchemaChangesFromAFollower()
		 {
			  // given
			  _cluster.awaitLeader();

			  CoreGraphDatabase follower = _cluster.getMemberWithRole( Role.FOLLOWER ).database();

			  // when
			  try
			  {
					  using ( Transaction tx = follower.BeginTx() )
					  {
						follower.Schema().constraintFor(Label.label("Foo")).assertPropertyIsUnique("name").create();
						tx.Success();
						fail( "Should have thrown exception" );
					  }
			  }
			  catch ( WriteOperationsNotAllowedException ex )
			  {
					// expected
					assertThat( ex.Message, containsString( "No write operations are allowed" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowTokenCreationFromAFollowerWithNoInitialTokens() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowTokenCreationFromAFollowerWithNoInitialTokens()
		 {
			  // given
			  CoreClusterMember leader = _cluster.coreTx((db, tx) =>
			  {
				Db.createNode();
				tx.success();
			  });

			  AwaitForDataToBeApplied( leader );
			  dataMatchesEventually( leader, _cluster.coreMembers() );

			  CoreGraphDatabase follower = _cluster.getMemberWithRole( Role.FOLLOWER ).database();

			  // when
			  try
			  {
					  using ( Transaction tx = follower.BeginTx() )
					  {
						follower.AllNodes.GetEnumerator().next().setProperty("name", "Mark");
						tx.Success();
						fail( "Should have thrown exception" );
					  }
			  }
			  catch ( WriteOperationsNotAllowedException ex )
			  {
					assertThat( ex.Message, containsString( "No write operations are allowed" ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void awaitForDataToBeApplied(org.neo4j.causalclustering.discovery.CoreClusterMember leader) throws java.util.concurrent.TimeoutException
		 private void AwaitForDataToBeApplied( CoreClusterMember leader )
		 {
			  await( () => countNodes(leader) > 0, 10, SECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReplicateTransactionToCoreMemberAddedAfterInitialStartUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReplicateTransactionToCoreMemberAddedAfterInitialStartUp()
		 {
			  // given
			  _cluster.getCoreMemberById( 0 ).shutdown();

			  _cluster.addCoreMemberWithId( 3 ).start();
			  _cluster.getCoreMemberById( 0 ).start();

			  _cluster.coreTx((db, tx) =>
			  {
				Node node = Db.createNode();
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  // when
			  _cluster.addCoreMemberWithId( 4 ).start();
			  CoreClusterMember last = _cluster.coreTx((db, tx) =>
			  {
				Node node = Db.createNode();
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  // then
			  assertEquals( 2, countNodes( last ) );
			  dataMatchesEventually( last, _cluster.coreMembers() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReplicateTransactionAfterLeaderWasRemovedFromCluster() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReplicateTransactionAfterLeaderWasRemovedFromCluster()
		 {
			  // given
			  _cluster.coreTx((db, tx) =>
			  {
				Node node = Db.createNode();
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  // when
			  _cluster.removeCoreMember( _cluster.awaitLeader() );
			  _cluster.awaitLeader( 1, TimeUnit.MINUTES ); // <- let's give a bit more time for the leader to show up
			  CoreClusterMember last = _cluster.coreTx((db, tx) =>
			  {
				Node node = Db.createNode();
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  // then
			  assertEquals( 2, countNodes( last ) );
			  dataMatchesEventually( last, _cluster.coreMembers() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReplicateToCoreMembersAddedAfterInitialTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReplicateToCoreMembersAddedAfterInitialTransactions()
		 {
			  // when
			  CoreClusterMember last = null;
			  for ( int i = 0; i < 15; i++ )
			  {
					last = _cluster.coreTx((db, tx) =>
					{
					 Node node = Db.createNode();
					 node.setProperty( "foobar", "baz_bat" );
					 tx.success();
					});
			  }

			  _cluster.addCoreMemberWithId( 3 ).start();
			  _cluster.addCoreMemberWithId( 4 ).start();

			  // then
			  assertEquals( 15, countNodes( last ) );
			  dataMatchesEventually( last, _cluster.coreMembers() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReplicateTransactionsToReplacementCoreMembers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReplicateTransactionsToReplacementCoreMembers()
		 {
			  // when
			  _cluster.coreTx((db, tx) =>
			  {
				Node node = Db.createNode( label( "boo" ) );
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  _cluster.removeCoreMemberWithServerId( 0 );
			  CoreClusterMember replacement = _cluster.addCoreMemberWithId( 0 );
			  replacement.Start();

			  CoreClusterMember leader = _cluster.coreTx((db, tx) =>
			  {
				Db.schema().indexFor(label("boo")).on("foobar").create();
				tx.success();
			  });

			  // then
			  assertEquals( 1, countNodes( leader ) );
			  dataMatchesEventually( leader, _cluster.coreMembers() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToShutdownWhenTheLeaderIsTryingToReplicateTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToShutdownWhenTheLeaderIsTryingToReplicateTransaction()
		 {
			  // given
			  _cluster.coreTx((db, tx) =>
			  {
				Node node = Db.createNode( label( "boo" ) );
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );

			  // when
			  Thread thread = new Thread(() =>
			  {
				try
				{
					 _cluster.coreTx((db, tx) =>
					 {
						  Db.createNode();
						  tx.success();

						  _cluster.removeCoreMember( _cluster.getMemberWithAnyRole( Role.FOLLOWER, Role.CANDIDATE ) );
						  _cluster.removeCoreMember( _cluster.getMemberWithAnyRole( Role.FOLLOWER, Role.CANDIDATE ) );
						  latch.Signal();
					 });
					 fail( "Should have thrown" );
				}
				catch ( Exception )
				{
					 // expected
				}
			  });

			  thread.Start();

			  latch.await();

			  // then the cluster can shutdown...
			  _cluster.shutdown();
			  // ... and the thread running the tx does not get stuck
			  thread.Join( TimeUnit.MINUTES.toMillis( 1 ) );
		 }
	}

}