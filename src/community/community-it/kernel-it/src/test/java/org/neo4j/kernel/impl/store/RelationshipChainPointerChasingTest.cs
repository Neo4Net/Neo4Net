using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.impl.store
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.MyRelTypes.TEST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.MyRelTypes.TEST2;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.MyRelTypes.TEST_TRAVERSAL;

	/// <summary>
	/// Traversing a relationship chain has no consistency guarantees that there will be no change between
	/// starting the traversal and continuing through it. Therefore relationships might get deleted right
	/// when, or before, traversing there. The previous behaviour was to be aware of that and simply abort
	/// the chain traversal.
	/// 
	/// Given the fact that relationship ids will not be reused within the same database session the
	/// behaviour has been changed to continue through such unused relationships, reading its pointers,
	/// until arriving at either {@code -1} or a used relationship.
	/// </summary>
	public class RelationshipChainPointerChasingTest
	{
		 private const int THRESHOLD = 10;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule().withSetting(org.neo4j.graphdb.factory.GraphDatabaseSettings.dense_node_threshold, String.valueOf(THRESHOLD));
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule().withSetting(GraphDatabaseSettings.dense_node_threshold, THRESHOLD.ToString());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChaseTheLivingRelationships() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldChaseTheLivingRelationships()
		 {
			  // GIVEN a sound relationship chain
			  int numberOfRelationships = THRESHOLD / 2;
			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode();
					for ( int i = 0; i < numberOfRelationships; i++ )
					{
						 node.CreateRelationshipTo( Db.createNode(), TEST );
					}
					tx.Success();
			  }
			  Relationship[] relationships;
			  using ( Transaction tx = Db.beginTx() )
			  {
					relationships = asArray( typeof( Relationship ), node.Relationships );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					// WHEN getting the relationship iterator, i.e. starting to traverse this relationship chain,
					// the cursor eagerly goes to the first relationship before we call #hasNexxt/#next.
					IEnumerator<Relationship> iterator = node.Relationships.GetEnumerator();

					// Therefore we delete relationships [1] and [2] (the second and third), since currently
					// the relationship iterator has read [0] and have already decided to go to [1] after our next
					// call to #next
					DeleteRelationshipsInSeparateThread( relationships[1], relationships[2] );

					// THEN the relationship iterator should recognize the unused relationship, but still try to find
					// the next used relationship in this chain by following the pointers in the unused records.
					AssertNext( relationships[0], iterator );
					AssertNext( relationships[3], iterator );
					AssertNext( relationships[4], iterator );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( iterator.hasNext() );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldChaseTheLivingRelationshipGroups() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldChaseTheLivingRelationshipGroups()
		 {
			  // GIVEN
			  Node node;
			  Relationship relationshipInTheMiddle;
			  Relationship relationshipInTheEnd;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode();
					for ( int i = 0; i < THRESHOLD; i++ )
					{
						 node.CreateRelationshipTo( Db.createNode(), TEST );
					}
					relationshipInTheMiddle = node.CreateRelationshipTo( Db.createNode(), TEST2 );
					relationshipInTheEnd = node.CreateRelationshipTo( Db.createNode(), TEST_TRAVERSAL );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					// WHEN getting the relationship iterator, the first group record will be read and held,
					// already pointing to the next group
					IEnumerator<Relationship> relationships = node.Relationships.GetEnumerator();
					for ( int i = 0; i < THRESHOLD / 2; i++ )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 assertTrue( relationships.next().isType(TEST) );
					}

					// Here we're awfully certain that we're on this first group, so we go ahead and delete the
					// next one in a simulated concurrent transaction in another thread
					DeleteRelationshipsInSeparateThread( relationshipInTheMiddle );

					// THEN we should be able to, first of all, iterate through the rest of the relationships of the first type
					for ( int i = 0; i < THRESHOLD / 2; i++ )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 assertTrue( relationships.next().isType(TEST) );
					}
					// THEN we should be able to see the last relationship, after the deleted one
					// where the group for the deleted relationship also should've been deleted since it was the
					// only on of that type.
					AssertNext( relationshipInTheEnd, relationships );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( relationships.hasNext() );

					tx.Success();
			  }
		 }

		 private void AssertNext( Relationship expected, IEnumerator<Relationship> iterator )
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( "Expected there to be more relationships", iterator.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( "Unexpected next relationship", expected, iterator.next() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void deleteRelationshipsInSeparateThread(final org.neo4j.graphdb.Relationship... relationships) throws InterruptedException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private void DeleteRelationshipsInSeparateThread( params Relationship[] relationships )
		 {
			  ExecuteTransactionInSeparateThread(() =>
			  {
				foreach ( Relationship relationship in relationships )
				{
					 relationship.delete();
				}
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void executeTransactionInSeparateThread(final Runnable actionInsideTransaction) throws InterruptedException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private void ExecuteTransactionInSeparateThread( ThreadStart actionInsideTransaction )
		 {
			  Thread thread = new Thread(() =>
			  {
			  using ( Transaction tx = Db.beginTx() )
			  {
				  actionInsideTransaction.run();
				  tx.success();
			  }
			  });
			  thread.Start();
			  thread.Join();
		 }
	}

}