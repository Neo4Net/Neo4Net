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
namespace Neo4Net.Bolt.v1.transport.integration
{
	using CoreMatchers = org.hamcrest.CoreMatchers;
	using Matcher = org.hamcrest.Matcher;
	using Matchers = org.hamcrest.Matchers;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4NetPackV1 = Neo4Net.Bolt.v1.messaging.Neo4NetPackV1;
	using AckFailureMessage = Neo4Net.Bolt.v1.messaging.request.AckFailureMessage;
	using DiscardAllMessage = Neo4Net.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using InputPosition = Neo4Net.GraphDb.InputPosition;
	using SeverityLevel = Neo4Net.GraphDb.SeverityLevel;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasEntry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.messaging.util.MessageMatchers.hasNotification;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.messaging.util.MessageMatchers.msgFailure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.messaging.util.MessageMatchers.msgIgnored;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.messaging.util.MessageMatchers.msgRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.messaging.util.MessageMatchers.msgSuccess;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.runtime.spi.StreamMatchers.eqRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.v1.transport.integration.TransportTestUtil.eventuallyReceives;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringValue;

	public class TransportSessionIT : AbstractBoltTransportsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4NetWithSocket server = new Neo4NetWithSocket(getClass(), settings -> settings.put(Neo4Net.graphdb.factory.GraphDatabaseSettings.auth_enabled.name(), "false"));
		 public Neo4NetWithSocket Server = new Neo4NetWithSocket( this.GetType(), settings => settings.put(GraphDatabaseSettings.auth_enabled.name(), "false") );

		 private new HostnamePort _address;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _address = Server.lookupDefaultConnector();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNegotiateProtocolVersion() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNegotiateProtocolVersion()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() );

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNilOnNoApplicableVersion() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnNilOnNoApplicableVersion()
		 {
			  // When
			  Connection.connect( _address ).send( Util.acceptedVersions( 1337, 0, 0, 0 ) );

			  // Then
			  assertThat( Connection, eventuallyReceives( new sbyte[]{ 0, 0, 0, 0 } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunSimpleStatement() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunSimpleStatement()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", emptyMap()), new RunMessage("UNWIND [1,2,3] AS a RETURN a, a * a AS a_squared"), PullAllMessage.INSTANCE));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryTypeMatcher = hasEntry(is("type"), equalTo("r"));
			  Matcher<IDictionary<string, ?>> entryTypeMatcher = hasEntry( @is( "type" ), equalTo( "r" ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryFieldMatcher = hasEntry(is("fields"), equalTo(asList("a", "a_squared")));
			  Matcher<IDictionary<string, ?>> entryFieldMatcher = hasEntry( @is( "fields" ), equalTo( asList( "a", "a_squared" ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess(CoreMatchers.allOf(entryFieldMatcher, hasKey("result_available_after"))), msgRecord(eqRecord(equalTo(longValue(1L)), equalTo(longValue(1L)))), msgRecord(eqRecord(equalTo(longValue(2L)), equalTo(longValue(4L)))), msgRecord(eqRecord(equalTo(longValue(3L)), equalTo(longValue(9L)))), msgSuccess(CoreMatchers.allOf(entryTypeMatcher, hasKey("result_consumed_after"))) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWithMetadataToDiscardAll() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWithMetadataToDiscardAll()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", emptyMap()), new RunMessage("UNWIND [1,2,3] AS a RETURN a, a * a AS a_squared"), DiscardAllMessage.INSTANCE));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryFieldsMatcher = hasEntry(is("fields"), equalTo(asList("a", "a_squared")));
			  Matcher<IDictionary<string, ?>> entryFieldsMatcher = hasEntry( @is( "fields" ), equalTo( asList( "a", "a_squared" ) ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryTypeMatcher = hasEntry(is("type"), equalTo("r"));
			  Matcher<IDictionary<string, ?>> entryTypeMatcher = hasEntry( @is( "type" ), equalTo( "r" ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess(CoreMatchers.allOf(entryFieldsMatcher, hasKey("result_available_after"))), msgSuccess(CoreMatchers.allOf(entryTypeMatcher, hasKey("result_consumed_after"))) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRunQueryAfterAckFailure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRunQueryAfterAckFailure()
		 {
			  // Given
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", emptyMap()), new RunMessage("QINVALID"), PullAllMessage.INSTANCE));

			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgFailure(Neo4Net.Kernel.Api.Exceptions.Status_Statement.SyntaxError, string.Format("Invalid input 'Q': expected <init> (line 1, column 1 (offset: 0))%n" + "\"QINVALID\"%n" + " ^")), msgIgnored() ) );

			  // When
			  Connection.send( Util.chunk( AckFailureMessage.INSTANCE, new RunMessage( "RETURN 1" ), PullAllMessage.INSTANCE ) );

			  // Then
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess(), msgRecord(eqRecord(equalTo(longValue(1L)))), msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunProcedure()
		 {
			  // Given
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", emptyMap()), new RunMessage("CREATE (n:Test {age: 2}) RETURN n.age AS age"), PullAllMessage.INSTANCE));

			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> ageMatcher = hasEntry(is("fields"), equalTo(singletonList("age")));
			  Matcher<IDictionary<string, ?>> ageMatcher = hasEntry( @is( "fields" ), equalTo( singletonList( "age" ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess(CoreMatchers.allOf(ageMatcher, hasKey("result_available_after"))), msgRecord(eqRecord(equalTo(longValue(2L)))), msgSuccess() ) );

			  // When
			  Connection.send( Util.chunk( new RunMessage( "CALL db.labels() YIELD label" ), PullAllMessage.INSTANCE ) );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryFieldsMatcher = hasEntry(is("fields"), equalTo(singletonList("label")));
			  Matcher<IDictionary<string, ?>> entryFieldsMatcher = hasEntry( @is( "fields" ), equalTo( singletonList( "label" ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess( CoreMatchers.allOf( entryFieldsMatcher, hasKey( "result_available_after" ) ) ), msgRecord( eqRecord( Matchers.equalTo( stringValue( "Test" ) ) ) ), msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDeletedNodes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleDeletedNodes()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", emptyMap()), new RunMessage("CREATE (n:Test) DELETE n RETURN n"), PullAllMessage.INSTANCE));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryFieldsMatcher = hasEntry(is("fields"), equalTo(singletonList("n")));
			  Matcher<IDictionary<string, ?>> entryFieldsMatcher = hasEntry( @is( "fields" ), equalTo( singletonList( "n" ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess(CoreMatchers.allOf(entryFieldsMatcher, hasKey("result_available_after"))) ) );

			  //
			  //Record(0x71) {
			  //    fields: [ Node(0x4E) {
			  //                 id: 00
			  //                 labels: [] (90)
			  //                  props: {} (A)]
			  //}
			  assertThat( Connection, eventuallyReceives( Bytes( 0x00, 0x08, 0xB1, 0x71, 0x91, 0xB3, 0x4E, 0x00, 0x90, 0xA0, 0x00, 0x00 ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDeletedRelationships() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleDeletedRelationships()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", emptyMap()), new RunMessage("CREATE ()-[r:T {prop: 42}]->() DELETE r RETURN r"), PullAllMessage.INSTANCE));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryFieldsMatcher = hasEntry(is("fields"), equalTo(singletonList("r")));
			  Matcher<IDictionary<string, ?>> entryFieldsMatcher = hasEntry( @is( "fields" ), equalTo( singletonList( "r" ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess(CoreMatchers.allOf(entryFieldsMatcher, hasKey("result_available_after"))) ) );

			  //
			  //Record(0x71) {
			  //    fields: [ Relationship(0x52) {
			  //                 relId: 00
			  //                 startId: 00
			  //                 endId: 01
			  //                 type: "T" (81 54)
			  //                 props: {} (A0)]
			  //}
			  assertThat( Connection, eventuallyReceives( Bytes( 0x00, 0x0B, 0xB1, 0x71, 0x91, 0xB5, 0x52, 0x00, 0x00, 0x01, 0x81, 0x54, 0xA0, 0x00, 0x00 ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLeakStatsToNextStatement() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLeakStatsToNextStatement()
		 {
			  // Given
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", emptyMap()), new RunMessage("CREATE (n)"), PullAllMessage.INSTANCE));
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess(), msgSuccess() ) );

			  // When
			  Connection.send( Util.chunk( new RunMessage( "RETURN 1" ), PullAllMessage.INSTANCE ) );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> typeMatcher = hasEntry(is("type"), equalTo("r"));
			  Matcher<IDictionary<string, ?>> typeMatcher = hasEntry( @is( "type" ), equalTo( "r" ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgRecord(eqRecord(equalTo(longValue(1L)))), msgSuccess(CoreMatchers.allOf(typeMatcher, hasKey("result_consumed_after"))) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendNotifications() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendNotifications()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", emptyMap()), new RunMessage("EXPLAIN MATCH (a:THIS_IS_NOT_A_LABEL) RETURN count(*)"), PullAllMessage.INSTANCE));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess(), hasNotification(new TestNotification("Neo.ClientNotification.Statement.UnknownLabelWarning", "The provided label is not in the database.", "One of the labels in your query is not available in the database, " + "make sure you didn't misspell it or that the label is available when " + "you run this statement in your application (the missing label name is: " + "THIS_IS_NOT_A_LABEL)", SeverityLevel.WARNING, new InputPosition(17, 1, 18))) ) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailNicelyOnPointsWhenProtocolDoesNotSupportThem() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailNicelyOnPointsWhenProtocolDoesNotSupportThem()
		 {
			  // only V1 protocol does not support points
			  assumeThat( Neo4NetPack.version(), equalTo(Neo4NetPackV1.VERSION) );

			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", emptyMap()), new RunMessage("RETURN point({x:13, y:37, crs:'cartesian'}) as p"), PullAllMessage.INSTANCE));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> fieldsMatcher = hasEntry(is("fields"), equalTo(singletonList("p")));
			  Matcher<IDictionary<string, ?>> fieldsMatcher = hasEntry( @is( "fields" ), equalTo( singletonList( "p" ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess(CoreMatchers.allOf(fieldsMatcher, hasKey("result_available_after"))), msgFailure(Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, "Point is not supported as a return type in Bolt") ) );
		 }

		 private sbyte[] Bytes( params int[] ints )
		 {
			  sbyte[] bytes = new sbyte[ints.Length];
			  for ( int i = 0; i < ints.Length; i++ )
			  {
					bytes[i] = ( sbyte ) ints[i];
			  }
			  return bytes;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailNicelyOnNullKeysInMap() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailNicelyOnNullKeysInMap()
		 {
			  //Given
			  Dictionary<string, object> @params = new Dictionary<string, object>();
			  Dictionary<string, object> inner = new Dictionary<string, object>();
			  inner[null] = 42L;
			  inner["foo"] = 1337L;
			  @params["p"] = inner;

			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", emptyMap()), new RunMessage("RETURN {p}", ValueUtils.asMapValue(@params)), PullAllMessage.INSTANCE));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgFailure(Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, "Value `null` is not supported as key in maps, must be a non-nullable string."), msgIgnored() ) );

			  Connection.send( Util.chunk( AckFailureMessage.INSTANCE, new RunMessage( "RETURN 1" ), PullAllMessage.INSTANCE ) );

			  // Then
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess(), msgRecord(eqRecord(equalTo(longValue(1L)))), msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailNicelyWhenDroppingUnknownIndex() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailNicelyWhenDroppingUnknownIndex()
		 {
			  // When
			  Connection.connect( _address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", emptyMap()), new RunMessage("DROP INDEX on :Movie12345(id)"), PullAllMessage.INSTANCE));

			  // Then
			  assertThat( Connection, Util.eventuallyReceivesSelectedProtocolVersion() );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgFailure(Neo4Net.Kernel.Api.Exceptions.Status_Schema.IndexDropFailed, "Unable to drop index on :Movie12345(id): No such INDEX ON :Movie12345(id)."), msgIgnored() ) );
		 }
	}

}