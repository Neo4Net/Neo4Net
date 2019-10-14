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
	using Answer = org.mockito.stubbing.Answer;


	using Jobs = Neo4Net.Bolt.testing.Jobs;
	using Predicates = Neo4Net.Functions.Predicates;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using LogService = Neo4Net.Logging.@internal.LogService;
	using SimpleLogService = Neo4Net.Logging.@internal.SimpleLogService;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Matchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ExecutorBoltSchedulerConcurrencyTest
	{
		private bool InstanceFieldsInitialized = false;

		public ExecutorBoltSchedulerConcurrencyTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_logService = new SimpleLogService( _logProvider, _logProvider );
			_executorFactory = new NotifyingThreadPoolFactory( this );
			_boltScheduler = new ExecutorBoltScheduler( CONNECTOR_KEY, _executorFactory, _jobScheduler, _logService, MAX_POOL_SIZE, MAX_POOL_SIZE, Duration.ofMinutes( 1 ), 0, ForkJoinPool.commonPool() );
		}

		 private const string CONNECTOR_KEY = "connector-id";
		 private const int MAX_POOL_SIZE = 5;

		 private readonly System.Threading.CountdownEvent _beforeExecuteEvent = new System.Threading.CountdownEvent( 1 );
		 private readonly System.Threading.CountdownEvent _beforeExecuteBarrier = new System.Threading.CountdownEvent( MAX_POOL_SIZE );
		 private readonly System.Threading.CountdownEvent _afterExecuteEvent = new System.Threading.CountdownEvent( 1 );
		 private readonly System.Threading.CountdownEvent _afterExecuteBarrier = new System.Threading.CountdownEvent( MAX_POOL_SIZE );

		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();
		 private LogService _logService;
		 private ExecutorFactory _executorFactory;
		 private readonly JobScheduler _jobScheduler = mock( typeof( JobScheduler ) );
		 private ExecutorBoltScheduler _boltScheduler;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  when( _jobScheduler.threadFactory( any() ) ).thenReturn(Executors.defaultThreadFactory());

			  _boltScheduler.start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Cleanup()
		 {
			  _boltScheduler.stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvokeHandleSchedulingErrorIfNoThreadsAvailable() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvokeHandleSchedulingErrorIfNoThreadsAvailable()
		 {
			  AtomicInteger handleSchedulingErrorCounter = new AtomicInteger( 0 );
			  BoltConnection newConnection = newConnection( System.Guid.randomUUID().ToString() );
			  doAnswer( NewCountingAnswer( handleSchedulingErrorCounter ) ).when( newConnection ).handleSchedulingError( any() );

			  BlockAllThreads();

			  // register connection
			  _boltScheduler.created( newConnection );

			  // send a job and wait for it to enter handleSchedulingError and block there
			  CompletableFuture.runAsync( () => _boltScheduler.enqueued(newConnection, Jobs.noop()) );
			  Predicates.awaitForever( () => handleSchedulingErrorCounter.get() > 0, 500, MILLISECONDS );

			  // verify that handleSchedulingError is called once
			  assertEquals( 1, handleSchedulingErrorCounter.get() );

			  // allow all threads to complete
			  _afterExecuteEvent.Signal();
			  _afterExecuteBarrier.await();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotScheduleNewJobIfHandlingSchedulingError() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotScheduleNewJobIfHandlingSchedulingError()
		 {
			  AtomicInteger handleSchedulingErrorCounter = new AtomicInteger( 0 );
			  AtomicBoolean exitCondition = new AtomicBoolean();
			  BoltConnection newConnection = newConnection( System.Guid.randomUUID().ToString() );
			  doAnswer( NewBlockingAnswer( handleSchedulingErrorCounter, exitCondition ) ).when( newConnection ).handleSchedulingError( any() );

			  BlockAllThreads();

			  // register connection
			  _boltScheduler.created( newConnection );

			  // send a job and wait for it to enter handleSchedulingError and block there
			  CompletableFuture.runAsync( () => _boltScheduler.enqueued(newConnection, Jobs.noop()) );
			  Predicates.awaitForever( () => handleSchedulingErrorCounter.get() > 0, 500, MILLISECONDS );

			  // allow all threads to complete
			  _afterExecuteEvent.Signal();
			  _afterExecuteBarrier.await();

			  // post a job
			  _boltScheduler.enqueued( newConnection, Jobs.noop() );

			  // exit handleSchedulingError
			  exitCondition.set( true );

			  // verify that handleSchedulingError is called once and processNextBatch never.
			  assertEquals( 1, handleSchedulingErrorCounter.get() );
			  verify( newConnection, never() ).processNextBatch();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void blockAllThreads() throws InterruptedException
		 private void BlockAllThreads()
		 {
			  for ( int i = 0; i < MAX_POOL_SIZE; i++ )
			  {
					BoltConnection connection = NewConnection( System.Guid.randomUUID().ToString() );
					_boltScheduler.created( connection );
					_boltScheduler.enqueued( connection, Jobs.noop() );
			  }

			  _beforeExecuteEvent.Signal();
			  _beforeExecuteBarrier.await();
		 }

		 private Answer<T> NewCountingAnswer<T>( AtomicInteger counter )
		 {
			  return invocationOnMock =>
			  {
				counter.incrementAndGet();
				return null;
			  };
		 }

		 private Answer<T> NewBlockingAnswer<T>( AtomicInteger counter, AtomicBoolean exitCondition )
		 {
			  return invocationOnMock =>
			  {
				counter.incrementAndGet();
				Predicates.awaitForever( () => Thread.CurrentThread.Interrupted || exitCondition.get(), 500, MILLISECONDS );
				return null;
			  };
		 }

		 private BoltConnection NewConnection( string id )
		 {
			  BoltConnection result = mock( typeof( BoltConnection ) );
			  when( result.Id() ).thenReturn(id);
			  when( result.RemoteAddress() ).thenReturn(new InetSocketAddress("localhost", 32_000));
			  return result;
		 }

		 private class NotifyingThreadPoolFactory : ExecutorFactory
		 {
			 private readonly ExecutorBoltSchedulerConcurrencyTest _outerInstance;

			 public NotifyingThreadPoolFactory( ExecutorBoltSchedulerConcurrencyTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override ExecutorService Create( int corePoolSize, int maxPoolSize, Duration keepAlive, int queueSize, bool startCoreThreads, ThreadFactory threadFactory )
			  {
					return new NotifyingThreadPoolExecutor( _outerInstance, corePoolSize, maxPoolSize, keepAlive, new SynchronousQueue<ThreadStart>(), threadFactory, new ThreadPoolExecutor.AbortPolicy() );
			  }
		 }

		 private class NotifyingThreadPoolExecutor : ThreadPoolExecutor
		 {
			 private readonly ExecutorBoltSchedulerConcurrencyTest _outerInstance;


			  internal NotifyingThreadPoolExecutor( ExecutorBoltSchedulerConcurrencyTest outerInstance, int corePoolSize, int maxPoolSize, Duration keepAlive, BlockingQueue<ThreadStart> workQueue, ThreadFactory threadFactory, RejectedExecutionHandler rejectionHandler ) : base( corePoolSize, maxPoolSize, keepAlive.toMillis(), MILLISECONDS, workQueue, threadFactory, rejectionHandler )
			  {
				  this._outerInstance = outerInstance;
			  }

			  protected internal override void BeforeExecute( Thread t, ThreadStart r )
			  {
					try
					{
						 outerInstance.beforeExecuteEvent.await();
						 base.BeforeExecute( t, r );
						 outerInstance.beforeExecuteBarrier.Signal();
					}
					catch ( Exception ex )
					{
						 throw new Exception( ex );
					}
			  }

			  protected internal override void AfterExecute( ThreadStart r, Exception t )
			  {
					try
					{
						 outerInstance.afterExecuteEvent.await();
						 base.AfterExecute( r, t );
						 outerInstance.afterExecuteBarrier.Signal();
					}
					catch ( Exception ex )
					{
						 throw new Exception( ex );
					}
			  }
		 }
	}

}