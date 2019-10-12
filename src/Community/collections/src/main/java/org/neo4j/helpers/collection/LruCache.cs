using System.Collections.Generic;

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
namespace Neo4Net.Helpers.Collection
{

	/// <summary>
	/// Simple implementation of Least-recently-used cache.
	/// <para>
	/// The cache has a <CODE>maxSize</CODE> set and when the number of cached
	/// elements exceeds that limit the least recently used element will be removed.
	/// </para>
	/// </summary>
	public class LruCache<K, E>
	{
		 private readonly string _name;
		 private int _maxSize;

		 private readonly IDictionary<K, E> cache = new LinkedHashMapAnonymousInnerClass();

		 private class LinkedHashMapAnonymousInnerClass : LinkedHashMap<K, E>
		 {
			 public LinkedHashMapAnonymousInnerClass() : base(500, 0.75f, true)
			 {
			 }

			 protected internal override bool removeEldestEntry( KeyValuePair<K, E> eldest )
			 {
				  // synchronization miss with old value on maxSize here is ok
				  if ( outerInstance.size() > outerInstance.maxSize )
				  {
						outerInstance.elementCleaned( eldest.Value );
						return true;
				  }
				  return false;
			 }
		 }

		 /// <summary>
		 /// Creates a LRU cache. If {@code maxSize < 1} an
		 /// IllegalArgumentException is thrown.
		 /// </summary>
		 /// <param name="name">    name of cache </param>
		 /// <param name="maxSize"> maximum size of this cache </param>
		 public LruCache( string name, int maxSize )
		 {
			  if ( string.ReferenceEquals( name, null ) || maxSize < 1 )
			  {
					throw new System.ArgumentException( "maxSize=" + maxSize + ", name=" + name );
			  }
			  this._name = name;
			  this._maxSize = maxSize;
		 }

		 public virtual string Name
		 {
			 get
			 {
				  return this._name;
			 }
		 }

		 /// <summary>
		 /// Returns the maximum size of this cache.
		 /// </summary>
		 /// <returns> maximum size </returns>
		 public virtual int MaxSize()
		 {
			  return _maxSize;
		 }

		 public virtual void Put( K key, E element )
		 {
			 lock ( this )
			 {
				  if ( key == default( K ) || element == default( E ) )
				  {
						throw new System.ArgumentException( "key=" + key + ", element=" + element );
				  }
				  cache.put( key, element );
			 }
		 }

		 public virtual E Remove( K key )
		 {
			 lock ( this )
			 {
				  if ( key == default( K ) )
				  {
						throw new System.ArgumentException( "Null parameter" );
				  }
				  return cache.remove( key );
			 }
		 }

		 public virtual E Get( K key )
		 {
			 lock ( this )
			 {
				  if ( key == default( K ) )
				  {
						throw new System.ArgumentException();
				  }
				  return cache.get( key );
			 }
		 }

		 public virtual void Clear()
		 {
			 lock ( this )
			 {
				  foreach ( KeyValuePair<K, E> keEntry in cache.entrySet() )
				  {
						ElementCleaned( keEntry.Value );
				  }
				  cache.clear();
			 }
		 }

		 public virtual int Size()
		 {
			 lock ( this )
			 {
				  return cache.size();
			 }
		 }

		 public virtual ISet<K> KeySet()
		 {
			 lock ( this )
			 {
				  return cache.Keys;
			 }
		 }

		 /// <summary>
		 /// Changes the max size of the cache. If <CODE>newMaxSize</CODE> is
		 /// greater then <CODE>maxSize()</CODE> next invoke to <CODE>maxSize()</CODE>
		 /// will return <CODE>newMaxSize</CODE> and the entries in cache will not
		 /// be modified.
		 /// <para>
		 /// If <CODE>newMaxSize</CODE> is less then <CODE>size()</CODE>
		 /// the cache will shrink itself removing least recently used element until
		 /// <CODE>size()</CODE> equals <CODE>newMaxSize</CODE>. For each element
		 /// removed the <seealso cref="elementCleaned"/> method is invoked.
		 /// </para>
		 /// <para>
		 /// If <CODE>newMaxSize</CODE> is less then <CODE>1</CODE> an
		 /// <seealso cref="System.ArgumentException"/> is thrown.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="newMaxSize"> the new maximum size of the cache </param>
		 public virtual void Resize( int newMaxSize )
		 {
			 lock ( this )
			 {
				  if ( newMaxSize < 1 )
				  {
						throw new System.ArgumentException( "newMaxSize=" + newMaxSize );
				  }
      
				  if ( newMaxSize >= Size() )
				  {
						_maxSize = newMaxSize;
				  }
				  else
				  {
						_maxSize = newMaxSize;
						IEnumerator<KeyValuePair<K, E>> itr = cache.entrySet().GetEnumerator();
						while ( itr.MoveNext() && cache.size() > _maxSize )
						{
							 E element = itr.Current.Value;
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
							 itr.remove();
							 ElementCleaned( element );
						}
				  }
			 }
		 }

		 public virtual void ElementCleaned( E element )
		 {
		 }
	}

}