﻿/*
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
namespace Org.Neo4j.Hashing
{
	/// <seealso cref= HashFunction#javaUtilHashing() </seealso>
	internal class JavaUtilHashFunction : HashFunction
	{
		 internal static readonly HashFunction Instance = new JavaUtilHashFunction();

		 private JavaUtilHashFunction()
		 {
		 }

		 public override long Initialise( long seed )
		 {
			  return seed;
		 }

		 public override long Update( long intermediateHash, long value )
		 {
			  return HashSingleValueToInt( intermediateHash + value );
		 }

		 public override long Finalise( long intermediateHash )
		 {
			  return intermediateHash;
		 }

		 public override int HashSingleValueToInt( long value )
		 {
			  int h = ( int )( ( ( long )( ( ulong )value >> 32 ) ) ^ value );
			  h ^= ( ( int )( ( uint )h >> 20 ) ) ^ ( ( int )( ( uint )h >> 12 ) );
			  return h ^ ( ( int )( ( uint )h >> 7 ) ) ^ ( ( int )( ( uint )h >> 4 ) );
		 }
	}

}