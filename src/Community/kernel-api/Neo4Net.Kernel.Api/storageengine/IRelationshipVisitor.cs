using System;

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
namespace Neo4Net.Kernel.Api.StorageEngine
{
	/// <summary>
	/// Visitor of relationship data.
	/// </summary>
	/// @param <EXCEPTION> exception thrown from the <seealso cref="visit(long, int, long, long)"/> method. </param>
	public interface IRelationshipVisitor<EXCEPTION> where EXCEPTION : Exception
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

		 void Visit( long relationshipId, int typeId, long startNodeId, long endNodeId );
	}

	 public interface RelationshipVisitor_Home
	 {
		  bool relationshipVisit<EXCEPTION>( long relId, IRelationshipVisitor<EXCEPTION> visitor );
	 }

}