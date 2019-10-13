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
namespace Neo4Net.@internal.Kernel.Api
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using SchemaKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.SchemaKernelException;
	using LabelSchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using RelationTypeSchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.RelationTypeSchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.IndexReference.NO_INDEX;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("Duplicates") public abstract class SchemaReadWriteTestBase<G extends KernelAPIWriteTestSupport> extends KernelAPIWriteTestBase<G>
	public abstract class SchemaReadWriteTestBase<G> : KernelAPIWriteTestBase<G> where G : KernelAPIWriteTestSupport
	{
		 private int _label, _label2, _type, _prop1, _prop2, _prop3;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaRead schemaRead = transaction.SchemaRead();
					SchemaWrite schemaWrite = transaction.SchemaWrite();
					IEnumerator<ConstraintDescriptor> constraints = schemaRead.ConstraintsGetAll();
					while ( constraints.MoveNext() )
					{
						 schemaWrite.ConstraintDrop( constraints.Current );
					}
					IEnumerator<IndexReference> indexes = schemaRead.IndexesGetAll();
					while ( indexes.MoveNext() )
					{
						 schemaWrite.IndexDrop( indexes.Current );
					}

					TokenWrite tokenWrite = transaction.TokenWrite();
					_label = tokenWrite.LabelGetOrCreateForName( "label" );
					_label2 = tokenWrite.LabelGetOrCreateForName( "label2" );
					_type = tokenWrite.RelationshipTypeGetOrCreateForName( "relationship" );
					_prop1 = tokenWrite.PropertyKeyGetOrCreateForName( "prop1" );
					_prop2 = tokenWrite.PropertyKeyGetOrCreateForName( "prop2" );
					_prop3 = tokenWrite.PropertyKeyGetOrCreateForName( "prop3" );
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindNonExistentIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindNonExistentIndex()
		 {
			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaRead schemaRead = transaction.SchemaRead();
					assertThat( schemaRead.Index( _label, _prop1 ), equalTo( IndexReference.NO_INDEX ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateIndex()
		 {
			  IndexReference index;
			  using ( Transaction transaction = beginTransaction() )
			  {
					index = transaction.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaRead schemaRead = transaction.SchemaRead();
					assertThat( schemaRead.Index( _label, _prop1 ), equalTo( index ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createdIndexShouldPopulateInTx() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreatedIndexShouldPopulateInTx()
		 {
			  IndexReference index;
			  using ( Transaction tx = beginTransaction() )
			  {
					SchemaReadCore before = tx.SchemaRead().snapshot();
					index = tx.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop1));
					assertThat( tx.SchemaRead().indexGetState(index), equalTo(InternalIndexState.Populating) );
					assertThat( tx.SchemaRead().snapshot().indexGetState(index), equalTo(InternalIndexState.Populating) );
					assertThat( before.IndexGetState( index ), equalTo( InternalIndexState.Populating ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropIndex()
		 {
			  IndexReference index;
			  using ( Transaction transaction = beginTransaction() )
			  {
					index = transaction.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().indexDrop(index);
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaRead schemaRead = transaction.SchemaRead();
					assertThat( schemaRead.Index( _label, _prop1 ), equalTo( NO_INDEX ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToDropNoIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToDropNoIndex()
		 {
			  //Expect
			  Exception.expect( typeof( SchemaKernelException ) );

			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().indexDrop(IndexReference.NO_INDEX);
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToDropNonExistentIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToDropNonExistentIndex()
		 {
			  IndexReference index;
			  using ( Transaction transaction = beginTransaction() )
			  {
					index = transaction.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().indexDrop(index);
					transaction.Success();
			  }

			  //Expect
			  Exception.expect( typeof( SchemaKernelException ) );

			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().indexDrop(index);
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfExistingIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfExistingIndex()
		 {
			  //Given
			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  //Expect
			  Exception.expect( typeof( SchemaKernelException ) );

			  //When
			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeIndexFromTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeIndexFromTransaction()
		 {
			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop2));
					SchemaRead schemaRead = transaction.SchemaRead();
					IndexReference index = schemaRead.Index( _label, _prop2 );
					assertThat( index.Properties(), equalTo(new int[]{ _prop2 }) );
					assertThat( 2, equalTo( Iterators.asList( schemaRead.IndexesGetAll() ).Count ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeIndexFromTransactionInSnapshot() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeIndexFromTransactionInSnapshot()
		 {
			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaReadCore schemaReadBefore = transaction.SchemaRead().snapshot();
					transaction.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop2));
					SchemaReadCore schemaReadAfter = transaction.SchemaRead().snapshot();

					IndexReference index = schemaReadBefore.Index( LabelDescriptor( _label, _prop2 ) );
					assertThat( index.Properties(), equalTo(new int[]{ _prop2 }) );
					assertThat( 2, equalTo( Iterators.asList( schemaReadBefore.IndexesGetAll() ).Count ) );

					index = schemaReadAfter.Index( LabelDescriptor( _label, _prop2 ) );
					assertThat( index.Properties(), equalTo(new int[]{ _prop2 }) );
					assertThat( 2, equalTo( Iterators.asList( schemaReadAfter.IndexesGetAll() ).Count ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeDroppedIndexFromTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeDroppedIndexFromTransaction()
		 {
			  IndexReference index;
			  using ( Transaction transaction = beginTransaction() )
			  {
					index = transaction.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().indexDrop(index);
					SchemaRead schemaRead = transaction.SchemaRead();
					assertThat( schemaRead.Index( _label, _prop2 ), equalTo( NO_INDEX ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeDroppedIndexFromTransactionInSnapshot() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeDroppedIndexFromTransactionInSnapshot()
		 {
			  IndexReference index;
			  using ( Transaction transaction = beginTransaction() )
			  {
					index = transaction.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaReadCore schemaReadBefore = transaction.SchemaRead().snapshot();
					transaction.SchemaWrite().indexDrop(index);
					SchemaReadCore schemaReadAfter = transaction.SchemaRead().snapshot();

					assertThat( schemaReadBefore.Index( LabelDescriptor( _label, _prop2 ) ), equalTo( NO_INDEX ) );
					assertThat( schemaReadAfter.Index( LabelDescriptor( _label, _prop2 ) ), equalTo( NO_INDEX ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAllIndexes()
		 {
			  IndexReference toRetain;
			  IndexReference toRetain2;
			  IndexReference toDrop;
			  IndexReference created;

			  using ( Transaction tx = beginTransaction() )
			  {
					toRetain = tx.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop1));
					toRetain2 = tx.SchemaWrite().indexCreate(LabelDescriptor(_label2, _prop1));
					toDrop = tx.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop2));
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					created = tx.SchemaWrite().indexCreate(LabelDescriptor(_label2, _prop2));
					tx.SchemaWrite().indexDrop(toDrop);

					IEnumerable<IndexReference> allIndexes = () => tx.SchemaRead().indexesGetAll();
					assertThat( allIndexes, containsInAnyOrder( toRetain, toRetain2, created ) );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllIndexesInSnapshot() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAllIndexesInSnapshot()
		 {
			  IndexReference toRetain;
			  IndexReference toRetain2;
			  IndexReference toDrop;
			  IndexReference created;

			  using ( Transaction tx = beginTransaction() )
			  {
					toRetain = tx.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop1));
					toRetain2 = tx.SchemaWrite().indexCreate(LabelDescriptor(_label2, _prop1));
					toDrop = tx.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop2));
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					SchemaReadCore before = tx.SchemaRead().snapshot();
					created = tx.SchemaWrite().indexCreate(LabelDescriptor(_label2, _prop2));
					tx.SchemaWrite().indexDrop(toDrop);

					IEnumerable<IndexReference> allIndexes = () => tx.SchemaRead().snapshot().indexesGetAll();
					assertThat( allIndexes, containsInAnyOrder( toRetain, toRetain2, created ) );
					assertThat( before.indexesGetAll, containsInAnyOrder( toRetain, toRetain2, created ) );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListIndexesByLabel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListIndexesByLabel()
		 {
			  int wrongLabel;

			  IndexReference inStore;
			  IndexReference droppedInTx;
			  IndexReference createdInTx;

			  using ( Transaction tx = beginTransaction() )
			  {
					wrongLabel = tx.TokenWrite().labelGetOrCreateForName("wrongLabel");
					tx.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(wrongLabel, _prop1));

					inStore = tx.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop1));
					droppedInTx = tx.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop2));

					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					createdInTx = tx.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop3));
					tx.SchemaWrite().indexCreate(LabelDescriptor(wrongLabel, _prop2));
					tx.SchemaWrite().indexDrop(droppedInTx);

					IEnumerable<IndexReference> indexes = () => tx.SchemaRead().indexesGetForLabel(_label);
					assertThat( indexes, containsInAnyOrder( inStore, createdInTx ) );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListIndexesByLabelInSnapshot() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListIndexesByLabelInSnapshot()
		 {
			  int wrongLabel;

			  IndexReference inStore;
			  IndexReference droppedInTx;
			  IndexReference createdInTx;

			  using ( Transaction tx = beginTransaction() )
			  {
					wrongLabel = tx.TokenWrite().labelGetOrCreateForName("wrongLabel");
					tx.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(wrongLabel, _prop1));

					inStore = tx.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop1));
					droppedInTx = tx.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop2));

					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					SchemaReadCore before = tx.SchemaRead().snapshot();
					createdInTx = tx.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop3));
					tx.SchemaWrite().indexCreate(LabelDescriptor(wrongLabel, _prop2));
					tx.SchemaWrite().indexDrop(droppedInTx);

					IEnumerable<IndexReference> indexes = () => tx.SchemaRead().snapshot().indexesGetForLabel(_label);
					assertThat( indexes, containsInAnyOrder( inStore, createdInTx ) );
					assertThat( () => before.IndexesGetForLabel(_label), containsInAnyOrder(inStore, createdInTx) );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateUniquePropertyConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateUniquePropertyConstraint()
		 {
			  ConstraintDescriptor constraint;
			  using ( Transaction transaction = beginTransaction() )
			  {
					constraint = transaction.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaRead schemaRead = transaction.SchemaRead();
					assertTrue( schemaRead.ConstraintExists( constraint ) );
					assertThat( asList( schemaRead.ConstraintsGetForLabel( _label ) ), equalTo( singletonList( constraint ) ) );
					assertThat( asList( schemaRead.Snapshot().constraintsGetForLabel(_label) ), equalTo(singletonList(constraint)) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropUniquePropertyConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropUniquePropertyConstraint()
		 {
			  ConstraintDescriptor constraint;
			  using ( Transaction transaction = beginTransaction() )
			  {
					constraint = transaction.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().constraintDrop(constraint);
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaRead schemaRead = transaction.SchemaRead();
					assertFalse( schemaRead.ConstraintExists( constraint ) );
					assertThat( asList( schemaRead.ConstraintsGetForLabel( _label ) ), empty() );
					assertThat( asList( schemaRead.Snapshot().constraintsGetForLabel(_label) ), empty() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToCreateUniqueConstraintIfExistingIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToCreateUniqueConstraintIfExistingIndex()
		 {
			  //Given
			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  //Expect
			  Exception.expect( typeof( SchemaKernelException ) );

			  //When
			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToCreateIndexIfExistingUniqueConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToCreateIndexIfExistingUniqueConstraint()
		 {
			  //Given
			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  //Expect
			  Exception.expect( typeof( SchemaKernelException ) );

			  //When
			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToDropIndexIfExistingUniqueConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToDropIndexIfExistingUniqueConstraint()
		 {
			  //Given
			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  //Expect
			  Exception.expect( typeof( SchemaKernelException ) );

			  //When
			  using ( Transaction transaction = beginTransaction() )
			  {
					IndexReference index = transaction.SchemaRead().index(_label, _prop1);
					transaction.SchemaWrite().indexDrop(index);
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToCreateUniqueConstraintIfConstraintNotSatisfied() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToCreateUniqueConstraintIfConstraintNotSatisfied()
		 {
			  //Given
			  using ( Transaction transaction = beginTransaction() )
			  {
					Write write = transaction.DataWrite();
					long node1 = write.NodeCreate();
					write.NodeAddLabel( node1, _label );
					write.NodeSetProperty( node1, _prop1, Values.intValue( 42 ) );
					long node2 = write.NodeCreate();
					write.NodeAddLabel( node2, _label );
					write.NodeSetProperty( node2, _prop1, Values.intValue( 42 ) );
					transaction.Success();
			  }

			  //Expect
			  Exception.expect( typeof( SchemaKernelException ) );

			  //When
			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(_label, _prop1));
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeUniqueConstraintFromTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeUniqueConstraintFromTransaction()
		 {
			  ConstraintDescriptor existing;
			  using ( Transaction transaction = beginTransaction() )
			  {
					existing = transaction.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaReadCore before = transaction.SchemaRead().snapshot();
					ConstraintDescriptor newConstraint = transaction.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(_label, _prop2));
					SchemaRead schemaRead = transaction.SchemaRead();
					assertTrue( schemaRead.ConstraintExists( existing ) );
					assertTrue( schemaRead.ConstraintExists( newConstraint ) );
					assertThat( asList( schemaRead.ConstraintsGetForLabel( _label ) ), containsInAnyOrder( existing, newConstraint ) );
					assertThat( asList( schemaRead.Snapshot().constraintsGetForLabel(_label) ), containsInAnyOrder(existing, newConstraint) );
					assertThat( asList( before.ConstraintsGetForLabel( _label ) ), containsInAnyOrder( existing, newConstraint ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeDroppedUniqueConstraintFromTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeDroppedUniqueConstraintFromTransaction()
		 {
			  ConstraintDescriptor existing;
			  using ( Transaction transaction = beginTransaction() )
			  {
					existing = transaction.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaReadCore before = transaction.SchemaRead().snapshot();
					transaction.SchemaWrite().constraintDrop(existing);
					SchemaRead schemaRead = transaction.SchemaRead();
					assertFalse( schemaRead.ConstraintExists( existing ) );
					assertThat( asList( schemaRead.ConstraintsGetForLabel( _label ) ), empty() );
					assertThat( asList( schemaRead.Snapshot().constraintsGetForLabel(_label) ), empty() );
					assertThat( asList( before.ConstraintsGetForLabel( _label ) ), empty() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateNodeKeyConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateNodeKeyConstraint()
		 {
			  ConstraintDescriptor constraint;
			  using ( Transaction transaction = beginTransaction() )
			  {
					constraint = transaction.SchemaWrite().nodeKeyConstraintCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaRead schemaRead = transaction.SchemaRead();
					assertTrue( schemaRead.ConstraintExists( constraint ) );
					assertThat( asList( schemaRead.ConstraintsGetForLabel( _label ) ), equalTo( singletonList( constraint ) ) );
					assertThat( asList( schemaRead.Snapshot().constraintsGetForLabel(_label) ), equalTo(singletonList(constraint)) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropNodeKeyConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropNodeKeyConstraint()
		 {
			  ConstraintDescriptor constraint;
			  using ( Transaction transaction = beginTransaction() )
			  {
					constraint = transaction.SchemaWrite().nodeKeyConstraintCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().constraintDrop(constraint);
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaRead schemaRead = transaction.SchemaRead();
					assertFalse( schemaRead.ConstraintExists( constraint ) );
					assertThat( asList( schemaRead.ConstraintsGetForLabel( _label ) ), empty() );
					assertThat( asList( schemaRead.Snapshot().constraintsGetForLabel(_label) ), empty() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToNodeKeyConstraintIfConstraintNotSatisfied() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToNodeKeyConstraintIfConstraintNotSatisfied()
		 {
			  //Given
			  using ( Transaction transaction = beginTransaction() )
			  {
					Write write = transaction.DataWrite();
					long node = write.NodeCreate();
					write.NodeAddLabel( node, _label );
					transaction.Success();
			  }

			  //Expect
			  Exception.expect( typeof( SchemaKernelException ) );

			  //When
			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().nodeKeyConstraintCreate(LabelDescriptor(_label, _prop1));
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeNodeKeyConstraintFromTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeNodeKeyConstraintFromTransaction()
		 {
			  ConstraintDescriptor existing;
			  using ( Transaction transaction = beginTransaction() )
			  {
					existing = transaction.SchemaWrite().nodeKeyConstraintCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaReadCore before = transaction.SchemaRead().snapshot();
					ConstraintDescriptor newConstraint = transaction.SchemaWrite().nodeKeyConstraintCreate(LabelDescriptor(_label, _prop2));
					SchemaRead schemaRead = transaction.SchemaRead();
					assertTrue( schemaRead.ConstraintExists( existing ) );
					assertTrue( schemaRead.ConstraintExists( newConstraint ) );
					assertThat( asList( schemaRead.ConstraintsGetForLabel( _label ) ), containsInAnyOrder( existing, newConstraint ) );
					assertThat( asList( schemaRead.Snapshot().constraintsGetForLabel(_label) ), containsInAnyOrder(existing, newConstraint) );
					assertThat( asList( before.ConstraintsGetForLabel( _label ) ), containsInAnyOrder( existing, newConstraint ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeDroppedNodeKeyConstraintFromTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeDroppedNodeKeyConstraintFromTransaction()
		 {
			  ConstraintDescriptor existing;
			  using ( Transaction transaction = beginTransaction() )
			  {
					existing = transaction.SchemaWrite().nodeKeyConstraintCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaReadCore before = transaction.SchemaRead().snapshot();
					transaction.SchemaWrite().constraintDrop(existing);
					SchemaRead schemaRead = transaction.SchemaRead();
					assertFalse( schemaRead.ConstraintExists( existing ) );
					assertThat( asList( schemaRead.ConstraintsGetForLabel( _label ) ), empty() );
					assertThat( asList( schemaRead.Snapshot().constraintsGetForLabel(_label) ), empty() );
					assertThat( asList( before.ConstraintsGetForLabel( _label ) ), empty() );

			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateNodePropertyExistenceConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateNodePropertyExistenceConstraint()
		 {
			  ConstraintDescriptor constraint;
			  using ( Transaction transaction = beginTransaction() )
			  {
					constraint = transaction.SchemaWrite().nodePropertyExistenceConstraintCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaRead schemaRead = transaction.SchemaRead();
					assertTrue( schemaRead.ConstraintExists( constraint ) );
					assertThat( asList( schemaRead.ConstraintsGetForLabel( _label ) ), equalTo( singletonList( constraint ) ) );
					assertThat( asList( schemaRead.Snapshot().constraintsGetForLabel(_label) ), equalTo(singletonList(constraint)) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropNodePropertyExistenceConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropNodePropertyExistenceConstraint()
		 {
			  ConstraintDescriptor constraint;
			  using ( Transaction transaction = beginTransaction() )
			  {
					constraint = transaction.SchemaWrite().nodePropertyExistenceConstraintCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().constraintDrop(constraint);
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaRead schemaRead = transaction.SchemaRead();
					assertFalse( schemaRead.ConstraintExists( constraint ) );
					assertThat( asList( schemaRead.ConstraintsGetForLabel( _label ) ), empty() );
					assertThat( asList( schemaRead.Snapshot().constraintsGetForLabel(_label) ), empty() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToCreatePropertyExistenceConstraintIfConstraintNotSatisfied() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToCreatePropertyExistenceConstraintIfConstraintNotSatisfied()
		 {
			  //Given
			  using ( Transaction transaction = beginTransaction() )
			  {
					Write write = transaction.DataWrite();
					long node = write.NodeCreate();
					write.NodeAddLabel( node, _label );
					transaction.Success();
			  }

			  //Expect
			  Exception.expect( typeof( SchemaKernelException ) );

			  //When
			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().nodePropertyExistenceConstraintCreate(LabelDescriptor(_label, _prop1));
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeNodePropertyExistenceConstraintFromTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeNodePropertyExistenceConstraintFromTransaction()
		 {
			  ConstraintDescriptor existing;
			  using ( Transaction transaction = beginTransaction() )
			  {
					existing = transaction.SchemaWrite().nodePropertyExistenceConstraintCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaReadCore before = transaction.SchemaRead().snapshot();
					ConstraintDescriptor newConstraint = transaction.SchemaWrite().nodePropertyExistenceConstraintCreate(LabelDescriptor(_label, _prop2));
					SchemaRead schemaRead = transaction.SchemaRead();
					assertTrue( schemaRead.ConstraintExists( existing ) );
					assertTrue( schemaRead.ConstraintExists( newConstraint ) );
					assertThat( asList( schemaRead.ConstraintsGetForLabel( _label ) ), containsInAnyOrder( existing, newConstraint ) );
					assertThat( asList( schemaRead.Snapshot().constraintsGetForLabel(_label) ), containsInAnyOrder(existing, newConstraint) );
					assertThat( asList( before.ConstraintsGetForLabel( _label ) ), containsInAnyOrder( existing, newConstraint ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeDroppedNodePropertyExistenceConstraintFromTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeDroppedNodePropertyExistenceConstraintFromTransaction()
		 {
			  ConstraintDescriptor existing;
			  using ( Transaction transaction = beginTransaction() )
			  {
					existing = transaction.SchemaWrite().nodePropertyExistenceConstraintCreate(LabelDescriptor(_label, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaReadCore before = transaction.SchemaRead().snapshot();
					transaction.SchemaWrite().constraintDrop(existing);
					SchemaRead schemaRead = transaction.SchemaRead();
					assertFalse( schemaRead.ConstraintExists( existing ) );

					assertThat( schemaRead.Index( _label, _prop2 ), equalTo( NO_INDEX ) );
					assertThat( asList( schemaRead.ConstraintsGetForLabel( _label ) ), empty() );
					assertThat( asList( schemaRead.Snapshot().constraintsGetForLabel(_label) ), empty() );
					assertThat( asList( before.ConstraintsGetForLabel( _label ) ), empty() );

			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateRelationshipPropertyExistenceConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateRelationshipPropertyExistenceConstraint()
		 {
			  ConstraintDescriptor constraint;
			  using ( Transaction transaction = beginTransaction() )
			  {
					constraint = transaction.SchemaWrite().relationshipPropertyExistenceConstraintCreate(TypeDescriptor(_type, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaRead schemaRead = transaction.SchemaRead();
					assertTrue( schemaRead.ConstraintExists( constraint ) );
					assertThat( asList( schemaRead.ConstraintsGetForRelationshipType( _type ) ), equalTo( singletonList( constraint ) ) );
					assertThat( asList( schemaRead.Snapshot().constraintsGetForRelationshipType(_type) ), equalTo(singletonList(constraint)) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropRelationshipPropertyExistenceConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropRelationshipPropertyExistenceConstraint()
		 {
			  ConstraintDescriptor constraint;
			  using ( Transaction transaction = beginTransaction() )
			  {
					constraint = transaction.SchemaWrite().relationshipPropertyExistenceConstraintCreate(TypeDescriptor(_type, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().constraintDrop(constraint);
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaRead schemaRead = transaction.SchemaRead();
					assertFalse( schemaRead.ConstraintExists( constraint ) );
					assertThat( asList( schemaRead.ConstraintsGetForRelationshipType( _type ) ), empty() );
					assertThat( asList( schemaRead.Snapshot().constraintsGetForRelationshipType(_type) ), empty() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToCreateRelationshipPropertyExistenceConstraintIfConstraintNotSatisfied() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToCreateRelationshipPropertyExistenceConstraintIfConstraintNotSatisfied()
		 {
			  //Given
			  using ( Transaction transaction = beginTransaction() )
			  {
					Write write = transaction.DataWrite();
					write.RelationshipCreate( write.NodeCreate(), _type, write.NodeCreate() );
					transaction.Success();
			  }

			  //Expect
			  Exception.expect( typeof( SchemaKernelException ) );

			  //When
			  using ( Transaction transaction = beginTransaction() )
			  {
					transaction.SchemaWrite().relationshipPropertyExistenceConstraintCreate(TypeDescriptor(_type, _prop1));
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeRelationshipPropertyExistenceConstraintFromTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeRelationshipPropertyExistenceConstraintFromTransaction()
		 {
			  ConstraintDescriptor existing;
			  using ( Transaction transaction = beginTransaction() )
			  {
					existing = transaction.SchemaWrite().relationshipPropertyExistenceConstraintCreate(TypeDescriptor(_type, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaReadCore before = transaction.SchemaRead().snapshot();
					ConstraintDescriptor newConstraint = transaction.SchemaWrite().relationshipPropertyExistenceConstraintCreate(TypeDescriptor(_type, _prop2));
					SchemaRead schemaRead = transaction.SchemaRead();
					assertTrue( schemaRead.ConstraintExists( existing ) );
					assertTrue( schemaRead.ConstraintExists( newConstraint ) );
					assertThat( asList( schemaRead.ConstraintsGetForRelationshipType( _type ) ), containsInAnyOrder( existing, newConstraint ) );
					assertThat( asList( schemaRead.Snapshot().constraintsGetForRelationshipType(_type) ), containsInAnyOrder(existing, newConstraint) );
					assertThat( asList( before.ConstraintsGetForRelationshipType( _type ) ), containsInAnyOrder( existing, newConstraint ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeDroppedRelationshipPropertyExistenceConstraintFromTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeDroppedRelationshipPropertyExistenceConstraintFromTransaction()
		 {
			  ConstraintDescriptor existing;
			  using ( Transaction transaction = beginTransaction() )
			  {
					existing = transaction.SchemaWrite().relationshipPropertyExistenceConstraintCreate(TypeDescriptor(_type, _prop1));
					transaction.Success();
			  }

			  using ( Transaction transaction = beginTransaction() )
			  {
					SchemaReadCore before = transaction.SchemaRead().snapshot();
					transaction.SchemaWrite().constraintDrop(existing);
					SchemaRead schemaRead = transaction.SchemaRead();
					assertFalse( schemaRead.ConstraintExists( existing ) );

					assertThat( schemaRead.Index( _type, _prop2 ), equalTo( NO_INDEX ) );
					assertThat( asList( schemaRead.ConstraintsGetForLabel( _label ) ), empty() );
					assertThat( asList( schemaRead.Snapshot().constraintsGetForLabel(_label) ), empty() );
					assertThat( asList( before.ConstraintsGetForLabel( _label ) ), empty() );

			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllConstraints() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAllConstraints()
		 {
			  ConstraintDescriptor toRetain;
			  ConstraintDescriptor toRetain2;
			  ConstraintDescriptor toDrop;
			  ConstraintDescriptor created;
			  using ( Transaction tx = beginTransaction() )
			  {
					toRetain = tx.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(_label, _prop1));
					toRetain2 = tx.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(_label2, _prop1));
					toDrop = tx.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(_label, _prop2));
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					created = tx.SchemaWrite().nodePropertyExistenceConstraintCreate(LabelDescriptor(_label, _prop1));
					tx.SchemaWrite().constraintDrop(toDrop);

					IEnumerable<ConstraintDescriptor> allConstraints = () => tx.SchemaRead().constraintsGetAll();
					assertThat( allConstraints, containsInAnyOrder( toRetain, toRetain2, created ) );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllConstraintsInSnapshot() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAllConstraintsInSnapshot()
		 {
			  ConstraintDescriptor toRetain;
			  ConstraintDescriptor toRetain2;
			  ConstraintDescriptor toDrop;
			  ConstraintDescriptor created;
			  using ( Transaction tx = beginTransaction() )
			  {
					toRetain = tx.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(_label, _prop1));
					toRetain2 = tx.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(_label2, _prop1));
					toDrop = tx.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(_label, _prop2));
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					SchemaReadCore before = tx.SchemaRead().snapshot();
					created = tx.SchemaWrite().nodePropertyExistenceConstraintCreate(LabelDescriptor(_label, _prop1));
					tx.SchemaWrite().constraintDrop(toDrop);

					IEnumerable<ConstraintDescriptor> allConstraints = () => tx.SchemaRead().snapshot().constraintsGetAll();
					assertThat( allConstraints, containsInAnyOrder( toRetain, toRetain2, created ) );
					assertThat( before.constraintsGetAll, containsInAnyOrder( toRetain, toRetain2, created ) );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListConstraintsByLabel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListConstraintsByLabel()
		 {
			  int wrongLabel;

			  ConstraintDescriptor inStore;
			  ConstraintDescriptor droppedInTx;
			  ConstraintDescriptor createdInTx;

			  using ( Transaction tx = beginTransaction() )
			  {
					wrongLabel = tx.TokenWrite().labelGetOrCreateForName("wrongLabel");
					tx.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(wrongLabel, _prop1));

					inStore = tx.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(_label, _prop1));
					droppedInTx = tx.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(_label, _prop2));

					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					SchemaReadCore before = tx.SchemaRead().snapshot();
					createdInTx = tx.SchemaWrite().nodePropertyExistenceConstraintCreate(LabelDescriptor(_label, _prop1));
					tx.SchemaWrite().nodePropertyExistenceConstraintCreate(LabelDescriptor(wrongLabel, _prop1));
					tx.SchemaWrite().constraintDrop(droppedInTx);

					IEnumerable<ConstraintDescriptor> allConstraints = () => tx.SchemaRead().constraintsGetForLabel(_label);
					assertThat( allConstraints, containsInAnyOrder( inStore, createdInTx ) );
					assertThat( () => before.ConstraintsGetForLabel(_label), containsInAnyOrder(inStore, createdInTx) );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException.class) public void shouldFailIndexCreateForRepeatedProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIndexCreateForRepeatedProperties()
		 {
			  using ( Transaction tx = beginTransaction() )
			  {
					tx.SchemaWrite().indexCreate(LabelDescriptor(_label, _prop1, _prop1));
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException.class) public void shouldFailUniquenessConstraintCreateForRepeatedProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailUniquenessConstraintCreateForRepeatedProperties()
		 {
			  using ( Transaction tx = beginTransaction() )
			  {
					tx.SchemaWrite().uniquePropertyConstraintCreate(LabelDescriptor(_label, _prop1, _prop1));
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException.class) public void shouldFailNodeKeyCreateForRepeatedProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailNodeKeyCreateForRepeatedProperties()
		 {
			  using ( Transaction tx = beginTransaction() )
			  {
					tx.SchemaWrite().nodeKeyConstraintCreate(LabelDescriptor(_label, _prop1, _prop1));
			  }
		 }

		 protected internal abstract RelationTypeSchemaDescriptor TypeDescriptor( int label, params int[] props );

		 protected internal abstract LabelSchemaDescriptor LabelDescriptor( int label, params int[] props );
	}

}