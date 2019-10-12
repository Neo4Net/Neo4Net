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
namespace Neo4Net.Kernel.Impl.Api.store
{
	using Test = org.junit.Test;


	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using Iterators = Neo4Net.Helpers.Collection.Iterators;
	using LabelSchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using MultiTokenSchemaDescriptor = Neo4Net.Kernel.api.schema.MultiTokenSchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using IndexProviderMap = Neo4Net.Kernel.Impl.Api.index.IndexProviderMap;
	using StandardConstraintSemantics = Neo4Net.Kernel.impl.constraints.StandardConstraintSemantics;
	using ConstraintRule = Neo4Net.Kernel.impl.store.record.ConstraintRule;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;
	using CapableIndexDescriptor = Neo4Net.Storageengine.Api.schema.CapableIndexDescriptor;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexDescriptorFactory = Neo4Net.Storageengine.Api.schema.IndexDescriptorFactory;
	using SchemaRule = Neo4Net.Storageengine.Api.schema.SchemaRule;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using Race = Neo4Net.Test.Race;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.constraints.ConstraintDescriptorFactory.uniqueForLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.ConstraintRule.constraintRule;

	public class SchemaCacheTest
	{
		private bool InstanceFieldsInitialized = false;

		public SchemaCacheTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_hans = NewIndexRule( 1, 0, 5 );
			_witch = NodePropertyExistenceConstraintRule( 2, 3, 6 );
			_gretel = NewIndexRule( 3, 0, 7 );
			_robot = RelPropertyExistenceConstraintRule( 7L, 8, 9 );
		}

		 private SchemaRule _hans;
		 private SchemaRule _witch;
		 private SchemaRule _gretel;
		 private ConstraintRule _robot;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void should_construct_schema_cache()
		 public virtual void ShouldConstructSchemaCache()
		 {
			  // GIVEN
			  ICollection<SchemaRule> rules = asList( _hans, _witch, _gretel, _robot );
			  SchemaCache cache = new SchemaCache( new ConstraintSemantics(), rules, IndexProviderMap.EMPTY );

			  // THEN
			  assertEquals( asSet( _hans, _gretel ), Iterables.asSet( cache.IndexDescriptors() ) );
			  assertEquals( asSet( _witch, _robot ), Iterables.asSet( cache.ConstraintRules() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addRemoveIndexes()
		 public virtual void AddRemoveIndexes()
		 {
			  ICollection<SchemaRule> rules = asList( _hans, _witch, _gretel, _robot );
			  SchemaCache cache = new SchemaCache( new ConstraintSemantics(), rules, IndexProviderMap.EMPTY );

			  StoreIndexDescriptor rule1 = NewIndexRule( 10, 11, 12 );
			  StoreIndexDescriptor rule2 = NewIndexRule( 13, 14, 15 );
			  cache.AddSchemaRule( rule1 );
			  cache.AddSchemaRule( rule2 );

			  cache.RemoveSchemaRule( _hans.Id );
			  cache.RemoveSchemaRule( _witch.Id );

			  assertEquals( asSet( _gretel, rule1, rule2 ), Iterables.asSet( cache.IndexDescriptors() ) );
			  assertEquals( asSet( _robot ), Iterables.asSet( cache.ConstraintRules() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addSchemaRules()
		 public virtual void AddSchemaRules()
		 {
			  // GIVEN
			  SchemaCache cache = NewSchemaCache();

			  // WHEN
			  cache.AddSchemaRule( _hans );
			  cache.AddSchemaRule( _gretel );
			  cache.AddSchemaRule( _witch );
			  cache.AddSchemaRule( _robot );

			  // THEN
			  assertEquals( asSet( _hans, _gretel ), Iterables.asSet( cache.IndexDescriptors() ) );
			  assertEquals( asSet( _witch, _robot ), Iterables.asSet( cache.ConstraintRules() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void should_list_constraints()
		 public virtual void ShouldListConstraints()
		 {
			  // GIVEN
			  SchemaCache cache = NewSchemaCache();

			  // WHEN
			  cache.AddSchemaRule( UniquenessConstraintRule( 0L, 1, 2, 133L ) );
			  cache.AddSchemaRule( UniquenessConstraintRule( 1L, 3, 4, 133L ) );
			  cache.AddSchemaRule( RelPropertyExistenceConstraintRule( 2L, 5, 6 ) );
			  cache.AddSchemaRule( NodePropertyExistenceConstraintRule( 3L, 7, 8 ) );

			  // THEN
			  ConstraintDescriptor unique1 = uniqueForLabel( 1, 2 );
			  ConstraintDescriptor unique2 = uniqueForLabel( 3, 4 );
			  ConstraintDescriptor existsRel = ConstraintDescriptorFactory.existsForRelType( 5, 6 );
			  ConstraintDescriptor existsNode = ConstraintDescriptorFactory.existsForLabel( 7, 8 );

			  assertEquals( asSet( unique1, unique2, existsRel, existsNode ), asSet( cache.Constraints() ) );

			  assertEquals( asSet( unique1 ), asSet( cache.ConstraintsForLabel( 1 ) ) );

			  assertEquals( asSet( unique1 ), asSet( cache.ConstraintsForSchema( unique1.Schema() ) ) );

			  assertEquals( asSet(), asSet(cache.ConstraintsForSchema(forLabel(1, 3))) );

			  assertEquals( asSet( existsRel ), asSet( cache.ConstraintsForRelationshipType( 5 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void should_remove_constraints()
		 public virtual void ShouldRemoveConstraints()
		 {
			  // GIVEN
			  SchemaCache cache = NewSchemaCache();

			  cache.AddSchemaRule( UniquenessConstraintRule( 0L, 1, 2, 133L ) );
			  cache.AddSchemaRule( UniquenessConstraintRule( 1L, 3, 4, 133L ) );

			  // WHEN
			  cache.RemoveSchemaRule( 0L );

			  // THEN
			  ConstraintDescriptor dropped = uniqueForLabel( 1, 1 );
			  ConstraintDescriptor unique = uniqueForLabel( 3, 4 );
			  assertEquals( asSet( unique ), asSet( cache.Constraints() ) );

			  assertEquals( asSet(), asSet(cache.ConstraintsForLabel(1)) );

			  assertEquals( asSet(), asSet(cache.ConstraintsForSchema(dropped.Schema())) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void adding_constraints_should_be_idempotent()
		 public virtual void AddingConstraintsShouldBeIdempotent()
		 {
			  // given
			  SchemaCache cache = NewSchemaCache();

			  cache.AddSchemaRule( UniquenessConstraintRule( 0L, 1, 2, 133L ) );

			  // when
			  cache.AddSchemaRule( UniquenessConstraintRule( 0L, 1, 2, 133L ) );

			  // then
			  assertEquals( asList( uniqueForLabel( 1, 2 ) ), Iterators.asList( cache.Constraints() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResolveIndexDescriptor()
		 public virtual void ShouldResolveIndexDescriptor()
		 {
			  // Given
			  SchemaCache cache = NewSchemaCache();

			  cache.AddSchemaRule( NewIndexRule( 1L, 1, 2 ) );
			  cache.AddSchemaRule( NewIndexRule( 2L, 1, 3 ) );
			  cache.AddSchemaRule( NewIndexRule( 3L, 2, 2 ) );

			  // When
			  LabelSchemaDescriptor schema = forLabel( 1, 3 );
			  IndexDescriptor descriptor = cache.IndexDescriptor( schema );

			  // Then
			  assertThat( descriptor.Schema(), equalTo(schema) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void schemaCacheSnapshotsShouldBeReadOnly()
		 public virtual void SchemaCacheSnapshotsShouldBeReadOnly()
		 {
			  // Given
			  SchemaCache cache = NewSchemaCache();

			  cache.AddSchemaRule( NewIndexRule( 1L, 1, 2 ) );
			  cache.AddSchemaRule( NewIndexRule( 2L, 2, 3 ) );

			  SchemaCache snapshot = cache.Snapshot();

			  cache.AddSchemaRule( NewIndexRule( 3L, 1, 2 ) );

			  // When
			  ISet<CapableIndexDescriptor> indexes = asSet( snapshot.IndexDescriptorsForLabel( 1 ) );

			  // Then
			  ISet<StoreIndexDescriptor> expected = asSet( NewIndexRule( 1L, 1, 2 ) );
			  assertEquals( expected, indexes );

			  try
			  {
					snapshot.AddSchemaRule( NewIndexRule( 3L, 1, 2 ) );
					fail( "SchemaCache snapshots should not permit mutation." );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// Good.
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNullWhenNoIndexExists()
		 public virtual void ShouldReturnNullWhenNoIndexExists()
		 {
			  // Given
			  SchemaCache schemaCache = NewSchemaCache();

			  // When
			  IndexDescriptor schemaIndexDescriptor = schemaCache.IndexDescriptor( forLabel( 1, 1 ) );

			  // Then
			  assertNull( schemaIndexDescriptor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListConstraintsForLabel()
		 public virtual void ShouldListConstraintsForLabel()
		 {
			  // Given
			  ConstraintRule rule1 = UniquenessConstraintRule( 0, 1, 1, 0 );
			  ConstraintRule rule2 = UniquenessConstraintRule( 1, 2, 1, 0 );
			  ConstraintRule rule3 = NodePropertyExistenceConstraintRule( 2, 1, 2 );

			  SchemaCache cache = NewSchemaCache();
			  cache.AddSchemaRule( rule1 );
			  cache.AddSchemaRule( rule2 );
			  cache.AddSchemaRule( rule3 );

			  // When
			  ISet<ConstraintDescriptor> listed = asSet( cache.ConstraintsForLabel( 1 ) );

			  // Then
			  ISet<ConstraintDescriptor> expected = asSet( rule1.ConstraintDescriptor, rule3.ConstraintDescriptor );
			  assertEquals( expected, listed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListConstraintsForSchema()
		 public virtual void ShouldListConstraintsForSchema()
		 {
			  // Given
			  ConstraintRule rule1 = UniquenessConstraintRule( 0, 1, 1, 0 );
			  ConstraintRule rule2 = UniquenessConstraintRule( 1, 2, 1, 0 );
			  ConstraintRule rule3 = NodePropertyExistenceConstraintRule( 2, 1, 2 );

			  SchemaCache cache = NewSchemaCache();
			  cache.AddSchemaRule( rule1 );
			  cache.AddSchemaRule( rule2 );
			  cache.AddSchemaRule( rule3 );

			  // When
			  ISet<ConstraintDescriptor> listed = asSet( cache.ConstraintsForSchema( rule3.Schema() ) );

			  // Then
			  assertEquals( singleton( rule3.ConstraintDescriptor ), listed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListConstraintsForRelationshipType()
		 public virtual void ShouldListConstraintsForRelationshipType()
		 {
			  // Given
			  ConstraintRule rule1 = RelPropertyExistenceConstraintRule( 0, 1, 1 );
			  ConstraintRule rule2 = RelPropertyExistenceConstraintRule( 0, 2, 1 );
			  ConstraintRule rule3 = RelPropertyExistenceConstraintRule( 0, 1, 2 );

			  SchemaCache cache = NewSchemaCache();
			  cache.AddSchemaRule( rule1 );
			  cache.AddSchemaRule( rule2 );
			  cache.AddSchemaRule( rule3 );

			  // When
			  ISet<ConstraintDescriptor> listed = asSet( cache.ConstraintsForRelationshipType( 1 ) );

			  // Then
			  ISet<ConstraintDescriptor> expected = asSet( rule1.ConstraintDescriptor, rule3.ConstraintDescriptor );
			  assertEquals( expected, listed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void concurrentSchemaRuleAdd() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConcurrentSchemaRuleAdd()
		 {
			  SchemaCache cache = NewSchemaCache();
			  Race race = new Race();
			  int indexNumber = 10;
			  for ( int i = 0; i < indexNumber; i++ )
			  {
					int id = i;
					race.AddContestant( () => cache.addSchemaRule(NewIndexRule(id, id, id)) );
			  }
			  race.Go();

			  assertEquals( indexNumber, Iterables.count( cache.IndexDescriptors() ) );
			  for ( int labelId = 0; labelId < indexNumber; labelId++ )
			  {
					assertEquals( 1, Iterators.count( cache.IndexDescriptorsForLabel( labelId ) ) );
			  }
			  for ( int propertyId = 0; propertyId < indexNumber; propertyId++ )
			  {
					assertEquals( 1, Iterators.count( cache.IndexesByProperty( propertyId ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void concurrentSchemaRuleRemove() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConcurrentSchemaRuleRemove()
		 {
			  SchemaCache cache = NewSchemaCache();
			  int indexNumber = 20;
			  for ( int i = 0; i < indexNumber; i++ )
			  {
					cache.AddSchemaRule( NewIndexRule( i, i, i ) );
			  }
			  Race race = new Race();
			  int numberOfDeletions = 10;
			  for ( int i = 0; i < numberOfDeletions; i++ )
			  {
					int indexId = i;
					race.AddContestant( () => cache.removeSchemaRule(indexId) );
			  }
			  race.Go();

			  assertEquals( indexNumber - numberOfDeletions, Iterables.count( cache.IndexDescriptors() ) );
			  for ( int labelId = numberOfDeletions; labelId < indexNumber; labelId++ )
			  {
					assertEquals( 1, Iterators.count( cache.IndexDescriptorsForLabel( labelId ) ) );
			  }
			  for ( int propertyId = numberOfDeletions; propertyId < indexNumber; propertyId++ )
			  {
					assertEquals( 1, Iterators.count( cache.IndexesByProperty( propertyId ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removeSchemaWithRepeatedLabel()
		 public virtual void RemoveSchemaWithRepeatedLabel()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SchemaCache cache = newSchemaCache();
			  SchemaCache cache = NewSchemaCache();

			  const int id = 1;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int[] repeatedLabels = {0, 1, 0};
			  int[] repeatedLabels = new int[] { 0, 1, 0 };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.api.schema.MultiTokenSchemaDescriptor schema = org.neo4j.kernel.api.schema.SchemaDescriptorFactory.multiToken(repeatedLabels, org.neo4j.storageengine.api.EntityType.NODE, 1);
			  MultiTokenSchemaDescriptor schema = SchemaDescriptorFactory.multiToken( repeatedLabels, EntityType.NODE, 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.storageengine.api.schema.StoreIndexDescriptor storeIndexDescriptor = org.neo4j.storageengine.api.schema.IndexDescriptorFactory.forSchema(schema).withId(id);
			  StoreIndexDescriptor storeIndexDescriptor = IndexDescriptorFactory.forSchema( schema ).withId( id );
			  cache.AddSchemaRule( storeIndexDescriptor );
			  cache.RemoveSchemaRule( id );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removeSchemaWithRepeatedRelType()
		 public virtual void RemoveSchemaWithRepeatedRelType()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SchemaCache cache = newSchemaCache();
			  SchemaCache cache = NewSchemaCache();

			  const int id = 1;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int[] repeatedRelTypes = {0, 1, 0};
			  int[] repeatedRelTypes = new int[] { 0, 1, 0 };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.api.schema.MultiTokenSchemaDescriptor schema = org.neo4j.kernel.api.schema.SchemaDescriptorFactory.multiToken(repeatedRelTypes, org.neo4j.storageengine.api.EntityType.RELATIONSHIP, 1);
			  MultiTokenSchemaDescriptor schema = SchemaDescriptorFactory.multiToken( repeatedRelTypes, EntityType.RELATIONSHIP, 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.storageengine.api.schema.StoreIndexDescriptor storeIndexDescriptor = org.neo4j.storageengine.api.schema.IndexDescriptorFactory.forSchema(schema).withId(id);
			  StoreIndexDescriptor storeIndexDescriptor = IndexDescriptorFactory.forSchema( schema ).withId( id );
			  cache.AddSchemaRule( storeIndexDescriptor );
			  cache.RemoveSchemaRule( id );
		 }

		 private StoreIndexDescriptor NewIndexRule( long id, int label, int propertyKey )
		 {
			  return TestIndexDescriptorFactory.forLabel( label, propertyKey ).withId( id );
		 }

		 private ConstraintRule NodePropertyExistenceConstraintRule( long ruleId, int labelId, int propertyId )
		 {
			  return constraintRule( ruleId, ConstraintDescriptorFactory.existsForLabel( labelId, propertyId ) );
		 }

		 private ConstraintRule RelPropertyExistenceConstraintRule( long ruleId, int relTypeId, int propertyId )
		 {
			  return constraintRule( ruleId, ConstraintDescriptorFactory.existsForRelType( relTypeId, propertyId ) );
		 }

		 private ConstraintRule UniquenessConstraintRule( long ruleId, int labelId, int propertyId, long indexId )
		 {
			  return constraintRule( ruleId, uniqueForLabel( labelId, propertyId ), indexId );
		 }

		 private static SchemaCache NewSchemaCache( params SchemaRule[] rules )
		 {
			  return new SchemaCache( new ConstraintSemantics(), (rules == null || rules.Length == 0) ? Collections.emptyList() : Arrays.asList(rules), IndexProviderMap.EMPTY );
		 }

		 private class ConstraintSemantics : StandardConstraintSemantics
		 {
			  protected internal override ConstraintDescriptor ReadNonStandardConstraint( ConstraintRule rule, string errorMessage )
			  {
					if ( !rule.ConstraintDescriptor.enforcesPropertyExistence() )
					{
						 throw new System.InvalidOperationException( "Unsupported constraint type: " + rule );
					}
					return rule.ConstraintDescriptor;
			  }
		 }
	}

}