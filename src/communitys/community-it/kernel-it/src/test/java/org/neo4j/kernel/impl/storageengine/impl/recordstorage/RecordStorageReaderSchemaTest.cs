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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using Test = org.junit.Test;


	using Label = Neo4Net.Graphdb.Label;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using IndexDefinition = Neo4Net.Graphdb.schema.IndexDefinition;
	using IndexNotFoundKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using ConstraintDescriptor = Neo4Net.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using StorageSchemaReader = Neo4Net.Storageengine.Api.StorageSchemaReader;
	using CapableIndexDescriptor = Neo4Net.Storageengine.Api.schema.CapableIndexDescriptor;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexDescriptorFactory = Neo4Net.Storageengine.Api.schema.IndexDescriptorFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class RecordStorageReaderSchemaTest : RecordStorageReaderTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllIndexes()
		 public virtual void ShouldListAllIndexes()
		 {
			  // Given
			  CreateIndex( Label1, PropertyKey );
			  CreateIndex( Label2, PropertyKey );

			  // When
			  ISet<CapableIndexDescriptor> indexes = asSet( StorageReader.indexesGetAll() );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Set<?> expectedIndexes = asSet(indexDescriptor(label1, propertyKey), indexDescriptor(label2, propertyKey));
			  ISet<object> expectedIndexes = asSet( IndexDescriptor( Label1, PropertyKey ), IndexDescriptor( Label2, PropertyKey ) );

			  assertEquals( expectedIndexes, indexes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllIndexesAtTimeOfSnapshot()
		 public virtual void ShouldListAllIndexesAtTimeOfSnapshot()
		 {
			  // Given
			  CreateIndex( Label1, PropertyKey );

			  // When
			  StorageSchemaReader snapshot = StorageReader.schemaSnapshot();
			  CreateIndex( Label2, PropertyKey );
			  ISet<CapableIndexDescriptor> indexes = asSet( snapshot.IndexesGetAll() );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Set<?> expectedIndexes = asSet(indexDescriptor(label1, propertyKey));
			  ISet<object> expectedIndexes = asSet( IndexDescriptor( Label1, PropertyKey ) );

			  assertEquals( expectedIndexes, indexes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void gettingIndexStateOfDroppedIndexViaSnapshotShouldThrow()
		 public virtual void GettingIndexStateOfDroppedIndexViaSnapshotShouldThrow()
		 {
			  // Given
			  CreateIndex( Label1, PropertyKey );

			  // When
			  StorageSchemaReader snapshot = StorageReader.schemaSnapshot();
			  DropIndex( Label1, PropertyKey );
			  try
			  {
					snapshot.IndexGetState( IndexDescriptor( Label1, PropertyKey ) );
					fail( "Should have thrown exception when asking for state of dropped index." );
			  }
			  catch ( IndexNotFoundKernelException )
			  {
					// Good.
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllConstraints()
		 public virtual void ShouldListAllConstraints()
		 {
			  // Given
			  CreateUniquenessConstraint( Label1, PropertyKey );
			  CreateUniquenessConstraint( Label2, PropertyKey );

			  // When
			  ISet<ConstraintDescriptor> constraints = asSet( StorageReader.constraintsGetAll() );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Set<?> expectedConstraints = asSet(uniqueConstraintDescriptor(label1, propertyKey), uniqueConstraintDescriptor(label2, propertyKey));
			  ISet<object> expectedConstraints = asSet( UniqueConstraintDescriptor( Label1, PropertyKey ), UniqueConstraintDescriptor( Label2, PropertyKey ) );

			  assertEquals( expectedConstraints, constraints );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllConstraintsAtTimeOfSnapshot()
		 public virtual void ShouldListAllConstraintsAtTimeOfSnapshot()
		 {
			  // Given
			  CreateUniquenessConstraint( Label1, PropertyKey );

			  // When
			  StorageSchemaReader snapshot = StorageReader.schemaSnapshot();
			  CreateUniquenessConstraint( Label2, PropertyKey );
			  ISet<ConstraintDescriptor> constraints = asSet( snapshot.ConstraintsGetAll() );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Set<?> expectedConstraints = asSet(uniqueConstraintDescriptor(label1, propertyKey));
			  ISet<object> expectedConstraints = asSet( UniqueConstraintDescriptor( Label1, PropertyKey ) );

			  assertEquals( expectedConstraints, constraints );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllConstraintsForLabel()
		 public virtual void ShouldListAllConstraintsForLabel()
		 {
			  // Given
			  CreateUniquenessConstraint( Label1, PropertyKey );
			  CreateUniquenessConstraint( Label2, PropertyKey );

			  // When
			  ISet<ConstraintDescriptor> constraints = asSet( StorageReader.constraintsGetForLabel( LabelId( Label1 ) ) );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Set<?> expectedConstraints = asSet(uniqueConstraintDescriptor(label1, propertyKey));
			  ISet<object> expectedConstraints = asSet( UniqueConstraintDescriptor( Label1, PropertyKey ) );

			  assertEquals( expectedConstraints, constraints );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllConstraintsForLabelAtTimeOfSnapshot()
		 public virtual void ShouldListAllConstraintsForLabelAtTimeOfSnapshot()
		 {
			  // Given
			  CreateUniquenessConstraint( Label1, PropertyKey );
			  CreateUniquenessConstraint( Label2, PropertyKey );

			  // When
			  StorageSchemaReader snapshot = StorageReader.schemaSnapshot();
			  CreateUniquenessConstraint( Label1, OtherPropertyKey );
			  ISet<ConstraintDescriptor> constraints = asSet( snapshot.ConstraintsGetForLabel( LabelId( Label1 ) ) );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Set<?> expectedConstraints = asSet(uniqueConstraintDescriptor(label1, propertyKey));
			  ISet<object> expectedConstraints = asSet( UniqueConstraintDescriptor( Label1, PropertyKey ) );

			  assertEquals( expectedConstraints, constraints );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllConstraintsForLabelAndProperty()
		 public virtual void ShouldListAllConstraintsForLabelAndProperty()
		 {
			  // Given
			  CreateUniquenessConstraint( Label1, PropertyKey );
			  CreateUniquenessConstraint( Label1, OtherPropertyKey );

			  // When
			  ISet<ConstraintDescriptor> constraints = asSet( StorageReader.constraintsGetForSchema( UniqueConstraintDescriptor( Label1, PropertyKey ).schema() ) );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Set<?> expectedConstraints = asSet(uniqueConstraintDescriptor(label1, propertyKey));
			  ISet<object> expectedConstraints = asSet( UniqueConstraintDescriptor( Label1, PropertyKey ) );

			  assertEquals( expectedConstraints, constraints );
		 }

		 private void CreateIndex( Label label, string propertyKey )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(label).on(propertyKey).create();
					tx.Success();
			  }
		 }

		 private void DropIndex( Label label, string propertyKey )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					IEnumerable<IndexDefinition> indexes = Db.schema().getIndexes(label);
					foreach ( IndexDefinition index in indexes )
					{
						 IEnumerator<string> keys = index.PropertyKeys.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 if ( keys.hasNext() && keys.next().Equals(propertyKey) && !keys.hasNext() )
						 {
							  index.Drop();
						 }
					}
					tx.Success();
			  }
		 }

		 private void CreateUniquenessConstraint( Label label, string propertyKey )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().constraintFor(label).assertPropertyIsUnique(propertyKey).create();
					tx.Success();
			  }
		 }

		 private IndexDescriptor IndexDescriptor( Label label, string propertyKey )
		 {
			  int labelId = labelId( label );
			  int propKeyId = PropertyKeyId( propertyKey );
			  return IndexDescriptorFactory.forSchema( SchemaDescriptorFactory.forLabel( labelId, propKeyId ) );
		 }

		 private ConstraintDescriptor UniqueConstraintDescriptor( Label label, string propertyKey )
		 {
			  int labelId = labelId( label );
			  int propKeyId = PropertyKeyId( propertyKey );
			  return ConstraintDescriptorFactory.uniqueForLabel( labelId, propKeyId );
		 }
	}

}