﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Values
{
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using MethodSource = org.junit.jupiter.@params.provider.MethodSource;


	using Point = Org.Neo4j.Graphdb.spatial.Point;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;
	using MapValueBuilder = Org.Neo4j.Values.@virtual.MapValueBuilder;
	using NodeValue = Org.Neo4j.Values.@virtual.NodeValue;
	using PathValue = Org.Neo4j.Values.@virtual.PathValue;
	using RelationshipValue = Org.Neo4j.Values.@virtual.RelationshipValue;
	using VirtualNodeValue = Org.Neo4j.Values.@virtual.VirtualNodeValue;
	using VirtualRelationshipValue = Org.Neo4j.Values.@virtual.VirtualRelationshipValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.Cartesian;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.WGS84;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateTimeValue.datetime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DateValue.date;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.DurationValue.duration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.LocalDateTimeValue.localDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.LocalTimeValue.localTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.TimeValue.time;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.booleanArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.booleanValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.byteArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.byteValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.charArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.charValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.doubleArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.doubleValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.floatArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.floatValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.intArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.numberValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.pointArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.pointValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.shortArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.shortValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.emptyMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.fromList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.list;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.nodeValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.relationshipValue;

	internal class ValueMapperTest
	{
		 private static Stream<AnyValue> Parameters()
		 {
			  NodeValue node1 = nodeValue( 1, stringArray(), emptyMap() );
			  NodeValue node2 = nodeValue( 2, stringArray(), emptyMap() );
			  NodeValue node3 = nodeValue( 3, stringArray(), emptyMap() );
			  RelationshipValue relationship1 = relationshipValue( 100, node1, node2, stringValue( "ONE" ), emptyMap() );
			  RelationshipValue relationship2 = relationshipValue( 200, node2, node2, stringValue( "TWO" ), emptyMap() );
			  return Stream.of( node1, relationship1, path( new NodeValue[] { node1, node2, node3 }, new RelationshipValue[]{ relationship1, relationship2 } ), map( new string[] { "alpha", "beta" }, new AnyValue[] { stringValue( "one" ), numberValue( 2 ) } ), NO_VALUE, list( numberValue( 1 ), stringValue( "fine" ), node2 ), stringValue( "hello world" ), stringArray( "hello", "brave", "new", "world" ), booleanValue( false ), booleanArray( new bool[] { true, false, true } ), charValue( '\n' ), charArray( new char[] { 'h', 'e', 'l', 'l', 'o' } ), byteValue( ( sbyte ) 3 ), byteArray( new sbyte[] { 0x00, unchecked( ( sbyte ) 0x99 ), unchecked( ( sbyte ) 0xcc ) } ), shortValue( ( short ) 42 ), shortArray( new short[] { 1337, unchecked( ( short ) 0xcafe ), unchecked( ( short ) 0xbabe ) } ), intValue( 987654321 ), intArray( new int[] { 42, 11 } ), longValue( 9876543210L ), longArray( new long[] { 0xcafebabe, 0x1ee7 } ), floatValue( float.MaxValue ), floatArray( new float[] { float.NegativeInfinity, float.Epsilon } ), doubleValue( Double.MIN_NORMAL ), doubleArray( new double[] { double.PositiveInfinity, double.MaxValue } ), datetime( 2018, 1, 16, 10, 36, 43, 123456788, ZoneId.of( "Europe/Stockholm" ) ), localDateTime( 2018, 1, 16, 10, 36, 43, 123456788 ), date( 2018, 1, 16 ), time( 10, 36, 43, 123456788, ZoneOffset.ofHours( 1 ) ), localTime( 10, 36, 43, 123456788 ), duration( 399, 4, 48424, 133701337 ), pointValue( Cartesian, 11, 32 ), pointArray( new Point[] { pointValue( Cartesian, 11, 32 ), pointValue( WGS84, 13, 56 ) } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("parameters") void shouldMapToJavaObject(AnyValue value)
		 internal virtual void ShouldMapToJavaObject( AnyValue value )
		 {
			  // given
			  ValueMapper<object> mapper = new Mapper();

			  // when
			  object mapped = value.Map( mapper );

			  // then
			  assertEquals( value, ValueOf( mapped ) );
		 }

		 private static AnyValue ValueOf( object obj )
		 {
			  if ( obj is MappedGraphType )
			  {
					return ( ( MappedGraphType ) obj ).Value;
			  }
			  Value value = Values.unsafeOf( obj, true );
			  if ( value != null )
			  {
					return value;
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (obj instanceof java.util.List<?>)
			  if ( obj is IList<object> )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return fromList(((java.util.List<?>) obj).stream().map(ValueMapperTest::valueOf).collect(toList()));
					return fromList( ( ( IList<object> ) obj ).Select( ValueMapperTest.valueOf ).ToList() );
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (obj instanceof java.util.Map<?,?>)
			  if ( obj is IDictionary<object, ?> )
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.Map<String,?> map = (java.util.Map<String,?>) obj;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
					IDictionary<string, ?> map = ( IDictionary<string, ?> ) obj;
					MapValueBuilder builder = new MapValueBuilder( map.Count );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Map.Entry<String,?> entry : map.entrySet())
					foreach ( KeyValuePair<string, ?> entry in map.SetOfKeyValuePairs() )
					{
						 builder.Add( entry.Key, ValueOf( entry.Value ) );
					}

					return builder.Build();
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new AssertionError( "cannot convert: " + obj + " (a " + obj.GetType().FullName + ")" );
		 }

		 private class Mapper : ValueMapper_JavaMapper
		 {
			  public override object MapPath( PathValue value )
			  {
					return new MappedGraphType( value );
			  }

			  public override object MapNode( VirtualNodeValue value )
			  {
					return new MappedGraphType( value );
			  }

			  public override object MapRelationship( VirtualRelationshipValue value )
			  {
					return new MappedGraphType( value );
			  }
		 }

		 private class MappedGraphType
		 {
			  internal readonly VirtualValue Value;

			  internal MappedGraphType( VirtualValue value )
			  {

					this.Value = value;
			  }
		 }
	}

}