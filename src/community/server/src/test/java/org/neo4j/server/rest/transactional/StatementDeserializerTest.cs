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
namespace Neo4Net.Server.rest.transactional
{
	using Test = org.junit.Test;


	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Neo4NetError = Neo4Net.Server.rest.transactional.error.Neo4NetError;
	using UTF8 = Neo4Net.Strings.UTF8;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.domain.JsonHelper.createJsonFrom;

	public class StatementDeserializerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("unchecked") public void shouldDeserializeSingleStatement()
		 public virtual void ShouldDeserializeSingleStatement()
		 {
			  // Given
			  string json = createJsonFrom( map( "statements", asList( map( "statement", "Blah blah", "parameters", map( "one", 12 ) ) ) ) );

			  // When
			  StatementDeserializer de = new StatementDeserializer( new MemoryStream( UTF8.encode( json ) ) );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( de.HasNext(), equalTo(true) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  Statement stmt = de.Next();

			  assertThat( stmt.StatementConflict(), equalTo("Blah blah") );
			  assertThat( stmt.Parameters(), equalTo(map("one", 12)) );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( de.HasNext(), equalTo(false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectMapWithADifferentFieldBeforeStatement()
		 public virtual void ShouldRejectMapWithADifferentFieldBeforeStatement()
		 {
			  // NOTE: We don't really want this behaviour, but it's a symptom of keeping
			  // streaming behaviour while moving the statement list into a map.

			  string json = "{ \"timeout\" : 200, \"statements\" : [ { \"statement\" : \"ignored\", \"parameters\" : {}} ] }";

			  AssertYieldsErrors( json, new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, new DeserializationException( "Unable to deserialize request. Expected first field to be 'statements', but was 'timeout'." ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTotallyIgnoreInvalidJsonAfterStatementArrayHasFinished()
		 public virtual void ShouldTotallyIgnoreInvalidJsonAfterStatementArrayHasFinished()
		 {
			  // NOTE: We don't really want this behaviour, but it's a symptom of keeping
			  // streaming behaviour while moving the statement list into a map.

			  // Given
			  string json = "{ \"statements\" : [ { \"statement\" : \"Blah blah\", \"parameters\" : {\"one\" : 12}} ] " +
						 "totally invalid json is totally ignored";

			  // When
			  StatementDeserializer de = new StatementDeserializer( new MemoryStream( UTF8.encode( json ) ) );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( de.HasNext(), equalTo(true) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  Statement stmt = de.Next();

			  assertThat( stmt.StatementConflict(), equalTo("Blah blah") );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( de.HasNext(), equalTo(false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreUnknownFields()
		 public virtual void ShouldIgnoreUnknownFields()
		 {
			  // Given
			  string json = "{ \"statements\" : [ { \"a\" : \"\", \"b\" : { \"k\":1 }, \"statement\" : \"blah\" } ] }";

			  // When
			  StatementDeserializer de = new StatementDeserializer( new MemoryStream( UTF8.encode( json ) ) );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( de.HasNext(), equalTo(true) );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( de.Next().statement(), equalTo("blah") );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( de.HasNext(), equalTo(false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTakeParametersBeforeStatement()
		 public virtual void ShouldTakeParametersBeforeStatement()
		 {
			  // Given
			  string json = "{ \"statements\" : [ { \"a\" : \"\", \"parameters\" : { \"k\":1 }, \"statement\" : \"blah\"}]}";

			  // When
			  StatementDeserializer de = new StatementDeserializer( new MemoryStream( UTF8.encode( json ) ) );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( de.HasNext(), equalTo(true) );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  Statement stmt = de.Next();
			  assertThat( stmt.StatementConflict(), equalTo("blah") );
			  assertThat( stmt.Parameters(), equalTo(map("k", 1)) );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( de.HasNext(), equalTo(false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTreatEmptyInputStreamAsEmptyStatementList()
		 public virtual void ShouldTreatEmptyInputStreamAsEmptyStatementList()
		 {
			  // Given
			  sbyte[] json = new sbyte[0];

			  // When
			  StatementDeserializer de = new StatementDeserializer( new MemoryStream( json ) );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( de.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( de.Errors().hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("unchecked") public void shouldDeserializeMultipleStatements()
		 public virtual void ShouldDeserializeMultipleStatements()
		 {
			  // Given
			  string json = createJsonFrom( map( "statements", asList( map( "statement", "Blah blah", "parameters", map( "one", 12 ) ), map( "statement", "Blah bluh", "parameters", map( "asd", asList( "one, two" ) ) ) ) ) );

			  // When
			  StatementDeserializer de = new StatementDeserializer( new MemoryStream( UTF8.encode( json ) ) );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( de.HasNext(), equalTo(true) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  Statement stmt = de.Next();

			  assertThat( stmt.StatementConflict(), equalTo("Blah blah") );
			  assertThat( stmt.Parameters(), equalTo(map("one", 12)) );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( de.HasNext(), equalTo(true) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  Statement stmt2 = de.Next();

			  assertThat( stmt2.StatementConflict(), equalTo("Blah bluh") );
			  assertThat( stmt2.Parameters(), equalTo(map("asd", asList("one, two"))) );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( de.HasNext(), equalTo(false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotThrowButReportErrorOnInvalidInput()
		 public virtual void ShouldNotThrowButReportErrorOnInvalidInput()
		 {
			  AssertYieldsErrors( "{}", new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, new DeserializationException( "Unable to " + "deserialize request. " + "Expected [START_OBJECT, FIELD_NAME, START_ARRAY], " + "found [START_OBJECT, END_OBJECT, null]." ) ) );

			  AssertYieldsErrors( "{ \"statements\":\"WAIT WAT A STRING NOO11!\" }", new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, new DeserializationException( "Unable to " + "deserialize request. Expected [START_OBJECT, FIELD_NAME, START_ARRAY], found [START_OBJECT, " + "FIELD_NAME, VALUE_STRING]." ) ) );

			  AssertYieldsErrors( "[{]}", new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, new DeserializationException( "Unable to deserialize request: Unexpected close marker ']': " + "expected '}' " + "(for OBJECT starting at [Source: TestInputStream; line: 1, column: 1])\n " + "at [Source: TestInputStream; line: 1, column: 4]" ) ) );

			  AssertYieldsErrors( "{ \"statements\" : \"ITS A STRING\" }", new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, new DeserializationException( "Unable to deserialize request. " + "Expected [START_OBJECT, FIELD_NAME, START_ARRAY], " + "found [START_OBJECT, FIELD_NAME, VALUE_STRING]." ) ) );

			  AssertYieldsErrors( "{ \"statements\" : [ { \"statement\" : [\"dd\"] } ] }", new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, new DeserializationException( "Unable to deserialize request: Can not deserialize instance of" + " java.lang.String out of START_ARRAY token\n at [Source: TestInputStream; line: 1, " + "column: 22]" ) ) );

			  AssertYieldsErrors( "{ \"statements\" : [ { \"statement\" : \"stmt\", \"parameters\" : [\"AN ARRAY!!\"] } ] }", new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, new DeserializationException( "Unable to deserialize request: Can not deserialize instance of" + " java.util.LinkedHashMap out of START_ARRAY token\n at [Source: TestInputStream; " + "line: 1, column: 42]" ) ) );
		 }

		 private void AssertYieldsErrors( string json, params Neo4NetError[] expectedErrors )
		 {
			  StatementDeserializer de = new StatementDeserializer( new ByteArrayInputStreamAnonymousInnerClass( this, UTF8.encode( json ) ) );
			  while ( de.MoveNext() )
			  {
					de.Current;
			  }

			  IEnumerator<Neo4NetError> actual = de.Errors();
			  IEnumerator<Neo4NetError> expected = asList( expectedErrors ).GetEnumerator();
			  while ( actual.MoveNext() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( expected.hasNext() );
					Neo4NetError error = actual.Current;
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Neo4NetError expectedError = expected.next();

					assertThat( error.Message, equalTo( expectedError.Message ) );
					assertThat( error.Status(), equalTo(expectedError.Status()) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( expected.hasNext() );
		 }

		 private class ByteArrayInputStreamAnonymousInnerClass : MemoryStream
		 {
			 private readonly StatementDeserializerTest _outerInstance;

			 public ByteArrayInputStreamAnonymousInnerClass( StatementDeserializerTest outerInstance, sbyte[] encode ) : base( encode )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override string ToString()
			 {
				  return "TestInputStream";
			 }
		 }

	}

}