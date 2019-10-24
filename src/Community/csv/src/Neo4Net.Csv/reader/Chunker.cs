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

	/// <summary>
	/// Takes a bigger stream of data and chunks it up into smaller chunks. The <seealso cref="Chunk chunks"/> are allocated
	/// explicitly and are passed into <seealso cref="nextChunk(Chunk)"/> to be filled/assigned with data representing
	/// next chunk from the stream. This design allows for efficient reuse of chunks when there are multiple concurrent
	/// processors, each processing chunks of data.
	/// </summary>
	public interface IChunker : System.IDisposable
	{
		 /// <returns> a new allocated <seealso cref="Chunk"/> which is to be later passed into <seealso cref="nextChunk(Chunk)"/>
		 /// to fill it with data. When a <seealso cref="Chunk"/> has been fully processed then it can be passed into
		 /// <seealso cref="nextChunk(Chunk)"/> again to get more data. </returns>
		 Source_Chunk NewChunk();

		 /// <summary>
		 /// Fills a previously <seealso cref="newChunk() allocated chunk"/> with data to be processed after completion
		 /// of this call.
		 /// </summary>
		 /// <param name="chunk"> <seealso cref="Chunk"/> to fill with data. </param>
		 /// <returns> {@code true} if at least some amount of data was passed into the given <seealso cref="Chunk"/>,
		 /// otherwise {@code false} denoting the end of the stream. </returns>
		 /// <exception cref="IOException"> on I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean nextChunk(org.Neo4Net.csv.reader.Source_Chunk chunk) throws java.io.IOException;
		 bool NextChunk( Source_Chunk chunk );

		 /// <returns> byte position of how much data has been returned from <seealso cref="nextChunk(Chunk)"/>. </returns>
		 long Position();
	}

}