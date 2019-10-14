using System.Collections.Concurrent;
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
namespace Neo4Net.Kernel.impl.cache
{

	public class ClockCache<K, V>
	{
		 private readonly LinkedList<Page<V>> _clock = new ConcurrentLinkedQueue<Page<V>>();
		 private readonly IDictionary<K, Page<V>> _cache = new ConcurrentDictionary<K, Page<V>>();
		 private readonly int _maxSize;
		 private readonly AtomicInteger _currentSize = new AtomicInteger( 0 );
		 private readonly string _name;

		 public ClockCache( string name, int size )
		 {
			  if ( string.ReferenceEquals( name, null ) )
			  {
					throw new System.ArgumentException( "name cannot be null" );
			  }
			  if ( size <= 0 )
			  {
					throw new System.ArgumentException( size + " is not > 0" );
			  }
			  this._name = name;
			  this._maxSize = size;
		 }

		 public virtual void Put( K key, V value )
		 {
			  if ( key == default( K ) )
			  {
					throw new System.ArgumentException( "null key not allowed" );
			  }
			  if ( value == default( V ) )
			  {
					throw new System.ArgumentException( "null value not allowed" );
			  }
			  Page<V> theValue = _cache[key];
			  if ( theValue == null )
			  {
					theValue = new Page<V>();
					_cache[key] = theValue;
					_clock.AddLast( theValue );
			  }
			  if ( theValue.Value == default( V ) )
			  {
					_currentSize.incrementAndGet();
			  }
			  theValue.Flag = true;
			  theValue.Value = value;
			  CheckSize();
		 }

		 public virtual V Get( K key )
		 {
			  if ( key == default( K ) )
			  {
					throw new System.ArgumentException( "cannot get null key" );
			  }
			  Page<V> theElement = _cache[key];
			  if ( theElement == null || theElement.Value == default( V ) )
			  {
					return default( V );
			  }
			  theElement.Flag = true;
			  return theElement.Value;
		 }

		 private void CheckSize()
		 {
			  while ( _currentSize.get() > _maxSize )
			  {
					Evict();
			  }
		 }

		 private void Evict()
		 {
			  Page<V> theElement = null;
			  while ( ( theElement = _clock.RemoveFirst() ) != null )
			  {
					try
					{
						 if ( theElement.Flag )
						 {
							  theElement.Flag = false;
						 }
						 else
						 {
							  V valueCleaned = theElement.Value;
							  ElementCleaned( valueCleaned );
							  theElement.Value = default( V );
							  _currentSize.decrementAndGet();
							  return;
						 }
					}
					finally
					{
						 _clock.AddLast( theElement );
					}
			  }
		 }

		 protected internal virtual void ElementCleaned( V element )
		 {
			  // to be overridden as required
		 }

		 public virtual ICollection<V> Values()
		 {
			  ISet<V> toReturn = new HashSet<V>();
			  foreach ( Page<V> page in _cache.Values )
			  {
					if ( page.Value != default( V ) )
					{
						 toReturn.Add( page.Value );
					}
			  }
			  return toReturn;
		 }

		 public virtual V Remove( K key )
		 {
			  if ( key == default( K ) )
			  {
					throw new System.ArgumentException( "cannot remove null key" );
			  }
			  Page<V> toRemove = _cache.Remove( key );
			  if ( toRemove == null || toRemove.Value == default( V ) )
			  {
					return default( V );
			  }
			  _currentSize.decrementAndGet();
			  V toReturn = toRemove.Value;
			  toRemove.Value = default( V );
			  toRemove.Flag = false;
			  return toReturn;
		 }

		 public virtual string Name
		 {
			 get
			 {
				  return _name;
			 }
		 }

		 public virtual void Clear()
		 {
			  _cache.Clear();
			  _clock.Clear();
			  _currentSize.set( 0 );
		 }

		 public virtual int Size()
		 {
			  return _currentSize.get();
		 }

		 private class Page<E>
		 {
			  internal volatile bool Flag = true;
			  internal volatile E Value;

			  public override bool Equals( object obj )
			  {
					if ( obj == null )
					{
						 return false;
					}
					if ( !( obj is Page ) )
					{
						 return false;
					}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Page<?> other = (Page) obj;
					Page<object> other = ( Page ) obj;
					if ( Value == default( E ) )
					{
						 return other.Value == null;
					}
					return Value.Equals( other.Value );
			  }

			  public override int GetHashCode()
			  {
					return Value == default( E ) ? 0 : Value.GetHashCode();
			  }
		 }
	}

}