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
namespace Neo4Net.GraphAlgo
{
	using DoubleEvaluator = Neo4Net.GraphAlgo.Utils.DoubleEvaluator;
	using DoubleEvaluatorWithDefault = Neo4Net.GraphAlgo.Utils.DoubleEvaluatorWithDefault;
	using GeoEstimateEvaluator = Neo4Net.GraphAlgo.Utils.GeoEstimateEvaluator;
	using IntegerEvaluator = Neo4Net.GraphAlgo.Utils.IntegerEvaluator;

	/// <summary>
	/// Factory for common evaluators used by some graph algos, f.ex
	/// <seealso cref="CostEvaluator"/> and <seealso cref="EstimateEvaluator"/>.
	/// 
	/// @author Mattias Persson
	/// </summary>
	public abstract class CommonEvaluators
	{
		 public static CostEvaluator<double> DoubleCostEvaluator( string relationshipCostPropertyKey )
		 {
			  return new DoubleEvaluator( relationshipCostPropertyKey );
		 }

		 public static CostEvaluator<double> DoubleCostEvaluator( string relationshipCostPropertyKey, double defaultCost )
		 {
			  return new DoubleEvaluatorWithDefault( relationshipCostPropertyKey, defaultCost );
		 }

		 public static CostEvaluator<int> IntCostEvaluator( string relationshipCostPropertyKey )
		 {
			  return new IntegerEvaluator( relationshipCostPropertyKey );
		 }

		 public static EstimateEvaluator<double> GeoEstimateEvaluator( string latitudePropertyKey, string longitudePropertyKey )
		 {
			  return new GeoEstimateEvaluator( latitudePropertyKey, longitudePropertyKey );
		 }
	}

}