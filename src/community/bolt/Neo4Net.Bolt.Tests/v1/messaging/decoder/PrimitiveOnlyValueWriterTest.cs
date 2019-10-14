using System;

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
namespace Neo4Net.Bolt.v1.messaging.decoder
{
	using Test = org.junit.jupiter.api.Test;
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using MethodSource = org.junit.jupiter.@params.provider.MethodSource;


	using AnyValue = Neo4Net.Values.AnyValue;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using LongValue = Neo4Net.Values.Storable.LongValue;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Values = Neo4Net.Values.Storable.Values;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.byteArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.pointValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.nodeValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.relationshipValue;

	internal class PrimitiveOnlyValueWriterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConvertStringValueToString()
		 internal virtual void ShouldConvertStringValueToString()
		 {
			  PrimitiveOnlyValueWriter writer = new PrimitiveOnlyValueWriter();
			  TextValue value = stringValue( "Hello" );

			  assertEquals( "Hello", writer.ValueAsObject( value ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConvertLongValueToLong()
		 internal virtual void ShouldConvertLongValueToLong()
		 {
			  PrimitiveOnlyValueWriter writer = new PrimitiveOnlyValueWriter();
			  LongValue value = longValue( 42 );

			  assertEquals( 42L, writer.ValueAsObject( value ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConvertMultipleValues()
		 internal virtual void ShouldConvertMultipleValues()
		 {
			  PrimitiveOnlyValueWriter writer = new PrimitiveOnlyValueWriter();

			  TextValue value1 = stringValue( "Hello" );
			  TextValue value2 = stringValue( " " );
			  TextValue value3 = stringValue( "World!" );
			  LongValue value4 = longValue( 42 );

			  assertEquals( "Hello", writer.ValueAsObject( value1 ) );
			  assertEquals( " ", writer.ValueAsObject( value2 ) );
			  assertEquals( "World!", writer.ValueAsObject( value3 ) );
			  assertEquals( 42L, writer.ValueAsObject( value4 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("unsupportedValues") void shouldFailToWriteComplexValue(org.neo4j.values.AnyValue value)
		 internal virtual void ShouldFailToWriteComplexValue( AnyValue value )
		 {
			  PrimitiveOnlyValueWriter writer = new PrimitiveOnlyValueWriter();
			  assertThrows( typeof( System.NotSupportedException ), () => writer.ValueAsObject(value) );
		 }

		 private static Stream<AnyValue> UnsupportedValues()
		 {
			  return Stream.of( nodeValue( 42, stringArray( "Person" ), EMPTY_MAP ), NewRelationshipValue(), pointValue(CoordinateReferenceSystem.WGS84, new double[2]), byteArray(new sbyte[]{ 1, 2, 3 }), Values.of(Duration.ofHours(1)), Values.of(LocalDate.now()), Values.of(LocalTime.now()), Values.of(OffsetTime.now()), Values.of(DateTime.Now), Values.of(ZonedDateTime.now()) );
		 }

		 private static RelationshipValue NewRelationshipValue()
		 {
			  NodeValue startNode = nodeValue( 24, stringArray( "Person" ), EMPTY_MAP );
			  NodeValue endNode = nodeValue( 42, stringArray( "Person" ), EMPTY_MAP );
			  return relationshipValue( 42, startNode, endNode, stringValue( "KNOWS" ), EMPTY_MAP );
		 }
	}

}