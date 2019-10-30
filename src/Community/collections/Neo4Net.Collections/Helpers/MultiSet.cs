using System.Collections.Generic;

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
namespace Neo4Net.Collections.Helpers
{

	public class MultiSet<T>
	{
		 private readonly IDictionary<T, long> _inner;
		 private long _size;

		 public MultiSet()
		 {
			  _inner = new Dictionary<T, long>();
		 }

		 public MultiSet( int initialCapacity )
		 {
			  _inner = new Dictionary<T, long>( initialCapacity );
		 }

		 public virtual bool Contains( T value )
		 {
			  return _inner.ContainsKey( value );
		 }

		 public virtual long Count( T value )
		 {
			  return Unbox( _inner[value] );
		 }

		 public virtual long Add( T value )
		 {
			  return Increment( value, +1 );
		 }

		 public virtual long Remove( T value )
		 {
			 return Increment( value, -1 );
		 }

		 public virtual long Replace( T value, long newCount )
		 {
			  if ( newCount <= 0 )
			  {
					long previous = Unbox( _inner.Remove( value ) );
					_size -= previous;
					return previous;
			  }
			  else
			  {
					long previous = Unbox( _inner.put( value, newCount ) );
					_size += newCount - previous;
					return previous;
			  }
		 }

		 public virtual long Increment( T value, long amount )
		 {
			  long previous = Count( value );
			  if ( amount == 0 )
			  {
					return previous;
			  }

			  long newCount = previous + amount;
			  if ( newCount <= 0 )
			  {
					_inner.Remove( value );
					_size -= previous;
					return 0;
			  }
			  else
			  {
					_inner[value] = newCount;
					_size += amount;
					return newCount;
			  }
		 }

		 public virtual bool Empty
		 {
			 get
			 {
				  return _inner.Count == 0;
			 }
		 }

		 public virtual long Size()
		 {
			  return _size;
		 }

		 public virtual int UniqueSize()
		 {
			  return _inner.Count;
		 }

		 public virtual void Clear()
		 {
			  _inner.Clear();
			  _size = 0;
		 }

		 public override bool Equals( object other )
		 {
			  return ( this == other ) || ( !( other == null || this.GetType() != other.GetType() ) && _inner.Equals(((MultiSet) other)._inner) );

		 }

		 public override int GetHashCode()
		 {
			  return _inner.GetHashCode();
		 }

		 private long Unbox( long? value )
		 {
			  return value == null ? 0 : value.Value;
		 }

		 public virtual ISet<KeyValuePair<T, long>> EntrySet()
		 {
			  return _inner.SetOfKeyValuePairs();
		 }
	}

}