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
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.BufferValueWriter.Specials.beginArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.BufferValueWriter.Specials.byteArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.BufferValueWriter.Specials.endArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueWriter_ArrayType.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueWriter_ArrayType.CHAR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueWriter_ArrayType.DOUBLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueWriter_ArrayType.FLOAT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueWriter_ArrayType.INT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueWriter_ArrayType.LOCAL_DATE_TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueWriter_ArrayType.LONG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueWriter_ArrayType.SHORT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueWriter_ArrayType.STRING;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(value = Parameterized.class) public class ValueWriteToTest
	public class ValueWriteToTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static Iterable<WriteTest> data()
		 public static IEnumerable<WriteTest> Data()
		 {
			  return Arrays.asList( shouldWrite( true, true ), shouldWrite( false, false ), shouldWrite( ( sbyte ) 0, ( sbyte ) 0 ), shouldWrite( ( sbyte ) 42, ( sbyte ) 42 ), shouldWrite( ( short ) 42, ( short ) 42 ), shouldWrite( 42, 42 ), shouldWrite( 42L, 42L ), shouldWrite( 42.0f, 42.0f ), shouldWrite( 42.0, 42.0 ), shouldWrite( 'x', 'x' ), shouldWrite( "Hi", "Hi" ), shouldWrite( Values.NoValue, ( object ) null ), shouldWrite( Values.PointValue( CoordinateReferenceSystem.Cartesian, 1, 2 ), Values.PointValue( CoordinateReferenceSystem.Cartesian, 1, 2 ) ), shouldWrite( Values.PointValue( CoordinateReferenceSystem.Wgs84, 1, 2 ), Values.PointValue( CoordinateReferenceSystem.Wgs84, 1, 2 ) ), shouldWrite( LocalDate.of( 1991, 10, 18 ), DateValue.Date( 1991, 10, 18 ) ), shouldWrite( new sbyte[]{ 1, 2, 3 }, byteArray( new sbyte[]{ 1, 2, 3 } ) ), shouldWrite( new short[]{ 1, 2, 3 }, beginArray( 3, SHORT ), ( short ) 1, ( short ) 2, ( short ) 3, endArray() ), shouldWrite(new int[]{ 1, 2, 3 }, beginArray(3, INT), 1, 2, 3, endArray()), shouldWrite(new long[]{ 1, 2, 3 }, beginArray(3, LONG), 1L, 2L, 3L, endArray()), shouldWrite(new float[]{ 1, 2, 3 }, beginArray(3, FLOAT), 1.0f, 2.0f, 3.0f, endArray()), shouldWrite(new double[]{ 1, 2, 3 }, beginArray(3, DOUBLE), 1.0, 2.0, 3.0, endArray()), shouldWrite(new char[]{ 'a', 'b' }, beginArray(2, CHAR), 'a', 'b', endArray()), shouldWrite(new string[]{ "a", "b" }, beginArray(2, STRING), "a", "b", endArray()), shouldWrite(new bool[]{ true, false }, beginArray(2, BOOLEAN), true, false, endArray()), shouldWrite(new DateTime[]{ new DateTime(1991, 10, 18, 6, 37, 0, 0), new DateTime(1992, 10, 18, 6, 37, 0, 0) }, beginArray(2, LOCAL_DATE_TIME), LocalDateTimeValue.LocalDateTime(1991, 10, 18, 6, 37, 0, 0), LocalDateTimeValue.LocalDateTime(1992, 10, 18, 6, 37, 0, 0), endArray()), shouldWrite(new sbyte[]{ 1, 2, 3 }, byteArray(new sbyte[]{ 1, 2, 3 })) );
		 }

		 private WriteTest _currentTest;

		 public ValueWriteToTest( WriteTest currentTest )
		 {
			  this._currentTest = currentTest;
		 }

		 private static WriteTest ShouldWrite( object value, params object[] expected )
		 {
			  return new WriteTest( Values.Of( value ), expected );
		 }

		 private static WriteTest ShouldWrite( Value value, params object[] expected )
		 {
			  return new WriteTest( value, expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @org.junit.Test public void runTest()
		 public virtual void RunTest()
		 {
			  _currentTest.verifyWriteTo();
		 }

		 private class WriteTest
		 {
			  internal readonly Value Value;
			  internal readonly object[] Expected;

			  internal WriteTest( Value value, params object[] expected )
			  {
					this.Value = value;
					this.Expected = expected;
			  }

			  public override string ToString()
			  {
					return string.Format( "{0} should write {1}", Value, Arrays.ToString( Expected ) );
			  }

			  internal virtual void VerifyWriteTo()
			  {
					BufferValueWriter writer = new BufferValueWriter();
					Value.writeTo( writer );
					writer.AssertBuffer( Expected );
			  }
		 }
	}

}