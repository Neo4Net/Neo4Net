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
namespace Neo4Net.Kernel.impl.storemigration.participant
{
	using Neo4Net.Kernel.impl.store;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using InputIterator = Neo4Net.@unsafe.Impl.Batchimport.InputIterator;
	using InputChunk = Neo4Net.@unsafe.Impl.Batchimport.input.InputChunk;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.min;

	/// <summary>
	/// An <seealso cref="InputIterator"/> backed by a <seealso cref="RecordStore"/>, iterating over all used records.
	/// </summary>
	/// @param <RECORD> type of <seealso cref="AbstractBaseRecord"/> </param>
	internal abstract class StoreScanAsInputIterator<RECORD> : InputIterator where RECORD : Neo4Net.Kernel.impl.store.record.AbstractBaseRecord
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