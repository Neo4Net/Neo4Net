using System;

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
namespace Org.Neo4j.Collection.primitive
{
	public interface PrimitiveLongObjectVisitor<VALUE, E> where E : Exception
	{
		 /// <summary>
		 /// Visit the given entry.
		 /// </summary>
		 /// <param name="key"> The key of the entry. </param>
		 /// <param name="value"> The value of the entry. </param>
		 /// <returns> 'true' to signal that the iteration should be stopped, 'false' to signal that the iteration should
		 /// continue if there are more entries to look at. </returns>
		 /// <exception cref="E"> any thrown exception of type 'E' will bubble up through the 'visit' method. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean visited(long key, VALUE value) throws E;
		 bool Visited( long key, VALUE value );
	}

}