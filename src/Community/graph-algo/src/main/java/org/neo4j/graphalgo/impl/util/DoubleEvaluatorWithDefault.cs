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
namespace Neo4Net.Graphalgo.impl.util
{
	using Neo4Net.Graphalgo;
	using Direction = Neo4Net.Graphdb.Direction;
	using Relationship = Neo4Net.Graphdb.Relationship;

	public class DoubleEvaluatorWithDefault : CostEvaluator<double>
	{
		 private string _costPropertyName;
		 private readonly double _defaultCost;

		 public DoubleEvaluatorWithDefault( string costPropertyName, double defaultCost ) : base()
		 {
			  this._costPropertyName = costPropertyName;
			  this._defaultCost = defaultCost;
		 }

		 public override double? GetCost( Relationship relationship, Direction direction )
		 {
			  return ( ( Number ) relationship.GetProperty( _costPropertyName, _defaultCost ) ).doubleValue();
		 }
	}

}