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
namespace Neo4Net.Consistency.checking.cache
{
	using Record = Neo4Net.Kernel.impl.store.record.Record;
	using ByteArray = Neo4Net.@unsafe.Impl.Batchimport.cache.ByteArray;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.cache.CacheSlots_Fields.ID_SLOT_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory_Fields.AUTO_WITHOUT_PAGECACHE;

	/// <summary>
	/// Simply combining a <seealso cref="ByteArray"/> with <seealso cref="ByteArrayBitsManipulator"/>, so that each byte[] index can be split up into
	/// slots, i.e. holding multiple values for space efficiency and convenience.
	/// </summary>
	public class PackedMultiFieldCache
	{
		 private readonly ByteArray _array;
		 private ByteArrayBitsManipulator _slots;
		 private long[] _initValues;

		 internal static ByteArray DefaultArray()
		 {
			  return AUTO_WITHOUT_PAGECACHE.newDynamicByteArray( 1_000_000, new sbyte[ByteArrayBitsManipulator.MAX_BYTES] );
		 }

		 public PackedMultiFieldCache( params int[] slotSizes ) : this( DefaultArray(), slotSizes )
		 {
		 }

		 public PackedMultiFieldCache( ByteArray array, params int[] slotSizes )
		 {
			  this._array = array;
			  SlotSizes = slotSizes;
		 }

		 public virtual void Put( long index, params long[] values )
		 {
			  for ( int i = 0; i < values.Length; i++ )
			  {
					_slots.set( _array, index, i, values[i] );
			  }
		 }

		 public virtual void Put( long index, int slot, long value )
		 {
			  _slots.set( _array, index, slot, value );
		 }

		 public virtual long Get( long index, int slot )
		 {
			  return _slots.get( _array, index, slot );
		 }

		 public virtual params int[] SlotSizes
		 {
			 set
			 {
				  this._slots = new ByteArrayBitsManipulator( value );
				  this._initValues = GetInitVals( value );
			 }
		 }

		 public virtual void Clear()
		 {
			  long length = _array.length();
			  for ( long i = 0; i < length; i++ )
			  {
					Clear( i );
			  }
		 }

		 public virtual void Clear( long index )
		 {
			  Put( index, _initValues );
		 }

		 private static long[] GetInitVals( int[] slotSizes )
		 {
			  long[] initVals = new long[slotSizes.Length];
			  for ( int i = 0; i < initVals.Length; i++ )
			  {
					initVals[i] = IsId( slotSizes, i ) ? Record.NO_NEXT_RELATIONSHIP.intValue() : 0;
			  }
			  return initVals;
		 }

		 private static bool IsId( int[] slotSizes, int i )
		 {
			  return slotSizes[i] >= ID_SLOT_SIZE;
		 }
	}

}