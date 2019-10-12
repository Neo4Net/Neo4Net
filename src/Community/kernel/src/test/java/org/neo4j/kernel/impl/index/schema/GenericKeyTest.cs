using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using MethodSource = org.junit.jupiter.@params.provider.MethodSource;


	using SpaceFillingCurve = Neo4Net.Gis.Spatial.Index.curves.SpaceFillingCurve;
	using ByteArrayPageCursor = Neo4Net.Io.pagecache.ByteArrayPageCursor;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConfiguredSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using IndexSpecificSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using UTF8 = Neo4Net.@string.UTF8;
	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using AnyValues = Neo4Net.Values.AnyValues;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using PointArray = Neo4Net.Values.Storable.PointArray;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using RandomValues = Neo4Net.Values.Storable.RandomValues;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.GenericKey.NO_ENTITY_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexKey.Inclusion.NEUTRAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.COMPARATOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.booleanArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.byteArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.dateArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.dateTimeArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.doubleArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.durationArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.floatArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.intArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.isGeometryArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.isGeometryValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.localDateTimeArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.localTimeArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.of;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.pointArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.shortArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.timeArray;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class GenericKeyTest
	internal class GenericKeyTest
	{
		 private readonly IndexSpecificSpaceFillingCurveSettingsCache _noSpecificIndexSettings = new IndexSpecificSpaceFillingCurveSettingsCache( new ConfiguredSpaceFillingCurveSettingsCache( Config.defaults() ), new Dictionary<CoordinateReferenceSystem, SpaceFillingCurveSettings>() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private static org.neo4j.test.rule.RandomRule random;
		 private static RandomRule _random;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setupRandomConfig()
		 internal virtual void SetupRandomConfig()
		 {
			  _random = _random.withConfiguration( new ConfigurationAnonymousInnerClass( this ) );
			  _random.reset();
		 }

		 private class ConfigurationAnonymousInnerClass : RandomValues.Configuration
		 {
			 private readonly GenericKeyTest _outerInstance;

			 public ConfigurationAnonymousInnerClass( GenericKeyTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public int stringMinLength()
			 {
				  return 0;
			 }

			 public int stringMaxLength()
			 {
				  return 50;
			 }

			 public int arrayMinLength()
			 {
				  return 0;
			 }

			 public int arrayMaxLength()
			 {
				  return 10;
			 }

			 public int maxCodePoint()
			 {
				  return RandomValues.MAX_BMP_CODE_POINT;
			 }
		 }

		 /* TESTS FOR SLOT STATE (not including entityId) */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("validValueGenerators") void readWhatIsWritten(ValueGenerator valueGenerator)
		 internal virtual void ReadWhatIsWritten( ValueGenerator valueGenerator )
		 {
			  // Given
			  PageCursor cursor = NewPageCursor();
			  GenericKey writeState = NewKeyState();
			  Value value = valueGenerator();
			  int offset = cursor.Offset;

			  // When
			  writeState.WriteValue( value, NEUTRAL );
			  writeState.Put( cursor );

			  // Then
			  GenericKey readState = NewKeyState();
			  int size = writeState.Size();
			  cursor.Offset = offset;
			  assertTrue( readState.Get( cursor, size ), "failed to read" );
			  assertEquals( 0, readState.CompareValueTo( writeState ), "key states are not equal" );
			  Value readValue = readState.AsValue();
			  assertEquals( value, readValue, "deserialized values are not equal" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("validValueGenerators") void copyShouldCopy(ValueGenerator valueGenerator)
		 internal virtual void CopyShouldCopy( ValueGenerator valueGenerator )
		 {
			  // Given
			  GenericKey from = NewKeyState();
			  Value value = valueGenerator();
			  from.WriteValue( value, NEUTRAL );
			  GenericKey to = GenericKeyStateWithSomePreviousState( valueGenerator );

			  // When
			  to.CopyFrom( from );

			  // Then
			  assertEquals( 0, from.CompareValueTo( to ), "states not equals after copy" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void copyShouldCopyExtremeValues()
		 internal virtual void CopyShouldCopyExtremeValues()
		 {
			  // Given
			  GenericKey extreme = NewKeyState();
			  GenericKey copy = NewKeyState();

			  foreach ( ValueGroup valueGroup in ValueGroup.values() )
			  {
					if ( valueGroup != ValueGroup.NO_VALUE )
					{
						 extreme.InitValueAsLowest( valueGroup );
						 copy.CopyFrom( extreme );
						 assertEquals( 0, extreme.CompareValueTo( copy ), "states not equals after copy, valueGroup=" + valueGroup );
						 extreme.InitValueAsHighest( valueGroup );
						 copy.CopyFrom( extreme );
						 assertEquals( 0, extreme.CompareValueTo( copy ), "states not equals after copy, valueGroup=" + valueGroup );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("validComparableValueGenerators") void compareToMustAlignWithValuesCompareTo(ValueGenerator valueGenerator)
		 internal virtual void CompareToMustAlignWithValuesCompareTo( ValueGenerator valueGenerator )
		 {
			  // Given
			  IList<Value> values = new List<Value>();
			  IList<GenericKey> states = new List<GenericKey>();
			  for ( int i = 0; i < 10; i++ )
			  {
					Value value = valueGenerator();
					values.Add( value );
					GenericKey state = NewKeyState();
					state.WriteValue( value, NEUTRAL );
					states.Add( state );
			  }

			  // When
			  values.sort( COMPARATOR );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  states.sort( GenericKey::compareValueTo );

			  // Then
			  for ( int i = 0; i < values.Count; i++ )
			  {
					assertEquals( values[i], states[i].AsValue(), "sort order was different" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void comparePointsMustOnlyReturnZeroForEqualPoints()
		 internal virtual void ComparePointsMustOnlyReturnZeroForEqualPoints()
		 {
			  PointValue firstPoint = _random.randomValues().nextPointValue();
			  PointValue equalPoint = Values.point( firstPoint );
			  CoordinateReferenceSystem crs = firstPoint.CoordinateReferenceSystem;
			  SpaceFillingCurve curve = _noSpecificIndexSettings.forCrs( crs, false );
			  long? spaceFillingCurveValue = curve.DerivedValueFor( firstPoint.Coordinate() );
			  PointValue centerPoint = Values.pointValue( crs, curve.CenterPointFor( spaceFillingCurveValue.Value ) );

			  GenericKey firstKey = NewKeyState();
			  firstKey.WriteValue( firstPoint, NEUTRAL );
			  GenericKey equalKey = NewKeyState();
			  equalKey.WriteValue( equalPoint, NEUTRAL );
			  GenericKey centerKey = NewKeyState();
			  centerKey.WriteValue( centerPoint, NEUTRAL );
			  GenericKey noCoordsKey = NewKeyState();
			  noCoordsKey.WriteValue( equalPoint, NEUTRAL );
			  GeometryType.NoCoordinates = noCoordsKey;

			  assertEquals( 0, firstKey.CompareValueTo( equalKey ), "expected keys to be equal" );
			  assertEquals( firstPoint.CompareTo( centerPoint ) != 0, firstKey.CompareValueTo( centerKey ) != 0, "expected keys to be equal if and only if source points are equal" );
			  assertEquals( 0, firstKey.CompareValueTo( noCoordsKey ), "expected keys to be equal" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void comparePointArraysMustOnlyReturnZeroForEqualArrays()
		 internal virtual void ComparePointArraysMustOnlyReturnZeroForEqualArrays()
		 {
			  PointArray firstArray = _random.randomValues().nextPointArray();
			  PointValue[] sourcePointValues = firstArray.AsObjectCopy();
			  PointArray equalArray = Values.pointArray( sourcePointValues );
			  PointValue[] centerPointValues = new PointValue[sourcePointValues.Length];
			  for ( int i = 0; i < sourcePointValues.Length; i++ )
			  {
					PointValue sourcePointValue = sourcePointValues[i];
					CoordinateReferenceSystem crs = sourcePointValue.CoordinateReferenceSystem;
					SpaceFillingCurve curve = _noSpecificIndexSettings.forCrs( crs, false );
					long? spaceFillingCurveValue = curve.DerivedValueFor( sourcePointValue.Coordinate() );
					centerPointValues[i] = Values.pointValue( crs, curve.CenterPointFor( spaceFillingCurveValue.Value ) );
			  }
			  PointArray centerArray = Values.pointArray( centerPointValues );

			  GenericKey firstKey = NewKeyState();
			  firstKey.WriteValue( firstArray, NEUTRAL );
			  GenericKey equalKey = NewKeyState();
			  equalKey.WriteValue( equalArray, NEUTRAL );
			  GenericKey centerKey = NewKeyState();
			  centerKey.WriteValue( centerArray, NEUTRAL );
			  GenericKey noCoordsKey = NewKeyState();
			  noCoordsKey.WriteValue( equalArray, NEUTRAL );
			  GeometryType.NoCoordinates = noCoordsKey;

			  assertEquals( 0, firstKey.CompareValueTo( equalKey ), "expected keys to be equal" );
			  assertEquals( firstArray.compareToSequence( centerArray, AnyValues.COMPARATOR ) != 0, firstKey.CompareValueTo( centerKey ) != 0, "expected keys to be equal if and only if source points are equal" );
			  assertEquals( 0, firstKey.CompareValueTo( noCoordsKey ), "expected keys to be equal" );
		 }

		 // The reason this test doesn't test incomparable values is that it relies on ordering being same as that of the Values module.
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("validComparableValueGenerators") void mustProduceValidMinimalSplitters(ValueGenerator valueGenerator)
		 internal virtual void MustProduceValidMinimalSplitters( ValueGenerator valueGenerator )
		 {
			  // Given
			  Value value1 = valueGenerator();
			  Value value2 = UniqueSecondValue( valueGenerator, value1 );

			  // When
			  Value left = PickSmaller( value1, value2 );
			  Value right = PickOther( value1, value2, left );

			  // Then
			  AssertValidMinimalSplitter( left, right );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("validValueGenerators") void mustProduceValidMinimalSplittersWhenValuesAreEqual(ValueGenerator valueGenerator)
		 internal virtual void MustProduceValidMinimalSplittersWhenValuesAreEqual( ValueGenerator valueGenerator )
		 {
			  AssertValidMinimalSplitterForEqualValues( valueGenerator() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("validValueGenerators") void mustReportCorrectSize(ValueGenerator valueGenerator)
		 internal virtual void MustReportCorrectSize( ValueGenerator valueGenerator )
		 {
			  // Given
			  PageCursor cursor = NewPageCursor();
			  Value value = valueGenerator();
			  GenericKey state = NewKeyState();
			  state.WriteValue( value, NEUTRAL );
			  int offsetBefore = cursor.Offset;

			  // When
			  int reportedSize = state.Size();
			  state.Put( cursor );
			  int offsetAfter = cursor.Offset;

			  // Then
			  int actualSize = offsetAfter - offsetBefore;
			  assertEquals( reportedSize, actualSize, string.Format( "did not report correct size, value={0}, actualSize={1:D}, reportedSize={2:D}", value, actualSize, reportedSize ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void lowestMustBeLowest()
		 internal virtual void LowestMustBeLowest()
		 {
			  // GEOMETRY
			  AssertLowest( PointValue.MIN_VALUE );
			  // ZONED_DATE_TIME
			  AssertLowest( DateTimeValue.MIN_VALUE );
			  // LOCAL_DATE_TIME
			  AssertLowest( LocalDateTimeValue.MIN_VALUE );
			  // DATE
			  AssertLowest( DateValue.MIN_VALUE );
			  // ZONED_TIME
			  AssertLowest( TimeValue.MIN_VALUE );
			  // LOCAL_TIME
			  AssertLowest( LocalTimeValue.MIN_VALUE );
			  // DURATION (duration, period)
			  AssertLowest( DurationValue.duration( Duration.ofSeconds( long.MinValue, 0 ) ) );
			  AssertLowest( DurationValue.duration( Period.of( int.MinValue, int.MinValue, int.MinValue ) ) );
			  // TEXT
			  AssertLowest( of( UTF8.decode( new sbyte[0] ) ) );
			  // BOOLEAN
			  AssertLowest( of( false ) );
			  // NUMBER (byte, short, int, long, float, double)
			  AssertLowest( of( sbyte.MinValue ) );
			  AssertLowest( of( short.MinValue ) );
			  AssertLowest( of( int.MinValue ) );
			  AssertLowest( of( long.MinValue ) );
			  AssertLowest( of( float.NegativeInfinity ) );
			  AssertLowest( of( double.NegativeInfinity ) );
			  // GEOMETRY_ARRAY
			  AssertLowest( pointArray( new PointValue[0] ) );
			  // ZONED_DATE_TIME_ARRAY
			  AssertLowest( dateTimeArray( new ZonedDateTime[0] ) );
			  // LOCAL_DATE_TIME_ARRAY
			  AssertLowest( localDateTimeArray( new DateTime[0] ) );
			  // DATE_ARRAY
			  AssertLowest( dateArray( new LocalDate[0] ) );
			  // ZONED_TIME_ARRAY
			  AssertLowest( timeArray( new OffsetTime[0] ) );
			  // LOCAL_TIME_ARRAY
			  AssertLowest( localTimeArray( new LocalTime[0] ) );
			  // DURATION_ARRAY (DurationValue, TemporalAmount)
			  AssertLowest( durationArray( new DurationValue[0] ) );
			  AssertLowest( durationArray( new TemporalAmount[0] ) );
			  // TEXT_ARRAY
			  AssertLowest( of( new string[0] ) );
			  // BOOLEAN_ARRAY
			  AssertLowest( of( new bool[0] ) );
			  // NUMBER_ARRAY (byte[], short[], int[], long[], float[], double[])
			  AssertLowest( of( new sbyte[0] ) );
			  AssertLowest( of( new short[0] ) );
			  AssertLowest( of( new int[0] ) );
			  AssertLowest( of( new long[0] ) );
			  AssertLowest( of( new float[0] ) );
			  AssertLowest( of( new double[0] ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void highestMustBeHighest()
		 internal virtual void HighestMustBeHighest()
		 {
			  // GEOMETRY
			  AssertHighest( PointValue.MAX_VALUE );
			  // ZONED_DATE_TIME
			  AssertHighest( DateTimeValue.MAX_VALUE );
			  // LOCAL_DATE_TIME
			  AssertHighest( LocalDateTimeValue.MAX_VALUE );
			  // DATE
			  AssertHighest( DateValue.MAX_VALUE );
			  // ZONED_TIME
			  AssertHighest( TimeValue.MAX_VALUE );
			  // LOCAL_TIME
			  AssertHighest( LocalTimeValue.MAX_VALUE );
			  // DURATION (duration, period)
			  AssertHighest( DurationValue.duration( Duration.ofSeconds( long.MaxValue, 999_999_999 ) ) );
			  AssertHighest( DurationValue.duration( Period.of( int.MaxValue, int.MaxValue, int.MaxValue ) ) );
			  // TEXT
			  AssertHighestString();
			  // BOOLEAN
			  AssertHighest( of( true ) );
			  // NUMBER (byte, short, int, long, float, double)
			  AssertHighest( of( sbyte.MaxValue ) );
			  AssertHighest( of( short.MaxValue ) );
			  AssertHighest( of( int.MaxValue ) );
			  AssertHighest( of( long.MaxValue ) );
			  AssertHighest( of( float.PositiveInfinity ) );
			  AssertHighest( of( double.PositiveInfinity ) );
			  // GEOMETRY_ARRAY
			  AssertHighest( pointArray( new PointValue[]{ PointValue.MAX_VALUE } ) );
			  // ZONED_DATE_TIME_ARRAY
			  AssertHighest( dateTimeArray( new ZonedDateTime[]{ DateTimeValue.MAX_VALUE.asObjectCopy() } ) );
			  // LOCAL_DATE_TIME_ARRAY
			  AssertHighest( localDateTimeArray( new DateTime[]{ LocalDateTimeValue.MAX_VALUE.asObjectCopy() } ) );
			  // DATE_ARRAY
			  AssertHighest( dateArray( new LocalDate[]{ DateValue.MAX_VALUE.asObjectCopy() } ) );
			  // ZONED_TIME_ARRAY
			  AssertHighest( timeArray( new OffsetTime[]{ TimeValue.MAX_VALUE.asObjectCopy() } ) );
			  // LOCAL_TIME_ARRAY
			  AssertHighest( localTimeArray( new LocalTime[]{ LocalTimeValue.MAX_VALUE.asObjectCopy() } ) );
			  // DURATION_ARRAY (DurationValue, TemporalAmount)
			  AssertHighest( durationArray( new DurationValue[]{ DurationValue.duration( Duration.ofSeconds( long.MaxValue, 999_999_999 ) ) } ) );
			  AssertHighest( durationArray( new DurationValue[]{ DurationValue.duration( Period.of( int.MaxValue, int.MaxValue, int.MaxValue ) ) } ) );
			  AssertHighest( durationArray( new TemporalAmount[]{ Duration.ofSeconds( long.MaxValue, 999_999_999 ) } ) );
			  AssertHighest( durationArray( new TemporalAmount[]{ Period.of( int.MaxValue, int.MaxValue, int.MaxValue ) } ) );
			  // TEXT_ARRAY
			  AssertHighestStringArray();
			  // BOOLEAN_ARRAY
			  AssertHighest( booleanArray( new bool[]{ true } ) );
			  // NUMBER_ARRAY (byte[], short[], int[], long[], float[], double[])
			  AssertHighest( byteArray( new sbyte[]{ sbyte.MaxValue } ) );
			  AssertHighest( shortArray( new short[]{ short.MaxValue } ) );
			  AssertHighest( intArray( new int[]{ int.MaxValue } ) );
			  AssertHighest( longArray( new long[]{ long.MaxValue } ) );
			  AssertHighest( floatArray( new float[]{ float.PositiveInfinity } ) );
			  AssertHighest( doubleArray( new double[]{ double.PositiveInfinity } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNeverOverwriteDereferencedTextValues()
		 internal virtual void ShouldNeverOverwriteDereferencedTextValues()
		 {
			  // Given a value that we dereference
			  Value srcValue = Values.utf8Value( "First string".GetBytes( Encoding.UTF8 ) );
			  GenericKey genericKeyState = NewKeyState();
			  genericKeyState.WriteValue( srcValue, NEUTRAL );
			  Value dereferencedValue = genericKeyState.AsValue();
			  assertEquals( srcValue, dereferencedValue );

			  // and write to page
			  PageCursor cursor = NewPageCursor();
			  int offset = cursor.Offset;
			  genericKeyState.Put( cursor );
			  int keySize = cursor.Offset - offset;
			  cursor.Offset = offset;

			  // we should not overwrite the first dereferenced value when initializing from a new value
			  genericKeyState.Clear();
			  Value srcValue2 = Values.utf8Value( "Secondstring".GetBytes( Encoding.UTF8 ) ); // <- Same length as first string
			  genericKeyState.WriteValue( srcValue2, NEUTRAL );
			  Value dereferencedValue2 = genericKeyState.AsValue();
			  assertEquals( srcValue2, dereferencedValue2 );
			  assertEquals( srcValue, dereferencedValue );

			  // and we should not overwrite the second value when we read back the first value from page
			  genericKeyState.Clear();
			  genericKeyState.Get( cursor, keySize );
			  Value dereferencedValue3 = genericKeyState.AsValue();
			  assertEquals( srcValue, dereferencedValue3 );
			  assertEquals( srcValue2, dereferencedValue2 );
			  assertEquals( srcValue, dereferencedValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void indexedCharShouldComeBackAsCharValue()
		 internal virtual void IndexedCharShouldComeBackAsCharValue()
		 {
			  ShouldReadBackToExactOriginalValue( _random.randomValues().nextCharValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void indexedCharArrayShouldComeBackAsCharArrayValue()
		 internal virtual void IndexedCharArrayShouldComeBackAsCharArrayValue()
		 {
			  ShouldReadBackToExactOriginalValue( _random.randomValues().nextCharArray() );
		 }

		 /* TESTS FOR KEY STATE (including entityId) */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("validValueGenerators") void minimalSplitterForSameValueShouldDivideLeftAndRight(ValueGenerator valueGenerator)
		 internal virtual void MinimalSplitterForSameValueShouldDivideLeftAndRight( ValueGenerator valueGenerator )
		 {
			  // Given
			  Value value = valueGenerator();
			  GenericLayout layout = NewLayout( 1 );
			  GenericKey left = layout.NewKey();
			  GenericKey right = layout.NewKey();
			  GenericKey minimalSplitter = layout.NewKey();

			  // keys with same value but different entityId
			  left.Initialize( 1 );
			  left.InitFromValue( 0, value, NEUTRAL );
			  right.Initialize( 2 );
			  right.InitFromValue( 0, value, NEUTRAL );

			  // When creating minimal splitter
			  layout.MinimalSplitter( left, right, minimalSplitter );

			  // Then that minimal splitter need to correctly divide left and right
			  assertTrue( layout.Compare( left, minimalSplitter ) < 0, "Expected minimal splitter to be strictly greater than left but wasn't for value " + value );
			  assertTrue( layout.Compare( minimalSplitter, right ) <= 0, "Expected right to be greater than or equal to minimal splitter but wasn't for value " + value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("validValueGenerators") void minimalSplitterShouldRemoveEntityIdIfPossible(ValueGenerator valueGenerator)
		 internal virtual void MinimalSplitterShouldRemoveEntityIdIfPossible( ValueGenerator valueGenerator )
		 {
			  // Given
			  Value firstValue = valueGenerator();
			  Value secondValue = UniqueSecondValue( valueGenerator, firstValue );
			  Value leftValue = PickSmaller( firstValue, secondValue );
			  Value rightValue = PickOther( firstValue, secondValue, leftValue );

			  GenericLayout layout = NewLayout( 1 );
			  GenericKey left = layout.NewKey();
			  GenericKey right = layout.NewKey();
			  GenericKey minimalSplitter = layout.NewKey();

			  // keys with unique values
			  left.Initialize( 1 );
			  left.InitFromValue( 0, leftValue, NEUTRAL );
			  right.Initialize( 2 );
			  right.InitFromValue( 0, rightValue, NEUTRAL );

			  // When creating minimal splitter
			  layout.MinimalSplitter( left, right, minimalSplitter );

			  // Then that minimal splitter should have entity id shaved off
			  assertEquals( NO_ENTITY_ID, minimalSplitter.EntityId, "Expected minimal splitter to have entityId removed when constructed from keys with unique values: " + "left=" + leftValue + ", right=" + rightValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("validValueGenerators") void minimalSplitterForSameValueShouldDivideLeftAndRightCompositeKey(ValueGenerator valueGenerator)
		 internal virtual void MinimalSplitterForSameValueShouldDivideLeftAndRightCompositeKey( ValueGenerator valueGenerator )
		 {
			  // Given composite keys with same set of values
			  int nbrOfSlots = _random.Next( 1, 5 );
			  GenericLayout layout = NewLayout( nbrOfSlots );
			  GenericKey left = layout.NewKey();
			  GenericKey right = layout.NewKey();
			  GenericKey minimalSplitter = layout.NewKey();
			  left.Initialize( 1 );
			  right.Initialize( 2 );
			  Value[] values = new Value[nbrOfSlots];
			  for ( int slot = 0; slot < nbrOfSlots; slot++ )
			  {
					Value value = valueGenerator();
					values[slot] = value;
					left.InitFromValue( slot, value, NEUTRAL );
					right.InitFromValue( slot, value, NEUTRAL );
			  }

			  // When creating minimal splitter
			  layout.MinimalSplitter( left, right, minimalSplitter );

			  // Then that minimal splitter need to correctly divide left and right
			  assertTrue( layout.Compare( left, minimalSplitter ) < 0, "Expected minimal splitter to be strictly greater than left but wasn't for value " + Arrays.ToString( values ) );
			  assertTrue( layout.Compare( minimalSplitter, right ) <= 0, "Expected right to be greater than or equal to minimal splitter but wasn't for value " + Arrays.ToString( values ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("validValueGenerators") void minimalSplitterShouldRemoveEntityIdIfPossibleCompositeKey(ValueGenerator valueGenerator)
		 internal virtual void MinimalSplitterShouldRemoveEntityIdIfPossibleCompositeKey( ValueGenerator valueGenerator )
		 {
			  // Given
			  int nbrOfSlots = _random.Next( 1, 5 );
			  int differingSlot = _random.Next( nbrOfSlots );
			  GenericLayout layout = NewLayout( nbrOfSlots );
			  GenericKey left = layout.NewKey();
			  GenericKey right = layout.NewKey();
			  GenericKey minimalSplitter = layout.NewKey();
			  left.Initialize( 1 );
			  right.Initialize( 2 );
			  // Same value on all except one slot
			  for ( int slot = 0; slot < nbrOfSlots; slot++ )
			  {
					if ( slot == differingSlot )
					{
						 continue;
					}
					Value value = valueGenerator();
					left.InitFromValue( slot, value, NEUTRAL );
					right.InitFromValue( slot, value, NEUTRAL );
			  }
			  Value firstValue = valueGenerator();
			  Value secondValue = UniqueSecondValue( valueGenerator, firstValue );
			  Value leftValue = PickSmaller( firstValue, secondValue );
			  Value rightValue = PickOther( firstValue, secondValue, leftValue );
			  left.InitFromValue( differingSlot, leftValue, NEUTRAL );
			  right.InitFromValue( differingSlot, rightValue, NEUTRAL );

			  // When creating minimal splitter
			  layout.MinimalSplitter( left, right, minimalSplitter );

			  // Then that minimal splitter should have entity id shaved off
			  assertEquals( NO_ENTITY_ID, minimalSplitter.EntityId, "Expected minimal splitter to have entityId removed when constructed from keys with unique values: " + "left=" + leftValue + ", right=" + rightValue );
		 }

		 private void ShouldReadBackToExactOriginalValue( Value srcValue )
		 {
			  // given
			  GenericKey state = NewKeyState();
			  state.Clear();
			  state.WriteValue( srcValue, NEUTRAL );
			  Value retrievedValueAfterWrittenToState = state.AsValue();
			  assertEquals( srcValue, retrievedValueAfterWrittenToState );
			  assertEquals( srcValue.GetType(), retrievedValueAfterWrittenToState.GetType() );

			  // ... which is written to cursor
			  PageCursor cursor = NewPageCursor();
			  int offset = cursor.Offset;
			  state.Put( cursor );
			  int keySize = cursor.Offset - offset;
			  cursor.Offset = offset;

			  // when reading it back
			  state.Clear();
			  state.Get( cursor, keySize );

			  // then it should also be retrieved as char value
			  Value retrievedValueAfterReadFromCursor = state.AsValue();
			  assertEquals( srcValue, retrievedValueAfterReadFromCursor );
			  assertEquals( srcValue.GetType(), retrievedValueAfterReadFromCursor.GetType() );
		 }

		 private void AssertHighestStringArray()
		 {
			  for ( int i = 0; i < 1000; i++ )
			  {
					AssertHighest( _random.randomValues().nextTextArray() );
			  }
		 }

		 private void AssertHighestString()
		 {
			  for ( int i = 0; i < 1000; i++ )
			  {
					AssertHighest( _random.randomValues().nextTextValue() );
			  }
		 }

		 private void AssertHighest( Value value )
		 {
			  GenericKey highestOfAll = NewKeyState();
			  GenericKey highestInValueGroup = NewKeyState();
			  GenericKey other = NewKeyState();
			  highestOfAll.InitValueAsHighest( ValueGroup.UNKNOWN );
			  highestInValueGroup.InitValueAsHighest( value.ValueGroup() );
			  other.WriteValue( value, NEUTRAL );
			  assertTrue( highestInValueGroup.CompareValueTo( other ) > 0, "highestInValueGroup not higher than " + value );
			  assertTrue( highestOfAll.CompareValueTo( other ) > 0, "highestOfAll not higher than " + value );
			  assertTrue( highestOfAll.CompareValueTo( highestInValueGroup ) > 0 || highestOfAll.TypeConflict == highestInValueGroup.TypeConflict, "highestOfAll not higher than highestInValueGroup" );
		 }

		 private void AssertLowest( Value value )
		 {
			  GenericKey lowestOfAll = NewKeyState();
			  GenericKey lowestInValueGroup = NewKeyState();
			  GenericKey other = NewKeyState();
			  lowestOfAll.InitValueAsLowest( ValueGroup.UNKNOWN );
			  lowestInValueGroup.InitValueAsLowest( value.ValueGroup() );
			  other.WriteValue( value, NEUTRAL );
			  assertTrue( lowestInValueGroup.CompareValueTo( other ) <= 0 );
			  assertTrue( lowestOfAll.CompareValueTo( other ) <= 0 );
			  assertTrue( lowestOfAll.CompareValueTo( lowestInValueGroup ) <= 0 );
		 }

		 private Value PickSmaller( Value value1, Value value2 )
		 {
			  return COMPARATOR.compare( value1, value2 ) < 0 ? value1 : value2;
		 }

		 private Value PickOther( Value value1, Value value2, Value currentValue )
		 {
			  return currentValue == value1 ? value2 : value1;
		 }

		 private Value UniqueSecondValue( ValueGenerator valueGenerator, Value firstValue )
		 {
			  Value secondValue;
			  do
			  {
					secondValue = valueGenerator();
			  } while ( COMPARATOR.compare( firstValue, secondValue ) == 0 );
			  return secondValue;
		 }

		 private void AssertValidMinimalSplitter( Value left, Value right )
		 {
			  GenericKey leftState = NewKeyState();
			  leftState.WriteValue( left, NEUTRAL );
			  GenericKey rightState = NewKeyState();
			  rightState.WriteValue( right, NEUTRAL );

			  GenericKey minimalSplitter = NewKeyState();
			  rightState.MinimalSplitter( leftState, rightState, minimalSplitter );

			  assertTrue( leftState.CompareValueTo( minimalSplitter ) < 0, "left state not less than minimal splitter, leftState=" + leftState + ", rightState=" + rightState + ", minimalSplitter=" + minimalSplitter );
			  assertTrue( rightState.CompareValueTo( minimalSplitter ) >= 0, "right state not less than minimal splitter, leftState=" + leftState + ", rightState=" + rightState + ", minimalSplitter=" + minimalSplitter );
		 }

		 private void AssertValidMinimalSplitterForEqualValues( Value value )
		 {
			  GenericKey leftState = NewKeyState();
			  leftState.WriteValue( value, NEUTRAL );
			  GenericKey rightState = NewKeyState();
			  rightState.WriteValue( value, NEUTRAL );

			  GenericKey minimalSplitter = NewKeyState();
			  rightState.MinimalSplitter( leftState, rightState, minimalSplitter );

			  assertEquals( 0, leftState.CompareValueTo( minimalSplitter ), "left state not equal to minimal splitter, leftState=" + leftState + ", rightState=" + rightState + ", minimalSplitter=" + minimalSplitter );
			  assertEquals( 0, rightState.CompareValueTo( minimalSplitter ), "right state not equal to minimal splitter, leftState=" + leftState + ", rightState=" + rightState + ", minimalSplitter=" + minimalSplitter );
		 }

		 private static Value NextValidValue( bool includeIncomparable )
		 {
			  Value value;
			  do
			  {
					value = _random.randomValues().nextValue();
			  } while ( !includeIncomparable && IsIncomparable( value ) );
			  return value;
		 }

		 private static bool IsIncomparable( Value value )
		 {
			  return isGeometryValue( value ) || isGeometryArray( value );
		 }

		 private static ValueGenerator[] ListValueGenerators( bool includeIncomparable )
		 {
			  IList<ValueGenerator> generators = new IList<ValueGenerator> { () => _random.randomValues().nextDateTimeValue(), () => _random.randomValues().nextLocalDateTimeValue(), () => _random.randomValues().nextDateValue(), () => _random.randomValues().nextTimeValue(), () => _random.randomValues().nextLocalTimeValue(), () => _random.randomValues().nextPeriod(), () => _random.randomValues().nextDuration(), () => _random.randomValues().nextCharValue(), () => _random.randomValues().nextTextValue(), () => _random.randomValues().nextAlphaNumericTextValue(), () => _random.randomValues().nextBooleanValue(), () => _random.randomValues().nextNumberValue(), () => _random.randomValues().nextDateTimeArray(), () => _random.randomValues().nextLocalDateTimeArray(), () => _random.randomValues().nextDateArray(), () => _random.randomValues().nextTimeArray(), () => _random.randomValues().nextLocalTimeArray(), () => _random.randomValues().nextDurationArray(), () => _random.randomValues().nextDurationArray(), () => _random.randomValues().nextCharArray(), () => _random.randomValues().nextTextArray(), () => _random.randomValues().nextAlphaNumericTextArray(), () => _random.randomValues().nextBooleanArray(), () => _random.randomValues().nextByteArray(), () => _random.randomValues().nextShortArray(), () => _random.randomValues().nextIntArray(), () => _random.randomValues().nextLongArray(), () => _random.randomValues().nextFloatArray(), () => _random.randomValues().nextDoubleArray(), () => NextValidValue(includeIncomparable) };

			  if ( includeIncomparable )
			  {
					( ( IList<ValueGenerator> )generators ).AddRange( asList( () => _random.randomValues().nextPointValue(), () => _random.randomValues().nextGeographicPoint(), () => _random.randomValues().nextGeographic3DPoint(), () => _random.randomValues().nextCartesianPoint(), () => _random.randomValues().nextCartesian3DPoint(), () => _random.randomValues().nextGeographicPointArray(), () => _random.randomValues().nextGeographic3DPoint(), () => _random.randomValues().nextCartesianPointArray(), () => _random.randomValues().nextCartesian3DPointArray() ) );
			  }

			  return generators.ToArray();
		 }

		 private static Stream<ValueGenerator> ValidValueGenerators()
		 {
			  return Stream.of( ListValueGenerators( true ) );
		 }

		 private static Stream<ValueGenerator> ValidComparableValueGenerators()
		 {
			  return Stream.of( ListValueGenerators( false ) );
		 }

		 private GenericKey GenericKeyStateWithSomePreviousState( ValueGenerator valueGenerator )
		 {
			  GenericKey to = NewKeyState();
			  if ( _random.nextBoolean() )
			  {
					// Previous value
					NativeIndexKey.Inclusion inclusion = _random.among( Enum.GetValues( typeof( NativeIndexKey.Inclusion ) ) );
					Value value = valueGenerator();
					to.WriteValue( value, inclusion );
			  }
			  // No previous state
			  return to;
		 }

		 private PageCursor NewPageCursor()
		 {
			  return ByteArrayPageCursor.wrap( Neo4Net.Io.pagecache.PageCache_Fields.PAGE_SIZE );
		 }

		 private GenericKey NewKeyState()
		 {
			  return new GenericKey( _noSpecificIndexSettings );
		 }

		 private GenericLayout NewLayout( int numberOfSlots )
		 {
			  return new GenericLayout( numberOfSlots, _noSpecificIndexSettings );
		 }

		 private delegate Value ValueGenerator();
	}

}