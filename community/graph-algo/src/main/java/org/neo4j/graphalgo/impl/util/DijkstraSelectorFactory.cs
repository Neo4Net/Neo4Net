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
namespace Org.Neo4j.Graphalgo.impl.util
{
	using Org.Neo4j.Graphalgo;
	using TraversalBranch = Org.Neo4j.Graphdb.traversal.TraversalBranch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.OUTGOING;

	public class DijkstraSelectorFactory : BestFirstSelectorFactory<double, double>
	{
		 private readonly CostEvaluator<double> _evaluator;

		 public DijkstraSelectorFactory( PathInterest<double> interest, CostEvaluator<double> evaluator ) : base( interest )
		 {
			  this._evaluator = evaluator;
		 }

		 protected internal override double? CalculateValue( TraversalBranch next )
		 {
			  return next.Length() == 0 ? 0d : _evaluator.getCost(next.LastRelationship(), OUTGOING);
		 }

		 protected internal override double? AddPriority( TraversalBranch source, double? currentAggregatedValue, double? value )
		 {
			  return WithDefault( currentAggregatedValue, 0d ) + WithDefault( value, 0d );
		 }

		 private T WithDefault<T>( T valueOrNull, T valueIfNull )
		 {
			  return valueOrNull != default( T ) ? valueOrNull : valueIfNull;
		 }

		 protected internal override double? StartData
		 {
			 get
			 {
				  return 0d;
			 }
		 }
	}

}