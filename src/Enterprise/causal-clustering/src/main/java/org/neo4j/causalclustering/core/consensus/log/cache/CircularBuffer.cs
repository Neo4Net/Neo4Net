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
//	import static Math.floorMod;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.fill;

	/// <summary>
	/// <pre>
	/// Design
	/// 
	/// S: start index
	/// E: end index
	/// 
	/// When S == E the buffer is empty.
	/// 
	/// Examples:
	/// 
	///              S
	///              v
	/// Empty   [ | | | | | | ]
	///              ^
	///              E
	/// 
	/// 
	///                S
	///                v
	/// Size 2  [ | | | | | | ]
	///                    ^
	///                    E
	/// 
	/// 
	///                 S
	///                 v
	/// Full   [ | | | | | | ]
	///               ^
	///               E
	/// 
	/// New items are put at the current E, and then E is moved one step forward (circularly).
	/// The item at E is never a valid item.
	/// 
	/// If moving E one step forward moves it onto S
	/// - then it knocks an element out
	/// - and S is also moved one step forward
	/// 
	/// The S element has index 0.
	/// Removing an element moves S forward (circularly).
	/// </summary>
	/// @param <V> type of elements. </param>
	public class CircularBuffer<V>
	{
		 private readonly int _arraySize; // externally visible capacity is arraySize - 1
		 private object[] _elementArr;

		 private int _s;
		 private int _e;

		 internal CircularBuffer( int capacity )
		 {
			  if ( capacity <= 0 )
			  {
					throw new System.ArgumentException( "Capacity must be > 0." );
			  }

			  this._arraySize = capacity + 1; // 1 item as sentinel (can't hold entries)
			  this._elementArr = new object[_arraySize];
		 }

		 /// <summary>
		 /// Clears the underlying buffer and fills the provided eviction array with all evicted elements.
		 /// The provided array must have the same capacity as the circular buffer.
		 /// </summary>
		 /// <param name="evictions"> Caller-provided array for evictions. </param>
		 public virtual void Clear( V[] evictions )
		 {
			  if ( evictions.Length != _arraySize - 1 )
			  {
					throw new System.ArgumentException( "The eviction array must be of the same size as the capacity of the circular buffer." );
			  }

			  int i = 0;
			  while ( _s != _e )
			  {
					//noinspection unchecked
					evictions[i++] = ( V ) _elementArr[_s];
					_s = Pos( _s, 1 );
			  }

			  _s = 0;
			  _e = 0;

			  fill( _elementArr, null );
		 }

		 private int Pos( int @base, int delta )
		 {
			  return floorMod( @base + delta, _arraySize );
		 }

		 /// <summary>
		 /// Append to the end of the buffer, possibly overwriting the
		 /// oldest entry.
		 /// </summary>
		 /// <returns> any knocked out item, or null if nothing was knocked out. </returns>
		 public virtual V Append( V e )
		 {
			  _elementArr[_e] = e;
			  _e = Pos( _e, 1 );
			  if ( _e == _s )
			  {
					//noinspection unchecked
					V old = ( V ) _elementArr[_e];
					_elementArr[_e] = null;
					_s = Pos( _s, 1 );
					return old;
			  }
			  else
			  {
					return default( V );
			  }
		 }

		 public virtual V Read( int idx )
		 {
			  //noinspection unchecked
			  return ( V ) _elementArr[Pos( _s, idx )];
		 }

		 public virtual V Remove()
		 {
			  if ( _s == _e )
			  {
					return default( V );
			  }
			  //noinspection unchecked
			  V e = ( V ) _elementArr[_s];
			  _elementArr[_s] = null;
			  _s = Pos( _s, 1 );
			  return e;
		 }

		 public virtual V RemoveHead()
		 {
			  if ( _s == _e )
			  {
					return default( V );
			  }

			  _e = Pos( _e, -1 );
			  //noinspection unchecked
			  V e = ( V ) _elementArr[_e];
			  _elementArr[_e] = null;
			  return e;
		 }

		 public virtual int Size()
		 {
			  return floorMod( _e - _s, _arraySize );
		 }
	}

}