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
namespace Org.Neo4j.Kernel.Api.Impl.Fulltext
{

	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Org.Neo4j.Kernel.Api.Impl.Index;
	using Org.Neo4j.Kernel.Api.Index;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using Group = Org.Neo4j.Scheduler.Group;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using BinaryLatch = Org.Neo4j.Util.concurrent.BinaryLatch;

	/// <summary>
	/// A sink for index updates that will eventually be applied.
	/// </summary>
	public class IndexUpdateSink
	{
		 private readonly JobScheduler _scheduler;
		 private readonly Semaphore _updateQueueLimit;

		 internal IndexUpdateSink( JobScheduler scheduler, int eventuallyConsistentUpdateQueueLimit )
		 {
			  this._scheduler = scheduler;
			  _updateQueueLimit = new Semaphore( eventuallyConsistentUpdateQueueLimit );
		 }

		 public virtual void EnqueueUpdate<T1, T2>( DatabaseIndex<T1> index, IndexUpdater indexUpdater, IndexEntryUpdate<T2> update ) where T1 : Org.Neo4j.Storageengine.Api.schema.IndexReader
		 {
			  _updateQueueLimit.acquireUninterruptibly();
			  ThreadStart eventualUpdate = () =>
			  {
				try
				{
					 indexUpdater.Process( update );
				}
				catch ( IndexEntryConflictException e )
				{
					 MarkAsFailed( index, e );
				}
				finally
				{
					 _updateQueueLimit.release();
				}
			  };

			  try
			  {
					_scheduler.schedule( Group.INDEX_UPDATING, eventualUpdate );
			  }
			  catch ( Exception e )
			  {
					_updateQueueLimit.release(); // Avoid leaking permits if job scheduling fails.
					throw e;
			  }
		 }

		 private static void MarkAsFailed<T1>( DatabaseIndex<T1> index, IndexEntryConflictException conflict ) where T1 : Org.Neo4j.Storageengine.Api.schema.IndexReader
		 {
			  try
			  {
					index.MarkAsFailed( conflict.Message );
			  }
			  catch ( IOException ioe )
			  {
					ioe.addSuppressed( conflict );
					throw new UncheckedIOException( ioe );
			  }
		 }

		 public virtual void CloseUpdater<T1>( DatabaseIndex<T1> index, IndexUpdater indexUpdater ) where T1 : Org.Neo4j.Storageengine.Api.schema.IndexReader
		 {
			  _scheduler.schedule(Group.INDEX_UPDATING, () =>
			  {
				try
				{
					 indexUpdater.Close();
				}
				catch ( IndexEntryConflictException e )
				{
					 MarkAsFailed( index, e );
				}
			  });
		 }

		 public virtual void AwaitUpdateApplication()
		 {
			  BinaryLatch updateLatch = new BinaryLatch();
			  _scheduler.schedule( Group.INDEX_UPDATING, updateLatch.release );
			  updateLatch.Await();
		 }
	}

}