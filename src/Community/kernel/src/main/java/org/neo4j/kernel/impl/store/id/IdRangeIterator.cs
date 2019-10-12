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
namespace Neo4Net.Kernel.impl.store.id
{

	using IdValidator = Neo4Net.Kernel.impl.store.id.validation.IdValidator;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;

	public class IdRangeIterator : IdSequence
	{
		 public static readonly IdRangeIterator EMPTY_ID_RANGE_ITERATOR = new IdRangeIteratorAnonymousInnerClass();

		 private class IdRangeIteratorAnonymousInnerClass : IdRangeIterator
		 {
			 public IdRangeIteratorAnonymousInnerClass() : base(new IdRange(EMPTY_LONG_ARRAY, 0, 0))
			 {
			 }

			 public override long nextId()
			 {
				  return VALUE_REPRESENTING_NULL;
			 }
		 }

		 public const long VALUE_REPRESENTING_NULL = -1;
		 private int _position;
		 private readonly long[] _defrag;
		 private readonly long _start;
		 private readonly int _length;

		 public IdRangeIterator( IdRange idRange )
		 {
			  this._defrag = idRange.DefragIds;
			  this._start = idRange.RangeStart;
			  this._length = idRange.RangeLength;
		 }

		 public override long NextId()
		 {
			  try
			  {
					if ( _position < _defrag.Length )
					{
						 return _defrag[_position];
					}

					long candidate = NextRangeCandidate();
					if ( IdValidator.isReservedId( candidate ) )
					{
						 _position++;
						 candidate = NextRangeCandidate();
					}
					return candidate;
			  }
			  finally
			  {
					++_position;
			  }
		 }

		 public override IdRange NextIdBatch( int size )
		 {
			  int sizeLeft = size;
			  long[] rangeDefrag = EMPTY_LONG_ARRAY;
			  if ( _position < _defrag.Length )
			  {
					// There are defragged ids to grab
					int numberOfDefrags = min( sizeLeft, _defrag.Length - _position );
					rangeDefrag = Arrays.copyOfRange( _defrag, _position, numberOfDefrags + _position );
					_position += numberOfDefrags;
					sizeLeft -= numberOfDefrags;
			  }

			  long rangeStart = 0;
			  int rangeLength = 0;
			  int rangeOffset = CurrentRangeOffset();
			  int rangeAvailable = _length - rangeOffset;
			  if ( sizeLeft > 0 && rangeAvailable > 0 )
			  {
					rangeStart = _start + rangeOffset;
					rangeLength = min( rangeAvailable, sizeLeft );
					_position += rangeLength;
			  }
			  return new IdRange( rangeDefrag, rangeStart, rangeLength );
		 }

		 private long NextRangeCandidate()
		 {
			  int offset = CurrentRangeOffset();
			  return ( offset < _length ) ? ( _start + offset ) : VALUE_REPRESENTING_NULL;
		 }

		 private int CurrentRangeOffset()
		 {
			  return _position - _defrag.Length;
		 }

		 public override string ToString()
		 {
			  return "IdRangeIterator[start:" + _start + ", length:" + _length + ", position:" + _position + ", defrag:" + Arrays.ToString( _defrag ) + "]";
		 }
	}

}