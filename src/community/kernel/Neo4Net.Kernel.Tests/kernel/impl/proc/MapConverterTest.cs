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
namespace Neo4Net.Kernel.impl.proc
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using DefaultParameterValue = Neo4Net.Internal.Kernel.Api.procs.DefaultParameterValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.procs.DefaultParameterValue.ntMap;

	public class MapConverterTest
	{
		 private readonly MapConverter _converter = new MapConverter();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNullString()
		 public virtual void ShouldHandleNullString()
		 {
			  // Given
			  string mapString = "null";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntMap( null ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleEmptyMap()
		 public virtual void ShouldHandleEmptyMap()
		 {
			  // Given
			  string mapString = "{}";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntMap( emptyMap() ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleEmptyMapWithSpaces()
		 public virtual void ShouldHandleEmptyMapWithSpaces()
		 {
			  // Given
			  string mapString = " {  }  ";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntMap( emptyMap() ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSingleQuotedValue()
		 public virtual void ShouldHandleSingleQuotedValue()
		 {
			  // Given
			  string mapString = "{key: 'value'}";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntMap( map( "key", "value" ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleEscapedSingleQuotedInValue1()
		 public virtual void ShouldHandleEscapedSingleQuotedInValue1()
		 {
			  // Given
			  string mapString = "{key: 'va\'lue'}";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntMap( map( "key", "va\'lue" ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleEscapedSingleQuotedInValue2()
		 public virtual void ShouldHandleEscapedSingleQuotedInValue2()
		 {
			  // Given
			  string mapString = "{key: \"va\'lue\"}";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntMap( map( "key", "va\'lue" ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleEscapedDoubleQuotedInValue1()
		 public virtual void ShouldHandleEscapedDoubleQuotedInValue1()
		 {
			  // Given
			  string mapString = "{key: \"va\"lue\"}";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntMap( map( "key", "va\"lue" ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleEscapedDoubleQuotedInValue2()
		 public virtual void ShouldHandleEscapedDoubleQuotedInValue2()
		 {
			  // Given
			  string mapString = "{key: 'va\"lue'}";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntMap( map( "key", "va\"lue" ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDoubleQuotedValue()
		 public virtual void ShouldHandleDoubleQuotedValue()
		 {
			  // Given
			  string mapString = "{key: \"value\"}";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntMap( map( "key", "value" ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSingleQuotedKey()
		 public virtual void ShouldHandleSingleQuotedKey()
		 {
			  // Given
			  string mapString = "{'key;: 'value'}";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntMap( map( "key", "value" ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDoubleQuotedKey()
		 public virtual void ShouldHandleDoubleQuotedKey()
		 {
			  // Given
			  string mapString = "{\"key\": \"value\"}";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntMap( map( "key", "value" ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleKeyWithEscapedSingleQuote()
		 public virtual void ShouldHandleKeyWithEscapedSingleQuote()
		 {
			  // Given
			  string mapString = "{\"k\'ey\": \"value\"}";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntMap( map( "k\'ey", "value" ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleKeyWithEscapedDoubleQuote()
		 public virtual void ShouldHandleKeyWithEscapedDoubleQuote()
		 {
			  // Given
			  string mapString = "{\"k\"ey\": \"value\"}";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntMap( map( "k\"ey", "value" ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleIntegerValue()
		 public virtual void ShouldHandleIntegerValue()
		 {
			  // Given
			  string mapString = "{key: 1337}";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntMap( map( "key", 1337L ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleFloatValue()
		 public virtual void ShouldHandleFloatValue()
		 {
			  // Given
			  string mapString = "{key: 2.718281828}";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntMap( map( "key", 2.718281828 ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNullValue()
		 public virtual void ShouldHandleNullValue()
		 {
			  // Given
			  string mapString = "{key: null}";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntMap( map( "key", null ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleFalseValue()
		 public virtual void ShouldHandleFalseValue()
		 {
			  // Given
			  string mapString = "{key: false}";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntMap( map( "key", false ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleTrueValue()
		 public virtual void ShouldHandleTrueValue()
		 {
			  // Given
			  string mapString = "{key: true}";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntMap( map( "key", true ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMultipleKeys()
		 public virtual void ShouldHandleMultipleKeys()
		 {
			  // Given
			  string mapString = "{k1: 2.718281828, k2: 'e'}";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntMap( map( "k1", 2.718281828, "k2", "e" ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenDuplicateKey()
		 public virtual void ShouldFailWhenDuplicateKey()
		 {
			  // Given
			  string mapString = "{k1: 2.718281828, k1: 'e'}";

			  // Expect
			  Exception.expect( typeof( System.ArgumentException ) );
			  Exception.expectMessage( "Multiple occurrences of key 'k1'" );

			  // When
			  _converter.apply( mapString );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldHandleNestedMaps()
		 public virtual void ShouldHandleNestedMaps()
		 {
			  // Given
			  string mapString = "{k1: 1337, k2: { k1 : 1337, k2: {k1: 1337}}}";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  IDictionary<string, object> map1 = ( IDictionary<string, object> ) converted.Value();
			  IDictionary<string, object> map2 = ( IDictionary<string, object> ) map1["k2"];
			  IDictionary<string, object> map3 = ( IDictionary<string, object> ) map2["k2"];
			  assertThat( map1["k1"], equalTo( 1337L ) );
			  assertThat( map2["k1"], equalTo( 1337L ) );
			  assertThat( map3["k1"], equalTo( 1337L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnMalformedMap()
		 public virtual void ShouldFailOnMalformedMap()
		 {
			  // Given
			  string mapString = "{k1: 2.718281828, k2: 'e'}}";

			  // Expect
			  Exception.expect( typeof( System.ArgumentException ) );
			  Exception.expectMessage( "{k1: 2.718281828, k2: 'e'}} contains unbalanced '{', '}'." );

			  // When
			  _converter.apply( mapString );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldHandleMapsWithLists()
		 public virtual void ShouldHandleMapsWithLists()
		 {
			  // Given
			  string mapString = "{k1: [1337, 42]}";

			  // When
			  DefaultParameterValue converted = _converter.apply( mapString );

			  // Then
			  IDictionary<string, object> map1 = ( IDictionary<string, object> ) converted.Value();
			  assertThat( map1["k1"], equalTo( asList( 1337L, 42L ) ) );

		 }
	}

}