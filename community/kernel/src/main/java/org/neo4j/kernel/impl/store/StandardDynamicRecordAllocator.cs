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
namespace Org.Neo4j.Kernel.impl.store
{
	using IdSequence = Org.Neo4j.Kernel.impl.store.id.IdSequence;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;

	public class StandardDynamicRecordAllocator : DynamicRecordAllocator
	{
		 protected internal readonly IdSequence IdGenerator;
		 private readonly int _dataSize;

		 public StandardDynamicRecordAllocator( IdSequence idGenerator, int dataSize )
		 {
			  this.IdGenerator = idGenerator;
			  this._dataSize = dataSize;
		 }

		 public virtual int RecordDataSize
		 {
			 get
			 {
				  return _dataSize;
			 }
		 }

		 public override DynamicRecord NextRecord()
		 {
			  return AllocateRecord( IdGenerator.nextId() );
		 }

		 public static DynamicRecord AllocateRecord( long id )
		 {
			  DynamicRecord record = new DynamicRecord( id );
			  record.SetCreated();
			  record.InUse = true;
			  return record;
		 }
	}

}