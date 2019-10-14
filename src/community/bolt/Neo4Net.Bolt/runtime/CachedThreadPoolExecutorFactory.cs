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

	using Log = Neo4Net.Logging.Log;

	public class CachedThreadPoolExecutorFactory : ExecutorFactory
	{
		 public const int UNBOUNDED_QUEUE = -1;
		 public const int SYNCHRONOUS_QUEUE = 0;

		 private readonly Log _log;
		 private readonly RejectedExecutionHandler _rejectionHandler;

		 public CachedThreadPoolExecutorFactory( Log log ) : this( log, new ThreadPoolExecutor.AbortPolicy() )
		 {
		 }

		 public CachedThreadPoolExecutorFactory( Log log, RejectedExecutionHandler rejectionHandler )
		 {
			  this._log = log;
			  this._rejectionHandler = rejectionHandler;
		 }

		 public override ExecutorService Create( int corePoolSize, int maxPoolSize, Duration keepAlive, int queueSize, bool startCoreThreads, ThreadFactory threadFactory )
		 {
			  ThreadPool result = new ThreadPool( this, corePoolSize, maxPoolSize, keepAlive, CreateTaskQueue( queueSize ), threadFactory, _rejectionHandler );

			  if ( startCoreThreads )
			  {
					result.prestartAllCoreThreads();
			  }

			  return result;
		 }

		 private static BlockingQueue<ThreadStart> CreateTaskQueue( int queueSize )
		 {
			  if ( queueSize == UNBOUNDED_QUEUE )
			  {
					return new LinkedBlockingQueue<ThreadStart>();
			  }
			  else if ( queueSize == SYNCHRONOUS_QUEUE )
			  {
					return new SynchronousQueue<ThreadStart>();
			  }
			  else if ( queueSize > 0 )
			  {
					return new ArrayBlockingQueue<ThreadStart>( queueSize );
			  }

			  throw new System.ArgumentException( string.Format( "Unsupported queue size {0:D} for thread pool creation.", queueSize ) );
		 }

		 private class ThreadPool : ThreadPoolExecutor
		 {
			 private readonly CachedThreadPoolExecutorFactory _outerInstance;


			  internal ThreadPool( CachedThreadPoolExecutorFactory outerInstance, int corePoolSize, int maxPoolSize, Duration keepAlive, BlockingQueue<ThreadStart> workQueue, ThreadFactory threadFactory, RejectedExecutionHandler rejectionHandler ) : base( corePoolSize, maxPoolSize, keepAlive.toMillis(), MILLISECONDS, workQueue, threadFactory, rejectionHandler )
			  {
				  this._outerInstance = outerInstance;
			  }

		 }

	}

}