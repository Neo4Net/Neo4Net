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

	/// <summary>
	/// Releases batches of record ids to be read, potentially in parallel, by downstream batches.
	/// </summary>
	public class BatchFeedStep : PullingProducerStep
	{
		 private readonly RecordIdIterator _ids;
		 private readonly int _recordSize;
		 private volatile long _count;

		 public BatchFeedStep( StageControl control, Configuration config, RecordIdIterator ids, int recordSize ) : base( control, config )
		 {
			  this._ids = ids;
			  this._recordSize = recordSize;
		 }

		 protected internal override object NextBatchOrNull( long ticket, int batchSize )
		 {
			  _count += batchSize;
			  return _ids.nextBatch();
		 }

		 protected internal override long Position()
		 {
			  return _count * _recordSize;
		 }
	}

}