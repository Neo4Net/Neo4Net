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
namespace Org.Neo4j.Kernel.impl.store.allocator
{

	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;

	/// <summary>
	/// Dynamic allocator that allow to use already available records in dynamic records allocation.
	/// As part of record allocation provided records will be marked as created if they where not used before
	/// and marked as used.
	/// </summary>
	public class ReusableRecordsAllocator : DynamicRecordAllocator
	{
		 private readonly int _recordSize;
		 private readonly IEnumerator<DynamicRecord> _recordIterator;

		 public ReusableRecordsAllocator( int recordSize, params DynamicRecord[] records )
		 {
			  this._recordSize = recordSize;
			  this._recordIterator = Iterators.iterator( records );
		 }

		 public ReusableRecordsAllocator( int recordSize, ICollection<DynamicRecord> records )
		 {
			  this._recordSize = recordSize;
			  this._recordIterator = records.GetEnumerator();
		 }

		 public ReusableRecordsAllocator( int recordSize, IEnumerator<DynamicRecord> recordsIterator )
		 {
			  this._recordSize = recordSize;
			  this._recordIterator = recordsIterator;
		 }

		 public virtual int RecordDataSize
		 {
			 get
			 {
				  return _recordSize;
			 }
		 }

		 public override DynamicRecord NextRecord()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  DynamicRecord record = _recordIterator.next();
			  if ( !record.InUse() )
			  {
					record.SetCreated();
			  }
			  record.InUse = true;
			  return record;
		 }

		 /// <summary>
		 /// Check if we have more available pre allocated records </summary>
		 /// <returns> true if record is available </returns>
		 public virtual bool HasNext()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return _recordIterator.hasNext();
		 }
	}

}