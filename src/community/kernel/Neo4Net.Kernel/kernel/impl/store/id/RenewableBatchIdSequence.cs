using System.Diagnostics;

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

	using Resource = Neo4Net.GraphDb.Resource;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.id.IdRangeIterator.VALUE_REPRESENTING_NULL;

	/// <summary>
	/// An <seealso cref="IdSequence"/> which does internal batching by using another <seealso cref="IdSequence"/> as source of batches.
	/// Meant to be used by a single thread at a time.
	/// </summary>
	public class RenewableBatchIdSequence : IdSequence, Resource
	{
		 private readonly IdSequence _source;
		 private readonly int _batchSize;
		 private readonly System.Action<long> _excessIdConsumer;
		 private IdSequence _currentBatch;
		 private bool _closed;

		 internal RenewableBatchIdSequence( IdSequence source, int batchSize, System.Action<long> excessIdConsumer )
		 {
			  this._source = source;
			  this._batchSize = batchSize;
			  this._excessIdConsumer = excessIdConsumer;
		 }

		 /// <summary>
		 /// Even if instances are meant to be accessed by a single thread at a time, lifecycle calls
		 /// can guard for it nonetheless. Only the first call to close will perform close.
		 /// </summary>
		 public override void Close()
		 {
			 lock ( this )
			 {
				  if ( !_closed && _currentBatch != null )
				  {
						long id;
						while ( ( id = _currentBatch.nextId() ) != VALUE_REPRESENTING_NULL )
						{
							 _excessIdConsumer.accept( id );
						}
						_currentBatch = null;
				  }
				  _closed = true;
			 }
		 }

		 public override long NextId()
		 {
			  Debug.Assert( !_closed );

			  long id;
			  while ( _currentBatch == null || ( id = _currentBatch.nextId() ) == VALUE_REPRESENTING_NULL )
			  {
					_currentBatch = _source.nextIdBatch( _batchSize ).GetEnumerator();
			  }
			  return id;
		 }

		 public override IdRange NextIdBatch( int size )
		 {
			  throw new System.NotSupportedException( "Haven't been needed so far" );
		 }
	}

}