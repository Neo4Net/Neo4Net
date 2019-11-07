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
namespace Neo4Net.Collections.primitive
{
	using IntKeyLongValueTable = Neo4Net.Collections.primitive.hopscotch.IntKeyLongValueTable;
	using Neo4Net.Collections.primitive.hopscotch;
	using Neo4Net.Collections.primitive.hopscotch;
	using Neo4Net.Collections.primitive.hopscotch;
	using LongKeyIntValueTable = Neo4Net.Collections.primitive.hopscotch.LongKeyIntValueTable;
	using LongKeyLongValueTable = Neo4Net.Collections.primitive.hopscotch.LongKeyLongValueTable;
	using LongKeyLongValueUnsafeTable = Neo4Net.Collections.primitive.hopscotch.LongKeyLongValueUnsafeTable;
	using Neo4Net.Collections.primitive.hopscotch;
	using Neo4Net.Collections.primitive.hopscotch;
	using Neo4Net.Collections.primitive.hopscotch;
	using PrimitiveIntHashSet = Neo4Net.Collections.primitive.hopscotch.PrimitiveIntHashSet;
	using PrimitiveIntLongHashMap = Neo4Net.Collections.primitive.hopscotch.PrimitiveIntLongHashMap;
	using Neo4Net.Collections.primitive.hopscotch;
	using PrimitiveLongHashSet = Neo4Net.Collections.primitive.hopscotch.PrimitiveLongHashSet;
	using PrimitiveLongIntHashMap = Neo4Net.Collections.primitive.hopscotch.PrimitiveLongIntHashMap;
	using PrimitiveLongLongHashMap = Neo4Net.Collections.primitive.hopscotch.PrimitiveLongLongHashMap;
	using Neo4Net.Collections.primitive.hopscotch;
	using GlobalMemoryTracker = Neo4Net.Memory.GlobalMemoryTracker;
	using IMemoryAllocationTracker = Neo4Net.Memory.IMemoryAllocationTracker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.collection.primitive.hopscotch.HopScotchHashingAlgorithm.NO_MONITOR;

	/// <summary>
	/// Convenient factory for common primitive sets and maps.
	/// </summary>
	/// <seealso cref= PrimitiveIntCollections </seealso>
	/// <seealso cref= PrimitiveLongCollections </seealso>
	public class Primitive
	{
		 /// <summary>
		 /// Used as value marker for sets, where values aren't applicable. The hop scotch algorithm still
		 /// deals with values so having this will have no-value collections, like sets communicate
		 /// the correct semantics to the algorithm.
		 /// </summary>
		 public static readonly object ValueMarker = new object();
		 public static readonly int DefaultHeapCapacity = 1 << 4;
		 public static readonly int DefaultOffheapCapacity = 1 << 20;

		 private Primitive()
		 {
		 }

		 public static PrimitiveLongList LongList()
		 {
			  return new PrimitiveLongList();
		 }

		 public static PrimitiveLongList LongList( int size )
		 {
			  return new PrimitiveLongList( size );
		 }

		 // Some example would be...
		 public static PrimitiveLongSet LongSet()
		 {
			  return LongSet( DefaultHeapCapacity );
		 }

		 public static PrimitiveLongSet LongSet( int initialCapacity )
		 {
			  return new PrimitiveLongHashSet( new LongKeyTable<object>( initialCapacity, ValueMarker ), ValueMarker, NO_MONITOR );
		 }

		 public static PrimitiveLongSet OffHeapLongSet()
		 {
			  return OffHeapLongSet( GlobalMemoryTracker.INSTANCE );
		 }

		 public static PrimitiveLongSet OffHeapLongSet( IMemoryAllocationTracker allocationTracker )
		 {
			  return OffHeapLongSet( DefaultOffheapCapacity, allocationTracker );
		 }

		 public static PrimitiveLongSet OffHeapLongSet( int initialCapacity, IMemoryAllocationTracker allocationTracker )
		 {
			  return new PrimitiveLongHashSet( new LongKeyUnsafeTable<object>( initialCapacity, ValueMarker, allocationTracker ), ValueMarker, NO_MONITOR );
		 }

		 public static PrimitiveLongIntMap LongIntMap()
		 {
			  return LongIntMap( DefaultHeapCapacity );
		 }

		 public static PrimitiveLongIntMap LongIntMap( int initialCapacity )
		 {
			  return new PrimitiveLongIntHashMap( new LongKeyIntValueTable( initialCapacity ), NO_MONITOR );
		 }

		 public static PrimitiveLongLongMap LongLongMap()
		 {
			  return LongLongMap( DefaultHeapCapacity );
		 }

		 public static PrimitiveLongLongMap LongLongMap( int initialCapacity )
		 {
			  return new PrimitiveLongLongHashMap( new LongKeyLongValueTable( initialCapacity ), NO_MONITOR );
		 }

		 public static PrimitiveLongLongMap OffHeapLongLongMap()
		 {
			  return OffHeapLongLongMap( GlobalMemoryTracker.INSTANCE );
		 }

		 public static PrimitiveLongLongMap OffHeapLongLongMap( IMemoryAllocationTracker allocationTracker )
		 {
			  return OffHeapLongLongMap( DefaultOffheapCapacity, allocationTracker );
		 }

		 public static PrimitiveLongLongMap OffHeapLongLongMap( int initialCapacity, IMemoryAllocationTracker allocationTracker )
		 {
			  return new PrimitiveLongLongHashMap( new LongKeyLongValueUnsafeTable( initialCapacity, allocationTracker ), NO_MONITOR );
		 }

		 public static PrimitiveLongObjectMap<VALUE> LongObjectMap<VALUE>()
		 {
			  return LongObjectMap( DefaultHeapCapacity );
		 }

		 public static PrimitiveLongObjectMap<VALUE> LongObjectMap<VALUE>( int initialCapacity )
		 {
			  return new PrimitiveLongObjectHashMap<VALUE>( new LongKeyObjectValueTable<VALUE>( initialCapacity ), NO_MONITOR );
		 }

		 public static PrimitiveIntSet IntSet()
		 {
			  return IntSet( DefaultHeapCapacity );
		 }

		 public static PrimitiveIntSet IntSet( int initialCapacity )
		 {
			  return new PrimitiveIntHashSet( new IntKeyTable<object>( initialCapacity, ValueMarker ), ValueMarker, NO_MONITOR );
		 }

		 public static PrimitiveIntSet OffHeapIntSet()
		 {
			  return OffHeapIntSet( GlobalMemoryTracker.INSTANCE );
		 }

		 public static PrimitiveIntSet OffHeapIntSet( IMemoryAllocationTracker allocationTracker )
		 {
			  return new PrimitiveIntHashSet( new IntKeyUnsafeTable<object>( DefaultOffheapCapacity, ValueMarker, allocationTracker ), ValueMarker, NO_MONITOR );
		 }

		 public static PrimitiveIntSet OffHeapIntSet( int initialCapacity, IMemoryAllocationTracker allocationTracker )
		 {
			  return new PrimitiveIntHashSet( new IntKeyUnsafeTable<object>( initialCapacity, ValueMarker, allocationTracker ), ValueMarker, NO_MONITOR );
		 }

		 public static PrimitiveIntObjectMap<VALUE> IntObjectMap<VALUE>()
		 {
			  return IntObjectMap( DefaultHeapCapacity );
		 }

		 public static PrimitiveIntObjectMap<VALUE> IntObjectMap<VALUE>( int initialCapacity )
		 {
			  return new PrimitiveIntObjectHashMap<VALUE>( new IntKeyObjectValueTable<VALUE>( initialCapacity ), NO_MONITOR );
		 }

		 public static PrimitiveIntLongMap IntLongMap()
		 {
			  return IntLongMap( DefaultHeapCapacity );
		 }

		 public static PrimitiveIntLongMap IntLongMap( int initialCapacity )
		 {
			  return new PrimitiveIntLongHashMap( new IntKeyLongValueTable( initialCapacity ), NO_MONITOR );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static PrimitiveLongIterator iterator(final long... longs)
		 public static PrimitiveLongIterator Iterator( params long[] longs )
		 {
			  return new PrimitiveLongIteratorAnonymousInnerClass( longs );
		 }

		 private class PrimitiveLongIteratorAnonymousInnerClass : PrimitiveLongIterator
		 {
			 private long[] _longs;

			 public PrimitiveLongIteratorAnonymousInnerClass( long[] longs )
			 {
				 this._longs = longs;
			 }

			 internal int i;

			 public bool hasNext()
			 {
				  return i < _longs.Length;
			 }

			 public long next()
			 {
				  return _longs[i++];
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static PrimitiveIntIterator iterator(final int... ints)
		 public static PrimitiveIntIterator Iterator( params int[] ints )
		 {
			  return new PrimitiveIntIteratorAnonymousInnerClass( ints );
		 }

		 private class PrimitiveIntIteratorAnonymousInnerClass : PrimitiveIntIterator
		 {
			 private int[] _ints;

			 public PrimitiveIntIteratorAnonymousInnerClass( int[] ints )
			 {
				 this._ints = ints;
			 }

			 internal int i;

			 public bool hasNext()
			 {
				  return i < _ints.Length;
			 }

			 public int next()
			 {
				  return _ints[i++];
			 }
		 }
	}

}