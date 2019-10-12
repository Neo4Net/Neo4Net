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
namespace Neo4Net.Kernel.impl.index.labelscan
{
	/// <summary>
	/// A small bit set of maximum 64 bits. Used in <seealso cref="LabelScanLayout"/>.
	/// </summary>
	internal class LabelScanValue
	{
		 internal static readonly int RangeSize = ( sizeof( long ) * 8 );
		 internal static readonly int RangeSizeBytes = Long.BYTES;

		 /// <summary>
		 /// Small bit set.
		 /// </summary>
		 internal long Bits;

		 /// <summary>
		 /// Sets bit at given {@code index}, where {@code index=0} is the lowest index, {@code index=63} the highest.
		 /// </summary>
		 /// <param name="index"> index into the bit set of the bit to set. </param>
		 internal virtual LabelScanValue Set( int index )
		 {
			  long mask = 1L << index;
			  Bits |= mask;
			  return this;
		 }

		 /// <summary>
		 /// Adds all bits from {@code other} to this bit set.
		 /// Result is a union of the two bit sets. {@code other} is kept intact.
		 /// </summary>
		 /// <param name="other"> value containing bits to add. </param>
		 /// <returns> this instance, now with added bits from {@code other}. </returns>
		 internal virtual LabelScanValue Add( LabelScanValue other )
		 {
			  Bits |= other.Bits;
			  return this;
		 }

		 /// <summary>
		 /// Removes all bits in {@code other} from this bit set.
		 /// Result is bits in this set before the call with all bits from {@code other} removed.
		 /// {@code other} is kept intact.
		 /// </summary>
		 /// <param name="other"> value containing bits to remove. </param>
		 /// <returns> this instance, now with removed bits from {@code other}. </returns>
		 internal virtual LabelScanValue Remove( LabelScanValue other )
		 {
			  Bits &= ~other.Bits;
			  return this;
		 }

		 /// <summary>
		 /// Clears all bits in this bit set.
		 /// </summary>
		 internal virtual void Clear()
		 {
			  Bits = 0;
		 }

		 public override string ToString()
		 {
			  return Bits.ToString();
		 }
	}

}