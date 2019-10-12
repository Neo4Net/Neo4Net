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
namespace Neo4Net.Storageengine.Api
{

	/// <summary>
	/// Reads <seealso cref="StorageCommand commands"/> from a <seealso cref="ReadableChannel channel"/>.
	/// Instances must handle concurrent threads calling it with potentially different channels.
	/// </summary>
	public interface CommandReader
	{
		 /// <summary>
		 /// Reads the next <seealso cref="StorageCommand"/> from <seealso cref="ReadableChannel channel"/>.
		 /// </summary>
		 /// <param name="channel"> <seealso cref="ReadableChannel"/> to read from. </param>
		 /// <returns> <seealso cref="StorageCommand"/> or {@code null} if end reached. </returns>
		 /// <exception cref="IOException"> if channel throws exception. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: StorageCommand read(ReadableChannel channel) throws java.io.IOException;
		 StorageCommand Read( ReadableChannel channel );
	}

}