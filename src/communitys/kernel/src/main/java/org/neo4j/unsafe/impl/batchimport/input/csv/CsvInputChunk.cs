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
namespace Neo4Net.@unsafe.Impl.Batchimport.input.csv
{

	using Chunker = Neo4Net.Csv.Reader.Chunker;

	/// <summary>
	/// <seealso cref="InputChunk"/> that gets data from <seealso cref="Chunker"/>. Making it explicit in the interface simplifies implementation
	/// where there are different types of <seealso cref="Chunker"/> for different scenarios.
	/// </summary>
	public interface CsvInputChunk : InputChunk
	{
		 /// <summary>
		 /// Fills this <seealso cref="InputChunk"/> from the given <seealso cref="Chunker"/>.
		 /// </summary>
		 /// <param name="chunker"> to read next chunk from. </param>
		 /// <returns> {@code true} if there was data read, otherwise {@code false}, meaning end of stream. </returns>
		 /// <exception cref="IOException"> on I/O read error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean fillFrom(org.neo4j.csv.reader.Chunker chunker) throws java.io.IOException;
		 bool FillFrom( Chunker chunker );
	}

}