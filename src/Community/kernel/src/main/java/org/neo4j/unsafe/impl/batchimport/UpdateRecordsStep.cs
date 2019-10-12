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
namespace Neo4Net.@unsafe.Impl.Batchimport
{

	using Neo4Net.Kernel.impl.store;
	using IdSequence = Neo4Net.Kernel.impl.store.id.IdSequence;
	using IdValidator = Neo4Net.Kernel.impl.store.id.validation.IdValidator;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using BatchSender = Neo4Net.@unsafe.Impl.Batchimport.staging.BatchSender;
	using Neo4Net.@unsafe.Impl.Batchimport.staging;
	using StageControl = Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl;
	using Key = Neo4Net.@unsafe.Impl.Batchimport.stats.Key;
	using Keys = Neo4Net.@unsafe.Impl.Batchimport.stats.Keys;
	using Stat = Neo4Net.@unsafe.Impl.Batchimport.stats.Stat;
	using StatsProvider = Neo4Net.@unsafe.Impl.Batchimport.stats.StatsProvider;
	using PrepareIdSequence = Neo4Net.@unsafe.Impl.Batchimport.store.PrepareIdSequence;

	/// <summary>
	/// Updates a batch of records to a store.
	/// </summary>
	public class UpdateRecordsStep<RECORD> : ProcessorStep<RECORD[]>, StatsProvider where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
	{
		 protected internal readonly RecordStore<RECORD> Store;
		 private readonly int _recordSize;
		 private readonly PrepareIdSequence _prepareIdSequence;
		 private readonly LongAdder _recordsUpdated = new LongAdder();

		 public UpdateRecordsStep( StageControl control, Configuration config, RecordStore<RECORD> store, PrepareIdSequence prepareIdSequence ) : base( control, "v", config, config.ParallelRecordWrites() ? 0 : 1 )
		 {
			  this.Store = store;
			  this._prepareIdSequence = prepareIdSequence;
			  this._recordSize = store.RecordSize;
		 }

		 protected internal override void Process( RECORD[] batch, BatchSender sender )
		 {
			  System.Func<long, IdSequence> idSequence = _prepareIdSequence.apply( Store );
			  int recordsUpdatedInThisBatch = 0;
			  foreach ( RECORD record in batch )
			  {
					if ( record != null && record.inUse() && !IdValidator.isReservedId(record.Id) )
					{
						 Store.prepareForCommit( record, idSequence( record.Id ) );
						 Store.updateRecord( record );
						 recordsUpdatedInThisBatch++;
					}
			  }
			  _recordsUpdated.add( recordsUpdatedInThisBatch );
		 }

		 protected internal override void CollectStatsProviders( ICollection<StatsProvider> into )
		 {
			  base.CollectStatsProviders( into );
			  into.Add( this );
		 }

		 public override Stat Stat( Key key )
		 {
			  return key == Keys.io_throughput ? new IoThroughputStat( startTime, endTime, _recordSize * _recordsUpdated.sum() ) : null;
		 }

		 public override Key[] Keys()
		 {
			  return new Keys[] { Keys.io_throughput };
		 }
	}

}