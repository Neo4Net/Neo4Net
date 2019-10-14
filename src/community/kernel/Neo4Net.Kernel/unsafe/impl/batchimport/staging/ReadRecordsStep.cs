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
namespace Neo4Net.@unsafe.Impl.Batchimport.staging
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using Neo4Net.Kernel.impl.store;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;

	/// <summary>
	/// Reads records from a <seealso cref="RecordStore"/> and sends batches of those records downstream.
	/// A <seealso cref="PageCursor"/> is used during the life cycle of this <seealso cref="Step"/>, e.g. between
	/// <seealso cref="start(int)"/> and <seealso cref="close()"/>.
	/// </summary>
	/// @param <RECORD> type of <seealso cref="AbstractBaseRecord"/> </param>
	public class ReadRecordsStep<RECORD> : ProcessorStep<LongIterator> where RECORD : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
	{
		 private readonly RecordStore<RECORD> _store;
		 private readonly int _batchSize;
		 private readonly RecordDataAssembler<RECORD> _assembler;

		 public ReadRecordsStep( StageControl control, Configuration config, bool inRecordWritingStage, RecordStore<RECORD> store ) : this( control, config, inRecordWritingStage, store, new RecordDataAssembler<>( store.newRecord ) )
		 {
		 }

		 public ReadRecordsStep( StageControl control, Configuration config, bool inRecordWritingStage, RecordStore<RECORD> store, RecordDataAssembler<RECORD> converter ) : base( control, ">", config, ParallelReading( config, inRecordWritingStage ) ? 0 : 1 )
		 {
			  this._store = store;
			  this._assembler = converter;
			  this._batchSize = config.BatchSize();
		 }

		 private static bool ParallelReading( Configuration config, bool inRecordWritingStage )
		 {
			  return ( inRecordWritingStage && config.HighIO() ) || (!inRecordWritingStage && config.ParallelRecordReads());
		 }

		 public override void Start( int orderingGuarantees )
		 {
			  base.Start( orderingGuarantees | Step_Fields.ORDER_SEND_DOWNSTREAM );
		 }

		 protected internal override void Process( LongIterator idRange, BatchSender sender )
		 {
			  if ( !idRange.hasNext() )
			  {
					return;
			  }

			  long id = idRange.next();
			  RECORD[] batch = control.reuse( () => _assembler.newBatchObject(_batchSize) );
			  int i = 0;
			  // Just use the first record in the batch here to satisfy the record cursor.
			  // The truth is that we'll be using the read method which accepts an external record anyway so it doesn't matter.
			  using ( PageCursor cursor = _store.openPageCursorForReading( id ) )
			  {
					bool hasNext = true;
					while ( hasNext )
					{
						 if ( _assembler.append( _store, cursor, batch, id, i ) )
						 {
							  i++;
						 }
						 if ( hasNext = idRange.hasNext() )
						 {
							  id = idRange.next();
						 }
					}
			  }

			  sender.Send( _assembler.cutOffAt( batch, i ) );
		 }
	}

}