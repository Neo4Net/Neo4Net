/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Kernel.impl.core
{
	using Test = org.junit.Test;

	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using Race = Org.Neo4j.Test.Race;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.dense_node_threshold;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;

	/// <summary>
	/// This isn't a deterministic test, but instead tries to trigger a race condition
	/// for a couple of seconds. The original issues is mostly seen immediately, but after
	/// a fix is in this test will take the full amount of seconds unfortunately.
	/// </summary>
	public class TestConcurrentRelationshipChainLoadingIssue
	{
		 private readonly int _relCount = 2;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryToTriggerRelationshipLoadingStoppingMidWay() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryToTriggerRelationshipLoadingStoppingMidWay()
		 {
			  TryToTriggerRelationshipLoadingStoppingMidWay( 50 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tryToTriggerRelationshipLoadingStoppingMidWayForDenseNodeRepresentation() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TryToTriggerRelationshipLoadingStoppingMidWayForDenseNodeRepresentation()
		 {
			  TryToTriggerRelationshipLoadingStoppingMidWay( 1 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void tryToTriggerRelationshipLoadingStoppingMidWay(int denseNodeThreshold) throws Throwable
		 private void TryToTriggerRelationshipLoadingStoppingMidWay( int denseNodeThreshold )
		 {
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder().setConfig(dense_node_threshold, "" + denseNodeThreshold).newGraphDatabase();
			  Node node = CreateNodeWithRelationships( db );

			  CheckStateToHelpDiagnoseFlakyTest( db, node );

			  long end = currentTimeMillis() + SECONDS.toMillis(5);
			  int iterations = 0;
			  while ( currentTimeMillis() < end && iterations < 100 )
			  {
					TryOnce( db, node );
					iterations++;
			  }

			  Db.shutdown();
		 }

		 private void CheckStateToHelpDiagnoseFlakyTest( GraphDatabaseAPI db, Node node )
		 {
			  LoadNode( db, node );
			  LoadNode( db, node );
		 }

		 private void LoadNode( GraphDatabaseAPI db, Node node )
		 {
			  using ( Transaction ignored = Db.beginTx() )
			  {
					Iterables.count( node.Relationships );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void tryOnce(final org.neo4j.kernel.internal.GraphDatabaseAPI db, final org.neo4j.graphdb.Node node) throws Throwable
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private void TryOnce( GraphDatabaseAPI db, Node node )
		 {
			  Race race = ( new Race() ).withRandomStartDelays();
			  race.AddContestants(Runtime.Runtime.availableProcessors(), () =>
			  {
				using ( Transaction ignored = Db.beginTx() )
				{
					 assertEquals( _relCount, count( node.Relationships ) );
				}
			  });
			  race.Go();
		 }

		 private Node CreateNodeWithRelationships( GraphDatabaseAPI db )
		 {
			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode();
					for ( int i = 0; i < _relCount / 2; i++ )
					{
						 node.CreateRelationshipTo( node, MyRelTypes.TEST );
					}
					for ( int i = 0; i < _relCount / 2; i++ )
					{
						 node.CreateRelationshipTo( node, MyRelTypes.TEST2 );
					}
					tx.Success();
					return node;
			  }
		 }
	}

}