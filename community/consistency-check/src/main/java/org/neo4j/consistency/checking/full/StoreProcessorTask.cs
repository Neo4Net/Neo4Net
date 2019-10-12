using System;

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
namespace Org.Neo4j.Consistency.checking.full
{
	using CacheAccess = Org.Neo4j.Consistency.checking.cache.CacheAccess;
	using Org.Neo4j.Consistency.checking.full;
	using Statistics = Org.Neo4j.Consistency.statistics.Statistics;
	using ProgressListener = Org.Neo4j.Helpers.progress.ProgressListener;
	using ProgressMonitorFactory = Org.Neo4j.Helpers.progress.ProgressMonitorFactory;
	using Org.Neo4j.Kernel.impl.store;
	using StoreAccess = Org.Neo4j.Kernel.impl.store.StoreAccess;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;

	public class StoreProcessorTask<R> : ConsistencyCheckerTask where R : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
	{
		 private readonly RecordStore<R> _store;
		 private readonly StoreProcessor _processor;
		 private readonly ProgressListener _progressListener;
		 private readonly StoreAccess _storeAccess;
		 private readonly CacheAccess _cacheAccess;
		 private readonly QueueDistribution _distribution;

		 internal StoreProcessorTask( string name, Statistics statistics, int threads, RecordStore<R> store, StoreAccess storeAccess, string builderPrefix, ProgressMonitorFactory.MultiPartBuilder builder, CacheAccess cacheAccess, StoreProcessor processor, QueueDistribution distribution ) : base( name, statistics, threads )
		 {
			  this._store = store;
			  this._storeAccess = storeAccess;
			  this._cacheAccess = cacheAccess;
			  this._processor = processor;
			  this._distribution = distribution;
			  this._progressListener = builder.ProgressForPart( name + IndexedPartName( store.StorageFile.Name, builderPrefix ), store.HighId );
		 }

		 private string IndexedPartName( string storeFileName, string prefix )
		 {
			  return prefix.Length != 0 ? "_" : format( "%s_pass_%s", storeFileName, prefix );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public void run()
		 public override void Run()
		 {
			  statistics.reset();
			  BeforeProcessing( _processor );
			  try
			  {
					if ( _processor.Stage.CacheSlotSizes.Length > 0 )
					{
						 _cacheAccess.CacheSlotSizes = _processor.Stage.CacheSlotSizes;
					}
					_cacheAccess.Forward = _processor.Stage.Forward;

					if ( _processor.Stage.Parallel )
					{
						 long highId;
						 if ( _processor.Stage == CheckStage.Stage1NSPropsLabels )
						 {
							  highId = _storeAccess.NodeStore.HighId;
						 }
						 else if ( _processor.Stage == CheckStage.Stage8PSProps )
						 {
							  highId = _storeAccess.PropertyStore.HighId;
						 }
						 else
						 {
							  highId = _storeAccess.NodeStore.HighId;
						 }
						 long recordsPerCPU = RecordDistributor.CalculateRecordsPerCpu( highId, numberOfThreads );
						 QueueDistribution_QueueDistributor<R> distributor = _distribution.distributor( recordsPerCPU, numberOfThreads );
						 _processor.applyFilteredParallel( _store, _progressListener, numberOfThreads, recordsPerCPU, distributor );
					}
					else
					{
						 _processor.applyFiltered( _store, _progressListener );
					}
					_cacheAccess.Forward = true;
			  }
			  catch ( Exception e )
			  {
					_progressListener.failed( e );
					throw new Exception( e );
			  }
			  finally
			  {
					AfterProcessing( _processor );
			  }
			  statistics.print( name );
		 }

		 protected internal virtual void BeforeProcessing( StoreProcessor processor )
		 {
			  // intentionally empty
		 }

		 protected internal virtual void AfterProcessing( StoreProcessor processor )
		 {
			  // intentionally empty
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "[" + name + " @ " + _processor.Stage + ", " + _store + ":" + _store.HighId + "]";
		 }
	}

}