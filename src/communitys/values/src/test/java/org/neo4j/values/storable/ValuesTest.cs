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
namespace Neo4Net.Values.Storable
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
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
//	import static org.neo4j.values.storable.Values.shortArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.shortValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.utils.AnyValueTestUtil.assertEqual;

	internal class ValuesTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeEqualToItself()
		 internal virtual void ShouldBeEqualToItself()
		 {
			  assertEqual( booleanValue( false ), booleanValue( false ) );
			  assertEqual( byteValue( ( sbyte ) 0 ), byteValue( ( sbyte ) 0 ) );
			  assertEqual( shortValue( ( short ) 0 ), shortValue( ( short ) 0 ) );
			  assertEqual( intValue( 0 ), intValue( 0 ) );
			  assertEqual( longValue( 0 ), longValue( 0 ) );
			  assertEqual( floatValue( 0.0f ), floatValue( 0.0f ) );
			  assertEqual( doubleValue( 0.0 ), doubleValue( 0.0 ) );
			  assertEqual( stringValue( "" ), stringValue( "" ) );

			  assertEqual( booleanValue( true ), booleanValue( true ) );
			  assertEqual( byteValue( ( sbyte ) 1 ), byteValue( ( sbyte ) 1 ) );
			  assertEqual( shortValue( ( short ) 1 ), shortValue( ( short ) 1 ) );
			  assertEqual( intValue( 1 ), intValue( 1 ) );
			  assertEqual( longValue( 1 ), longValue( 1 ) );
			  assertEqual( floatValue( 1.0f ), floatValue( 1.0f ) );
			  assertEqual( doubleValue( 1.0 ), doubleValue( 1.0 ) );
			  assertEqual( charValue( 'x' ), charValue( 'x' ) );
			  assertEqual( stringValue( "hi" ), stringValue( "hi" ) );

			  assertEqual( booleanArray( new bool[]{} ), booleanArray( new bool[]{} ) );
			  assertEqual( byteArray( new sbyte[]{} ), byteArray( new sbyte[]{} ) );
			  assertEqual( shortArray( new short[]{} ), shortArray( new short[]{} ) );
			  assertEqual( intArray( new int[]{} ), intArray( new int[]{} ) );
			  assertEqual( longArray( new long[]{} ), longArray( new long[]{} ) );
			  assertEqual( floatArray( new float[]{} ), floatArray( new float[]{} ) );
			  assertEqual( doubleArray( new double[]{} ), doubleArray( new double[]{} ) );
			  assertEqual( charArray( new char[]{} ), charArray( new char[]{} ) );
			  assertEqual( stringArray(), stringArray() );

			  assertEqual( booleanArray( new bool[]{ true } ), booleanArray( new bool[]{ true } ) );
			  assertEqual( byteArray( new sbyte[]{ 1 } ), byteArray( new sbyte[]{ 1 } ) );
			  assertEqual( shortArray( new short[]{ 1 } ), shortArray( new short[]{ 1 } ) );
			  assertEqual( intArray( new int[]{ 1 } ), intArray( new int[]{ 1 } ) );
			  assertEqual( longArray( new long[]{ 1 } ), longArray( new long[]{ 1 } ) );
			  assertEqual( floatArray( new float[]{ 1.0f } ), floatArray( new float[]{ 1.0f } ) );
			  assertEqual( doubleArray( new double[]{ 1.0 } ), doubleArray( new double[]{ 1.0 } ) );
			  assertEqual( charArray( new char[]{ 'x' } ), charArray( new char[]{ 'x' } ) );
			  assertEqual( stringArray( "hi" ), stringArray( "hi" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pointValueShouldRequireConsistentInput()
		 internal virtual void PointValueShouldRequireConsistentInput()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => Values.PointValue(CoordinateReferenceSystem.Cartesian, 1, 2, 3) );
			  assertThrows( typeof( System.ArgumentException ), () => Values.PointValue(CoordinateReferenceSystem.Cartesian_3D, 1, 2) );
			  assertThrows( typeof( System.ArgumentException ), () => Values.PointValue(CoordinateReferenceSystem.Wgs84, 1, 2, 3) );
			  assertThrows( typeof( System.ArgumentException ), () => Values.PointValue(CoordinateReferenceSystem.Wgs84_3d, 1, 2) );
		 }
	}

}