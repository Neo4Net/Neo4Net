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
namespace Org.Neo4j.Kernel.Impl.Api.index
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaDescriptorSupplier = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptorSupplier;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using ConstraintDescriptorFactory = Org.Neo4j.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using ConstraintRule = Org.Neo4j.Kernel.impl.store.record.ConstraintRule;
	using EntityType = Org.Neo4j.Storageengine.Api.EntityType;
	using CapableIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.CapableIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.emptyIterableOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.EntityType.NODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.EntityType.RELATIONSHIP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptorFactory.forSchema;

	public class IndexMapTest
	{
		 private static readonly long[] _noEntityToken = new long[] {};
		 private IndexMap _indexMap;

		 private LabelSchemaDescriptor _schema3_4 = SchemaDescriptorFactory.forLabel( 3, 4 );
		 private LabelSchemaDescriptor _schema5_6_7 = SchemaDescriptorFactory.forLabel( 5, 6, 7 );
		 private LabelSchemaDescriptor _schema5_8 = SchemaDescriptorFactory.forLabel( 5, 8 );
		 private SchemaDescriptor _node35_8 = SchemaDescriptorFactory.multiToken( new int[] { 3, 5 }, NODE, 8 );
		 private SchemaDescriptor _rel35_8 = SchemaDescriptorFactory.multiToken( new int[] { 3, 5 }, RELATIONSHIP, 8 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _indexMap = new IndexMap();
			  _indexMap.putIndexProxy( new TestIndexProxy( this, forSchema( _schema3_4 ).withId( 1 ).withoutCapabilities() ) );
			  _indexMap.putIndexProxy( new TestIndexProxy( this, forSchema( _schema5_6_7 ).withId( 2 ).withoutCapabilities() ) );
			  _indexMap.putIndexProxy( new TestIndexProxy( this, forSchema( _schema5_8 ).withId( 3 ).withoutCapabilities() ) );
			  _indexMap.putIndexProxy( new TestIndexProxy( this, forSchema( _node35_8 ).withId( 4 ).withoutCapabilities() ) );
			  _indexMap.putIndexProxy( new TestIndexProxy( this, forSchema( _rel35_8 ).withId( 5 ).withoutCapabilities() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetRelatedIndexForLabel()
		 public virtual void ShouldGetRelatedIndexForLabel()
		 {
			  assertThat( GetRelatedIndexes( EntityTokens( 3 ), _noEntityToken, Properties(), false, NODE ), containsInAnyOrder(_schema3_4, _node35_8) );
		 }

		 private ISet<SchemaDescriptor> GetRelatedIndexes( long[] changedEntityTokens, long[] unchangedEntityTokens, int[] properties, bool propertyListIsComplete, EntityType type )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return _indexMap.getRelatedIndexes( changedEntityTokens, unchangedEntityTokens, properties, propertyListIsComplete, type ).Select( SchemaDescriptorSupplier::schema ).collect( toSet() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetRelatedIndexForProperty()
		 public virtual void ShouldGetRelatedIndexForProperty()
		 {
			  assertThat( GetRelatedIndexes( _noEntityToken, EntityTokens( 3, 4, 5 ), Properties( 4 ), false, NODE ), containsInAnyOrder( _schema3_4 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetRelatedIndexesForLabel()
		 public virtual void ShouldGetRelatedIndexesForLabel()
		 {
			  assertThat( GetRelatedIndexes( EntityTokens( 5 ), EntityTokens( 3, 4 ), Properties(), false, NODE ), containsInAnyOrder(_schema5_6_7, _schema5_8, _node35_8) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetRelatedIndexes()
		 public virtual void ShouldGetRelatedIndexes()
		 {
			  assertThat( GetRelatedIndexes( EntityTokens( 3 ), EntityTokens( 4, 5 ), Properties( 7 ), false, NODE ), containsInAnyOrder( _schema3_4, _schema5_6_7, _node35_8 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetRelatedIndexOnce()
		 public virtual void ShouldGetRelatedIndexOnce()
		 {
			  assertThat( GetRelatedIndexes( EntityTokens( 3 ), _noEntityToken, Properties( 4 ), false, NODE ), containsInAnyOrder( _schema3_4, _node35_8 ) );

			  assertThat( GetRelatedIndexes( _noEntityToken, EntityTokens( 5 ), Properties( 6, 7 ), false, NODE ), containsInAnyOrder( _schema5_6_7 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleUnrelated()
		 public virtual void ShouldHandleUnrelated()
		 {
			  assertThat( GetRelatedIndexes( _noEntityToken, _noEntityToken, Properties(), false, NODE ), emptyIterableOf(typeof(SchemaDescriptor)) );

			  assertTrue( GetRelatedIndexes( EntityTokens( 2 ), _noEntityToken, Properties(), false, NODE ).Count == 0 );

			  assertThat( GetRelatedIndexes( _noEntityToken, EntityTokens( 2 ), Properties( 1 ), false, NODE ), emptyIterableOf( typeof( SchemaDescriptor ) ) );

			  assertTrue( GetRelatedIndexes( EntityTokens( 2 ), EntityTokens( 2 ), Properties( 1 ), false, NODE ).Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetMultiLabelForAnyOfTheLabels()
		 public virtual void ShouldGetMultiLabelForAnyOfTheLabels()
		 {
			  assertThat( GetRelatedIndexes( EntityTokens( 3 ), _noEntityToken, Properties(), false, NODE ), containsInAnyOrder(_schema3_4, _node35_8) );

			  assertThat( GetRelatedIndexes( EntityTokens( 5 ), _noEntityToken, Properties(), false, NODE ), containsInAnyOrder(_schema5_8, _schema5_6_7, _node35_8) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyGetRelIndexesForRelUpdates()
		 public virtual void ShouldOnlyGetRelIndexesForRelUpdates()
		 {
			  assertThat( GetRelatedIndexes( EntityTokens( 3 ), _noEntityToken, Properties(), false, RELATIONSHIP ), containsInAnyOrder(_rel35_8) );

			  assertThat( GetRelatedIndexes( EntityTokens( 5 ), _noEntityToken, Properties(), false, RELATIONSHIP ), containsInAnyOrder(_rel35_8) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removalsShouldOnlyRemoveCorrectProxy()
		 public virtual void RemovalsShouldOnlyRemoveCorrectProxy()
		 {
			  _indexMap.removeIndexProxy( 4 );
			  assertThat( GetRelatedIndexes( EntityTokens( 3 ), _noEntityToken, Properties(), false, NODE ), containsInAnyOrder(_schema3_4) );
			  assertThat( GetRelatedIndexes( EntityTokens( 3 ), _noEntityToken, Properties(), false, RELATIONSHIP ), containsInAnyOrder(_rel35_8) );

			  _indexMap.removeIndexProxy( 7 );
			  assertThat( GetRelatedIndexes( EntityTokens( 5 ), _noEntityToken, Properties(), false, NODE ), containsInAnyOrder(_schema5_8, _schema5_6_7) );
			  assertThat( GetRelatedIndexes( EntityTokens( 5 ), _noEntityToken, Properties(), false, RELATIONSHIP ), containsInAnyOrder(_rel35_8) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetRelatedNodeConstraints()
		 public virtual void ShouldGetRelatedNodeConstraints()
		 {
			  // given
			  ConstraintRule constraint1 = ConstraintRule.constraintRule( 1L, ConstraintDescriptorFactory.uniqueForLabel( 1, 5, 6 ), null );
			  ConstraintRule constraint2 = ConstraintRule.constraintRule( 2L, ConstraintDescriptorFactory.uniqueForLabel( 1, 5 ), null );
			  ConstraintRule constraint3 = ConstraintRule.constraintRule( 3L, ConstraintDescriptorFactory.uniqueForLabel( 2, 5 ), null );
			  _indexMap.putUniquenessConstraint( constraint1 );
			  _indexMap.putUniquenessConstraint( constraint2 );
			  _indexMap.putUniquenessConstraint( constraint3 );

			  // when/then
			  assertEquals( asSet( constraint2.ConstraintDescriptor ), _indexMap.getRelatedConstraints( EntityTokens( 1 ), EntityTokens(), Properties(5), true, NODE ) );
			  assertEquals( asSet( constraint1.ConstraintDescriptor, constraint2.ConstraintDescriptor ), _indexMap.getRelatedConstraints( EntityTokens( 1 ), EntityTokens(), Properties(5), false, NODE ) );
			  assertEquals( asSet( constraint1.ConstraintDescriptor, constraint2.ConstraintDescriptor ), _indexMap.getRelatedConstraints( EntityTokens( 1 ), EntityTokens(), Properties(5, 6), true, NODE ) );
			  assertEquals( asSet( constraint1.ConstraintDescriptor, constraint2.ConstraintDescriptor ), _indexMap.getRelatedConstraints( EntityTokens(), EntityTokens(1), Properties(5), false, NODE ) );
			  assertEquals( asSet( constraint1.ConstraintDescriptor, constraint2.ConstraintDescriptor, constraint3.ConstraintDescriptor ), _indexMap.getRelatedConstraints( EntityTokens( 1, 2 ), EntityTokens(), Properties(), false, NODE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveNodeConstraints()
		 public virtual void ShouldRemoveNodeConstraints()
		 {
			  // given
			  ConstraintRule constraint1 = ConstraintRule.constraintRule( 1L, ConstraintDescriptorFactory.uniqueForLabel( 1, 5, 6 ), null );
			  ConstraintRule constraint2 = ConstraintRule.constraintRule( 2L, ConstraintDescriptorFactory.uniqueForLabel( 1, 5 ), null );
			  ConstraintRule constraint3 = ConstraintRule.constraintRule( 3L, ConstraintDescriptorFactory.uniqueForLabel( 2, 5 ), null );
			  _indexMap.putUniquenessConstraint( constraint1 );
			  _indexMap.putUniquenessConstraint( constraint2 );
			  _indexMap.putUniquenessConstraint( constraint3 );
			  assertEquals( asSet( constraint2.ConstraintDescriptor ), _indexMap.getRelatedConstraints( EntityTokens( 1 ), EntityTokens(), Properties(5), true, NODE ) );

			  // and when
			  _indexMap.removeUniquenessConstraint( constraint1.Id );
			  _indexMap.removeUniquenessConstraint( constraint2.Id );
			  _indexMap.removeUniquenessConstraint( constraint3.Id );

			  // then
			  assertTrue( _indexMap.getRelatedConstraints( EntityTokens( 1 ), EntityTokens(), Properties(5), true, NODE ).Count == 0 );
		 }

		 // HELPERS

		 private long[] EntityTokens( params long[] entityTokenIds )
		 {
			  return entityTokenIds;
		 }

		 private int[] Properties( params int[] propertyIds )
		 {
			  return propertyIds;
		 }

		 private class TestIndexProxy : IndexProxyAdapter
		 {
			 private readonly IndexMapTest _outerInstance;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly CapableIndexDescriptor DescriptorConflict;

			  internal TestIndexProxy( IndexMapTest outerInstance, CapableIndexDescriptor descriptor )
			  {
				  this._outerInstance = outerInstance;
					this.DescriptorConflict = descriptor;
			  }

			  public override CapableIndexDescriptor Descriptor
			  {
				  get
				  {
						return DescriptorConflict;
				  }
			  }
		 }
	}

}