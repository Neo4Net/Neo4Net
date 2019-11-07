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
namespace Neo4Net.Values.utils
{
	using Test = org.junit.jupiter.api.Test;

	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using TextArray = Neo4Net.Values.Storable.TextArray;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;
	using ListValue = Neo4Net.Values.@virtual.ListValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using NodeReference = Neo4Net.Values.@virtual.NodeReference;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using PathValue = Neo4Net.Values.@virtual.PathValue;
	using RelationshipReference = Neo4Net.Values.@virtual.RelationshipReference;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.DateTimeValue.datetime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.DateValue.date;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.DurationValue.duration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.LocalDateTimeValue.localDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.LocalTimeValue.localTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.TimeValue.time;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.byteValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValues.EMPTY_MAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValues.list;

	internal class PrettyPrinterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleNodeReference()
		 internal virtual void ShouldHandleNodeReference()
		 {
			  // Given
			  NodeReference node = VirtualValues.node( 42L );
			  PrettyPrinter printer = new PrettyPrinter();

			  // When
			  node.WriteTo( printer );

			  // Then
			  assertThat( printer.Value(), equalTo("(id=42)") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleNodeValue()
		 internal virtual void ShouldHandleNodeValue()
		 {
			  // Given
			  NodeValue node = VirtualValues.nodeValue( 42L, Values.stringArray( "L1", "L2", "L3" ), Props( "foo", intValue( 42 ), "bar", list( intValue( 1337 ), stringValue( "baz" ) ) ) );
			  PrettyPrinter printer = new PrettyPrinter();

			  // When
			  node.WriteTo( printer );

			  // Then
			  assertThat( printer.Value(), equalTo("(id=42 :L1:L2:L3 {bar: [1337, \"baz\"], foo: 42})") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleNodeValueWithoutLabels()
		 internal virtual void ShouldHandleNodeValueWithoutLabels()
		 {
			  // Given
			  NodeValue node = VirtualValues.nodeValue( 42L, Values.stringArray(), Props("foo", intValue(42), "bar", list(intValue(1337), stringValue("baz"))) );
			  PrettyPrinter printer = new PrettyPrinter();

			  // When
			  node.WriteTo( printer );

			  // Then
			  assertThat( printer.Value(), equalTo("(id=42 {bar: [1337, \"baz\"], foo: 42})") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleNodeValueWithoutProperties()
		 internal virtual void ShouldHandleNodeValueWithoutProperties()
		 {
			  // Given
			  NodeValue node = VirtualValues.nodeValue( 42L, Values.stringArray( "L1", "L2", "L3" ), EMPTY_MAP );
			  PrettyPrinter printer = new PrettyPrinter();

			  // When
			  node.WriteTo( printer );

			  // Then
			  assertThat( printer.Value(), equalTo("(id=42 :L1:L2:L3)") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleNodeValueWithoutLabelsNorProperties()
		 internal virtual void ShouldHandleNodeValueWithoutLabelsNorProperties()
		 {
			  // Given
			  NodeValue node = VirtualValues.nodeValue( 42L, Values.stringArray(), EMPTY_MAP );
			  PrettyPrinter printer = new PrettyPrinter();

			  // When
			  node.WriteTo( printer );

			  // Then
			  assertThat( printer.Value(), equalTo("(id=42)") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleEdgeReference()
		 internal virtual void ShouldHandleEdgeReference()
		 {
			  // Given
			  RelationshipReference rel = VirtualValues.relationship( 42L );
			  PrettyPrinter printer = new PrettyPrinter();

			  // When
			  rel.WriteTo( printer );

			  // Then
			  assertThat( printer.Value(), equalTo("-[id=42]-") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleEdgeValue()
		 internal virtual void ShouldHandleEdgeValue()
		 {
			  // Given
			  NodeValue startNode = VirtualValues.nodeValue( 1L, Values.stringArray( "L" ), EMPTY_MAP );
			  NodeValue endNode = VirtualValues.nodeValue( 2L, Values.stringArray( "L" ), EMPTY_MAP );
			  RelationshipValue rel = VirtualValues.relationshipValue( 42L, startNode, endNode, stringValue( "R" ), Props( "foo", intValue( 42 ), "bar", list( intValue( 1337 ), stringValue( "baz" ) ) ) );
			  PrettyPrinter printer = new PrettyPrinter();

			  // When
			  rel.WriteTo( printer );

			  // Then
			  assertThat( printer.Value(), equalTo("-[id=42 :R {bar: [1337, \"baz\"], foo: 42}]-") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleEdgeValueWithoutProperties()
		 internal virtual void ShouldHandleEdgeValueWithoutProperties()
		 {
			  NodeValue startNode = VirtualValues.nodeValue( 1L, Values.stringArray( "L" ), EMPTY_MAP );
			  NodeValue endNode = VirtualValues.nodeValue( 2L, Values.stringArray( "L" ), EMPTY_MAP );
			  RelationshipValue rel = VirtualValues.relationshipValue( 42L, startNode, endNode, stringValue( "R" ), EMPTY_MAP );
			  PrettyPrinter printer = new PrettyPrinter();

			  // When
			  rel.WriteTo( printer );

			  // Then
			  assertThat( printer.Value(), equalTo("-[id=42 :R]-") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleEdgeValueWithoutLabelsNorProperties()
		 internal virtual void ShouldHandleEdgeValueWithoutLabelsNorProperties()
		 {
			  // Given
			  NodeValue node = VirtualValues.nodeValue( 42L, Values.stringArray(), EMPTY_MAP );
			  PrettyPrinter printer = new PrettyPrinter();

			  // When
			  node.WriteTo( printer );

			  // Then
			  assertThat( printer.Value(), equalTo("(id=42)") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandlePaths()
		 internal virtual void ShouldHandlePaths()
		 {
			  // Given
			  NodeValue startNode = VirtualValues.nodeValue( 1L, Values.stringArray( "L" ), EMPTY_MAP );
			  NodeValue endNode = VirtualValues.nodeValue( 2L, Values.stringArray( "L" ), EMPTY_MAP );
			  RelationshipValue rel = VirtualValues.relationshipValue( 42L, startNode, endNode, stringValue( "R" ), EMPTY_MAP );
			  PathValue path = VirtualValues.path( new NodeValue[]{ startNode, endNode }, new RelationshipValue[]{ rel } );
			  PrettyPrinter printer = new PrettyPrinter();

			  // When
			  path.WriteTo( printer );

			  // Then
			  assertThat( printer.Value(), equalTo("(id=1 :L)-[id=42 :R]->(id=2 :L)") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleMaps()
		 internal virtual void ShouldHandleMaps()
		 {
			  // Given
			  PrettyPrinter printer = new PrettyPrinter();
			  MapValue mapValue = Props( "k1", intValue( 42 ) );

			  // When
			  mapValue.WriteTo( printer );

			  // Then
			  assertThat( printer.Value(), equalTo("{k1: 42}") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleLists()
		 internal virtual void ShouldHandleLists()
		 {
			  // Given
			  PrettyPrinter printer = new PrettyPrinter();
			  ListValue list = VirtualValues.list( stringValue( "foo" ), byteValue( ( sbyte ) 42 ) );

			  // When
			  list.WriteTo( printer );

			  // Then
			  assertThat( printer.Value(), equalTo("[\"foo\", 42]") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleArrays()
		 internal virtual void ShouldHandleArrays()
		 {
			  // Given
			  PrettyPrinter printer = new PrettyPrinter();
			  TextArray array = Values.stringArray( "a", "b", "c" );

			  // When
			  array.WriteTo( printer );

			  // Then
			  assertThat( printer.Value(), equalTo("[\"a\", \"b\", \"c\"]") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleBooleans()
		 internal virtual void ShouldHandleBooleans()
		 {
			  // Given
			  Value array = Values.booleanArray( new bool[]{ true, false, true } );
			  PrettyPrinter printer = new PrettyPrinter();

			  // When
			  array.WriteTo( printer );

			  // Then
			  assertThat( printer.Value(), equalTo("[true, false, true]") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleByteArrays()
		 internal virtual void ShouldHandleByteArrays()
		 {
			  // Given
			  Value array = Values.byteArray( new sbyte[]{ 2, 3, 42 } );
			  PrettyPrinter printer = new PrettyPrinter();

			  // When
			  array.WriteTo( printer );

			  // Then
			  assertThat( printer.Value(), equalTo("[2, 3, 42]") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleNull()
		 internal virtual void ShouldHandleNull()
		 {
			  // Given
			  PrettyPrinter printer = new PrettyPrinter();

			  // When
			  Values.NO_VALUE.WriteTo( printer );

			  // Then
			  assertThat( printer.Value(), equalTo("<null>") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandlePoints()
		 internal virtual void ShouldHandlePoints()
		 {
			  // Given
			  PointValue pointValue = Values.pointValue( CoordinateReferenceSystem.Cartesian, 11d, 12d );
			  PrettyPrinter printer = new PrettyPrinter();

			  // When
			  pointValue.WriteTo( printer );

			  // Then
			  assertThat( printer.Value(), equalTo("{geometry: {type: \"Point\", coordinates: [11.0, 12.0], " + "crs: {type: link, properties: " + "{href: \"http://spatialreference.org/ref/sr-org/7203/\", code: " + "7203}}}}") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeAbleToUseAnyQuoteMark()
		 internal virtual void ShouldBeAbleToUseAnyQuoteMark()
		 {
			  // Given
			  TextValue hello = stringValue( "(ツ)" );
			  PrettyPrinter printer = new PrettyPrinter( "__" );

			  // When
			  hello.WriteTo( printer );

			  // Then
			  assertThat( printer.Value(), equalTo("__(ツ)__") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleDuration()
		 internal virtual void ShouldHandleDuration()
		 {
			  DurationValue duration = duration( 12, 45, 90, 9911 );
			  PrettyPrinter printer = new PrettyPrinter();

			  duration.WriteTo( printer );

			  assertEquals( "{duration: {months: 12, days: 45, seconds: 90, nanos: 9911}}", printer.Value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleDate()
		 internal virtual void ShouldHandleDate()
		 {
			  DateValue date = date( 1991, 9, 24 );
			  PrettyPrinter printer = new PrettyPrinter();

			  date.WriteTo( printer );

			  assertEquals( "{date: \"1991-09-24\"}", printer.Value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleLocalTime()
		 internal virtual void ShouldHandleLocalTime()
		 {
			  LocalTimeValue localTime = localTime( 18, 39, 24, 111222777 );
			  PrettyPrinter printer = new PrettyPrinter();

			  localTime.WriteTo( printer );

			  assertEquals( "{localTime: \"18:39:24.111222777\"}", printer.Value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleTime()
		 internal virtual void ShouldHandleTime()
		 {
			  TimeValue time = time( 11, 19, 11, 123456789, ZoneOffset.ofHoursMinutes( -9, -30 ) );
			  PrettyPrinter printer = new PrettyPrinter();

			  time.WriteTo( printer );

			  assertEquals( "{time: \"11:19:11.123456789-09:30\"}", printer.Value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleLocalDateTime()
		 internal virtual void ShouldHandleLocalDateTime()
		 {
			  LocalDateTimeValue localDateTime = localDateTime( 2015, 8, 8, 8, 40, 29, 999888111 );
			  PrettyPrinter printer = new PrettyPrinter();

			  localDateTime.WriteTo( printer );

			  assertEquals( "{localDateTime: \"2015-08-08T08:40:29.999888111\"}", printer.Value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleDateTimeWithTimeZoneId()
		 internal virtual void ShouldHandleDateTimeWithTimeZoneId()
		 {
			  DateTimeValue datetime = datetime( 2045, 2, 7, 12, 0x0, 40, 999888999, "Europe/London" );
			  PrettyPrinter printer = new PrettyPrinter();

			  datetime.WriteTo( printer );

			  assertEquals( "{datetime: \"2045-02-07T12:00:40.999888999Z[Europe/London]\"}", printer.Value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleDateTimeWithTimeZoneOffset()
		 internal virtual void ShouldHandleDateTimeWithTimeZoneOffset()
		 {
			  DateTimeValue datetime = datetime( 1988, 4, 19, 10, 12, 59, 112233445, ZoneOffset.ofHoursMinutes( 3, 15 ) );
			  PrettyPrinter printer = new PrettyPrinter();

			  datetime.WriteTo( printer );

			  assertEquals( "{datetime: \"1988-04-19T10:12:59.112233445+03:15\"}", printer.Value() );
		 }

		 private MapValue Props( params object[] keyValue )
		 {
			  string[] keys = new string[keyValue.Length / 2];
			  AnyValue[] values = new AnyValue[keyValue.Length / 2];
			  for ( int i = 0; i < keyValue.Length; i++ )
			  {
					if ( i % 2 == 0 )
					{
						 keys[i / 2] = ( string ) keyValue[i];
					}
					else
					{
						 values[i / 2] = ( AnyValue ) keyValue[i];
					}
			  }
			  return VirtualValues.map( keys, values );
		 }
	}

}