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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using IEntityNotFoundException = Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.NO_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.stringValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("Duplicates") public abstract class NodeWriteTestBase<G extends KernelAPIWriteTestSupport> extends KernelAPIWriteTestBase<G>
	public abstract class NodeWriteTestBase<G> : KernelAPIWriteTestBase<G> where G : KernelAPIWriteTestSupport
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 private const string PROPERTY_KEY = "prop";
		 private const string LABEL_NAME = "Town";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateNode()
		 {
			  long node;
			  using ( Transaction tx = BeginTransaction() )
			  {
					node = tx.DataWrite().nodeCreate();
					tx.Success();
			  }

			  using ( Neo4Net.GraphDb.Transaction ignore = graphDb.beginTx() )
			  {
					assertEquals( node, graphDb.getNodeById( node ).Id );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackOnFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRollbackOnFailure()
		 {
			  long node;
			  using ( Transaction tx = BeginTransaction() )
			  {
					node = tx.DataWrite().nodeCreate();
					tx.Failure();
			  }

			  try
			  {
					  using ( Neo4Net.GraphDb.Transaction ignore = graphDb.beginTx() )
					  {
						graphDb.getNodeById( node );
						fail( "There should be no node" );
					  }
			  }
			  catch ( NotFoundException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveNode()
		 {
			  long node = CreateNode();

			  using ( Transaction tx = BeginTransaction() )
			  {
					tx.DataWrite().nodeDelete(node);
					tx.Success();
			  }
			  using ( Neo4Net.GraphDb.Transaction ignore = graphDb.beginTx() )
			  {
					try
					{
						 graphDb.getNodeById( node );
						 fail( "Did not remove node" );
					}
					catch ( NotFoundException )
					{
						 // expected
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRemoveNodeThatDoesNotExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotRemoveNodeThatDoesNotExist()
		 {
			  long node = 0;

			  using ( Transaction tx = BeginTransaction() )
			  {
					assertFalse( tx.DataWrite().nodeDelete(node) );
					tx.Failure();
			  }
			  using ( Transaction tx = BeginTransaction() )
			  {
					assertFalse( tx.DataWrite().nodeDelete(node) );
					tx.Success();
			  }
			  // should not crash
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddLabelNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddLabelNode()
		 {
			  // Given
			  long node = CreateNode();

			  // When
			  using ( Transaction tx = BeginTransaction() )
			  {
					int labelId = tx.Token().labelGetOrCreateForName(LABEL_NAME);
					assertTrue( tx.DataWrite().nodeAddLabel(node, labelId) );
					tx.Success();
			  }

			  // Then
			  AssertLabels( node, LABEL_NAME );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddLabelNodeOnce() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddLabelNodeOnce()
		 {
			  long node = CreateNodeWithLabel( LABEL_NAME );

			  using ( Transaction tx = BeginTransaction() )
			  {
					int labelId = tx.Token().labelGetOrCreateForName(LABEL_NAME);
					assertFalse( tx.DataWrite().nodeAddLabel(node, labelId) );
					tx.Success();
			  }

			  AssertLabels( node, LABEL_NAME );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveLabel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveLabel()
		 {
			  long nodeId = CreateNodeWithLabel( LABEL_NAME );

			  using ( Transaction tx = BeginTransaction() )
			  {
					int labelId = tx.Token().labelGetOrCreateForName(LABEL_NAME);
					assertTrue( tx.DataWrite().nodeRemoveLabel(nodeId, labelId) );
					tx.Success();
			  }

			  AssertNoLabels( nodeId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAddLabelToNonExistingNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAddLabelToNonExistingNode()
		 {
			  long node = 1337L;

			  using ( Transaction tx = BeginTransaction() )
			  {
					int labelId = tx.Token().labelGetOrCreateForName(LABEL_NAME);
					Exception.expect( typeof( KernelException ) );
					tx.DataWrite().nodeAddLabel(node, labelId);
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveLabelOnce() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveLabelOnce()
		 {
			  int labelId;
			  long nodeId = CreateNodeWithLabel( LABEL_NAME );

			  using ( Transaction tx = BeginTransaction() )
			  {
					labelId = tx.Token().labelGetOrCreateForName(LABEL_NAME);
					assertTrue( tx.DataWrite().nodeRemoveLabel(nodeId, labelId) );
					tx.Success();
			  }

			  using ( Transaction tx = BeginTransaction() )
			  {
					labelId = tx.Token().labelGetOrCreateForName(LABEL_NAME);
					assertFalse( tx.DataWrite().nodeRemoveLabel(nodeId, labelId) );
					tx.Success();
			  }

			  AssertNoLabels( nodeId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddPropertyToNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddPropertyToNode()
		 {
			  // Given
			  long node = CreateNode();

			  // When
			  using ( Transaction tx = BeginTransaction() )
			  {
					int token = tx.Token().propertyKeyGetOrCreateForName(PROPERTY_KEY);
					assertThat( tx.DataWrite().nodeSetProperty(node, token, stringValue("hello")), equalTo(NO_VALUE) );
					tx.Success();
			  }

			  // Then
			  AssertProperty( node, PROPERTY_KEY, "hello" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRollbackSetNodeProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRollbackSetNodeProperty()
		 {
			  // Given
			  long node = CreateNode();

			  // When
			  using ( Transaction tx = BeginTransaction() )
			  {
					int token = tx.Token().propertyKeyGetOrCreateForName(PROPERTY_KEY);
					assertThat( tx.DataWrite().nodeSetProperty(node, token, stringValue("hello")), equalTo(NO_VALUE) );
					tx.Failure();
			  }

			  // Then
			  AssertNoProperty( node, PROPERTY_KEY );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowWhenSettingPropertyOnDeletedNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowWhenSettingPropertyOnDeletedNode()
		 {
			  // Given
			  long node = CreateNode();
			  DeleteNode( node );

			  // When
			  try
			  {
					  using ( Transaction tx = BeginTransaction() )
					  {
						int token = tx.Token().propertyKeyGetOrCreateForName(PROPERTY_KEY);
						tx.DataWrite().nodeSetProperty(node, token, stringValue("hello"));
						fail( "Expected IEntityNotFoundException" );
					  }
			  }
			  catch ( IEntityNotFoundException )
			  {
					// wanted
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdatePropertyToNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdatePropertyToNode()
		 {
			  // Given
			  long node = CreateNodeWithProperty( PROPERTY_KEY, 42 );

			  // When
			  using ( Transaction tx = BeginTransaction() )
			  {
					int token = tx.Token().propertyKeyGetOrCreateForName(PROPERTY_KEY);
					assertThat( tx.DataWrite().nodeSetProperty(node, token, stringValue("hello")), equalTo(intValue(42)) );
					tx.Success();
			  }

			  // Then
			  AssertProperty( node, PROPERTY_KEY, "hello" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemovePropertyFromNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemovePropertyFromNode()
		 {
			  // Given
			  long node = CreateNodeWithProperty( PROPERTY_KEY, 42 );

			  // When
			  using ( Transaction tx = BeginTransaction() )
			  {
					int token = tx.Token().propertyKeyGetOrCreateForName(PROPERTY_KEY);
					assertThat( tx.DataWrite().nodeRemoveProperty(node, token), equalTo(intValue(42)) );
					tx.Success();
			  }

			  // Then
			  AssertNoProperty( node, PROPERTY_KEY );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveNonExistingPropertyFromNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveNonExistingPropertyFromNode()
		 {
			  // Given
			  long node = CreateNode();

			  // When
			  using ( Transaction tx = BeginTransaction() )
			  {
					int token = tx.Token().propertyKeyGetOrCreateForName(PROPERTY_KEY);
					assertThat( tx.DataWrite().nodeRemoveProperty(node, token), equalTo(NO_VALUE) );
					tx.Success();
			  }

			  // Then
			  AssertNoProperty( node, PROPERTY_KEY );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemovePropertyFromNodeTwice() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemovePropertyFromNodeTwice()
		 {
			  // Given
			  long node = CreateNodeWithProperty( PROPERTY_KEY, 42 );

			  // When
			  using ( Transaction tx = BeginTransaction() )
			  {
					int token = tx.Token().propertyKeyGetOrCreateForName(PROPERTY_KEY);
					assertThat( tx.DataWrite().nodeRemoveProperty(node, token), equalTo(intValue(42)) );
					assertThat( tx.DataWrite().nodeRemoveProperty(node, token), equalTo(NO_VALUE) );
					tx.Success();
			  }

			  // Then
			  AssertNoProperty( node, PROPERTY_KEY );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdatePropertyToNodeInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdatePropertyToNodeInTransaction()
		 {
			  // Given
			  long node = CreateNode();

			  // When
			  using ( Transaction tx = BeginTransaction() )
			  {
					int token = tx.Token().propertyKeyGetOrCreateForName(PROPERTY_KEY);
					assertThat( tx.DataWrite().nodeSetProperty(node, token, stringValue("hello")), equalTo(NO_VALUE) );
					assertThat( tx.DataWrite().nodeSetProperty(node, token, stringValue("world")), equalTo(stringValue("hello")) );
					assertThat( tx.DataWrite().nodeSetProperty(node, token, intValue(1337)), equalTo(stringValue("world")) );
					tx.Success();
			  }

			  // Then
			  AssertProperty( node, PROPERTY_KEY, 1337 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveReSetAndTwiceRemovePropertyOnNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveReSetAndTwiceRemovePropertyOnNode()
		 {
			  // given
			  long node = CreateNodeWithProperty( PROPERTY_KEY, "bar" );

			  // when

			  using ( Transaction tx = BeginTransaction() )
			  {
					int prop = tx.Token().propertyKeyGetOrCreateForName(PROPERTY_KEY);
					tx.DataWrite().nodeRemoveProperty(node, prop);
					tx.DataWrite().nodeSetProperty(node, prop, Values.of("bar"));
					tx.DataWrite().nodeRemoveProperty(node, prop);
					tx.DataWrite().nodeRemoveProperty(node, prop);
					tx.Success();
			  }

			  // then
			  AssertNoProperty( node, PROPERTY_KEY );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotWriteWhenSettingPropertyToSameValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotWriteWhenSettingPropertyToSameValue()
		 {
			  // Given
			  Value theValue = stringValue( "The Value" );
			  long nodeId = CreateNodeWithProperty( PROPERTY_KEY, theValue.AsObject() );

			  // When
			  Transaction tx = BeginTransaction();
			  int property = tx.Token().propertyKeyGetOrCreateForName(PROPERTY_KEY);
			  assertThat( tx.DataWrite().nodeSetProperty(nodeId, property, theValue), equalTo(theValue) );
			  tx.Success();

			  assertThat( tx.CloseTransaction(), equalTo(Transaction_Fields.READ_ONLY) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetAndReadLargeByteArrayPropertyToNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetAndReadLargeByteArrayPropertyToNode()
		 {
			  // Given
			  int prop;
			  long node = CreateNode();
			  Value largeByteArray = Values.of( new sbyte[100_000] );

			  // When
			  using ( Transaction tx = BeginTransaction() )
			  {
					prop = tx.Token().propertyKeyGetOrCreateForName(PROPERTY_KEY);
					assertThat( tx.DataWrite().nodeSetProperty(node, prop, largeByteArray), equalTo(NO_VALUE) );
					tx.Success();
			  }

			  // Then
			  using ( Transaction tx = BeginTransaction(), NodeCursor nodeCursor = tx.Cursors().allocateNodeCursor(), PropertyCursor propertyCursor = tx.Cursors().allocatePropertyCursor() )
			  {
					tx.DataRead().singleNode(node, nodeCursor);
					assertTrue( nodeCursor.Next() );
					nodeCursor.Properties( propertyCursor );
					assertTrue( propertyCursor.Next() );
					assertEquals( propertyCursor.PropertyKey(), prop );
					assertThat( propertyCursor.PropertyValue(), equalTo(largeByteArray) );
			  }
		 }

		 // HELPERS

		 private long CreateNode()
		 {
			  long node;
			  using ( Neo4Net.GraphDb.Transaction ctx = graphDb.beginTx() )
			  {
					node = graphDb.createNode().Id;
					ctx.Success();
			  }
			  return node;
		 }

		 private void DeleteNode( long node )
		 {
			  using ( Neo4Net.GraphDb.Transaction ctx = graphDb.beginTx() )
			  {
					graphDb.getNodeById( node ).delete();
					ctx.Success();
			  }
		 }

		 private long CreateNodeWithLabel( string labelName )
		 {
			  long node;
			  using ( Neo4Net.GraphDb.Transaction ctx = graphDb.beginTx() )
			  {
					node = graphDb.createNode( label( labelName ) ).Id;
					ctx.Success();
			  }
			  return node;
		 }

		 private long CreateNodeWithProperty( string propertyKey, object value )
		 {
			  Node node;
			  using ( Neo4Net.GraphDb.Transaction ctx = graphDb.beginTx() )
			  {
					node = graphDb.createNode();
					node.SetProperty( propertyKey, value );
					ctx.Success();
			  }
			  return node.Id;
		 }

		 private void AssertNoLabels( long nodeId )
		 {
			  using ( Neo4Net.GraphDb.Transaction ignore = graphDb.beginTx() )
			  {
					assertThat( graphDb.getNodeById( nodeId ).Labels, equalTo( Iterables.empty() ) );
			  }
		 }

		 private void AssertLabels( long nodeId, string label )
		 {
			  using ( Neo4Net.GraphDb.Transaction ignore = graphDb.beginTx() )
			  {
					assertThat( graphDb.getNodeById( nodeId ).Labels, containsInAnyOrder( label( label ) ) );
			  }
		 }

		 private void AssertNoProperty( long node, string propertyKey )
		 {
			  using ( Neo4Net.GraphDb.Transaction ignore = graphDb.beginTx() )
			  {
					assertFalse( graphDb.getNodeById( node ).hasProperty( propertyKey ) );
			  }
		 }

		 private void AssertProperty( long node, string propertyKey, object value )
		 {
			  using ( Neo4Net.GraphDb.Transaction ignore = graphDb.beginTx() )
			  {
					assertThat( graphDb.getNodeById( node ).getProperty( propertyKey ), equalTo( value ) );
			  }
		 }
	}

}