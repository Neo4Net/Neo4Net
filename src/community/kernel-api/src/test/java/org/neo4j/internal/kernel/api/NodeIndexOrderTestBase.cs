using System.Collections.Generic;

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
namespace Neo4Net.Internal.Kernel.Api
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Label = Neo4Net.GraphDb.Label;
	using Neo4Net.Helpers.Collections;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.stringValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public abstract class NodeIndexOrderTestBase<G extends KernelAPIWriteTestSupport> extends KernelAPIWriteTestBase<G>
	public abstract class NodeIndexOrderTestBase<G> : KernelAPIWriteTestBase<G> where G : KernelAPIWriteTestSupport
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static Iterable<Object[]> data()
		 public static IEnumerable<object[]> Data()
		 {
			  return Arrays.asList(new object[][]
			  {
				  new object[] { IndexOrder.Ascending }
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public IndexOrder indexOrder;
		 public IndexOrder IndexOrder;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRangeScanInOrder() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRangeScanInOrder()
		 {
			  IList<Pair<long, Value>> expected = new List<Pair<long, Value>>();

			  using ( Transaction tx = beginTransaction() )
			  {
					expected.Add( NodeWithProp( tx, "hello" ) );
					NodeWithProp( tx, "bellow" );
					expected.Add( NodeWithProp( tx, "schmello" ) );
					expected.Add( NodeWithProp( tx, "low" ) );
					expected.Add( NodeWithProp( tx, "trello" ) );
					NodeWithProp( tx, "yellow" );
					expected.Add( NodeWithProp( tx, "low" ) );
					NodeWithProp( tx, "below" );
					tx.Success();
			  }

			  CreateIndex();

			  // when
			  using ( Transaction tx = beginTransaction() )
			  {
					int label = tx.TokenRead().nodeLabel("Node");
					int prop = tx.TokenRead().propertyKey("prop");
					IndexReference index = tx.SchemaRead().index(label, prop);

					using ( NodeValueIndexCursor cursor = tx.Cursors().allocateNodeValueIndexCursor() )
					{
						 NodeWithProp( tx, "allow" );
						 expected.Add( NodeWithProp( tx, "now" ) );
						 expected.Add( NodeWithProp( tx, "jello" ) );
						 NodeWithProp( tx, "willow" );

						 IndexQuery query = IndexQuery.Range( prop, "hello", true, "trello", true );
						 tx.DataRead().nodeIndexSeek(index, cursor, IndexOrder, true, query);

						 AssertResultsInOrder( expected, cursor );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrefixScanInOrder() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPrefixScanInOrder()
		 {
			  IList<Pair<long, Value>> expected = new List<Pair<long, Value>>();

			  using ( Transaction tx = beginTransaction() )
			  {
					expected.Add( NodeWithProp( tx, "bee hive" ) );
					NodeWithProp( tx, "a" );
					expected.Add( NodeWithProp( tx, "become" ) );
					expected.Add( NodeWithProp( tx, "be" ) );
					expected.Add( NodeWithProp( tx, "bachelor" ) );
					NodeWithProp( tx, "street smart" );
					expected.Add( NodeWithProp( tx, "builder" ) );
					NodeWithProp( tx, "ceasar" );
					tx.Success();
			  }

			  CreateIndex();

			  // when
			  using ( Transaction tx = beginTransaction() )
			  {
					int label = tx.TokenRead().nodeLabel("Node");
					int prop = tx.TokenRead().propertyKey("prop");
					IndexReference index = tx.SchemaRead().index(label, prop);

					using ( NodeValueIndexCursor cursor = tx.Cursors().allocateNodeValueIndexCursor() )
					{
						 NodeWithProp( tx, "allow" );
						 expected.Add( NodeWithProp( tx, "bastard" ) );
						 expected.Add( NodeWithProp( tx, "bully" ) );
						 NodeWithProp( tx, "willow" );

						 IndexQuery query = IndexQuery.StringPrefix( prop, stringValue( "b" ) );
						 tx.DataRead().nodeIndexSeek(index, cursor, IndexOrder, true, query);

						 AssertResultsInOrder( expected, cursor );
					}
			  }
		 }

		 private void AssertResultsInOrder( IList<Pair<long, Value>> expected, NodeValueIndexCursor cursor )
		 {
			  IComparer<Pair<long, Value>> comparator = IndexOrder == IndexOrder.Ascending ? ( a, b ) => Values.COMPARATOR.Compare( a.other(), b.other() ) : (a, b) => Values.COMPARATOR.Compare(b.other(), a.other());

			  expected.sort( comparator );
			  IEnumerator<Pair<long, Value>> expectedRows = expected.GetEnumerator();
			  while ( cursor.Next() && expectedRows.MoveNext() )
			  {
					Pair<long, Value> expectedRow = expectedRows.Current;
					assertThat( cursor.NodeReference(), equalTo(expectedRow.First()) );
					for ( int i = 0; i < cursor.NumberOfProperties(); i++ )
					{
						 Value value = cursor.PropertyValue( i );
						 assertThat( value, equalTo( expectedRow.Other() ) );
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( expectedRows.hasNext() );
			  assertFalse( cursor.Next() );
		 }

		 private void CreateIndex()
		 {
			  using ( Neo4Net.GraphDb.Transaction tx = graphDb.beginTx() )
			  {
					graphDb.schema().indexFor(Label.label("Node")).on("prop").create();
					tx.Success();
			  }

			  using ( Neo4Net.GraphDb.Transaction tx = graphDb.beginTx() )
			  {
					graphDb.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.helpers.collection.Pair<long,org.Neo4Net.values.storable.Value> nodeWithProp(Transaction tx, Object value) throws Exception
		 private Pair<long, Value> NodeWithProp( Transaction tx, object value )
		 {
			  Write write = tx.DataWrite();
			  long node = write.NodeCreate();
			  write.NodeAddLabel( node, tx.TokenWrite().labelGetOrCreateForName("Node") );
			  Value val = Values.of( value );
			  write.NodeSetProperty( node, tx.TokenWrite().propertyKeyGetOrCreateForName("prop"), val );
			  return Pair.of( node, val );
		 }
	}

}