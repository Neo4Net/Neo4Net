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
	/// <seealso cref= HashFunction#incrementalXXH64() </seealso>
	internal class IncrementalXXH64 : HashFunction
	{
		 internal static readonly HashFunction Instance = new IncrementalXXH64();

		 private const long PRIME1 = -7046029288634856825L;
		 private const long PRIME2 = -4417276706812531889L;
		 private const long PRIME3 = 1609587929392839161L;
		 private const long PRIME4 = -8796714831421723037L;
		 private const long PRIME5 = 2870177450012600261L;

		 private IncrementalXXH64()
		 {
		 }

		 public override long Initialise( long seed )
		 {
			  return seed + PRIME5;
		 }

		 public override long Update( long hash, long block )
		 {
			  hash += 8;
			  block *= PRIME2;
			  block = Long.rotateLeft( block, 31 );
			  block *= PRIME1;
			  hash ^= block;
			  hash = Long.rotateLeft( hash, 27 ) * PRIME1 + PRIME4;
			  return hash;
		 }

		 public override long Finalise( long hash )
		 {
			  hash ^= ( long )( ( ulong )hash >> 33 );
			  hash *= PRIME2;
			  hash ^= ( long )( ( ulong )hash >> 29 );
			  hash *= PRIME3;
			  hash ^= ( long )( ( ulong )hash >> 32 );
			  return hash;
		 }
	}

}