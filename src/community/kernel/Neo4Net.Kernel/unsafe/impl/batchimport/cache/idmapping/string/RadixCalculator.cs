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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string
{
	using MutableInt = org.apache.commons.lang3.mutable.MutableInt;

	/// <summary>
	/// Calculates the radix of <seealso cref="Long"/> values.
	/// </summary>
	public abstract class RadixCalculator
	{
		 protected internal const int RADIX_BITS = 24;
		 protected internal const long LENGTH_BITS = unchecked( ( long )0xFE000000_00000000L );
		 protected internal static readonly int LengthMask = ( int )( ( long )( ( ulong )LENGTH_BITS >> ( 64 - RADIX_BITS ) ) );
		 protected internal static readonly int HashcodeMask = ( int )( ( long )( ( ulong )0x00FFFF00_00000000L >> ( 64 - RADIX_BITS ) ) );

		 public abstract int RadixOf( long value );

		 /// <summary>
		 /// Radix optimized for strings encoded into long by <seealso cref="StringEncoder"/>.
		 /// </summary>
		 public class String : RadixCalculator
		 {
			  public override int RadixOf( long value )
			  {
					int index = ( int )( ( long )( ( ulong )value >> ( 64 - RADIX_BITS ) ) );
					index = ( ( int )( ( uint )( index & LengthMask ) >> 1 ) ) | ( index & HashcodeMask );
					return index;
			  }
		 }

		 /// <summary>
		 /// Radix optimized for strings encoded into long by <seealso cref="LongEncoder"/>.
		 /// </summary>
		 public class Long : RadixCalculator
		 {
			  internal readonly MutableInt RadixShift;

			  public Long( MutableInt radixShift )
			  {
					this.RadixShift = radixShift;
			  }

			  public override int RadixOf( long value )
			  {
					long val1 = value & ~LENGTH_BITS;
					val1 = ( long )( ( ulong )val1 >> RadixShift.intValue() );
					int index = ( int ) val1;
					return index;
			  }
		 }
	}

}