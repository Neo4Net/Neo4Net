using System;

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
namespace Neo4Net.Kernel.impl.util.collection
{
	using LongIterable = org.eclipse.collections.api.LongIterable;
	using LongProcedure = org.eclipse.collections.api.block.procedure.primitive.LongProcedure;
	using MutableLongIterator = org.eclipse.collections.api.iterator.MutableLongIterator;
	using MutableMultimap = org.eclipse.collections.api.multimap.MutableMultimap;
	using ImmutableLongSet = org.eclipse.collections.api.set.primitive.ImmutableLongSet;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using Multimaps = org.eclipse.collections.impl.factory.Multimaps;
	using SynchronizedLongSet = org.eclipse.collections.impl.set.mutable.primitive.SynchronizedLongSet;
	using UnmodifiableLongSet = org.eclipse.collections.impl.set.mutable.primitive.UnmodifiableLongSet;

	using Resource = Neo4Net.GraphDb.Resource;
	using VisibleForTesting = Neo4Net.Utils.VisibleForTesting;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.bitCount;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.util.Preconditions.checkArgument;

	/// <summary>
	/// Off heap implementation of long hash set.
	/// <ul>
	/// <li>It is <b>not thread-safe</b>
	/// <li>It has to be closed to prevent native memory leakage
	/// <li>Iterators returned by this set are fail-fast
	/// </ul>
	/// </summary>
	internal class MutableLinearProbeLongHashSet : AbstractLinearProbeLongHashSet, MutableLongSet, Resource
	{
		 internal const int DEFAULT_CAPACITY = 32;
		 internal const int REMOVALS_RATIO = 4;
		 private const double LOAD_FACTOR = 0.75;

		 private readonly MemoryAllocator _allocator;
		 private readonly MutableMultimap<Memory, FrozenCopy> _frozenCopies = Multimaps.mutable.list.empty();

		 private int _resizeOccupancyThreshold;
		 private int _resizeRemovalsThreshold;
		 private int _removals;
		 private bool _frozen;

		 internal MutableLinearProbeLongHashSet( MemoryAllocator allocator )
		 {
			  this._allocator = requireNonNull( allocator );
			  AllocateMemory( DEFAULT_CAPACITY );
		 }

		 public override bool Add( long element )
		 {
			  ++ModCount;
			  if ( element == 0 )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean hadZero = hasZero;
					bool hadZero = HasZero;
					HasZero = true;
					return hadZero != HasZero;
			  }
			  if ( element == 1 )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean hadOne = hasOne;
					bool hadOne = HasOne;
					HasOne = true;
					return hadOne != HasOne;
			  }
			  return AddToMemory( element );
		 }

		 public override bool AddAll( params long[] elements )
		 {
			  ++ModCount;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int prevSize = size();
			  int prevSize = Size();
			  foreach ( long element in elements )
			  {
					Add( element );
			  }
			  return prevSize != Size();
		 }

		 public override bool AddAll( LongIterable elements )
		 {
			  ++ModCount;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int prevSize = size();
			  int prevSize = Size();
			  elements.forEach( this.add );
			  return prevSize != Size();
		 }

		 public override bool Remove( long element )
		 {
			  ++ModCount;
			  if ( element == 0 )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean hadZero = hasZero;
					bool hadZero = HasZero;
					HasZero = false;
					return hadZero != HasZero;
			  }
			  if ( element == 1 )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean hadOne = hasOne;
					bool hadOne = HasOne;
					HasOne = false;
					return hadOne != HasOne;
			  }
			  return RemoveFromMemory( element );
		 }

		 public override bool RemoveAll( LongIterable elements )
		 {
			  ++ModCount;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int prevSize = size();
			  int prevSize = Size();
			  elements.forEach( this.remove );
			  return prevSize != Size();
		 }

		 public override bool RemoveAll( params long[] elements )
		 {
			  ++ModCount;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int prevSize = size();
			  int prevSize = Size();
			  foreach ( long element in elements )
			  {
					Remove( element );
			  }
			  return prevSize != Size();
		 }

		 public override bool RetainAll( LongIterable elements )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override bool RetainAll( params long[] source )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void Clear()
		 {
			  ++ModCount;
			  CopyIfFrozen();
			  Memory.clear();
			  HasZero = false;
			  HasOne = false;
			  ElementsInMemory = 0;
			  _removals = 0;
		 }

		 public override MutableLongIterator LongIterator()
		 {
			  return new FailFastIterator( this );
		 }

		 public override void Close()
		 {
			  ++ModCount;
			  if ( Memory != null )
			  {
					_frozenCopies.forEachKeyMultiValues((mem, copies) =>
					{
					 mem.free();
					 copies.forEach( FrozenCopy.invalidate );
					});
					if ( !_frozenCopies.containsKey( Memory ) )
					{
						 Memory.free();
					}
					Memory = null;
					_frozenCopies.clear();
			  }
		 }

		 public override MutableLongSet Tap( LongProcedure procedure )
		 {
			  Each( procedure );
			  return this;
		 }

		 public override MutableLongSet With( long element )
		 {
			  Add( element );
			  return this;
		 }

		 public override MutableLongSet Without( long element )
		 {
			  Remove( element );
			  return this;
		 }

		 public override MutableLongSet WithAll( LongIterable elements )
		 {
			  AddAll( elements );
			  return this;
		 }

		 public override MutableLongSet WithoutAll( LongIterable elements )
		 {
			  RemoveAll( elements );
			  return this;
		 }

		 public override MutableLongSet AsUnmodifiable()
		 {
			  return new UnmodifiableLongSet( this );
		 }

		 public override MutableLongSet AsSynchronized()
		 {
			  return new SynchronizedLongSet( this );
		 }

		 public override LongSet Freeze()
		 {
			  _frozen = true;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final FrozenCopy frozen = new FrozenCopy();
			  FrozenCopy frozen = new FrozenCopy( this );
			  _frozenCopies.put( Memory, frozen );
			  return frozen;
		 }

		 public override ImmutableLongSet ToImmutable()
		 {
			  throw new System.NotSupportedException();
		 }

		 private bool RemoveFromMemory( long element )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int idx = indexOf(element);
			  int idx = IndexOf( element );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long valueAtIdx = memory.readLong((long) idx * Long.BYTES);
			  long valueAtIdx = Memory.readLong( ( long ) idx * Long.BYTES );

			  if ( valueAtIdx != element )
			  {
					return false;
			  }

			  CopyIfFrozen();

			  Memory.writeLong( ( long ) idx * Long.BYTES, REMOVED );
			  --ElementsInMemory;
			  ++_removals;

			  if ( _removals >= _resizeRemovalsThreshold )
			  {
					RehashWithoutGrow();
			  }
			  return true;
		 }

		 private bool AddToMemory( long element )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int idx = indexOf(element);
			  int idx = IndexOf( element );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long valueAtIdx = valueAt(idx);
			  long valueAtIdx = ValueAt( idx );

			  if ( valueAtIdx == element )
			  {
					return false;
			  }

			  if ( valueAtIdx == REMOVED )
			  {
					--_removals;
			  }

			  CopyIfFrozen();

			  Memory.writeLong( ( long ) idx * Long.BYTES, element );
			  ++ElementsInMemory;

			  if ( ElementsInMemory >= _resizeOccupancyThreshold )
			  {
					GrowAndRehash();
			  }

			  return true;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting void growAndRehash()
		 internal virtual void GrowAndRehash()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int newCapacity = capacity * 2;
			  int newCapacity = Capacity * 2;
			  if ( newCapacity < Capacity )
			  {
					throw new Exception( "LongSet reached capacity limit" );
			  }
			  Rehash( newCapacity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting void rehashWithoutGrow()
		 internal virtual void RehashWithoutGrow()
		 {
			  Rehash( Capacity );
		 }

		 private void AllocateMemory( int newCapacity )
		 {
			  checkArgument( newCapacity > 1 && bitCount( newCapacity ) == 1, "Capacity must be power of 2" );
			  Capacity = newCapacity;
			  _resizeOccupancyThreshold = ( int )( newCapacity * LOAD_FACTOR );
			  _resizeRemovalsThreshold = newCapacity / REMOVALS_RATIO;
			  Memory = _allocator( ( long ) newCapacity * Long.BYTES, true );
		 }

		 private void Rehash( int newCapacity )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int prevCapacity = capacity;
			  int prevCapacity = Capacity;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Memory prevMemory = memory;
			  Memory prevMemory = Memory;
			  ElementsInMemory = 0;
			  _removals = 0;
			  AllocateMemory( newCapacity );

			  for ( int i = 0; i < prevCapacity; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long value = prevMemory.readLong((long) i * Long.BYTES);
					long value = prevMemory.ReadLong( ( long ) i * Long.BYTES );
					if ( IsRealValue( value ) )
					{
						 Add( value );
					}
			  }

			  prevMemory.Free();
		 }

		 private void CopyIfFrozen()
		 {
			  if ( _frozen )
			  {
					_frozen = false;
					Memory = Memory.copy();
			  }
		 }

		 internal class FrozenCopy : AbstractLinearProbeLongHashSet
		 {
			 private readonly MutableLinearProbeLongHashSet _outerInstance;


			  internal FrozenCopy( MutableLinearProbeLongHashSet outerInstance ) : base( outerInstance )
			  {
				  this._outerInstance = outerInstance;
			  }

			  public override LongSet Freeze()
			  {
					return this;
			  }

			  public override ImmutableLongSet ToImmutable()
			  {
					throw new System.NotSupportedException();
			  }

			  internal virtual void Invalidate()
			  {
					++FrozenCopy.this.ModCount;
			  }
		 }
	}

}