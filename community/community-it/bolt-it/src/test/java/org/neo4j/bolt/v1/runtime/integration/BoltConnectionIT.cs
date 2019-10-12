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
namespace Org.Neo4j.Bolt.v1.runtime.integration
{
	using CoreMatchers = org.hamcrest.CoreMatchers;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using BoltResponseHandler = Org.Neo4j.Bolt.runtime.BoltResponseHandler;
	using BoltResult = Org.Neo4j.Bolt.runtime.BoltResult;
	using BoltStateMachine = Org.Neo4j.Bolt.runtime.BoltStateMachine;
	using Neo4jError = Org.Neo4j.Bolt.runtime.Neo4jError;
	using BoltResponseRecorder = Org.Neo4j.Bolt.testing.BoltResponseRecorder;
	using BoltTestUtil = Org.Neo4j.Bolt.testing.BoltTestUtil;
	using RecordedBoltResponse = Org.Neo4j.Bolt.testing.RecordedBoltResponse;
	using BoltResponseMessage = Org.Neo4j.Bolt.v1.messaging.BoltResponseMessage;
	using AckFailureMessage = Org.Neo4j.Bolt.v1.messaging.request.AckFailureMessage;
	using DiscardAllMessage = Org.Neo4j.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Org.Neo4j.Bolt.v1.messaging.request.InitMessage;
	using PullAllMessage = Org.Neo4j.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Org.Neo4j.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Org.Neo4j.Bolt.v1.messaging.request.RunMessage;
	using QueryResult_Record = Org.Neo4j.Cypher.result.QueryResult_Record;
	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using ValueUtils = Org.Neo4j.Kernel.impl.util.ValueUtils;
	using AnyValue = Org.Neo4j.Values.AnyValue;
	using LongValue = Org.Neo4j.Values.Storable.LongValue;
	using TextValue = Org.Neo4j.Values.Storable.TextValue;
	using MapValue = Org.Neo4j.Values.@virtual.MapValue;
	using VirtualValues = Org.Neo4j.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.failedWithStatus;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.succeeded;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.verifyKillsConnection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.NullResponseHandler.nullResponseHandler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.BoltResponseMessage.IGNORED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.BoltResponseMessage.SUCCESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public class BoltConnectionIT
	public class BoltConnectionIT
	{
		 private static readonly MapValue _emptyParams = VirtualValues.EMPTY_MAP;
		 private const string USER_AGENT = "BoltConnectionIT/0.0";
		 private static readonly BoltChannel _boltChannel = BoltTestUtil.newTestBoltChannel();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public SessionRule env = new SessionRule();
		 public SessionRule Env = new SessionRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseConnectionAckFailureBeforeInit() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseConnectionAckFailureBeforeInit()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );

			  // when
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  verifyKillsConnection( () => machine.process(AckFailureMessage.INSTANCE, recorder) );

			  // then
			  assertThat( recorder.NextResponse(), failedWithStatus(Org.Neo4j.Kernel.Api.Exceptions.Status_Request.Invalid) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseConnectionResetBeforeInit() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseConnectionResetBeforeInit()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );

			  // when
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  verifyKillsConnection( () => machine.process(ResetMessage.INSTANCE, recorder) );

			  // then
			  assertThat( recorder.NextResponse(), failedWithStatus(Org.Neo4j.Kernel.Api.Exceptions.Status_Request.Invalid) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseConnectionOnRunBeforeInit() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseConnectionOnRunBeforeInit()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );

			  // when
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  verifyKillsConnection( () => machine.process(new RunMessage("RETURN 1", Map()), recorder) );

			  // then
			  assertThat( recorder.NextResponse(), failedWithStatus(Org.Neo4j.Kernel.Api.Exceptions.Status_Request.Invalid) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseConnectionOnDiscardAllBeforeInit() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseConnectionOnDiscardAllBeforeInit()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );

			  // when
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  verifyKillsConnection( () => machine.process(DiscardAllMessage.INSTANCE, recorder) );

			  // then
			  assertThat( recorder.NextResponse(), failedWithStatus(Org.Neo4j.Kernel.Api.Exceptions.Status_Request.Invalid) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseConnectionOnPullAllBeforeInit() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseConnectionOnPullAllBeforeInit()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );

			  // when
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  verifyKillsConnection( () => machine.process(PullAllMessage.INSTANCE, recorder) );

			  // then
			  assertThat( recorder.NextResponse(), failedWithStatus(Org.Neo4j.Kernel.Api.Exceptions.Status_Request.Invalid) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExecuteStatement() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExecuteStatement()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  // When
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( new RunMessage( "CREATE (n {k:'k'}) RETURN n.k", _emptyParams ), recorder );

			  // Then
			  assertThat( recorder.NextResponse(), succeeded() );

			  // When
			  recorder.Reset();
			  machine.Process( PullAllMessage.INSTANCE, recorder );

			  // Then
			  recorder.NextResponse().assertRecord(0, stringValue("k"));
			  //assertThat( pulling.next(), streamContaining( StreamMatchers.eqRecord( equalTo( "k" ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSucceedOn__run__pullAll__run() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSucceedOn_Run_PullAll_Run()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  // And Given that I've ran and pulled one stream
			  machine.Process( new RunMessage( "RETURN 1", _emptyParams ), nullResponseHandler() );
			  machine.Process( PullAllMessage.INSTANCE, nullResponseHandler() );

			  // When I run a new statement
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( new RunMessage( "RETURN 2", _emptyParams ), recorder );

			  // Then
			  assertThat( recorder.NextResponse(), succeeded() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSucceedOn__run__discardAll__run() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSucceedOn_Run_DiscardAll_Run()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  // And Given that I've ran and pulled one stream
			  machine.Process( new RunMessage( "RETURN 1", _emptyParams ), nullResponseHandler() );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );

			  // When I run a new statement
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( new RunMessage( "RETURN 2", _emptyParams ), recorder );

			  // Then
			  assertThat( recorder.NextResponse(), succeeded() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSucceedOn__run_BEGIN__pullAll__run_COMMIT__pullALL__run_COMMIT() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSucceedOn_RunBEGIN_PullAll_RunCOMMIT_PullALL_RunCOMMIT()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  // And Given that I've ran and pulled one stream
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( new RunMessage( "BEGIN", _emptyParams ), recorder );
			  machine.Process( PullAllMessage.INSTANCE, recorder );
			  machine.Process( new RunMessage( "COMMIT", _emptyParams ), recorder );
			  machine.Process( PullAllMessage.INSTANCE, recorder );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );

			  // When I run a new statement
			  recorder.Reset();
			  machine.Process( new RunMessage( "BEGIN", _emptyParams ), recorder );

			  // Then
			  assertThat( recorder.NextResponse(), succeeded() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSendBookmarkInPullAllResponse() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSendBookmarkInPullAllResponse()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  // And Given that I've ran and pulled one stream
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( new RunMessage( "BEGIN", _emptyParams ), nullResponseHandler() );
			  machine.Process( PullAllMessage.INSTANCE, nullResponseHandler() );
			  machine.Process( new RunMessage( "RETURN 1", _emptyParams ), nullResponseHandler() );
			  machine.Process( PullAllMessage.INSTANCE, nullResponseHandler() );
			  machine.Process( new RunMessage( "COMMIT", _emptyParams ), nullResponseHandler() );
			  machine.Process( PullAllMessage.INSTANCE, recorder );
			  AnyValue bookmark = recorder.NextResponse().metadata("bookmark");
			  assertNotNull( bookmark );
			  string bookmarkStr = ( ( TextValue ) bookmark ).stringValue();

			  // When I run a new statement
			  recorder.Reset();
			  machine.Process( new RunMessage( "BEGIN", Map( "bookmark", bookmarkStr ) ), nullResponseHandler() );
			  machine.Process( PullAllMessage.INSTANCE, recorder );

			  // Then
			  RecordedBoltResponse response = recorder.NextResponse();
			  assertThat( response, succeeded() );
			  assertThat( response.HasMetadata( "bookmark" ), @is( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOn__run__run() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOn_Run_Run()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  // And Given that I've ran one statement
			  machine.Process( new RunMessage( "RETURN 1", _emptyParams ), nullResponseHandler() );

			  // When I run a new statement, before consuming the stream
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  verifyKillsConnection( () => machine.process(new RunMessage("RETURN 1", _emptyParams), recorder) );

			  // Then
			  assertThat( recorder.NextResponse(), failedWithStatus(Org.Neo4j.Kernel.Api.Exceptions.Status_Request.Invalid) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOn__pullAll__pullAll() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOn_PullAll_PullAll()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  // And Given that I've ran and pulled one stream
			  machine.Process( new RunMessage( "RETURN 1", _emptyParams ), nullResponseHandler() );
			  machine.Process( PullAllMessage.INSTANCE, nullResponseHandler() );

			  // Then further attempts to PULL should be treated as protocol violations
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  verifyKillsConnection( () => machine.process(PullAllMessage.INSTANCE, recorder) );

			  // Then
			  assertThat( recorder.NextResponse(), failedWithStatus(Org.Neo4j.Kernel.Api.Exceptions.Status_Request.Invalid) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOn__pullAll__discardAll() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOn_PullAll_DiscardAll()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  // And Given that I've ran and pulled one stream
			  machine.Process( new RunMessage( "RETURN 1", _emptyParams ), nullResponseHandler() );
			  machine.Process( PullAllMessage.INSTANCE, nullResponseHandler() );

			  // When I attempt to pull more items from the stream
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  verifyKillsConnection( () => machine.process(DiscardAllMessage.INSTANCE, recorder) );

			  // Then
			  assertThat( recorder.NextResponse(), failedWithStatus(Org.Neo4j.Kernel.Api.Exceptions.Status_Request.Invalid) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOn__discardAll__discardAll() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOn_DiscardAll_DiscardAll()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  // And Given that I've ran and pulled one stream
			  machine.Process( new RunMessage( "RETURN 1", _emptyParams ), nullResponseHandler() );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );

			  // When I attempt to pull more items from the stream
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  verifyKillsConnection( () => machine.process(DiscardAllMessage.INSTANCE, recorder) );

			  // Then
			  assertThat( recorder.NextResponse(), failedWithStatus(Org.Neo4j.Kernel.Api.Exceptions.Status_Request.Invalid) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOn__discardAll__pullAll() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOn_DiscardAll_PullAll()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  // And Given that I've ran and pulled one stream
			  machine.Process( new RunMessage( "RETURN 1", _emptyParams ), nullResponseHandler() );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );

			  // When I attempt to pull more items from the stream
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  verifyKillsConnection( () => machine.process(PullAllMessage.INSTANCE, recorder) );

			  // Then
			  assertThat( recorder.NextResponse(), failedWithStatus(Org.Neo4j.Kernel.Api.Exceptions.Status_Request.Invalid) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleImplicitCommitFailure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleImplicitCommitFailure()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );
			  machine.Process( new RunMessage( "CREATE (n:Victim)-[:REL]->()", _emptyParams ), nullResponseHandler() );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );

			  // When I perform an action that will fail on commit
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( new RunMessage( "MATCH (n:Victim) DELETE n", _emptyParams ), recorder );
			  // Then the statement running should have succeeded
			  assertThat( recorder.NextResponse(), succeeded() );

			  recorder.Reset();
			  machine.Process( DiscardAllMessage.INSTANCE, recorder );

			  // But the stop should have failed, since it implicitly triggers commit and thus triggers a failure
			  assertThat( recorder.NextResponse(), failedWithStatus(Org.Neo4j.Kernel.Api.Exceptions.Status_Schema.ConstraintValidationFailed) );
			  //assertThat( discarding.next(), failedWith( Status.Schema.ConstraintValidationFailed ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowUserControlledRollbackOnExplicitTxFailure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowUserControlledRollbackOnExplicitTxFailure()
		 {
			  // Given whenever en explicit transaction has a failure,
			  // it is more natural for drivers to see the failure, acknowledge it
			  // and send a `ROLLBACK`, because that means that all failures in the
			  // transaction, be they client-local or inside neo, can be handled the
			  // same way by a driver.
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  machine.Process( new RunMessage( "BEGIN", _emptyParams ), nullResponseHandler() );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );
			  machine.Process( new RunMessage( "CREATE (n:Victim)-[:REL]->()", _emptyParams ), nullResponseHandler() );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );

			  // When I perform an action that will fail
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( new RunMessage( "this is not valid syntax", _emptyParams ), recorder );

			  // Then I should see a failure
			  assertThat( recorder.NextResponse(), failedWithStatus(Org.Neo4j.Kernel.Api.Exceptions.Status_Statement.SyntaxError) );

			  // And when I acknowledge that failure, and roll back the transaction
			  recorder.Reset();
			  machine.Process( AckFailureMessage.INSTANCE, recorder );
			  machine.Process( new RunMessage( "ROLLBACK", _emptyParams ), recorder );

			  // Then both operations should succeed
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleFailureDuringResultPublishing() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleFailureDuringResultPublishing()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch pullAllCallbackCalled = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent pullAllCallbackCalled = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<org.neo4j.bolt.runtime.Neo4jError> error = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<Neo4jError> error = new AtomicReference<Neo4jError>();

			  // When something fails while publishing the result stream
			  machine.Process( new RunMessage( "RETURN 1", _emptyParams ), nullResponseHandler() );
			  machine.Process( PullAllMessage.INSTANCE, new BoltResponseHandlerAnonymousInnerClass( this, pullAllCallbackCalled, error ) );

			  // Then
			  assertTrue( pullAllCallbackCalled.await( 30, TimeUnit.SECONDS ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.Neo4jError err = error.get();
			  Neo4jError err = error.get();
			  assertThat( err.Status(), equalTo(Org.Neo4j.Kernel.Api.Exceptions.Status_General.UnknownError) );
			  assertThat( err.Message(), CoreMatchers.containsString("Ooopsies!") );
		 }

		 private class BoltResponseHandlerAnonymousInnerClass : BoltResponseHandler
		 {
			 private readonly BoltConnectionIT _outerInstance;

			 private System.Threading.CountdownEvent _pullAllCallbackCalled;
			 private AtomicReference<Neo4jError> _error;

			 public BoltResponseHandlerAnonymousInnerClass( BoltConnectionIT outerInstance, System.Threading.CountdownEvent pullAllCallbackCalled, AtomicReference<Neo4jError> error )
			 {
				 this.outerInstance = outerInstance;
				 this._pullAllCallbackCalled = pullAllCallbackCalled;
				 this._error = error;
			 }

			 public void onRecords( BoltResult result, bool pull )
			 {
				  throw new Exception( "Ooopsies!" );
			 }

			 public void onMetadata( string key, AnyValue value )
			 {
			 }

			 public void markFailed( Neo4jError err )
			 {
				  _error.set( err );
				  _pullAllCallbackCalled.Signal();
			 }

			 public void markIgnored()
			 {
			 }

			 public void onFinish()
			 {
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToCleanlyRunMultipleSessionsInSingleThread() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToCleanlyRunMultipleSessionsInSingleThread()
		 {
			  // Given
			  BoltStateMachine firstMachine = Env.newMachine( _boltChannel );
			  firstMachine.Process( new InitMessage( USER_AGENT, emptyMap() ), null );
			  BoltStateMachine secondMachine = Env.newMachine( _boltChannel );
			  secondMachine.Process( new InitMessage( USER_AGENT, emptyMap() ), null );

			  // And given I've started a transaction in one session
			  RunAndPull( firstMachine, "BEGIN" );

			  // When I issue a statement in a separate session
			  object[] stream = RunAndPull( secondMachine, "CREATE (a:Person) RETURN id(a)" );
			  long id = ( ( LongValue )( ( QueryResult_Record ) stream[0] ).fields()[0] ).value();

			  // And when I roll back that first session transaction
			  RunAndPull( firstMachine, "ROLLBACK" );

			  // Then the two should not have interfered with each other
			  stream = RunAndPull( secondMachine, "MATCH (a:Person) WHERE id(a) = " + id + " RETURN COUNT(*)" );
			  assertThat( ( ( QueryResult_Record ) stream[0] ).fields()[0], equalTo(longValue(1L)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportUsingPeriodicCommitInSession() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupportUsingPeriodicCommitInSession()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );
			  MapValue @params = Map( "csvFileUrl", CreateLocalIrisData( machine ) );
			  long txIdBeforeQuery = Env.lastClosedTxId();
			  long batch = 40;

			  // When
			  object[] result = RunAndPull( machine, "USING PERIODIC COMMIT " + batch + "\n" + "LOAD CSV WITH HEADERS FROM {csvFileUrl} AS l\n" + "MATCH (c:Class {name: l.class_name})\n" + "CREATE (s:Sample {sepal_length: l.sepal_length, sepal_width: l.sepal_width, " + "petal_length: l.petal_length, petal_width: l.petal_width})\n" + "CREATE (c)<-[:HAS_CLASS]-(s)\n" + "RETURN count(*) AS c", @params );

			  // Then
			  assertThat( result.Length, equalTo( 1 ) );
			  QueryResult_Record record = ( QueryResult_Record ) result[0];

			  AnyValue[] fields = record.Fields();
			  assertThat( fields.Length, equalTo( 1 ) );
			  assertThat( fields[0], equalTo( longValue( 150L ) ) );

			  /*
			   * 7 tokens have been created for
			   * 'Sample' label
			   * 'HAS_CLASS' relationship type
			   * 'name', 'sepal_length', 'sepal_width', 'petal_length', and 'petal_width' property keys
			   *
			   * Note that the token id for the label 'Class' has been created in `createLocalIrisData(...)` so it shouldn't1
			   * be counted again here
			   */
			  long tokensCommits = 7;
			  long commits = ( _irisData.Split( "\n", true ).length - 1 ) / batch;
			  long txId = Env.lastClosedTxId();
			  assertEquals( tokensCommits + commits + txIdBeforeQuery, txId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSupportUsingPeriodicCommitInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSupportUsingPeriodicCommitInTransaction()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );
			  MapValue @params = Map( "csvFileUrl", CreateLocalIrisData( machine ) );
			  RunAndPull( machine, "BEGIN" );

			  // When
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( new RunMessage( "USING PERIODIC COMMIT 40\n" + "LOAD CSV WITH HEADERS FROM {csvFileUrl} AS l\n" + "MATCH (c:Class {name: l.class_name})\n" + "CREATE (s:Sample {sepal_length: l.sepal_length, sepal_width: l.sepal_width, petal_length: l" + ".petal_length, petal_width: l.petal_width})\n" + "CREATE (c)<-[:HAS_CLASS]-(s)\n" + "RETURN count(*) AS c", @params ), recorder );

			  // Then
			  assertThat( recorder.NextResponse(), failedWithStatus(Org.Neo4j.Kernel.Api.Exceptions.Status_Statement.SemanticError) );
			  // "Executing queries that use periodic commit in an open transaction is not possible."
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseTransactionOnCommit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseTransactionOnCommit()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  RunAndPull( machine, "BEGIN" );
			  RunAndPull( machine, "RETURN 1" );
			  RunAndPull( machine, "COMMIT" );

			  assertFalse( HasTransaction( machine ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseTransactionEvenIfCommitFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseTransactionEvenIfCommitFails()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  RunAndPull( machine, "BEGIN" );
			  RunAndPull( machine, "X", Map(), IGNORED );
			  machine.Process( AckFailureMessage.INSTANCE, nullResponseHandler() );
			  RunAndPull( machine, "COMMIT", Map(), IGNORED );
			  machine.Process( AckFailureMessage.INSTANCE, nullResponseHandler() );

			  assertFalse( HasTransaction( machine ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseTransactionOnRollback() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseTransactionOnRollback()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  RunAndPull( machine, "BEGIN" );
			  RunAndPull( machine, "RETURN 1" );
			  RunAndPull( machine, "ROLLBACK" );

			  assertFalse( HasTransaction( machine ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseTransactionOnRollbackAfterFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseTransactionOnRollbackAfterFailure()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  RunAndPull( machine, "BEGIN" );
			  RunAndPull( machine, "X", Map(), IGNORED );
			  machine.Process( AckFailureMessage.INSTANCE, nullResponseHandler() );
			  RunAndPull( machine, "ROLLBACK" );

			  assertFalse( HasTransaction( machine ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowNewTransactionAfterFailure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowNewTransactionAfterFailure()
		 {
			  // Given
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  // And given I've started a transaction that failed
			  RunAndPull( machine, "BEGIN" );
			  machine.Process( new RunMessage( "invalid", _emptyParams ), nullResponseHandler() );
			  machine.Process( ResetMessage.INSTANCE, nullResponseHandler() );

			  // When
			  RunAndPull( machine, "BEGIN" );
			  object[] stream = RunAndPull( machine, "RETURN 1" );

			  // Then
			  assertThat( ( ( QueryResult_Record ) stream[0] ).fields()[0], equalTo(longValue(1L)) );
		 }

		 private static bool HasTransaction( BoltStateMachine machine )
		 {
			  return ( ( BoltStateMachineV1 ) machine ).statementProcessor().hasTransaction();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String createLocalIrisData(org.neo4j.bolt.runtime.BoltStateMachine machine) throws Exception
		 private string CreateLocalIrisData( BoltStateMachine machine )
		 {
			  foreach ( string className in _irisClassNames )
			  {
					MapValue @params = Map( "className", className );
					RunAndPull( machine, "CREATE (c:Class {name: {className}}) RETURN c", @params );
			  }

			  return Env.putTmpFile( "iris", ".csv", _irisData ).toExternalForm();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Object[] runAndPull(org.neo4j.bolt.runtime.BoltStateMachine machine, String statement) throws Exception
		 private object[] RunAndPull( BoltStateMachine machine, string statement )
		 {
			  return RunAndPull( machine, statement, _emptyParams, SUCCESS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.cypher.result.QueryResult_Record[] runAndPull(org.neo4j.bolt.runtime.BoltStateMachine machine, String statement, org.neo4j.values.virtual.MapValue params) throws Exception
		 private QueryResult_Record[] RunAndPull( BoltStateMachine machine, string statement, MapValue @params )
		 {
			  return RunAndPull( machine, statement, @params, SUCCESS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.cypher.result.QueryResult_Record[] runAndPull(org.neo4j.bolt.runtime.BoltStateMachine machine, String statement, org.neo4j.values.virtual.MapValue params, org.neo4j.bolt.v1.messaging.BoltResponseMessage expectedResponse) throws Exception
		 private QueryResult_Record[] RunAndPull( BoltStateMachine machine, string statement, MapValue @params, BoltResponseMessage expectedResponse )
		 {
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  machine.Process( new RunMessage( statement, @params ), nullResponseHandler() );
			  machine.Process( PullAllMessage.INSTANCE, recorder );
			  RecordedBoltResponse response = recorder.NextResponse();
			  assertEquals( expectedResponse, response.Message() );
			  return response.Records();
		 }

		 private MapValue Map( params object[] keyValues )
		 {
			  return ValueUtils.asMapValue( MapUtil.map( keyValues ) );
		 }

		 private static string[] _irisClassNames = new string[] { "Iris-setosa", "Iris-versicolor", "Iris-virginica" };

		 private static string _irisData = "sepal_length,sepal_width,petal_length,petal_width,class_name\n" +
							  "5.1,3.5,1.4,0.2,Iris-setosa\n" +
							  "4.9,3.0,1.4,0.2,Iris-setosa\n" +
							  "4.7,3.2,1.3,0.2,Iris-setosa\n" +
							  "4.6,3.1,1.5,0.2,Iris-setosa\n" +
							  "5.0,3.6,1.4,0.2,Iris-setosa\n" +
							  "5.4,3.9,1.7,0.4,Iris-setosa\n" +
							  "4.6,3.4,1.4,0.3,Iris-setosa\n" +
							  "5.0,3.4,1.5,0.2,Iris-setosa\n" +
							  "4.4,2.9,1.4,0.2,Iris-setosa\n" +
							  "4.9,3.1,1.5,0.1,Iris-setosa\n" +
							  "5.4,3.7,1.5,0.2,Iris-setosa\n" +
							  "4.8,3.4,1.6,0.2,Iris-setosa\n" +
							  "4.8,3.0,1.4,0.1,Iris-setosa\n" +
							  "4.3,3.0,1.1,0.1,Iris-setosa\n" +
							  "5.8,4.0,1.2,0.2,Iris-setosa\n" +
							  "5.7,4.4,1.5,0.4,Iris-setosa\n" +
							  "5.4,3.9,1.3,0.4,Iris-setosa\n" +
							  "5.1,3.5,1.4,0.3,Iris-setosa\n" +
							  "5.7,3.8,1.7,0.3,Iris-setosa\n" +
							  "5.1,3.8,1.5,0.3,Iris-setosa\n" +
							  "5.4,3.4,1.7,0.2,Iris-setosa\n" +
							  "5.1,3.7,1.5,0.4,Iris-setosa\n" +
							  "4.6,3.6,1.0,0.2,Iris-setosa\n" +
							  "5.1,3.3,1.7,0.5,Iris-setosa\n" +
							  "4.8,3.4,1.9,0.2,Iris-setosa\n" +
							  "5.0,3.0,1.6,0.2,Iris-setosa\n" +
							  "5.0,3.4,1.6,0.4,Iris-setosa\n" +
							  "5.2,3.5,1.5,0.2,Iris-setosa\n" +
							  "5.2,3.4,1.4,0.2,Iris-setosa\n" +
							  "4.7,3.2,1.6,0.2,Iris-setosa\n" +
							  "4.8,3.1,1.6,0.2,Iris-setosa\n" +
							  "5.4,3.4,1.5,0.4,Iris-setosa\n" +
							  "5.2,4.1,1.5,0.1,Iris-setosa\n" +
							  "5.5,4.2,1.4,0.2,Iris-setosa\n" +
							  "4.9,3.1,1.5,0.2,Iris-setosa\n" +
							  "5.0,3.2,1.2,0.2,Iris-setosa\n" +
							  "5.5,3.5,1.3,0.2,Iris-setosa\n" +
							  "4.9,3.6,1.4,0.1,Iris-setosa\n" +
							  "4.4,3.0,1.3,0.2,Iris-setosa\n" +
							  "5.1,3.4,1.5,0.2,Iris-setosa\n" +
							  "5.0,3.5,1.3,0.3,Iris-setosa\n" +
							  "4.5,2.3,1.3,0.3,Iris-setosa\n" +
							  "4.4,3.2,1.3,0.2,Iris-setosa\n" +
							  "5.0,3.5,1.6,0.6,Iris-setosa\n" +
							  "5.1,3.8,1.9,0.4,Iris-setosa\n" +
							  "4.8,3.0,1.4,0.3,Iris-setosa\n" +
							  "5.1,3.8,1.6,0.2,Iris-setosa\n" +
							  "4.6,3.2,1.4,0.2,Iris-setosa\n" +
							  "5.3,3.7,1.5,0.2,Iris-setosa\n" +
							  "5.0,3.3,1.4,0.2,Iris-setosa\n" +
							  "7.0,3.2,4.7,1.4,Iris-versicolor\n" +
							  "6.4,3.2,4.5,1.5,Iris-versicolor\n" +
							  "6.9,3.1,4.9,1.5,Iris-versicolor\n" +
							  "5.5,2.3,4.0,1.3,Iris-versicolor\n" +
							  "6.5,2.8,4.6,1.5,Iris-versicolor\n" +
							  "5.7,2.8,4.5,1.3,Iris-versicolor\n" +
							  "6.3,3.3,4.7,1.6,Iris-versicolor\n" +
							  "4.9,2.4,3.3,1.0,Iris-versicolor\n" +
							  "6.6,2.9,4.6,1.3,Iris-versicolor\n" +
							  "5.2,2.7,3.9,1.4,Iris-versicolor\n" +
							  "5.0,2.0,3.5,1.0,Iris-versicolor\n" +
							  "5.9,3.0,4.2,1.5,Iris-versicolor\n" +
							  "6.0,2.2,4.0,1.0,Iris-versicolor\n" +
							  "6.1,2.9,4.7,1.4,Iris-versicolor\n" +
							  "5.6,2.9,3.6,1.3,Iris-versicolor\n" +
							  "6.7,3.1,4.4,1.4,Iris-versicolor\n" +
							  "5.6,3.0,4.5,1.5,Iris-versicolor\n" +
							  "5.8,2.7,4.1,1.0,Iris-versicolor\n" +
							  "6.2,2.2,4.5,1.5,Iris-versicolor\n" +
							  "5.6,2.5,3.9,1.1,Iris-versicolor\n" +
							  "5.9,3.2,4.8,1.8,Iris-versicolor\n" +
							  "6.1,2.8,4.0,1.3,Iris-versicolor\n" +
							  "6.3,2.5,4.9,1.5,Iris-versicolor\n" +
							  "6.1,2.8,4.7,1.2,Iris-versicolor\n" +
							  "6.4,2.9,4.3,1.3,Iris-versicolor\n" +
							  "6.6,3.0,4.4,1.4,Iris-versicolor\n" +
							  "6.8,2.8,4.8,1.4,Iris-versicolor\n" +
							  "6.7,3.0,5.0,1.7,Iris-versicolor\n" +
							  "6.0,2.9,4.5,1.5,Iris-versicolor\n" +
							  "5.7,2.6,3.5,1.0,Iris-versicolor\n" +
							  "5.5,2.4,3.8,1.1,Iris-versicolor\n" +
							  "5.5,2.4,3.7,1.0,Iris-versicolor\n" +
							  "5.8,2.7,3.9,1.2,Iris-versicolor\n" +
							  "6.0,2.7,5.1,1.6,Iris-versicolor\n" +
							  "5.4,3.0,4.5,1.5,Iris-versicolor\n" +
							  "6.0,3.4,4.5,1.6,Iris-versicolor\n" +
							  "6.7,3.1,4.7,1.5,Iris-versicolor\n" +
							  "6.3,2.3,4.4,1.3,Iris-versicolor\n" +
							  "5.6,3.0,4.1,1.3,Iris-versicolor\n" +
							  "5.5,2.5,4.0,1.3,Iris-versicolor\n" +
							  "5.5,2.6,4.4,1.2,Iris-versicolor\n" +
							  "6.1,3.0,4.6,1.4,Iris-versicolor\n" +
							  "5.8,2.6,4.0,1.2,Iris-versicolor\n" +
							  "5.0,2.3,3.3,1.0,Iris-versicolor\n" +
							  "5.6,2.7,4.2,1.3,Iris-versicolor\n" +
							  "5.7,3.0,4.2,1.2,Iris-versicolor\n" +
							  "5.7,2.9,4.2,1.3,Iris-versicolor\n" +
							  "6.2,2.9,4.3,1.3,Iris-versicolor\n" +
							  "5.1,2.5,3.0,1.1,Iris-versicolor\n" +
							  "5.7,2.8,4.1,1.3,Iris-versicolor\n" +
							  "6.3,3.3,6.0,2.5,Iris-virginica\n" +
							  "5.8,2.7,5.1,1.9,Iris-virginica\n" +
							  "7.1,3.0,5.9,2.1,Iris-virginica\n" +
							  "6.3,2.9,5.6,1.8,Iris-virginica\n" +
							  "6.5,3.0,5.8,2.2,Iris-virginica\n" +
							  "7.6,3.0,6.6,2.1,Iris-virginica\n" +
							  "4.9,2.5,4.5,1.7,Iris-virginica\n" +
							  "7.3,2.9,6.3,1.8,Iris-virginica\n" +
							  "6.7,2.5,5.8,1.8,Iris-virginica\n" +
							  "7.2,3.6,6.1,2.5,Iris-virginica\n" +
							  "6.5,3.2,5.1,2.0,Iris-virginica\n" +
							  "6.4,2.7,5.3,1.9,Iris-virginica\n" +
							  "6.8,3.0,5.5,2.1,Iris-virginica\n" +
							  "5.7,2.5,5.0,2.0,Iris-virginica\n" +
							  "5.8,2.8,5.1,2.4,Iris-virginica\n" +
							  "6.4,3.2,5.3,2.3,Iris-virginica\n" +
							  "6.5,3.0,5.5,1.8,Iris-virginica\n" +
							  "7.7,3.8,6.7,2.2,Iris-virginica\n" +
							  "7.7,2.6,6.9,2.3,Iris-virginica\n" +
							  "6.0,2.2,5.0,1.5,Iris-virginica\n" +
							  "6.9,3.2,5.7,2.3,Iris-virginica\n" +
							  "5.6,2.8,4.9,2.0,Iris-virginica\n" +
							  "7.7,2.8,6.7,2.0,Iris-virginica\n" +
							  "6.3,2.7,4.9,1.8,Iris-virginica\n" +
							  "6.7,3.3,5.7,2.1,Iris-virginica\n" +
							  "7.2,3.2,6.0,1.8,Iris-virginica\n" +
							  "6.2,2.8,4.8,1.8,Iris-virginica\n" +
							  "6.1,3.0,4.9,1.8,Iris-virginica\n" +
							  "6.4,2.8,5.6,2.1,Iris-virginica\n" +
							  "7.2,3.0,5.8,1.6,Iris-virginica\n" +
							  "7.4,2.8,6.1,1.9,Iris-virginica\n" +
							  "7.9,3.8,6.4,2.0,Iris-virginica\n" +
							  "6.4,2.8,5.6,2.2,Iris-virginica\n" +
							  "6.3,2.8,5.1,1.5,Iris-virginica\n" +
							  "6.1,2.6,5.6,1.4,Iris-virginica\n" +
							  "7.7,3.0,6.1,2.3,Iris-virginica\n" +
							  "6.3,3.4,5.6,2.4,Iris-virginica\n" +
							  "6.4,3.1,5.5,1.8,Iris-virginica\n" +
							  "6.0,3.0,4.8,1.8,Iris-virginica\n" +
							  "6.9,3.1,5.4,2.1,Iris-virginica\n" +
							  "6.7,3.1,5.6,2.4,Iris-virginica\n" +
							  "6.9,3.1,5.1,2.3,Iris-virginica\n" +
							  "5.8,2.7,5.1,1.9,Iris-virginica\n" +
							  "6.8,3.2,5.9,2.3,Iris-virginica\n" +
							  "6.7,3.3,5.7,2.5,Iris-virginica\n" +
							  "6.7,3.0,5.2,2.3,Iris-virginica\n" +
							  "6.3,2.5,5.0,1.9,Iris-virginica\n" +
							  "6.5,3.0,5.2,2.0,Iris-virginica\n" +
							  "6.2,3.4,5.4,2.3,Iris-virginica\n" +
							  "5.9,3.0,5.1,1.8,Iris-virginica\n" +
							  "\n";
	}

}