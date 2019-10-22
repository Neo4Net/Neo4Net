using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.core
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using ArgumentMatchers = org.mockito.ArgumentMatchers;
	using InOrder = org.mockito.InOrder;
	using Mockito = org.mockito.Mockito;


	using ContinuousJob = Neo4Net.causalclustering.core.consensus.ContinuousJob;
	using Neo4Net.causalclustering.core.consensus;
	using ReplicatedString = Neo4Net.causalclustering.core.consensus.ReplicatedString;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using ReplicatedContent = Neo4Net.causalclustering.core.replication.ReplicatedContent;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.causalclustering.messaging;
	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.ArrayUtil.lastOf;

	public class BatchingMessageHandlerTest
	{
		private bool InstanceFieldsInitialized = false;

		public BatchingMessageHandlerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_jobSchedulerFactory = ignored => _mockJob;
		}

		 private static readonly BoundedPriorityQueue.Config _inQueueConfig = new BoundedPriorityQueue.Config( 64, 1024 );
		 private static readonly BatchingMessageHandler.Config _batchConfig = new BatchingMessageHandler.Config( 16, 256 );
		 private readonly Instant _now = Instant.now();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private org.Neo4Net.causalclustering.messaging.LifecycleMessageHandler<org.Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<?>> downstreamHandler = mock(org.Neo4Net.causalclustering.messaging.LifecycleMessageHandler.class);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private LifecycleMessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<object>> _downstreamHandler = mock( typeof( LifecycleMessageHandler ) );
		 private ClusterId _localClusterId = new ClusterId( System.Guid.randomUUID() );
		 private ContinuousJob _mockJob = mock( typeof( ContinuousJob ) );
		 private System.Func<ThreadStart, ContinuousJob> _jobSchedulerFactory;

		 private ExecutorService _executor;
		 private MemberId _leader = new MemberId( System.Guid.randomUUID() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _executor = Executors.newCachedThreadPool();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void After()
		 {
			  _executor.shutdown();
			  _executor.awaitTermination( 60, TimeUnit.SECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeInnerHandlerWhenRun()
		 public virtual void ShouldInvokeInnerHandlerWhenRun()
		 {
			  // given
			  BatchingMessageHandler batchHandler = new BatchingMessageHandler( _downstreamHandler, _inQueueConfig, _batchConfig, _jobSchedulerFactory, NullLogProvider.Instance );

			  NewEntry.Request message = new NewEntry.Request( null, Content( "dummy" ) );

			  batchHandler.Handle( Wrap( message ) );
			  verifyZeroInteractions( _downstreamHandler );

			  // when
			  batchHandler.Run();

			  // then
			  NewEntry.BatchRequest expected = new NewEntry.BatchRequest( singletonList( new ReplicatedString( "dummy" ) ) );
			  verify( _downstreamHandler ).handle( Wrap( expected ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeHandlerOnQueuedMessage() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeHandlerOnQueuedMessage()
		 {
			  // given
			  BatchingMessageHandler batchHandler = new BatchingMessageHandler( _downstreamHandler, _inQueueConfig, _batchConfig, _jobSchedulerFactory, NullLogProvider.Instance );
			  ReplicatedString content = new ReplicatedString( "dummy" );
			  NewEntry.Request message = new NewEntry.Request( null, content );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> future = executor.submit(batchHandler);
			  Future<object> future = _executor.submit( batchHandler );

			  // Some time for letting the batch handler block on its internal queue.
			  //
			  // It is fine if it sometimes doesn't get that far in time, just that we
			  // usually want to test the wake up from blocking state.
			  Thread.Sleep( 50 );

			  // when
			  batchHandler.Handle( Wrap( message ) );

			  // then
			  future.get();
			  NewEntry.BatchRequest expected = new NewEntry.BatchRequest( singletonList( content ) );
			  verify( _downstreamHandler ).handle( Wrap( expected ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBatchRequests()
		 public virtual void ShouldBatchRequests()
		 {
			  // given
			  BatchingMessageHandler batchHandler = new BatchingMessageHandler( _downstreamHandler, _inQueueConfig, _batchConfig, _jobSchedulerFactory, NullLogProvider.Instance );
			  ReplicatedString contentA = new ReplicatedString( "A" );
			  ReplicatedString contentB = new ReplicatedString( "B" );
			  NewEntry.Request messageA = new NewEntry.Request( null, contentA );
			  NewEntry.Request messageB = new NewEntry.Request( null, contentB );

			  batchHandler.Handle( Wrap( messageA ) );
			  batchHandler.Handle( Wrap( messageB ) );
			  verifyZeroInteractions( _downstreamHandler );

			  // when
			  batchHandler.Run();

			  // then
			  NewEntry.BatchRequest expected = new NewEntry.BatchRequest( asList( contentA, contentB ) );
			  verify( _downstreamHandler ).handle( Wrap( expected ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBatchUsingReceivedInstantOfFirstReceivedMessage()
		 public virtual void ShouldBatchUsingReceivedInstantOfFirstReceivedMessage()
		 {
			  // given
			  BatchingMessageHandler batchHandler = new BatchingMessageHandler( _downstreamHandler, _inQueueConfig, _batchConfig, _jobSchedulerFactory, NullLogProvider.Instance );
			  ReplicatedString content = new ReplicatedString( "A" );
			  NewEntry.Request messageA = new NewEntry.Request( null, content );

			  Instant firstReceived = Instant.ofEpochMilli( 1L );
			  Instant secondReceived = firstReceived.plusMillis( 1L );

			  batchHandler.Handle( Wrap( firstReceived, messageA ) );
			  batchHandler.Handle( Wrap( secondReceived, messageA ) );

			  // when
			  batchHandler.Run();

			  // then
			  NewEntry.BatchRequest batchRequest = new NewEntry.BatchRequest( asList( content, content ) );
			  verify( _downstreamHandler ).handle( Wrap( firstReceived, batchRequest ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBatchNewEntriesAndHandleOtherMessagesFirst()
		 public virtual void ShouldBatchNewEntriesAndHandleOtherMessagesFirst()
		 {
			  // given
			  BatchingMessageHandler batchHandler = new BatchingMessageHandler( _downstreamHandler, _inQueueConfig, _batchConfig, _jobSchedulerFactory, NullLogProvider.Instance );

			  ReplicatedString contentA = new ReplicatedString( "A" );
			  ReplicatedString contentC = new ReplicatedString( "C" );

			  NewEntry.Request newEntryA = new NewEntry.Request( null, contentA );
			  Heartbeat heartbeatA = new Heartbeat( null, 0, 0, 0 );
			  NewEntry.Request newEntryB = new NewEntry.Request( null, contentC );
			  Heartbeat heartbeatB = new Heartbeat( null, 1, 1, 1 );

			  batchHandler.Handle( Wrap( newEntryA ) );
			  batchHandler.Handle( Wrap( heartbeatA ) );
			  batchHandler.Handle( Wrap( newEntryB ) );
			  batchHandler.Handle( Wrap( heartbeatB ) );
			  verifyZeroInteractions( _downstreamHandler );

			  // when
			  batchHandler.Run(); // heartbeatA
			  batchHandler.Run(); // heartbeatB
			  batchHandler.Run(); // batchRequest

			  // then
			  NewEntry.BatchRequest batchRequest = new NewEntry.BatchRequest( asList( contentA, contentC ) );

			  verify( _downstreamHandler ).handle( Wrap( heartbeatA ) );
			  verify( _downstreamHandler ).handle( Wrap( heartbeatB ) );
			  verify( _downstreamHandler ).handle( Wrap( batchRequest ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBatchSingleEntryAppendEntries()
		 public virtual void ShouldBatchSingleEntryAppendEntries()
		 {
			  BatchingMessageHandler batchHandler = new BatchingMessageHandler( _downstreamHandler, _inQueueConfig, _batchConfig, _jobSchedulerFactory, NullLogProvider.Instance );

			  long leaderTerm = 1;
			  long prevLogIndex = -1;
			  long prevLogTerm = -1;
			  long leaderCommit = 0;

			  RaftLogEntry entryA = new RaftLogEntry( 0, Content( "A" ) );
			  RaftLogEntry entryB = new RaftLogEntry( 0, Content( "B" ) );

			  AppendEntries.Request appendA = new AppendEntries.Request( _leader, leaderTerm, prevLogIndex, prevLogTerm, new RaftLogEntry[]{ entryA }, leaderCommit );

			  AppendEntries.Request appendB = new AppendEntries.Request( _leader, leaderTerm, prevLogIndex + 1, 0, new RaftLogEntry[]{ entryB }, leaderCommit );

			  batchHandler.Handle( Wrap( appendA ) );
			  batchHandler.Handle( Wrap( appendB ) );
			  verifyZeroInteractions( _downstreamHandler );

			  // when
			  batchHandler.Run();

			  // then
			  AppendEntries.Request expected = new AppendEntries.Request( _leader, leaderTerm, prevLogIndex, prevLogTerm, new RaftLogEntry[]{ entryA, entryB }, leaderCommit );

			  verify( _downstreamHandler ).handle( Wrap( expected ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBatchMultipleEntryAppendEntries()
		 public virtual void ShouldBatchMultipleEntryAppendEntries()
		 {
			  BatchingMessageHandler batchHandler = new BatchingMessageHandler( _downstreamHandler, _inQueueConfig, _batchConfig, _jobSchedulerFactory, NullLogProvider.Instance );

			  long leaderTerm = 1;
			  long prevLogIndex = -1;
			  long prevLogTerm = -1;
			  long leaderCommit = 0;

			  RaftLogEntry[] entriesA = Entries( 0, 0, 2 );
			  RaftLogEntry[] entriesB = Entries( 1, 3, 3 );
			  RaftLogEntry[] entriesC = Entries( 2, 4, 8 );
			  RaftLogEntry[] entriesD = Entries( 3, 9, 15 );

			  AppendEntries.Request appendA = new AppendEntries.Request( _leader, leaderTerm, prevLogIndex, prevLogTerm, entriesA, leaderCommit );

			  prevLogIndex += appendA.entries().length;
			  prevLogTerm = lastOf( appendA.entries() ).term();
			  leaderCommit += 2; // arbitrary

			  AppendEntries.Request appendB = new AppendEntries.Request( _leader, leaderTerm, prevLogIndex, prevLogTerm, entriesB, leaderCommit );

			  prevLogIndex += appendB.entries().length;
			  prevLogTerm = lastOf( appendB.entries() ).term();
			  leaderCommit += 5; // arbitrary

			  AppendEntries.Request appendC = new AppendEntries.Request( _leader, leaderTerm, prevLogIndex, prevLogTerm, ArrayUtil.concat( entriesC, entriesD ), leaderCommit );

			  batchHandler.Handle( Wrap( appendA ) );
			  batchHandler.Handle( Wrap( appendB ) );
			  batchHandler.Handle( Wrap( appendC ) );
			  verifyZeroInteractions( _downstreamHandler );

			  // when
			  batchHandler.Run();

			  // then
			  AppendEntries.Request expected = new AppendEntries.Request( _leader, leaderTerm, -1, -1, ArrayUtil.concatArrays( entriesA, entriesB, entriesC, entriesD ), leaderCommit );

			  verify( _downstreamHandler ).handle( Wrap( expected ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBatchAppendEntriesDifferentLeaderTerms()
		 public virtual void ShouldNotBatchAppendEntriesDifferentLeaderTerms()
		 {
			  BatchingMessageHandler batchHandler = new BatchingMessageHandler( _downstreamHandler, _inQueueConfig, _batchConfig, _jobSchedulerFactory, NullLogProvider.Instance );

			  long leaderTerm = 1;
			  long prevLogIndex = -1;
			  long prevLogTerm = -1;
			  long leaderCommit = 0;

			  RaftLogEntry[] entriesA = Entries( 0, 0, 2 );
			  RaftLogEntry[] entriesB = Entries( 1, 3, 3 );

			  AppendEntries.Request appendA = new AppendEntries.Request( _leader, leaderTerm, prevLogIndex, prevLogTerm, entriesA, leaderCommit );

			  prevLogIndex += appendA.entries().length;
			  prevLogTerm = lastOf( appendA.entries() ).term();

			  AppendEntries.Request appendB = new AppendEntries.Request( _leader, leaderTerm + 1, prevLogIndex, prevLogTerm, entriesB, leaderCommit );

			  batchHandler.Handle( Wrap( appendA ) );
			  batchHandler.Handle( Wrap( appendB ) );
			  verifyZeroInteractions( _downstreamHandler );

			  // when
			  batchHandler.Run();
			  batchHandler.Run();

			  // then
			  verify( _downstreamHandler ).handle( Wrap( appendA ) );
			  verify( _downstreamHandler ).handle( Wrap( appendB ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrioritiseCorrectly()
		 public virtual void ShouldPrioritiseCorrectly()
		 {
			  BatchingMessageHandler batchHandler = new BatchingMessageHandler( _downstreamHandler, _inQueueConfig, _batchConfig, _jobSchedulerFactory, NullLogProvider.Instance );

			  NewEntry.Request newEntry = new NewEntry.Request( null, Content( "" ) );
			  AppendEntries.Request append = new AppendEntries.Request( _leader, 1, -1, -1, Entries( 0, 0, 0 ), 0 );
			  AppendEntries.Request emptyAppend = new AppendEntries.Request( _leader, 1, -1, -1, RaftLogEntry.empty, 0 );
			  Heartbeat heartbeat = new Heartbeat( null, 0, 0, 0 );

			  batchHandler.Handle( Wrap( newEntry ) );
			  batchHandler.Handle( Wrap( append ) );
			  batchHandler.Handle( Wrap( heartbeat ) );
			  batchHandler.Handle( Wrap( emptyAppend ) );
			  verifyZeroInteractions( _downstreamHandler );

			  // when
			  batchHandler.Run();
			  batchHandler.Run();
			  batchHandler.Run();
			  batchHandler.Run();

			  // then
			  InOrder inOrder = Mockito.inOrder( _downstreamHandler );
			  inOrder.verify( _downstreamHandler ).handle( Wrap( heartbeat ) );
			  inOrder.verify( _downstreamHandler ).handle( Wrap( emptyAppend ) );
			  inOrder.verify( _downstreamHandler ).handle( Wrap( append ) );
			  inOrder.verify( _downstreamHandler ).handle( Wrap( new NewEntry.BatchRequest( singletonList( Content( "" ) ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropMessagesAfterBeingStopped() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropMessagesAfterBeingStopped()
		 {
			  // given
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  BatchingMessageHandler batchHandler = new BatchingMessageHandler( _downstreamHandler, _inQueueConfig, _batchConfig, _jobSchedulerFactory, logProvider );

			  NewEntry.Request message = new NewEntry.Request( null, null );
			  batchHandler.Stop();

			  // when
			  batchHandler.Handle( Wrap( message ) );
			  batchHandler.Run();

			  // then
			  verify( _downstreamHandler, never() ).handle(ArgumentMatchers.any(typeof(RaftMessages_ReceivedInstantClusterIdAwareMessage)));
			  logProvider.AssertAtLeastOnce( AssertableLogProvider.inLog( typeof( BatchingMessageHandler ) ).debug( "This handler has been stopped, dropping the message: %s", Wrap( message ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 5_000) public void shouldGiveUpAddingMessagesInTheQueueIfTheHandlerHasBeenStopped() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveUpAddingMessagesInTheQueueIfTheHandlerHasBeenStopped()
		 {
			  // given
			  BatchingMessageHandler batchHandler = new BatchingMessageHandler( _downstreamHandler, new BoundedPriorityQueue.Config( 1, 1, 1024 ), _batchConfig, _jobSchedulerFactory, NullLogProvider.Instance );
			  NewEntry.Request message = new NewEntry.Request( null, new ReplicatedString( "dummy" ) );
			  batchHandler.Handle( Wrap( message ) ); // fill the queue

			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );

			  // when
			  Thread thread = new Thread(() =>
			  {
			  latch.Signal();
			  batchHandler.Handle( Wrap( message ) );
			  });

			  thread.Start();

			  latch.await();

			  batchHandler.Stop();

			  thread.Join();

			  // then we are not stuck and we terminate
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDelegateStart() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDelegateStart()
		 {
			  // given
			  BatchingMessageHandler batchHandler = new BatchingMessageHandler( _downstreamHandler, _inQueueConfig, _batchConfig, _jobSchedulerFactory, NullLogProvider.Instance );
			  ClusterId clusterId = new ClusterId( System.Guid.randomUUID() );

			  // when
			  batchHandler.Start( clusterId );

			  // then
			  Mockito.verify( _downstreamHandler ).start( clusterId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDelegateStop() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDelegateStop()
		 {
			  // given
			  BatchingMessageHandler batchHandler = new BatchingMessageHandler( _downstreamHandler, _inQueueConfig, _batchConfig, _jobSchedulerFactory, NullLogProvider.Instance );

			  // when
			  batchHandler.Stop();

			  // then
			  Mockito.verify( _downstreamHandler ).stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartJob() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStartJob()
		 {
			  // given
			  BatchingMessageHandler batchHandler = new BatchingMessageHandler( _downstreamHandler, _inQueueConfig, _batchConfig, _jobSchedulerFactory, NullLogProvider.Instance );
			  ClusterId clusterId = new ClusterId( System.Guid.randomUUID() );

			  // when
			  batchHandler.Start( clusterId );

			  // then
			  Mockito.verify( _mockJob ).start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStopJob() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStopJob()
		 {
			  // given
			  BatchingMessageHandler batchHandler = new BatchingMessageHandler( _downstreamHandler, _inQueueConfig, _batchConfig, _jobSchedulerFactory, NullLogProvider.Instance );

			  // when
			  batchHandler.Stop();

			  // then
			  Mockito.verify( _mockJob ).stop();
		 }

		 private RaftMessages_ReceivedInstantClusterIdAwareMessage Wrap( RaftMessage message )
		 {
			  return Wrap( _now, message );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<?> wrap(java.time.Instant instant, RaftMessage message)
		 private RaftMessages_ReceivedInstantClusterIdAwareMessage<object> Wrap( Instant instant, RaftMessage message )
		 {
			  return RaftMessages_ReceivedInstantClusterIdAwareMessage.of( instant, _localClusterId, message );
		 }

		 private ReplicatedContent Content( string content )
		 {
			  return new ReplicatedString( content );
		 }

		 private RaftLogEntry[] Entries( long term, int min, int max )
		 {
			  RaftLogEntry[] entries = new RaftLogEntry[max - min + 1];
			  for ( int i = min; i <= max; i++ )
			  {
					entries[i - min] = new RaftLogEntry( term, new ReplicatedString( i.ToString() ) );
			  }
			  return entries;
		 }
	}

}