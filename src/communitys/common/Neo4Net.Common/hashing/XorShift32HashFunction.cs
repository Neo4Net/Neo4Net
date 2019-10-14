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
namespace Neo4Net.Hashing
{
	/// <seealso cref= HashFunction#xorShift32() </seealso>
	internal class XorShift32HashFunction : HashFunction
	{
		 internal static readonly XorShift32HashFunction Instance = new XorShift32HashFunction();

		 private XorShift32HashFunction()
		 {
		 }

		 public override long Initialize( long seed )
		 {
			  return 0;
		 }

		 public override long Update( long intermediateHash, long value )
		 {
			  return HashSingleValueToInt( intermediateHash + value );
		 }

		 public override long Finalize( long intermediateHash )
		 {
			  return intermediateHash;
		 }

		 public override int HashSingleValueToInt( long value )
		 {
			  value ^= value << 21;
			  value ^= ( long )( ( ulong )value >> 35 );
			  value ^= value << 4;
			  return ( int )( ( ( long )( ( ulong )value >> 32 ) ) ^ value );
		 }
	}

}