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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{

	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Neo4Net.Kernel.Api.Impl.Index;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using Group = Neo4Net.Scheduler.Group;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using BinaryLatch = Neo4Net.Utils.Concurrent.BinaryLatch;

	/// <summary>
	/// A sink for index updates that will eventually be applied.
	/// </summary>
	public class IndexUpdateSink
	{
		 private readonly IJobScheduler _scheduler;
		 private readonly Semaphore _updateQueueLimit;

		 internal IndexUpdateSink( IJobScheduler scheduler, int eventuallyConsistentUpdateQueueLimit )
		 {
			  this._scheduler = scheduler;
			  _updateQueueLimit = new Semaphore( eventuallyConsistentUpdateQueueLimit );
		 }

		 public virtual void EnqueueUpdate<T1, T2>( DatabaseIndex<T1> index, IndexUpdater indexUpdater, IndexEntryUpdate<T2> update ) where T1 : Neo4Net.Storageengine.Api.schema.IndexReader
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

		 private static void MarkAsFailed<T1>( DatabaseIndex<T1> index, IndexEntryConflictException conflict ) where T1 : Neo4Net.Storageengine.Api.schema.IndexReader
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

		 public virtual void CloseUpdater<T1>( DatabaseIndex<T1> index, IndexUpdater indexUpdater ) where T1 : Neo4Net.Storageengine.Api.schema.IndexReader
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