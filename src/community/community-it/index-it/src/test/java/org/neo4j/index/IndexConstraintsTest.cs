using System;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Index
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.GraphDb.Index;
	using ConstraintDefinition = Neo4Net.GraphDb.Schema.ConstraintDefinition;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.firstOrNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.single;

	public class IndexConstraintsTest
	{
		 private static readonly Label _label = Label.label( "Label" );
		 private const string PROPERTY_KEY = "x";

		 private IGraphDatabaseService _graphDb;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _graphDb = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void shutdown()
		 public virtual void Shutdown()
		 {
			  _graphDb.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMultipleCreate() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestMultipleCreate()
		 {
			  const int numThreads = 25;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String uuid = java.util.UUID.randomUUID().toString();
			  string uuid = System.Guid.randomUUID().ToString();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Node commonNode;
			  Node commonNode;
			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					commonNode = _graphDb.createNode();
					tx.Success();
			  }

			  ExecutorCompletionService<Node> ecs = new ExecutorCompletionService<Node>( Executors.newFixedThreadPool( numThreads ) );
			  for ( int i = 0; i < numThreads; i++ )
			  {
					ecs.submit(() =>
					{
					 using ( Transaction tx = _graphDb.beginTx() )
					 {
						  Node node = _graphDb.createNode();
						  // Acquire lock
						  tx.AcquireWriteLock( commonNode );
						  Index<Node> index = _graphDb.index().forNodes("uuids");
						  Node existing = index.get( "uuid", uuid ).Single;
						  if ( existing != null )
						  {
								throw new Exception( "Node already exists" );
						  }
						  node.setProperty( "uuid", uuid );
						  index.add( node, "uuid", uuid );
						  tx.Success();
						  return node;
					 }
					});
			  }
			  int numSucceeded = 0;
			  for ( int i = 0; i < numThreads; i++ )
			  {
					try
					{
						 ecs.take().get();
						 ++numSucceeded;
					}
					catch ( ExecutionException )
					{
					}
			  }
			  assertEquals( 1, numSucceeded );
		 }

		 // The following tests verify that multiple interacting schema commands can be applied in the same transaction.

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void convertIndexToConstraint()
		 public virtual void ConvertIndexToConstraint()
		 {
			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					_graphDb.schema().indexFor(_label).on(PROPERTY_KEY).create();
					tx.Success();
			  }

			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					IndexDefinition index = firstOrNull( _graphDb.schema().getIndexes(_label) );
					index.Drop();

					_graphDb.schema().constraintFor(_label).assertPropertyIsUnique(PROPERTY_KEY).create();
					tx.Success();
			  }
			  // assert no exception is thrown
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void convertIndexToConstraintWithExistingData()
		 public virtual void ConvertIndexToConstraintWithExistingData()
		 {
			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					for ( int i = 0; i < 2000; i++ )
					{
						 Node node = _graphDb.createNode( _label );
						 node.SetProperty( PROPERTY_KEY, i );
					}
					tx.Success();
			  }

			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					_graphDb.schema().indexFor(_label).on(PROPERTY_KEY).create();
					tx.Success();
			  }

			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					IndexDefinition index = firstOrNull( _graphDb.schema().getIndexes(_label) );
					index.Drop();

					_graphDb.schema().constraintFor(_label).assertPropertyIsUnique(PROPERTY_KEY).create();
					tx.Success();
			  }
			  // assert no exception is thrown
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void convertConstraintToIndex()
		 public virtual void ConvertConstraintToIndex()
		 {
			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					_graphDb.schema().constraintFor(_label).assertPropertyIsUnique(PROPERTY_KEY).create();
					tx.Success();
			  }

			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					ConstraintDefinition constraint = firstOrNull( _graphDb.schema().getConstraints(_label) );
					constraint.Drop();

					_graphDb.schema().indexFor(_label).on(PROPERTY_KEY).create();
					tx.Success();
			  }
			  // assert no exception is thrown
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void creatingAndDroppingAndCreatingIndexInSameTransaction()
		 public virtual void CreatingAndDroppingAndCreatingIndexInSameTransaction()
		 {
			  // go increasingly meaner
			  for ( int times = 1; times <= 4; times++ )
			  {
					try
					{
						 // when: CREATE, DROP, CREATE => effect: CREATE
						 using ( Transaction tx = _graphDb.beginTx() )
						 {
							  Recreate( _graphDb.schema().indexFor(_label).on(PROPERTY_KEY).create(), times );
							  tx.Success();
						 }
						 // then
						 assertNotNull( "Index should exist", GetIndex( _label, PROPERTY_KEY ) );

						 // when: DROP, CREATE => effect: <none>
						 using ( Transaction tx = _graphDb.beginTx() )
						 {
							  Recreate( GetIndex( _label, PROPERTY_KEY ), times );
							  tx.Success();
						 }
						 // then
						 assertNotNull( "Index should exist", GetIndex( _label, PROPERTY_KEY ) );

						 // when: DROP, CREATE, DROP => effect: DROP
						 using ( Transaction tx = _graphDb.beginTx() )
						 {
							  Recreate( GetIndex( _label, PROPERTY_KEY ), times ).drop();
							  tx.Success();
						 }
						 // then
						 assertNull( "Index should be removed", GetIndex( _label, PROPERTY_KEY ) );
					}
					catch ( Exception e )
					{
						 throw new AssertionError( "times=" + times, e );
					}
			  }
		 }

		 private IndexDefinition Recreate( IndexDefinition index, int times )
		 {
			  for ( int i = 0; i < times; i++ )
			  {
					index.Drop();
					index = _graphDb.schema().indexFor(single(index.Labels)).on(single(index.PropertyKeys)).create();
			  }
			  return index;
		 }

		 private IndexDefinition GetIndex( Label label, string propertyKey )
		 {
			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					IndexDefinition found = null;
					foreach ( IndexDefinition index in _graphDb.schema().getIndexes(label) )
					{
						 if ( propertyKey.Equals( single( index.PropertyKeys ) ) )
						 {
							  assertNull( "Found multiple indexes.", found );
							  found = index;
						 }
					}
					tx.Success();
					return found;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveIndexForConstraintEvenIfDroppedInCreatingTransaction()
		 public virtual void ShouldRemoveIndexForConstraintEvenIfDroppedInCreatingTransaction()
		 {
			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					// given
					_graphDb.schema().constraintFor(_label).assertPropertyIsUnique(PROPERTY_KEY).create().drop();
					// when - rolling back
					tx.Failure();
			  }
			  // then
			  assertNull( "Should not have constraint index", GetIndex( _label, PROPERTY_KEY ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void creatingAndDroppingAndCreatingConstraintInSameTransaction()
		 public virtual void CreatingAndDroppingAndCreatingConstraintInSameTransaction()
		 {
			  // go increasingly meaner
			  for ( int times = 1; times <= 4; times++ )
			  {
					try
					{
						 // when: CREATE, DROP, CREATE => effect: CREATE
						 using ( Transaction tx = _graphDb.beginTx() )
						 {
							  Recreate( _graphDb.schema().constraintFor(_label).assertPropertyIsUnique(PROPERTY_KEY).create(), times );
							  tx.Success();
						 }
						 // then
						 assertNotNull( "Constraint should exist", GetConstraint( _label, PROPERTY_KEY ) );
						 assertNotNull( "Should have constraint index", GetIndex( _label, PROPERTY_KEY ) );

						 // when: DROP, CREATE => effect: <none>
						 using ( Transaction tx = _graphDb.beginTx() )
						 {
							  Recreate( GetConstraint( _label, PROPERTY_KEY ), times );
							  tx.Success();
						 }
						 // then
						 assertNotNull( "Constraint should exist", GetConstraint( _label, PROPERTY_KEY ) );
						 assertNotNull( "Should have constraint index", GetIndex( _label, PROPERTY_KEY ) );

						 // when: DROP, CREATE, DROP => effect: DROP
						 using ( Transaction tx = _graphDb.beginTx() )
						 {
							  Recreate( GetConstraint( _label, PROPERTY_KEY ), times ).drop();
							  tx.Success();
						 }
						 // then
						 assertNull( "Constraint should be removed", GetConstraint( _label, PROPERTY_KEY ) );
						 assertNull( "Should not have constraint index", GetIndex( _label, PROPERTY_KEY ) );
					}
					catch ( Exception e )
					{
						 throw new AssertionError( "times=" + times, e );
					}
			  }
		 }

		 private ConstraintDefinition Recreate( ConstraintDefinition constraint, int times )
		 {
			  for ( int i = 0; i < times; i++ )
			  {
					constraint.Drop();
					constraint = _graphDb.schema().constraintFor(constraint.Label).assertPropertyIsUnique(single(constraint.PropertyKeys)).create();
			  }
			  return constraint;
		 }

		 private ConstraintDefinition GetConstraint( Label label, string propertyKey )
		 {
			  using ( Transaction tx = _graphDb.beginTx() )
			  {
					ConstraintDefinition found = null;
					foreach ( ConstraintDefinition constraint in _graphDb.schema().getConstraints(label) )
					{
						 if ( propertyKey.Equals( single( constraint.PropertyKeys ) ) )
						 {
							  assertNull( "Found multiple constraints.", found );
							  found = constraint;
						 }
					}
					tx.Success();
					return found;
			  }
		 }
	}

}