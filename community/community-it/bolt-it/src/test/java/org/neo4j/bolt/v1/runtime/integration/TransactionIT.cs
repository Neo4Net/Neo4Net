using System;
using System.Threading;

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
	using Request = org.eclipse.jetty.server.Request;
	using Server = org.eclipse.jetty.server.Server;
	using ServerConnector = org.eclipse.jetty.server.ServerConnector;
	using AbstractHandler = org.eclipse.jetty.server.handler.AbstractHandler;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using BoltConnectionFatality = Org.Neo4j.Bolt.runtime.BoltConnectionFatality;
	using BoltStateMachine = Org.Neo4j.Bolt.runtime.BoltStateMachine;
	using BoltResponseRecorder = Org.Neo4j.Bolt.testing.BoltResponseRecorder;
	using BoltTestUtil = Org.Neo4j.Bolt.testing.BoltTestUtil;
	using AckFailureMessage = Org.Neo4j.Bolt.v1.messaging.request.AckFailureMessage;
	using DiscardAllMessage = Org.Neo4j.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Org.Neo4j.Bolt.v1.messaging.request.InitMessage;
	using PullAllMessage = Org.Neo4j.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Org.Neo4j.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Org.Neo4j.Bolt.v1.messaging.request.RunMessage;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using ValueUtils = Org.Neo4j.Kernel.impl.util.ValueUtils;
	using Barrier = Org.Neo4j.Test.Barrier;
	using DoubleLatch = Org.Neo4j.Test.DoubleLatch;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;
	using BinaryLatch = Org.Neo4j.Util.concurrent.BinaryLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.AllOf.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.containsRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.failedWithStatus;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.succeeded;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.succeededWithMetadata;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.succeededWithRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.BoltMatchers.wasIgnored;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.testing.NullResponseHandler.nullResponseHandler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_MAP;


	public class TransactionIT
	{
		 private const string USER_AGENT = "TransactionIT/0.0";
		 private static readonly Pattern _bookmarkPattern = Pattern.compile( "neo4j:bookmark:v1:tx[0-9]+" );
		 private static readonly BoltChannel _boltChannel = BoltTestUtil.newTestBoltChannel();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public SessionRule env = new SessionRule();
		 public SessionRule Env = new SessionRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleBeginCommit() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleBeginCommit()
		 {
			  // Given
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  // When
			  machine.Process( new RunMessage( "BEGIN", EMPTY_MAP ), recorder );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );

			  machine.Process( new RunMessage( "CREATE (n:InTx)", EMPTY_MAP ), recorder );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );

			  machine.Process( new RunMessage( "COMMIT", EMPTY_MAP ), recorder );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );

			  // Then
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleBeginRollback() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleBeginRollback()
		 {
			  // Given
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  // When
			  machine.Process( new RunMessage( "BEGIN", EMPTY_MAP ), recorder );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );

			  machine.Process( new RunMessage( "CREATE (n:InTx)", EMPTY_MAP ), recorder );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );

			  machine.Process( new RunMessage( "ROLLBACK", EMPTY_MAP ), recorder );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );

			  // Then
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFailWhenOutOfOrderRollbackInAutoCommitMode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFailWhenOutOfOrderRollbackInAutoCommitMode()
		 {
			  // Given
			  BoltResponseRecorder runRecorder = new BoltResponseRecorder();
			  BoltResponseRecorder pullAllRecorder = new BoltResponseRecorder();
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  // When
			  machine.Process( new RunMessage( "ROLLBACK", EMPTY_MAP ), runRecorder );
			  machine.Process( PullAllMessage.INSTANCE, pullAllRecorder );

			  // Then
			  assertThat( runRecorder.NextResponse(), succeeded() );
			  assertThat( pullAllRecorder.NextResponse(), succeeded() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReceiveBookmarkOnCommitAndDiscardAll() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReceiveBookmarkOnCommitAndDiscardAll()
		 {
			  // Given
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  // When
			  machine.Process( new RunMessage( "BEGIN", EMPTY_MAP ), recorder );
			  machine.Process( DiscardAllMessage.INSTANCE, recorder );

			  machine.Process( new RunMessage( "CREATE (a:Person)", EMPTY_MAP ), recorder );
			  machine.Process( DiscardAllMessage.INSTANCE, recorder );

			  machine.Process( new RunMessage( "COMMIT", EMPTY_MAP ), recorder );
			  machine.Process( DiscardAllMessage.INSTANCE, recorder );

			  // Then
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeededWithMetadata("bookmark", _bookmarkPattern) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReceiveBookmarkOnCommitAndPullAll() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReceiveBookmarkOnCommitAndPullAll()
		 {
			  // Given
			  BoltResponseRecorder recorder = new BoltResponseRecorder();
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );

			  // When
			  machine.Process( new RunMessage( "BEGIN", EMPTY_MAP ), recorder );
			  machine.Process( DiscardAllMessage.INSTANCE, recorder );

			  machine.Process( new RunMessage( "CREATE (a:Person)", EMPTY_MAP ), recorder );
			  machine.Process( DiscardAllMessage.INSTANCE, recorder );

			  machine.Process( new RunMessage( "COMMIT", EMPTY_MAP ), recorder );
			  machine.Process( PullAllMessage.INSTANCE, recorder );

			  // Then
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeededWithMetadata("bookmark", _bookmarkPattern) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadYourOwnWrites() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadYourOwnWrites()
		 {
			  using ( Transaction tx = Env.graph().beginTx() )
			  {
					Node node = Env.graph().createNode(Label.label("A"));
					node.SetProperty( "prop", "one" );
					tx.Success();
			  }

			  BinaryLatch latch = new BinaryLatch();

			  long dbVersion = Env.lastClosedTxId();
			  Thread thread = new Thread(() =>
			  {
			  try
			  {
				  using ( BoltStateMachine machine = Env.newMachine( _boltChannel ) )
				  {
					  machine.process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );
					  latch.Await();
					  machine.process( new RunMessage( "MATCH (n:A) SET n.prop = 'two'", EMPTY_MAP ), nullResponseHandler() );
					  machine.process( PullAllMessage.INSTANCE, nullResponseHandler() );
				  }
			  }
			  catch ( BoltConnectionFatality connectionFatality )
			  {
				  throw new Exception( connectionFatality );
			  }
			  });
			  thread.Start();

			  long dbVersionAfterWrite = dbVersion + 1;
			  using ( BoltStateMachine machine = Env.newMachine( _boltChannel ) )
			  {
					BoltResponseRecorder recorder = new BoltResponseRecorder();
					machine.process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );
					latch.Release();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String bookmark = "neo4j:bookmark:v1:tx" + dbVersionAfterWrite;
					string bookmark = "neo4j:bookmark:v1:tx" + dbVersionAfterWrite;
					machine.process( new RunMessage( "BEGIN", ValueUtils.asMapValue( singletonMap( "bookmark", bookmark ) ) ), nullResponseHandler() );
					machine.process( PullAllMessage.INSTANCE, recorder );
					machine.process( new RunMessage( "MATCH (n:A) RETURN n.prop", EMPTY_MAP ), nullResponseHandler() );
					machine.process( PullAllMessage.INSTANCE, recorder );
					machine.process( new RunMessage( "COMMIT", EMPTY_MAP ), nullResponseHandler() );
					machine.process( PullAllMessage.INSTANCE, recorder );

					assertThat( recorder.NextResponse(), succeeded() );
					assertThat( recorder.NextResponse(), succeededWithRecord("two") );
					assertThat( recorder.NextResponse(), succeededWithMetadata("bookmark", _bookmarkPattern) );
			  }

			  thread.Join();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTerminateQueriesEvenIfUsingPeriodicCommit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTerminateQueriesEvenIfUsingPeriodicCommit()
		 {
			  // Spawns a throttled HTTP server, runs a PERIODIC COMMIT that fetches data from this server,
			  // and checks that the query able to be terminated

			  // We start with 3, because that is how many actors we have -
			  // 1. the http server
			  // 2. the running query
			  // 3. the one terminating 2
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.DoubleLatch latch = new org.neo4j.test.DoubleLatch(3, true);
			  DoubleLatch latch = new DoubleLatch( 3, true );

			  // This is used to block the http server between the first and second batch
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.Barrier_Control barrier = new org.neo4j.test.Barrier_Control();
			  Org.Neo4j.Test.Barrier_Control barrier = new Org.Neo4j.Test.Barrier_Control();

			  // Serve CSV via local web server, let Jetty find a random port for us
			  Server server = CreateHttpServer( latch, barrier, 20, 30 );
			  server.start();
			  int localPort = GetLocalPort( server );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.BoltStateMachine[] machine = {null};
			  BoltStateMachine[] machine = new BoltStateMachine[] { null };

			  Thread thread = new Thread(() =>
			  {
			  try
			  {
				  using ( BoltStateMachine stateMachine = Env.newMachine( _boltChannel ) )
				  {
					  machine[0] = stateMachine;
					  stateMachine.process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );
					  string query = format( "USING PERIODIC COMMIT 10 LOAD CSV FROM 'http://localhost:%d' AS line " + "CREATE (n:A {id: line[0], square: line[1]}) " + "WITH count(*) as number " + "CREATE (n:ShouldNotExist)", localPort );
					  try
					  {
						  latch.Start();
						  stateMachine.process( new RunMessage( query, EMPTY_MAP ), nullResponseHandler() );
						  stateMachine.process( PullAllMessage.INSTANCE, nullResponseHandler() );
					  }
					  finally
					  {
						  latch.Finish();
					  }
				  }
			  }
			  catch ( BoltConnectionFatality connectionFatality )
			  {
				  throw new Exception( connectionFatality );
			  }
			  });
			  thread.Name = "query runner";
			  thread.Start();

			  // We block this thread here, waiting for the http server to spin up and the running query to get started
			  latch.StartAndWaitForAllToStart();
			  Thread.Sleep( 1000 );

			  // This is the call that RESETs the Bolt connection and will terminate the running query
			  machine[0].Process( ResetMessage.INSTANCE, nullResponseHandler() );

			  barrier.Release();

			  // We block again here, waiting for the running query to have been terminated, and for the server to have
			  // wrapped up and finished streaming http results
			  latch.FinishAndWaitForAllToFinish();

			  // And now we check that the last node did not get created
			  using ( Transaction ignored = Env.graph().beginTx() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( "Query was not terminated in time - nodes were created!", Env.graph().findNodes(Label.label("ShouldNotExist")).hasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInterpretEmptyStatementAsReuseLastStatementInAutocommitTransaction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInterpretEmptyStatementAsReuseLastStatementInAutocommitTransaction()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.BoltStateMachine machine = env.newMachine(BOLT_CHANNEL);
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );
			  BoltResponseRecorder recorder = new BoltResponseRecorder();

			  // When
			  machine.Process( new RunMessage( "RETURN 1", EMPTY_MAP ), nullResponseHandler() );
			  machine.Process( PullAllMessage.INSTANCE, recorder );
			  machine.Process( new RunMessage( "", EMPTY_MAP ), nullResponseHandler() );
			  machine.Process( PullAllMessage.INSTANCE, recorder );

			  // Then
			  assertThat( recorder.NextResponse(), succeededWithRecord(1L) );
			  assertThat( recorder.NextResponse(), succeededWithRecord(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInterpretEmptyStatementAsReuseLastStatementInExplicitTransaction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInterpretEmptyStatementAsReuseLastStatementInExplicitTransaction()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.BoltStateMachine machine = env.newMachine(BOLT_CHANNEL);
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );
			  BoltResponseRecorder recorder = new BoltResponseRecorder();

			  // When
			  machine.Process( new RunMessage( "BEGIN", EMPTY_MAP ), nullResponseHandler() );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );
			  machine.Process( new RunMessage( "RETURN 1", EMPTY_MAP ), nullResponseHandler() );
			  machine.Process( PullAllMessage.INSTANCE, recorder );
			  machine.Process( new RunMessage( "", EMPTY_MAP ), nullResponseHandler() );
			  machine.Process( PullAllMessage.INSTANCE, recorder );
			  machine.Process( new RunMessage( "COMMIT", EMPTY_MAP ), nullResponseHandler() );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );

			  // Then
			  assertThat( recorder.NextResponse(), succeededWithRecord(1L) );
			  assertThat( recorder.NextResponse(), succeededWithRecord(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void beginShouldNotOverwriteLastStatement() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BeginShouldNotOverwriteLastStatement()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.BoltStateMachine machine = env.newMachine(BOLT_CHANNEL);
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );
			  BoltResponseRecorder recorder = new BoltResponseRecorder();

			  // When
			  machine.Process( new RunMessage( "RETURN 1", EMPTY_MAP ), nullResponseHandler() );
			  machine.Process( PullAllMessage.INSTANCE, recorder );
			  machine.Process( new RunMessage( "BEGIN", EMPTY_MAP ), nullResponseHandler() );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );
			  machine.Process( new RunMessage( "", EMPTY_MAP ), nullResponseHandler() );
			  machine.Process( PullAllMessage.INSTANCE, recorder );
			  machine.Process( new RunMessage( "COMMIT", EMPTY_MAP ), nullResponseHandler() );
			  machine.Process( DiscardAllMessage.INSTANCE, nullResponseHandler() );

			  // Then
			  assertThat( recorder.NextResponse(), succeededWithRecord(1L) );
			  assertThat( recorder.NextResponse(), succeededWithRecord(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseAutoCommitTransactionAndRespondToNextStatementWhenRunFails() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseAutoCommitTransactionAndRespondToNextStatementWhenRunFails()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.BoltStateMachine machine = env.newMachine(BOLT_CHANNEL);
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );
			  BoltResponseRecorder recorder = new BoltResponseRecorder();

			  // When
			  machine.Process( new RunMessage( "INVALID QUERY", EMPTY_MAP ), recorder );
			  machine.Process( PullAllMessage.INSTANCE, recorder );
			  machine.Process( AckFailureMessage.INSTANCE, recorder );
			  machine.Process( new RunMessage( "RETURN 2", EMPTY_MAP ), recorder );
			  machine.Process( PullAllMessage.INSTANCE, recorder );

			  // Then
			  assertThat( recorder.NextResponse(), failedWithStatus(Org.Neo4j.Kernel.Api.Exceptions.Status_Statement.SyntaxError) );
			  assertThat( recorder.NextResponse(), wasIgnored() );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeededWithRecord(2L) );
			  assertEquals( 0, recorder.ResponseCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseAutoCommitTransactionAndRespondToNextStatementWhenStreamingFails() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseAutoCommitTransactionAndRespondToNextStatementWhenStreamingFails()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.bolt.runtime.BoltStateMachine machine = env.newMachine(BOLT_CHANNEL);
			  BoltStateMachine machine = Env.newMachine( _boltChannel );
			  machine.Process( new InitMessage( USER_AGENT, emptyMap() ), nullResponseHandler() );
			  BoltResponseRecorder recorder = new BoltResponseRecorder();

			  // When
			  machine.Process( new RunMessage( "UNWIND [1, 0] AS x RETURN 1 / x", EMPTY_MAP ), recorder );
			  machine.Process( PullAllMessage.INSTANCE, recorder );
			  machine.Process( AckFailureMessage.INSTANCE, recorder );
			  machine.Process( new RunMessage( "RETURN 2", EMPTY_MAP ), recorder );
			  machine.Process( PullAllMessage.INSTANCE, recorder );

			  // Then
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), allOf(containsRecord(1L), failedWithStatus(Org.Neo4j.Kernel.Api.Exceptions.Status_Statement.ArithmeticError)) );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeededWithRecord(2L) );
			  assertEquals( 0, recorder.ResponseCount() );
		 }

		 public static Server CreateHttpServer( DoubleLatch latch, Org.Neo4j.Test.Barrier_Control innerBarrier, int firstBatchSize, int otherBatchSize )
		 {
			  Server server = new Server( 0 );
			  server.Handler = new AbstractHandlerAnonymousInnerClass( latch, innerBarrier, firstBatchSize, otherBatchSize );
			  return server;
		 }

		 private class AbstractHandlerAnonymousInnerClass : AbstractHandler
		 {
			 private DoubleLatch _latch;
			 private Org.Neo4j.Test.Barrier_Control _innerBarrier;
			 private int _firstBatchSize;
			 private int _otherBatchSize;

			 public AbstractHandlerAnonymousInnerClass( DoubleLatch latch, Org.Neo4j.Test.Barrier_Control innerBarrier, int firstBatchSize, int otherBatchSize )
			 {
				 this._latch = latch;
				 this._innerBarrier = innerBarrier;
				 this._firstBatchSize = firstBatchSize;
				 this._otherBatchSize = otherBatchSize;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handle(String target, org.eclipse.jetty.server.Request baseRequest, javax.servlet.http.HttpServletRequest request, javax.servlet.http.HttpServletResponse response) throws java.io.IOException
			 public override void handle( string target, Request baseRequest, HttpServletRequest request, HttpServletResponse response )
			 {
				  response.ContentType = "text/plain; charset=utf-8";
				  response.Status = HttpServletResponse.SC_OK;
				  PrintWriter @out = response.Writer;

				  writeBatch( @out, _firstBatchSize );
				  @out.flush();
				  _latch.start();
				  _innerBarrier.reached();

				  _latch.finish();
				  writeBatch( @out, _otherBatchSize );
				  baseRequest.Handled = true;
			 }

			 private void writeBatch( PrintWriter @out, int batchSize )
			 {
				  for ( int i = 0; i < batchSize; i++ )
				  {
						@out.write( format( "%d %d\n", i, i * i ) );
						i++;
				  }
			 }
		 }

		 private int GetLocalPort( Server server )
		 {
			  return ( ( ServerConnector )( server.Connectors[0] ) ).LocalPort;
		 }

	}

}