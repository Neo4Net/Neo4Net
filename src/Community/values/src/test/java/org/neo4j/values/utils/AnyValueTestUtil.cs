using System;

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
namespace Neo4Net.Values.utils
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class AnyValueTestUtil
	{
		 public static void AssertEqual( AnyValue a, AnyValue b )
		 {
			  assertEquals( FormatMessage( "should be equivalent to", a, b ), a, b );
			  assertEquals( FormatMessage( "should be equivalent to", b, a ), b, a );
			  assertTrue( FormatMessage( "should be equal to", a, b ), a.TernaryEquals( b ) );
			  assertTrue( FormatMessage( "should be equal to", b, a ), b.TernaryEquals( a ) );
			  assertEquals( FormatMessage( "should have same hashcode as", a, b ), a.GetHashCode(), b.GetHashCode() );
		 }

		 private static string FormatMessage( string should, AnyValue a, AnyValue b )
		 {
			  return string.Format( "{0}({1}) {2} {3}({4})", a.GetType().Name, a.ToString(), should, b.GetType().Name, b.ToString() );
		 }

		 public static void AssertEqualValues( AnyValue a, AnyValue b )
		 {
			  assertEquals( a + " should be equivalent to " + b, a, b );
			  assertEquals( a + " should be equivalent to " + b, b, a );
			  assertTrue( a + " should be equal to " + b, a.TernaryEquals( b ) );
			  assertTrue( a + " should be equal to " + b, b.TernaryEquals( a ) );
		 }

		 public static void AssertNotEqual( AnyValue a, AnyValue b )
		 {
			  assertNotEquals( a + " should not be equivalent to " + b, a, b );
			  assertNotEquals( b + " should not be equivalent to " + a, b, a );
			  assertFalse( a + " should not equal " + b, a.TernaryEquals( b ) );
			  assertFalse( b + " should not equal " + a, b.TernaryEquals( a ) );
		 }

		 public static void AssertIncomparable( AnyValue a, AnyValue b )
		 {
			  assertNotEquals( a + " should not be equivalent to " + b, a, b );
			  assertNotEquals( b + " should not be equivalent to " + a, b, a );
			  assertNull( a + " should be incomparable to " + b, a.TernaryEquals( b ) );
			  assertNull( b + " should be incomparable to " + a, b.TernaryEquals( a ) );
		 }

		 public static X AssertThrows<X, T>( Type exception, System.Func<T> thunk ) where X : Exception
		 {
				 exception = typeof( X );
			  T value;
			  try
			  {
					value = thunk();
			  }
			  catch ( Exception e )
			  {
					if ( exception.IsInstanceOfType( e ) )
					{
						 return exception.cast( e );
					}
					else
					{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						 throw new AssertionError( "Expected " + exception.FullName, e );
					}
			  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  throw new AssertionError( "Expected " + exception.FullName + " but returned: " + value );
		 }
	}

}