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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.highestOneBit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;

	/// <summary>
	/// Contains the basic table capacity- and size calculations. Always keeps the capacity a power of 2
	/// for an efficient table mask.
	/// </summary>
	public abstract class PowerOfTwoQuantizedTable<VALUE> : Table<VALUE>
	{
		public abstract void RemoveHopBit( int index, int hd );
		public abstract void MoveHopBit( int index, int hd, int delta );
		public abstract void PutHopBit( int index, int hd );
		public abstract long HopBits( int index );
		public abstract VALUE Remove( int index );
		public abstract long Move( int fromIndex, int toIndex );
		public abstract VALUE PutValue( int index, VALUE value );
		public abstract void Put( int index, long key, VALUE value );
		public abstract VALUE Value( int index );
		public abstract long Key( int index );
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly int HConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly int CapacityConflict;
		 protected internal int TableMask;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal int SizeConflict;

		 protected internal PowerOfTwoQuantizedTable( int capacity, int h )
		 {
			  if ( h < 4 || h > 32 )
			  {
					throw new System.ArgumentException( "h needs to be 4 <= h <= 32, was " + h );
			  }

			  this.HConflict = h;
			  this.CapacityConflict = Quantize( max( capacity, 2 ) );
			  this.TableMask = highestOneBit( this.CapacityConflict ) - 1;
		 }

		 public static int BaseCapacity( int h )
		 {
			  return h << 1;
		 }

		 protected internal static int Quantize( int capacity )
		 {
			  int candidate = Integer.highestOneBit( capacity );
			  return candidate == capacity ? candidate : candidate << 1;
		 }

		 public override int H()
		 {
			  return HConflict;
		 }

		 public override int Mask()
		 {
			  return TableMask;
		 }

		 public override int Capacity()
		 {
			  return CapacityConflict;
		 }

		 public virtual bool Empty
		 {
			 get
			 {
				  return SizeConflict == 0;
			 }
		 }

		 public override int Size()
		 {
			  return SizeConflict;
		 }

		 public override void Clear()
		 {
			  SizeConflict = 0;
		 }

		 public override int Version()
		 { // Versioning not supported by default
			  return 0;
		 }

		 public override int Version( int index )
		 { // Versioning not supported by default
			  return 0;
		 }

		 public override long NullKey()
		 {
			  return -1L;
		 }

		 public override Table<VALUE> Grow()
		 {
			  return NewInstance( CapacityConflict << 1 );
		 }

		 protected internal abstract Table<VALUE> NewInstance( int newCapacity );

		 public override string ToString()
		 {
			  return format( "hopscotch-table[%s|capacity:%d, size:%d, usage:%f]", this.GetType().Name, CapacityConflict, SizeConflict, SizeConflict / ((double) CapacityConflict) );
		 }

		 public override void Close()
		 { // Nothing to do by default
		 }
	}

}