using System;
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
namespace Neo4Net.Bolt.runtime
{
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using Matchers = org.hamcrest.Matchers;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ArgumentMatchers = org.mockito.ArgumentMatchers;


	using AuthenticationException = Neo4Net.Bolt.security.auth.AuthenticationException;
	using Jobs = Neo4Net.Bolt.testing.Jobs;
	using PackOutput = Neo4Net.Bolt.v1.packstream.PackOutput;
	using Job = Neo4Net.Bolt.v1.runtime.Job;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using LogService = Neo4Net.Logging.@internal.LogService;
	using SimpleLogService = Neo4Net.Logging.@internal.SimpleLogService;
	using Neo4Net.Test.rule.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.isA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyCollection;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.argThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.same;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class DefaultBoltConnectionTest
	{
		private bool InstanceFieldsInitialized = false;

		public DefaultBoltConnectionTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_logService = new SimpleLogService( _logProvider );
		}

		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();
		 private LogService _logService;
		 private readonly BoltConnectionLifetimeListener _connectionListener = mock( typeof( BoltConnectionLifetimeListener ) );
		 private readonly BoltConnectionQueueMonitor _queueMonitor = mock( typeof( BoltConnectionQueueMonitor ) );
		 private readonly EmbeddedChannel _channel = new EmbeddedChannel();
		 private readonly PackOutput _output = mock( typeof( PackOutput ) );

		 private BoltChannel _boltChannel;
		 private BoltStateMachine _stateMachine;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.concurrent.OtherThreadRule<bool> otherThread = new org.neo4j.test.rule.concurrent.OtherThreadRule<>();
		 public OtherThreadRule<bool> OtherThread = new OtherThreadRule<bool>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _boltChannel = new BoltChannel( "bolt-1", "bolt", _channel );
			  _stateMachine = mock( typeof( BoltStateMachine ) );
			  when( _stateMachine.shouldStickOnThread() ).thenReturn(false);
			  when( _stateMachine.hasOpenStatement() ).thenReturn(false);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup()
		 public virtual void Cleanup()
		 {
			  _channel.finishAndReleaseAll();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void idShouldReturnBoltChannelId()
		 public virtual void IdShouldReturnBoltChannelId()
		 {
			  BoltConnection connection = NewConnection();

			  assertEquals( _boltChannel.id(), connection.Id() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void localAddressShouldReturnBoltServerAddress()
		 public virtual void LocalAddressShouldReturnBoltServerAddress()
		 {
			  BoltConnection connection = NewConnection();

			  assertEquals( _boltChannel.serverAddress(), connection.LocalAddress() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void remoteAddressShouldReturnBoltClientAddress()
		 public virtual void RemoteAddressShouldReturnBoltClientAddress()
		 {
			  BoltConnection connection = NewConnection();

			  assertEquals( _boltChannel.clientAddress(), connection.RemoteAddress() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void channelShouldReturnBoltRawChannel()
		 public virtual void ChannelShouldReturnBoltRawChannel()
		 {
			  BoltConnection connection = NewConnection();

			  assertEquals( _boltChannel.rawChannel(), connection.Channel() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void hasPendingJobsShouldReportFalseWhenInitialised()
		 public virtual void HasPendingJobsShouldReportFalseWhenInitialised()
		 {
			  BoltConnection connection = NewConnection();

			  assertFalse( connection.HasPendingJobs() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void startShouldNotifyListener()
		 public virtual void StartShouldNotifyListener()
		 {
			  BoltConnection connection = NewConnection();

			  connection.Start();

			  verify( _connectionListener ).created( connection );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stopShouldNotifyListenerOnTheNextBatch()
		 public virtual void StopShouldNotifyListenerOnTheNextBatch()
		 {
			  BoltConnection connection = NewConnection();
			  connection.Start();

			  connection.Stop();
			  connection.ProcessNextBatch();

			  verify( _connectionListener ).closed( connection );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enqueuedShouldNotifyQueueMonitor()
		 public virtual void EnqueuedShouldNotifyQueueMonitor()
		 {
			  Job job = Jobs.noop();
			  BoltConnection connection = NewConnection();

			  connection.Enqueue( job );

			  verify( _queueMonitor ).enqueued( connection, job );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enqueuedShouldQueueJob()
		 public virtual void EnqueuedShouldQueueJob()
		 {
			  Job job = Jobs.noop();
			  BoltConnection connection = NewConnection();

			  connection.Enqueue( job );

			  assertTrue( connection.HasPendingJobs() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void processNextBatchShouldDoNothingIfQueueIsEmptyAndConnectionNotClosed()
		 public virtual void ProcessNextBatchShouldDoNothingIfQueueIsEmptyAndConnectionNotClosed()
		 {
			  BoltConnection connection = NewConnection();

			  connection.ProcessNextBatch();

			  verify( _queueMonitor, never() ).drained(same(connection), anyCollection());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void processNextBatchShouldNotifyQueueMonitorAboutDrain()
		 public virtual void ProcessNextBatchShouldNotifyQueueMonitorAboutDrain()
		 {
			  IList<Job> drainedJobs = new List<Job>();
			  Job job = Jobs.noop();
			  BoltConnection connection = NewConnection();
			  doAnswer( inv => ( ( IList<Job> )drainedJobs ).AddRange( inv.getArgument( 1 ) ) ).when( _queueMonitor ).drained( same( connection ), anyCollection() );

			  connection.Enqueue( job );
			  connection.ProcessNextBatch();

			  verify( _queueMonitor ).drained( same( connection ), anyCollection() );
			  assertTrue( drainedJobs.Contains( job ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void processNextBatchShouldDrainMaxBatchSizeItemsOnEachCall()
		 public virtual void ProcessNextBatchShouldDrainMaxBatchSizeItemsOnEachCall()
		 {
			  IList<Job> drainedJobs = new List<Job>();
			  IList<Job> pushedJobs = new List<Job>();
			  BoltConnection connection = NewConnection( 10 );
			  doAnswer( inv => ( ( IList<Job> )drainedJobs ).AddRange( inv.getArgument( 1 ) ) ).when( _queueMonitor ).drained( same( connection ), anyCollection() );

			  for ( int i = 0; i < 15; i++ )
			  {
					Job newJob = Jobs.noop();
					pushedJobs.Add( newJob );
					connection.Enqueue( newJob );
			  }

			  connection.ProcessNextBatch();

			  verify( _queueMonitor ).drained( same( connection ), anyCollection() );
			  assertEquals( 10, drainedJobs.Count );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
			  assertTrue( drainedJobs.containsAll( pushedJobs.subList( 0, 10 ) ) );

			  drainedJobs.Clear();
			  connection.ProcessNextBatch();

			  verify( _queueMonitor, times( 2 ) ).drained( same( connection ), anyCollection() );
			  assertEquals( 5, drainedJobs.Count );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
			  assertTrue( drainedJobs.containsAll( pushedJobs.subList( 10, 15 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void interruptShouldInterruptStateMachine()
		 public virtual void InterruptShouldInterruptStateMachine()
		 {
			  BoltConnection connection = NewConnection();

			  connection.Interrupt();

			  verify( _stateMachine ).interrupt();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stopShouldFirstMarkStateMachineForTermination()
		 public virtual void StopShouldFirstMarkStateMachineForTermination()
		 {
			  BoltConnection connection = NewConnection();

			  connection.Stop();

			  verify( _stateMachine ).markForTermination();
			  verify( _queueMonitor ).enqueued( ArgumentMatchers.eq( connection ), any( typeof( Job ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stopShouldCloseStateMachineOnProcessNextBatch()
		 public virtual void StopShouldCloseStateMachineOnProcessNextBatch()
		 {
			  BoltConnection connection = NewConnection();

			  connection.Stop();

			  connection.ProcessNextBatch();

			  verify( _queueMonitor ).enqueued( ArgumentMatchers.eq( connection ), any( typeof( Job ) ) );
			  verify( _stateMachine ).markForTermination();
			  verify( _stateMachine ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stopShouldCloseStateMachineIfEnqueueEndsWithRejectedExecutionException()
		 public virtual void StopShouldCloseStateMachineIfEnqueueEndsWithRejectedExecutionException()
		 {
			  BoltConnection connection = NewConnection();

			  doAnswer(i =>
			  {
				connection.HandleSchedulingError( new RejectedExecutionException() );
				return null;
			  }).when( _queueMonitor ).enqueued( ArgumentMatchers.eq( connection ), any( typeof( Job ) ) );

			  connection.Stop();

			  verify( _stateMachine ).markForTermination();
			  verify( _stateMachine ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogBoltConnectionAuthFatalityError()
		 public virtual void ShouldLogBoltConnectionAuthFatalityError()
		 {
			  BoltConnection connection = NewConnection();
			  connection.Enqueue(machine =>
			  {
				throw new BoltConnectionAuthFatality( new AuthenticationException( Status.Security.Unauthorized, "inner error" ) );
			  });
			  connection.ProcessNextBatch();
			  verify( _stateMachine ).close();
			  _logProvider.assertExactly( AssertableLogProvider.inLog( containsString( typeof( BoltServer ).Assembly.GetName().Name ) ).warn(containsString("inner error")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void processNextBatchShouldCloseConnectionOnFatalAuthenticationError()
		 public virtual void ProcessNextBatchShouldCloseConnectionOnFatalAuthenticationError()
		 {
			  BoltConnection connection = NewConnection();

			  connection.Enqueue(machine =>
			  {
				throw new BoltConnectionAuthFatality( "auth failure", new Exception( "inner error" ) );
			  });

			  connection.ProcessNextBatch();

			  verify( _stateMachine ).close();
			  _logProvider.assertNone( AssertableLogProvider.inLog( containsString( typeof( BoltServer ).Assembly.GetName().Name ) ).warn(Matchers.any(typeof(string))) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void processNextBatchShouldCloseConnectionAndLogOnFatalBoltError()
		 public virtual void ProcessNextBatchShouldCloseConnectionAndLogOnFatalBoltError()
		 {
			  BoltConnectionFatality exception = new BoltProtocolBreachFatality( "fatal bolt error" );
			  BoltConnection connection = NewConnection();

			  connection.Enqueue(machine =>
			  {
				throw exception;
			  });

			  connection.ProcessNextBatch();

			  verify( _stateMachine ).close();
			  _logProvider.assertExactly( AssertableLogProvider.inLog( containsString( typeof( BoltServer ).Assembly.GetName().Name ) ).error(containsString("Protocol breach detected in bolt session"), @is(exception)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void processNextBatchShouldCloseConnectionAndLogOnUnexpectedException()
		 public virtual void ProcessNextBatchShouldCloseConnectionAndLogOnUnexpectedException()
		 {
			  Exception exception = new Exception( "unexpected exception" );
			  BoltConnection connection = NewConnection();

			  connection.Enqueue(machine =>
			  {
				throw exception;
			  });

			  connection.ProcessNextBatch();

			  verify( _stateMachine ).close();
			  _logProvider.assertExactly( AssertableLogProvider.inLog( containsString( typeof( BoltServer ).Assembly.GetName().Name ) ).error(containsString("Unexpected error detected in bolt session"), @is(exception)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void processNextBatchShouldThrowAssertionErrorIfStatementOpen() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ProcessNextBatchShouldThrowAssertionErrorIfStatementOpen()
		 {
			  BoltConnection connection = NewConnection( 1 );
			  connection.Enqueue( Jobs.noop() );
			  connection.Enqueue( Jobs.noop() );

			  // force to a message waiting loop
			  when( _stateMachine.hasOpenStatement() ).thenReturn(true);

			  connection.ProcessNextBatch();

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  _logProvider.assertExactly( AssertableLogProvider.inLog( typeof( DefaultBoltConnection ).FullName ).error( startsWith( "Unexpected error" ), isA( typeof( AssertionError ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void processNextBatchShouldNotThrowAssertionErrorIfStatementOpenButStopping() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ProcessNextBatchShouldNotThrowAssertionErrorIfStatementOpenButStopping()
		 {
			  BoltConnection connection = NewConnection( 1 );
			  connection.Enqueue( Jobs.noop() );
			  connection.Enqueue( Jobs.noop() );

			  // force to a message waiting loop
			  when( _stateMachine.hasOpenStatement() ).thenReturn(true);

			  connection.Stop();
			  connection.ProcessNextBatch();

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  _logProvider.assertNone( AssertableLogProvider.inLog( typeof( DefaultBoltConnection ).FullName ).error( startsWith( "Unexpected error" ), isA( typeof( AssertionError ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void processNextBatchShouldReturnWhenConnectionIsStopped() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ProcessNextBatchShouldReturnWhenConnectionIsStopped()
		 {
			  BoltConnection connection = NewConnection( 1 );
			  connection.Enqueue( Jobs.noop() );
			  connection.Enqueue( Jobs.noop() );

			  // force to a message waiting loop
			  when( _stateMachine.shouldStickOnThread() ).thenReturn(true);

			  Future<bool> future = OtherThread.execute( state => connection.ProcessNextBatch() );

			  connection.Stop();

			  OtherThread.get().awaitFuture(future);

			  verify( _stateMachine ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFlushErrorAndCloseConnectionIfFailedToSchedule() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFlushErrorAndCloseConnectionIfFailedToSchedule()
		 {
			  // Given
			  BoltConnection connection = NewConnection();

			  // When
			  RejectedExecutionException error = new RejectedExecutionException( "Failed to schedule" );
			  connection.HandleSchedulingError( error );

			  // Then
			  verify( _stateMachine ).markFailed( argThat( e => e.status().Equals(Neo4Net.Kernel.Api.Exceptions.Status_Request.NoThreadsAvailable) ) );
			  verify( _stateMachine ).close();
			  verify( _output ).flush();
		 }

		 private DefaultBoltConnection NewConnection()
		 {
			  return NewConnection( 10 );
		 }

		 private DefaultBoltConnection NewConnection( int maxBatchSize )
		 {
			  return new DefaultBoltConnection( _boltChannel, _output, _stateMachine, _logService, _connectionListener, _queueMonitor, maxBatchSize );
		 }

	}

}