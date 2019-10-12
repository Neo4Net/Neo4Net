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
namespace Org.Apache.Lucene.Index
{

	using Predicates = Org.Neo4j.Function.Predicates;
	using NamedThreadFactory = Org.Neo4j.Helpers.NamedThreadFactory;
	using FeatureToggles = Org.Neo4j.Util.FeatureToggles;

	/// <summary>
	/// Lucene indexes merge scheduler that execute merges in a thread pool instead of starting separate thread for each
	/// merge. Each writer have it's own scheduler but all of them use shared pool.
	/// 
	/// Current implementation is minimalistic in a number of things it knows about lucene internals to simplify
	/// migrations overhead. It wraps up lucene internal merge tasks and execute them in a common thread pool.
	/// In case if pool and queue exhausted merge will be performed in callers thread.
	/// Since we cant rely on lucene per writer merge threads we need to perform writer tasks counting ourselves to prevent
	/// cases while writer will be closed in the middle of merge and will wait for all writer related merges to complete
	/// before allowing close of writer scheduler.
	/// </summary>
	public class PooledConcurrentMergeScheduler : ConcurrentMergeScheduler
	{
		 private static readonly int _poolQueueCapacity = FeatureToggles.getInteger( typeof( PooledConcurrentMergeScheduler ), "pool.queue.capacity", 100 );
		 private static readonly int _poolMinimumThreads = FeatureToggles.getInteger( typeof( PooledConcurrentMergeScheduler ), "pool.minimum.threads", 4 );
		 private static readonly int _poolMaximumThreads = FeatureToggles.getInteger( typeof( PooledConcurrentMergeScheduler ), "pool.maximum.threads", 10 );

		 private readonly LongAdder _writerTaskCounter = new LongAdder();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void merge(IndexWriter writer, MergeTrigger trigger, boolean newMergesFound) throws java.io.IOException
		 public override void Merge( IndexWriter writer, MergeTrigger trigger, bool newMergesFound )
		 {
			  while ( true )
			  {
					MergePolicy.OneMerge merge = writer.NextMerge;
					if ( merge == null )
					{
						 return;
					}
					bool success = false;
					try
					{
						 MergeThread mergeThread = getMergeThread( writer, merge );
						 _writerTaskCounter.increment();
						 PooledConcurrentMergePool.MergeThreadsPool.submit( MergeTask( mergeThread ) );
						 success = true;
					}
					finally
					{
						 if ( !success )
						 {
							  writer.mergeFinish( merge );
							  _writerTaskCounter.decrement();
						 }
					}
			  }
		 }

		 public override void Close()
		 {
			  WaitForAllTasks();
			  base.Close();
		 }

		 protected internal override void UpdateMergeThreads()
		 {
			  //noop
		 }

		 internal override void RemoveMergeThread()
		 {
			  // noop
		 }

		 internal virtual long WriterTaskCount
		 {
			 get
			 {
				  return _writerTaskCounter.longValue();
			 }
		 }

		 private ThreadStart MergeTask( MergeThread mergeThread )
		 {
			  return new MergeTask( mergeThread, _writerTaskCounter );
		 }

		 private void WaitForAllTasks()
		 {
			  try
			  {
					Predicates.await( () => _writerTaskCounter.longValue() == 0, 10, TimeUnit.MINUTES, 10, TimeUnit.MILLISECONDS );
			  }
			  catch ( Exception t )
			  {
					throw new Exception( t );
			  }
		 }

		 private class PooledConcurrentMergePool
		 {
			  internal static readonly ExecutorService MergeThreadsPool = new ThreadPoolExecutor( _poolMinimumThreads, MaximumPoolSize, 60L, TimeUnit.SECONDS, new ArrayBlockingQueue<>( _poolQueueCapacity ), new NamedThreadFactory( "Lucene-Merge", true ), new ThreadPoolExecutor.CallerRunsPolicy() );

			  internal static int MaximumPoolSize
			  {
				  get
				  {
						return Math.Max( _poolMaximumThreads, Runtime.Runtime.availableProcessors() );
				  }
			  }
		 }

		 private class MergeTask : ThreadStart
		 {
			  internal readonly MergeThread MergeThread;
			  internal readonly LongAdder TaskCounter;

			  internal MergeTask( MergeThread mergeThread, LongAdder taskCounter )
			  {
					this.MergeThread = mergeThread;
					this.TaskCounter = taskCounter;
			  }

			  public override void Run()
			  {
					try
					{
						 MergeThread.run();
					}
					finally
					{
						 TaskCounter.decrement();
					}
			  }
		 }
	}

}