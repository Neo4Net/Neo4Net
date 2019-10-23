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
namespace Neo4Net.Kernel.Api.Internal
{
	using ImmutableSet = org.eclipse.collections.api.set.ImmutableSet;
	using Collectors2 = org.eclipse.collections.impl.collector.Collectors2;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Label = Neo4Net.GraphDb.Label;
	using TransactionTerminatedException = Neo4Net.GraphDb.TransactionTerminatedException;
	using Neo4Net.Helpers.Collections;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.stringValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public abstract class NodeIndexTransactionStateTestBase<G extends KernelAPIWriteTestSupport> extends KernelAPIWriteTestBase<G>
	public abstract class NodeIndexTransactionStateTestBase<G> : KernelAPIWriteTestBase<G> where G : KernelAPIWriteTestSupport
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters() public static Iterable<Object[]> data()
		 public static IEnumerable<object[]> Data()
		 {
			  return Arrays.asList(new object[][]
			  {
				  new object[] { true },
				  new object[] { false }
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public boolean needsValues;
		 public bool NeedsValues;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformStringSuffixSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformStringSuffixSearch()
		 {
			  // given
			  ISet<Pair<long, Value>> expected = new HashSet<Pair<long, Value>>();
			  using ( Transaction tx = BeginTransaction() )
			  {
					expected.Add( NodeWithProp( tx, "1suff" ) );
					NodeWithProp( tx, "pluff" );
					tx.Success();
			  }

			  CreateIndex();

			  // when
			  using ( Transaction tx = BeginTransaction() )
			  {
					int label = tx.TokenRead().nodeLabel("Node");
					int prop = tx.TokenRead().propertyKey("prop");
					expected.Add( NodeWithProp( tx, "2suff" ) );
					NodeWithPropId( tx, "skruff" );
					IndexReference index = tx.SchemaRead().index(label, prop);
					AssertNodeAndValueForSeek( expected, tx, index, NeedsValues, "pasuff", IndexQuery.StringSuffix( prop, stringValue( "suff" ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformScan()
		 {
			  // given
			  ISet<Pair<long, Value>> expected = new HashSet<Pair<long, Value>>();
			  long nodeToDelete;
			  long nodeToChange;
			  using ( Transaction tx = BeginTransaction() )
			  {
					expected.Add( NodeWithProp( tx, "suff1" ) );
					expected.Add( NodeWithProp( tx, "supp" ) );
					nodeToDelete = NodeWithPropId( tx, "supp" );
					nodeToChange = NodeWithPropId( tx, "supper" );
					tx.Success();
			  }

			  CreateIndex();

			  // when
			  using ( Transaction tx = BeginTransaction() )
			  {
					int label = tx.TokenRead().nodeLabel("Node");
					int prop = tx.TokenRead().propertyKey("prop");
					expected.Add( NodeWithProp( tx, "suff2" ) );
					tx.DataWrite().nodeDelete(nodeToDelete);
					tx.DataWrite().nodeRemoveProperty(nodeToChange, prop);

					IndexReference index = tx.SchemaRead().index(label, prop);

					// For now, scans cannot request values, since Spatial cannot provide them
					// If we have to possibility to accept values IFF they exist (which corresponds
					// to ValueCapability PARTIAL) this needs to change
					AssertNodeAndValueForScan( expected, tx, index, false, "noff" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformEqualitySeek() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformEqualitySeek()
		 {
			  // given
			  ISet<Pair<long, Value>> expected = new HashSet<Pair<long, Value>>();
			  using ( Transaction tx = BeginTransaction() )
			  {
					expected.Add( NodeWithProp( tx, "banana" ) );
					NodeWithProp( tx, "apple" );
					tx.Success();
			  }

			  CreateIndex();

			  // when
			  using ( Transaction tx = BeginTransaction() )
			  {
					int label = tx.TokenRead().nodeLabel("Node");
					int prop = tx.TokenRead().propertyKey("prop");
					expected.Add( NodeWithProp( tx, "banana" ) );
					NodeWithProp( tx, "dragonfruit" );
					IndexReference index = tx.SchemaRead().index(label, prop);
					// Equality seek does never provide values
					AssertNodeAndValueForSeek( expected, tx, index, false, "banana", IndexQuery.Exact( prop, "banana" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformStringPrefixSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformStringPrefixSearch()
		 {
			  // given
			  ISet<Pair<long, Value>> expected = new HashSet<Pair<long, Value>>();
			  using ( Transaction tx = BeginTransaction() )
			  {
					expected.Add( NodeWithProp( tx, "suff1" ) );
					NodeWithPropId( tx, "supp" );
					tx.Success();
			  }

			  CreateIndex();

			  // when
			  using ( Transaction tx = BeginTransaction() )
			  {
					int label = tx.TokenRead().nodeLabel("Node");
					int prop = tx.TokenRead().propertyKey("prop");
					expected.Add( NodeWithProp( tx, "suff2" ) );
					NodeWithPropId( tx, "skruff" );
					IndexReference index = tx.SchemaRead().index(label, prop);

					AssertNodeAndValueForSeek( expected, tx, index, NeedsValues, "suffpa", IndexQuery.StringPrefix( prop, stringValue( "suff" ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformStringRangeSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformStringRangeSearch()
		 {
			  // given
			  ISet<Pair<long, Value>> expected = new HashSet<Pair<long, Value>>();
			  using ( Transaction tx = BeginTransaction() )
			  {
					expected.Add( NodeWithProp( tx, "banana" ) );
					NodeWithProp( tx, "apple" );
					tx.Success();
			  }

			  CreateIndex();

			  // when
			  using ( Transaction tx = BeginTransaction() )
			  {
					int label = tx.TokenRead().nodeLabel("Node");
					int prop = tx.TokenRead().propertyKey("prop");
					expected.Add( NodeWithProp( tx, "cherry" ) );
					NodeWithProp( tx, "dragonfruit" );
					IndexReference index = tx.SchemaRead().index(label, prop);
					AssertNodeAndValueForSeek( expected, tx, index, NeedsValues, "berry", IndexQuery.Range( prop, "b", true, "d", false ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformStringRangeSearchWithAddedNodeInTxState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformStringRangeSearchWithAddedNodeInTxState()
		 {
			  // given
			  ISet<Pair<long, Value>> expected = new HashSet<Pair<long, Value>>();
			  long nodeToChange;
			  using ( Transaction tx = BeginTransaction() )
			  {
					expected.Add( NodeWithProp( tx, "banana" ) );
					nodeToChange = NodeWithPropId( tx, "apple" );
					tx.Success();
			  }

			  CreateIndex();

			  // when
			  using ( Transaction tx = BeginTransaction() )
			  {
					int label = tx.TokenRead().nodeLabel("Node");
					int prop = tx.TokenRead().propertyKey("prop");
					expected.Add( NodeWithProp( tx, "cherry" ) );
					NodeWithProp( tx, "dragonfruit" );
					IndexReference index = tx.SchemaRead().index(label, prop);
					TextValue newProperty = stringValue( "blueberry" );
					tx.DataWrite().nodeSetProperty(nodeToChange, prop, newProperty);
					expected.Add( Pair.of( nodeToChange, newProperty ) );

					AssertNodeAndValueForSeek( expected, tx, index, NeedsValues, "berry", IndexQuery.Range( prop, "b", true, "d", false ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformStringRangeSearchWithRemovedNodeInTxState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformStringRangeSearchWithRemovedNodeInTxState()
		 {
			  // given
			  ISet<Pair<long, Value>> expected = new HashSet<Pair<long, Value>>();
			  long nodeToChange;
			  using ( Transaction tx = BeginTransaction() )
			  {
					nodeToChange = NodeWithPropId( tx, "banana" );
					NodeWithPropId( tx, "apple" );
					tx.Success();
			  }

			  CreateIndex();

			  // when
			  using ( Transaction tx = BeginTransaction() )
			  {
					int label = tx.TokenRead().nodeLabel("Node");
					int prop = tx.TokenRead().propertyKey("prop");
					expected.Add( NodeWithProp( tx, "cherry" ) );
					NodeWithProp( tx, "dragonfruit" );
					IndexReference index = tx.SchemaRead().index(label, prop);
					TextValue newProperty = stringValue( "kiwi" );
					tx.DataWrite().nodeSetProperty(nodeToChange, prop, newProperty);

					AssertNodeAndValueForSeek( expected, tx, index, NeedsValues, "berry", IndexQuery.Range( prop, "b", true, "d", false ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformStringRangeSearchWithDeletedNodeInTxState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformStringRangeSearchWithDeletedNodeInTxState()
		 {
			  // given
			  ISet<Pair<long, Value>> expected = new HashSet<Pair<long, Value>>();
			  long nodeToChange;
			  using ( Transaction tx = BeginTransaction() )
			  {
					nodeToChange = NodeWithPropId( tx, "banana" );
					NodeWithPropId( tx, "apple" );
					tx.Success();
			  }

			  CreateIndex();

			  // when
			  using ( Transaction tx = BeginTransaction() )
			  {
					int label = tx.TokenRead().nodeLabel("Node");
					int prop = tx.TokenRead().propertyKey("prop");
					expected.Add( NodeWithProp( tx, "cherry" ) );
					NodeWithProp( tx, "dragonfruit" );
					IndexReference index = tx.SchemaRead().index(label, prop);
					tx.DataWrite().nodeDelete(nodeToChange);

					AssertNodeAndValueForSeek( expected, tx, index, NeedsValues, "berry", IndexQuery.Range( prop, "b", true, "d", false ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformStringContainsSearch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformStringContainsSearch()
		 {
			  // given
			  ISet<Pair<long, Value>> expected = new HashSet<Pair<long, Value>>();
			  using ( Transaction tx = BeginTransaction() )
			  {
					expected.Add( NodeWithProp( tx, "gnomebat" ) );
					NodeWithPropId( tx, "fishwombat" );
					tx.Success();
			  }

			  CreateIndex();

			  // when
			  using ( Transaction tx = BeginTransaction() )
			  {
					int label = tx.TokenRead().nodeLabel("Node");
					int prop = tx.TokenRead().propertyKey("prop");
					expected.Add( NodeWithProp( tx, "homeopatic" ) );
					NodeWithPropId( tx, "telephonecompany" );
					IndexReference index = tx.SchemaRead().index(label, prop);

					AssertNodeAndValueForSeek( expected, tx, index, NeedsValues, "immense", IndexQuery.StringContains( prop, stringValue( "me" ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIfTransactionTerminated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowIfTransactionTerminated()
		 {
			  using ( Transaction tx = BeginTransaction() )
			  {
					// given
					Terminate( tx );

					// expect
					Exception.expect( typeof( TransactionTerminatedException ) );

					// when
					tx.DataRead().nodeExists(42);
			  }
		 }

		 protected internal abstract void Terminate( Transaction transaction );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long nodeWithPropId(Transaction tx, Object value) throws Exception
		 private long NodeWithPropId( Transaction tx, object value )
		 {
			  return NodeWithProp( tx, value ).first();
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

		 /// <summary>
		 /// Perform an index seek and assert that the correct nodes and values were found.
		 /// 
		 /// Since this method modifies TX state for the test it is not safe to call this method more than once in the same transaction.
		 /// </summary>
		 /// <param name="expected"> the expected nodes and values </param>
		 /// <param name="tx"> the transaction </param>
		 /// <param name="index"> the index </param>
		 /// <param name="needsValues"> if the index is expected to provide values </param>
		 /// <param name="anotherValueFoundByQuery"> a values that would be found by the index queries, if a node with that value existed. This method
		 /// will create a node with that value, after initializing the cursor and assert that the new node is not found. </param>
		 /// <param name="queries"> the index queries </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertNodeAndValueForSeek(java.util.Set<org.Neo4Net.helpers.collection.Pair<long,org.Neo4Net.values.storable.Value>> expected, Transaction tx, IndexReference index, boolean needsValues, Object anotherValueFoundByQuery, IndexQuery... queries) throws Exception
		 private void AssertNodeAndValueForSeek( ISet<Pair<long, Value>> expected, Transaction tx, IndexReference index, bool needsValues, object anotherValueFoundByQuery, params IndexQuery[] queries )
		 {
			  using ( NodeValueIndexCursor nodes = tx.Cursors().allocateNodeValueIndexCursor() )
			  {
					tx.DataRead().nodeIndexSeek(index, nodes, IndexOrder.None, needsValues, queries);
					AssertNodeAndValue( expected, tx, needsValues, anotherValueFoundByQuery, nodes );
			  }
		 }

		 /// <summary>
		 /// Perform an index scan and assert that the correct nodes and values were found.
		 /// 
		 /// Since this method modifies TX state for the test it is not safe to call this method more than once in the same transaction.
		 /// </summary>
		 /// <param name="expected"> the expected nodes and values </param>
		 /// <param name="tx"> the transaction </param>
		 /// <param name="index"> the index </param>
		 /// <param name="needsValues"> if the index is expected to provide values </param>
		 /// <param name="anotherValueFoundByQuery"> a values that would be found by, if a node with that value existed. This method
		 /// will create a node with that value, after initializing the cursor and assert that the new node is not found. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertNodeAndValueForScan(java.util.Set<org.Neo4Net.helpers.collection.Pair<long,org.Neo4Net.values.storable.Value>> expected, Transaction tx, IndexReference index, boolean needsValues, Object anotherValueFoundByQuery) throws Exception
		 private void AssertNodeAndValueForScan( ISet<Pair<long, Value>> expected, Transaction tx, IndexReference index, bool needsValues, object anotherValueFoundByQuery )
		 {
			  using ( NodeValueIndexCursor nodes = tx.Cursors().allocateNodeValueIndexCursor() )
			  {
					tx.DataRead().nodeIndexScan(index, nodes, IndexOrder.None, needsValues);
					AssertNodeAndValue( expected, tx, needsValues, anotherValueFoundByQuery, nodes );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertNodeAndValue(java.util.Set<org.Neo4Net.helpers.collection.Pair<long,org.Neo4Net.values.storable.Value>> expected, Transaction tx, boolean needsValues, Object anotherValueFoundByQuery, NodeValueIndexCursor nodes) throws Exception
		 private void AssertNodeAndValue( ISet<Pair<long, Value>> expected, Transaction tx, bool needsValues, object anotherValueFoundByQuery, NodeValueIndexCursor nodes )
		 {
			  // Modify tx state with changes that should not be reflected in the cursor, since it was already initialized in the above statement
			  foreach ( Pair<long, Value> pair in expected )
			  {
					tx.DataWrite().nodeDelete(pair.First());
			  }
			  NodeWithPropId( tx, anotherValueFoundByQuery );

			  if ( needsValues )
			  {
					ISet<Pair<long, Value>> found = new HashSet<Pair<long, Value>>();
					while ( nodes.Next() )
					{
						 found.Add( Pair.of( nodes.NodeReference(), nodes.PropertyValue(0) ) );
					}

					assertThat( found, equalTo( expected ) );
			  }
			  else
			  {
					ISet<long> foundIds = new HashSet<long>();
					while ( nodes.Next() )
					{
						 foundIds.Add( nodes.NodeReference() );
					}
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					ImmutableSet<long> expectedIds = expected.Select( Pair::first ).collect( Collectors2.toImmutableSet() );

					assertThat( foundIds, equalTo( expectedIds ) );
			  }
		 }
	}

}