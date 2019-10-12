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
namespace Org.Neo4j.Kernel.Api.Index
{
	using MutableIntIterator = org.eclipse.collections.api.iterator.MutableIntIterator;
	using Test = org.junit.Test;


	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using PropertyKeyValue = Org.Neo4j.Kernel.api.properties.PropertyKeyValue;
	using MultiTokenSchemaDescriptor = Org.Neo4j.Kernel.api.schema.MultiTokenSchemaDescriptor;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using EntityUpdates = Org.Neo4j.Kernel.Impl.Api.index.EntityUpdates;
	using PropertyLoader = Org.Neo4j.Kernel.Impl.Api.index.PropertyLoader;
	using EntityType = Org.Neo4j.Storageengine.Api.EntityType;
	using StorageProperty = Org.Neo4j.Storageengine.Api.StorageProperty;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.emptyIterable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class EntityUpdatesTest
	public class EntityUpdatesTest
	{
		 private const long NODE_ID = 0;
		 private const int LABEL_ID1 = 0;
		 private const int LABEL_ID2 = 1;
		 private const int UNUSED_LABEL_ID = 2;
		 private const int PROPERTY_KEY_ID1 = 0;
		 private const int PROPERTY_KEY_ID2 = 1;
		 private const int PROPERTY_KEY_ID3 = 2;
		 private static readonly long[] _label = new long[]{ LABEL_ID1 };
		 private static readonly long[] _allLabels = new long[]{ LABEL_ID1, LABEL_ID2 };
		 private static readonly long[] _empty = new long[]{};

		 private static readonly LabelSchemaDescriptor _index1 = SchemaDescriptorFactory.forLabel( LABEL_ID1, PROPERTY_KEY_ID1 );
		 private static readonly LabelSchemaDescriptor _index2 = SchemaDescriptorFactory.forLabel( LABEL_ID1, PROPERTY_KEY_ID2 );
		 private static readonly LabelSchemaDescriptor _index3 = SchemaDescriptorFactory.forLabel( LABEL_ID1, PROPERTY_KEY_ID3 );
		 private static readonly LabelSchemaDescriptor _index123 = SchemaDescriptorFactory.forLabel( LABEL_ID1, PROPERTY_KEY_ID1, PROPERTY_KEY_ID2, PROPERTY_KEY_ID3 );
		 private static readonly IList<LabelSchemaDescriptor> _indexes = Arrays.asList( _index1, _index2, _index3, _index123 );
		 private static readonly MultiTokenSchemaDescriptor _nonSchemaIndex = SchemaDescriptorFactory.multiToken( new int[]{ LABEL_ID1, LABEL_ID2 }, EntityType.NODE, PROPERTY_KEY_ID1, PROPERTY_KEY_ID2, PROPERTY_KEY_ID3 );

		 private static readonly StorageProperty _property1 = new PropertyKeyValue( PROPERTY_KEY_ID1, Values.of( "Neo" ) );
		 private static readonly StorageProperty _property2 = new PropertyKeyValue( PROPERTY_KEY_ID2, Values.of( 100L ) );
		 private static readonly StorageProperty _property3 = new PropertyKeyValue( PROPERTY_KEY_ID3, Values.pointValue( CoordinateReferenceSystem.WGS84, 12.3, 45.6 ) );
		 private static readonly Value[] _values123 = new Value[]{ _property1.value(), _property2.value(), _property3.value() };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGenerateUpdatesForEmptyNodeUpdates()
		 public virtual void ShouldNotGenerateUpdatesForEmptyNodeUpdates()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( _indexes, AssertNoLoading(), EntityType.NODE ), emptyIterable() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGenerateUpdateForMultipleExistingPropertiesAndLabels()
		 public virtual void ShouldNotGenerateUpdateForMultipleExistingPropertiesAndLabels()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).existing( PROPERTY_KEY_ID1, Values.of( "Neo" ) ).existing( PROPERTY_KEY_ID2, Values.of( 100L ) ).existing( PROPERTY_KEY_ID3, Values.pointValue( CoordinateReferenceSystem.WGS84, 12.3, 45.6 ) ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( _indexes, AssertNoLoading(), EntityType.NODE ), emptyIterable() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGenerateUpdatesForLabelAdditionWithNoProperties()
		 public virtual void ShouldNotGenerateUpdatesForLabelAdditionWithNoProperties()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _empty ).withTokensAfter( _label ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( _indexes, PropertyLoader(), EntityType.NODE ), emptyIterable() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateUpdateForLabelAdditionWithExistingProperty()
		 public virtual void ShouldGenerateUpdateForLabelAdditionWithExistingProperty()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _empty ).withTokensAfter( _label ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( _indexes, PropertyLoader( _property1 ), EntityType.NODE ), containsInAnyOrder( IndexEntryUpdate.Add( NODE_ID, _index1, _property1.value() ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateUpdatesForLabelAdditionWithExistingProperties()
		 public virtual void ShouldGenerateUpdatesForLabelAdditionWithExistingProperties()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _empty ).withTokensAfter( _label ).existing( PROPERTY_KEY_ID1, Values.of( "Neo" ) ).existing( PROPERTY_KEY_ID2, Values.of( 100L ) ).existing( PROPERTY_KEY_ID3, Values.pointValue( CoordinateReferenceSystem.WGS84, 12.3, 45.6 ) ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( _indexes, PropertyLoader( _property1, _property2, _property3 ), EntityType.NODE ), containsInAnyOrder( IndexEntryUpdate.Add( NODE_ID, _index1, _property1.value() ), IndexEntryUpdate.Add(NODE_ID, _index2, _property2.value()), IndexEntryUpdate.Add(NODE_ID, _index3, _property3.value()), IndexEntryUpdate.Add(NODE_ID, _index123, _values123) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGenerateUpdateForPartialCompositeSchemaIndexUpdate()
		 public virtual void ShouldNotGenerateUpdateForPartialCompositeSchemaIndexUpdate()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).withTokensAfter( _label ).added( PROPERTY_KEY_ID1, Values.of( "Neo" ) ).added( PROPERTY_KEY_ID3, Values.pointValue( CoordinateReferenceSystem.WGS84, 12.3, 45.6 ) ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( singleton( _index123 ), PropertyLoader(), EntityType.NODE ), emptyIterable() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateUpdateForWhenCompletingCompositeSchemaIndexUpdate()
		 public virtual void ShouldGenerateUpdateForWhenCompletingCompositeSchemaIndexUpdate()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).withTokensAfter( _label ).added( PROPERTY_KEY_ID1, Values.of( "Neo" ) ).added( PROPERTY_KEY_ID3, Values.pointValue( CoordinateReferenceSystem.WGS84, 12.3, 45.6 ) ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( singleton( _index123 ), PropertyLoader( _property2 ), EntityType.NODE ), containsInAnyOrder( IndexEntryUpdate.Add( NODE_ID, _index123, _values123 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGenerateUpdatesForLabelRemovalWithNoProperties()
		 public virtual void ShouldNotGenerateUpdatesForLabelRemovalWithNoProperties()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).withTokensAfter( _empty ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( _indexes, PropertyLoader(), EntityType.NODE ), emptyIterable() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateUpdateForLabelRemovalWithExistingProperty()
		 public virtual void ShouldGenerateUpdateForLabelRemovalWithExistingProperty()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).withTokensAfter( _empty ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( _indexes, PropertyLoader( _property1 ), EntityType.NODE ), containsInAnyOrder( IndexEntryUpdate.Remove( NODE_ID, _index1, _property1.value() ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateUpdatesForLabelRemovalWithExistingProperties()
		 public virtual void ShouldGenerateUpdatesForLabelRemovalWithExistingProperties()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).withTokensAfter( _empty ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( _indexes, PropertyLoader( _property1, _property2, _property3 ), EntityType.NODE ), containsInAnyOrder( IndexEntryUpdate.Remove( NODE_ID, _index1, _property1.value() ), IndexEntryUpdate.Remove(NODE_ID, _index2, _property2.value()), IndexEntryUpdate.Remove(NODE_ID, _index3, _property3.value()), IndexEntryUpdate.Remove(NODE_ID, _index123, _values123) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGenerateUpdatesForPropertyAdditionWithNoLabels()
		 public virtual void ShouldNotGenerateUpdatesForPropertyAdditionWithNoLabels()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).added( _property1.propertyKeyId(), _property1.value() ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( _indexes, AssertNoLoading(), EntityType.NODE ), emptyIterable() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateUpdatesForSinglePropertyAdditionWithLabels()
		 public virtual void ShouldGenerateUpdatesForSinglePropertyAdditionWithLabels()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).added( _property1.propertyKeyId(), _property1.value() ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( _indexes, PropertyLoader(), EntityType.NODE ), containsInAnyOrder(IndexEntryUpdate.Add(NODE_ID, _index1, _property1.value())) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateUpdatesForMultiplePropertyAdditionWithLabels()
		 public virtual void ShouldGenerateUpdatesForMultiplePropertyAdditionWithLabels()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).added( _property1.propertyKeyId(), _property1.value() ).added(_property2.propertyKeyId(), _property2.value()).added(_property3.propertyKeyId(), _property3.value()).build();

			  // Then
			  assertThat( updates.ForIndexKeys( _indexes, PropertyLoader( _property1, _property2, _property3 ), EntityType.NODE ), containsInAnyOrder( IndexEntryUpdate.Add( NODE_ID, _index1, _property1.value() ), IndexEntryUpdate.Add(NODE_ID, _index2, _property2.value()), IndexEntryUpdate.Add(NODE_ID, _index3, _property3.value()), IndexEntryUpdate.Add(NODE_ID, _index123, _values123) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGenerateUpdatesForLabelAddAndPropertyRemove()
		 public virtual void ShouldNotGenerateUpdatesForLabelAddAndPropertyRemove()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _empty ).withTokensAfter( _label ).removed( _property1.propertyKeyId(), _property1.value() ).removed(_property2.propertyKeyId(), _property2.value()).removed(_property3.propertyKeyId(), _property3.value()).build();

			  // Then
			  assertThat( updates.ForIndexKeys( _indexes, AssertNoLoading(), EntityType.NODE ), emptyIterable() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGenerateUpdatesForLabelRemoveAndPropertyAdd()
		 public virtual void ShouldNotGenerateUpdatesForLabelRemoveAndPropertyAdd()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).withTokensAfter( _empty ).added( _property1.propertyKeyId(), _property1.value() ).added(_property2.propertyKeyId(), _property2.value()).added(_property3.propertyKeyId(), _property3.value()).build();

			  // Then
			  assertThat( updates.ForIndexKeys( _indexes, AssertNoLoading(), EntityType.NODE ), emptyIterable() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLoadPropertyForLabelsAndNoPropertyChanges()
		 public virtual void ShouldNotLoadPropertyForLabelsAndNoPropertyChanges()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( singleton( _index1 ), AssertNoLoading(), EntityType.NODE ), emptyIterable() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLoadPropertyForNoLabelsAndButPropertyAddition()
		 public virtual void ShouldNotLoadPropertyForNoLabelsAndButPropertyAddition()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _empty ).added( _property1.propertyKeyId(), _property1.value() ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( singleton( _index1 ), AssertNoLoading(), EntityType.NODE ), emptyIterable() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateUpdateForPartialNonSchemaIndexUpdate()
		 public virtual void ShouldGenerateUpdateForPartialNonSchemaIndexUpdate()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).withTokensAfter( _label ).added( PROPERTY_KEY_ID1, Values.of( "Neo" ) ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( singleton( _nonSchemaIndex ), PropertyLoader(), EntityType.NODE ), containsInAnyOrder(IndexEntryUpdate.Add(NODE_ID, _nonSchemaIndex, _property1.value(), null, null)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateUpdateForFullNonSchemaIndexUpdate()
		 public virtual void ShouldGenerateUpdateForFullNonSchemaIndexUpdate()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).withTokensAfter( _label ).added( _property1.propertyKeyId(), _property1.value() ).added(_property2.propertyKeyId(), _property2.value()).added(_property3.propertyKeyId(), _property3.value()).build();

			  // Then
			  assertThat( updates.ForIndexKeys( singleton( _nonSchemaIndex ), PropertyLoader(), EntityType.NODE ), containsInAnyOrder(IndexEntryUpdate.Add(NODE_ID, _nonSchemaIndex, _values123)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateUpdateForSingleChangeNonSchemaIndex()
		 public virtual void ShouldGenerateUpdateForSingleChangeNonSchemaIndex()
		 {
			  // When
			  Value newValue2 = Values.of( 10L );
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).withTokensAfter( _label ).changed( _property2.propertyKeyId(), _property2.value(), newValue2 ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( singleton( _nonSchemaIndex ), PropertyLoader( _property1, _property2, _property3 ), EntityType.NODE ), containsInAnyOrder( IndexEntryUpdate.Change( NODE_ID, _nonSchemaIndex, _values123, new Value[]{ _property1.value(), newValue2, _property3.value() } ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateUpdateForAllChangedNonSchemaIndex()
		 public virtual void ShouldGenerateUpdateForAllChangedNonSchemaIndex()
		 {
			  // When
			  Value newValue1 = Values.of( "Nio" );
			  Value newValue2 = Values.of( 10L );
			  Value newValue3 = Values.pointValue( CoordinateReferenceSystem.WGS84, 32.3, 15.6 );
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).withTokensAfter( _label ).changed( _property1.propertyKeyId(), _property1.value(), newValue1 ).changed(_property2.propertyKeyId(), _property2.value(), newValue2).changed(_property3.propertyKeyId(), _property3.value(), newValue3).build();

			  // Then
			  assertThat( updates.ForIndexKeys( singleton( _nonSchemaIndex ), PropertyLoader( _property1, _property2, _property3 ), EntityType.NODE ), containsInAnyOrder( IndexEntryUpdate.Change( NODE_ID, _nonSchemaIndex, _values123, new Value[]{ newValue1, newValue2, newValue3 } ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateUpdateWhenRemovingLastPropForNonSchemaIndex()
		 public virtual void ShouldGenerateUpdateWhenRemovingLastPropForNonSchemaIndex()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).withTokensAfter( _label ).removed( _property2.propertyKeyId(), _property2.value() ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( singleton( _nonSchemaIndex ), PropertyLoader( _property2 ), EntityType.NODE ), containsInAnyOrder( IndexEntryUpdate.Remove( NODE_ID, _nonSchemaIndex, null, _property2.value(), null ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateUpdateWhenRemovingOnePropertyForNonSchemaIndex()
		 public virtual void ShouldGenerateUpdateWhenRemovingOnePropertyForNonSchemaIndex()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).withTokensAfter( _label ).removed( _property2.propertyKeyId(), _property2.value() ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( singleton( _nonSchemaIndex ), PropertyLoader( _property1, _property2, _property3 ), EntityType.NODE ), containsInAnyOrder( IndexEntryUpdate.Change( NODE_ID, _nonSchemaIndex, _values123, new Value[]{ _property1.value(), null, _property3.value() } ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateUpdateWhenAddingOneTokenForNonSchemaIndex()
		 public virtual void ShouldGenerateUpdateWhenAddingOneTokenForNonSchemaIndex()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _empty ).withTokensAfter( _label ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( singleton( _nonSchemaIndex ), PropertyLoader( _property1, _property2, _property3 ), EntityType.NODE ), containsInAnyOrder( IndexEntryUpdate.Add( NODE_ID, _nonSchemaIndex, _values123 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateUpdateWhenAddingMultipleTokensForNonSchemaIndex()
		 public virtual void ShouldGenerateUpdateWhenAddingMultipleTokensForNonSchemaIndex()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _empty ).withTokensAfter( _allLabels ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( singleton( _nonSchemaIndex ), PropertyLoader( _property1, _property2, _property3 ), EntityType.NODE ), containsInAnyOrder( IndexEntryUpdate.Add( NODE_ID, _nonSchemaIndex, _values123 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGenerateUpdateWhenAddingAnotherTokenForNonSchemaIndex()
		 public virtual void ShouldNotGenerateUpdateWhenAddingAnotherTokenForNonSchemaIndex()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).withTokensAfter( _allLabels ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( singleton( _nonSchemaIndex ), PropertyLoader( _property1, _property2, _property3 ), EntityType.NODE ), emptyIterable() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGenerateUpdateWhenAddingAnotherUselessTokenForNonSchemaIndex()
		 public virtual void ShouldNotGenerateUpdateWhenAddingAnotherUselessTokenForNonSchemaIndex()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).withTokensAfter( LABEL_ID1, UNUSED_LABEL_ID ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( singleton( _nonSchemaIndex ), PropertyLoader( _property1, _property2, _property3 ), EntityType.NODE ), emptyIterable() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateUpdateWhenSwitchingToUselessTokenForNonSchemaIndex()
		 public virtual void ShouldGenerateUpdateWhenSwitchingToUselessTokenForNonSchemaIndex()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).withTokensAfter( UNUSED_LABEL_ID ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( singleton( _nonSchemaIndex ), PropertyLoader( _property1, _property2, _property3 ), EntityType.NODE ), containsInAnyOrder( IndexEntryUpdate.Remove( NODE_ID, _nonSchemaIndex, _values123 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGenerateUpdateWhenRemovingOneTokenForNonSchemaIndex()
		 public virtual void ShouldNotGenerateUpdateWhenRemovingOneTokenForNonSchemaIndex()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _allLabels ).withTokensAfter( _label ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( singleton( _nonSchemaIndex ), PropertyLoader( _property1, _property2, _property3 ), EntityType.NODE ), emptyIterable() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateUpdateWhenRemovingLastTokenForNonSchemaIndex()
		 public virtual void ShouldGenerateUpdateWhenRemovingLastTokenForNonSchemaIndex()
		 {
			  // When
			  EntityUpdates updates = EntityUpdates.forEntity( NODE_ID, false ).withTokens( _label ).withTokensAfter( _empty ).build();

			  // Then
			  assertThat( updates.ForIndexKeys( singleton( _nonSchemaIndex ), PropertyLoader( _property1, _property2, _property3 ), EntityType.NODE ), containsInAnyOrder( IndexEntryUpdate.Remove( NODE_ID, _nonSchemaIndex, _values123 ) ) );
		 }

		 private PropertyLoader PropertyLoader( params StorageProperty[] properties )
		 {
			  IDictionary<int, Value> propertyMap = new Dictionary<int, Value>();
			  foreach ( StorageProperty p in properties )
			  {
					propertyMap[p.PropertyKeyId()] = p.Value();
			  }
			  return ( nodeId1, type, propertyIds, sink ) =>
			  {
				MutableIntIterator iterator = propertyIds.intIterator();
				while ( iterator.hasNext() )
				{
					 int propertyId = iterator.next();
					 if ( propertyMap.ContainsKey( propertyId ) )
					 {
						  sink.onProperty( propertyId, propertyMap[propertyId] );
						  iterator.remove();
					 }
				}
			  };
		 }

		 private PropertyLoader AssertNoLoading()
		 {
			  return ( nodeId1, type, propertyIds, sink ) => fail( "Should never attempt to load properties!" );
		 }
	}

}