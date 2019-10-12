using System.Collections.Generic;

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
namespace Org.Neo4j.Graphdb.schema
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using RecordStorageEngine = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using SchemaStore = Org.Neo4j.Kernel.impl.store.SchemaStore;
	using ConstraintRule = Org.Neo4j.Kernel.impl.store.record.ConstraintRule;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Org.Neo4j.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.filter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.single;

	public class DropBrokenUniquenessConstraintIT
	{
		 private readonly Label _label = Label.label( "Label" );
		 private readonly string _key = "key";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.EmbeddedDatabaseRule();
		 public readonly DatabaseRule Db = new EmbeddedDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropUniquenessConstraintWithBackingIndexNotInUse()
		 public virtual void ShouldDropUniquenessConstraintWithBackingIndexNotInUse()
		 {
			  // given
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().constraintFor(_label).assertPropertyIsUnique(_key).create();
					tx.Success();
			  }

			  // when intentionally breaking the schema by setting the backing index rule to unused
			  RecordStorageEngine storageEngine = Db.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) );
			  SchemaStore schemaStore = storageEngine.TestAccessNeoStores().SchemaStore;
			  SchemaRule indexRule = single( filter( rule => rule is StoreIndexDescriptor, schemaStore.LoadAllSchemaRules() ) );
			  SetSchemaRecordNotInUse( schemaStore, indexRule.Id );
			  // At this point the SchemaCache doesn't know about this change so we have to reload it
			  storageEngine.LoadSchemaCache();
			  using ( Transaction tx = Db.beginTx() )
			  {
					single( Db.schema().getConstraints(_label).GetEnumerator() ).drop();
					tx.Success();
			  }

			  // then
			  using ( Transaction ignore = Db.beginTx() )
			  {
					assertFalse( Db.schema().Constraints.GetEnumerator().hasNext() );
					assertFalse( Db.schema().Indexes.GetEnumerator().hasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropUniquenessConstraintWithBackingIndexHavingNoOwner() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropUniquenessConstraintWithBackingIndexHavingNoOwner()
		 {
			  // given
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().constraintFor(_label).assertPropertyIsUnique(_key).create();
					tx.Success();
			  }

			  // when intentionally breaking the schema by setting the backing index rule to unused
			  RecordStorageEngine storageEngine = Db.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) );
			  SchemaStore schemaStore = storageEngine.TestAccessNeoStores().SchemaStore;
			  SchemaRule indexRule = single( filter( rule => rule is StoreIndexDescriptor, schemaStore.LoadAllSchemaRules() ) );
			  SetOwnerNull( schemaStore, ( StoreIndexDescriptor ) indexRule );
			  // At this point the SchemaCache doesn't know about this change so we have to reload it
			  storageEngine.LoadSchemaCache();
			  using ( Transaction tx = Db.beginTx() )
			  {
					single( Db.schema().getConstraints(_label).GetEnumerator() ).drop();
					tx.Success();
			  }

			  // then
			  using ( Transaction ignore = Db.beginTx() )
			  {
					assertFalse( Db.schema().Constraints.GetEnumerator().hasNext() );
					assertFalse( Db.schema().Indexes.GetEnumerator().hasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropUniquenessConstraintWhereConstraintRecordIsMissing() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropUniquenessConstraintWhereConstraintRecordIsMissing()
		 {
			  // given
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().constraintFor(_label).assertPropertyIsUnique(_key).create();
					tx.Success();
			  }

			  // when intentionally breaking the schema by setting the backing index rule to unused
			  RecordStorageEngine storageEngine = Db.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) );
			  SchemaStore schemaStore = storageEngine.TestAccessNeoStores().SchemaStore;
			  SchemaRule indexRule = single( filter( rule => rule is ConstraintRule, schemaStore.LoadAllSchemaRules() ) );
			  SetSchemaRecordNotInUse( schemaStore, indexRule.Id );
			  // At this point the SchemaCache doesn't know about this change so we have to reload it
			  storageEngine.LoadSchemaCache();
			  using ( Transaction tx = Db.beginTx() )
			  {
					// We don't use single() here, because it is okay for the schema cache reload to clean up after us.
					Db.schema().getConstraints(_label).forEach(ConstraintDefinition.drop);
					Db.schema().getIndexes(_label).forEach(IndexDefinition.drop);
					tx.Success();
			  }

			  // then
			  using ( Transaction ignore = Db.beginTx() )
			  {
					assertFalse( Db.schema().Constraints.GetEnumerator().hasNext() );
					assertFalse( Db.schema().Indexes.GetEnumerator().hasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropUniquenessConstraintWhereConstraintRecordIsMissingAndIndexHasNoOwner() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropUniquenessConstraintWhereConstraintRecordIsMissingAndIndexHasNoOwner()
		 {
			  // given
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().constraintFor(_label).assertPropertyIsUnique(_key).create();
					tx.Success();
			  }

			  // when intentionally breaking the schema by setting the backing index rule to unused
			  RecordStorageEngine storageEngine = Db.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) );
			  SchemaStore schemaStore = storageEngine.TestAccessNeoStores().SchemaStore;
			  SchemaRule constraintRule = single( filter( rule => rule is ConstraintRule, schemaStore.LoadAllSchemaRules() ) );
			  SetSchemaRecordNotInUse( schemaStore, constraintRule.Id );
			  SchemaRule indexRule = single( filter( rule => rule is StoreIndexDescriptor, schemaStore.LoadAllSchemaRules() ) );
			  SetOwnerNull( schemaStore, ( StoreIndexDescriptor ) indexRule );
			  // At this point the SchemaCache doesn't know about this change so we have to reload it
			  storageEngine.LoadSchemaCache();
			  using ( Transaction tx = Db.beginTx() )
			  {
					// We don't use single() here, because it is okay for the schema cache reload to clean up after us.
					Db.schema().getConstraints(_label).forEach(ConstraintDefinition.drop);
					Db.schema().getIndexes(_label).forEach(IndexDefinition.drop);
					tx.Success();
			  }

			  // then
			  using ( Transaction ignore = Db.beginTx() )
			  {
					assertFalse( Db.schema().Constraints.GetEnumerator().hasNext() );
					assertFalse( Db.schema().Indexes.GetEnumerator().hasNext() );
			  }
		 }

		 private void SetOwnerNull( SchemaStore schemaStore, StoreIndexDescriptor rule )
		 {
			  rule = rule.WithOwningConstraint( null );
			  IList<DynamicRecord> dynamicRecords = schemaStore.AllocateFrom( rule );
			  foreach ( DynamicRecord record in dynamicRecords )
			  {
					schemaStore.UpdateRecord( record );
			  }
		 }

		 private void SetSchemaRecordNotInUse( SchemaStore schemaStore, long id )
		 {
			  DynamicRecord record = schemaStore.NewRecord();
			  record.Id = id;
			  record.InUse = false;
			  schemaStore.UpdateRecord( record );
		 }
	}

}