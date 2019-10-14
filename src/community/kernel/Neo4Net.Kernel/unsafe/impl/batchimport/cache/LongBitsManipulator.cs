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

	using Bits = Neo4Net.Kernel.impl.util.Bits;

	/// <summary>
	/// Turns a long into 64 bits of memory where variables can be allocated in, for example:
	/// <pre>
	/// [eeee,eeee][dddd,dddd][dddd,dddd][dddd,cccc][bbbb,bbbb][bbbb,bbbb][bbaa,aaaa][aaaa,aaaa]
	/// </pre>
	/// Which has the variables a (14 bits), b (18 bits), c (4 bits), d (20 bits) and e (8 bits)
	/// </summary>
	public class LongBitsManipulator
	{
		 private class Slot
		 {
			  internal readonly long Mask;
			  internal readonly long MaxValue;
			  internal readonly int BitOffset;

			  internal Slot( int bits, long mask, int bitOffset )
			  {
					this.Mask = mask;
					this.BitOffset = bitOffset;
					this.MaxValue = ( 1L << bits ) - 1;
			  }

			  public virtual long Get( long field )
			  {
					long raw = field & Mask;
					return raw == Mask ? -1 : ( long )( ( ulong )raw >> BitOffset );
			  }

			  public virtual long Set( long field, long value )
			  {
					if ( value < -1 || value > MaxValue )
					{
						 throw new System.InvalidOperationException( "Invalid value " + value + ", max is " + MaxValue );
					}

					long otherBits = field & ~Mask;
					return ( ( value << BitOffset ) & Mask ) | otherBits;
			  }

			  public virtual long Clear( long field, bool trueForAllOnes )
			  {
					long otherBits = field & ~Mask;
					return trueForAllOnes ? otherBits | Mask : otherBits;
			  }

			  public override string ToString()
			  {
					return this.GetType().Name + "[" + Bits.numbersToBitString(new long[] { MaxValue << BitOffset }) + "]";
			  }
		 }

		 private readonly Slot[] _slots;

		 public LongBitsManipulator( params int[] slotsAndTheirBitCounts )
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
					long mask = ( 1L << bits ) - 1;
					mask <<= bitCursor;
					slots[i] = new Slot( bits, mask, bitCursor );
					bitCursor += bits;
			  }
			  return slots;
		 }

		 public virtual long Set( long field, int slotIndex, long value )
		 {
			  return Slot( slotIndex ).set( field, value );
		 }

		 public virtual long Get( long field, int slotIndex )
		 {
			  return Slot( slotIndex ).get( field );
		 }

		 public virtual long Clear( long field, int slotIndex, bool trueForAllOnes )
		 {
			  return Slot( slotIndex ).clear( field, trueForAllOnes );
		 }

		 public virtual long Template( params bool[] trueForOnes )
		 {
			  if ( trueForOnes.Length != _slots.Length )
			  {
					throw new System.ArgumentException( "Invalid boolean arguments, expected " + _slots.Length );
			  }

			  long field = 0;
			  for ( int i = 0; i < trueForOnes.Length; i++ )
			  {
					field = _slots[i].clear( field, trueForOnes[i] );
			  }
			  return field;
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