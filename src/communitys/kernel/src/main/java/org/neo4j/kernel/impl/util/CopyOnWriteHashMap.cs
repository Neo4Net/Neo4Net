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
namespace Neo4Net.Kernel.impl.util
{

	/// <summary>
	/// Optimized for rare writes and very frequent reads in a thread safe way.
	/// Reads impose no synchronization or locking.
	/// 
	/// <seealso cref="keySet()"/>, <seealso cref="values()"/> and <seealso cref="entrySet()"/> wraps the
	/// returned iterators since they provide the <seealso cref="Iterator.remove() remove"/>
	/// method which isn't supported by this implementation. These iterators are also
	/// views of the map at that point in time so they don't change during their
	/// life time.
	/// 
	/// @author Mattias Persson
	/// </summary>
	/// @param <K> key type </param>
	/// @param <V> value type </param>
	public class CopyOnWriteHashMap<K, V> : IDictionary<K, V>
	{
		 private volatile IDictionary<K, V> _actual = new Dictionary<K, V>();

		 public override int Size()
		 {
			  return _actual.Count;
		 }

		 public override bool Empty
		 {
			 get
			 {
				  return _actual.Count == 0;
			 }
		 }

		 public override bool ContainsKey( object key )
		 {
			  return _actual.ContainsKey( key );
		 }

		 public override bool ContainsValue( object value )
		 {
			  return _actual.ContainsValue( value );
		 }

		 public override V Get( object key )
		 {
			  return _actual[key];
		 }

		 private IDictionary<K, V> Copy()
		 {
			  return new Dictionary<K, V>( _actual );
		 }

		 public virtual IDictionary<K, V> Snapshot()
		 {
			  return Collections.unmodifiableMap( _actual );
		 }

		 public override V Put( K key, V value )
		 {
			 lock ( this )
			 {
				  IDictionary<K, V> copy = copy();
				  V previous = copy[key] = value;
				  _actual = copy;
				  return previous;
			 }
		 }

		 public override V Remove( object key )
		 {
			 lock ( this )
			 {
				  IDictionary<K, V> copy = copy();
				  V previous = copy.Remove( key );
				  _actual = copy;
				  return previous;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public synchronized void putAll(@Nonnull Map<? extends K, ? extends V> m)
		 public override void PutAll<T1>( IDictionary<T1> m ) where T1 : K
		 {
			 lock ( this )
			 {
				  IDictionary<K, V> copy = copy();
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
				  copy.putAll( m );
				  _actual = copy;
			 }
		 }

		 public override void Clear()
		 {
			 lock ( this )
			 {
				  _actual = new Dictionary<K, V>();
			 }
		 }

		 private class UnsupportedRemoveIterator<T> : IEnumerator<T>
		 {
			  internal readonly IEnumerator<T> Actual;

			  internal UnsupportedRemoveIterator( IEnumerator<T> actual )
			  {
					this.Actual = actual;
			  }

			  public override bool HasNext()
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return Actual.hasNext();
			  }

			  public override T Next()
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return Actual.next();
			  }

			  public override void Remove()
			  {
					throw new System.NotSupportedException();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public java.util.Set<K> keySet()
		 public override ISet<K> KeySet()
		 {
			  return new AbstractSetAnonymousInnerClass( this );
		 }

		 private class AbstractSetAnonymousInnerClass : AbstractSet<K>
		 {
			 private readonly CopyOnWriteHashMap<K, V> _outerInstance;

			 public AbstractSetAnonymousInnerClass( CopyOnWriteHashMap<K, V> outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool remove( object o )
			 {
				  return _outerInstance.Remove( o ) != null;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public java.util.Iterator<K> iterator()
			 public override IEnumerator<K> iterator()
			 {
				  return new UnsupportedRemoveIterator<K>( _outerInstance.actual.Keys.GetEnumerator() );
			 }

			 public override int size()
			 {
				  return _outerInstance.actual.Count;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public java.util.Collection<V> values()
		 public override ICollection<V> Values()
		 {
			  return new AbstractCollectionAnonymousInnerClass( this );
		 }

		 private class AbstractCollectionAnonymousInnerClass : AbstractCollection<V>
		 {
			 private readonly CopyOnWriteHashMap<K, V> _outerInstance;

			 public AbstractCollectionAnonymousInnerClass( CopyOnWriteHashMap<K, V> outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public java.util.Iterator<V> iterator()
			 public override IEnumerator<V> iterator()
			 {
				  return new UnsupportedRemoveIterator<V>( _outerInstance.actual.Values.GetEnumerator() );
			 }

			 public override int size()
			 {
				  return _outerInstance.actual.Count;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public java.util.Set<Entry<K, V>> entrySet()
		 public override ISet<Entry<K, V>> EntrySet()
		 {
			  return new AbstractSetAnonymousInnerClass2( this );
		 }

		 private class AbstractSetAnonymousInnerClass2 : AbstractSet<Entry<K, V>>
		 {
			 private readonly CopyOnWriteHashMap<K, V> _outerInstance;

			 public AbstractSetAnonymousInnerClass2( CopyOnWriteHashMap<K, V> outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool remove( object o )
			 {
				  throw new System.NotSupportedException();
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public java.util.Iterator<Entry<K, V>> iterator()
			 public override IEnumerator<Entry<K, V>> iterator()
			 {
				  return new UnsupportedRemoveIteratorAnonymousInnerClass( this );
			 }

			 private class UnsupportedRemoveIteratorAnonymousInnerClass : UnsupportedRemoveIterator<Entry<K, V>>
			 {
				 private readonly AbstractSetAnonymousInnerClass2 _outerInstance;

				 public UnsupportedRemoveIteratorAnonymousInnerClass( AbstractSetAnonymousInnerClass2 outerInstance ) : base( outerInstance.outerInstance.actual.SetOfKeyValuePairs().GetEnumerator() )
				 {
					 this.outerInstance = outerInstance;
				 }

				 public override Entry<K, V> next()
				 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Entry<K, V> actualNext = super.next();
					  Entry<K, V> actualNext = base.next();
					  return new EntryAnonymousInnerClass( this, actualNext );
				 }

				 private class EntryAnonymousInnerClass : Entry<K, V>
				 {
					 private readonly UnsupportedRemoveIteratorAnonymousInnerClass _outerInstance;

					 private Entry<K, V> _actualNext;

					 public EntryAnonymousInnerClass( UnsupportedRemoveIteratorAnonymousInnerClass outerInstance, Entry<K, V> actualNext )
					 {
						 this.outerInstance = outerInstance;
						 this._actualNext = actualNext;
					 }

					 public override K Key
					 {
						 get
						 {
							  return _actualNext.Key;
						 }
					 }

					 public override V Value
					 {
						 get
						 {
							  return _actualNext.Value;
						 }
					 }

					 public override V setValue( V value )
					 {
						  throw new System.NotSupportedException();
					 }

					 public override bool Equals( object obj )
					 {
						  return _actualNext.Equals( obj );
					 }

					 public override int GetHashCode()
					 {
						  return _actualNext.GetHashCode();
					 }
				 }
			 }

			 public override int size()
			 {
				  return outerInstance.actual.Count;
			 }
		 }
	}

}