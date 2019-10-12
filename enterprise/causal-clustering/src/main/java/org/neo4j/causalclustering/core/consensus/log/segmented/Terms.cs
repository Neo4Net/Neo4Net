using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.causalclustering.core.consensus.log.segmented
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;

	/// <summary>
	/// Keeps track of all the terms in memory for efficient lookup.
	/// The implementation favours lookup of recent entries.
	/// 
	/// Exposed methods shadow the regular RAFT log manipulation
	/// functions and must be invoked from respective places.
	/// 
	/// During recovery truncate should be called between every segment
	/// switch to "simulate" eventual truncations as a reason for switching
	/// segments. It is ok to call truncate even if the reason was not a
	/// truncation.
	/// </summary>
	public class Terms
	{
		 private int _size;

		 private long[] _indexes;
		 private long[] _terms;

		 private long _min; // inclusive
		 private long _max; // inclusive

		 internal Terms( long prevIndex, long prevTerm )
		 {
			  Skip( prevIndex, prevTerm );
		 }

		 internal virtual void Append( long index, long term )
		 {
			 lock ( this )
			 {
				  if ( index != _max + 1 )
				  {
						throw new System.InvalidOperationException( format( "Must append in order. %s but expected index is %d", AppendMessage( index, term ), _max + 1 ) );
				  }
				  else if ( _size > 0 && term < _terms[_size - 1] )
				  {
						throw new System.InvalidOperationException( format( "Non-monotonic term. %s but highest term is %d", AppendMessage( index, term ), _terms[_size - 1] ) );
				  }
      
				  _max = index;
      
				  if ( _size == 0 || term != _terms[_size - 1] )
				  {
						Size = _size + 1;
						_indexes[_size - 1] = index;
						_terms[_size - 1] = term;
				  }
			 }
		 }

		 private string AppendMessage( long index, long term )
		 {
			  return format( "Tried to append [index: %d, term: %d]", index, term );
		 }

		 private int Size
		 {
			 set
			 {
				  if ( value != _size )
				  {
						_size = value;
						_indexes = Arrays.copyOf( _indexes, _size );
						_terms = Arrays.copyOf( _terms, _size );
				  }
			 }
		 }

		 /// <summary>
		 /// Truncate from the specified index.
		 /// </summary>
		 /// <param name="fromIndex"> The index to truncate from (inclusive). </param>
		 internal virtual void Truncate( long fromIndex )
		 {
			 lock ( this )
			 {
				  if ( fromIndex < 0 || fromIndex < _min )
				  {
						throw new System.InvalidOperationException( "Cannot truncate a negative index. Tried to truncate from " + fromIndex );
				  }
      
				  _max = fromIndex - 1;
      
				  int newSize = _size;
				  while ( newSize > 0 && _indexes[newSize - 1] >= fromIndex )
				  {
						newSize--;
				  }
      
				  Size = newSize;
			 }
		 }

		 /// <summary>
		 /// Prune up to specified index.
		 /// </summary>
		 /// <param name="upToIndex"> The last index to prune (exclusive). </param>
		 internal virtual void Prune( long upToIndex )
		 {
			 lock ( this )
			 {
				  _min = max( upToIndex, _min );
      
				  int lastToPrune = FindRangeContaining( _min ) - 1; // we can prune the ranges preceding
      
				  if ( lastToPrune < 0 )
				  {
						return;
				  }
      
				  _size = ( _indexes.Length - 1 ) - lastToPrune;
				  _indexes = Arrays.copyOfRange( _indexes, lastToPrune + 1, _indexes.Length );
				  _terms = Arrays.copyOfRange( _terms, lastToPrune + 1, _terms.Length );
			 }
		 }

		 private int FindRangeContaining( long index )
		 {
			  for ( int i = 0; i < _indexes.Length; i++ )
			  {
					if ( _indexes[i] > index )
					{
						 return i - 1;
					}
					else if ( _indexes[i] == index )
					{
						 return i;
					}
			  }

			  return index > _indexes[_indexes.Length - 1] ? _indexes.Length - 1 : -1;
		 }

		 internal virtual void Skip( long prevIndex, long prevTerm )
		 {
			 lock ( this )
			 {
				  _min = _max = prevIndex;
				  _size = 1;
				  _indexes = new long[_size];
				  _terms = new long[_size];
				  _indexes[0] = prevIndex;
				  _terms[0] = prevTerm;
			 }
		 }

		 internal virtual long Get( long logIndex )
		 {
			 lock ( this )
			 {
				  if ( logIndex == -1 || logIndex < _min || logIndex > _max )
				  {
						return -1;
				  }
      
				  for ( int i = _size - 1; i >= 0; i-- )
				  {
						if ( logIndex >= _indexes[i] )
						{
							 return _terms[i];
						}
				  }
      
				  throw new Exception( "Should be possible to find index >= min" );
			 }
		 }

		 internal virtual long Latest()
		 {
			 lock ( this )
			 {
				  return _size == 0 ? -1 : _terms[_size - 1];
			 }
		 }
	}

}