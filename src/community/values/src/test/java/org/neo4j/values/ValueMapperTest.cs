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
namespace Neo4Net.Values
{
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using MethodSource = org.junit.jupiter.@params.provider.MethodSource;


	using Point = Neo4Net.GraphDb.Spatial.Point;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;
	using MapValueBuilder = Neo4Net.Values.@virtual.MapValueBuilder;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using PathValue = Neo4Net.Values.@virtual.PathValue;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;
	using VirtualNodeValue = Neo4Net.Values.@virtual.VirtualNodeValue;
	using VirtualRelationshipValue = Neo4Net.Values.@virtual.VirtualRelationshipValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.CoordinateReferenceSystem.Cartesian;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.CoordinateReferenceSystem.WGS84;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.DateTimeValue.datetime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.DateValue.date;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.DurationValue.duration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.LocalDateTimeValue.localDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.LocalTimeValue.localTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.TimeValue.time;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.NO_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.booleanArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.booleanValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.byteArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.byteValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.charArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.charValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.doubleArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.doubleValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.floatArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.floatValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.intArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.longArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.numberValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.pointArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.pointValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.shortArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.shortValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.stringArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.emptyMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.fromList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.list;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.nodeValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.relationshipValue;

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
//ORIGINAL LINE: return fromList(((java.util.List<?>) obj).stream().map(ValueMapperTest::ValueOf).collect(toList()));
					return fromList( ( ( IList<object> ) obj ).Select( ValueMapperTest.ValueOf ).ToList() );
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