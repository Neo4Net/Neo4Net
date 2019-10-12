using System.Collections.Generic;

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
namespace Neo4Net.Values.Storable
{
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.utils.AnyValueTestUtil.assertEqual;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.utils.AnyValueTestUtil.assertNotEqual;

	/// <summary>
	/// This test was faithfully converted (including personal remarks) from PropertyEqualityTest.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(value = Parameterized.class) public class ValueEqualityTest
	public class ValueEqualityTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static Iterable<Test> data()
		 public static IEnumerable<Test> Data()
		 {
			  return Arrays.asList( shouldMatch( true, true ), shouldMatch( false, false ), shouldNotMatch( true, false ), shouldNotMatch( false, true ), shouldNotMatch( true, 0 ), shouldNotMatch( false, 0 ), shouldNotMatch( true, 1 ), shouldNotMatch( false, 1 ), shouldNotMatch( false, "false" ), shouldNotMatch( true, "true" ), shouldMatch( ( sbyte ) 42, ( sbyte ) 42 ), shouldMatch( ( sbyte ) 42, ( short ) 42 ), shouldNotMatch( ( sbyte ) 42, 42 + 256 ), shouldMatch( ( sbyte ) 43, 43 ), shouldMatch( ( sbyte ) 43, 43L ), shouldMatch( ( sbyte ) 23, 23.0d ), shouldMatch( ( sbyte ) 23, 23.0f ), shouldNotMatch( ( sbyte ) 23, 23.5 ), shouldNotMatch( ( sbyte ) 23, 23.5f ), shouldMatch( ( short ) 11, ( sbyte ) 11 ), shouldMatch( ( short ) 42, ( short ) 42 ), shouldNotMatch( ( short ) 42, 42 + 65536 ), shouldMatch( ( short ) 43, 43 ), shouldMatch( ( short ) 43, 43L ), shouldMatch( ( short ) 23, 23.0f ), shouldMatch( ( short ) 23, 23.0d ), shouldNotMatch( ( short ) 23, 23.5 ), shouldNotMatch( ( short ) 23, 23.5f ), shouldMatch( 11, ( sbyte ) 11 ), shouldMatch( 42, ( short ) 42 ), shouldNotMatch( 42, 42 + 4294967296L ), shouldMatch( 43, 43 ), shouldMatch( int.MaxValue, int.MaxValue ), shouldMatch( 43, ( long ) 43 ), shouldMatch( 23, 23.0 ), shouldNotMatch( 23, 23.5 ), shouldNotMatch( 23, 23.5f ), shouldMatch( 11L, ( sbyte ) 11 ), shouldMatch( 42L, ( short ) 42 ), shouldMatch( 43L, 43 ), shouldMatch( 43L, 43L ), shouldMatch( 87L, 87L ), shouldMatch( long.MaxValue, long.MaxValue ), shouldMatch( 23L, 23.0 ), shouldNotMatch( 23L, 23.5 ), shouldNotMatch( 23L, 23.5f ), shouldMatch( 9007199254740992L, 9007199254740992D ), shouldMatch( 11f, ( sbyte ) 11 ), shouldMatch( 42f, ( short ) 42 ), shouldMatch( 43f, 43 ), shouldMatch( 43f, 43L ), shouldMatch( 23f, 23.0 ), shouldNotMatch( 23f, 23.5 ), shouldNotMatch( 23f, 23.5f ), shouldMatch( 3.14f, 3.14f ), shouldNotMatch( 3.14f, 3.14d ), shouldMatch( 11d, ( sbyte ) 11 ), shouldMatch( 42d, ( short ) 42 ), shouldMatch( 43d, 43 ), shouldMatch( 43d, 43d ), shouldMatch( 23d, 23.0 ), shouldNotMatch( 23d, 23.5 ), shouldNotMatch( 23d, 23.5f ), shouldNotMatch( 3.14d, 3.14f ), shouldMatch( 3.14d, 3.14d ), shouldMatch( "A", "A" ), shouldMatch( 'A', 'A' ), shouldMatch( 'A', "A" ), shouldMatch( "A", 'A' ), shouldNotMatch( "AA", 'A' ), shouldNotMatch( "a", "A" ), shouldNotMatch( "A", "a" ), shouldNotMatch( "0", 0 ), shouldNotMatch( '0', 0 ), shouldMatch( new int[]{ 1, 2, 3 }, new int[]{ 1, 2, 3 } ), shouldMatch( new int[]{ 1, 2, 3 }, new long[]{ 1, 2, 3 } ), shouldMatch( new int[]{ 1, 2, 3 }, new double[]{ 1.0, 2.0, 3.0 } ), shouldMatch( new string[]{ "A", "B", "C" }, new string[]{ "A", "B", "C" } ), shouldMatch( new string[]{ "A", "B", "C" }, new char[]{ 'A', 'B', 'C' } ), shouldMatch( new char[]{ 'A', 'B', 'C' }, new string[]{ "A", "B", "C" } ), shouldNotMatch( false, new bool[]{ false } ), shouldNotMatch( 1, new int[]{ 1 } ), shouldNotMatch( "apa", new string[]{ "apa" } ) );
		 }

		 private Test _currentTest;

		 public ValueEqualityTest( Test currentTest )
		 {
			  this._currentTest = currentTest;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @org.junit.Test public void runTest()
		 public virtual void RunTest()
		 {
			  _currentTest.checkAssertion();
		 }

		 private static Test ShouldMatch( bool propertyValue, object value )
		 {
			  return new Test( Values.BooleanValue( propertyValue ), value, true );
		 }

		 private static Test ShouldNotMatch( bool propertyValue, object value )
		 {
			  return new Test( Values.BooleanValue( propertyValue ), value, false );
		 }

		 private static Test ShouldMatch( sbyte propertyValue, object value )
		 {
			  return new Test( Values.ByteValue( propertyValue ), value, true );
		 }

		 private static Test ShouldNotMatch( sbyte propertyValue, object value )
		 {
			  return new Test( Values.ByteValue( propertyValue ), value, false );
		 }

		 private static Test ShouldMatch( short propertyValue, object value )
		 {
			  return new Test( Values.ShortValue( propertyValue ), value, true );
		 }

		 private static Test ShouldNotMatch( short propertyValue, object value )
		 {
			  return new Test( Values.ShortValue( propertyValue ), value, false );
		 }

		 private static Test ShouldMatch( float propertyValue, object value )
		 {
			  return new Test( Values.FloatValue( propertyValue ), value, true );
		 }

		 private static Test ShouldNotMatch( float propertyValue, object value )
		 {
			  return new Test( Values.FloatValue( propertyValue ), value, false );
		 }

		 private static Test ShouldMatch( long propertyValue, object value )
		 {
			  return new Test( Values.LongValue( propertyValue ), value, true );
		 }

		 private static Test ShouldNotMatch( long propertyValue, object value )
		 {
			  return new Test( Values.LongValue( propertyValue ), value, false );
		 }

		 private static Test ShouldMatch( double propertyValue, object value )
		 {
			  return new Test( Values.DoubleValue( propertyValue ), value, true );
		 }

		 private static Test ShouldNotMatch( double propertyValue, object value )
		 {
			  return new Test( Values.DoubleValue( propertyValue ), value, false );
		 }

		 private static Test ShouldMatch( string propertyValue, object value )
		 {
			  return new Test( Values.StringValue( propertyValue ), value, true );
		 }

		 private static Test ShouldNotMatch( string propertyValue, object value )
		 {
			  return new Test( Values.StringValue( propertyValue ), value, false );
		 }

		 private static Test ShouldMatch( char propertyValue, object value )
		 {
			  return new Test( Values.CharValue( propertyValue ), value, true );
		 }

		 private static Test ShouldNotMatch( char propertyValue, object value )
		 {
			  return new Test( Values.CharValue( propertyValue ), value, false );
		 }

		 private static Test ShouldMatch( int[] propertyValue, object value )
		 {
			  return new Test( Values.IntArray( propertyValue ), value, true );
		 }

		 public static Test ShouldNotMatch( int[] propertyValue, object value )
		 {
			  return new Test( Values.IntArray( propertyValue ), value, false );
		 }

		 private static Test ShouldMatch( char[] propertyValue, object value )
		 {
			  return new Test( Values.CharArray( propertyValue ), value, true );
		 }

		 public static Test ShouldNotMatch( char[] propertyValue, object value )
		 {
			  return new Test( Values.CharArray( propertyValue ), value, false );
		 }

		 private static Test ShouldMatch( string[] propertyValue, object value )
		 {
			  return new Test( Values.StringArray( propertyValue ), value, true );
		 }

		 private class Test
		 {
			  internal readonly Value A;
			  internal readonly Value B;
			  internal readonly bool ShouldMatch;

			  internal Test( Value a, object b, bool shouldMatch )
			  {
					this.A = a;
					this.B = Values.Of( b );
					this.ShouldMatch = shouldMatch;
			  }

			  public override string ToString()
			  {
					return string.Format( "{0} {1} {2}", A, ShouldMatch ? "==" : "!=", B );
			  }

			  internal virtual void CheckAssertion()
			  {
					if ( ShouldMatch )
					{
						 assertEqual( A, B );
					}
					else
					{
						 assertNotEqual( A, B );
					}
			  }
		 }
	}

}