﻿using System;

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
namespace Neo4Net.Bolt.v2.messaging
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4NetPack = Neo4Net.Bolt.messaging.Neo4NetPack;
	using PackedInputArray = Neo4Net.Bolt.v1.packstream.PackedInputArray;
	using PackedOutputArray = Neo4Net.Bolt.v1.packstream.PackedOutputArray;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using AnyValue = Neo4Net.Values.AnyValue;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using TimeZones = Neo4Net.Values.Storable.TimeZones;
	using ListValue = Neo4Net.Values.@virtual.ListValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.packstream.PackStream.INT_16;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.packstream.PackStream.INT_32;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.CoordinateReferenceSystem.Cartesian;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.CoordinateReferenceSystem.Cartesian_3D;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.CoordinateReferenceSystem.WGS84;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.CoordinateReferenceSystem.WGS84_3D;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.DateTimeValue.datetime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.doubleValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.unsafePointValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValues.list;

	public class Neo4NetPackV2Test
	{
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 private static readonly string[] _timeZoneNames = TimeZones.supportedTimeZones().stream().filter(s => ZoneId.AvailableZoneIds.contains(s)).toArray(string[]::new);

		 private const int RANDOM_VALUES_TO_TEST = 1_000;
		 private const int RANDOM_LISTS_TO_TEST = 1_000;
		 private const int RANDOM_LIST_MAX_SIZE = 500;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.RandomRule random = new Neo4Net.test.rule.RandomRule();
		 public RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToPackPointWithIllegalDimensions()
		 public virtual void ShouldFailToPackPointWithIllegalDimensions()
		 {
			  TestPackingPointsWithWrongDimensions( 0 );
			  TestPackingPointsWithWrongDimensions( 1 );
			  TestPackingPointsWithWrongDimensions( 4 );
			  TestPackingPointsWithWrongDimensions( 100 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToUnpack2DPointWithIncorrectCoordinate() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToUnpack2DPointWithIncorrectCoordinate()
		 {
			  Neo4NetPackV2 Neo4NetPack = new Neo4NetPackV2();
			  PackedOutputArray output = new PackedOutputArray();
			  Neo4Net.Bolt.messaging.Neo4NetPack_Packer packer = Neo4NetPack.NewPacker( output );

			  packer.PackStructHeader( 3, Neo4NetPackV2.POINT_2_D );
			  packer.pack( intValue( WGS84.Code ) );
			  packer.pack( doubleValue( 42.42 ) );

			  try
			  {
					Unpack( output );
					fail( "Exception expected" );
			  }
			  catch ( UncheckedIOException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToUnpack3DPointWithIncorrectCoordinate() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToUnpack3DPointWithIncorrectCoordinate()
		 {
			  Neo4NetPackV2 Neo4NetPack = new Neo4NetPackV2();
			  PackedOutputArray output = new PackedOutputArray();
			  Neo4Net.Bolt.messaging.Neo4NetPack_Packer packer = Neo4NetPack.NewPacker( output );

			  packer.PackStructHeader( 4, Neo4NetPackV2.POINT_3_D );
			  packer.pack( intValue( Cartesian.Code ) );
			  packer.pack( doubleValue( 1.0 ) );
			  packer.pack( doubleValue( 100.1 ) );

			  try
			  {
					Unpack( output );
					fail( "Exception expected" );
			  }
			  catch ( UncheckedIOException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackAndUnpack2DPoints()
		 public virtual void ShouldPackAndUnpack2DPoints()
		 {
			  TestPackingAndUnpacking( this.randomPoint2D );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackAndUnpack3DPoints()
		 public virtual void ShouldPackAndUnpack3DPoints()
		 {
			  TestPackingAndUnpacking( this.randomPoint3D );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackAndUnpackListsOf2DPoints()
		 public virtual void ShouldPackAndUnpackListsOf2DPoints()
		 {
			  TestPackingAndUnpacking( () => RandomList(this.randomPoint2D) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackAndUnpackListsOf3DPoints()
		 public virtual void ShouldPackAndUnpackListsOf3DPoints()
		 {
			  TestPackingAndUnpacking( () => RandomList(this.randomPoint3D) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackAndUnpackDuration()
		 public virtual void ShouldPackAndUnpackDuration()
		 {
			  TestPackingAndUnpacking( this.randomDuration );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackAndUnpackPeriod()
		 public virtual void ShouldPackAndUnpackPeriod()
		 {
			  TestPackingAndUnpacking( this.randomPeriod );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackAndUnpackListsOfDuration()
		 public virtual void ShouldPackAndUnpackListsOfDuration()
		 {
			  TestPackingAndUnpacking( () => RandomList(this.randomDuration) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackAndUnpackDate()
		 public virtual void ShouldPackAndUnpackDate()
		 {
			  TestPackingAndUnpacking( this.randomDate );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackAndUnpackListsOfDate()
		 public virtual void ShouldPackAndUnpackListsOfDate()
		 {
			  TestPackingAndUnpacking( () => RandomList(this.randomDate) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackAndUnpackLocalTime()
		 public virtual void ShouldPackAndUnpackLocalTime()
		 {
			  TestPackingAndUnpacking( this.randomLocalTime );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackAndUnpackListsOfLocalTime()
		 public virtual void ShouldPackAndUnpackListsOfLocalTime()
		 {
			  TestPackingAndUnpacking( () => RandomList(this.randomLocalTime) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackAndUnpackTime()
		 public virtual void ShouldPackAndUnpackTime()
		 {
			  TestPackingAndUnpacking( this.randomTime );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackAndUnpackListsOfTime()
		 public virtual void ShouldPackAndUnpackListsOfTime()
		 {
			  TestPackingAndUnpacking( () => RandomList(this.randomTime) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackAndUnpackLocalDateTime()
		 public virtual void ShouldPackAndUnpackLocalDateTime()
		 {
			  TestPackingAndUnpacking( this.randomLocalDateTime );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackAndUnpackListsOfLocalDateTime()
		 public virtual void ShouldPackAndUnpackListsOfLocalDateTime()
		 {
			  TestPackingAndUnpacking( () => RandomList(this.randomLocalDateTime) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackAndUnpackDateTimeWithTimeZoneName()
		 public virtual void ShouldPackAndUnpackDateTimeWithTimeZoneName()
		 {
			  TestPackingAndUnpacking( this.randomDateTimeWithTimeZoneName );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackAndUnpackListsOfDateTimeWithTimeZoneName()
		 public virtual void ShouldPackAndUnpackListsOfDateTimeWithTimeZoneName()
		 {
			  TestPackingAndUnpacking( () => RandomList(this.randomDateTimeWithTimeZoneName) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackAndUnpackDateTimeWithTimeZoneOffset()
		 public virtual void ShouldPackAndUnpackDateTimeWithTimeZoneOffset()
		 {
			  TestPackingAndUnpacking( this.randomDateTimeWithTimeZoneOffset );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackAndUnpackListsOfDateTimeWithTimeZoneOffset()
		 public virtual void ShouldPackAndUnpackListsOfDateTimeWithTimeZoneOffset()
		 {
			  TestPackingAndUnpacking( () => RandomList(this.randomDateTimeWithTimeZoneOffset) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackLocalDateTimeWithTimeZoneOffset()
		 public virtual void ShouldPackLocalDateTimeWithTimeZoneOffset()
		 {
			  DateTime localDateTime = new DateTime( 2015, 3, 23, 19, 15, 59, 10 );
			  ZoneOffset offset = ZoneOffset.ofHoursMinutes( -5, -15 );
			  ZonedDateTime zonedDateTime = ZonedDateTime.of( localDateTime, offset );

			  PackedOutputArray packedOutput = Pack( datetime( zonedDateTime ) );
			  ByteBuffer buffer = ByteBuffer.wrap( packedOutput.Bytes() );

			  buffer.Short; // skip struct header
			  assertEquals( INT_32, buffer.get() );
			  assertEquals( localDateTime.toEpochSecond( UTC ), buffer.Int );
			  assertEquals( localDateTime.Nano, buffer.get() );
			  assertEquals( INT_16, buffer.get() );
			  assertEquals( offset.TotalSeconds, buffer.Short );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPackLocalDateTimeWithTimeZoneId()
		 public virtual void ShouldPackLocalDateTimeWithTimeZoneId()
		 {
			  DateTime localDateTime = new DateTime( 1999, 12, 30, 9, 49, 20, 999999999 );
			  ZoneId zoneId = ZoneId.of( "Europe/Stockholm" );
			  ZonedDateTime zonedDateTime = ZonedDateTime.of( localDateTime, zoneId );

			  PackedOutputArray packedOutput = Pack( datetime( zonedDateTime ) );
			  ByteBuffer buffer = ByteBuffer.wrap( packedOutput.Bytes() );

			  buffer.Short; // skip struct header
			  assertEquals( INT_32, buffer.get() );
			  assertEquals( localDateTime.toEpochSecond( UTC ), buffer.Int );
			  assertEquals( INT_32, buffer.get() );
			  assertEquals( localDateTime.Nano, buffer.Int );
			  buffer.Short; // skip zoneId string header
			  sbyte[] zoneIdBytes = new sbyte[zoneId.Id.getBytes( UTF_8 ).length];
			  buffer.get( zoneIdBytes );
			  assertEquals( zoneId.Id, StringHelper.NewString( zoneIdBytes, UTF_8 ) );
		 }

		 private static void TestPackingAndUnpacking<T>( System.Func<T> randomValueGenerator ) where T : Neo4Net.Values.AnyValue
		 {
			  TestPackingAndUnpacking( index => randomValueGenerator() );
		 }

		 private static void TestPackingAndUnpacking<T>( System.Func<int, T> randomValueGenerator ) where T : Neo4Net.Values.AnyValue
		 {
			  IntStream.range( 0, RANDOM_VALUES_TO_TEST ).mapToObj( randomValueGenerator.apply ).forEach(originalValue =>
			  {
						  T unpackedValue = PackAndUnpack( originalValue );
						  assertEquals( originalValue, unpackedValue );
			  });
		 }

		 private void TestPackingPointsWithWrongDimensions( int dimensions )
		 {
			  PointValue point = RandomPoint( 0, dimensions );
			  try
			  {
					Pack( point );
					fail( "Exception expected" );
			  }
			  catch ( System.ArgumentException )
			  {
			  }
		 }

		 private static T PackAndUnpack<T>( T value ) where T : Neo4Net.Values.AnyValue
		 {
			  return Unpack( Pack( value ) );
		 }

		 private static PackedOutputArray Pack( AnyValue value )
		 {
			  try
			  {
					Neo4NetPackV2 Neo4NetPack = new Neo4NetPackV2();
					PackedOutputArray output = new PackedOutputArray();
					Neo4Net.Bolt.messaging.Neo4NetPack_Packer packer = Neo4NetPack.NewPacker( output );
					packer.Pack( value );
					return output;
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static <T extends Neo4Net.values.AnyValue> T unpack(Neo4Net.bolt.v1.packstream.PackedOutputArray output)
		 private static T Unpack<T>( PackedOutputArray output ) where T : Neo4Net.Values.AnyValue
		 {
			  try
			  {
					Neo4NetPackV2 Neo4NetPack = new Neo4NetPackV2();
					PackedInputArray input = new PackedInputArray( output.Bytes() );
					Neo4Net.Bolt.messaging.Neo4NetPack_Unpacker unpacker = Neo4NetPack.NewUnpacker( input );
					AnyValue unpack = unpacker.Unpack();
					return ( T ) unpack;
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private ListValue RandomList<T>( System.Func<T> randomValueGenerator ) where T : Neo4Net.Values.AnyValue
		 {
			  return RandomList( index => randomValueGenerator() );
		 }

		 private ListValue RandomList<T>( System.Func<int, T> randomValueGenerator ) where T : Neo4Net.Values.AnyValue
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  AnyValue[] values = Random.ints( RANDOM_LISTS_TO_TEST, 1, RANDOM_LIST_MAX_SIZE ).mapToObj( randomValueGenerator.apply ).toArray( AnyValue[]::new );

			  return list( values );
		 }

		 private PointValue RandomPoint2D( int index )
		 {
			  return RandomPoint( index, 2 );
		 }

		 private PointValue RandomPoint3D( int index )
		 {
			  return RandomPoint( index, 3 );
		 }

		 private PointValue RandomPoint( int index, int dimension )
		 {
			  CoordinateReferenceSystem crs;
			  if ( index % 2 == 0 )
			  {
					crs = dimension == 2 ? WGS84 : WGS84_3D;
			  }
			  else
			  {
					crs = dimension == 2 ? Cartesian : Cartesian_3D;
			  }

			  return unsafePointValue( crs, Random.doubles( dimension, double.Epsilon, double.MaxValue ).toArray() );
		 }

		 private DurationValue RandomDuration()
		 {
			  return Random.randomValues().nextDuration();
		 }

		 private DurationValue RandomPeriod()
		 {
			  return Random.randomValues().nextPeriod();
		 }

		 private DateValue RandomDate()
		 {
			  return Random.randomValues().nextDateValue();
		 }

		 private LocalTimeValue RandomLocalTime()
		 {
			  return Random.randomValues().nextLocalTimeValue();
		 }

		 private TimeValue RandomTime()
		 {
			  return Random.randomValues().nextTimeValue();
		 }

		 private LocalDateTimeValue RandomLocalDateTime()
		 {
			  return Random.randomValues().nextLocalDateTimeValue();
		 }

		 private DateTimeValue RandomDateTimeWithTimeZoneName()
		 {
			  return Random.randomValues().nextDateTimeValue(RandomZoneIdWithName());
		 }

		 private DateTimeValue RandomDateTimeWithTimeZoneOffset()
		 {
			  return Random.randomValues().nextDateTimeValue(RandomZoneOffset());
		 }

		 private ZoneOffset RandomZoneOffset()
		 {
			  return ZoneOffset.ofTotalSeconds( Random.Next( ZoneOffset.MIN.TotalSeconds, ZoneOffset.MAX.TotalSeconds ) );
		 }

		 private ZoneId RandomZoneIdWithName()
		 {
			  string timeZoneName = _timeZoneNames[Random.Next( _timeZoneNames.Length )];
			  return ZoneId.of( timeZoneName );
		 }
	}

}