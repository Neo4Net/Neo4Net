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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;
	using IntSets = org.eclipse.collections.impl.factory.primitive.IntSets;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using EnumSource = org.junit.jupiter.@params.provider.EnumSource;
	using MethodSource = org.junit.jupiter.@params.provider.MethodSource;


	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor;
	using MultiTokenSchemaDescriptor = Neo4Net.Kernel.api.schema.MultiTokenSchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;
	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ArrayUtils.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor_PropertySchemaType.COMPLETE_ALL_TOKENS;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class SchemaDescriptorLookupSetTest
	internal class SchemaDescriptorLookupSetTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.RandomRule random;
		 private RandomRule _random;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(DescriptorFactory.class) void shouldLookupSingleKeyDescriptors(DescriptorFactory factory)
		 internal virtual void ShouldLookupSingleKeyDescriptors( DescriptorFactory factory )
		 {
			  // given
			  SchemaDescriptorLookupSet<SchemaDescriptor> set = new SchemaDescriptorLookupSet<SchemaDescriptor>();
			  SchemaDescriptor expected = factory.descriptor( 1, 2 );
			  set.Add( expected );

			  // when
			  ISet<SchemaDescriptor> descriptors = new HashSet<SchemaDescriptor>();
			  set.MatchingDescriptorsForPartialListOfProperties( descriptors, Longs( 1 ), Ints( 2 ) );

			  // then
			  assertEquals( asSet( expected ), descriptors );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(DescriptorFactory.class) void shouldLookupSingleKeyAndSharedCompositeKeyDescriptors(DescriptorFactory factory)
		 internal virtual void ShouldLookupSingleKeyAndSharedCompositeKeyDescriptors( DescriptorFactory factory )
		 {
			  // given
			  SchemaDescriptorLookupSet<SchemaDescriptor> set = new SchemaDescriptorLookupSet<SchemaDescriptor>();
			  SchemaDescriptor expected1 = factory.descriptor( 1, 2 );
			  SchemaDescriptor expected2 = factory.descriptor( 1, 2, 3 );
			  set.Add( expected1 );
			  set.Add( expected2 );

			  // when
			  ISet<SchemaDescriptor> descriptors = new HashSet<SchemaDescriptor>();
			  set.MatchingDescriptorsForPartialListOfProperties( descriptors, Longs( 1 ), Ints( 2 ) );

			  // then
			  assertEquals( asSet( expected1, expected2 ), descriptors );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(DescriptorFactory.class) void shouldLookupCompositeKeyDescriptor(DescriptorFactory factory)
		 internal virtual void ShouldLookupCompositeKeyDescriptor( DescriptorFactory factory )
		 {
			  // given
			  SchemaDescriptorLookupSet<SchemaDescriptor> set = new SchemaDescriptorLookupSet<SchemaDescriptor>();
			  SchemaDescriptor descriptor1 = factory.descriptor( 1, 2, 3 );
			  SchemaDescriptor descriptor2 = factory.descriptor( 1, 2, 4 );
			  SchemaDescriptor descriptor3 = factory.descriptor( 1, 2, 5, 6 );
			  set.Add( descriptor1 );
			  set.Add( descriptor2 );
			  set.Add( descriptor3 );

			  // when
			  ISet<SchemaDescriptor> descriptors = new HashSet<SchemaDescriptor>();
			  set.MatchingDescriptorsForCompleteListOfProperties( descriptors, Longs( 1 ), Ints( 2, 5, 6 ) );

			  // then
			  assertEquals( asSet( descriptor3 ), descriptors );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(DescriptorFactory.class) void shouldLookupAllByEntityToken(DescriptorFactory factory)
		 internal virtual void ShouldLookupAllByEntityToken( DescriptorFactory factory )
		 {
			  // given
			  SchemaDescriptorLookupSet<SchemaDescriptor> set = new SchemaDescriptorLookupSet<SchemaDescriptor>();
			  SchemaDescriptor descriptor1 = factory.descriptor( 1, 2, 3 );
			  SchemaDescriptor descriptor2 = factory.descriptor( 1, 2, 4 );
			  SchemaDescriptor descriptor3 = factory.descriptor( 1, 2, 5, 6 );
			  SchemaDescriptor descriptor4 = factory.descriptor( 2, 2, 3 );
			  SchemaDescriptor descriptor5 = factory.descriptor( 3, 2, 5, 6 );
			  set.Add( descriptor1 );
			  set.Add( descriptor2 );
			  set.Add( descriptor3 );
			  set.Add( descriptor4 );
			  set.Add( descriptor5 );

			  // when
			  ISet<SchemaDescriptor> descriptors = new HashSet<SchemaDescriptor>();
			  set.MatchingDescriptors( descriptors, Longs( 1 ) );

			  // then
			  assertEquals( asSet( descriptor1, descriptor2, descriptor3 ), descriptors );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("nodeAndRelationshipEntityTypes") void shouldMatchOnAnyEntityAndPropertyTokenForPartialPropertySchemaType(org.Neo4Net.Kernel.Api.StorageEngine.EntityType EntityType)
		 internal virtual void ShouldMatchOnAnyEntityAndPropertyTokenForPartialPropertySchemaType( EntityType EntityType )
		 {
			  // given
			  SchemaDescriptorLookupSet<SchemaDescriptor> set = new SchemaDescriptorLookupSet<SchemaDescriptor>();
			  MultiTokenSchemaDescriptor descriptor1 = SchemaDescriptorFactory.multiToken( Ints( 0, 1, 2 ), EntityType, 3, 4, 5 );
			  MultiTokenSchemaDescriptor descriptor2 = SchemaDescriptorFactory.multiToken( Ints( 0, 1 ), EntityType, 3, 4 );
			  MultiTokenSchemaDescriptor descriptor3 = SchemaDescriptorFactory.multiToken( Ints( 0, 2 ), EntityType, 4, 5 );
			  set.Add( descriptor1 );
			  set.Add( descriptor2 );
			  set.Add( descriptor3 );

			  // given that this test revolves around IEntity tokens 0,1,2 and property tokens 3,4,5 these 3 descriptors below matches either
			  // only those tokens for IEntity or property or neither. I.e. these should never be included in matching results
			  set.Add( SchemaDescriptorFactory.multiToken( Ints( 3, 4 ), EntityType, 4, 5 ) );
			  set.Add( SchemaDescriptorFactory.multiToken( Ints( 0, 1 ), EntityType, 6, 7 ) );
			  set.Add( SchemaDescriptorFactory.multiToken( Ints( 3, 4 ), EntityType, 6, 7 ) );

			  // when matching these descriptors (in this case partial/complete list doesn't quite matter because the descriptors
			  // themselves are partially matched anyway.
			  ISet<SchemaDescriptor> descriptors1 = new HashSet<SchemaDescriptor>();
			  ISet<SchemaDescriptor> descriptors1Partial = new HashSet<SchemaDescriptor>();
			  ISet<SchemaDescriptor> descriptors2 = new HashSet<SchemaDescriptor>();
			  ISet<SchemaDescriptor> descriptors2Partial = new HashSet<SchemaDescriptor>();
			  ISet<SchemaDescriptor> descriptors3 = new HashSet<SchemaDescriptor>();
			  ISet<SchemaDescriptor> descriptors3Partial = new HashSet<SchemaDescriptor>();
			  set.MatchingDescriptorsForCompleteListOfProperties( descriptors1, Longs( 0, 1 ), Ints( 4, 5 ) );
			  set.MatchingDescriptorsForPartialListOfProperties( descriptors1Partial, Longs( 0, 1 ), Ints( 4, 5 ) );
			  set.MatchingDescriptorsForCompleteListOfProperties( descriptors2, Longs( 0 ), Ints( 3 ) );
			  set.MatchingDescriptorsForPartialListOfProperties( descriptors2Partial, Longs( 0 ), Ints( 3 ) );
			  set.MatchingDescriptorsForCompleteListOfProperties( descriptors3, Longs( 1 ), Ints( 5 ) );
			  set.MatchingDescriptorsForPartialListOfProperties( descriptors3Partial, Longs( 1 ), Ints( 5 ) );

			  // then
			  assertEquals( asSet( descriptor1, descriptor2, descriptor3 ), descriptors1 );
			  assertEquals( asSet( descriptor1, descriptor2, descriptor3 ), descriptors1Partial );
			  assertEquals( asSet( descriptor1, descriptor2 ), descriptors2 );
			  assertEquals( asSet( descriptor1, descriptor2 ), descriptors2Partial );
			  assertEquals( asSet( descriptor1 ), descriptors3 );
			  assertEquals( asSet( descriptor1 ), descriptors3Partial );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddRemoveAndLookupRandomDescriptorsNoIdempotentOperations()
		 internal virtual void ShouldAddRemoveAndLookupRandomDescriptorsNoIdempotentOperations()
		 {
			  ShouldAddRemoveAndLookupRandomDescriptors( false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddRemoveAndLookupRandomDescriptorsWithIdempotentOperations()
		 internal virtual void ShouldAddRemoveAndLookupRandomDescriptorsWithIdempotentOperations()
		 {
			  ShouldAddRemoveAndLookupRandomDescriptors( true );
		 }

		 private void ShouldAddRemoveAndLookupRandomDescriptors( bool includeIdempotentAddsAndRemoves )
		 {
			  // given
			  IList<SchemaDescriptor> all = new List<SchemaDescriptor>();
			  SchemaDescriptorLookupSet<SchemaDescriptor> set = new SchemaDescriptorLookupSet<SchemaDescriptor>();
			  int highEntityKeyId = 8;
			  int highPropertyKeyId = 8;
			  int maxNumberOfEntityKeys = 3;
			  int maxNumberOfPropertyKeys = 3;

			  // when/then
			  for ( int i = 0; i < 100 ; i++ )
			  {
					// add some
					int countToAdd = _random.Next( 1, 5 );
					for ( int a = 0; a < countToAdd; a++ )
					{
						 SchemaDescriptor descriptor = RandomSchemaDescriptor( highEntityKeyId, highPropertyKeyId, maxNumberOfEntityKeys, maxNumberOfPropertyKeys );
						 if ( !includeIdempotentAddsAndRemoves && all.IndexOf( descriptor ) != -1 )
						 {
							  // Oops, we randomly generated a descriptor that already exists
							  continue;
						 }

						 set.Add( descriptor );
						 all.Add( descriptor );
					}

					// remove some
					int countToRemove = _random.Next( 0, 2 );
					for ( int r = 0; r < countToRemove && all.Count > 0; r++ )
					{
						 SchemaDescriptor descriptor = all.RemoveAt( _random.Next( all.Count ) );
						 set.Remove( descriptor );
						 if ( includeIdempotentAddsAndRemoves )
						 {
							  set.Remove( descriptor );
							  while ( all.Remove( descriptor ) )
							  {
									// Just continue removing duplicates until all are done
							  }
						 }
					}

					// lookup
					int countToLookup = 20;
					for ( int l = 0; l < countToLookup; l++ )
					{
						 int[] IEntityTokenIdsInts = RandomUniqueSortedIntArray( highEntityKeyId, _random.Next( 1, 3 ) );
						 long[] IEntityTokenIds = ToLongArray( IEntityTokenIdsInts );
						 int[] propertyKeyIds = RandomUniqueSortedIntArray( highPropertyKeyId, _random.Next( 1, maxNumberOfPropertyKeys ) );
						 ISet<SchemaDescriptor> actual = new HashSet<SchemaDescriptor>();

						 // lookup by only IEntity tokens
						 actual.Clear();
						 set.MatchingDescriptors( actual, IEntityTokenIds );
						 assertEquals( ExpectedDescriptors( all, FilterByEntity( IEntityTokenIdsInts ) ), actual );

						 // lookup by partial property list
						 actual.Clear();
						 set.MatchingDescriptorsForPartialListOfProperties( actual, IEntityTokenIds, propertyKeyIds );
						 assertEquals( ExpectedDescriptors( all, FilterByEntityAndPropertyPartial( IEntityTokenIdsInts, propertyKeyIds ) ), actual );

						 // lookup by complete property list
						 actual.Clear();
						 set.MatchingDescriptorsForCompleteListOfProperties( actual, IEntityTokenIds, propertyKeyIds );
						 assertEquals( ExpectedDescriptors( all, FilterByEntityAndPropertyComplete( IEntityTokenIdsInts, propertyKeyIds ) ), actual );
					}
			  }
		 }

		 private static System.Predicate<SchemaDescriptor> FilterByEntityAndPropertyComplete( int[] IEntityTokenIds, int[] propertyKeyIds )
		 {
			  return descriptor =>
			  {
				IntPredicate propertyKeyPredicate = indexPropertyId => contains( propertyKeyIds, indexPropertyId );
				bool propertiesAccepted = descriptor.propertySchemaType() == COMPLETE_ALL_TOKENS ? stream(descriptor.PropertyIds).allMatch(propertyKeyPredicate) : stream(descriptor.PropertyIds).anyMatch(propertyKeyPredicate);
				return stream( descriptor.EntityTokenIds ).anyMatch( indexEntityId => contains( IEntityTokenIds, indexEntityId ) ) && propertiesAccepted;
			  };
		 }

		 private static System.Predicate<SchemaDescriptor> FilterByEntityAndPropertyPartial( int[] IEntityTokenIds, int[] propertyKeyIds )
		 {
			  return descriptor => stream( descriptor.EntityTokenIds ).anyMatch( indexEntityId => contains( IEntityTokenIds, indexEntityId ) ) && stream( descriptor.PropertyIds ).anyMatch( indexPropertyId => contains( propertyKeyIds, indexPropertyId ) );
		 }

		 private static System.Predicate<SchemaDescriptor> FilterByEntity( int[] IEntityTokenIds )
		 {
			  return descriptor => stream( descriptor.EntityTokenIds ).anyMatch( indexEntityId => contains( IEntityTokenIds, indexEntityId ) );
		 }

		 private static ISet<SchemaDescriptor> ExpectedDescriptors( IList<SchemaDescriptor> all, System.Predicate<SchemaDescriptor> filter )
		 {
			  return asSet( Iterators.filter( filter, all.GetEnumerator() ) );
		 }

		 private SchemaDescriptor RandomSchemaDescriptor( int highEntityKeyId, int highPropertyKeyId, int maxNumberOfEntityKeys, int maxNumberOfPropertyKeys )
		 {
			  int numberOfEntityKeys = _random.Next( 1, maxNumberOfEntityKeys );
			  int[] IEntityKeys = RandomUniqueUnsortedIntArray( highEntityKeyId, numberOfEntityKeys );
			  int numberOfPropertyKeys = _random.Next( 1, maxNumberOfPropertyKeys );
			  int[] propertyKeys = RandomUniqueUnsortedIntArray( highPropertyKeyId, numberOfPropertyKeys );
			  return IEntityKeys.Length > 1 ? SchemaDescriptorFactory.multiToken( IEntityKeys, EntityType.NODE, propertyKeys ) : SchemaDescriptorFactory.forLabel( IEntityKeys[0], propertyKeys );
		 }

		 private int[] RandomUniqueUnsortedIntArray( int maxValue, int length )
		 {
			  int[] array = new int[length];
			  MutableIntSet seen = IntSets.mutable.empty();
			  for ( int i = 0; i < length; i++ )
			  {
					int candidate;
					do
					{
						 candidate = _random.Next( maxValue );
					} while ( !seen.add( candidate ) );
					array[i] = candidate;
			  }
			  return array;
		 }

		 private int[] RandomUniqueSortedIntArray( int maxValue, int length )
		 {
			  int[] array = RandomUniqueUnsortedIntArray( maxValue, length );
			  Arrays.sort( array );
			  return array;
		 }

		 private static long[] ToLongArray( int[] array )
		 {
			  long[] result = new long[array.Length];
			  for ( int i = 0; i < array.Length; i++ )
			  {
					result[i] = array[i];
			  }
			  return result;
		 }

		 private static int[] Ints( params int[] properties )
		 {
			  return properties;
		 }

		 private static long[] Longs( params long[] labels )
		 {
			  return labels;
		 }

		 private static EntityType[] NodeAndRelationshipEntityTypes()
		 {
			  return new EntityType[]{ EntityType.NODE, EntityType.RELATIONSHIP };
		 }

		 internal abstract class DescriptorFactory
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           NODE { SchemaDescriptor descriptor(int labelId, int... propertyKeyIds) { return org.Neo4Net.kernel.api.schema.SchemaDescriptorFactory.forLabel(labelId, propertyKeyIds); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           RELATIONSHIP { SchemaDescriptor descriptor(int relTypeId, int... propertyKeyIds) { return org.Neo4Net.kernel.api.schema.SchemaDescriptorFactory.forRelType(relTypeId, propertyKeyIds); } };

			  private static readonly IList<DescriptorFactory> valueList = new List<DescriptorFactory>();

			  static DescriptorFactory()
			  {
				  valueList.Add( NODE );
				  valueList.Add( RELATIONSHIP );
			  }

			  public enum InnerEnum
			  {
				  NODE,
				  RELATIONSHIP
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private DescriptorFactory( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal abstract Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor descriptor( int IEntityTokenId, params int[] propertyKeyIds );

			 public static IList<DescriptorFactory> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static DescriptorFactory ValueOf( string name )
			 {
				 foreach ( DescriptorFactory enumInstance in DescriptorFactory.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }
	}

}