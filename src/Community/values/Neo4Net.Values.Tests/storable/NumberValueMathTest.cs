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
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
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
//	import static org.Neo4Net.values.utils.ValueMath.overflowSafeAdd;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.utils.ValueMath.overflowSafeMultiply;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.utils.ValueMath.overflowSafeSubtract;

	internal class NumberValueMathTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddSimpleIntegers()
		 internal virtual void ShouldAddSimpleIntegers()
		 {
			  NumberValue[] integers = new NumberValue[]{ byteValue( ( sbyte ) 42 ), shortValue( ( short ) 42 ), intValue( 42 ), longValue( 42 ) };

			  foreach ( NumberValue a in integers )
			  {
					foreach ( NumberValue b in integers )
					{
						 assertThat( a.Plus( b ), equalTo( longValue( 84 ) ) );
						 assertThat( b.Plus( a ), equalTo( longValue( 84 ) ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSubtractSimpleIntegers()
		 internal virtual void ShouldSubtractSimpleIntegers()
		 {
			  NumberValue[] integers = new NumberValue[]{ byteValue( ( sbyte ) 42 ), shortValue( ( short ) 42 ), intValue( 42 ), longValue( 42 ) };

			  foreach ( NumberValue a in integers )
			  {
					foreach ( NumberValue b in integers )
					{
						 assertThat( a.Minus( b ), equalTo( longValue( 0 ) ) );
						 assertThat( b.Minus( a ), equalTo( longValue( 0 ) ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMultiplySimpleIntegers()
		 internal virtual void ShouldMultiplySimpleIntegers()
		 {
			  NumberValue[] integers = new NumberValue[]{ byteValue( ( sbyte ) 42 ), shortValue( ( short ) 42 ), intValue( 42 ), longValue( 42 ) };

			  foreach ( NumberValue a in integers )
			  {
					foreach ( NumberValue b in integers )
					{
						 assertThat( a.Times( b ), equalTo( longValue( 42 * 42 ) ) );
						 assertThat( b.Times( a ), equalTo( longValue( 42 * 42 ) ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAddSimpleFloats()
		 internal virtual void ShouldAddSimpleFloats()
		 {
			  NumberValue[] integers = new NumberValue[]{ byteValue( ( sbyte ) 42 ), shortValue( ( short ) 42 ), intValue( 42 ), longValue( 42 ) };
			  NumberValue[] floats = new NumberValue[]{ floatValue( 42 ), doubleValue( 42 ) };

			  foreach ( NumberValue a in integers )
			  {
					foreach ( NumberValue b in floats )
					{
						 assertThat( a.Plus( b ), equalTo( doubleValue( 84 ) ) );
						 assertThat( b.Plus( a ), equalTo( doubleValue( 84 ) ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSubtractSimpleFloats()
		 internal virtual void ShouldSubtractSimpleFloats()
		 {
			  NumberValue[] integers = new NumberValue[]{ byteValue( ( sbyte ) 42 ), shortValue( ( short ) 42 ), intValue( 42 ), longValue( 42 ) };
			  NumberValue[] floats = new NumberValue[]{ floatValue( 42 ), doubleValue( 42 ) };

			  foreach ( NumberValue a in integers )
			  {
					foreach ( NumberValue b in floats )
					{
						 assertThat( a.Minus( b ), equalTo( doubleValue( 0 ) ) );
						 assertThat( b.Minus( a ), equalTo( doubleValue( 0 ) ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMultiplySimpleFloats()
		 internal virtual void ShouldMultiplySimpleFloats()
		 {
			  NumberValue[] integers = new NumberValue[]{ byteValue( ( sbyte ) 42 ), shortValue( ( short ) 42 ), intValue( 42 ), longValue( 42 ) };
			  NumberValue[] floats = new NumberValue[]{ floatValue( 42 ), doubleValue( 42 ) };

			  foreach ( NumberValue a in integers )
			  {
					foreach ( NumberValue b in floats )
					{
						 assertThat( a.Times( b ), equalTo( doubleValue( 42 * 42 ) ) );
						 assertThat( b.Times( a ), equalTo( doubleValue( 42 * 42 ) ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDivideSimpleIntegers()
		 internal virtual void ShouldDivideSimpleIntegers()
		 {
			  NumberValue[] integers = new NumberValue[]{ byteValue( ( sbyte ) 42 ), shortValue( ( short ) 42 ), intValue( 42 ), longValue( 42 ) };

			  foreach ( NumberValue a in integers )
			  {
					foreach ( NumberValue b in integers )
					{
						 assertThat( a.DivideBy( b ), equalTo( longValue( 1 ) ) );
						 assertThat( b.DivideBy( a ), equalTo( longValue( 1 ) ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDivideSimpleFloats()
		 internal virtual void ShouldDivideSimpleFloats()
		 {
			  NumberValue[] integers = new NumberValue[]{ byteValue( ( sbyte ) 42 ), shortValue( ( short ) 42 ), intValue( 42 ), longValue( 42 ) };
			  NumberValue[] floats = new NumberValue[]{ floatValue( 42 ), doubleValue( 42 ) };

			  foreach ( NumberValue a in integers )
			  {
					foreach ( NumberValue b in floats )
					{
						 assertThat( a.DivideBy( b ), equalTo( doubleValue( 1.0 ) ) );
						 assertThat( b.DivideBy( a ), equalTo( doubleValue( 1.0 ) ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailOnOverflowingAdd()
		 internal virtual void ShouldFailOnOverflowingAdd()
		 {
			  assertThrows( typeof( ArithmeticException ), () => longValue(long.MaxValue).plus(longValue(1)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailOnOverflowingSubtraction()
		 internal virtual void ShouldFailOnOverflowingSubtraction()
		 {
			  assertThrows( typeof( ArithmeticException ), () => longValue(long.MaxValue).minus(longValue(-1)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailOnOverflowingMultiplication()
		 internal virtual void ShouldFailOnOverflowingMultiplication()
		 {
			  assertThrows( typeof( ArithmeticException ), () => longValue(long.MaxValue).times(2) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotOverflowOnSafeAddition()
		 internal virtual void ShouldNotOverflowOnSafeAddition()
		 {
			  assertThat( overflowSafeAdd( long.MaxValue, 1 ), equalTo( doubleValue( ( double ) long.MaxValue + 1 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotOverflowOnSafeSubtraction()
		 internal virtual void ShouldNotOverflowOnSafeSubtraction()
		 {
			  assertThat( overflowSafeSubtract( long.MaxValue, -1 ), equalTo( doubleValue( ( ( double ) long.MaxValue ) + ( double ) 1 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotOverflowOnMultiplication()
		 internal virtual void ShouldNotOverflowOnMultiplication()
		 {
			  assertThat( overflowSafeMultiply( long.MaxValue, 2 ), equalTo( doubleValue( ( double ) long.MaxValue * 2 ) ) );
		 }
	}

}