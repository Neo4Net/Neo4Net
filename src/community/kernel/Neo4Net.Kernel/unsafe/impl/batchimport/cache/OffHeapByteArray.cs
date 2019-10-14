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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{
	using MemoryAllocationTracker = Neo4Net.Memory.MemoryAllocationTracker;
	using UnsafeUtil = Neo4Net.@unsafe.Impl.Internal.Dragons.UnsafeUtil;

	public class OffHeapByteArray : OffHeapNumberArray<ByteArray>, ByteArray
	{
		 private readonly sbyte[] _defaultValue;

		 protected internal OffHeapByteArray( long length, sbyte[] defaultValue, long @base, MemoryAllocationTracker allocationTracker ) : base( length, defaultValue.Length, @base, allocationTracker )
		 {
			  this._defaultValue = defaultValue;
			  Clear();
		 }

		 public override void Swap( long fromIndex, long toIndex )
		 {
			  int bytesLeft = itemSize;
			  long fromAddress = Address( fromIndex, 0 );
			  long toAddress = Address( toIndex, 0 );

			  // piece-wise swap, as large chunks as possible: long, int, short and finally byte-wise swap
			  while ( bytesLeft > 0 )
			  {
					int chunkSize;
					if ( bytesLeft >= Long.BYTES )
					{
						 chunkSize = Long.BYTES;
						 long intermediary = GetLong( fromAddress );
						 UnsafeUtil.copyMemory( toAddress, fromAddress, chunkSize );
						 PutLong( toAddress, intermediary );
					}
					else if ( bytesLeft >= Integer.BYTES )
					{
						 chunkSize = Integer.BYTES;
						 int intermediary = GetInt( fromAddress );
						 UnsafeUtil.copyMemory( toAddress, fromAddress, chunkSize );
						 PutInt( toAddress, intermediary );
					}
					else if ( bytesLeft >= Short.BYTES )
					{
						 chunkSize = Short.BYTES;
						 short intermediary = GetShort( fromAddress );
						 UnsafeUtil.copyMemory( toAddress, fromAddress, chunkSize );
						 PutShort( toAddress, intermediary );
					}
					else
					{
						 chunkSize = Byte.BYTES;
						 sbyte intermediary = GetByte( fromAddress );
						 UnsafeUtil.copyMemory( toAddress, fromAddress, chunkSize );
						 PutByte( toAddress, intermediary );
					}
					fromAddress += chunkSize;
					toAddress += chunkSize;
					bytesLeft -= chunkSize;
			  }
		 }

		 public override void Clear()
		 {
			  if ( IsByteUniform( _defaultValue ) )
			  {
					UnsafeUtil.setMemory( Address, LengthConflict * itemSize, _defaultValue[0] );
			  }
			  else
			  {
					long intermediary = UnsafeUtil.allocateMemory( itemSize, AllocationTracker );
					for ( int i = 0; i < _defaultValue.Length; i++ )
					{
						 UnsafeUtil.putByte( intermediary + i, _defaultValue[i] );
					}

					for ( long i = 0, adr = Address; i < LengthConflict; i++, adr += itemSize )
					{
						 UnsafeUtil.copyMemory( intermediary, adr, itemSize );
					}
					UnsafeUtil.free( intermediary, itemSize, AllocationTracker );
			  }
		 }

		 private bool IsByteUniform( sbyte[] bytes )
		 {
			  sbyte reference = bytes[0];
			  for ( int i = 1; i < bytes.Length; i++ )
			  {
					if ( reference != bytes[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public override void Get( long index, sbyte[] into )
		 {
			  long address = address( index, 0 );
			  for ( int i = 0; i < itemSize; i++, address++ )
			  {
					into[i] = UnsafeUtil.getByte( address );
			  }
		 }

		 public override sbyte GetByte( long index, int offset )
		 {
			  return UnsafeUtil.getByte( Address( index, offset ) );
		 }

		 private sbyte GetByte( long p )
		 {
			  return UnsafeUtil.getByte( p );
		 }

		 public override short GetShort( long index, int offset )
		 {
			  return GetShort( Address( index, offset ) );
		 }

		 private short GetShort( long p )
		 {
			  if ( UnsafeUtil.allowUnalignedMemoryAccess )
			  {
					return UnsafeUtil.getShort( p );
			  }

			  return UnsafeUtil.getShortByteWiseLittleEndian( p );
		 }

		 public override int GetInt( long index, int offset )
		 {
			  return GetInt( Address( index, offset ) );
		 }

		 private int GetInt( long p )
		 {
			  if ( UnsafeUtil.allowUnalignedMemoryAccess )
			  {
					return UnsafeUtil.getInt( p );
			  }

			  return UnsafeUtil.getIntByteWiseLittleEndian( p );
		 }

		 public override long Get5ByteLong( long index, int offset )
		 {
			  long address = address( index, offset );
			  long low4b = GetInt( address ) & 0xFFFFFFFFL;
			  long high1b = UnsafeUtil.getByte( address + Integer.BYTES ) & 0xFF;
			  long result = low4b | ( high1b << 32 );
			  return result == 0xFFFFFFFFFFL ? -1 : result;
		 }

		 public override long Get6ByteLong( long index, int offset )
		 {
			  long address = address( index, offset );
			  long low4b = GetInt( address ) & 0xFFFFFFFFL;
			  long high2b = GetShort( address + Integer.BYTES ) & 0xFFFF;
			  long result = low4b | ( high2b << 32 );
			  return result == 0xFFFFFFFFFFFFL ? -1 : result;
		 }

		 public override long GetLong( long index, int offset )
		 {
			  long p = Address( index, offset );
			  return GetLong( p );
		 }

		 private long GetLong( long p )
		 {
			  if ( UnsafeUtil.allowUnalignedMemoryAccess )
			  {
					return UnsafeUtil.getLong( p );
			  }

			  return UnsafeUtil.getLongByteWiseLittleEndian( p );
		 }

		 public override void Set( long index, sbyte[] value )
		 {
			  long address = address( index, 0 );
			  for ( int i = 0; i < itemSize; i++, address++ )
			  {
					UnsafeUtil.putByte( address, value[i] );
			  }
		 }

		 public override void SetByte( long index, int offset, sbyte value )
		 {
			  UnsafeUtil.putByte( Address( index, offset ), value );
		 }

		 private void PutByte( long p, sbyte value )
		 {
			  UnsafeUtil.putByte( p, value );
		 }

		 public override void SetShort( long index, int offset, short value )
		 {
			  PutShort( Address( index, offset ), value );
		 }

		 private void PutShort( long p, short value )
		 {
			  if ( UnsafeUtil.allowUnalignedMemoryAccess )
			  {
					UnsafeUtil.putShort( p, value );
			  }
			  else
			  {
					UnsafeUtil.putShortByteWiseLittleEndian( p, value );
			  }
		 }

		 public override void SetInt( long index, int offset, int value )
		 {
			  PutInt( Address( index, offset ), value );
		 }

		 private void PutInt( long p, int value )
		 {
			  if ( UnsafeUtil.allowUnalignedMemoryAccess )
			  {
					UnsafeUtil.putInt( p, value );
			  }
			  else
			  {
					UnsafeUtil.putIntByteWiseLittleEndian( p, value );
			  }
		 }

		 public override void Set5ByteLong( long index, int offset, long value )
		 {
			  long address = address( index, offset );
			  PutInt( address, ( int ) value );
			  UnsafeUtil.putByte( address + Integer.BYTES, ( sbyte )( ( long )( ( ulong )value >> 32 ) ) );
		 }

		 public override void Set6ByteLong( long index, int offset, long value )
		 {
			  long address = address( index, offset );
			  PutInt( address, ( int ) value );
			  PutShort( address + Integer.BYTES, ( short )( ( long )( ( ulong )value >> 32 ) ) );
		 }

		 public override void SetLong( long index, int offset, long value )
		 {
			  long p = Address( index, offset );
			  PutLong( p, value );
		 }

		 private void PutLong( long p, long value )
		 {
			  if ( UnsafeUtil.allowUnalignedMemoryAccess )
			  {
					UnsafeUtil.putLong( p, value );
			  }
			  else
			  {
					UnsafeUtil.putLongByteWiseLittleEndian( p, value );
			  }
		 }

		 public override int Get3ByteInt( long index, int offset )
		 {
			  long address = address( index, offset );
			  int lowWord = UnsafeUtil.getShort( address ) & 0xFFFF;
			  int highByte = UnsafeUtil.getByte( address + Short.BYTES ) & 0xFF;
			  int result = lowWord | ( highByte << ( sizeof( short ) * 8 ) );
			  return result == 0xFFFFFF ? -1 : result;
		 }

		 public override void Set3ByteInt( long index, int offset, int value )
		 {
			  long address = address( index, offset );
			  UnsafeUtil.putShort( address, ( short ) value );
			  UnsafeUtil.putByte( address + Short.BYTES, ( sbyte )( ( int )( ( uint )value >> ( sizeof( short ) * 8 ) ) ) );
		 }

		 private long Address( long index, int offset )
		 {
			  CheckBounds( index );
			  return Address + ( rebase( index ) * itemSize ) + offset;
		 }

		 private void CheckBounds( long index )
		 {
			  long rebased = rebase( index );
			  if ( rebased < 0 || rebased >= LengthConflict )
			  {
					throw new System.IndexOutOfRangeException( "Wanted to access " + rebased + " but range is " + @base + "-" + LengthConflict );
			  }
		 }
	}

}