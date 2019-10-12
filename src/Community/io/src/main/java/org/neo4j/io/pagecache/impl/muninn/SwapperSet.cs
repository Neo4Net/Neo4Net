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
namespace Neo4Net.Io.pagecache.impl.muninn
{
	using IntIterator = org.eclipse.collections.api.iterator.IntIterator;
	using IntSet = org.eclipse.collections.api.set.primitive.IntSet;
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;
	using IntHashSet = org.eclipse.collections.impl.set.mutable.primitive.IntHashSet;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.safeCastIntToShort;

	/// <summary>
	/// The SwapperSet maintains the set of allocated <seealso cref="PageSwapper"/>s, and their mapping to swapper ids.
	/// These swapper ids are a limited resource, so they must eventually be reused as files are mapped and unmapped.
	/// Before a swapper id can be reused, we have to make sure that there are no pages in the page cache, that
	/// are bound to the old swapper id. To ensure this, we have to periodically <seealso cref="MuninnPageCache.vacuum(SwapperSet)"/>
	/// the page cache. The vacuum process will then fully evict all pages that are bound to a page swapper id that
	/// was freed before the start of the vacuum process.
	/// </summary>
	internal sealed class SwapperSet
	{
		 // The sentinel is used to reserve swapper id 0 as a special value.
		 private static readonly SwapperMapping _sentinel = new SwapperMapping( 0, null );
		 // The tombstone is used as a marker to reserve allocation entries that have been freed, but not yet vacuumed.
		 // An allocation cannot be reused until it has been vacuumed.
		 private static readonly SwapperMapping _tombstone = new SwapperMapping( 0, null );
		 private static readonly int _maxSwapperId = ( 1 << 21 ) - 1;
		 private volatile SwapperMapping[] _swapperMappings = new SwapperMapping[] { _sentinel };
		 private readonly MutableIntSet _free = new IntHashSet();
		 private readonly object _vacuumLock = new object();
		 private int _freeCounter; // Used in `free`; Guarded by `this`

		 /// <summary>
		 /// The mapping entry between a <seealso cref="PageSwapper"/> and its swapper id.
		 /// </summary>
		 internal sealed class SwapperMapping
		 {
			  public readonly int Id;
			  public readonly PageSwapper Swapper;

			  internal SwapperMapping( int id, PageSwapper swapper )
			  {
					this.Id = id;
					this.Swapper = swapper;
			  }
		 }

		 /// <summary>
		 /// Get the <seealso cref="SwapperMapping"/> for the given swapper id.
		 /// </summary>
		 internal SwapperMapping GetAllocation( int id )
		 {
			  CheckId( id );
			  SwapperMapping swapperMapping = _swapperMappings[id];
			  if ( swapperMapping == null || swapperMapping == _tombstone )
			  {
					return null;
			  }
			  return swapperMapping;
		 }

		 private void CheckId( int id )
		 {
			  if ( id == 0 )
			  {
					throw new System.ArgumentException( "0 is an invalid swapper id" );
			  }
		 }

		 /// <summary>
		 /// Allocate a new swapper id for the given <seealso cref="PageSwapper"/>.
		 /// </summary>
		 internal int Allocate( PageSwapper swapper )
		 {
			 lock ( this )
			 {
				  SwapperMapping[] swapperMappings = this._swapperMappings;
      
				  // First look for an available freed slot.
				  lock ( _free )
				  {
						if ( !_free.Empty )
						{
							 int id = _free.intIterator().next();
							 _free.remove( id );
							 swapperMappings[id] = new SwapperMapping( id, swapper );
							 this._swapperMappings = swapperMappings; // Volatile store synchronizes-with loads in getters.
							 return id;
						}
				  }
      
				  // No free slot was found above, so we extend the array to make room for a new slot.
				  int id = swapperMappings.Length;
				  if ( id + 1 > _maxSwapperId )
				  {
						throw new System.InvalidOperationException( "All swapper ids are allocated: " + _maxSwapperId );
				  }
				  swapperMappings = Arrays.copyOf( swapperMappings, id + 1 );
				  swapperMappings[id] = new SwapperMapping( id, swapper );
				  this._swapperMappings = swapperMappings; // Volatile store synchronizes-with loads in getters.
				  return id;
			 }
		 }

		 /// <summary>
		 /// Free the given swapper id, and return {@code true} if it is time for a
		 /// <seealso cref="MuninnPageCache.vacuum(SwapperSet)"/>, otherwise it returns {@code false}.
		 /// </summary>
		 internal bool Free( int id )
		 {
			 lock ( this )
			 {
				  CheckId( id );
				  SwapperMapping[] swapperMappings = this._swapperMappings;
				  SwapperMapping current = swapperMappings[id];
				  if ( current == null || current == _tombstone )
				  {
						throw new System.InvalidOperationException( "PageSwapper allocation id " + id + " is currently not allocated. Likely a double free bug." );
				  }
				  swapperMappings[id] = _tombstone;
				  this._swapperMappings = swapperMappings; // Volatile store synchronizes-with loads in getters.
				  _freeCounter++;
				  if ( _freeCounter == 20 )
				  {
						_freeCounter = 0;
						return true;
				  }
				  return false;
			 }
		 }

		 /// <summary>
		 /// Collect all freed page swapper ids, and pass them to the given callback, after which the freed ids will be
		 /// eligible for reuse.
		 /// This is done with careful synchronisation such that allocating and freeing of ids is allowed to mostly proceed
		 /// concurrently.
		 /// </summary>
		 internal void Vacuum( System.Action<IntSet> evictAllLoadedPagesCallback )
		 {
			  // We do this complicated locking to avoid blocking allocate() and free() as much as possible, while still only
			  // allow a single thread to do vacuum at a time, and at the same time have consistent locking around the
			  // set of free ids.
			  lock ( _vacuumLock )
			  {
					// Collect currently free ids.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableIntSet freeIds = new org.eclipse.collections.impl.set.mutable.primitive.IntHashSet();
					MutableIntSet freeIds = new IntHashSet();
					SwapperMapping[] swapperMappings = this._swapperMappings;
					for ( int id = 0; id < swapperMappings.Length; id++ )
					{
						 SwapperMapping swapperMapping = swapperMappings[id];
						 if ( swapperMapping == _tombstone )
						 {
							  freeIds.add( id );
						 }
					}

					// Evict all of them without holding up the lock on the free id set. This allows allocate() and free() to
					// proceed concurrently with our eviction. This is safe because we know that the ids we are evicting cannot
					// possibly be reused until we remove them from the free id set, which we won't do until after we've evicted
					// all of their loaded pages.
					evictAllLoadedPagesCallback( freeIds );

					// Finally, all of the pages that remained in memory with an unmapped swapper id have been evicted. We can
					// now safely allow those ids to be reused. Note, however, that free() might have been called while we were
					// doing this, so we can't just free.clear() the set; no, we have to explicitly remove only those specific
					// ids whose pages we evicted.
					lock ( this )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.IntIterator itr = freeIds.intIterator();
						 IntIterator itr = freeIds.intIterator();
						 while ( itr.hasNext() )
						 {
							  int freeId = itr.next();
							  swapperMappings[freeId] = null;
						 }
						 this._swapperMappings = swapperMappings; // Volatile store synchronizes-with loads in getters.
					}
					lock ( _free )
					{
						 _free.addAll( freeIds );
					}
			  }
		 }

		 internal int CountAvailableIds()
		 {
			 lock ( this )
			 {
				  // the max id is one less than the allowed count, but we subtract one for the reserved id 0
				  int available = _maxSwapperId;
				  available -= _swapperMappings.Length; // ids that are allocated are not available
				  available += _free.size(); // add back the ids that are free to be reused
				  return available;
			 }
		 }
	}

}