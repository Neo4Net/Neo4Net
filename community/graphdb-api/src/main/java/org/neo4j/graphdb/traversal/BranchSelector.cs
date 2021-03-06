﻿/*
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
namespace Org.Neo4j.Graphdb.traversal
{
	/// <summary>
	/// Decides "where to go next" in a traversal. It keeps state itself, f.ex. its
	/// own current position. Examples of implementations are "depth first" and
	/// "breadth first". This is an interface to implement if you'd like to implement
	/// f.ex. a "best first" selector based on your own criteria.
	/// </summary>
	public interface BranchSelector
	{
		 /// <summary>
		 /// Decides the next position ("where to go from here") from the current
		 /// position, based on the {@code rules}. Since <seealso cref="TraversalBranch"/>
		 /// has the <seealso cref="TraversalBranch.endNode()"/> of the position and the
		 /// <seealso cref="TraversalBranch.lastRelationship()"/> to how it got there, decisions
		 /// can be based on the current expansion source and the given rules.
		 /// </summary>
		 /// <param name="metadata"> the context for the traversal </param>
		 /// <returns> the next position based on the current position and the
		 /// {@code rules} of the traversal. </returns>
		 TraversalBranch Next( TraversalContext metadata );
	}

}