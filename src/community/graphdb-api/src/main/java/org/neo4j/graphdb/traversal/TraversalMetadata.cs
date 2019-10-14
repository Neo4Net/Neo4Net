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
namespace Neo4Net.Graphdb.traversal
{
	/// <summary>
	/// Provides metadata about a traversal.
	/// </summary>
	public interface TraversalMetadata
	{
		 /// <returns> number of paths returned up to this point in the traversal. </returns>
		 int NumberOfPathsReturned { get; }

		 /// <returns> number of relationships traversed up to this point in the traversal.
		 /// Some relationships in this counter might be unnecessarily traversed relationships,
		 /// but at the same time it gives an accurate measure of how many relationships are
		 /// requested from the underlying graph. Useful for comparing and first-level debugging
		 /// of queries. </returns>
		 int NumberOfRelationshipsTraversed { get; }
	}

}