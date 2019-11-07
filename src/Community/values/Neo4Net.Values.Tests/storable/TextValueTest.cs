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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.utf8Value;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class TextValueTest
	public class TextValueTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public System.Func<String,TextValue> value;
		 public System.Func<string, TextValue> Value;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static java.util.Collection<System.Func<String,TextValue>> functions()
		 public static ICollection<System.Func<string, TextValue>> Functions()
		 {
			  return asList( Values.stringValue, s => utf8Value( s.getBytes( StandardCharsets.UTF_8 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void replace()
		 public virtual void Replace()
		 {
			  assertThat( Value.apply( "hello" ).replace( "l", "w" ), equalTo( Value.apply( "hewwo" ) ) );
			  assertThat( Value.apply( "hello" ).replace( "ell", "ipp" ), equalTo( Value.apply( "hippo" ) ) );
			  assertThat( Value.apply( "hello" ).replace( "a", "x" ), equalTo( Value.apply( "hello" ) ) );
			  assertThat( Value.apply( "hello" ).replace( "e", "" ), equalTo( Value.apply( "hllo" ) ) );
			  assertThat( Value.apply( "" ).replace( "", "⁻" ), equalTo( Value.apply( "⁻" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void substring()
		 public virtual void Substring()
		 {
			  assertThat( Value.apply( "hello" ).substring( 2, 3 ), equalTo( Value.apply( "llo" ) ) );
			  assertThat( Value.apply( "hello" ).substring( 4, 1 ), equalTo( Value.apply( "o" ) ) );
			  assertThat( Value.apply( "hello" ).substring( 1, 2 ), equalTo( Value.apply( "ell" ) ) );
			  assertThat( Value.apply( "hello" ).substring( 8, -3 ), equalTo( StringValue.EMPTY ) );
			  assertThat( Value.apply( "0123456789" ).substring( 1 ), equalTo( Value.apply( "123456789" ) ) );
			  assertThat( Value.apply( "0123456789" ).substring( 5 ), equalTo( Value.apply( "56789" ) ) );
			  assertThat( Value.apply( "0123456789" ).substring( 15 ), equalTo( StringValue.EMPTY ) );
			  assertThat( Value.apply( "\uD83D\uDE21\uD83D\uDCA9\uD83D\uDC7B" ).substring( 1, 0 ), equalTo( Value.apply( "\uD83D\uDCA9" ) ) );
			  assertThat( Value.apply( "\uD83D\uDE21\uD83D\uDCA9\uD83D\uDC7B" ).substring( 1, 1 ), equalTo( Value.apply( "\uD83D\uDCA9\uD83D\uDC7B" ) ) );

			  Exception.expect( typeof( System.IndexOutOfRangeException ) );
			  Value.apply( "hello" ).substring( -4, 6 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void toLower()
		 public virtual void ToLower()
		 {
			  assertThat( Value.apply( "HELLO" ).toLower(), equalTo(Value.apply("hello")) );
			  assertThat( Value.apply( "Hello" ).toLower(), equalTo(Value.apply("hello")) );
			  assertThat( Value.apply( "hello" ).toLower(), equalTo(Value.apply("hello")) );
			  assertThat( Value.apply( "" ).toLower(), equalTo(Value.apply("")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void toUpper()
		 public virtual void ToUpper()
		 {
			  assertThat( Value.apply( "HELLO" ).toUpper(), equalTo(Value.apply("HELLO")) );
			  assertThat( Value.apply( "Hello" ).toUpper(), equalTo(Value.apply("HELLO")) );
			  assertThat( Value.apply( "hello" ).toUpper(), equalTo(Value.apply("HELLO")) );
			  assertThat( Value.apply( "" ).toUpper(), equalTo(Value.apply("")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void ltrim()
		 public virtual void Ltrim()
		 {
			  assertThat( Value.apply( "  HELLO" ).ltrim(), equalTo(Value.apply("HELLO")) );
			  assertThat( Value.apply( " Hello" ).ltrim(), equalTo(Value.apply("Hello")) );
			  assertThat( Value.apply( "  hello  " ).ltrim(), equalTo(Value.apply("hello  ")) );
			  assertThat( Value.apply( "\u2009㺂࿝鋦毠\u2009" ).ltrim(), equalTo(Value.apply("㺂࿝鋦毠\u2009")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rtrim()
		 public virtual void Rtrim()
		 {
			  assertThat( Value.apply( "HELLO  " ).rtrim(), equalTo(Value.apply("HELLO")) );
			  assertThat( Value.apply( "Hello  " ).rtrim(), equalTo(Value.apply("Hello")) );
			  assertThat( Value.apply( "  hello  " ).rtrim(), equalTo(Value.apply("  hello")) );
			  assertThat( Value.apply( "\u2009㺂࿝鋦毠\u2009" ).rtrim(), equalTo(Value.apply("\u2009㺂࿝鋦毠")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void trim()
		 public virtual void Trim()
		 {
			  assertThat( Value.apply( "  hello  " ).Trim(), equalTo(Value.apply("hello")) );
			  assertThat( Value.apply( "  hello " ).Trim(), equalTo(Value.apply("hello")) );
			  assertThat( Value.apply( "hello " ).Trim(), equalTo(Value.apply("hello")) );
			  assertThat( Value.apply( "  hello" ).Trim(), equalTo(Value.apply("hello")) );
			  assertThat( Value.apply( "\u2009㺂࿝鋦毠\u2009" ).Trim(), equalTo(Value.apply("㺂࿝鋦毠")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reverse()
		 public virtual void Reverse()
		 {
			  assertThat( Value.apply( "Foo" ).reverse(), equalTo(Value.apply("ooF")) );
			  assertThat( Value.apply( "" ).reverse(), equalTo(StringValue.EMPTY) );
			  assertThat( Value.apply( " L" ).reverse(), equalTo(Value.apply("L ")) );
			  assertThat( Value.apply( "\r\n" ).reverse(), equalTo(Value.apply("\n\r")) );
			  assertThat( Value.apply( "\uD801\uDC37" ).reverse(), equalTo(Value.apply("\uD801\uDC37")) );
			  assertThat( Value.apply( "This is literally a pile of crap \uD83D\uDCA9, it is fantastic" ).reverse(), equalTo(Value.apply("citsatnaf si ti ,\uD83D\uDCA9 parc fo elip a yllaretil si sihT")) );
			  assertThat( Value.apply( "\uD83D\uDE21\uD83D\uDCA9\uD83D\uDC7B" ).reverse(), equalTo(Value.apply("\uD83D\uDC7B\uD83D\uDCA9\uD83D\uDE21")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void split()
		 public virtual void Split()
		 {
			  assertThat( Value.apply( "HELLO" ).Split( "LL" ), equalTo( stringArray( "HE", "O" ) ) );
			  assertThat( Value.apply( "Separating,by,comma,is,a,common,use,case" ).Split( "," ), equalTo( stringArray( "Separating", "by", "comma", "is", "a", "common", "use", "case" ) ) );
			  assertThat( Value.apply( "HELLO" ).Split( "HELLO" ), equalTo( stringArray( "", "" ) ) );

		 }
	}

}