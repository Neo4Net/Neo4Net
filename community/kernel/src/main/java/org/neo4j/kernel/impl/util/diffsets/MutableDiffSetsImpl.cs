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
namespace Org.Neo4j.Kernel.impl.util.diffsets
{

	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;

	public class MutableDiffSetsImpl<T> : MutableDiffSets<T>
	{
		 private ISet<T> _addedElements;
		 private ISet<T> _removedElements;
		 private System.Predicate<T> _filter;

		 private MutableDiffSetsImpl( ISet<T> addedElements, ISet<T> removedElements )
		 {
			  this._addedElements = addedElements;
			  this._removedElements = removedElements;
		 }

		 public MutableDiffSetsImpl() : this(null, null)
		 {
		 }

		 public override bool Add( T elem )
		 {
			  bool wasRemoved = Removed( false ).remove( elem );
			  // Add to the addedElements only if it was not removed from the removedElements
			  return wasRemoved || Added( true ).Add( elem );
		 }

		 public override void AddAll( IEnumerator<T> elems )
		 {
			  while ( elems.MoveNext() )
			  {
					Add( elems.Current );
			  }
		 }

		 public override bool Remove( T elem )
		 {
			  bool removedFromAddedElements = Added( false ).remove( elem );
			  // Add to the removedElements only if it was not removed from the addedElements.
			  return removedFromAddedElements || Removed( true ).Add( elem );
		 }

		 public override void RemoveAll( IEnumerator<T> elems )
		 {
			  while ( elems.MoveNext() )
			  {
					Remove( elems.Current );
			  }
		 }

		 public override void Clear()
		 {
			  if ( _addedElements != null )
			  {
					_addedElements.Clear();
			  }
			  if ( _removedElements != null )
			  {
					_removedElements.Clear();
			  }
		 }

		 public override bool UnRemove( T item )
		 {
			  return Removed( false ).remove( item );
		 }

		 protected internal virtual ISet<T> Added( bool create )
		 {
			  if ( _addedElements == null )
			  {
					if ( !create )
					{
						 return Collections.emptySet();
					}
					_addedElements = new HashSet<T>();
			  }
			  return _addedElements;
		 }

		 private void EnsureFilterHasBeenCreated()
		 {
			  if ( _filter == null )
			  {
					_filter = item => !Removed( false ).Contains( item ) && !Added( false ).Contains( item );
			  }
		 }

		 public override string ToString()
		 {
			  return format( "{+%s, -%s}", Added( false ), Removed( false ) );
		 }

		 public override bool IsAdded( T elem )
		 {
			  return Added( false ).Contains( elem );
		 }

		 public override bool IsRemoved( T elem )
		 {
			  return Removed( false ).Contains( elem );
		 }

		 public virtual ISet<T> Added
		 {
			 get
			 {
				  return ResultSet( _addedElements );
			 }
		 }

		 public virtual ISet<T> Removed
		 {
			 get
			 {
				  return ResultSet( _removedElements );
			 }
		 }

		 public virtual bool Empty
		 {
			 get
			 {
				  return Added( false ).Count == 0 && Removed( false ).Count == 0;
			 }
		 }

		 public override IEnumerator<T> Apply<T1>( IEnumerator<T1> source ) where T1 : T
		 {
			  IEnumerator<T> result = ( System.Collections.IEnumerator ) source;
			  if ( ( _removedElements != null && _removedElements.Count > 0 ) || ( _addedElements != null && _addedElements.Count > 0 ) )
			  {
					EnsureFilterHasBeenCreated();
					result = Iterators.filter( _filter, result );
			  }
			  if ( _addedElements != null && _addedElements.Count > 0 )
			  {
					result = Iterators.concat( result, _addedElements.GetEnumerator() );
			  }
			  return result;
		 }

		 public override int Delta()
		 {
			  return Added( false ).Count - Removed( false ).Count;
		 }

		 public override MutableDiffSetsImpl<T> FilterAdded( System.Predicate<T> addedFilter )
		 {
			  return new MutableDiffSetsImpl<T>( Iterables.asSet( Iterables.filter( addedFilter, Added( false ) ) ), Iterables.asSet( Removed( false ) ) );
		 }

		 protected internal virtual ISet<T> Removed( bool create )
		 {
			  if ( _removedElements == null )
			  {
					if ( !create )
					{
						 return Collections.emptySet();
					}
					_removedElements = new HashSet<T>();
			  }
			  return _removedElements;
		 }

		 public virtual void Replace( T toRemove, T toAdd )
		 {
			  ISet<T> added = added( true ); // we're doing both add and remove on it, so pass in true
			  bool removedFromAdded = added.remove( toRemove );
			  Removed( false ).remove( toAdd );

			  added.Add( toAdd );
			  if ( !removedFromAdded )
			  {
					Removed( true ).Add( toRemove );
			  }
		 }

		 private ISet<T> ResultSet( ISet<T> coll )
		 {
			  return coll == null ? Collections.emptySet() : Collections.unmodifiableSet(coll);
		 }
	}

}