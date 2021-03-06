﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.store.allocator
{


	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;

	/// <summary>
	/// Composite allocator that allows to use available records first and then switch to provided record allocator to
	/// allocate more records if required.
	/// </summary>
	public class ReusableRecordsCompositeAllocator : DynamicRecordAllocator
	{
		 private readonly ReusableRecordsAllocator _reusableRecordsAllocator;
		 private readonly DynamicRecordAllocator _recordAllocator;

		 public ReusableRecordsCompositeAllocator( ICollection<DynamicRecord> records, DynamicRecordAllocator recordAllocator ) : this( records.GetEnumerator(), recordAllocator )
		 {
		 }

		 public ReusableRecordsCompositeAllocator( IEnumerator<DynamicRecord> recordsIterator, DynamicRecordAllocator recordAllocator )
		 {
			  this._reusableRecordsAllocator = new ReusableRecordsAllocator( recordAllocator.RecordDataSize, recordsIterator );
			  this._recordAllocator = recordAllocator;
		 }

		 public virtual int RecordDataSize
		 {
			 get
			 {
				  return _recordAllocator.RecordDataSize;
			 }
		 }

		 public override DynamicRecord NextRecord()
		 {
			  return _reusableRecordsAllocator.hasNext() ? _reusableRecordsAllocator.nextRecord() : _recordAllocator.nextRecord();
		 }
	}

}