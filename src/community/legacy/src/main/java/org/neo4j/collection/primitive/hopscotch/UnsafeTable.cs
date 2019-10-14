using System.Diagnostics;

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
namespace Neo4Net.Collections.primitive.hopscotch
{
	using MemoryAllocationTracker = Neo4Net.Memory.MemoryAllocationTracker;
	using UnsafeUtil = Neo4Net.@unsafe.Impl.Internal.Dragons.UnsafeUtil;

	public abstract class UnsafeTable<VALUE> : PowerOfTwoQuantizedTable<VALUE>
	{
		 private readonly int _bytesPerKey;
		 private readonly int _bytesPerEntry;
		 private readonly long _dataSize;
		 private readonly long _allocatedBytes;
		 // address which should be free when closing
		 private readonly long _allocatedAddress;
		 // address which should be used to access the table, the address where the table actually starts at
		 private readonly long _address;
		 protected internal readonly VALUE ValueMarker;
		 protected internal readonly MemoryAllocationTracker AllocationTracker;

		 protected internal UnsafeTable( int capacity, int bytesPerKey, VALUE valueMarker, MemoryAllocationTracker allocationTracker ) : base( capacity, 32 )
		 {
			  UnsafeUtil.assertHasUnsafe();
			  this.AllocationTracker = allocationTracker;
			  this._bytesPerKey = bytesPerKey;
			  this._bytesPerEntry = 4 + bytesPerKey;
			  this.ValueMarker = valueMarker;
			  this._dataSize = ( long ) this.CapacityConflict * _bytesPerEntry;

			  // Below is a piece of code which ensures that allocated memory is aligned to 4-byte boundary
			  // if memory system requires aligned memory access. The reason we pick 4-byte boundary is that
			  // it's the lowest common denominator and the size of our hop-bits field for every entry.
			  // So even for a table which would only deal with, say longs (8-byte), it would still need to
			  // read and write 4-byte hop-bits fields. Therefore this table can, if required to, read anything
			  // bigger than 4-byte fields as multiple 4-byte fields. This way it can play well with aligned
			  // memory access requirements.

			  Debug.Assert( _bytesPerEntry % Integer.BYTES == 0, "Bytes per entry needs to be divisible by 4, this constraint " + );
						 "is checked because on memory systems requiring aligned memory access this would otherwise break.";

			  if ( UnsafeUtil.allowUnalignedMemoryAccess )
			  {
					_allocatedBytes = _dataSize;
					this._allocatedAddress = this._address = UnsafeUtil.allocateMemory( _allocatedBytes, this.AllocationTracker );
			  }
			  else
			  {
					// There's an assertion above also verifying this, but it's only an actual problem if our memory system
					// requires aligned access, which seems to be the case right here and now.
					if ( ( _bytesPerEntry % Integer.BYTES ) != 0 )
					{
						 throw new System.ArgumentException( "Memory system requires aligned memory access and " + this.GetType().Name + " was designed to cope with this requirement by " + "being able to accessing data in 4-byte chunks, if needed to. " + "Although this table tried to be constructed with bytesPerKey:" + bytesPerKey + " yielding a bytesPerEntry:" + _bytesPerEntry + ", which isn't 4-byte aligned." );
					}

					_allocatedBytes = _dataSize + Integer.BYTES - 1;
					this._allocatedAddress = UnsafeUtil.allocateMemory( _allocatedBytes, this.AllocationTracker );
					this._address = UnsafeUtil.alignedMemory( _allocatedAddress, Integer.BYTES );
			  }

			  ClearMemory();
		 }

		 public override void Clear()
		 {
			  if ( !Empty )
			  {
					ClearMemory();
			  }
			  base.Clear();
		 }

		 private void ClearMemory()
		 {
			  UnsafeUtil.setMemory( _address, _dataSize, ( sbyte ) - 1 );
		 }

		 public override long Key( int index )
		 {
			  return InternalKey( KeyAddress( index ) );
		 }

		 protected internal abstract long InternalKey( long keyAddress );

		 public override VALUE Value( int index )
		 {
			  return ValueMarker;
		 }

		 public override void Put( int index, long key, VALUE value )
		 {
			  InternalPut( KeyAddress( index ), key, value );
			  SizeConflict++;
		 }

		 protected internal abstract void InternalPut( long keyAddress, long key, VALUE value );

		 public override VALUE PutValue( int index, VALUE value )
		 {
			  return value;
		 }

		 public override long Move( int fromIndex, int toIndex )
		 {
			  long adr = KeyAddress( fromIndex );
			  long key = InternalKey( adr );
			  VALUE value = InternalRemove( adr );
			  InternalPut( KeyAddress( toIndex ), key, value );
			  return key;
		 }

		 public override VALUE Remove( int index )
		 {
			  VALUE value = InternalRemove( KeyAddress( index ) );
			  SizeConflict--;
			  return value;
		 }

		 protected internal virtual VALUE InternalRemove( long keyAddress )
		 {
			  UnsafeUtil.setMemory( keyAddress, _bytesPerKey, ( sbyte ) - 1 );
			  return ValueMarker;
		 }

		 public override long HopBits( int index )
		 {
			  return ~( UnsafeUtil.getInt( HopBitsAddress( index ) ) | 0xFFFFFFFF00000000L );
		 }

		 public override void PutHopBit( int index, int hd )
		 {
			  long adr = HopBitsAddress( index );
			  int hopBits = UnsafeUtil.getInt( adr );
			  hopBits &= ~( 1 << hd );
			  UnsafeUtil.putInt( adr, hopBits );
		 }

		 public override void MoveHopBit( int index, int hd, int delta )
		 {
			  long adr = HopBitsAddress( index );
			  int hopBits = UnsafeUtil.getInt( adr );
			  hopBits ^= ( 1 << hd ) | ( 1 << ( hd + delta ) );
			  UnsafeUtil.putInt( adr, hopBits );
		 }

		 protected internal virtual long KeyAddress( int index )
		 {
			  return _address + ( index * ( ( long ) _bytesPerEntry ) ) + 4;
		 }

		 protected internal virtual long HopBitsAddress( int index )
		 {
			  return _address + ( index * ( ( long ) _bytesPerEntry ) );
		 }

		 public override void RemoveHopBit( int index, int hd )
		 {
			  long adr = HopBitsAddress( index );
			  int hopBits = UnsafeUtil.getInt( adr );
			  hopBits |= 1 << hd;
			  UnsafeUtil.putInt( adr, hopBits );
		 }

		 public override void Close()
		 {
			  UnsafeUtil.free( _allocatedAddress, _allocatedBytes, AllocationTracker );
		 }

		 protected internal static void AlignmentSafePutLongAsTwoInts( long address, long value )
		 {
			  if ( UnsafeUtil.allowUnalignedMemoryAccess )
			  {
					UnsafeUtil.putLong( address, value );
			  }
			  else
			  {
					// See javadoc in constructor as to why we do this
					UnsafeUtil.putInt( address, ( int ) value );
					UnsafeUtil.putInt( address + Integer.BYTES, ( int )( ( long )( ( ulong )value >> ( sizeof( int ) * 8 ) ) ) );
			  }
		 }

		 protected internal static long AlignmentSafeGetLongAsTwoInts( long address )
		 {
			  if ( UnsafeUtil.allowUnalignedMemoryAccess )
			  {
					return UnsafeUtil.getLong( address );
			  }

			  // See javadoc in constructor as to why we do this
			  long lsb = UnsafeUtil.getInt( address ) & 0xFFFFFFFFL;
			  long msb = UnsafeUtil.getInt( address + Integer.BYTES ) & 0xFFFFFFFFL;
			  return lsb | ( msb << ( sizeof( int ) * 8 ) );
		 }
	}

}