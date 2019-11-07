using System;
using System.Threading;

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
namespace Neo4Net.Bolt.v1.runtime.integration
{
	using Request = org.eclipse.jetty.server.Request;
	using Server = org.eclipse.jetty.server.Server;
	using ServerConnector = org.eclipse.jetty.server.ServerConnector;
	using AbstractHandler = org.eclipse.jetty.server.handler.AbstractHandler;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using BoltConnectionFatality = Neo4Net.Bolt.runtime.BoltConnectionFatality;
	using BoltStateMachine = Neo4Net.Bolt.runtime.BoltStateMachine;
	using BoltResponseRecorder = Neo4Net.Bolt.testing.BoltResponseRecorder;
	using BoltTestUtil = Neo4Net.Bolt.testing.BoltTestUtil;
	using AckFailureMessage = Neo4Net.Bolt.v1.messaging.request.AckFailureMessage;
	using DiscardAllMessage = Neo4Net.Bolt.v1.messaging.request.DiscardAllMessage;
	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using PullAllMessage = Neo4Net.Bolt.v1.messaging.request.PullAllMessage;
	using ResetMessage = Neo4Net.Bolt.v1.messaging.request.ResetMessage;
	using RunMessage = Neo4Net.Bolt.v1.messaging.request.RunMessage;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using Barrier = Neo4Net.Test.Barrier;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using BinaryLatch = Neo4Net.Utils.Concurrent.BinaryLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.AllOf.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.testing.BoltMatchers.containsRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.testing.BoltMatchers.failedWithStatus;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.testing.BoltMatchers.succeeded;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.testing.BoltMatchers.succeededWithMetadata;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.testing.BoltMatchers.succeededWithRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.testing.BoltMatchers.wasIgnored;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.testing.NullResponseHandler.nullResponseHandler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValues.EMPTY_MAP;


	public class TransactionIT
	{
		 private const string USER_AGENT = "TransactionIT/0.0";
		 private static readonly Pattern _bookmarkPattern = Pattern.compile( "Neo4Net:bookmark:v1:tx[0-9]+" );
		 private static readonly BoltChannel _boltChannel = BoltTestUtil.newTestBoltChannel();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public SessionRule env = new SessionRule();
		 public SessionRule Env = new SessionRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.SuppressOutput suppressOutput = Neo4Net.test.rule.SuppressOutput.suppressAll();
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
//ORIGINAL LINE: final String bookmark = "Neo4Net:bookmark:v1:tx" + dbVersionAfterWrite;
					string bookmark = "Neo4Net:bookmark:v1:tx" + dbVersionAfterWrite;
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
//ORIGINAL LINE: final Neo4Net.test.DoubleLatch latch = new Neo4Net.test.DoubleLatch(3, true);
			  DoubleLatch latch = new DoubleLatch( 3, true );

			  // This is used to block the http server between the first and second batch
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.test.Barrier_Control barrier = new Neo4Net.test.Barrier_Control();
			  Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();

			  // Serve CSV via local web server, let Jetty find a random port for us
			  Server server = CreateHttpServer( latch, barrier, 20, 30 );
			  server.start();
			  int localPort = GetLocalPort( server );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.bolt.runtime.BoltStateMachine[] machine = {null};
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
//ORIGINAL LINE: final Neo4Net.bolt.runtime.BoltStateMachine machine = env.newMachine(BOLT_CHANNEL);
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
//ORIGINAL LINE: final Neo4Net.bolt.runtime.BoltStateMachine machine = env.newMachine(BOLT_CHANNEL);
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
//ORIGINAL LINE: final Neo4Net.bolt.runtime.BoltStateMachine machine = env.newMachine(BOLT_CHANNEL);
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
//ORIGINAL LINE: final Neo4Net.bolt.runtime.BoltStateMachine machine = env.newMachine(BOLT_CHANNEL);
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
			  assertThat( recorder.NextResponse(), failedWithStatus(Neo4Net.Kernel.Api.Exceptions.Status_Statement.SyntaxError) );
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
//ORIGINAL LINE: final Neo4Net.bolt.runtime.BoltStateMachine machine = env.newMachine(BOLT_CHANNEL);
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
			  assertThat( recorder.NextResponse(), allOf(containsRecord(1L), failedWithStatus(Neo4Net.Kernel.Api.Exceptions.Status_Statement.ArithmeticError)) );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeeded() );
			  assertThat( recorder.NextResponse(), succeededWithRecord(2L) );
			  assertEquals( 0, recorder.ResponseCount() );
		 }

		 public static Server CreateHttpServer( DoubleLatch latch, Neo4Net.Test.Barrier_Control innerBarrier, int firstBatchSize, int otherBatchSize )
		 {
			  Server server = new Server( 0 );
			  server.Handler = new AbstractHandlerAnonymousInnerClass( latch, innerBarrier, firstBatchSize, otherBatchSize );
			  return server;
		 }

		 private class AbstractHandlerAnonymousInnerClass : AbstractHandler
		 {
			 private DoubleLatch _latch;
			 private Neo4Net.Test.Barrier_Control _innerBarrier;
			 private int _firstBatchSize;
			 private int _otherBatchSize;

			 public AbstractHandlerAnonymousInnerClass( DoubleLatch latch, Neo4Net.Test.Barrier_Control innerBarrier, int firstBatchSize, int otherBatchSize )
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