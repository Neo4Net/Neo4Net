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
namespace Neo4Net.Kernel.impl.store.id
{
	using IdValidator = Neo4Net.Kernel.impl.store.id.validation.IdValidator;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;

	/// <summary>
	/// <seealso cref="IdSequence"/> w/o any synchronization, purely a long incrementing.
	/// </summary>
	public class BatchingIdSequence : IdSequence
	{
		 private readonly long _startId;
		 private long _nextId;

		 public BatchingIdSequence() : this(0)
		 {
		 }

		 public BatchingIdSequence( long startId )
		 {
			  this._startId = startId;
			  this._nextId = startId;
		 }

		 public override long NextId()
		 {
			  long result = Peek();
			  _nextId++;
			  return result;
		 }

		 public override IdRange NextIdBatch( int size )
		 {
			  while ( IdValidator.hasReservedIdInRange( _nextId, _nextId + size ) )
			  {
					_nextId += size;
			  }

			  long startId = _nextId;
			  _nextId += size;
			  return new IdRange( EMPTY_LONG_ARRAY, startId, size );
		 }

		 public virtual void Reset()
		 {
			  _nextId = _startId;
		 }

		 public virtual void Set( long nextId )
		 {
			  this._nextId = nextId;
		 }

		 public virtual long Peek()
		 {
			  if ( IdValidator.isReservedId( _nextId ) )
			  {
					_nextId++;
			  }
			  return _nextId;
		 }
	}

}