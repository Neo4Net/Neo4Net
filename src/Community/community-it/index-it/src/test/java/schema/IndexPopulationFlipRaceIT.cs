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
namespace Schema
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Neo4Net.Helpers.Collection;
	using IndexReference = Neo4Net.@internal.Kernel.Api.IndexReference;
	using Kernel = Neo4Net.@internal.Kernel.Api.Kernel;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.Transaction_Type.@implicit;

	public class IndexPopulationFlipRaceIT
	{
		 private const int NODES_PER_INDEX = 10;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.EmbeddedDatabaseRule();
		 public readonly DatabaseRule Db = new EmbeddedDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAtomicallyFlipMultipleIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAtomicallyFlipMultipleIndexes()
		 {
			  // A couple of times since this is probabilistic, but also because there seems to be a difference
			  // in timings between the first time and all others... which is perhaps super obvious to some, but not to me.
			  for ( int i = 0; i < 10; i++ )
			  {
					// GIVEN
					CreateIndexesButDontWaitForThemToFullyPopulate( i );

					// WHEN
					Pair<long[], long[]> data = CreateDataThatGoesIntoToThoseIndexes( i );

					// THEN
					AwaitIndexes();
					VerifyThatThereAreExactlyOneIndexEntryPerNodeInTheIndexes( i, data );
			  }
		 }

		 private void AwaitIndexes()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(30, SECONDS);
					tx.Success();
			  }
		 }

		 private void CreateIndexesButDontWaitForThemToFullyPopulate( int i )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(LabelA(i)).on(KeyA(i)).create();

					if ( Random.nextBoolean() )
					{
						 Db.schema().indexFor(LabelB(i)).on(KeyB(i)).create();
					}
					else
					{
						 Db.schema().constraintFor(LabelB(i)).assertPropertyIsUnique(KeyB(i)).create();
					}
					tx.Success();
			  }
		 }

		 private static string KeyB( int i )
		 {
			  return "key_b" + i;
		 }

		 private static Label LabelB( int i )
		 {
			  return label( "Label_b" + i );
		 }

		 private static string KeyA( int i )
		 {
			  return "key_a" + i;
		 }

		 private static Label LabelA( int i )
		 {
			  return label( "Label_a" + i );
		 }

		 private Pair<long[], long[]> CreateDataThatGoesIntoToThoseIndexes( int i )
		 {
			  long[] dataA = new long[NODES_PER_INDEX];
			  long[] dataB = new long[NODES_PER_INDEX];
			  for ( int t = 0; t < NODES_PER_INDEX; t++ )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Node nodeA = Db.createNode( LabelA( i ) );
						 nodeA.SetProperty( KeyA( i ), dataA[t] = nodeA.Id );
						 Node nodeB = Db.createNode( LabelB( i ) );
						 nodeB.SetProperty( KeyB( i ), dataB[t] = nodeB.Id );
						 tx.Success();
					}
			  }
			  return Pair.of( dataA, dataB );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyThatThereAreExactlyOneIndexEntryPerNodeInTheIndexes(int i, org.neo4j.helpers.collection.Pair<long[],long[]> data) throws Exception
		 private void VerifyThatThereAreExactlyOneIndexEntryPerNodeInTheIndexes( int i, Pair<long[], long[]> data )
		 {
			  Kernel kernel = Db.DependencyResolver.resolveDependency( typeof( Kernel ) );
			  using ( Neo4Net.@internal.Kernel.Api.Transaction tx = kernel.BeginTransaction( @implicit, AnonymousContext.read() ) )
			  {
					int labelAId = tx.TokenRead().nodeLabel(LabelA(i).name());
					int keyAId = tx.TokenRead().propertyKey(KeyA(i));
					int labelBId = tx.TokenRead().nodeLabel(LabelB(i).name());
					int keyBId = tx.TokenRead().propertyKey(KeyB(i));
					IndexReference indexA = TestIndexDescriptorFactory.forLabel( labelAId, keyAId );
					IndexReference indexB = TestIndexDescriptorFactory.forLabel( labelBId, keyBId );

					for ( int j = 0; j < NODES_PER_INDEX; j++ )
					{
						 long nodeAId = data.First()[j];
						 assertEquals( 1, tx.SchemaRead().nodesCountIndexed(indexA, nodeAId, keyAId, Values.of(nodeAId)) );
						 long nodeBId = data.Other()[j];
						 assertEquals( 1, tx.SchemaRead().nodesCountIndexed(indexB, nodeBId, keyBId, Values.of(nodeBId)) );
					}
			  }
		 }
	}

}