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
namespace Neo4Net.Index.@internal.gbptree
{
	/// <summary>
	/// Logic for composing and decomposing stable/unstable generation number (unsigned int) to/from a single {@code long}.
	/// 
	/// <pre>
	/// long: [S,S,S,S,U,U,U,U]
	///       msb <-------> lsb
	/// </pre>
	/// </summary>
	internal class Generation
	{
		 private const long UNSTABLE_GENERATION_MASK = 0xFFFFFFFFL;
		 private static readonly int _stableGenerationShift = ( sizeof( int ) * 8 );

		 private Generation()
		 {
		 }

		 /// <summary>
		 /// Takes one stable and one unstable generation (both unsigned ints) and crams them into one {@code long}.
		 /// </summary>
		 /// <param name="stableGeneration"> stable generation. </param>
		 /// <param name="unstableGeneration"> unstable generation. </param>
		 /// <returns> the two generation numbers as one {@code long}. </returns>
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static long GenerationConflict( long stableGeneration, long unstableGeneration )
		 {
			  GenerationSafePointer.AssertGenerationOnWrite( stableGeneration );
			  GenerationSafePointer.AssertGenerationOnWrite( unstableGeneration );

			  return ( stableGeneration << _stableGenerationShift ) | unstableGeneration;
		 }

		 /// <summary>
		 /// Extracts and returns unstable generation from generation {@code long}.
		 /// </summary>
		 /// <param name="generation"> generation variable containing both stable and unstable generations. </param>
		 /// <returns> unstable generation from generation. </returns>
		 public static long UnstableGeneration( long generation )
		 {
			  return generation & UNSTABLE_GENERATION_MASK;
		 }

		 /// <summary>
		 /// Extracts and returns stable generation from generation {@code long}.
		 /// </summary>
		 /// <param name="generation"> generation variable containing both stable and unstable generations. </param>
		 /// <returns> stable generation from generation. </returns>
		 public static long StableGeneration( long generation )
		 {
			  return ( long )( ( ulong )generation >> _stableGenerationShift );
		 }
	}

}