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
namespace Neo4Net.Consistency.checking
{

	using Bits = Neo4Net.Kernel.impl.util.Bits;
	using ByteArray = Neo4Net.@unsafe.Impl.Batchimport.cache.ByteArray;

	/// <summary>
	/// Uses a <seealso cref="ByteArray"/> and can conveniently split up an index into slots, not only per byte, but arbitrary bit-sizes,
	/// e.g. two 40-bit fields and eight 1-bit fields. To favor simplicity there are two types of fields: booleans and long fields.
	/// Boolean fields are 1-bit fields that can be anywhere in the byte[] index, but multi-bit fields must start at the beginning of
	/// a byte in the index.
	/// </summary>
	public class ByteArrayBitsManipulator
	{
		 public const int MAX_BYTES = 11;
		 private static readonly int _maxBits = MAX_BYTES * ( sizeof( sbyte ) * 8 );
		 public static readonly int MaxSlotBits = 5 * ( sizeof( sbyte ) * 8 );
		 internal static readonly long MaxSlotValue = ( 1L << MaxSlotBits ) - 1;

		 private class Slot
		 {
			  internal readonly int ByteOffset;
			  internal readonly int BitOffset;
			  internal readonly int BitCount;
			  internal readonly long Mask;
			  internal readonly int FbMask;

			  internal Slot( int bits, int absoluteBitOffset )
			  {
					this.ByteOffset = absoluteBitOffset / ( sizeof( sbyte ) * 8 );
					this.BitOffset = absoluteBitOffset % ( sizeof( sbyte ) * 8 );
					this.BitCount = bits;
					this.Mask = ( 1L << bits ) - 1;
					this.FbMask = 1 << BitOffset;
			  }

			  public virtual long Get( ByteArray array, long index )
			  {
					// Basically two modes: boolean or 5B field, right?
					if ( BitCount == 1 )
					{
						 int field = array.GetByte( index, ByteOffset ) & 0xFF;
						 bool bitIsSet = ( field & FbMask ) != 0;
						 return bitIsSet ? -1 : 0; // the -1 here is a bit weird, but for the time being this is what the rest of the code expects
					}
					else // we know that this larger field starts at the beginning of a byte
					{
						 long field = array.Get5ByteLong( index, ByteOffset );
						 long raw = field & Mask;
						 return raw == Mask ? -1 : raw;
					}
			  }

			  public virtual void Set( ByteArray array, long index, long value )
			  {
					if ( value < -1 || value > Mask )
					{
						 throw new System.InvalidOperationException( "Invalid value " + value + ", max is " + Mask );
					}

					if ( BitCount == 1 )
					{
						 int field = array.GetByte( index, ByteOffset ) & 0xFF;
						 int otherBits = field & ~FbMask;
						 array.SetByte( index, ByteOffset, ( sbyte )( otherBits | ( value << BitOffset ) ) );
					}
					else
					{
						 long field = array.Get5ByteLong( index, ByteOffset );
						 long otherBits = field & ~Mask;
						 array.Set5ByteLong( index, ByteOffset, value | otherBits );
					}
			  }

			  public override string ToString()
			  {
					return this.GetType().Name + "[" + Bits.numbersToBitString(new long[] { Mask << BitOffset }) + "]";
			  }
		 }

		 private readonly Slot[] _slots;

		 public ByteArrayBitsManipulator( params int[] slotsAndTheirBitCounts )
		 {
			  _slots = IntoSlots( slotsAndTheirBitCounts );
		 }

		 private Slot[] IntoSlots( int[] slotsAndTheirSizes )
		 {
			  Slot[] slots = new Slot[slotsAndTheirSizes.Length];
			  int bitCursor = 0;
			  for ( int i = 0; i < slotsAndTheirSizes.Length; i++ )
			  {
					int bits = slotsAndTheirSizes[i];
					if ( bits > 1 && bitCursor % ( sizeof( sbyte ) * 8 ) != 0 )
					{
						 throw new System.ArgumentException( "Larger slots, i.e. size > 1 needs to be placed at the beginning of a byte" );
					}
					if ( bits > MaxSlotBits )
					{
						 throw new System.ArgumentException( "Too large slot " + bits + ", biggest allowed " + MaxSlotBits );
					}
					slots[i] = new Slot( bits, bitCursor );
					bitCursor += bits;
			  }
			  if ( bitCursor > _maxBits )
			  {
					throw new System.ArgumentException( "Max number of bits is " + _maxBits );
			  }
			  return slots;
		 }

		 public virtual void Set( ByteArray array, long index, int slotIndex, long value )
		 {
			  Slot( slotIndex ).set( array, index, value );
		 }

		 public virtual long Get( ByteArray array, long index, int slotIndex )
		 {
			  return Slot( slotIndex ).get( array, index );
		 }

		 private Slot Slot( int slotIndex )
		 {
			  if ( slotIndex < 0 || slotIndex >= _slots.Length )
			  {
					throw new System.ArgumentException( "Invalid slot " + slotIndex + ", I've got " + this );
			  }
			  return _slots[slotIndex];
		 }

		 public override string ToString()
		 {
			  return Arrays.ToString( _slots );
		 }
	}

}