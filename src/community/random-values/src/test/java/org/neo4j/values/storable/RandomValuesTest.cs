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
namespace Neo4Net.Values.Storable
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.notNullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.ZERO_INT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.longValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class RandomValuesTest
	public class RandomValuesTest
	{
		 private const int ITERATIONS = 500;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter() public RandomValues randomValues;
		 public RandomValues RandomValues;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public String name;
		 public string Name;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{1}") public static Iterable<Object[]> generators()
		 public static IEnumerable<object[]> Generators()
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return Arrays.asList( new object[]{ RandomValues.Create( ThreadLocalRandom.current() ), typeof(Random).FullName }, new object[]{ RandomValues.Create(new SplittableRandom()), typeof(SplittableRandom).FullName } );
		 }

		 private const sbyte BOUND = 100;
		 private static readonly LongValue _upper = longValue( BOUND );
		 private static readonly ISet<Type> _numberTypes = new HashSet<Type>( Arrays.asList( typeof( LongValue ), typeof( IntValue ), typeof( ShortValue ), typeof( ByteValue ), typeof( FloatValue ), typeof( DoubleValue ) ) );

		 private static readonly ISet<Type> _types = new HashSet<Type>( Arrays.asList( typeof( LongValue ), typeof( IntValue ), typeof( ShortValue ), typeof( ByteValue ), typeof( FloatValue ), typeof( DoubleValue ), typeof( TextValue ), typeof( BooleanValue ), typeof( PointValue ), typeof( DateTimeValue ), typeof( LocalDateTimeValue ), typeof( DateValue ), typeof( TimeValue ), typeof( LocalTimeValue ), typeof( DurationValue ) ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextLongValueUnbounded()
		 public virtual void NextLongValueUnbounded()
		 {
			  CheckDistribution( RandomValues.nextLongValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextLongValueBounded()
		 public virtual void NextLongValueBounded()
		 {
			  CheckDistribution( () => RandomValues.nextLongValue(BOUND) );
			  CheckBounded( () => RandomValues.nextLongValue(BOUND) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextLongValueBoundedAndShifted()
		 public virtual void NextLongValueBoundedAndShifted()
		 {
			  ISet<Value> values = new HashSet<Value>();
			  for ( int i = 0; i < ITERATIONS; i++ )
			  {
					LongValue value = RandomValues.nextLongValue( 1337, 1337 + BOUND );
					assertThat( value, notNullValue() );
					assertThat( value.CompareTo( longValue( 1337 ) ), greaterThanOrEqualTo( 0 ) );
					assertThat( value.ToString(), value.CompareTo(longValue(1337 + BOUND)), lessThanOrEqualTo(0) );
					values.Add( value );
			  }

			  assertThat( values.Count, greaterThan( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextBooleanValue()
		 public virtual void NextBooleanValue()
		 {
			  CheckDistribution( RandomValues.nextBooleanValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextIntValueUnbounded()
		 public virtual void NextIntValueUnbounded()
		 {
			  CheckDistribution( RandomValues.nextIntValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextIntValueBounded()
		 public virtual void NextIntValueBounded()
		 {
			  CheckDistribution( () => RandomValues.nextIntValue(BOUND) );
			  CheckBounded( () => RandomValues.nextIntValue(BOUND) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextShortValueUnbounded()
		 public virtual void NextShortValueUnbounded()
		 {
			  CheckDistribution( RandomValues.nextShortValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextShortValueBounded()
		 public virtual void NextShortValueBounded()
		 {
			  CheckDistribution( () => RandomValues.nextShortValue(BOUND) );
			  CheckBounded( () => RandomValues.nextShortValue(BOUND) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextByteValueUnbounded()
		 public virtual void NextByteValueUnbounded()
		 {
			  CheckDistribution( RandomValues.nextByteValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextByteValueBounded()
		 public virtual void NextByteValueBounded()
		 {
			  CheckDistribution( () => RandomValues.nextByteValue(BOUND) );
			  CheckBounded( () => RandomValues.nextByteValue(BOUND) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextFloatValue()
		 public virtual void NextFloatValue()
		 {
			  CheckDistribution( RandomValues.nextFloatValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextDoubleValue()
		 public virtual void NextDoubleValue()
		 {
			  CheckDistribution( RandomValues.nextDoubleValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextNumberValue()
		 public virtual void NextNumberValue()
		 {
			  HashSet<Type> seen = new HashSet<Type>( _numberTypes );

			  for ( int i = 0; i < ITERATIONS; i++ )
			  {
					NumberValue numberValue = RandomValues.nextNumberValue();
					assertThat( _numberTypes, hasItem( numberValue.GetType() ) );
					seen.remove( numberValue.GetType() );
			  }
			  assertThat( seen, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextAlphaNumericString()
		 public virtual void NextAlphaNumericString()
		 {
			  ISet<int> seenDigits = "ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvxyz0123456789".chars().boxed().collect(Collectors.toSet());
			  for ( int i = 0; i < ITERATIONS; i++ )
			  {
					TextValue textValue = RandomValues.nextAlphaNumericTextValue( 10, 20 );
					string asString = textValue.StringValue();
					for ( int j = 0; j < asString.Length; j++ )
					{
						 int ch = asString[j];
						 assertTrue( "Not a character nor letter: " + ch, Character.isAlphabetic( ch ) || char.IsDigit( ch ) );
						 seenDigits.remove( ch );
					}
			  }
			  assertThat( seenDigits, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextAsciiString()
		 public virtual void NextAsciiString()
		 {
			  for ( int i = 0; i < ITERATIONS; i++ )
			  {
					TextValue textValue = RandomValues.nextAsciiTextValue( 10, 20 );
					string asString = textValue.StringValue();
					int length = asString.Length;
					assertThat( length, greaterThanOrEqualTo( 10 ) );
					assertThat( length, lessThanOrEqualTo( 20 ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextString()
		 public virtual void NextString()
		 {
			  for ( int i = 0; i < ITERATIONS; i++ )
			  {
					TextValue textValue = RandomValues.nextTextValue( 10, 20 );
					string asString = textValue.StringValue();
					int length = asString.codePointCount( 0, asString.Length );
					assertThat( length, greaterThanOrEqualTo( 10 ) );
					assertThat( length, lessThanOrEqualTo( 20 ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextArray()
		 public virtual void NextArray()
		 {
			  HashSet<Type> seen = new HashSet<Type>( _types );
			  for ( int i = 0; i < ITERATIONS; i++ )
			  {
					ArrayValue arrayValue = RandomValues.nextArray();
					assertThat( arrayValue.Length(), greaterThanOrEqualTo(1) );
					AnyValue value = arrayValue.Value( 0 );
					AssertKnownType( value.GetType(), _types );
					MarkSeen( value.GetType(), seen );
			  }

			  assertThat( seen, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextValue()
		 public virtual void NextValue()
		 {
			  HashSet<Type> all = new HashSet<Type>( _types );
			  all.Add( typeof( ArrayValue ) );
			  HashSet<Type> seen = new HashSet<Type>( all );

			  for ( int i = 0; i < ITERATIONS; i++ )
			  {
					Value value = RandomValues.nextValue();
					AssertKnownType( value.GetType(), all );
					MarkSeen( value.GetType(), seen );
			  }

			  assertThat( seen, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextValueOfTypes()
		 public virtual void NextValueOfTypes()
		 {
			  ValueType[] allTypes = ValueType.values();
			  ValueType[] including = RandomValues.selection( allTypes, 1, allTypes.Length, false );
			  HashSet<Type> seen = new HashSet<Type>();
			  foreach ( ValueType type in including )
			  {
					seen.Add( type.valueClass );
			  }
			  for ( int i = 0; i < ITERATIONS; i++ )
			  {
					Value value = RandomValues.nextValueOfTypes( including );
					AssertValueAmongTypes( including, value );
					MarkSeen( value.GetType(), seen );
			  }
			  assertThat( seen, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void excluding()
		 public virtual void Excluding()
		 {
			  ValueType[] allTypes = ValueType.values();
			  ValueType[] excluding = RandomValues.selection( allTypes, 1, allTypes.Length, false );
			  ValueType[] including = Neo4Net.Values.Storable.RandomValues.Excluding( excluding );
			  foreach ( ValueType excludedType in excluding )
			  {
					if ( ArrayUtils.contains( including, excludedType ) )
					{
						 fail( "Including array " + Arrays.ToString( including ) + " contains excluded type " + excludedType );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nextBasicMultilingualPlaneTextValue()
		 public virtual void NextBasicMultilingualPlaneTextValue()
		 {
			  for ( int i = 0; i < ITERATIONS; i++ )
			  {
					TextValue value = RandomValues.nextBasicMultilingualPlaneTextValue();
					//make sure the value fits in 16bits, meaning that the size of the char[]
					//matches the number of code points.
					assertThat( value.Length(), equalTo(value.StringValue().Length) );
			  }
		 }

		 private void AssertValueAmongTypes( ValueType[] types, Value value )
		 {
			  foreach ( ValueType type in types )
			  {
					if ( value.GetType().IsAssignableFrom(type.valueClass) )
					{
						 return;
					}
			  }
			  fail( "Value " + value + " was not among types " + Arrays.ToString( types ) );
		 }

		 private void AssertKnownType( Type typeToCheck, ISet<Type> types )
		 {
			  foreach ( Type type in types )
			  {
					if ( type.IsAssignableFrom( typeToCheck ) )
					{
						 return;
					}
			  }
			  fail( typeToCheck + " is not an expected type " );
		 }

		 private void MarkSeen( Type typeToCheck, ISet<Type> seen )
		 {
			  seen.removeIf( t => typeToCheck.IsAssignableFrom( t ) );
		 }

		 private void CheckDistribution( System.Func<Value> supplier )
		 {
			  ISet<Value> values = new HashSet<Value>();
			  for ( int i = 0; i < ITERATIONS; i++ )
			  {
					Value value = supplier();
					assertThat( value, notNullValue() );
					values.Add( value );
			  }

			  assertThat( values.Count, greaterThan( 1 ) );
		 }

		 private void CheckBounded( System.Func<NumberValue> supplier )
		 {
			  for ( int i = 0; i < ITERATIONS; i++ )
			  {
					NumberValue value = supplier();
					assertThat( value, notNullValue() );
					assertThat( value.CompareTo( ZERO_INT ), greaterThanOrEqualTo( 0 ) );
					assertThat( value.CompareTo( _upper ), lessThan( 0 ) );
			  }
		 }
	}

}