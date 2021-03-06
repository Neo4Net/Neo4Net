﻿/*
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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;

	using PrimitiveLongCollections = Org.Neo4j.Collection.PrimitiveLongCollections;
	using Org.Neo4j.Kernel.impl.store;
	using IdRange = Org.Neo4j.Kernel.impl.store.id.IdRange;
	using IdRangeIterator = Org.Neo4j.Kernel.impl.store.id.IdRangeIterator;
	using IdSequence = Org.Neo4j.Kernel.impl.store.id.IdSequence;
	using IdValidator = Org.Neo4j.Kernel.impl.store.id.validation.IdValidator;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;

	/// <summary>
	/// Exposes batches of ids from a <seealso cref="RecordStore"/> as a <seealso cref="LongIterator"/>.
	/// It makes use of <seealso cref="IdSequence.nextIdBatch(int)"/> (with default batch size the number of records per page)
	/// and caches that batch, exhausting it in <seealso cref="next()"/> before getting next batch.
	/// </summary>
	public class BatchingIdGetter : PrimitiveLongCollections.PrimitiveLongBaseIterator, IdSequence
	{
		 private readonly IdSequence _source;
		 private IdRangeIterator _batch;
		 private readonly int _batchSize;

		 public BatchingIdGetter<T1>( RecordStore<T1> source ) where T1 : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord : this( source, source.RecordsPerPage )
		 {
		 }

		 public BatchingIdGetter<T1>( RecordStore<T1> source, int batchSize ) where T1 : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  this._source = source;
			  this._batchSize = batchSize;
		 }

		 protected internal override bool FetchNext()
		 {
			  return Next( NextId() );
		 }

		 public override long NextId()
		 {
			  long id;
			  if ( _batch == null || ( id = _batch.nextId() ) == -1 )
			  {
					IdRange idRange = _source.nextIdBatch( _batchSize );
					while ( IdValidator.hasReservedIdInRange( idRange.RangeStart, idRange.RangeStart + idRange.RangeLength ) )
					{
						 idRange = _source.nextIdBatch( _batchSize );
					}
					_batch = new IdRangeIterator( idRange );
					id = _batch.nextId();
			  }
			  return id;
		 }

		 public override IdRange NextIdBatch( int size )
		 {
			  throw new System.NotSupportedException();
		 }
	}

}