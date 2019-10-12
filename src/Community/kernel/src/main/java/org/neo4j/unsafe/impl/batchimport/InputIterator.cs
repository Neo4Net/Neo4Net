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
namespace Neo4Net.@unsafe.Impl.Batchimport
{

	using Neo4Net.Graphdb;
	using Input = Neo4Net.@unsafe.Impl.Batchimport.input.Input;
	using InputChunk = Neo4Net.@unsafe.Impl.Batchimport.input.InputChunk;

	/// <summary>
	/// A <seealso cref="ResourceIterator"/> with added methods suitable for <seealso cref="Input"/> into a <seealso cref="BatchImporter"/>.
	/// </summary>
	public interface InputIterator : System.IDisposable
	{
		 InputChunk NewChunk();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean next(org.neo4j.unsafe.impl.batchimport.input.InputChunk chunk) throws java.io.IOException;
		 bool Next( InputChunk chunk );
	}

}