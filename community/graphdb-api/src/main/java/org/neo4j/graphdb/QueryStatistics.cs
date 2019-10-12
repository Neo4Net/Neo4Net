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
namespace Org.Neo4j.Graphdb
{
	/// <summary>
	/// Represents statistics about the effects of a query.
	/// 
	/// If the query did not perform any <seealso cref="containsUpdates() updates"/>, all the methods of this interface will return
	/// {@code 0}.
	/// </summary>
	public interface QueryStatistics
	{
		 // NOTE: If you change this interface, be sure to update bolt

		 /// <summary>
		 /// Returns the number of nodes created by this query.
		 /// </summary>
		 /// <returns> the number of nodes created by this query. </returns>
		 int NodesCreated { get; }

		 /// <summary>
		 /// Returns the number of nodes deleted by this query.
		 /// </summary>
		 /// <returns> the number of nodes deleted by this query. </returns>
		 int NodesDeleted { get; }

		 /// <summary>
		 /// Returns the number of relationships created by this query.
		 /// </summary>
		 /// <returns> the number of relationships created by this query. </returns>
		 int RelationshipsCreated { get; }

		 /// <summary>
		 /// Returns the number of relationships deleted by this query.
		 /// </summary>
		 /// <returns> the number of relationships deleted by this query. </returns>
		 int RelationshipsDeleted { get; }

		 /// <summary>
		 /// Returns the number of properties set by this query. Setting a property to the same value again still counts
		 /// towards this.
		 /// </summary>
		 /// <returns> the number of properties set by this query. </returns>
		 int PropertiesSet { get; }

		 /// <summary>
		 /// Returns the number of labels added to any node by this query.
		 /// </summary>
		 /// <returns> the number of labels added to any node by this query. </returns>
		 int LabelsAdded { get; }

		 /// <summary>
		 /// Returns the number of labels removed from any node by this query.
		 /// </summary>
		 /// <returns> the number of labels removed from any node by this query. </returns>
		 int LabelsRemoved { get; }

		 /// <summary>
		 /// Returns the number of indexes added by this query.
		 /// </summary>
		 /// <returns> the number of indexes added by this query. </returns>
		 int IndexesAdded { get; }

		 /// <summary>
		 /// Returns the number of indexes removed by this query.
		 /// </summary>
		 /// <returns> the number of indexes removed by this query. </returns>
		 int IndexesRemoved { get; }

		 /// <summary>
		 /// Returns the number of constraints added by this query.
		 /// </summary>
		 /// <returns> the number of constraints added by this query. </returns>
		 int ConstraintsAdded { get; }

		 /// <summary>
		 /// Returns the number of constraints removed by this query.
		 /// </summary>
		 /// <returns> the number of constraints removed by this query. </returns>
		 int ConstraintsRemoved { get; }

		 /// <summary>
		 /// If the query updated the graph in any way, this method will return true.
		 /// </summary>
		 /// <returns> if the graph has been updated. </returns>
		 bool ContainsUpdates();
	}

}