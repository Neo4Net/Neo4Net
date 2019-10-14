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
namespace Neo4Net.Kernel.index
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Neo4Net.Graphdb.index;
	using IndexManager = Neo4Net.Graphdb.index.IndexManager;
	using BeginTx = Neo4Net.ha.BeginTx;
	using FinishTx = Neo4Net.ha.FinishTx;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using UpdatePuller = Neo4Net.Kernel.ha.UpdatePuller;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using RepairKit = Neo4Net.Kernel.impl.ha.ClusterManager.RepairKit;
	using Neo4Net.Test;
	using Neo4Net.Test.OtherThreadExecutor;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class IndexOperationsIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule();
		 public ClusterRule ClusterRule = new ClusterRule();

		 protected internal ClusterManager.ManagedCluster Cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  Cluster = ClusterRule.startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void index_modifications_are_propagated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexModificationsArePropagated()
		 {
			  // GIVEN
			  // -- a slave
			  string key = "name";
			  string value = "Mattias";
			  HighlyAvailableGraphDatabase author = Cluster.AnySlave;

			  // WHEN
			  // -- it creates a node associated with indexing in a transaction
			  long node = CreateNode( author, key, value, true );

			  // THEN
			  // -- all instances should see it after pulling updates
			  foreach ( HighlyAvailableGraphDatabase db in Cluster.AllMembers )
			  {
					Db.DependencyResolver.resolveDependency( typeof( UpdatePuller ) ).pullUpdates();
					AssertNodeAndIndexingExists( db, node, key, value );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void index_objects_can_be_reused_after_role_switch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexObjectsCanBeReusedAfterRoleSwitch()
		 {
			  // GIVEN
			  // -- an existing index
			  string key = "key";
			  string value = "value";
			  HighlyAvailableGraphDatabase master = Cluster.Master;
			  long nodeId = CreateNode( master, key, value, true );
			  Cluster.sync();
			  // -- get Index and IndexManager references to all dbs
			  IDictionary<HighlyAvailableGraphDatabase, IndexManager> indexManagers = new Dictionary<HighlyAvailableGraphDatabase, IndexManager>();
			  IDictionary<HighlyAvailableGraphDatabase, Index<Node>> indexes = new Dictionary<HighlyAvailableGraphDatabase, Index<Node>>();
			  foreach ( HighlyAvailableGraphDatabase db in Cluster.AllMembers )
			  {
					using ( Transaction transaction = Db.beginTx() )
					{
						 indexManagers[db] = Db.index();
						 indexes[db] = Db.index().forNodes(key);
						 transaction.Success();
					}
			  }

			  // WHEN
			  // -- there's a master switch
			  ClusterManager.RepairKit repair = Cluster.shutdown( master );
			  indexManagers.Remove( master );
			  indexes.Remove( master );

			  Cluster.await( ClusterManager.masterAvailable( master ) );
			  Cluster.await( ClusterManager.masterSeesSlavesAsAvailable( 1 ) );

			  // THEN
			  // -- the index instances should still be viable to use
			  foreach ( KeyValuePair<HighlyAvailableGraphDatabase, IndexManager> entry in indexManagers.SetOfKeyValuePairs() )
			  {
					HighlyAvailableGraphDatabase db = entry.Key;
					using ( Transaction transaction = Db.beginTx() )
					{
						 IndexManager indexManager = entry.Value;
						 assertTrue( indexManager.ExistsForNodes( key ) );
						 assertEquals( nodeId, indexManager.ForNodes( key ).get( key, value ).Single.Id );
					}
			  }

			  foreach ( KeyValuePair<HighlyAvailableGraphDatabase, Index<Node>> entry in indexes.SetOfKeyValuePairs() )
			  {
					HighlyAvailableGraphDatabase db = entry.Key;
					using ( Transaction transaction = Db.beginTx() )
					{
						 Index<Node> index = entry.Value;
						 assertEquals( nodeId, index.get( key, value ).Single.Id );
					}
			  }
			  repair.Repair();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void put_if_absent_works_across_instances() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PutIfAbsentWorksAcrossInstances()
		 {
			  // GIVEN
			  // -- two instances, each begin a transaction
			  string key = "key2";
			  string value = "value2";
			  HighlyAvailableGraphDatabase db1 = Cluster.Master;
			  HighlyAvailableGraphDatabase db2 = Cluster.AnySlave;
			  long node = CreateNode( db1, key, value, false );
			  Cluster.sync();
			  OtherThreadExecutor<HighlyAvailableGraphDatabase> w1 = new OtherThreadExecutor<HighlyAvailableGraphDatabase>( "w1", db1 );
			  OtherThreadExecutor<HighlyAvailableGraphDatabase> w2 = new OtherThreadExecutor<HighlyAvailableGraphDatabase>( "w2", db2 );
			  Transaction tx1 = w1.Execute( new BeginTx() );
			  Transaction tx2 = w2.Execute( new BeginTx() );

			  // WHEN
			  // -- second instance does putIfAbsent --> null
			  assertNull( w2.Execute( new PutIfAbsent( node, key, value ) ) );
			  // -- get a future to first instance putIfAbsent. Wait for it to go and await the lock
			  Future<Node> w1Future = w1.ExecuteDontWait( new PutIfAbsent( node, key, value ) );
			  w1.WaitUntilWaiting();
			  // -- second instance completes tx
			  w2.Execute( new FinishTx( tx2, true ) );
			  tx2.Success();
			  tx2.Close();

			  // THEN
			  // -- first instance can complete the future with a non-null result
			  assertNotNull( w1Future.get() );
			  w1.Execute( new FinishTx( tx1, true ) );
			  // -- assert the index has got one entry and both instances have the same data
			  AssertNodeAndIndexingExists( db1, node, key, value );
			  AssertNodeAndIndexingExists( db2, node, key, value );
			  Cluster.sync();
			  AssertNodeAndIndexingExists( Cluster.getAnySlave( db1, db2 ), node, key, value );

			  w2.Dispose();
			  w1.Dispose();
		 }

		 private long CreateNode( HighlyAvailableGraphDatabase author, string key, object value, bool index )
		 {
			  using ( Transaction tx = author.BeginTx() )
			  {
					Node node = author.CreateNode();
					node.SetProperty( key, value );
					if ( index )
					{
						 author.Index().forNodes(key).add(node, key, value);
					}
					tx.Success();
					return node.Id;
			  }
		 }

		 private void AssertNodeAndIndexingExists( HighlyAvailableGraphDatabase db, long nodeId, string key, object value )
		 {
			  using ( Transaction transaction = Db.beginTx() )
			  {
					Node node = Db.getNodeById( nodeId );
					assertEquals( value, node.GetProperty( key ) );
					assertTrue( Db.index().existsForNodes(key) );
					assertEquals( node, Db.index().forNodes(key).get(key, value).Single );
			  }
		 }

		 private class PutIfAbsent : OtherThreadExecutor.WorkerCommand<HighlyAvailableGraphDatabase, Node>
		 {
			  internal readonly long Node;
			  internal readonly string Key;
			  internal readonly string Value;

			  internal PutIfAbsent( long node, string key, string value )
			  {
					this.Node = node;
					this.Key = key;
					this.Value = value;
			  }

			  public override Node DoWork( HighlyAvailableGraphDatabase state )
			  {
					return state.Index().forNodes(Key).putIfAbsent(state.GetNodeById(Node), Key, Value);
			  }
		 }
	}

}