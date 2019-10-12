using System.Diagnostics;

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
namespace Neo4Net.causalclustering.core.consensus.log.cache
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;

	/// <summary>
	/// Keeps elements over a limited consecutive range cached.
	/// </summary>
	/// @param <V> The type of element to cache. </param>
	internal class ConsecutiveCache<V>
	{
		 private readonly CircularBuffer<V> _circle;
		 private long _endIndex = -1;

		 internal ConsecutiveCache( int capacity )
		 {
			  this._circle = new CircularBuffer<V>( capacity );
		 }

		 private long FirstIndex()
		 {
			  return _endIndex - _circle.size() + 1;
		 }

		 internal virtual void Put( long idx, V e, V[] evictions )
		 {
			  if ( idx < 0 )
			  {
					throw new System.ArgumentException( format( "Index must be >= 0 (was %d)", idx ) );
			  }
			  if ( e == default( V ) )
			  {
					throw new System.ArgumentException( "Null entries are not accepted" );
			  }

			  if ( idx == _endIndex + 1 )
			  {
					evictions[0] = _circle.append( e );
					_endIndex = _endIndex + 1;
			  }
			  else
			  {
					_circle.clear( evictions );
					_circle.append( e );
					_endIndex = idx;
			  }
		 }

		 internal virtual V Get( long idx )
		 {
			  if ( idx < 0 )
			  {
					throw new System.ArgumentException( format( "Index must be >= 0 (was %d)", idx ) );
			  }

			  if ( idx > _endIndex || idx < FirstIndex() )
			  {
					return default( V );
			  }

			  return _circle.read( toIntExact( idx - FirstIndex() ) );
		 }

		 public virtual void Clear( V[] evictions )
		 {
			  _circle.clear( evictions );
		 }

		 public virtual int Size()
		 {
			  return _circle.size();
		 }

		 public virtual void Prune( long upToIndex, V[] evictions )
		 {
			  long index = FirstIndex();
			  int i = 0;
			  while ( index <= min( upToIndex, _endIndex ) )
			  {
					evictions[i] = _circle.remove();
					Debug.Assert( evictions[i] != default( V ) );
					i++;
					index++;
			  }
		 }

		 public virtual V Remove()
		 {
			  return _circle.remove();
		 }

		 public virtual void Truncate( long fromIndex, V[] evictions )
		 {
			  if ( fromIndex > _endIndex )
			  {
					return;
			  }
			  long index = max( fromIndex, FirstIndex() );
			  int i = 0;
			  while ( index <= _endIndex )
			  {
					evictions[i++] = _circle.removeHead();
					index++;
			  }
			  _endIndex = fromIndex - 1;
		 }
	}

}