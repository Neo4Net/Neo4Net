using System;
using System.Diagnostics;
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

	using Suppliers = Neo4Net.Functions.Suppliers;
	using IndexPopulationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexPopulationFailedKernelException;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using ByteBufferFactory = Neo4Net.Kernel.Impl.Index.Schema.ByteBufferFactory;
	using UnsafeDirectByteBufferAllocator = Neo4Net.Kernel.Impl.Index.Schema.UnsafeDirectByteBufferAllocator;
	using GlobalMemoryTracker = Neo4Net.Memory.GlobalMemoryTracker;
	using ThreadSafePeakMemoryAllocationTracker = Neo4Net.Memory.ThreadSafePeakMemoryAllocationTracker;
	using CapableIndexDescriptor = Neo4Net.Storageengine.Api.schema.CapableIndexDescriptor;
	using PopulationProgress = Neo4Net.Storageengine.Api.schema.PopulationProgress;
	using Runnables = Neo4Net.Utils.Concurrent.Runnables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Thread.currentThread;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.FutureAdapter.latchGuardedValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.BlockBasedIndexPopulator.parseBlockSize;

	/// <summary>
	/// A background job for initially populating one or more index over existing data in the database.
	/// Use provided store view to scan store. Participating <seealso cref="IndexPopulator"/> are added with
	/// <seealso cref="addPopulator(IndexPopulator, CapableIndexDescriptor, string, FlippableIndexProxy, FailedIndexProxyFactory)"/>
	/// before <seealso cref="run() running"/> this job.
	/// </summary>
	public class IndexPopulationJob : ThreadStart
	{
		 private readonly IndexingService.Monitor _monitor;
		 private readonly bool _verifyBeforeFlipping;
		 private readonly ByteBufferFactory _bufferFactory;
		 private readonly ThreadSafePeakMemoryAllocationTracker _memoryAllocationTracker;
		 private readonly MultipleIndexPopulator _multiPopulator;
		 private readonly System.Threading.CountdownEvent _doneSignal = new System.Threading.CountdownEvent( 1 );

		 private volatile StoreScan<IndexPopulationFailedKernelException> _storeScan;
		 private volatile bool _cancelled;

		 public IndexPopulationJob( MultipleIndexPopulator multiPopulator, IndexingService.Monitor monitor, bool verifyBeforeFlipping )
		 {
			  this._multiPopulator = multiPopulator;
			  this._monitor = monitor;
			  this._verifyBeforeFlipping = verifyBeforeFlipping;
			  this._memoryAllocationTracker = new ThreadSafePeakMemoryAllocationTracker( GlobalMemoryTracker.INSTANCE );
			  this._bufferFactory = new ByteBufferFactory( () => new UnsafeDirectByteBufferAllocator(_memoryAllocationTracker), parseBlockSize() );
		 }

		 /// <summary>
		 /// Adds an <seealso cref="IndexPopulator"/> to be populated in this store scan. All participating populators must
		 /// be added before calling <seealso cref="run()"/>. </summary>
		 ///  <param name="populator"> <seealso cref="IndexPopulator"/> to participate. </param>
		 /// <param name="capableIndexDescriptor"> <seealso cref="CapableIndexDescriptor"/> meta information about index. </param>
		 /// <param name="indexUserDescription"> user description of this index. </param>
		 /// <param name="flipper"> <seealso cref="FlippableIndexProxy"/> to call after a successful population. </param>
		 /// <param name="failedIndexProxyFactory"> <seealso cref="FailedIndexProxyFactory"/> to use after an unsuccessful population. </param>
		 internal virtual MultipleIndexPopulator.IndexPopulation AddPopulator( IndexPopulator populator, CapableIndexDescriptor capableIndexDescriptor, string indexUserDescription, FlippableIndexProxy flipper, FailedIndexProxyFactory failedIndexProxyFactory )
		 {
			  Debug.Assert( _storeScan == null, "Population have already started, too late to add populators at this point" );
			  return this._multiPopulator.addPopulator( populator, capableIndexDescriptor, flipper, failedIndexProxyFactory, indexUserDescription );
		 }

		 /// <summary>
		 /// Scans the store using store view and populates all participating <seealso cref="IndexPopulator"/> with data relevant to
		 /// each index.
		 /// The scan continues as long as there's at least one non-failed populator.
		 /// </summary>
		 public override void Run()
		 {
			  string oldThreadName = currentThread().Name;
			  try
			  {
					if ( !_multiPopulator.hasPopulators() )
					{
						 return;
					}
					if ( _storeScan != null )
					{
						 throw new System.InvalidOperationException( "Population already started." );
					}

					currentThread().Name = "Index populator";
					try
					{
						 _multiPopulator.create();
						 _multiPopulator.resetIndexCounts();

						 _monitor.indexPopulationScanStarting();
						 IndexAllEntities();
						 _monitor.indexPopulationScanComplete();
						 if ( _cancelled )
						 {
							  _multiPopulator.cancel();
							  // We remain in POPULATING state
							  return;
						 }
						 _multiPopulator.flipAfterPopulation( _verifyBeforeFlipping );
					}
					catch ( Exception t )
					{
						 _multiPopulator.fail( t );
					}
			  }
			  finally
			  {
					// will only close "additional" resources, not the actual populators, since that's managed by flip
					Runnables.runAll( "Failed to close resources in IndexPopulationJob", () => _multiPopulator.close(true), () => _monitor.populationJobCompleted(_memoryAllocationTracker.peakMemoryUsage()), _bufferFactory.close, _doneSignal.countDown, () => currentThread().setName(oldThreadName) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void indexAllEntities() throws org.neo4j.kernel.api.exceptions.index.IndexPopulationFailedKernelException
		 private void IndexAllEntities()
		 {
			  _storeScan = _multiPopulator.indexAllEntities();
			  _storeScan.run();
		 }

		 internal virtual PopulationProgress GetPopulationProgress( MultipleIndexPopulator.IndexPopulation indexPopulation )
		 {
			  if ( _storeScan == null )
			  {
					// indexing hasn't begun yet
					return Neo4Net.Storageengine.Api.schema.PopulationProgress_Fields.None;
			  }
			  PopulationProgress storeScanProgress = _storeScan.Progress;
			  return indexPopulation.Progress( storeScanProgress );
		 }

		 public virtual Future<Void> Cancel()
		 {
			  // Stop the population
			  if ( _storeScan != null )
			  {
					_cancelled = true;
					_storeScan.stop();
					_monitor.populationCancelled();
			  }

			  return latchGuardedValue( Suppliers.singleton( null ), _doneSignal, "Index population job cancel" );
		 }

		 internal virtual void CancelPopulation( MultipleIndexPopulator.IndexPopulation population )
		 {
			  _multiPopulator.cancelIndexPopulation( population );
		 }

		 internal virtual void DropPopulation( MultipleIndexPopulator.IndexPopulation population )
		 {
			  _multiPopulator.dropIndexPopulation( population );
		 }

		 /// <summary>
		 /// A transaction happened that produced the given updates. Let this job incorporate its data,
		 /// feeding it to the <seealso cref="IndexPopulator"/>.
		 /// </summary>
		 /// <param name="update"> <seealso cref="IndexEntryUpdate"/> to queue. </param>
		 public virtual void Update<T1>( IndexEntryUpdate<T1> update )
		 {
			  _multiPopulator.queueUpdate( update );
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "[populator:" + _multiPopulator + "]";
		 }

		 /// <summary>
		 /// Awaits completion of this population job, but waits maximum the given time.
		 /// </summary>
		 /// <param name="time"> time to wait at the most for completion. A value of 0 means indefinite wait. </param>
		 /// <param name="unit"> <seealso cref="TimeUnit unit"/> of the {@code time}. </param>
		 /// <returns> {@code true} if the job is still running when leaving this method, otherwise {@code false} meaning that the job is completed. </returns>
		 /// <exception cref="InterruptedException"> if the wait got interrupted. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean awaitCompletion(long time, java.util.concurrent.TimeUnit unit) throws InterruptedException
		 public virtual bool AwaitCompletion( long time, TimeUnit unit )
		 {
			  if ( time == 0 )
			  {
					_doneSignal.await();
					return false;
			  }
			  bool completed = _doneSignal.await( time, unit );
			  return !completed;
		 }

		 public virtual ByteBufferFactory BufferFactory()
		 {
			  return _bufferFactory;
		 }
	}

}