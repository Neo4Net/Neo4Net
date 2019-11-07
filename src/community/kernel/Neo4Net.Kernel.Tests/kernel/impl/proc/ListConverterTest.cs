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


	using DefaultParameterValue = Neo4Net.Kernel.Api.Internal.procs.DefaultParameterValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.procs.DefaultParameterValue.ntList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTAny;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTFloat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTInteger;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTString;

	public class ListConverterTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNullString()
		 public virtual void ShouldHandleNullString()
		 {
			  // Given
			  ListConverter converter = new ListConverter( typeof( string ), NTString );
			  string listString = "null";

			  // When
			  DefaultParameterValue converted = converter.Apply( listString );

			  // Then
			  assertThat( converted, equalTo( ntList( null, NTString ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleEmptyList()
		 public virtual void ShouldHandleEmptyList()
		 {
			  // Given
			  ListConverter converter = new ListConverter( typeof( string ), NTString );
			  string listString = "[]";

			  // When
			  DefaultParameterValue converted = converter.Apply( listString );

			  // Then
			  assertThat( converted, equalTo( ntList( emptyList(), NTString ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleEmptyListWithSpaces()
		 public virtual void ShouldHandleEmptyListWithSpaces()
		 {
			  // Given
			  ListConverter converter = new ListConverter( typeof( string ), NTString );
			  string listString = " [  ]   ";

			  // When
			  DefaultParameterValue converted = converter.Apply( listString );

			  // Then
			  assertThat( converted, equalTo( ntList( emptyList(), NTString ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSingleQuotedValue()
		 public virtual void ShouldHandleSingleQuotedValue()
		 {
			  // Given
			  ListConverter converter = new ListConverter( typeof( string ), NTString );
			  string listString = "['foo', 'bar']";

			  // When
			  DefaultParameterValue converted = converter.Apply( listString );

			  // Then
			  assertThat( converted, equalTo( ntList( asList( "foo", "bar" ), NTString ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDoubleQuotedValue()
		 public virtual void ShouldHandleDoubleQuotedValue()
		 {
			  // Given
			  ListConverter converter = new ListConverter( typeof( string ), NTString );
			  string listString = "[\"foo\", \"bar\"]";

			  // When
			  DefaultParameterValue converted = converter.Apply( listString );

			  // Then
			  assertThat( converted, equalTo( ntList( asList( "foo", "bar" ), NTString ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleIntegerValue()
		 public virtual void ShouldHandleIntegerValue()
		 {
			  // Given
			  ListConverter converter = new ListConverter( typeof( Long ), NTInteger );
			  string listString = "[1337, 42]";

			  // When
			  DefaultParameterValue converted = converter.Apply( listString );

			  // Then
			  assertThat( converted, equalTo( ntList( asList( 1337L, 42L ), NTInteger ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleFloatValue()
		 public virtual void ShouldHandleFloatValue()
		 {
			  // Given
			  ListConverter converter = new ListConverter( typeof( Double ), NTFloat );
			  string listSting = "[2.718281828, 3.14]";

			  // When
			  DefaultParameterValue converted = converter.Apply( listSting );

			  // Then
			  assertThat( converted, equalTo( ntList( asList( 2.718281828, 3.14 ), NTFloat ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNullValue()
		 public virtual void ShouldHandleNullValue()
		 {
			  // Given
			  ListConverter converter = new ListConverter( typeof( Double ), NTFloat );
			  string listString = "[null]";

			  // When
			  DefaultParameterValue converted = converter.Apply( listString );

			  // Then
			  assertThat( converted, equalTo( ntList( singletonList( null ), NTFloat ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleBooleanValues()
		 public virtual void ShouldHandleBooleanValues()
		 {
			  // Given
			  ListConverter converter = new ListConverter( typeof( Boolean ), NTBoolean );
			  string mapString = "[false, true]";

			  // When
			  DefaultParameterValue converted = converter.Apply( mapString );

			  // Then
			  assertThat( converted, equalTo( ntList( asList( false, true ), NTBoolean ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldHandleNestedLists()
		 public virtual void ShouldHandleNestedLists()
		 {
			  // Given
			  ParameterizedType type = mock( typeof( ParameterizedType ) );
			  when( type.ActualTypeArguments ).thenReturn( new Type[]{ typeof( object ) } );
			  ListConverter converter = new ListConverter( type, NTList( NTAny ) );
			  string mapString = "[42, [42, 1337]]";

			  // When
			  DefaultParameterValue converted = converter.Apply( mapString );

			  // Then
			  IList<object> list = ( IList<object> ) converted.Value();
			  assertThat( list[0], equalTo( 42L ) );
			  assertThat( list[1], equalTo( asList( 42L, 1337L ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnInvalidMixedTyoes()
		 public virtual void ShouldFailOnInvalidMixedTyoes()
		 {
			  // Given
			  ListConverter converter = new ListConverter( typeof( Long ), NTInteger );
			  string listString = "[1337, 'forty-two']";

			  // Expect
			  Exception.expect( typeof( System.ArgumentException ) );
			  Exception.expectMessage( "Expects a list of Long but got a list of String" );

			  // When
			  converter.Apply( listString );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPassOnValidMixedTyoes()
		 public virtual void ShouldPassOnValidMixedTyoes()
		 {
			  // Given
			  ListConverter converter = new ListConverter( typeof( object ), NTAny );
			  string listString = "[1337, 'forty-two']";

			  // When
			  DefaultParameterValue value = converter.Apply( listString );

			  // Then
			  assertThat( value, equalTo( ntList( asList( 1337L, "forty-two" ), NTAny ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldHandleListsOfMaps()
		 public virtual void ShouldHandleListsOfMaps()
		 {
			  // Given
			  ListConverter converter = new ListConverter( typeof( System.Collections.IDictionary ), NTMap );
			  string mapString = "[{k1: 42}, {k1: 1337}]";

			  // When
			  DefaultParameterValue converted = converter.Apply( mapString );

			  // Then
			  IList<object> list = ( IList<object> ) converted.Value();
			  assertThat( list[0], equalTo( map( "k1", 42L ) ) );
			  assertThat( list[1], equalTo( map( "k1", 1337L ) ) );
		 }
	}

}