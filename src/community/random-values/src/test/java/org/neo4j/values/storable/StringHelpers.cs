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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.utf8Value;

	internal sealed class StringHelpers
	{
		 private StringHelpers()
		 {
			  throw new System.NotSupportedException();
		 }

		 internal static void AssertConsistent<T>( string @string, System.Func<TextValue, T> test )
		 {
			  TextValue textValue = stringValue( @string );
			  TextValue utf8Value = utf8Value( @string.GetBytes( UTF_8 ) );
			  T a = test( textValue );
			  T b = test( utf8Value );

			  string errorMsg = format( "operation not consistent for %s", @string );
			  assertThat( errorMsg, a, equalTo( b ) );
			  assertThat( errorMsg, b, equalTo( a ) );
		 }

		 internal static void AssertConsistent<T>( string string1, string string2, System.Func<TextValue, TextValue, T> test )
		 {
			  TextValue textValue1 = stringValue( string1 );
			  TextValue textValue2 = stringValue( string2 );
			  TextValue utf8Value1 = utf8Value( string1.GetBytes( UTF_8 ) );
			  TextValue utf8Value2 = utf8Value( string2.GetBytes( UTF_8 ) );
			  T a = test( textValue1, textValue2 );
			  T x = test( textValue1, utf8Value2 );
			  T y = test( utf8Value1, textValue2 );
			  T z = test( utf8Value1, utf8Value2 );

			  string errorMsg = format( "operation not consistent for `%s` and `%s`", string1, string2 );
			  assertThat( errorMsg, a, equalTo( x ) );
			  assertThat( errorMsg, x, equalTo( a ) );
			  assertThat( errorMsg, a, equalTo( y ) );
			  assertThat( errorMsg, y, equalTo( a ) );
			  assertThat( errorMsg, a, equalTo( z ) );
			  assertThat( errorMsg, z, equalTo( a ) );
		 }
	}

}