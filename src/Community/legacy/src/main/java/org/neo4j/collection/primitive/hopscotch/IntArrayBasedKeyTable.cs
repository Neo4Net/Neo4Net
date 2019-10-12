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
namespace Neo4Net.Collection.primitive.hopscotch
{

	/// <summary>
	/// Table implementation for handling primitive int/long keys and hop bits. The quantized unit is int so a
	/// multiple of ints will be used for every entry.
	/// <para>
	/// In this class, <code>index</code> refers to the index of an entry (key + value + hop bits), while
	/// <code>address</code> refers to the position of an int word in the internal <code>table</code>.
	/// 
	/// </para>
	/// </summary>
	/// @param <VALUE> essentially ignored, since no values are stored in this table. Although subclasses can. </param>
	public abstract class IntArrayBasedKeyTable<VALUE> : PowerOfTwoQuantizedTable<VALUE>
	{
		 protected internal int[] Table;
		 protected internal readonly VALUE SingleValue; // used as a pointer to pass around a primitive value in concrete subclasses
		 private readonly int _itemsPerEntry;

		 protected internal IntArrayBasedKeyTable( int capacity, int itemsPerEntry, int h, VALUE singleValue ) : base( capacity, h )
		 {
			  this.SingleValue = singleValue;
			  this._itemsPerEntry = itemsPerEntry;
			  InitializeTable();
			  ClearTable();
		 }

		 protected internal virtual void InitializeTable()
		 {
			  this.Table = new int[CapacityConflict * _itemsPerEntry];
		 }

		 protected internal virtual void ClearTable()
		 {
			  fill( Table, -1 );
		 }

		 protected internal virtual long PutLong( int actualIndex, long value )
		 {
			  long previous = GetLong( actualIndex );
			  Table[actualIndex] = ( int ) value;
			  Table[actualIndex + 1] = ( int )( ( int )( ( uint )( value & 0xFFFFFFFF00000000L ) >> 32 ) );
			  return previous;
		 }

		 protected internal virtual long GetLong( int actualIndex )
		 {
			  long low = Table[actualIndex] & 0xFFFFFFFFL;
			  long high = Table[actualIndex + 1] & 0xFFFFFFFFL;
			  return ( high << 32 ) | low;
		 }

		 public override void Put( int index, long key, VALUE value )
		 {
			  int address = address( index );
			  InternalPut( address, key, value );
			  SizeConflict++;
		 }

		 public override VALUE Remove( int index )
		 {
			  int address = address( index );
			  VALUE value = value( index );
			  InternalRemove( address );
			  SizeConflict--;
			  return value;
		 }

		 public override long Move( int fromIndex, int toIndex )
		 {
			  long key = key( fromIndex );
			  int fromAddress = Address( fromIndex );
			  int toAddress = Address( toIndex );
			  for ( int i = 0; i < _itemsPerEntry - 1; i++ )
			  {
					int tempValue = Table[fromAddress + i];
					Table[fromAddress + i] = Table[toAddress + i];
					Table[toAddress + i] = tempValue;
			  }
			  return key;
		 }

		 protected internal virtual void InternalRemove( int actualIndex )
		 {
			  Arrays.fill( Table, actualIndex, actualIndex + _itemsPerEntry - 1, -1 );
		 }

		 protected internal abstract void InternalPut( int actualIndex, long key, VALUE value );

		 public override VALUE Value( int index )
		 {
			  return SingleValue;
		 }

		 public override VALUE PutValue( int index, VALUE value )
		 {
			  return value;
		 }

		 public override long HopBits( int index )
		 {
			  return ~( Table[Address( index ) + _itemsPerEntry - 1] | 0xFFFFFFFF00000000L );
		 }

		 private int HopBit( int hd )
		 {
			  return 1 << hd;
		 }

		 public override void PutHopBit( int index, int hd )
		 {
			  Table[Address( index ) + _itemsPerEntry - 1] &= ~HopBit( hd );
		 }

		 public override void MoveHopBit( int index, int hd, int delta )
		 {
			  Table[Address( index ) + _itemsPerEntry - 1] ^= HopBit( hd ) | HopBit( hd + delta );
		 }

		 public override void RemoveHopBit( int index, int hd )
		 {
			  Table[Address( index ) + _itemsPerEntry - 1] |= HopBit( hd );
		 }

		 protected internal virtual int Address( int index )
		 {
			  return index * _itemsPerEntry;
		 }

		 public override void Clear()
		 {
			  if ( !Empty )
			  {
					ClearTable();
			  }
			  base.Clear();
		 }
	}

}