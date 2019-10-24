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
namespace Neo4Net.Cypher.Internal.codegen
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Path = Neo4Net.GraphDb.Path;
	using Point = Neo4Net.GraphDb.Spatial.Point;
	using MapUtil = Neo4Net.Collections.Helpers.MapUtil;
	using EmbeddedProxySPI = Neo4Net.Kernel.impl.core.EmbeddedProxySPI;
	using NodeProxy = Neo4Net.Kernel.impl.core.NodeProxy;
	using RelationshipProxy = Neo4Net.Kernel.impl.core.RelationshipProxy;
	using AnyValue = Neo4Net.Values.AnyValue;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using LongArray = Neo4Net.Values.Storable.LongArray;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using Values = Neo4Net.Values.Storable.Values;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;
	using ListValue = Neo4Net.Values.@virtual.ListValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using PathValue = Neo4Net.Values.@virtual.PathValue;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.byteValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.doubleValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.floatValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.shortValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.stringArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.EMPTY_MAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.relationshipValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.list;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.nodeValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.path;

	public class ParameterConverterTest
	{
		 private ParameterConverter _converter = new ParameterConverter( mock( typeof( EmbeddedProxySPI ) ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  EmbeddedProxySPI manager = mock( typeof( EmbeddedProxySPI ) );
			  when( manager.NewNodeProxy( anyLong() ) ).thenAnswer(invocationOnMock =>
			  {
				long id = invocationOnMock.getArgument( 0 );
				NodeProxy mock = mock( typeof( NodeProxy ) );
				when( mock.Id ).thenReturn( id );
				return mock;
			  });
			  when( manager.NewRelationshipProxy( anyLong() ) ).thenAnswer(invocationOnMock =>
			  {
				long id = invocationOnMock.getArgument( 0 );
				RelationshipProxy mock = mock( typeof( RelationshipProxy ) );
				when( mock.Id ).thenReturn( id );
				return mock;
			  });
			  _converter = new ParameterConverter( manager );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTurnAllIntegerTypesToLongs()
		 public virtual void ShouldTurnAllIntegerTypesToLongs()
		 {
			  AnyValue[] values = new AnyValue[]{ byteValue( ( sbyte ) 13 ), shortValue( ( short ) 13 ), intValue( 13 ), longValue( 13L ) };

			  foreach ( AnyValue val in values )
			  {
					val.WriteTo( _converter );
					object value = _converter.value();
					assertThat( value, instanceOf( typeof( Long ) ) );
					assertThat( value, equalTo( 13L ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTurnAllFloatingTypesToDoubles()
		 public virtual void ShouldTurnAllFloatingTypesToDoubles()
		 {
			  AnyValue[] values = new AnyValue[]{ floatValue( 13f ), doubleValue( 13d ) };

			  foreach ( AnyValue val in values )
			  {
					val.WriteTo( _converter );
					object value = _converter.value();
					assertThat( value, instanceOf( typeof( Double ) ) );
					assertThat( value, equalTo( 13d ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNodes()
		 public virtual void ShouldHandleNodes()
		 {
			  // Given
			  NodeValue nodeValue = nodeValue( 42L, stringArray( "L" ), EMPTY_MAP );

			  // When
			  nodeValue.WriteTo( _converter );

			  // Then
			  assertThat( _converter.value(), equalTo(VirtualValues.node(42L)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleRelationships()
		 public virtual void ShouldHandleRelationships()
		 {
			  // Given
			  RelationshipValue relValue = relationshipValue( 1L, nodeValue( 42L, stringArray( "L" ), EMPTY_MAP ), nodeValue( 42L, stringArray( "L" ), EMPTY_MAP ), stringValue( "R" ), EMPTY_MAP );

			  // When
			  relValue.WriteTo( _converter );

			  // Then
			  assertThat( _converter.value(), equalTo(VirtualValues.relationship(1L)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleBooleans()
		 public virtual void ShouldHandleBooleans()
		 {
			  TRUE.WriteTo( _converter );
			  assertThat( _converter.value(), equalTo(true) );
			  FALSE.WriteTo( _converter );
			  assertThat( _converter.value(), equalTo(false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandlePaths()
		 public virtual void ShouldHandlePaths()
		 {
			  // Given
			  NodeValue n1 = nodeValue( 42L, stringArray( "L" ), EMPTY_MAP );
			  NodeValue n2 = nodeValue( 43L, stringArray( "L" ), EMPTY_MAP );
			  PathValue p = path( new NodeValue[]{ n1, n2 }, new RelationshipValue[]{ relationshipValue( 1L, n1, n2, stringValue( "T" ), EMPTY_MAP ) } );

			  // When
			  p.WriteTo( _converter );

			  // Then
			  object value = _converter.value();
			  assertThat( value, instanceOf( typeof( Path ) ) );
			  Path path = ( Path ) value;
			  assertThat( path.Length(), equalTo(1) );
			  assertThat( path.StartNode().Id, equalTo(42L) );
			  assertThat( path.EndNode().Id, equalTo(43L) );
			  assertThat( path.Relationships().GetEnumerator().next().Id, equalTo(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandlePoints()
		 public virtual void ShouldHandlePoints()
		 {
			  // Given
			  PointValue pointValue = Values.pointValue( CoordinateReferenceSystem.WGS84, 1.0, 2.0 );

			  // When
			  pointValue.WriteTo( _converter );

			  // Then
			  object value = _converter.value();
			  assertThat( value, instanceOf( typeof( Point ) ) );
			  Point point = ( Point ) value;
			  assertThat( point.Coordinate.Coordinate[0], equalTo( 1.0 ) );
			  assertThat( point.Coordinate.Coordinate[1], equalTo( 2.0 ) );
			  assertThat( point.CRS.Code, equalTo( 4326 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDateTimeWithZoneOffset()
		 public virtual void ShouldHandleDateTimeWithZoneOffset()
		 {
			  // Given
			  DateTimeValue dvalue = DateTimeValue.datetime( 1, 2, 3, 4, 5, 6, 7, "+00:00" );

			  // When
			  dvalue.WriteTo( _converter );

			  // Then
			  object value = _converter.value();
			  assertThat( value, instanceOf( typeof( ZonedDateTime ) ) );
			  assertThat( value, equalTo( dvalue.AsObjectCopy() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDateTimeWithZoneId()
		 public virtual void ShouldHandleDateTimeWithZoneId()
		 {
			  // Given
			  DateTimeValue dvalue = DateTimeValue.datetime( 1, 2, 3, 4, 5, 6, 7, ZoneId.of( "Europe/Stockholm" ) );

			  // When
			  dvalue.WriteTo( _converter );

			  // Then
			  object value = _converter.value();
			  assertThat( value, instanceOf( typeof( ZonedDateTime ) ) );
			  assertThat( value, equalTo( dvalue.AsObjectCopy() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleLocalDateTime()
		 public virtual void ShouldHandleLocalDateTime()
		 {
			  // Given
			  LocalDateTimeValue dvalue = LocalDateTimeValue.localDateTime( 1, 2, 3, 4, 5, 6, 7 );

			  // When
			  dvalue.WriteTo( _converter );

			  // Then
			  object value = _converter.value();
			  assertThat( value, instanceOf( typeof( DateTime ) ) );
			  assertThat( value, equalTo( dvalue.AsObjectCopy() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDate()
		 public virtual void ShouldHandleDate()
		 {
			  // Given
			  DateValue dvalue = DateValue.date( 1, 2, 3 );

			  // When
			  dvalue.WriteTo( _converter );

			  // Then
			  object value = _converter.value();
			  assertThat( value, instanceOf( typeof( LocalDate ) ) );
			  assertThat( value, equalTo( dvalue.AsObjectCopy() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleTimeUTC()
		 public virtual void ShouldHandleTimeUTC()
		 {
			  // Given
			  TimeValue time = TimeValue.time( 1, 2, 3, 4, "+00:00" );

			  // When
			  time.WriteTo( _converter );

			  // Then
			  object value = _converter.value();
			  assertThat( value, instanceOf( typeof( OffsetTime ) ) );
			  assertThat( value, equalTo( time.AsObjectCopy() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleTimeWithOffset()
		 public virtual void ShouldHandleTimeWithOffset()
		 {
			  // Given
			  TimeValue time = TimeValue.time( 1, 2, 3, 4, "+01:00" );

			  // When
			  time.WriteTo( _converter );

			  // Then
			  object value = _converter.value();
			  assertThat( value, instanceOf( typeof( OffsetTime ) ) );
			  assertThat( value, equalTo( time.AsObjectCopy() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleLocalTime()
		 public virtual void ShouldHandleLocalTime()
		 {
			  // Given
			  LocalTimeValue dvalue = LocalTimeValue.localTime( 1, 2, 3, 4 );

			  // When
			  dvalue.WriteTo( _converter );

			  // Then
			  object value = _converter.value();
			  assertThat( value, instanceOf( typeof( LocalTime ) ) );
			  assertThat( value, equalTo( dvalue.AsObjectCopy() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDurations()
		 public virtual void ShouldHandleDurations()
		 {
			  // Given
			  DurationValue dvalue = DurationValue.duration( 1, 2, 3, 4 );

			  // When
			  dvalue.WriteTo( _converter );

			  // Then
			  object value = _converter.value();
			  assertThat( value, instanceOf( typeof( DurationValue ) ) );
			  assertThat( value, equalTo( dvalue.AsObjectCopy() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleLists()
		 public virtual void ShouldHandleLists()
		 {
			  // Given
			  ListValue list = list( stringValue( "foo" ), longValue( 42L ), TRUE );

			  // When
			  list.WriteTo( _converter );

			  // Then
			  assertThat( _converter.value(), equalTo(Arrays.asList("foo", 42L, true)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleArrays()
		 public virtual void ShouldHandleArrays()
		 {
			  // Given
			  LongArray longArray = Values.longArray( new long[]{ 1L, 2L, 3L } );

			  // When
			  longArray.WriteTo( _converter );

			  // Then
			  assertThat( _converter.value(), equalTo(new long[]{ 1L, 2L, 3L }) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMaps()
		 public virtual void ShouldHandleMaps()
		 {
			  // Given
			  MapValue map = map( new string[]{ "foo", "bar" }, new AnyValue[]{ longValue( 42L ), stringValue( "baz" ) } );

			  // When
			  map.WriteTo( _converter );

			  // Then
			  assertThat( _converter.value(), equalTo(MapUtil.map("foo", 42L, "bar", "baz")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleListWithMaps()
		 public virtual void ShouldHandleListWithMaps()
		 {
			  // Given
			  ListValue list = list( longValue( 42L ), map( new string[]{ "foo", "bar" }, new AnyValue[]{ longValue( 42L ), stringValue( "baz" ) } ) );

			  // When
			  list.WriteTo( _converter );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<?> converted = (java.util.List<?>) converter.value();
			  IList<object> converted = ( IList<object> ) _converter.value();
			  assertThat( converted[0], equalTo( 42L ) );
			  assertThat( converted[1], equalTo( MapUtil.map( "foo", 42L, "bar", "baz" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMapsWithLists()
		 public virtual void ShouldHandleMapsWithLists()
		 {
			  // Given
			  MapValue map = map( new string[]{ "foo", "bar" }, new AnyValue[]{ longValue( 42L ), list( stringValue( "baz" ) ) } );

			  // When
			  map.WriteTo( _converter );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Map<?,?> value = (java.util.Map<?,?>) converter.value();
			  IDictionary<object, ?> value = ( IDictionary<object, ?> ) _converter.value();
			  assertThat( value["foo"], equalTo( 42L ) );
			  assertThat( value["bar"], equalTo( singletonList( "baz" ) ) );
			  assertThat( value.Count, equalTo( 2 ) );
		 }
	}

}