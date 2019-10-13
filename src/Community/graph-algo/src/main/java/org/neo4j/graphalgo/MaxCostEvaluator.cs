﻿using System;

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
namespace Neo4Net.Graphalgo
{
	/// <summary>
	/// Evaluator for determining if the maximum path cost has been exceeded.
	/// 
	/// @author Peter Neubauer </summary>
	/// @param <T> The cost value type </param>
	/// @deprecated This is not in use anymore, left for backward comparability 
	[Obsolete("This is not in use anymore, left for backward comparability")]
	public interface MaxCostEvaluator<T>
	{
		 /// <summary>
		 /// Evaluates whether the maximum cost has been exceeded.
		 /// </summary>
		 /// <param name="currentCost"> the cost to be checked </param>
		 /// <returns> true if the maximum Cost is less that currentCost </returns>
		 bool MaxCostExceeded( T currentCost );
	}

}