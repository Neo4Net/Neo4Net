﻿/*
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
namespace Neo4Net.Graphalgo
{
	using Node = Neo4Net.Graphdb.Node;

	/// <summary>
	/// Evaluator used to estimate the weight of the remaining path from one node to
	/// another.
	/// 
	/// @author Mattias Persson </summary>
	/// @param <T> The data type of the estimated weight. </param>
	public interface EstimateEvaluator<T>
	{
		 /// <summary>
		 /// Estimate the weight of the remaining path from one node to another.
		 /// </summary>
		 /// <param name="node"> the node to estimate the weight from. </param>
		 /// <param name="goal"> the node to estimate the weight to. </param>
		 /// <returns> an estimation of the weight of the path from the first node to
		 ///         the second. </returns>
		 T GetCost( Node node, Node goal );
	}

}