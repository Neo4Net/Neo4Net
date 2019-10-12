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
namespace Neo4Net.Bolt.v3.runtime.integration
{
	using Matcher = org.hamcrest.Matcher;
	using Matchers = org.hamcrest.Matchers;
	using Test = org.junit.Test;


	using Neo4jPack = Neo4Net.Bolt.messaging.Neo4jPack;
	using DiscardAllMessage = Neo4Net.Bolt.v1.messaging.request.DiscardAllMessage;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Neo4Net.Bolt.v1.messaging.request.ResetMessage;
	using PackedOutputArray = Neo4Net.Bolt.v1.packstream.PackedOutputArray;
	using Bookmark = Neo4Net.Bolt.v1.runtime.bookmarking.Bookmark;
	using BeginMessage = Neo4Net.Bolt.v3.messaging.request.BeginMessage;
	using HelloMessage = Neo4Net.Bolt.v3.messaging.request.HelloMessage;
	using RunMessage = Neo4Net.Bolt.v3.messaging.request.RunMessage;
	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using KernelTransactionHandle = Neo4Net.Kernel.api.KernelTransactionHandle;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using KernelTransactions = Neo4Net.Kernel.Impl.Api.KernelTransactions;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.allOf;
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
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgFailure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgIgnored;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgSuccess;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.runtime.spi.StreamMatchers.eqRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.transport.integration.TransportTestUtil.eventuallyReceives;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.request.CommitMessage.COMMIT_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.request.RollbackMessage.ROLLBACK_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.ValueUtils.asMapValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;

	public class BoltV3TransportIT : BoltV3TransportBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNegotiateProtocolV3() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNegotiateProtocolV3()
		 {
			  Connection.connect( Address ).send( Util.acceptedVersions( 3, 0, 0, 0 ) ).send( Util.chunk( new HelloMessage( map( "user_agent", USER_AGENT ) ) ) );

			  assertThat( Connection, eventuallyReceives( new sbyte[]{ 0, 0, 0, 3 } ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess( allOf( hasKey( "server" ), hasKey( "connection_id" ) ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNegotiateProtocolV3WhenClientSupportsBothV1V2AndV3() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNegotiateProtocolV3WhenClientSupportsBothV1V2AndV3()
		 {
			  Connection.connect( Address ).send( Util.acceptedVersions( 3, 2, 1, 0 ) ).send( Util.chunk( new HelloMessage( map( "user_agent", USER_AGENT ) ) ) );

			  assertThat( Connection, eventuallyReceives( new sbyte[]{ 0, 0, 0, 3 } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunSimpleStatement() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunSimpleStatement()
		 {
			  // When
			  NegotiateBoltV3();
			  Connection.send( Util.chunk( new RunMessage( "UNWIND [1,2,3] AS a RETURN a, a * a AS a_squared" ), PullAllMessage.INSTANCE ) );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryFieldMatcher = hasEntry(is("fields"), equalTo(asList("a", "a_squared")));
			  Matcher<IDictionary<string, ?>> entryFieldMatcher = hasEntry( @is( "fields" ), equalTo( asList( "a", "a_squared" ) ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryTypeMatcher = hasEntry(is("type"), equalTo("r"));
			  Matcher<IDictionary<string, ?>> entryTypeMatcher = hasEntry( @is( "type" ), equalTo( "r" ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess( allOf( entryFieldMatcher, hasKey( "t_first" ) ) ), msgRecord( eqRecord( equalTo( longValue( 1L ) ), equalTo( longValue( 1L ) ) ) ), msgRecord( eqRecord( equalTo( longValue( 2L ) ), equalTo( longValue( 4L ) ) ) ), msgRecord( eqRecord( equalTo( longValue( 3L ) ), equalTo( longValue( 9L ) ) ) ), msgSuccess( allOf( entryTypeMatcher, hasKey( "t_last" ), hasKey( "bookmark" ) ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWithMetadataToDiscardAll() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWithMetadataToDiscardAll()
		 {
			  // When
			  NegotiateBoltV3();
			  Connection.send( Util.chunk( new RunMessage( "UNWIND [1,2,3] AS a RETURN a, a * a AS a_squared" ), DiscardAllMessage.INSTANCE ) );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryTypeMatcher = hasEntry(is("type"), equalTo("r"));
			  Matcher<IDictionary<string, ?>> entryTypeMatcher = hasEntry( @is( "type" ), equalTo( "r" ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryFieldsMatcher = hasEntry(is("fields"), equalTo(asList("a", "a_squared")));
			  Matcher<IDictionary<string, ?>> entryFieldsMatcher = hasEntry( @is( "fields" ), equalTo( asList( "a", "a_squared" ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess( allOf( entryFieldsMatcher, hasKey( "t_first" ) ) ), msgSuccess( allOf( entryTypeMatcher, hasKey( "t_last" ), hasKey( "bookmark" ) ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunSimpleStatementInTx() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunSimpleStatementInTx()
		 {
			  // When
			  NegotiateBoltV3();
			  Connection.send( Util.chunk( new BeginMessage(), new RunMessage("UNWIND [1,2,3] AS a RETURN a, a * a AS a_squared"), PullAllMessage.INSTANCE, COMMIT_MESSAGE ) );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryFieldMatcher = hasEntry(is("fields"), equalTo(asList("a", "a_squared")));
			  Matcher<IDictionary<string, ?>> entryFieldMatcher = hasEntry( @is( "fields" ), equalTo( asList( "a", "a_squared" ) ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryTypeMatcher = hasEntry(is("type"), equalTo("r"));
			  Matcher<IDictionary<string, ?>> entryTypeMatcher = hasEntry( @is( "type" ), equalTo( "r" ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess(allOf(entryFieldMatcher, hasKey("t_first"))), msgRecord(eqRecord(equalTo(longValue(1L)), equalTo(longValue(1L)))), msgRecord(eqRecord(equalTo(longValue(2L)), equalTo(longValue(4L)))), msgRecord(eqRecord(equalTo(longValue(3L)), equalTo(longValue(9L)))), msgSuccess(allOf(entryTypeMatcher, hasKey("t_last"), not(hasKey("bookmark")))), msgSuccess(allOf(hasKey("bookmark"))) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowRollbackSimpleStatementInTx() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowRollbackSimpleStatementInTx()
		 {
			  // When
			  NegotiateBoltV3();
			  Connection.send( Util.chunk( new BeginMessage(), new RunMessage("UNWIND [1,2,3] AS a RETURN a, a * a AS a_squared"), PullAllMessage.INSTANCE, ROLLBACK_MESSAGE ) );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryFieldMatcher = hasEntry(is("fields"), equalTo(asList("a", "a_squared")));
			  Matcher<IDictionary<string, ?>> entryFieldMatcher = hasEntry( @is( "fields" ), equalTo( asList( "a", "a_squared" ) ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryTypeMatcher = hasEntry(is("type"), equalTo("r"));
			  Matcher<IDictionary<string, ?>> entryTypeMatcher = hasEntry( @is( "type" ), equalTo( "r" ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess(allOf(entryFieldMatcher, hasKey("t_first"))), msgRecord(eqRecord(equalTo(longValue(1L)), equalTo(longValue(1L)))), msgRecord(eqRecord(equalTo(longValue(2L)), equalTo(longValue(4L)))), msgRecord(eqRecord(equalTo(longValue(3L)), equalTo(longValue(9L)))), msgSuccess(allOf(entryTypeMatcher, hasKey("t_last"), not(hasKey("bookmark")))), msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRunQueryAfterReset() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRunQueryAfterReset()
		 {
			  // Given
			  NegotiateBoltV3();
			  Connection.send( Util.chunk( new RunMessage( "QINVALID" ), PullAllMessage.INSTANCE ) );

			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Statement.SyntaxError, string.Format( "Invalid input 'Q': expected <init> (line 1, column 1 (offset: 0))%n" + "\"QINVALID\"%n" + " ^" ) ), msgIgnored() ) );

			  // When
			  Connection.send( Util.chunk( ResetMessage.INSTANCE, new RunMessage( "RETURN 1" ), PullAllMessage.INSTANCE ) );

			  // Then
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess(), msgRecord(eqRecord(equalTo(longValue(1L)))), msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunProcedure()
		 {
			  // Given
			  NegotiateBoltV3();
			  Connection.send( Util.chunk( new RunMessage( "CREATE (n:Test {age: 2}) RETURN n.age AS age" ), PullAllMessage.INSTANCE ) );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> ageMatcher = hasEntry(is("fields"), equalTo(singletonList("age")));
			  Matcher<IDictionary<string, ?>> ageMatcher = hasEntry( @is( "fields" ), equalTo( singletonList( "age" ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess( allOf( ageMatcher, hasKey( "t_first" ) ) ), msgRecord( eqRecord( equalTo( longValue( 2L ) ) ) ), msgSuccess() ) );

			  // When
			  Connection.send( Util.chunk( new RunMessage( "CALL db.labels() YIELD label" ), PullAllMessage.INSTANCE ) );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryFieldsMatcher = hasEntry(is("fields"), equalTo(singletonList("label")));
			  Matcher<IDictionary<string, ?>> entryFieldsMatcher = hasEntry( @is( "fields" ), equalTo( singletonList( "label" ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess( allOf( entryFieldsMatcher, hasKey( "t_first" ) ) ), msgRecord( eqRecord( Matchers.equalTo( stringValue( "Test" ) ) ) ), msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDeletedNodes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleDeletedNodes()
		 {
			  // When
			  NegotiateBoltV3();
			  Connection.send( Util.chunk( new RunMessage( "CREATE (n:Test) DELETE n RETURN n" ), PullAllMessage.INSTANCE ) );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryFieldsMatcher = hasEntry(is("fields"), equalTo(singletonList("n")));
			  Matcher<IDictionary<string, ?>> entryFieldsMatcher = hasEntry( @is( "fields" ), equalTo( singletonList( "n" ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess( allOf( entryFieldsMatcher, hasKey( "t_first" ) ) ) ) );

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
			  NegotiateBoltV3();
			  Connection.send( Util.chunk( new RunMessage( "CREATE ()-[r:T {prop: 42}]->() DELETE r RETURN r" ), PullAllMessage.INSTANCE ) );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> entryFieldsMatcher = hasEntry(is("fields"), equalTo(singletonList("r")));
			  Matcher<IDictionary<string, ?>> entryFieldsMatcher = hasEntry( @is( "fields" ), equalTo( singletonList( "r" ) ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess( allOf( entryFieldsMatcher, hasKey( "t_first" ) ) ) ) );

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
			  NegotiateBoltV3();
			  Connection.send( Util.chunk( new RunMessage( "CREATE (n)" ), PullAllMessage.INSTANCE ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess() ) );

			  // When
			  Connection.send( Util.chunk( new RunMessage( "RETURN 1" ), PullAllMessage.INSTANCE ) );

			  // Then
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.hamcrest.Matcher<java.util.Map<? extends String,?>> typeMatcher = hasEntry(is("type"), equalTo("r"));
			  Matcher<IDictionary<string, ?>> typeMatcher = hasEntry( @is( "type" ), equalTo( "r" ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgRecord(eqRecord(equalTo(longValue(1L)))), msgSuccess(allOf(typeMatcher, hasKey("t_last"))) ) );
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
			  NegotiateBoltV3();
			  Connection.send( Util.chunk( new RunMessage( "RETURN {p}", asMapValue( @params ) ), PullAllMessage.INSTANCE ) );

			  // Then
			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, "Value `null` is not supported as key in maps, must be a non-nullable string." ), msgIgnored() ) );

			  Connection.send( Util.chunk( ResetMessage.INSTANCE, new RunMessage( "RETURN 1" ), PullAllMessage.INSTANCE ) );

			  // Then
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess(), msgRecord(eqRecord(equalTo(longValue(1L)))), msgSuccess() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailNicelyWhenDroppingUnknownIndex() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailNicelyWhenDroppingUnknownIndex()
		 {
			  // When
			  NegotiateBoltV3();
			  Connection.send( Util.chunk( new RunMessage( "DROP INDEX on :Movie12345(id)" ), PullAllMessage.INSTANCE ) );

			  // Then
			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Schema.IndexDropFailed, "Unable to drop index on :Movie12345(id): No such INDEX ON :Movie12345(id)." ), msgIgnored() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetTxMetadata() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetTxMetadata()
		 {
			  // Given
			  NegotiateBoltV3();
			  IDictionary<string, object> txMetadata = map( "who-is-your-boss", "Molly-mostly-white" );
			  IDictionary<string, object> msgMetadata = map( "tx_metadata", txMetadata );
			  MapValue meta = asMapValue( msgMetadata );

			  Connection.send( Util.chunk( new BeginMessage( meta ), new RunMessage( "RETURN 1" ), PullAllMessage.INSTANCE ) );

			  // When
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess(), msgRecord(eqRecord(equalTo(longValue(1L)))), msgSuccess() ) );

			  // Then
			  GraphDatabaseAPI gdb = ( GraphDatabaseAPI ) Server.graphDatabaseService();
			  ISet<KernelTransactionHandle> txHandles = gdb.DependencyResolver.resolveDependency( typeof( KernelTransactions ) ).activeTransactions();
			  assertThat( txHandles.Count, equalTo( 1 ) );
			  foreach ( KernelTransactionHandle txHandle in txHandles )
			  {
					assertThat( txHandle.MetaData, equalTo( txMetadata ) );
			  }
			  Connection.send( Util.chunk( ROLLBACK_MESSAGE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendFailureMessageForBeginWithInvalidBookmark() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendFailureMessageForBeginWithInvalidBookmark()
		 {
			  NegotiateBoltV3();
			  string bookmarkString = "Not a good bookmark for BEGIN";
			  IDictionary<string, object> metadata = map( "bookmarks", singletonList( bookmarkString ) );

			  Connection.send( Util.chunk( 32, BeginMessage( metadata ) ) );

			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.InvalidBookmark, bookmarkString ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendFailureMessageForBeginWithInvalidTransactionTimeout() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendFailureMessageForBeginWithInvalidTransactionTimeout()
		 {
			  NegotiateBoltV3();
			  string txTimeout = "Tx timeout can't be a string for BEGIN";
			  IDictionary<string, object> metadata = map( "tx_timeout", txTimeout );

			  Connection.send( Util.chunk( 32, BeginMessage( metadata ) ) );

			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, txTimeout ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendFailureMessageForBeginWithInvalidTransactionMetadata() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendFailureMessageForBeginWithInvalidTransactionMetadata()
		 {
			  NegotiateBoltV3();
			  string txMetadata = "Tx metadata can't be a string for BEGIN";
			  IDictionary<string, object> metadata = map( "tx_metadata", txMetadata );

			  Connection.send( Util.chunk( 32, BeginMessage( metadata ) ) );

			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, txMetadata ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendFailureMessageForRunWithInvalidBookmark() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendFailureMessageForRunWithInvalidBookmark()
		 {
			  NegotiateBoltV3();
			  string bookmarkString = "Not a good bookmark for RUN";
			  IDictionary<string, object> metadata = map( "bookmarks", singletonList( bookmarkString ) );

			  Connection.send( Util.chunk( 32, RunMessage( metadata ) ) );

			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.InvalidBookmark, bookmarkString ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendFailureMessageForRunWithInvalidTransactionTimeout() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendFailureMessageForRunWithInvalidTransactionTimeout()
		 {
			  NegotiateBoltV3();
			  string txTimeout = "Tx timeout can't be a string for RUN";
			  IDictionary<string, object> metadata = map( "tx_timeout", txTimeout );

			  Connection.send( Util.chunk( 32, RunMessage( metadata ) ) );

			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, txTimeout ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendFailureMessageForRunWithInvalidTransactionMetadata() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendFailureMessageForRunWithInvalidTransactionMetadata()
		 {
			  NegotiateBoltV3();
			  string txMetadata = "Tx metadata can't be a string for RUN";
			  IDictionary<string, object> metadata = map( "tx_metadata", txMetadata );

			  Connection.send( Util.chunk( 32, RunMessage( metadata ) ) );

			  assertThat( Connection, Util.eventuallyReceives( msgFailure( Neo4Net.Kernel.Api.Exceptions.Status_Request.Invalid, txMetadata ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnUpdatedBookmarkAfterAutoCommitTransaction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnUpdatedBookmarkAfterAutoCommitTransaction()
		 {
			  NegotiateBoltV3();

			  // bookmark is expected to advance once the auto-commit transaction is committed
			  long lastClosedTransactionId = LastClosedTransactionId;
			  string expectedBookmark = ( new Bookmark( lastClosedTransactionId + 1 ) ).ToString();

			  Connection.send( Util.chunk( new RunMessage( "CREATE ()" ), PullAllMessage.INSTANCE ) );

			  assertThat( Connection, Util.eventuallyReceives( msgSuccess(), msgSuccess(allOf(hasEntry("bookmark", expectedBookmark))) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte[] beginMessage(java.util.Map<String,Object> metadata) throws java.io.IOException
		 private sbyte[] BeginMessage( IDictionary<string, object> metadata )
		 {
			  PackedOutputArray @out = new PackedOutputArray();
			  Neo4Net.Bolt.messaging.Neo4jPack_Packer packer = Util.Neo4jPack.newPacker( @out );

			  packer.PackStructHeader( 1, BeginMessage.SIGNATURE );
			  packer.pack( asMapValue( metadata ) );

			  return @out.Bytes();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte[] runMessage(java.util.Map<String,Object> metadata) throws java.io.IOException
		 private sbyte[] RunMessage( IDictionary<string, object> metadata )
		 {
			  PackedOutputArray @out = new PackedOutputArray();
			  Neo4Net.Bolt.messaging.Neo4jPack_Packer packer = Util.Neo4jPack.newPacker( @out );

			  packer.PackStructHeader( 3, RunMessage.SIGNATURE );
			  packer.Pack( "RETURN 1" );
			  packer.pack( EMPTY_MAP );
			  packer.pack( asMapValue( metadata ) );

			  return @out.Bytes();
		 }

		 private long LastClosedTransactionId
		 {
			 get
			 {
				  DependencyResolver resolver = ( ( GraphDatabaseAPI ) Server.graphDatabaseService() ).DependencyResolver;
				  TransactionIdStore txIdStore = resolver.ResolveDependency( typeof( TransactionIdStore ) );
				  return txIdStore.LastClosedTransactionId;
			 }
		 }
	}

}