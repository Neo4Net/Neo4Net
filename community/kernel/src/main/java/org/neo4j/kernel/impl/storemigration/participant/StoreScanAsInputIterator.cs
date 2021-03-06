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
namespace Org.Neo4j.Kernel.impl.storemigration.participant
{
	using Org.Neo4j.Kernel.impl.store;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using InputIterator = Org.Neo4j.@unsafe.Impl.Batchimport.InputIterator;
	using InputChunk = Org.Neo4j.@unsafe.Impl.Batchimport.input.InputChunk;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.min;

	/// <summary>
	/// An <seealso cref="InputIterator"/> backed by a <seealso cref="RecordStore"/>, iterating over all used records.
	/// </summary>
	/// @param <RECORD> type of <seealso cref="AbstractBaseRecord"/> </param>
	internal abstract class StoreScanAsInputIterator<RECORD> : InputIterator where RECORD : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
	{
		public abstract InputChunk NewChunk();
		 private readonly int _batchSize;
		 private readonly long _highId;
		 private long _id;

		 internal StoreScanAsInputIterator( RecordStore<RECORD> store )
		 {
			  this._batchSize = store.RecordsPerPage * 10;
			  this._highId = store.HighId;
		 }

		 public override void Close()
		 {
		 }

		 public override bool Next( InputChunk chunk )
		 {
			 lock ( this )
			 {
				  if ( _id >= _highId )
				  {
						return false;
				  }
				  long startId = _id;
				  _id = min( _highId, startId + _batchSize );
				  ( ( StoreScanChunk )chunk ).initialize( startId, _id );
				  return true;
			 }
		 }
	}

}