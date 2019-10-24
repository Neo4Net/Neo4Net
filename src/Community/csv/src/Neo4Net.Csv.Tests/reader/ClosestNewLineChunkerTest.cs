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
namespace Neo4Net.Csv.Reader
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.copyOfRange;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class ClosestNewLineChunkerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBackUpChunkToClosestNewline() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldBackUpChunkToClosestNewline()
		 {
			  // GIVEN
			  CharReadable reader = Readables.Wrap( "1234567\n8901234\n5678901234" );
			  // (next chunks):                                   ^            ^
			  // (actual chunks):                             ^        ^
			  using ( ClosestNewLineChunker source = new ClosestNewLineChunker( reader, 12 ) )
			  {
					// WHEN
					Source_Chunk chunk = source.NewChunk();
					assertTrue( source.NextChunk( chunk ) );
					assertArrayEquals( "1234567\n".ToCharArray(), CharactersOf(chunk) );
					assertTrue( source.NextChunk( chunk ) );
					assertArrayEquals( "8901234\n".ToCharArray(), CharactersOf(chunk) );
					assertTrue( source.NextChunk( chunk ) );
					assertArrayEquals( "5678901234".ToCharArray(), CharactersOf(chunk) );

					// THEN
					assertFalse( source.NextChunk( chunk ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailIfNoNewlineInChunk() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldFailIfNoNewlineInChunk()
		 {
			  // GIVEN
			  CharReadable reader = Readables.Wrap( "1234567\n89012345678901234" );
			  // (next chunks):                                   ^
			  // (actual chunks):                             ^
			  using ( ClosestNewLineChunker source = new ClosestNewLineChunker( reader, 12 ) )
			  {
					// WHEN
					Source_Chunk chunk = source.NewChunk();
					assertTrue( source.NextChunk( chunk ) );
					assertArrayEquals( "1234567\n".ToCharArray(), CharactersOf(chunk) );
					assertThrows( typeof( System.InvalidOperationException ), () => assertFalse(source.NextChunk(chunk)) );
			  }
		 }

		 private static char[] CharactersOf( Source_Chunk chunk )
		 {
			  return copyOfRange( chunk.Data(), chunk.StartPosition(), chunk.StartPosition() + chunk.Length() );
		 }
	}

}