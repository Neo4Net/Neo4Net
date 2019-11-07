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
namespace Neo4Net.Bolt.runtime
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Jobs = Neo4Net.Bolt.testing.Jobs;
	using Predicates = Neo4Net.Functions.Predicates;
	using Config = Neo4Net.Kernel.configuration.Config;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using NullLog = Neo4Net.Logging.NullLog;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using SimpleLogService = Neo4Net.Logging.Internal.SimpleLogService;
	using Group = Neo4Net.Scheduler.Group;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.matchers.CommonMatchers.matchesExceptionMessage;

	public class ExecutorBoltSchedulerTest
	{
		private bool InstanceFieldsInitialized = false;

		public ExecutorBoltSchedulerTest()
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
			_boltScheduler = new ExecutorBoltScheduler( CONNECTOR_KEY, _executorFactory, _jobScheduler, _logService, 0, 10, Duration.ofMinutes( 1 ), 0, ForkJoinPool.commonPool() );
		}

		 private const string CONNECTOR_KEY = "connector-id";

		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();
		 private LogService _logService;
		 private readonly Config _config = Config.defaults();
		 private readonly ExecutorFactory _executorFactory = new CachedThreadPoolExecutorFactory( NullLog.Instance );
		 private readonly IJobScheduler _jobScheduler = mock( typeof( IJobScheduler ) );
		 private ExecutorBoltScheduler _boltScheduler;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  when( _jobScheduler.threadFactory( any() ) ).thenReturn(Executors.defaultThreadFactory());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Cleanup()
		 {
			  _boltScheduler.stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void initShouldCreateThreadPool() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void InitShouldCreateThreadPool()
		 {
			  ExecutorFactory mockExecutorFactory = mock( typeof( ExecutorFactory ) );
			  when( mockExecutorFactory.Create( anyInt(), anyInt(), any(), anyInt(), anyBoolean(), any() ) ).thenReturn(Executors.newCachedThreadPool());
			  ExecutorBoltScheduler scheduler = new ExecutorBoltScheduler( CONNECTOR_KEY, mockExecutorFactory, _jobScheduler, _logService, 0, 10, Duration.ofMinutes( 1 ), 0, ForkJoinPool.commonPool() );

			  scheduler.Start();

			  verify( _jobScheduler ).threadFactory( Group.BOLT_WORKER );
			  verify( mockExecutorFactory, times( 1 ) ).create( anyInt(), anyInt(), any(typeof(Duration)), anyInt(), anyBoolean(), any(typeof(ThreadFactory)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shutdownShouldTerminateThreadPool() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShutdownShouldTerminateThreadPool()
		 {
			  ExecutorService cachedThreadPool = Executors.newCachedThreadPool();
			  ExecutorFactory mockExecutorFactory = mock( typeof( ExecutorFactory ) );
			  when( mockExecutorFactory.Create( anyInt(), anyInt(), any(), anyInt(), anyBoolean(), any() ) ).thenReturn(cachedThreadPool);
			  ExecutorBoltScheduler scheduler = new ExecutorBoltScheduler( CONNECTOR_KEY, mockExecutorFactory, _jobScheduler, _logService, 0, 10, Duration.ofMinutes( 1 ), 0, ForkJoinPool.commonPool() );

			  scheduler.Start();
			  scheduler.Stop();

			  assertTrue( cachedThreadPool.Shutdown );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createdShouldAddConnectionToActiveConnections() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreatedShouldAddConnectionToActiveConnections()
		 {
			  string id = System.Guid.randomUUID().ToString();
			  BoltConnection connection = NewConnection( id );

			  _boltScheduler.start();
			  _boltScheduler.created( connection );

			  verify( connection ).id();
			  assertTrue( _boltScheduler.isRegistered( connection ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void destroyedShouldRemoveConnectionFromActiveConnections() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DestroyedShouldRemoveConnectionFromActiveConnections()
		 {
			  string id = System.Guid.randomUUID().ToString();
			  BoltConnection connection = NewConnection( id );

			  _boltScheduler.start();
			  _boltScheduler.created( connection );
			  _boltScheduler.closed( connection );

			  assertFalse( _boltScheduler.isRegistered( connection ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enqueuedShouldScheduleJob() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void EnqueuedShouldScheduleJob()
		 {
			  string id = System.Guid.randomUUID().ToString();
			  AtomicBoolean exitCondition = new AtomicBoolean();
			  BoltConnection connection = NewConnection( id );
			  when( connection.ProcessNextBatch() ).thenAnswer(inv => AwaitExit(exitCondition));

			  _boltScheduler.start();
			  _boltScheduler.created( connection );
			  _boltScheduler.enqueued( connection, Jobs.noop() );

			  Predicates.await( () => _boltScheduler.isActive(connection), 1, MINUTES );
			  exitCondition.set( true );
			  Predicates.await( () => !_boltScheduler.isActive(connection), 1, MINUTES );

			  verify( connection ).processNextBatch();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void enqueuedShouldNotScheduleJobWhenActiveWorkItemExists() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void EnqueuedShouldNotScheduleJobWhenActiveWorkItemExists()
		 {
			  string id = System.Guid.randomUUID().ToString();
			  BoltConnection connection = NewConnection( id );
			  AtomicBoolean exitCondition = new AtomicBoolean();
			  when( connection.ProcessNextBatch() ).thenAnswer(inv => AwaitExit(exitCondition));

			  _boltScheduler.start();
			  _boltScheduler.created( connection );
			  _boltScheduler.enqueued( connection, Jobs.noop() );

			  Predicates.await( () => _boltScheduler.isActive(connection), 1, MINUTES );
			  _boltScheduler.enqueued( connection, Jobs.noop() );
			  exitCondition.set( true );
			  Predicates.await( () => !_boltScheduler.isActive(connection), 1, MINUTES );

			  verify( connection ).processNextBatch();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failingJobShouldLogAndStopConnection() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailingJobShouldLogAndStopConnection()
		 {
			  AtomicBoolean stopped = new AtomicBoolean();
			  string id = System.Guid.randomUUID().ToString();
			  BoltConnection connection = NewConnection( id );
			  doThrow( new Exception( "some unexpected error" ) ).when( connection ).processNextBatch();
			  doAnswer( inv => stopped.getAndSet( true ) ).when( connection ).stop();

			  _boltScheduler.start();
			  _boltScheduler.created( connection );
			  _boltScheduler.enqueued( connection, Jobs.noop() );

			  Predicates.await( () => stopped.get(), 1, MINUTES );

			  assertFalse( _boltScheduler.isActive( connection ) );
			  verify( connection ).processNextBatch();
			  verify( connection ).stop();

			  _logProvider.assertExactly( AssertableLogProvider.inLog( containsString( typeof( BoltServer ).Assembly.GetName().Name ) ).error(containsString("Unexpected error during job scheduling for session"), matchesExceptionMessage(containsString("some unexpected error"))) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void successfulJobsShouldTriggerSchedulingOfPendingJobs() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SuccessfulJobsShouldTriggerSchedulingOfPendingJobs()
		 {
			  AtomicInteger counter = new AtomicInteger();
			  string id = System.Guid.randomUUID().ToString();
			  BoltConnection connection = NewConnection( id );
			  when( connection.ProcessNextBatch() ).thenAnswer(inv => counter.incrementAndGet() > 0);
			  when( connection.HasPendingJobs() ).thenReturn(true).thenReturn(false);

			  _boltScheduler.start();
			  _boltScheduler.created( connection );
			  _boltScheduler.enqueued( connection, Jobs.noop() );

			  Predicates.await( () => counter.get() > 1, 1, MINUTES );

			  verify( connection, times( 2 ) ).processNextBatch();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void destroyedShouldCancelActiveWorkItem() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DestroyedShouldCancelActiveWorkItem()
		 {
			  AtomicInteger processNextBatchCount = new AtomicInteger();
			  string id = System.Guid.randomUUID().ToString();
			  BoltConnection connection = NewConnection( id );
			  AtomicBoolean exitCondition = new AtomicBoolean();
			  when( connection.ProcessNextBatch() ).thenAnswer(inv =>
			  {
				processNextBatchCount.incrementAndGet();
				return AwaitExit( exitCondition );
			  });

			  _boltScheduler.start();
			  _boltScheduler.created( connection );
			  _boltScheduler.enqueued( connection, Jobs.noop() );

			  Predicates.await( () => processNextBatchCount.get() > 0, 1, MINUTES );

			  _boltScheduler.closed( connection );

			  Predicates.await( () => !_boltScheduler.isActive(connection), 1, MINUTES );

			  assertFalse( _boltScheduler.isActive( connection ) );
			  assertEquals( 1, processNextBatchCount.get() );

			  exitCondition.set( true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createdWorkerThreadsShouldContainConnectorName() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreatedWorkerThreadsShouldContainConnectorName()
		 {
			  AtomicInteger executeBatchCompletionCount = new AtomicInteger();
			  AtomicReference<Thread> poolThread = new AtomicReference<Thread>();
			  AtomicReference<string> poolThreadName = new AtomicReference<string>();

			  string id = System.Guid.randomUUID().ToString();
			  BoltConnection connection = NewConnection( id );
			  when( connection.HasPendingJobs() ).thenAnswer(inv =>
			  {
				executeBatchCompletionCount.incrementAndGet();
				return false;
			  });
			  when( connection.ProcessNextBatch() ).thenAnswer(inv =>
			  {
				poolThread.set( Thread.CurrentThread );
				poolThreadName.set( Thread.CurrentThread.Name );
				return true;
			  });

			  _boltScheduler.start();
			  _boltScheduler.created( connection );
			  _boltScheduler.enqueued( connection, Jobs.noop() );

			  Predicates.await( () => executeBatchCompletionCount.get() > 0, 1, MINUTES );

			  assertThat( poolThread.get().Name, not(equalTo(poolThreadName.get())) );
			  assertThat( poolThread.get().Name, containsString(string.Format("[{0}]", CONNECTOR_KEY)) );
			  assertThat( poolThread.get().Name, not(containsString(string.Format("[{0}]", connection.RemoteAddress()))) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createdWorkerThreadsShouldContainConnectorNameAndRemoteAddressInTheirNamesWhenActive() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreatedWorkerThreadsShouldContainConnectorNameAndRemoteAddressInTheirNamesWhenActive()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<String> capturedThreadName = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<string> capturedThreadName = new AtomicReference<string>();

			  AtomicInteger processNextBatchCount = new AtomicInteger();
			  string id = System.Guid.randomUUID().ToString();
			  BoltConnection connection = NewConnection( id );
			  AtomicBoolean exitCondition = new AtomicBoolean();
			  when( connection.ProcessNextBatch() ).thenAnswer(inv =>
			  {
				capturedThreadName.set( Thread.CurrentThread.Name );
				processNextBatchCount.incrementAndGet();
				return AwaitExit( exitCondition );
			  });

			  _boltScheduler.start();
			  _boltScheduler.created( connection );
			  _boltScheduler.enqueued( connection, Jobs.noop() );

			  Predicates.await( () => processNextBatchCount.get() > 0, 1, MINUTES );

			  assertThat( capturedThreadName.get(), containsString(string.Format("[{0}]", CONNECTOR_KEY)) );
			  assertThat( capturedThreadName.get(), containsString(string.Format("[{0}]", connection.RemoteAddress())) );

			  exitCondition.set( true );
		 }

		 private BoltConnection NewConnection( string id )
		 {
			  BoltConnection result = mock( typeof( BoltConnection ) );
			  when( result.Id() ).thenReturn(id);
			  when( result.RemoteAddress() ).thenReturn(new InetSocketAddress("localhost", 32_000));
			  return result;
		 }

		 private static bool AwaitExit( AtomicBoolean exitCondition )
		 {
			  Predicates.awaitForever( () => Thread.CurrentThread.Interrupted || exitCondition.get(), 500, MILLISECONDS );
			  return true;
		 }
	}

}