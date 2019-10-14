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
namespace Neo4Net.@unsafe.Impl.Batchimport
{

	using InputChunk = Neo4Net.@unsafe.Impl.Batchimport.input.InputChunk;
	using InputEntityVisitor = Neo4Net.@unsafe.Impl.Batchimport.input.InputEntityVisitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;

	/// <summary>
	/// A utility to be able to write an <seealso cref="InputIterator"/> with low effort.
	/// Since <seealso cref="InputIterator"/> is multi-threaded in that multiple threads can call <seealso cref="newChunk()"/> and each
	/// call to <seealso cref="next(InputChunk)"/> handing out the next chunkstate instance from the supplied <seealso cref="System.Collections.IEnumerator"/>.
	/// </summary>
	/// @param <CHUNKSTATE> type of objects handed out from the supplied <seealso cref="System.Collections.IEnumerator"/>. </param>
	public class GeneratingInputIterator<CHUNKSTATE> : InputIterator
	{
		 private readonly System.Func<long, CHUNKSTATE> _states;
		 private readonly long _totalCount;
		 private readonly int _batchSize;
		 private readonly Generator<CHUNKSTATE> _generator;
		 private readonly long _startId;

		 private long _nextBatch;
		 private long _numberOfBatches;

		 public GeneratingInputIterator( long totalCount, int batchSize, System.Func<long, CHUNKSTATE> states, Generator<CHUNKSTATE> generator, long startId )
		 {
			  this._totalCount = totalCount;
			  this._batchSize = batchSize;
			  this._states = states;
			  this._generator = generator;
			  this._startId = startId;
			  this._numberOfBatches = batchSize == 0 ? 0 : ( totalCount - 1 ) / batchSize + 1;
		 }

		 public override void Close()
		 {
		 }

		 public override InputChunk NewChunk()
		 {
			  return new Chunk( this );
		 }

		 public override bool Next( InputChunk chunk )
		 {
			 lock ( this )
			 {
				  if ( _numberOfBatches > 1 )
				  {
						_numberOfBatches--;
						long batch = _nextBatch++;
						( ( Chunk ) chunk ).Initialize( _states.apply( batch ), batch, _batchSize );
						return true;
				  }
				  else if ( _numberOfBatches == 1 )
				  {
						_numberOfBatches--;
						int rest = toIntExact( _totalCount % _batchSize );
						int size = rest != 0 ? rest : _batchSize;
						long batch = _nextBatch++;
						( ( Chunk ) chunk ).Initialize( _states.apply( batch ), batch, size );
						return true;
				  }
				  return false;
			 }
		 }

		 private class Chunk : InputChunk
		 {
			 private readonly GeneratingInputIterator<CHUNKSTATE> _outerInstance;

			 public Chunk( GeneratingInputIterator<CHUNKSTATE> outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal CHUNKSTATE State;
			  internal int Count;
			  internal int ItemInBatch;
			  internal long BaseId;

			  public override void Close()
			  {
			  }

			  /// <param name="state"> CHUNKSTATE which is the source of data generation for this chunk. </param>
			  /// <param name="batch"> zero-based id (order) of this batch. </param>
			  internal virtual void Initialize( CHUNKSTATE state, long batch, int count )
			  {
					this.State = state;
					this.Count = count;
					this.BaseId = outerInstance.startId + batch * outerInstance.batchSize;
					this.ItemInBatch = 0;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next(org.neo4j.unsafe.impl.batchimport.input.InputEntityVisitor visitor) throws java.io.IOException
			  public override bool Next( InputEntityVisitor visitor )
			  {
					if ( ItemInBatch < Count )
					{
						 outerInstance.generator.Accept( State, visitor, BaseId + ItemInBatch );
						 visitor.EndOfEntity();
						 ItemInBatch++;
						 return true;
					}
					return false;
			  }
		 }

		 public static readonly InputIterator EMPTY = new GeneratingInputIteratorAnonymousInnerClass();

		 private class GeneratingInputIteratorAnonymousInnerClass : GeneratingInputIterator<Void>
		 {
			 public GeneratingInputIteratorAnonymousInnerClass() : base(0, 1, batch -> null, null, 0)
			 {
			 }

		 }

		 public static readonly InputIterable EmptyIterable = () => EMPTY;

		 public interface Generator<CHUNKSTATE>
		 {
			  void Accept( CHUNKSTATE state, InputEntityVisitor visitor, long id );
		 }
	}

}