using System;
using System.Collections.Generic;
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
namespace Neo4Net.Kernel.Impl.Api.index
{

	using Predicates = Neo4Net.Functions.Predicates;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using IndexPopulationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexPopulationFailedKernelException;
	using Neo4Net.Kernel.Api.Index;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;
	using FeatureToggles = Neo4Net.Utils.FeatureToggles;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.NamedThreadFactory.daemon;

	/// <summary>
	/// A <seealso cref="MultipleIndexPopulator"/> that gathers all incoming updates from the <seealso cref="IndexStoreView"/> in batches of
	/// size <seealso cref="BATCH_SIZE"/> and then flushes each batch from different thread using <seealso cref="ExecutorService executor"/>.
	/// <para>
	/// It is possible for concurrent updates from transactions to arrive while index population is in progress. Such
	/// updates are inserted in the queue. When store scan notices that queue size has reached <seealso cref="QUEUE_THRESHOLD"/> than
	/// it drains all batched updates and waits for all submitted to the executor tasks to complete and flushes updates from
	/// the queue using <seealso cref="MultipleIndexUpdater"/>. If queue size never reaches <seealso cref="QUEUE_THRESHOLD"/> than all queued
	/// concurrent updates are flushed after the store scan in <seealso cref="MultipleIndexPopulator.flipAfterPopulation(bool)"/>.
	/// </para>
	/// <para>
	/// Inner <seealso cref="ExecutorService executor"/> is shut down after the store scan completes.
	/// </para>
	/// </summary>
	public class BatchingMultipleIndexPopulator : MultipleIndexPopulator
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_taskQueueSize = FeatureToggles.getInteger( this.GetType(), TASK_QUEUE_SIZE_NAME, NumberOfPopulationWorkers * 2 );
		}

		 internal const string TASK_QUEUE_SIZE_NAME = "task_queue_size";
		 internal const string AWAIT_TIMEOUT_MINUTES_NAME = "await_timeout_minutes";
		 public const string MAXIMUM_NUMBER_OF_WORKERS_NAME = "population_workers_maximum";

		 private static readonly string _eol = Environment.NewLine;
		 private const string FLUSH_THREAD_NAME_PREFIX = "Index Population Flush Thread";

		 // Maximum number of workers processing batches of updates from the scan. It is capped because there's only a single
		 // thread generating updates and it generally cannot saturate all the workers anyway.
		 private readonly int _maximumNumberOfWorkers = FeatureToggles.getInteger( this.GetType(), MAXIMUM_NUMBER_OF_WORKERS_NAME, min(8, Runtime.Runtime.availableProcessors() - 1) );
		 private int _taskQueueSize;
		 private readonly int _awaitTimeoutMinutes = FeatureToggles.getInteger( this.GetType(), AWAIT_TIMEOUT_MINUTES_NAME, 30 );

		 private readonly AtomicLong _activeTasks = new AtomicLong();
		 private readonly ExecutorService _executor;

		 /// <summary>
		 /// Creates a new multi-threaded populator for the given store view. </summary>
		 ///  <param name="storeView"> the view of the store as a visitable of nodes </param>
		 /// <param name="logProvider"> the log provider </param>
		 /// <param name="type"> entity type to populate </param>
		 /// <param name="schemaState"> the schema state </param>
		 internal BatchingMultipleIndexPopulator( IndexStoreView storeView, LogProvider logProvider, EntityType type, SchemaState schemaState ) : base( storeView, logProvider, type, schemaState )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._executor = CreateThreadPool();
		 }

		 /// <summary>
		 /// Creates a new multi-threaded populator with the specified thread pool.
		 /// <para>
		 /// <b>NOTE:</b> for testing only.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="storeView"> the view of the store as a visitable of nodes </param>
		 /// <param name="executor"> the thread pool to use for batched index insertions </param>
		 /// <param name="logProvider"> the log provider </param>
		 /// <param name="schemaState"> the schema state </param>
		 internal BatchingMultipleIndexPopulator( IndexStoreView storeView, ExecutorService executor, LogProvider logProvider, SchemaState schemaState ) : base( storeView, logProvider, EntityType.NODE, schemaState )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._executor = executor;
		 }

		 public override StoreScan<IndexPopulationFailedKernelException> IndexAllEntities()
		 {
			  StoreScan<IndexPopulationFailedKernelException> storeScan = base.IndexAllEntities();
			  return new BatchingStoreScan<IndexPopulationFailedKernelException>( this, storeScan );
		 }

		 protected internal override void FlushAll()
		 {
			  base.FlushAll();
			  AwaitCompletion();
		 }

		 public override string ToString()
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  string updatesString = Populations.Select( population => population.batchedUpdates.size() + " updates" ).collect(joining(", ", "[", "]"));

			  return "BatchingMultipleIndexPopulator{activeTasks=" + _activeTasks + ", executor=" + _executor + ", " +
						"batchedUpdates = " + updatesString + ", queuedUpdates = " + UpdatesQueue.Count + "}";
		 }

		 /// <summary>
		 /// Awaits <seealso cref="AWAIT_TIMEOUT_MINUTES"/> minutes for all previously submitted batch-flush tasks to complete.
		 /// Restores the interrupted status and exits normally when interrupted during waiting.
		 /// </summary>
		 /// <exception cref="IllegalStateException"> if tasks did not complete in <seealso cref="AWAIT_TIMEOUT_MINUTES"/> minutes. </exception>
		 private void AwaitCompletion()
		 {
			  try
			  {
					Log.debug( "Waiting " + _awaitTimeoutMinutes + " minutes for all submitted and active " + "flush tasks to complete." + _eol + this );

					System.Func<bool> allSubmittedTasksCompleted = () => _activeTasks.get() == 0;
					Predicates.await( allSubmittedTasksCompleted, _awaitTimeoutMinutes, TimeUnit.MINUTES );
			  }
			  catch ( TimeoutException )
			  {
					HandleTimeout();
			  }
		 }

		 /// <summary>
		 /// Insert the given batch of updates into the index defined by the given <seealso cref="IndexPopulation"/>.
		 /// Called from <seealso cref="MultipleIndexPopulator.flush(IndexPopulation)"/>.
		 /// </summary>
		 /// <param name="population"> the index population. </param>
		 internal override void DoFlush( IndexPopulation population )
		 {
			  _activeTasks.incrementAndGet();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<org.neo4j.kernel.api.index.IndexEntryUpdate<?>> batch = population.takeCurrentBatch();
			  IList<IndexEntryUpdate<object>> batch = population.TakeCurrentBatch();

			  _executor.execute(() =>
			  {
				try
				{
					 string batchDescription = "EMPTY";
					 if ( PrintDebug )
					 {
						  if ( batch.Count > 0 )
						  {
								batchDescription = format( "[%d, %d - %d]", batch.Count, batch[0].EntityId, batch[batch.Count - 1].EntityId );
						  }
						  Log.info( "Applying scan batch %s", batchDescription );
					 }
					 population.Populator.add( batch );
					 if ( PrintDebug )
					 {
						  Log.info( "Applied scan batch %s", batchDescription );
					 }
				}
				catch ( Exception failure )
				{
					 Fail( population, failure );
				}
				finally
				{
					 _activeTasks.decrementAndGet();
				}
			  });
		 }

		 /// <summary>
		 /// Shuts down the executor waiting <seealso cref="AWAIT_TIMEOUT_MINUTES"/> minutes for it's termination.
		 /// Restores the interrupted status and exits normally when interrupted during waiting.
		 /// </summary>
		 /// <param name="now"> <code>true</code> if <seealso cref="ExecutorService.shutdownNow()"/> should be used and <code>false</code> if
		 /// <seealso cref="ExecutorService.shutdown()"/> should be used. </param>
		 /// <exception cref="IllegalStateException"> if tasks did not complete in <seealso cref="AWAIT_TIMEOUT_MINUTES"/> minutes. </exception>
		 private void ShutdownExecutor( bool now )
		 {
			  Log.info( ( now ? "Forcefully shutting" : "Shutting" ) + " down executor." + _eol + this );
			  if ( now )
			  {
					_executor.shutdownNow();
			  }
			  else
			  {
					_executor.shutdown();
			  }

			  try
			  {
					bool tasksCompleted = _executor.awaitTermination( _awaitTimeoutMinutes, TimeUnit.MINUTES );
					if ( !tasksCompleted )
					{
						 HandleTimeout();
					}
			  }
			  catch ( InterruptedException )
			  {
					HandleInterrupt();
			  }
		 }

		 private void HandleTimeout()
		 {
			  throw new System.InvalidOperationException( "Index population tasks were not able to complete in " + _awaitTimeoutMinutes + " minutes." + _eol + this + _eol + AllStackTraces() );
		 }

		 private void HandleInterrupt()
		 {
			  Thread.CurrentThread.Interrupt();
			  Log.warn( "Interrupted while waiting for index population tasks to complete." + _eol + this );
		 }

		 private ExecutorService CreateThreadPool()
		 {
			  int threads = NumberOfPopulationWorkers;
			  BlockingQueue<ThreadStart> workQueue = new LinkedBlockingQueue<ThreadStart>( _taskQueueSize );
			  ThreadFactory threadFactory = daemon( FLUSH_THREAD_NAME_PREFIX );
			  RejectedExecutionHandler rejectedExecutionHandler = new ThreadPoolExecutor.CallerRunsPolicy();
			  return new ThreadPoolExecutor( threads, threads, 0L, TimeUnit.MILLISECONDS, workQueue, threadFactory, rejectedExecutionHandler );
		 }

		 /// <summary>
		 /// Finds all threads and corresponding stack traces which can potentially cause the
		 /// <seealso cref="ExecutorService executor"/> to not terminate in <seealso cref="AWAIT_TIMEOUT_MINUTES"/> minutes.
		 /// </summary>
		 /// <returns> thread dump as string. </returns>
		 private static string AllStackTraces()
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return Thread.AllStackTraces.entrySet().Select(entry => Exceptions.stringify(entry.Key, entry.Value)).collect(joining());
		 }

		 /// <summary>
		 /// Calculate number of workers that will perform index population
		 /// </summary>
		 /// <returns> number of threads that will be used for index population </returns>
		 private int NumberOfPopulationWorkers
		 {
			 get
			 {
				  return Math.Max( 2, _maximumNumberOfWorkers );
			 }
		 }

		 public override void Close( bool populationCompletedSuccessfully )
		 {
			  base.Close( populationCompletedSuccessfully );
			  ShutdownExecutor( !populationCompletedSuccessfully );
		 }

		 /// <summary>
		 /// A delegating <seealso cref="StoreScan"/> implementation that flushes all pending updates and terminates the executor after
		 /// the delegate store scan completes.
		 /// </summary>
		 /// @param <E> type of the exception this store scan might get. </param>
		 private class BatchingStoreScan<E> : DelegatingStoreScan<E> where E : Exception
		 {
			 private readonly BatchingMultipleIndexPopulator _outerInstance;

			  internal BatchingStoreScan( BatchingMultipleIndexPopulator outerInstance, StoreScan<E> @delegate ) : base( @delegate )
			  {
				  this._outerInstance = outerInstance;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run() throws E
			  public override void Run()
			  {
					base.Run();
					_outerInstance.log.info( "Completed node store scan. " + "Flushing all pending updates." + _eol + _outerInstance );
					outerInstance.FlushAll();
			  }
		 }
	}

}