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
	using LongIterable = org.eclipse.collections.api.LongIterable;
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;

	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using CollectionsFactory = Org.Neo4j.Kernel.impl.util.collection.CollectionsFactory;

	/// <summary>
	/// Primitive long version of collection that with given a sequence of add and removal operations, tracks
	/// which elements need to actually be added and removed at minimum from some
	/// target collection such that the result is equivalent to just
	/// executing the sequence of additions and removals in order.
	/// </summary>
	public class MutableLongDiffSetsImpl : MutableLongDiffSets
	{
		 private static readonly MutableLongSet _notInitialized = LongSets.mutable.empty().asUnmodifiable();

		 private readonly CollectionsFactory _collectionsFactory;
		 private MutableLongSet _added;
		 private MutableLongSet _removed;

		 public MutableLongDiffSetsImpl( MutableLongSet added, MutableLongSet removed, CollectionsFactory collectionsFactory )
		 {
			  this._added = added;
			  this._removed = removed;
			  this._collectionsFactory = collectionsFactory;
		 }

		 public MutableLongDiffSetsImpl( CollectionsFactory collectionsFactory ) : this( _notInitialized, _notInitialized, collectionsFactory )
		 {
		 }

		 public override bool IsAdded( long element )
		 {
			  return _added.contains( element );
		 }

		 public override bool IsRemoved( long element )
		 {
			  return _removed.contains( element );
		 }

		 public override void RemoveAll( LongIterable elements )
		 {
			  CheckRemovedElements();
			  elements.each( this.removeElement );
		 }

		 public override void AddAll( LongIterable elements )
		 {
			  CheckAddedElements();
			  elements.each( this.addElement );
		 }

		 public override void Add( long element )
		 {
			  CheckAddedElements();
			  AddElement( element );
		 }

		 public override bool Remove( long element )
		 {
			  CheckRemovedElements();
			  return RemoveElement( element );
		 }

		 public override LongIterator Augment( LongIterator source )
		 {
			  return DiffApplyingPrimitiveLongIterator.Augment( source, _added, _removed );
		 }

		 public override PrimitiveLongResourceIterator Augment( PrimitiveLongResourceIterator source )
		 {
			  return DiffApplyingPrimitiveLongIterator.Augment( source, _added, _removed );
		 }

		 public override int Delta()
		 {
			  return _added.size() - _removed.size();
		 }

		 public virtual LongSet Added
		 {
			 get
			 {
				  return _added;
			 }
		 }

		 public virtual LongSet Removed
		 {
			 get
			 {
				  return _removed;
			 }
		 }

		 public virtual bool Empty
		 {
			 get
			 {
				  return _added.Empty && _removed.Empty;
			 }
		 }

		 private void AddElement( long element )
		 {
			  if ( _removed.Empty || !_removed.remove( element ) )
			  {
					_added.add( element );
			  }
		 }

		 private bool RemoveElement( long element )
		 {
			  if ( !_added.Empty && _added.remove( element ) )
			  {
					return true;
			  }
			  return _removed.add( element );
		 }

		 private void CheckAddedElements()
		 {
			  if ( _added == _notInitialized )
			  {
					_added = _collectionsFactory.newLongSet();
			  }
		 }

		 private void CheckRemovedElements()
		 {
			  if ( _removed == _notInitialized )
			  {
					_removed = _collectionsFactory.newLongSet();
			  }
		 }
	}

}