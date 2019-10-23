using System;
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
namespace Neo4Net.Kernel.Impl.Api.state
{
	using MutableList = org.eclipse.collections.api.list.MutableList;
	using Pair = org.eclipse.collections.api.tuple.Pair;
	using ObjectLongPair = org.eclipse.collections.api.tuple.primitive.ObjectLongPair;
	using FastList = org.eclipse.collections.impl.list.mutable.FastList;
	using Tuples = org.eclipse.collections.impl.tuple.Tuples;
	using AfterAll = org.junit.jupiter.api.AfterAll;
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using DynamicTest = org.junit.jupiter.api.DynamicTest;
	using Test = org.junit.jupiter.api.Test;
	using TestFactory = org.junit.jupiter.api.TestFactory;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using Point = Neo4Net.GraphDb.Spatial.Point;
	using CachingOffHeapBlockAllocator = Neo4Net.Kernel.impl.util.collection.CachingOffHeapBlockAllocator;
	using OffHeapMemoryAllocator = Neo4Net.Kernel.impl.util.collection.OffHeapMemoryAllocator;
	using LocalMemoryTracker = Neo4Net.Memory.LocalMemoryTracker;
	using IMemoryAllocationTracker = Neo4Net.Memory.IMemoryAllocationTracker;
	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using NoValue = Neo4Net.Values.Storable.NoValue;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ArrayUtils.EMPTY_BOOLEAN_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ArrayUtils.EMPTY_BYTE_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ArrayUtils.EMPTY_CHAR_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ArrayUtils.EMPTY_DOUBLE_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ArrayUtils.EMPTY_FLOAT_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ArrayUtils.EMPTY_INT_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ArrayUtils.EMPTY_LONG_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ArrayUtils.EMPTY_SHORT_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ArrayUtils.EMPTY_STRING_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.collections.impl.tuple.primitive.PrimitiveTuples.pair;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.pointValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.stringValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class AppendOnlyValuesContainerTest
	internal class AppendOnlyValuesContainerTest
	{
		private bool InstanceFieldsInitialized = false;

		public AppendOnlyValuesContainerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_container = new AppendOnlyValuesContainer( new OffHeapMemoryAllocator( _memoryTracker, _blockAllocator ) );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.RandomRule rnd;
		 private RandomRule _rnd;

		 private readonly CachingOffHeapBlockAllocator _blockAllocator = new CachingOffHeapBlockAllocator();
		 private readonly IMemoryAllocationTracker _memoryTracker = new LocalMemoryTracker();

		 private AppendOnlyValuesContainer _container;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterAll static void afterAll()
		 internal static void AfterAll()
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void afterEach()
		 internal virtual void AfterEach()
		 {
			  _container.close();
			  assertEquals( 0, _memoryTracker.usedDirectMemory(), "Got memory leak" );
			  _blockAllocator.release();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @TestFactory Stream<org.junit.jupiter.api.DynamicTest> addGet()
		 internal virtual Stream<DynamicTest> AddGet()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.eclipse.collections.api.tuple.Pair<String, org.Neo4Net.values.storable.Value[]>> inputs = asList(testInput("NoValue", System.Func.identity(), org.Neo4Net.values.storable.NoValue.NO_VALUE), testInput("Boolean", org.Neo4Net.values.storable.Values::booleanValue, true, false, true, false), testInput("BooleanArray", org.Neo4Net.values.storable.Values::booleanArray, new boolean[] {false, true, false}, EMPTY_BOOLEAN_ARRAY), testInput("Byte", org.Neo4Net.values.storable.Values::byteValue, (byte) 0, (byte) 1, (byte) -1, Byte.MIN_VALUE, Byte.MAX_VALUE), testInput("ByteArray", org.Neo4Net.values.storable.Values::byteArray, new byte[] {(byte) 0, (byte) 1, (byte) -1, Byte.MIN_VALUE, Byte.MAX_VALUE}, EMPTY_BYTE_ARRAY), testInput("Short", org.Neo4Net.values.storable.Values::shortValue, (short) 0, (short) 1, (short) -1, Short.MIN_VALUE, Short.MAX_VALUE), testInput("ShortArray", org.Neo4Net.values.storable.Values::shortArray, new short[] {(short) 0, (short) 1, (short) -1, Short.MIN_VALUE, Short.MAX_VALUE}, EMPTY_SHORT_ARRAY), testInput("Char", org.Neo4Net.values.storable.Values::charValue, 'a', '\uFFFF', '∂', '©'), testInput("CharArray", org.Neo4Net.values.storable.Values::charArray, new char[] {'a', '\uFFFF', '∂', '©'}, EMPTY_CHAR_ARRAY), testInput("Int", org.Neo4Net.values.storable.Values::intValue, 0, 1, -1, Integer.MIN_VALUE, Integer.MAX_VALUE), testInput("IntArray", org.Neo4Net.values.storable.Values::intArray, new int[] {0, 1, -1, Integer.MIN_VALUE, Integer.MAX_VALUE}, EMPTY_INT_ARRAY), testInput("Long", org.Neo4Net.values.storable.Values::longValue, 0L, 1L, -1L, Long.MIN_VALUE, Long.MAX_VALUE), testInput("LongArray", org.Neo4Net.values.storable.Values::longArray, new long[] {0L, 1L, -1L, Long.MIN_VALUE, Long.MAX_VALUE}, EMPTY_LONG_ARRAY), testInput("Double", org.Neo4Net.values.storable.Values::doubleValue, 0.0, 1.0, -1.0, Double.MIN_VALUE, Double.MAX_VALUE, Double.NEGATIVE_INFINITY, Double.POSITIVE_INFINITY), testInput("DoubleArray", org.Neo4Net.values.storable.Values::doubleArray, new double[] {0.0, 1.0, -1.0, Double.MIN_VALUE, Double.MAX_VALUE, Double.NEGATIVE_INFINITY, Double.POSITIVE_INFINITY}, EMPTY_DOUBLE_ARRAY), testInput("Float", org.Neo4Net.values.storable.Values::floatValue, 0.0f, 1.0f, -1.0f, Float.MIN_VALUE, Float.MAX_VALUE, Float.NEGATIVE_INFINITY, Float.POSITIVE_INFINITY), testInput("FloatArray", org.Neo4Net.values.storable.Values::floatArray, new float[] {0.0f, 1.0f, -1.0f, Float.MIN_VALUE, Float.MAX_VALUE, Float.NEGATIVE_INFINITY, Float.POSITIVE_INFINITY}, EMPTY_FLOAT_ARRAY), testInput("String", org.Neo4Net.values.storable.Values::stringValue, "", "x", "foobar"), testInput("StringArray", org.Neo4Net.values.storable.Values::stringArray, new String[] {"", "x", "foobar"}, EMPTY_STRING_ARRAY), testInput("Point", input -> pointValue(input.getOne(), input.getTwo()), org.eclipse.collections.impl.tuple.Tuples.pair(org.Neo4Net.values.storable.CoordinateReferenceSystem.WGS84, new double[] {1.0, 2.0}), org.eclipse.collections.impl.tuple.Tuples.pair(org.Neo4Net.values.storable.CoordinateReferenceSystem.WGS84_3D, new double[] {1.0, 2.0, 3.0}), org.eclipse.collections.impl.tuple.Tuples.pair(org.Neo4Net.values.storable.CoordinateReferenceSystem.Cartesian, new double[] {1.0, 2.0}), org.eclipse.collections.impl.tuple.Tuples.pair(org.Neo4Net.values.storable.CoordinateReferenceSystem.Cartesian_3D, new double[] {1.0, 2.0, 3.0})), testInput("PointArray", org.Neo4Net.values.storable.Values::pointArray, new org.Neo4Net.GraphDb.Spatial.Point[] { pointValue(org.Neo4Net.values.storable.CoordinateReferenceSystem.WGS84, 1.0, 2.0), pointValue(org.Neo4Net.values.storable.CoordinateReferenceSystem.WGS84_3D, 1.0, 2.0, 3.0), pointValue(org.Neo4Net.values.storable.CoordinateReferenceSystem.Cartesian, 1.0, 2.0), pointValue(org.Neo4Net.values.storable.CoordinateReferenceSystem.Cartesian_3D, 1.0, 2.0, 3.0) }, new org.Neo4Net.GraphDb.Spatial.Point[0]), testInput("Duration", org.Neo4Net.values.storable.Values::durationValue, (java.time.temporal.TemporalAmount) java.time.Duration.parse("P2DT3H4M"), java.time.Period.parse("P1Y2M3W4D")), testInput("DurationArray", org.Neo4Net.values.storable.Values::durationArray, new java.time.temporal.TemporalAmount[] {java.time.Duration.parse("P2DT3H4M"), java.time.Period.parse("P1Y2M3W4D")}, new java.time.temporal.TemporalAmount[0]), testInput("Date", org.Neo4Net.values.storable.DateValue::date, java.time.LocalDate.now(), java.time.LocalDate.parse("1977-05-25")), testInput("DateArray", org.Neo4Net.values.storable.Values::dateArray, new java.time.LocalDate[] {java.time.LocalDate.now(), java.time.LocalDate.parse("1977-05-25")}, new java.time.LocalDate[0]), testInput("Time", org.Neo4Net.values.storable.TimeValue::time, java.time.OffsetTime.now(), java.time.OffsetTime.parse("19:28:34.123+02:00")), testInput("TimeArray", org.Neo4Net.values.storable.Values::timeArray, new java.time.OffsetTime[] {java.time.OffsetTime.now(), java.time.OffsetTime.parse("19:28:34.123+02:00")}, new java.time.OffsetTime[0]), testInput("LocalTime", org.Neo4Net.values.storable.LocalTimeValue::localTime, java.time.LocalTime.now(), java.time.LocalTime.parse("19:28:34.123")), testInput("LocalTimeArray", org.Neo4Net.values.storable.Values::localTimeArray, new java.time.LocalTime[] {java.time.LocalTime.now(), java.time.LocalTime.parse("19:28:34.123")}, new java.time.LocalTime[0]), testInput("LocalDateTime", org.Neo4Net.values.storable.LocalDateTimeValue::localDateTime, java.time.LocalDateTime.now(), java.time.LocalDateTime.parse("1956-10-04T19:28:34.123")), testInput("LocalDateTimeArray", org.Neo4Net.values.storable.Values::localDateTimeArray, new java.time.LocalDateTime[] {java.time.LocalDateTime.now(), java.time.LocalDateTime.parse("1956-10-04T19:28:34.123")}, new java.time.LocalDateTime[0]), testInput("DateTime", org.Neo4Net.values.storable.DateTimeValue::datetime, java.time.ZonedDateTime.now(), java.time.ZonedDateTime.parse("1956-10-04T19:28:34.123+01:00[Europe/Paris]"), java.time.ZonedDateTime.parse("1956-10-04T19:28:34.123+01:15"), java.time.ZonedDateTime.parse("2018-09-13T16:12:16.12345+14:00[Pacific/Kiritimati]"), java.time.ZonedDateTime.parse("2018-09-13T16:12:16.12345-12:00[Etc/GMT+12]"), java.time.ZonedDateTime.parse("2018-09-13T16:12:16.12345-18:00"), java.time.ZonedDateTime.parse("2018-09-13T16:12:16.12345+18:00")), testInput("DateTimeArray", org.Neo4Net.values.storable.Values::dateTimeArray, new java.time.ZonedDateTime[] { java.time.ZonedDateTime.parse("1956-10-04T19:28:34.123+01:00[Europe/Paris]"), java.time.ZonedDateTime.parse("1956-10-04T19:28:34.123+01:15"), java.time.ZonedDateTime.parse("2018-09-13T16:12:16.12345+14:00[Pacific/Kiritimati]"), java.time.ZonedDateTime.parse("2018-09-13T16:12:16.12345-12:00[Etc/GMT+12]"), java.time.ZonedDateTime.parse("2018-09-13T16:12:16.12345-18:00"), java.time.ZonedDateTime.parse("2018-09-13T16:12:16.12345+18:00") }, new java.time.ZonedDateTime[0]));
			  IList<Pair<string, Value[]>> inputs = new IList<Pair<string, Value[]>> { TestInput( "NoValue", System.Func.identity(), NoValue.NO_VALUE ), TestInput("Boolean", Values.booleanValue, true, false, true, false), TestInput("BooleanArray", Values.booleanArray, new bool[] { false, true, false }, EMPTY_BOOLEAN_ARRAY), TestInput("Byte", Values.byteValue, (sbyte) 0, (sbyte) 1, (sbyte) -1, sbyte.MinValue, sbyte.MaxValue), TestInput("ByteArray", Values.byteArray, new sbyte[] { (sbyte) 0, (sbyte) 1, (sbyte) -1, sbyte.MinValue, sbyte.MaxValue }, EMPTY_BYTE_ARRAY), TestInput("Short", Values.shortValue, (short) 0, (short) 1, (short) -1, short.MinValue, short.MaxValue), TestInput("ShortArray", Values.shortArray, new short[] { (short) 0, (short) 1, (short) -1, short.MinValue, short.MaxValue }, EMPTY_SHORT_ARRAY), TestInput("Char", Values.charValue, 'a', '\uFFFF', '∂', '©'), TestInput("CharArray", Values.charArray, new char[] { 'a', '\uFFFF', '∂', '©' }, EMPTY_CHAR_ARRAY), TestInput("Int", Values.intValue, 0, 1, -1, int.MinValue, int.MaxValue), TestInput("IntArray", Values.intArray, new int[] { 0, 1, -1, int.MinValue, int.MaxValue }, EMPTY_INT_ARRAY), TestInput("Long", Values.longValue, 0L, 1L, -1L, long.MinValue, long.MaxValue), TestInput("LongArray", Values.longArray, new long[] { 0L, 1L, -1L, long.MinValue, long.MaxValue }, EMPTY_LONG_ARRAY), TestInput("Double", Values.doubleValue, 0.0, 1.0, -1.0, double.Epsilon, double.MaxValue, double.NegativeInfinity, double.PositiveInfinity), TestInput("DoubleArray", Values.doubleArray, new double[] { 0.0, 1.0, -1.0, double.Epsilon, double.MaxValue, double.NegativeInfinity, double.PositiveInfinity }, EMPTY_DOUBLE_ARRAY), TestInput("Float", Values.floatValue, 0.0f, 1.0f, -1.0f, float.Epsilon, float.MaxValue, float.NegativeInfinity, float.PositiveInfinity), TestInput("FloatArray", Values.floatArray, new float[] { 0.0f, 1.0f, -1.0f, float.Epsilon, float.MaxValue, float.NegativeInfinity, float.PositiveInfinity }, EMPTY_FLOAT_ARRAY), TestInput("String", Values.stringValue, "", "x", "foobar"), TestInput("StringArray", Values.stringArray, new string[] { "", "x", "foobar" }, EMPTY_STRING_ARRAY), TestInput("Point", input => pointValue(input.One, input.Two), Tuples.pair(CoordinateReferenceSystem.WGS84, new double[] { 1.0, 2.0 }), Tuples.pair(CoordinateReferenceSystem.WGS84_3D, new double[] { 1.0, 2.0, 3.0 }), Tuples.pair(CoordinateReferenceSystem.Cartesian, new double[] { 1.0, 2.0 }), Tuples.pair(CoordinateReferenceSystem.Cartesian_3D, new double[] { 1.0, 2.0, 3.0 })), TestInput("PointArray", Values.pointArray, new Point[] { pointValue(CoordinateReferenceSystem.WGS84, 1.0, 2.0), pointValue(CoordinateReferenceSystem.WGS84_3D, 1.0, 2.0, 3.0), pointValue(CoordinateReferenceSystem.Cartesian, 1.0, 2.0), pointValue(CoordinateReferenceSystem.Cartesian_3D, 1.0, 2.0, 3.0) }, new Point[0]), TestInput("Duration", Values.durationValue, (TemporalAmount) Duration.parse("P2DT3H4M"), Period.parse("P1Y2M3W4D")), TestInput("DurationArray", Values.durationArray, new TemporalAmount[] { Duration.parse("P2DT3H4M"), Period.parse("P1Y2M3W4D") }, new TemporalAmount[0]), TestInput("Date", DateValue.date, LocalDate.now(), LocalDate.parse("1977-05-25")), TestInput("DateArray", Values.dateArray, new LocalDate[] { LocalDate.now(), LocalDate.parse("1977-05-25") }, new LocalDate[0]), TestInput("Time", TimeValue.time, OffsetTime.now(), OffsetTime.parse("19:28:34.123+02:00")), TestInput("TimeArray", Values.timeArray, new OffsetTime[] { OffsetTime.now(), OffsetTime.parse("19:28:34.123+02:00") }, new OffsetTime[0]), TestInput("LocalTime", LocalTimeValue.localTime, LocalTime.now(), LocalTime.parse("19:28:34.123")), TestInput("LocalTimeArray", Values.localTimeArray, new LocalTime[] { LocalTime.now(), LocalTime.parse("19:28:34.123") }, new LocalTime[0]), TestInput("LocalDateTime", LocalDateTimeValue.localDateTime, DateTime.Now, DateTime.Parse("1956-10-04T19:28:34.123")), TestInput("LocalDateTimeArray", Values.localDateTimeArray, new DateTime[] { DateTime.Now, DateTime.Parse("1956-10-04T19:28:34.123") }, new DateTime[0]), TestInput("DateTime", DateTimeValue.datetime, ZonedDateTime.now(), ZonedDateTime.parse("1956-10-04T19:28:34.123+01:00[Europe/Paris]"), ZonedDateTime.parse("1956-10-04T19:28:34.123+01:15"), ZonedDateTime.parse("2018-09-13T16:12:16.12345+14:00[Pacific/Kiritimati]"), ZonedDateTime.parse("2018-09-13T16:12:16.12345-12:00[Etc/GMT+12]"), ZonedDateTime.parse("2018-09-13T16:12:16.12345-18:00"), ZonedDateTime.parse("2018-09-13T16:12:16.12345+18:00")), TestInput("DateTimeArray", Values.dateTimeArray, new ZonedDateTime[] { ZonedDateTime.parse("1956-10-04T19:28:34.123+01:00[Europe/Paris]"), ZonedDateTime.parse("1956-10-04T19:28:34.123+01:15"), ZonedDateTime.parse("2018-09-13T16:12:16.12345+14:00[Pacific/Kiritimati]"), ZonedDateTime.parse("2018-09-13T16:12:16.12345-12:00[Etc/GMT+12]"), ZonedDateTime.parse("2018-09-13T16:12:16.12345-18:00"), ZonedDateTime.parse("2018-09-13T16:12:16.12345+18:00") }, new ZonedDateTime[0]) };

			  return DynamicTest.stream(inputs.GetEnumerator(), Pair.getOne, pair =>
			  {
				Value[] values = pair.Two;
				long[] refs = values.Select( _container.add ).ToArray();
				for ( int i = 0; i < values.Length; i++ )
				{
					 assertEquals( values[i], _container.get( refs[i] ) );
				}
			  });
		 }

		 private static Pair<string, Value[]> TestInput<T>( string name, System.Func<T, Value> ctor, params T[] values )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Tuples.pair( name, java.util.values.Select( ctor ).ToArray( Value[]::new ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getFailsOnInvalidRef()
		 internal virtual void getFailsOnInvalidRef()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long ref = container.add(intValue(42));
			  long @ref = _container.add( intValue( 42 ) );
			  _container.get( @ref );
			  assertThrows( typeof( System.ArgumentException ), () => _container.get(128L), "invalid chunk offset" );
			  assertThrows( typeof( System.ArgumentException ), () => _container.get(1L << 32), "invalid chunk index" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void remove()
		 internal virtual void Remove()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long ref = container.add(intValue(42));
			  long @ref = _container.add( intValue( 42 ) );
			  _container.remove( @ref );
			  assertThrows( typeof( System.ArgumentException ), () => _container.get(@ref) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void valueSizeExceedsChunkSize()
		 internal virtual void ValueSizeExceedsChunkSize()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final AppendOnlyValuesContainer container2 = new AppendOnlyValuesContainer(4, new TestMemoryAllocator());
			  AppendOnlyValuesContainer container2 = new AppendOnlyValuesContainer( 4, new TestMemoryAllocator() );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long ref1 = container2.add(longValue(42));
			  long ref1 = container2.Add( longValue( 42 ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long ref2 = container2.add(stringValue("1234567890ABCDEF"));
			  long ref2 = container2.Add( stringValue( "1234567890ABCDEF" ) );

			  assertEquals( longValue( 42 ), container2.Get( ref1 ) );
			  assertEquals( stringValue( "1234567890ABCDEF" ), container2.Get( ref2 ) );

			  container2.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void close()
		 internal virtual void Close()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final AppendOnlyValuesContainer container2 = new AppendOnlyValuesContainer(4, new TestMemoryAllocator());
			  AppendOnlyValuesContainer container2 = new AppendOnlyValuesContainer( 4, new TestMemoryAllocator() );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long ref = container2.add(intValue(42));
			  long @ref = container2.Add( intValue( 42 ) );
			  container2.Close();
			  assertThrows( typeof( System.InvalidOperationException ), () => container2.Add(intValue(1)) );
			  assertThrows( typeof( System.InvalidOperationException ), () => container2.Get(@ref) );
			  assertThrows( typeof( System.InvalidOperationException ), () => container2.Remove(@ref) );
			  assertThrows( typeof( System.InvalidOperationException ), container2.close );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void randomizedTest()
		 internal virtual void RandomizedTest()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int count = 10000 + rnd.nextInt(1000);
			  int count = 10000 + _rnd.Next( 1000 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.eclipse.collections.api.tuple.primitive.ObjectLongPair<org.Neo4Net.values.storable.Value>> valueRefPairs = new java.util.ArrayList<>();
			  IList<ObjectLongPair<Value>> valueRefPairs = new List<ObjectLongPair<Value>>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.list.MutableList<org.eclipse.collections.api.tuple.primitive.ObjectLongPair<org.Neo4Net.values.storable.Value>> toRemove = new org.eclipse.collections.impl.list.mutable.FastList<>();
			  MutableList<ObjectLongPair<Value>> toRemove = new FastList<ObjectLongPair<Value>>();

			  for ( int i = 0; i < count; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.values.storable.Value value = rnd.randomValues().nextValue();
					Value value = _rnd.randomValues().nextValue();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long ref = container.add(value);
					long @ref = _container.add( value );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.tuple.primitive.ObjectLongPair<org.Neo4Net.values.storable.Value> pair = pair(value, ref);
					ObjectLongPair<Value> pair = pair( value, @ref );
					if ( _rnd.nextBoolean() )
					{
						 toRemove.add( pair );
					}
					else
					{
						 valueRefPairs.Add( pair );
					}
			  }

			  toRemove.shuffleThis( _rnd.random() );
			  foreach ( ObjectLongPair<Value> valueRefPair in toRemove )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.values.storable.Value removed = container.remove(valueRefPair.getTwo());
					Value removed = _container.remove( valueRefPair.Two );
					assertEquals( valueRefPair.One, removed );
					assertThrows( typeof( System.ArgumentException ), () => _container.remove(valueRefPair.Two) );
					assertThrows( typeof( System.ArgumentException ), () => _container.get(valueRefPair.Two) );
			  }

			  foreach ( ObjectLongPair<Value> valueRefPair in valueRefPairs )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.values.storable.Value actualValue = container.get(valueRefPair.getTwo());
					Value actualValue = _container.get( valueRefPair.Two );
					assertEquals( valueRefPair.One, actualValue );
			  }
		 }
	}

}