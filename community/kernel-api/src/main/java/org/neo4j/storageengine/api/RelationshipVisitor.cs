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
namespace Org.Neo4j.Storageengine.Api
{
	/// <summary>
	/// Visitor of relationship data.
	/// </summary>
	/// @param <EXCEPTION> exception thrown from the <seealso cref="visit(long, int, long, long)"/> method. </param>
	public interface RelationshipVisitor<EXCEPTION> where EXCEPTION : Exception
	{
		 /// <summary>
		 /// Objects which can accept these <seealso cref="RelationshipVisitor visitors"/> should implement this interface.
		 /// </summary>

		 /// <summary>
		 /// Visits data about a relationship.
		 /// </summary>
		 /// <param name="relationshipId"> relationship id to visit data for. </param>
		 /// <param name="typeId"> relationship type id for the relationship. </param>
		 /// <param name="startNodeId"> id of start node of the relationship. </param>
		 /// <param name="endNodeId"> id of the end node of the relationship. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void visit(long relationshipId, int typeId, long startNodeId, long endNodeId) throws EXCEPTION;
		 void Visit( long relationshipId, int typeId, long startNodeId, long endNodeId );
	}

	 public interface RelationshipVisitor_Home
	 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: <EXCEPTION extends Exception> boolean relationshipVisit(long relId, RelationshipVisitor<EXCEPTION> visitor) throws EXCEPTION;
		  bool relationshipVisit<EXCEPTION>( long relId, RelationshipVisitor<EXCEPTION> visitor );
	 }

}