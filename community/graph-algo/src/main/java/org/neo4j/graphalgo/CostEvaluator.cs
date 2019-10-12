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
namespace Org.Neo4j.Graphalgo
{
	using Direction = Org.Neo4j.Graphdb.Direction;
	using Relationship = Org.Neo4j.Graphdb.Relationship;

	/// <summary>
	/// In order to make the solving of shortest path problems as general as
	/// possible, the algorithms accept objects handling all relevant tasks regarding
	/// costs of paths. This allows the user to represent the costs in any possible
	/// way, and to calculate them in any way. The usual case is numbers that we just
	/// add together, but what if we have for example factors we would like to
	/// multiply instead? This is handled by this system, which works as follows. A
	/// CostEvaluator is used to get the cost for a single relationship. These costs
	/// are then added through a CostAccumulator. Costs for alternative paths are
	/// compared with a common java.util.Comparator.
	/// 
	/// @author Patrik Larsson </summary>
	/// @param <T> The data type the edge weights are represented by. </param>
	public interface CostEvaluator<T>
	{
		 /// <summary>
		 /// This is the general method for looking up costs for relationships. This
		 /// can do anything, like looking up a property or running some small
		 /// calculation.
		 /// </summary>
		 /// <param name="relationship"> the relationship to get the cost for </param>
		 /// <param name="direction"> The direction in which the relationship is being
		 ///            evaluated, either <seealso cref="Direction.INCOMING"/> or
		 ///            <seealso cref="Direction.OUTGOING"/>. </param>
		 /// <returns> The cost for this edge/relationship </returns>
		 T GetCost( Relationship relationship, Direction direction );
	}

}